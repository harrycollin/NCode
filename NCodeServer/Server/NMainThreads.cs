using NCode.Utilities;
using NCode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCode.Core.Protocols;

namespace NCode
{
    public class NMainThreads : NMainFunctions
    {
        /// <summary>
        /// Starts the main processes
        /// </summary>
        public void Start(string name, int tcpport, int udpport, int rconport, string password, string rconpassword, bool AutoStart)
        {
            //Start the main thread.
            MainThread = new Thread(MainLoop);
            MainThread.Start();

            //Start the RCon thread.
            RConThread = new Thread(RConThreadLoop);
            RConThread.Start();

            //Set the password
            RConPassword = rconpassword;

            //Checks to see if the TCP RCon listener can be started on the specified port.
            if (StartListeningRCon(rconport))
            {
                //Start the RCon server
                RunRConServer = true;
                Tools.Print("RCon Server started on port: " + rconport);
            }

            //Won't start the game server unless specified to in the config. (Default is true)
            if (AutoStart)
            {
                //Checks to see if the Tcp listener started without error. 
                if (StartListeningGameServer(tcpport, udpport))
                {
                    Tools.Print("Game Server started on port: " + tcpport);

                    //Assign the Tcp port in the MainFunctions
                    TcpListenPort = tcpport;

                    //Tries to load all objects from the database.
                    try
                    {
                        //Grabs the objects from database class. 
                        HashSet<NetworkObject> objects = DatabaseRequest.LoadObjects();
                        //Adds each one to the NetworkObjects dictionary for quick access
                        foreach (NetworkObject i in objects) NetworkObjects.Add(i.GUID, i);
                        Tools.Print("Loaded " + objects.Count + " objects from the database", Tools.MessageType.notification);
                    }
                    catch (Exception e)
                    {
                        Tools.Print("Failed to load objects from the database.", Tools.MessageType.error, e);
                        //Add a stop to the server.
                        return;
                    }

                    //Creates and preloads channels with their objects. Accesses them from the NetworkObjects dictionary.
                    foreach (KeyValuePair<Guid, NetworkObject> i in NetworkObjects)
                    {
                        if (i.Value.LastChannelID > 0)
                        {
                            //Check if channel exists.
                            if (ActiveChannels.ContainsKey(i.Value.LastChannelID))
                            {
                                ActiveChannels[i.Value.LastChannelID].AddObject(i.Value);
                            }
                            else //Make new channel
                            {
                                Tools.Print("Created new channel " + i.Value.LastChannelID);
                                NChannel newChannel = new NChannel();
                                newChannel.ID = i.Value.LastChannelID;
                                newChannel.AddObject(i.Value);
                                ActiveChannels.Add(i.Value.LastChannelID, newChannel);
                            }
                        }
                    }
                    //Start the game server
                    RunGameServer = true;
                }                             
            }         
        }

        /// <summary>
        /// Simply used to add random shit to the database. Can be used to test functionalites across server. 
        /// </summary>
        void AddNewObjects()
        {
            Random r = new Random();

            for (int i = 0; i < 100; i++)
            {
                NetworkObject o = new NetworkObject();
                o.LastChannelID = r.Next(20);
                o.GUID = Guid.NewGuid();
                DatabaseRequest.SaveNewObject(o);
            }
        }

        //Starts listening for tcp connections on the specified port.
        public bool StartListeningRCon(int TcpPort)
        {
            //Lock the state. Stops anything else using these resources.
            lock (RConServerThreadLock)
            {
                //Stops the Main TcpProtocol if its already running.
                if (RConTcp != null)
                {
                    RConTcp.Stop();
                    RConTcp = null;
                }

                if (TcpPort != 0)
                {
                    try
                    {
                        //Start a new TcpListener on the specifed port with a max number of backlogged connections. 
                        RConTcp = new TcpListener(IPAddress.Any, TcpPort);
                        RConTcp.Start(50);
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        Tools.Print("@NMainThreads.StartListening", Tools.MessageType.error, ex);
                    }
                }
            }
            return false;
        }

