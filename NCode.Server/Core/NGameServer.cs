using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NCode.Core;
using NCode.Core.Protocols;
using NCode.Core.Utilities;
using Buffer = NCode.Core.Buffer;
using static NCode.Core.Utilities.Tools;

namespace NCode.Server.Core
{
    public class NGameServer
    {
        public static int ServerProtocolVersion => TNTcpProtocol.ProtocolVersion;

        /// <summary>
        /// Time in ticks
        /// </summary>
        public long TickTime = 0;

        private bool _runThreads;

        private TcpListener _mainTcpListener;
        private TNUdpProtocol _mainUdpProtocol;

        private readonly NPacketProcessor _packetProcessor = new NPacketProcessor();

        private Thread _coreThread;

        public static int TcpListenPort = 0;
        public static int UdpListenPort = 0;

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
            PrintInfo($"Game Server started on TCP port: {TcpListenPort}, UDP port: {UdpListenPort}");
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
            if (TcpListenPort != -1)
            {
                try
                {
                    //Start a new TcpListener on the specifed port with a max number of backlogged connections. 
                    _mainTcpListener = new TcpListener(IPAddress.Any, TcpListenPort);
                    _mainTcpListener.Start();
                }
                catch (Exception e)
                {
                    PrintError($"Failed to start listening on TCP port {TcpListenPort}.", e);
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
                    _packetProcessor.mainUdp = _mainUdpProtocol;

                }
                catch (Exception e)
                {
                    PrintError($"Failed to start listening on UDP port {UdpListenPort}.", e);
                    return false;
                }
            }
            else { return false; }

            return true;
        }

        public void AddCustomPacketHandler(Packet packet, NPacketProcessor.OnPacket handler)
        {
            _packetProcessor.AddCustomHandler(packet, handler);
        }

        private void CoreProcesses()
        {
            //Loop forever until server is stopped.
            for (; ;)
            {
                if (!CheckForPendingConnections())
                {
                    _runThreads = false;
                    PrintError("Error occured in Pending Connections");
                }

                if (!UdpProcessor())
                {
                    _runThreads = false;
                    PrintError("Error occured in UDP Processing");
                }

                if (!ProcessTcpPackets())
                {
                    _runThreads = false;
                    PrintError("Error occured in TCP Processing");
                }

                //The tick time divided by 10000 as a counter (used to calculate ping, thread times, etc.)
                TickTime = DateTime.UtcNow.Ticks / 10000;

                Thread.Sleep(1);

                //Implement shutdown action
                if (!_runThreads) break;
            }

        }



        private bool ProcessTcpPackets()
        {
            //Loops through each player in the player list.
            foreach (var player in NPlayer.PlayerDictionary.Values.ToList())
            {
                // Remove disconnected players (Checks the player's TcpProtocol to see if the socket is still connected)
                if (!player.IsPlayerSocketConnected)
                {
                    NPlayer.RemovePlayer(player.ClientId);
                    continue;
                }

                // If the player doesn't send any packets in a while, disconnect him
                if (player.TimeoutTime > 0 &&
                    player.LastReceiveTime + player.TimeoutTime < TickTime)
                {
                    NPlayer.RemovePlayer(player.ClientId);
                    continue;
                }

                Buffer packet;

                // Process up to 100 packets from this player's InQueue at a time. (This is processed after checking for disconnected sockets. 
                for (var e = 0; e < 100 && player.NextPacket(out packet); e++)
                    _packetProcessor.ProcessPacket(player, packet, true);
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

                if ((Packet)reader.ReadByte() != Packet.RequestSetupUdp) continue;
                player = NPlayer.GetPlayer(reader.ReadInt32());
                if (player != null)
                {
                    PrintInfo($"Player {player.ClientId} ({player.RemoteTcpEndPoint}) has setup UDP connectivity.");

                    //Add the player to the UdpEndpoint Dictionary
                    NPlayer.PlayerUdpEnpointDictionary.Add(udpEndpoint, player);
                    //Set the Udp Endpoint
                    player.UdpEndpoint = udpEndpoint;
                    //Set the UDP setup as true
                    player.IsPlayerUdpConnected = true;

                    //Send the response packet to the player
                    var writer = player.BeginSend(Packet.ResponseSetupUdp);
                    writer.Write(true);
                    player.EndSend();
                }
                else
                {
                    Print("Unable to find player. Udp setup failed.");
                }
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
                        socket.Close();
                        PrintError("Remote client socket couldn't be accepted.");
                    }
                    else
                    {
                        //Add the connection as a new player(not yet verified)
                        NPlayer.AddPlayer(socket);
                    }
                }
                catch (Exception e)
                {
                    PrintError("@MainLoop - add game server pending connection", e);
                    return false;
                }
            }
            return true;
        }
    }
}

