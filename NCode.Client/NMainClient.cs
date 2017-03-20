using System;
using System.IO;
using System.Net;
using System.Threading;
using NCode.Core;
using NCode.Core.BaseClasses;
using NCode.Core.Protocols;
using NCode.Core.Utilities;
using Buffer = NCode.Core.Buffer;

namespace NCode.Client
{
    public sealed class NMainClient : ClientEvents
    {

        #region Public Variables

        /// <summary>
        /// The client's Guid. Assigned by the server.
        /// </summary>
        public Guid ClientGuid { get; private set; }

        /// <summary>
        /// Whether the player is trying to connect to a server
        /// </summary>
        public bool IsTryingToConnect => _tcpClient.isTryingToConnect;

        /// <summary>
        /// Whether the player is connected to a server (verified aswell)
        /// </summary>
        public bool IsConnected => _tcpClient.isConnected;

        /// <summary>
        /// Whether the player's socket is connected to the server.
        /// </summary>
        public bool IsSocketConnected => _tcpClient.isSocketConnected;

        /// <summary>
        /// Whether the Udp Client is setup
        /// </summary>
        public bool IsUdpSetup => _udpClient.isActive;

        /// <summary>
        /// Stops the UpdateThread if set to false
        /// </summary>
        public bool ContinueUpdateThread;

        #endregion

        #region Private Vars

        /// <summary>
        /// The main thread?
        /// </summary>
        private Thread _mainThread;

        /// <summary>
        /// A temporary buffer to write to.
        /// </summary>
        private Buffer _tempBuffer;

        /// <summary>
        /// The Tcp protocol for this player.
        /// </summary>
        private readonly TNTcpProtocol _tcpClient = new TNTcpProtocol();

        /// <summary>
        /// The Udp protocol for this player.
        /// </summary>
        private readonly TNUdpProtocol _udpClient = new TNUdpProtocol();

        /// <summary>
        /// The current tick time of this client.
        /// </summary>
        private long _clientTime = 0;

        /// <summary>
        /// The endpoint for udp traffic on the server.
        /// </summary>
        private IPEndPoint _serverUdpEndpoint;

        /// <summary>
        /// The last time this client sent a ping.
        /// </summary>
        private long _lastPingTime = 0;

        #endregion

        #region Misc Vars

        

        #endregion

        public bool StartUpdateThread()
        {
            if (_mainThread != null) return false;
            try
            {
                ContinueUpdateThread = true;
                _mainThread = new Thread(_clientUpdate);
                _mainThread.Start();
                Tools.Print("Client Update Thread Started..");
            }
            catch (ThreadStateException threadStateException)
            {
                Tools.Print("ERROR::CLIENT::UPDATE|THREAD|START::THREAD|STATE", Tools.MessageType.error, threadStateException);
                throw;
            }
            catch (OutOfMemoryException outOfMemoryException)
            {
                Tools.Print("ERROR::CLIENT::UPDATE|THREAD|START::OUT|OF|MEMORY", Tools.MessageType.error, outOfMemoryException);
                throw;
            }
            return true;
        }

        public void StopUpdateThread()
        {
            ContinueUpdateThread = false;
        }

        public void Connect(IPAddress ipAddress, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            _tcpClient.Connect(ipEndPoint);
            Tools.Print("Trying to connect to:"+ipEndPoint);
        }

        /// <summary>
        /// Disconnects from the remote server.
        /// </summary>
        public bool Disconnect()
        {
            if (!IsSocketConnected) return false;
            _tcpClient.Disconnect();
            onDisconnect();
            return true;
        }

        /// <summary>
        /// Begins the sending process
        /// </summary>
        public BinaryWriter BeginSend(Packet packet)
        {
            _tempBuffer = Buffer.Create();
            return _tempBuffer.BeginPacket(packet);
        }

        /// <summary>
        /// Ends the sending process
        /// </summary>
        public void EndSend(bool reliable)
        {
            _tempBuffer.EndPacket();
            if (!IsSocketConnected) return;
            if (reliable || !IsUdpSetup || _serverUdpEndpoint == null || !_udpClient.isActive)
            {
                _tcpClient.SendTcpPacket(_tempBuffer);
            }
            else _udpClient.Send(_tempBuffer, _serverUdpEndpoint);
        }

