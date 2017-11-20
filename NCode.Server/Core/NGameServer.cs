using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NCode.Core;
using NCode.Core.Protocols;
using static NCode.Core.Utilities.Tools;
using Buffer = NCode.Core.Buffer;
using NCode.Server.Systems.Channel;
using System.Collections.Generic;
using System.IO;
using NCode.Core.Entity;

namespace NCode.Server.Core
{
    public class NGameServer
    {
        public static int ServerProtocolVersion => TNTcpProtocol.ProtocolVersion;

        /// <summary>
        ///     Time in ticks
        /// </summary>
        public long TickTime;

        public static int TcpListenPort;
        public static int UdpListenPort;
        private bool _runThreads;

        private TcpListener _mainTcpListener;
        private TNUdpProtocol _mainUdpProtocol;

        private Thread _coreThread;
        
        public NGameServer(string name, int tcpport, int udpport, int rconport, string password, bool autoStart)
        {
            TcpListenPort = tcpport;
            UdpListenPort = udpport;
            if (!autoStart) return;
            Start();
        }

        /// <summary>
        ///     Starts the main processes
        /// </summary>
        public void Start()
        {
            //Checks to see if the Tcp listener started without error. 
            if (!StartListeningGameServer()) return;
            _runThreads = true;
            _coreThread = new Thread(CoreProcesses) { Priority = ThreadPriority.Highest };
            _coreThread.Start();
            PrintInfo($"Game Server started on TCP port: {TcpListenPort}, UDP port: {UdpListenPort}");
        }

        private void SetDelegates()
        {

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
            else return false;

            if (UdpListenPort != -1)
                try
                {
                    _mainUdpProtocol = new TNUdpProtocol();
                    _mainUdpProtocol.Start(UdpListenPort);
                }
                catch (Exception e)
                {
                    PrintError($"Failed to start listening on UDP port {UdpListenPort}.", e);
                    return false;
                }
            else return false;

            return true;
        }

        public void AddCustomPacketHandler(Packet packet, OnPacket handler)
        {
            AddCustomHandler(packet, handler);
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
                    NPlayer.RemovePlayer(player.PlayerID);
                    continue;
                }

                // If the player doesn't send any packets in a while, disconnect him
                if (player.TimeoutTime > 0 &&
                    player.LastReceiveTime + player.TimeoutTime < TickTime)
                {
                    NPlayer.RemovePlayer(player.PlayerID);
                    continue;
                }

                // Process up to 100 packets from this player's InQueue at a time. (This is processed after checking for disconnected sockets. 
                for (var e = 0; e < 100 && player.NextPacket(out Buffer packet); e++)
                    ProcessPacket(player, packet, true);
            }

