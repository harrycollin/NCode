using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NCode.Core;
using NCode.Core.Protocols;
using NCode.Core.Utilities;
using Buffer = NCode.Core.Buffer;

namespace NCode.Server.Core
{
    public class NGameServer
    {
        /// <summary>
        /// Time in ticks
        /// </summary>
        public long TickTime = 0;

        private bool _runThreads = false;

        private TcpListener _mainTcpListener;
        private TNUdpProtocol _mainUdpProtocol;

        private Dictionary<int, NPlayer> _playerDictionary = new Dictionary<int, NPlayer>();
        private NPacketProcessor _packetProcessor = new NPacketProcessor();

        private Thread _coreThread;

        public readonly int TcpListenPort = 0;
        public readonly int UdpListenPort = 0;

        /// <summary>
        /// A dictionary of custom packet listeners. The key is the packet. 
        /// </summary>
        private Dictionary<Packet, OnPacket> _packetHandlers = new Dictionary<Packet, OnPacket>();
        public delegate void OnPacket(Packet response, BinaryReader reader);

        public NGameServer(string name, int tcpport, int udpport, int rconport, string password, bool autoStart)
        {
            TcpListenPort = tcpport;
            UdpListenPort = udpport;
            if (!autoStart) return;
            Start();
        }

        /// <summary>
        /// Starts the main processes
        /// </summary>
        public void Start()
        {
            //Checks to see if the Tcp listener started without error. 
            if (!StartListeningGameServer()) return;
            _runThreads = true;
            _coreThread = new Thread(CoreProcesses) {Priority = ThreadPriority.Highest};
            _coreThread.Start();
            Tools.Print("Game Server started on port: " + TcpListenPort);
        }

        //Starts listening for tcp connections on the specified port.
        public bool StartListeningGameServer()
        {

            //Stops the Main TcpProtocol if its already running.
            if (_mainTcpListener != null)
            {
                _mainTcpListener.Stop();
                _mainTcpListener = null;
            }
            if (TcpListenPort != 0)
            {
                try
                {
                    //Start a new TcpListener on the specifed port with a max number of backlogged connections. 
                    _mainTcpListener = new TcpListener(IPAddress.Any, TcpListenPort);
                    _mainTcpListener.Start();
                    Tools.Print("TCP Listening on port " + TcpListenPort);
                }
                catch (System.Exception ex)
                {
                    Tools.Print("@NMainThreads.StartListening", Tools.MessageType.error, ex);
                    return false;
                }
            }
            else { return false; }

            if (UdpListenPort != -1)
            {
                try
                {
                    _mainUdpProtocol = new TNUdpProtocol();
                    _mainUdpProtocol.Start(UdpListenPort);
                    Tools.Print("UDP Listening on port " + UdpListenPort);
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else { return false; }

            return true;
        }

        private void CoreProcesses()
        {

                //Loop forever until server is stopped.
                while (_runThreads)
                {
                    if (!CheckForPendingConnections()) _runThreads = false;
                    if (!UdpProcessor()) _runThreads = false;
                    if (!ProcessTcpPackets()) _runThreads = false;

                    //The tick time divided by 10000 as a counter (used to calculate ping, thread times, etc.)
                    TickTime = DateTime.UtcNow.Ticks / 10000;
                }
        }



        private bool ProcessTcpPackets()
        {
            
                //Loops through each player in the player list.
                foreach (var keyValuePair in NPlayer.PlayerIdDictionary)
                {
                    // Remove disconnected players (Checks the player's TcpProtocol to see if the socket is still connected)
                    if (!keyValuePair.Value.IsPlayerTcpConnected)
                    {
                        //SendPlayerDisconnected(player);
                        NPlayer.RemovePlayer(keyValuePair.Key);

                        //Breaks the current iterartion instead of break; which breaks the whole 'for' statement.
                        continue;
                    }

                    // If the player doesn't send any packets in a while, disconnect him
                    if (keyValuePair.Value.TimeoutTime > 0 && keyValuePair.Value.LastReceiveTime + keyValuePair.Value.TimeoutTime < TickTime)
                    {
                        //SendPlayerDisconnected(player);
                        NPlayer.RemovePlayer(keyValuePair.Key);
                        continue;
                    }

                    Buffer packet;

                    // Process up to 100 packets from this player's InQueue at a time. (This is processed after checking for disconnected sockets. 
                    for (int e = 0; e < 100 && keyValuePair.Value.NextPacket(out packet); e++)
                    {
                        _packetProcessor.ProcessPacket(keyValuePair.Value, packet, true);
                    }
                }
            
            return true;
        }

        private bool UdpProcessor()
        {
            if (!_mainUdpProtocol.isActive) return false;

            Buffer buffer;

            //Process up to 1000 udp packets each time (Tweak for performance if needed. Will be added to cfg if needed).
            IPEndPoint udpEndpoint;
            for (var e = 0; e < 200 && _mainUdpProtocol.ReceivePacket(out buffer, out udpEndpoint); e++)
            {
                var player = NPlayer.GetPlayer(udpEndpoint);
                if (player != null)
                {
                    _packetProcessor.ProcessPacket(player, buffer, false);
                    continue;
                }

                var reader = buffer.BeginReading();

                if ((Packet) reader.ReadByte() != Packet.SetupUDP) continue;
                /* player = NPlayer.GetPlayer((Guid)reader.ReadObject());
                 if (player != null)
                 {
                     Tools.Print(player.RemoteTcpEndPoint + " has setup their udp connection");

                     //Add the player to the UdpEndpoint Dictionary
                     PlayerUdpEPDictionary.Add(udpEndpoint, player);
                     //Set the Udp Endpoint
                     player.UdpEndpoint = udpEndpoint;
                     //Set the UDP setup as true
                     player.IsPlayerUdpConnected = true;

                     //Send the response packet to the player
                     var writer = player.BeginSend(Packet.SetupUDP);
                     writer.Write(true);
                     player.EndSend();
                 }
                 else
                 {
                     Tools.Print("GetPlayer null");
                 }*/
            }
            return true;

        }
        public bool CheckForPendingConnections()
        {
            //Add any game server pending connections (Pending connections on the Game server's tcp listener).
            while (_mainTcpListener != null && _mainTcpListener.Pending())
            {
                //Accept a new connection and assign it a socket.
                Socket socket = _mainTcpListener.AcceptSocket();
                try
                {
                    //The clients ip address.
                    IPEndPoint remoteClient = (IPEndPoint)socket.RemoteEndPoint;
                    if (remoteClient == null)
                    {
                        //Close the socket if the ip is null.
                        socket.Close(); socket = null;
                    }
                    else
                    {
                        //Add the connection as a new player(not yet verified)
                        NPlayer.AddPlayer(socket);
                    }
                }
                catch (Exception e)
                {
                    Tools.Print("@MainLoop - add game server pending connection", Tools.MessageType.error, e);
                    return false;
                }
            }
            return true;
        }
    }
}