        public void ResponseClientSetup(BinaryReader reader)
        {
            
        }

        /// <summary>
        /// Checks for queued packet and carries out routine functions. 
        /// </summary>
        private void _clientUpdate()
        {
            while (ContinueUpdateThread)
            {
                Thread.Sleep(1);

                if (!_tcpClient.isSocketConnected && !_tcpClient.isTryingToConnect) { onDisconnect(); return; }

                //Set the client time
                _clientTime = DateTime.UtcNow.Ticks / 10000;

                //Temporary variables for packets
                Buffer tempBuffer;
                IPEndPoint tempIp;

                var keepProcessing = true;

                //Process all Udp packets first.
                while (keepProcessing && _udpClient.ReceivePacket(out tempBuffer, out tempIp))
                {
                    keepProcessing = ProcessPacket(tempBuffer, tempIp);
                    tempBuffer.Recycle();
                }

                //Process Tcp packets
                while (keepProcessing && _tcpClient.ReceivePacket(out tempBuffer))
                {
                    ProcessPacket(tempBuffer, null);
                    tempBuffer.Recycle();
                }
                
                //Check if we need to ping (failing to ping regularly will result in the server disconnecting this player).
                if (_lastPingTime + 3000 >= _clientTime || _tcpClient.stage != TNTcpProtocol.Stage.Connected) continue;

                _lastPingTime = _clientTime;
                BinaryWriter writer = _tcpClient.BeginSend(Packet.Ping);
                _tcpClient.EndSend();

            }
        }

        #region Packet Processor

        /// <summary>
        /// Processes queued packets. 
        /// </summary>
        private bool ProcessPacket(Buffer packet, IPEndPoint source)
        {
            BinaryReader reader = packet.BeginReading();
            int packetId = reader.ReadByte();
            Packet response = (Packet)packetId;
            
            //Filters out any packets that have custom handlers. 
            OnPacket callback;

            if (packetHandlers.TryGetValue(response, out callback) && callback != null)
            {
                callback(response, reader);
                return true;
            }
            Tools.Print(response.ToString());
            switch (response)
            {
                case Packet.ForwardToChannels:
                    {                     
                        Guid guid = (Guid)reader.ReadObject();
                        int RFCID = reader.ReadInt32();
                        object[] parameters = reader.ReadObjectArrayEx();
                        onRFC(guid, RFCID, parameters);
                        break;
                    }               
                case Packet.Ping:
                    {
                        _tcpClient.lastReceivedTime = _clientTime;
                        break;
                    }
                case Packet.ResponseClientSetup:
                    {
                        if (reader.ReadByte() == 0)
                        {
                            _tcpClient.Disconnect();
                            Tools.Print("Server - Client version mismatch");
                        }
                        else
                        {
                            _tcpClient.ClientGuid = (Guid)reader.ReadObject();
                            IPEndPoint remoteIp = _tcpClient.socket.RemoteEndPoint as IPEndPoint;
                            _serverUdpEndpoint = new IPEndPoint(remoteIp.Address, reader.ReadInt32());
                            BinaryWriter writer = BeginSend(Packet.SetupUDP);
                            writer.WriteObject(_tcpClient.ClientGuid);
                            EndSend(false);
                            _tcpClient.stage = TNTcpProtocol.Stage.Connected;
                        }
                        break;
                    }                                       
                case Packet.ResponseSpawnPlayerObject:
                    {
                        Tools.Print("Response for spawn");
                        if (reader.ReadBoolean())
                        {
                            onSpawnPlayerResponse((NetworkObject)reader.ReadObject());
                        }
                        break;
                    }
                case Packet.SetupUDP:
                    {
                        if (reader.ReadBoolean())
                        {
                            Tools.Print("UDP Setup!");
                            onConnect();
                        }
                        break;
                    }
                default:
                    {
                        Tools.Print("No defined Packet");
                        break;
                    }
            }
            return false;
        }

        #endregion

    }
}