            return true;
        }

        private bool UdpProcessor()
        {
            if (!_mainUdpProtocol.isActive) return false;

            //Process up to 200 udp packets each time (Tweak for performance if needed. Will be added to cfg if needed).
            for (var i = 0;
                i < 200 && _mainUdpProtocol.ReceivePacket(out Buffer buffer, out IPEndPoint udpEndpoint);
                i++)
            {
                var player = NPlayer.GetPlayer(udpEndpoint);
                if (player != null)
                {
                    ProcessPacket(player, buffer, false);
                    continue;
                }

                var reader = buffer.BeginReading();

                if ((Packet)reader.ReadByte() != Packet.RequestSetupUdp) continue;
                player = NPlayer.GetPlayer(reader.ReadInt32());
                if (player != null)
                {
                    PrintInfo($"Player {player.PlayerID} ({player.RemoteTcpEndPoint}) has setup UDP connectivity.");

                    //Add the player to the UdpEndpoint Dictionary
                    NPlayer.PlayerUdpEnpointDictionary.Add(udpEndpoint, player);
                    //Set the Udp Endpoint
                    player.UdpEndpoint = udpEndpoint;
                    //Set the UDP setup as true
                    player.IsPlayerUdpConnected = true;

                    NServerEvents.playerConnected?.Invoke(player);
                    NPlayer.InitializePlayer(player);
                    //Send the response packet to the player
                    var writer = player.BeginSend(Packet.ResponseSetupUdp);
                    writer.Write(true);
                    player.EndSend();

                    NServerEvents.playerConnected?.Invoke(player);
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
                var socket = _mainTcpListener.AcceptSocket();
                try
                {
                    //The clients ip address.
                    var remoteClient = (IPEndPoint)socket.RemoteEndPoint;
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

        /// <summary>
        ///     A dictionary of custom packet listeners. The key is the packet.
        /// </summary>
        private readonly Dictionary<Packet, OnPacket> _packetHandlers = new Dictionary<Packet, OnPacket>();

        public delegate void OnPacket(Packet response, BinaryReader reader);

        public void AddCustomHandler(Packet packet, OnPacket handler)
        {
            _packetHandlers.Add(packet, handler);
        }

        public bool ProcessPacket(NPlayer player, Buffer buffer, bool reliable)
        {
            //Begin reading the packet. Returns the BinaryReader loaded with the memorystream to be read. 
            var reader = buffer.BeginReading();

            //Identifies the packet.
            var packetType = (Packet)reader.ReadByte();

            if (packetType == 0) return true;

            //Filters out any packets that have custom handlers. 
            if (_packetHandlers.TryGetValue(packetType, out OnPacket callback) && callback != null)
            {
                callback(packetType, reader);
                return true;
            }

            switch (packetType)
            {
                case Packet.Ping:
                    {
                        break;
                    }
                case Packet.RequestClientSetup:
                    {
                        try
                        {
                            var protocolVersion = reader.ReadInt32();

                            if (ServerProtocolVersion == protocolVersion)
                            {
                                var writer = player.BeginSend(Packet.ResponseClientSetup);
                                //Send information about this player.
                                writer.WriteObject(player.PlayerInfo);
                                writer.Write(UdpListenPort);
                                player.EndSend();
                                PrintInfo($"Player {player.PlayerID} has connected.");
                                break;
                            }
                            else
                            {
                                var writer = player.BeginSend(Packet.Error);
                                writer.Write("Version mismatch!");
                                writer.Write(true);
                                player.EndSend();
                                Print($"Player {player.PlayerID} has failed to connect. Version mismatch.");
                            }
                        }
                        catch (Exception e)
                        {
                            PrintError($"Cannot parse {packetType} packet to type 'int'. Client setup failed.", e);
                            throw;
                        }
                        break;
                    }
                case Packet.CreateEntity:
                    {
                        var channelID = -1;
                        NNetworkEntity entity = null;
                        try
                        {
                            channelID = reader.ReadInt32();
                            entity = (NNetworkEntity)reader.ReadObject();
                        }
                        catch (NullReferenceException e)
                        {
                            PrintError("Null reference exception.", e);
                        }

                        if (entity != null)
                        {
                            NEntityCache.AddOrUpdate(entity);
                            NChannel.Channels[channelID].AddEntity(entity.Guid);
                        }
                        break;
                    }


                case Packet.UpdateEntity:
                    {
                        NNetworkEntity entity = null;
                        try
                        {
                            entity = (NNetworkEntity)reader.ReadObject();
                        }
                        catch (NullReferenceException)
                        {
                        }

                        if (entity != null)
                        {
                            NEntityCache.AddOrUpdate(entity);
                            foreach (var otherPlayer in NPlayer.PlayerDictionary.Values.ToList())
                            {
                                if (otherPlayer == player) continue;
                                var writer = otherPlayer.BeginSend(Packet.UpdateEntity);
                                writer.WriteObject(entity);
                                otherPlayer.EndSend();
                            }
                        }
                        break;
                    }
                case Packet.DestroyEntity:
                    {
                        var entity = Guid.Empty;
                        try
                        {
                            entity = (Guid)reader.ReadObject();
                        }
                        catch (NullReferenceException)
                        {
                        }

                        NEntityCache.Remove(entity);
                        foreach (var otherPlayer in NPlayer.PlayerDictionary.Values.ToList())
                        {
                            if (otherPlayer == player) continue;
                            var writer = otherPlayer.BeginSend(Packet.DestroyEntity);
                            writer.WriteObject(entity);
                            otherPlayer.EndSend();
                        }

                        break;
                    }
                case Packet.TransferEntity:
                    {
                        Guid entity;
                        int channelA;
                        int channelB;

                        try
                        {
                            entity = (Guid)reader.ReadObject();
                            channelA = reader.ReadInt32();
                            channelB = reader.ReadInt32();
                        }
                        catch (EndOfStreamException endOfStream)
                        {
                            PrintError($"Exception reading {packetType}.", endOfStream);
                            break;
                        }

                        NChannel.TransferEntity(entity, channelA, channelB);

                        break;
                    }
                case Packet.ForwardToAll:
                case Packet.ForwardToChannels:
                    {
                        ProcessForwardPacket(player, buffer, packetType, reader, reliable);
                        break;
                    }
                case Packet.JoinChannel:
                    {
                        NChannel.JoinChannel(player, reader.ReadInt32());
                        break;
                    }
                case Packet.LeaveChannel:
                    {
                        NChannel.LeaveChannel(player, reader.ReadInt32());
                        break;
                    }
                default:
                    {
                        PrintError($"Packet with the ID:{packetType} has not been defined for processing.");
                        return false;
                    }
            }
            return true;
        }

        private void ProcessForwardPacket(NPlayer player, Buffer buffer, Packet packet, BinaryReader reader, bool reliable)
        {
            var start = buffer.position - 5;
            buffer.position = start;

            //Simply send the packet to all players
            if (packet == Packet.ForwardToAll)
            {
                foreach (var i in NPlayer.PlayerDictionary.Values.ToList())
                {
                    if (i == player) continue;
                    if (reliable) i.SendTcpPacket(buffer);
                    else if (i.IsPlayerUdpConnected) _mainUdpProtocol.Send(buffer, i.UdpEndpoint);
                }
            }
            //More work is needed if it is to all connected channels
            else if (packet == Packet.ForwardToChannels)
            {
                var alreadySentTo = new List<NPlayer>();

                foreach (var channel in NChannel.Channels.Values.ToList())
                {
                    if (!channel.HasPlayer(player)) continue;

                    foreach (var otherPlayer in channel.GetPlayers())
                    {
                        if (otherPlayer == player) continue;
                        if (alreadySentTo.Contains(otherPlayer)) continue;
                        alreadySentTo.Add(otherPlayer);

                        if (reliable) otherPlayer.SendTcpPacket(buffer);
                        else if (otherPlayer.IsPlayerUdpConnected) _mainUdpProtocol.Send(buffer, otherPlayer.UdpEndpoint);
                    }
                }
            }
        }

    }
}