        //Starts listening for tcp connections on the specified port.
        public bool StartListeningGameServer(int TcpPort, int udpPort)
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
                if (TcpPort != 0)
                {
                    try
                    {
                        //Start a new TcpListener on the specifed port with a max number of backlogged connections. 
                        MainTcp = new TcpListener(IPAddress.Any, TcpPort);                    
                        MainTcp.Start(50);
                    }
                    catch (System.Exception ex)
                    {
                        Tools.Print("@NMainThreads.StartListening", Tools.MessageType.error, ex);
                        return false;
                    }
                }
                else { return false; }

                if(udpPort != -1)
                {
                    try
                    {
                        MainUdp = new NUdpProtocol();
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
        void MainLoop()
        {
            Tools.Print("Starting main thread.");
            for (;;)
            {
                //Loop forever until server is stopped.
                while (RunGameServer)
                {
                    //Locks any resources that could potentially be accessed externally. Prevents sharing violations. 
                    lock (GameServerThreadLock)
                    {
                        //The tick time divided by 10000 as a counter (used to calculate ping, thread times, etc.)
                        TickTime = DateTime.UtcNow.Ticks / 10000;

                        //Reassigned to the remote udp packet's sender's endpoint. Temporary assignment. 
                        IPEndPoint udpEndpoint;

                        if (MainUdp.isActive)
                        {
                            NBuffer buffer;
                            //Proccess up to 1000 udp packets each time (Tweak for performance if needed. Will be added to cfg if needed).
                            for (int e = 0; e < 1000 && MainUdp.ReceivePacket(out buffer, out udpEndpoint); e++)
                            {
                                NTcpPlayer player = GetPlayer(udpEndpoint);
                                if(player != null)
                                {
                                    ProcessPlayerPacket(player, buffer); 
                                    continue;
                                }

                                BinaryReader reader = buffer.BeginReading();
                 
                                if (buffer.packet == Packet.SetupUDP)
                                {
                                    player = GetPlayer((Guid)reader.ReadObject());
                                    if (player != null)
                                    {
                                        Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " has setup their udp connection");
                                        PlayerUdpEPDictionary.Add(udpEndpoint, player);
                                        BinaryWriter writer = player.BeginSend(Packet.SetupUDP);
                                        writer.Write(true);
                                        player.EndSend();
                                    }
                                    else
                                    {
                                        Tools.Print("GetPlayer null");
                                    }
                                }
                            }              
                        }
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
                        for (int i = 0; i < PlayersList.Count; i++)
                        {
                            //Access a player by index
                            NTcpPlayer player = PlayersList[i];

                            // Remove disconnected players (Checks the player's TcpProtocol to see if the socket is still connected)
                            if (!player.isSocketConnected)
                            {
                                SendPlayerDisconnected(player);
                                RemovePlayer(player);
                                //Breaks the current iterartion instead of break; which breaks the whole 'for' statement.
                                continue;
                            }

                            // If the player doesn't send any packets in a while, disconnect him
                            if (player.Timeout > 0 && player.LastReceiveTime + player.Timeout < TickTime)
                            {
                                SendPlayerDisconnected(player);
                                RemovePlayer(player);
                                continue;
                            }

                            // Process up to 100 packets from this player's InQueue at a time. (This is processed after checking for disconnected sockets. 
                            for (int e = 0; e < 100 && player.NextPacket(out packet); e++)
                            {
                                ProcessPlayerPacket(player, packet);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes RCon client packets. Separated from the player packets to reduces lag and latency.
        /// </summary>
        void RConThreadLoop()
        {
            Tools.Print("Starting RCon thread.");


            //Loop forever until server is stopped.
            while(RunRConServer)
            {
                lock (RConServerThreadLock)
                {
                    //The tick time divided by 10000 as a counter (used to calculate ping etc.)
                    TickTime = DateTime.UtcNow.Ticks / 10000;

                    //Add rcon server pending connections
                    while (RConTcp != null && RConTcp.Pending())
                    {
                        //Accept a new connection and assign it a socket.
                        Socket socket = RConTcp.AcceptSocket();
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
                                //Add the connection as a new client(not yet verified)
                                AddRConClient(socket);
                            }
                        }
                        catch (Exception e)
                        {
                            Tools.Print("@RConThreadLoop - add rcon client pending connection", Tools.MessageType.error, e);
                        }
                    }



                    //Loops through each client in the client list.
                    for (int i = 0; i < RConClients.Count; i++)
                    {
                        //Access a player by index
                        NRConClient client = RConClients[i];

                        // Remove disconnected players (Checks the player's TcpProtocol to see if the socket is still connected)
                        if (!client.isSocketConnected)
                        {

                            RConClients.Remove(client);
                            //Breaks the current iterartion instead of break; which breaks the whole 'for' statement.
                            continue;
                        }
                        // If the player doesn't send any packets in a while, disconnect him
                        if (client.Timeout > 0 && client.LastReceiveTime + client.Timeout < TickTime)
                        {

                            RConClients.Remove(client);
                            continue;
                        }
                        // Process up to 100 packets from this player's InQueue at a time. (This is processed after checking for disconnected sockets. 
                        for (int e = 0; e < 100 && client.NextPacket(out packet); e++)
                        {
                            ProcessRConPacket(client, packet);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Processes a single packet.
        /// </summary>
        void ProcessPlayerPacket(NTcpPlayer player, NBuffer packet)
        {       
            //Begin reading the packet. Returns the BinaryReader loaded with the memorystream to be read. 
            BinaryReader reader = packet.BeginReading();

            //Identifies the packet.
            if (packet.packet != 0)
            {
                Packet p = packet.packet;
                if (player.State != NTcpProtocol.ConnectionState.connected && p != Packet.RequestClientSetup) { RemovePlayer(player); }

                Tools.Print("Received Packet: " + p.ToString());
                //Put higher priority cases at the top.
                switch (p)
                {
                    case Packet.RFC:
                        {
                            ReceiveRFC(player, reader, packet);
                            break;
                        }
                    case Packet.Empty:
                        {
                            break;
                        }
                    case Packet.Ping:
                        {
                            int playerTime = reader.ReadInt32();                  
                            break;
                        }
                    case Packet.RequestJoinChannel:
                        {
                            JoinChannel(reader.ReadInt32(), player);
                            break;
                        }
                    case Packet.RequestLeaveChannel:
                        {
                            LeaveChannel(reader.ReadInt32(), player);
                            break;
                        }       
                    case Packet.RequestClientSetup:
                        {
                            ClientSetup(reader, player);
                            break;
                        }                 
                    case Packet.RequestCreateObject:
                        {
                            NetworkObject obj = (NetworkObject)reader.ReadObject();
                            ClientRequestCreateObject(obj, player);
                            break;
                        }
                    case Packet.TestData:
                        {
                            Tools.Print(reader.ReadBoolean().ToString());
                            Tools.Print(reader.ReadInt32().ToString());
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Processes RCon client packets.
        /// </summary>
        void ProcessRConPacket(NRConClient client, NBuffer packet)
        {
            BinaryReader reader = packet.BeginReading();
            Packet p = packet.packet;
            Tools.Print("Received from rcon client");

            if (client.State != NTcpProtocol.ConnectionState.connected && p != Packet.RequestClientSetup) { RemoveRConClient(client); }
            switch (p)
            {
                
                case Packet.RequestClientSetup:
                    {
                        BinaryWriter writer = client.BeginSend(Packet.ResponseClientSetup);

                        if (client.ProtocolVersion == reader.ReadInt32())
                        {
                            writer.Write(true);
                            client.EndSend();
                            client.State = NTcpProtocol.ConnectionState.connected;
                        }
                        else
                        {
                            writer.Write(false);
                            client.EndSend();
                            RemoveRConClient(client);
                        }
                        break;
                    }

                case Packet.RConRequestAuthenticate:
                    {
                        BinaryWriter writer = client.BeginSend(Packet.RConResponseAuthenticate);
                        if (RConPassword == reader.ReadString())
                        {
                            Tools.Print("RCon client [" + client.thisSocket.RemoteEndPoint.ToString() + "] connected.");
                            writer.Write(true);
                        }
                        else
                        {
                            writer.Write(false);
                        }
                        client.EndSend();
                        break;
                    }
                case Packet.RConStopGameServer:
                    {
                        Tools.Print("Stopping");
                        if (MainTcp != null && RunGameServer) StopGameServer();
                        break;
                    }
            }
        }
    }
}
