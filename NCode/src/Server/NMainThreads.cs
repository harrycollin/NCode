using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NCode.Core;
using NCode.Core.BaseClasses;
using NCode.Core.Protocols;
using NCode.KleosTypes.Virtual;
using NCode.Utilities;

namespace NCode.Server
{
    public class NMainThreads : NMainFunctions
    {
        /// <summary>
        /// Starts the main processes
        /// </summary>
        public void Start(string name, int tcpport, int udpport, int rconport, string password, bool AutoStart)
        {
            AddSomeObjects();

            //Start the main thread.
            MainThread = new Thread(MainLoop);
            MainThread.Start();

            //Start the udp thread.
            UdpThread = new Thread(UdpProcessor);
            UdpThread.Start();

            NObjectTracker.StartTrackingTheads();
           

            //Won't start the game server unless specified to in the config. (Default is true)
            if (AutoStart)
            {
                //Checks to see if the Tcp listener started without error. 
                if (StartListeningGameServer(tcpport, udpport))
                {
                    Tools.Print("Game Server started on port: " + tcpport);

                    //Assign the Tcp port in the MainFunctions
                    TcpListenPort = tcpport;

                    //Start the game server
                    RunGameServer = true;
         
                }
            }
        }

        public void AddSomeObjects()
        {
            NetworkObject building1 = new NetworkObject(2, true, SpawnPriority.High);
            NetworkObject building2 = new NetworkObject(2, true, SpawnPriority.High);

            building1.position = new V3(-265, 0, 298);
            building2.position = new V3(-223f, 0, 298);

            NObjectTracker.NetworkObjects.Add(building1.GUID, building1);
            NObjectTracker.NetworkObjects.Add(building2.GUID, building2);

        }

        //Starts listening for tcp connections on the specified port.
        public bool StartListeningGameServer(int tcpPort, int udpPort)
        {
            //Lock the state. Stops anything else using these resources.
            lock (GameServerThreadLock)
            {
                //Stops the Main TcpProtocol if its already running.
                if (MainTcp != null)
                {
                    MainTcp.Stop();
                    MainTcp = null;
                }
                if (tcpPort != 0)
                {
                    try
                    {
                        //Start a new TcpListener on the specifed port with a max number of backlogged connections. 
                        MainTcp = new TcpListener(IPAddress.Any, tcpPort);
                        MainTcp.Start();
                        Tools.Print("TCP Listening on port " + tcpPort);
                    }
                    catch (System.Exception ex)
                    {
                        Tools.Print("@NMainThreads.StartListening", Tools.MessageType.error, ex);
                        return false;
                    }
                }
                else { return false; }

                if (udpPort != -1)
                {
                    try
                    {
                        MainUdp = new TNUdpProtocol();
                        UdpPort = udpPort;
                        MainUdp.Start(udpPort);
                        Tools.Print("UDP Listening on port " + udpPort);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                else { return false; }

                return true;
            }
        }


        /// <summary>
        /// The main thread on the server. Cycles through pending connections, queued packets etc.
        /// </summary>
        private void MainLoop()
        {
            int ticks = 0;
            for (;;)
            {
                //Loop forever until server is stopped.
                while (RunGameServer)
                {
                   
                    var watch = Stopwatch.StartNew();
                    //Locks any resources that could potentially be accessed externally. Prevents sharing violations. 
                    lock (GameServerThreadLock)
                    {
                        //The tick time divided by 10000 as a counter (used to calculate ping, thread times, etc.)
                        TickTime = DateTime.UtcNow.Ticks / 10000;
                       
                        //Add any game server pending connections (Pending connections on the Game server's tcp listener).
                        while (MainTcp != null && MainTcp.Pending())
                        {
                            //Accept a new connection and assign it a socket.
                            Socket socket = MainTcp.AcceptSocket();
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
                                    AddPlayer(socket);
                                }
                            }
                            catch (Exception e)
                            {
                                Tools.Print("@MainLoop - add game server pending connection", Tools.MessageType.error, e);
                            }
                        }

                        //Loops through each player in the player list.
                        for (int i = 0; i < MainPlayerList.Count; i++)
                        {
                            //Access a player by index
                            NTcpPlayer player = MainPlayerList[i];

                            // Remove disconnected players (Checks the player's TcpProtocol to see if the socket is still connected)
                            if (!player.isSocketConnected)
                            {
                                //SendPlayerDisconnected(player);
                                RemovePlayer(player);
                                //Breaks the current iterartion instead of break; which breaks the whole 'for' statement.
                                continue;
                            }

                            // If the player doesn't send any packets in a while, disconnect him
                            if (player.timeoutTime > 0 && player.lastReceivedTime + player.timeoutTime < TickTime)
                            {
                                //SendPlayerDisconnected(player);
                                RemovePlayer(player, true);
                                continue;
                            }

                            // Process up to 100 packets from this player's InQueue at a time. (This is processed after checking for disconnected sockets. 
                            for (int e = 0; e < 100 && player.ReceivePacket(out packet); e++)
                            {
                                ProcessPlayerPacket(player, packet, true);
                            }
                        }
                    }
                    watch.Stop();
                    MainThreadTimeInMs = watch.ElapsedTicks;
                    ticks++;
                    if (ticks != 30000) continue;
                    UpdateView();
                    ticks = 0;
                }
            }
        }

