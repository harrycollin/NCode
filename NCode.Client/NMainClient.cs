using System;
using System.IO;
using System.Net;
using NCode.Core;
using NCode.Core.Protocols;
using NCode.Core.Utilities;
using Buffer = NCode.Core.Buffer;
using Random = UnityEngine.Random;
using NCode.Core.Entity;
using static NCode.Core.Utilities.Tools;

namespace NCode.Client
{
    public sealed class NMainClient : NClientEvents
    {

        #region Public Variables

        /// <summary>
        /// The client's ID. Assigned by the server.
        /// </summary>
        public int ClientId { get; private set; }

        /// <summary>
        /// The server's remote endpoint.
        /// </summary>
        public EndPoint RemoteEndPoint => _tcpClient.Socket.RemoteEndPoint;

        /// <summary>
        /// Whether the player is trying to connect to a server
        /// </summary>
        public bool IsTryingToConnect => _tcpClient.isTryingToConnect;

        /// <summary>
        /// Whether the player is connected to a server (verified aswell)
        /// </summary>
        public bool IsConnected => _tcpClient.IsConnected;

        /// <summary>
        /// Whether the player's socket is connected to the server.
        /// </summary>
        public bool IsSocketConnected => _tcpClient.IsSocketConnected;

        /// <summary>
        /// Whether the Udp Client is setup
        /// </summary>
        public bool IsUdpSetup => _udpClient.isActive;

        /// <summary>
        /// Stops the UpdateThread if set to false
        /// </summary>
        public bool ContinueUpdateThread;

        /// <summary>
        /// The list of connected channels
        /// </summary>
        public System.Collections.Generic.List<int> ConnectedChannels = new System.Collections.Generic.List<int>();

        #endregion

        #region Private Vars


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

        public void Connect(IPAddress ipAddress, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            _tcpClient.Connect(ipEndPoint);
            _udpClient.Start(Random.Range(30000,50000));
            Print("Trying to connect to:" + ipEndPoint);
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
            if (reliable || _serverUdpEndpoint == null || !_udpClient.isActive)
            {
                _tcpClient.SendTcpPacket(_tempBuffer);
            }
            else
            {
                _udpClient.Send(_tempBuffer, _serverUdpEndpoint);
            }
        }

        

        /// <summary>
        /// Checks for queued packet and carries out routine functions. 
        /// </summary>
        public void ClientUpdate()
        {
            if (!_tcpClient.IsSocketConnected && !_tcpClient.isTryingToConnect)
            {
                //onDisconnect();
                return;
            }

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
            if (_lastPingTime + 3000 >= _clientTime || _tcpClient.stage != TNTcpProtocol.Stage.Connected) return;

            _lastPingTime = _clientTime;
            _tcpClient.BeginSend(Packet.Ping);
            _tcpClient.EndSend();
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
            Print(response.ToString());
            switch (response)
            {
                           
                case Packet.Ping:
                    {
                        _tcpClient.lastReceivedTime = _clientTime;
                        break;
                    }
                case Packet.ResponseClientSetup:
                {
                    var responseId = reader.ReadInt32();
                    if (responseId == -1)
                    {
                        _tcpClient.Disconnect();
                        PrintError("Unable to connect to remote server. Server responsed with version mismatch.");
                    }
                    else
                    {
                        ClientId = responseId;
                        Print(ClientId);
                        var remoteIp = _tcpClient.Socket.RemoteEndPoint as IPEndPoint;
                        _serverUdpEndpoint = new IPEndPoint(remoteIp.Address, reader.ReadInt32());
                        Print(_serverUdpEndpoint);
                        var writer = BeginSend(Packet.RequestSetupUdp);
                        writer.Write(ClientId);
                        EndSend(false);
                    }
                    break;
                }                                       
                
                case Packet.ResponseSetupUdp:
                    {
                        if (reader.ReadBoolean())
                        {
                            Tools.Print("UDP Setup!");
                            _tcpClient.stage = TNTcpProtocol.Stage.Connected;
                            onConnect?.Invoke();
                        }
                        break;
                    }
                case Packet.JoinChannel:
                    {
                        int channel = 0;

                        try
                        {
                            channel = reader.ReadInt32();
                            if (!ConnectedChannels.Contains(channel))
                            {
                                ConnectedChannels.Add(channel);
                                Print($"Joined Channel {channel}.");
                            }
                        }
                        catch (EndOfStreamException exception)
                        {

                        }
                        break;
                    }
                case Packet.LeaveChannel:
                    {
                        int channel = 0;

                        try
                        {
                            channel = reader.ReadInt32();
                            if (ConnectedChannels.Contains(channel))
                            {
                                ConnectedChannels.Remove(channel);
                                Print($"Left Channel {channel}.");
                            }
                        }
                        catch (EndOfStreamException exception)
                        {

                        }
                        break;
                    }

                case Packet.CreateEntity:
                    {
                        onCreateEntity((NNetworkEntity)reader.ReadObject());
                        break;
                    }
                case Packet.UpdateEntity:
                    {
                        onEntityUpdate((NNetworkEntity)reader.ReadObject());
                        break;
                    }
                case Packet.DestroyEntity:
                    {
                        onDestroyEntity((Guid)reader.ReadObject());
                        break;
                    }

                case Packet.ForwardToChannels:
                case Packet.ForwardToAll:
                    {
                        var guid = (Guid)reader.ReadObject();
                        var rfcid = reader.ReadInt32();
                        var parameters = reader.ReadObjectArrayEx();
                        onRemoteFunctionCall(guid, rfcid, parameters);
                        break;
                    }
                default:
                    {
                        PrintError($"Packet with the ID:{response} has not been defined for processing.");
                        break;
                    }
            }
            return false;
        }

        #endregion

    }
}