        private void UdpProcessor()
        {
            IPEndPoint udpEndpoint;
            for (;;)
            {
                //Loop forever until server is stopped.
                while (RunGameServer)
                {
                    var watch = Stopwatch.StartNew();
                    //Reassigned to the remote udp packet's sender's endpoint. Temporary assignment. 

                    if (MainUdp.isActive)
                    {
                        Core.Buffer buffer;
                        //Process up to 1000 udp packets each time (Tweak for performance if needed. Will be added to cfg if needed).
                        for (int e = 0; e < 200 && MainUdp.ReceivePacket(out buffer, out udpEndpoint); e++)
                        {
                            NTcpPlayer player = GetPlayer(udpEndpoint);
                            if (player != null)
                            {
                                ProcessPlayerPacket(player, buffer, false);
                                continue;
                            }

                            BinaryReader reader = buffer.BeginReading();

                            if ((Packet) reader.ReadByte() != Packet.SetupUDP) continue;
                            player = GetPlayer((Guid)reader.ReadObject());
                            if (player != null)
                            {
                                Tools.Print(player.socket.RemoteEndPoint + " has setup their udp connection");

                                //Add the player to the UdpEndpoint Dictionary
                                PlayerUdpEPDictionary.Add(udpEndpoint, player);
                                //Set the Udp Endpoint
                                player.UdpEndpoint = udpEndpoint;
                                //Set the UDP setup as true
                                player.IsPlayerUdpConnected = true;

                                player.IsPlayerSetupComplete = true;
                                //Send the response packet to the player
                                var writer = player.BeginSend(Packet.SetupUDP);
                                writer.Write(true);
                                player.EndSend();
                            }
                            else
                            {
                                Tools.Print("GetPlayer null");
                            }
                        }
                    }
                    watch.Stop();
                    UdpThreadTimeInMs = watch.ElapsedTicks;
                }
            }
        }

        /// <summary>
        /// Processes a single packet.
        /// </summary>
        private void ProcessPlayerPacket(NTcpPlayer player, Core.Buffer packet, bool reliable)
        {
            //Begin reading the packet. Returns the BinaryReader loaded with the memorystream to be read. 
            BinaryReader reader = packet.BeginReading();

            Packet packetType = (Packet)reader.ReadByte();
            //Identifies the packet.
            if (packetType != 0)
            {
                Tools.Print(packetType.ToString());
                if (player.stage != TNTcpProtocol.Stage.Connected && packetType != Packet.RequestClientSetup) { RemovePlayer(player); }

                //Put higher priority cases at the top.
                switch (packetType)
                {
                    case Packet.ForwardToAll:
                        {
                            ProcessForwardPacket(player, packet, packetType, reader, reliable);
                            break;
                        }
                    case Packet.ForwardToChannels:
                        {
                            ProcessForwardPacket(player, packet, packetType, reader, reliable);
                            break;
                        }
                    case Packet.PlayerPositionUpdate:
                        {
                            //Send to everyone in channel
                            player.playerObject.position = (V3)reader.ReadObject();
                            
                            break;
                        }
                    case Packet.RequestSetPlayerObject:
                        {
                            player.playerObject = (NetworkObject)reader.ReadObject();
                            break;
                        }
                    case Packet.RequestSpawnPlayerObject:
                        {
                            NetworkObject playerObject = (NetworkObject)reader.ReadObject();
                            if (playerObject != null && playerObject.position != V3.Zero())
                            {
                                ProcessPlayerSpawn(player, playerObject);
                            }
                            break;
                        }
                    case Packet.RequestClientSetup:
                        {
                            ClientSetup(reader, player);
                            break;
                        }

                        // Custom packets will get filtered here.
                    default:
                        {
                            switch (packetType)
                            {
                                case Packet.RequestPlayerInfo:
                                    {
                                        PlayerInfo received = (PlayerInfo)reader.ReadObject();
                                        if (received == null) RemovePlayer(player);
                                        if (!DatabaseRequest.DoesEntryExist(received.steamid))
                                        {                                          
                                            PlayerInfo info = new PlayerInfo();
                                            info.steamid = received.steamid;
                                            DatabaseRequest.NewEntry(info);

                                            BinaryWriter writer = player.BeginSend(Packet.ResponsePlayerInfo);
                                            writer.Write(false);
                                            player.EndSend();
                                        }
                                        else
                                        {
                                            BinaryWriter writer = player.BeginSend(Packet.ResponsePlayerInfo);
                                            writer.Write(true);
                                            writer.WriteObject(DatabaseRequest.GetEntry(received));
                                            player.EndSend();
                                        }
                                        break;
                                    }
                                case Packet.SavePlayerData:
                                    {
                                        DatabaseRequest.SaveEntry((PlayerInfo)reader.ReadObject());
                                        break;
                                    }
                            }
                            break;
                        } 
                }
            }
        }

        void ProcessForwardPacket(NTcpPlayer player, Core.Buffer buffer, Packet packet, BinaryReader reader, bool reliable)
        {
            int start = buffer.position - 5;
            buffer.position = start;

            //Simply send the packet to all players
            if (packet == Packet.ForwardToAll)
            {
                for (int i = 0; i < MainPlayerList.size; i++)
                {
                    if (MainPlayerList[i] != player)
                    {
                        if (reliable) MainPlayerList[i].SendTcpPacket(buffer);
                        else if (MainPlayerList[i].IsPlayerUdpConnected) MainUdp.Send(buffer, MainPlayerList[i].UdpEndpoint);
                    }
                }
            }
            //More work is needed if it is to all connected channels
            else if(packet == Packet.ForwardToChannels)
            {
                foreach(Guid i in player.SyncingPlayers())
                {
                    if (reliable) PlayerDictionary[i].SendTcpPacket(buffer);
                    else if (PlayerDictionary[i].IsPlayerUdpConnected) MainUdp.Send(buffer, PlayerDictionary[i].UdpEndpoint);                                
                }
            }
        }
    }
}
