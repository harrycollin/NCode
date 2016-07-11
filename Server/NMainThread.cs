using NCode.BaseClasses;
using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCode
{
    public class NMainThread : NMainFunctions
    {
        /// <summary>
        /// Starts the server. 
        /// </summary>
        public void Start(string name, int tcpport, int udpport, string password)
        {
            //Checks to see if the Tcp listener started without error. 
            if (StartListening(tcpport))
            {
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

               

                //Start the main thread.
                MainThread = new Thread(MainThreadLoop);
                MainThread.Start();
            }
        }

        void AddNewObject()
        {
            Random r = new Random();
            
            for(int i = 0; i < 100; i++)
            {
                NetworkObject o = new NetworkObject();
                o.LastChannelID = r.Next(20);
                o.GUID = Generate.GenerateGUID();
                DatabaseRequest.SaveNewObject(o);
            }
        }

        //Starts listening for tcp connections on the specified port.
        public bool StartListening(int TcpPort)
        {
            //Lock the state. Stops anything else using these resources.
            lock (Lock)
            {
                //Stops the Main TcpProtocol if its already running.
                if (MainTcp != null)
                {
                    MainTcp.Stop();
                    MainTcp = null;
                }

                //Assign the Tcp port in the MainFunctions
                TcpListenPort = TcpPort;

                if (TcpPort != 0)
                {
                    try
                    {   
                        //Start a new TcpListener on the specifed port with a max number of backlogged connections. 
                        MainTcp = new TcpListener(IPAddress.Any, TcpPort);
                        MainTcp.Start(50);
                        Tools.Print("Game Server started on port: " + TcpPort);
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        Tools.Print("@NMainThread.StartListening", Tools.MessageType.error, ex);
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// The main thread on the server. Cycles through pending connections, queued packets etc.
        /// </summary>
        void MainThreadLoop()
        {
            Tools.Print("Starting main thread.");

            //Loop forever until server is stopped.
            for (;;)
            {

                lock (Lock)
                {
                    //Will be used when UDP is inplemented
                    IPEndPoint IP;

                    //The tick time divided by 10000 as a counter (used to calculate ping etc.)
                    TickTime = DateTime.UtcNow.Ticks / 10000;

                    //Add pending connections
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
                            Tools.Print("@MainThreadLoop - add pending connection", Tools.MessageType.error, e);
                        }                                             
                    }

                    //Loops through each player in the player list.
                    for(int i = 0; i < MainPlayers.Count; i++)
                    {
                        //Access a player by index
                        NTcpPlayer player = MainPlayers[i];

                        // Remove disconnected players (Checks the player's TcpProtocol to see if the socket is still connected)
                        if (!player.isSocketConnected)
                        {
                            RemovePlayer(player);

                            //Breaks the current iterartion instead of break; which breaks the whole 'for' statement.
                            continue;
                        }

                        // If the player doesn't send any packets in a while, disconnect him
                        if (player.Timeout > 0 && player.LastReceiveTime + player.Timeout < TickTime)
                        {
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

        /// <summary>
        /// Processes a single packet.
        /// </summary>
        void ProcessPlayerPacket(NTcpPlayer player, NPacketContainer packet)
        {
            //Begin reading the packet. Returns the BinaryReader loaded with the memorystream to be read. 
            BinaryReader reader = packet.BeginReading();
            
            //Identifies the packet.
            if(packet.packetid != 0) { 
            Packet p = packet.packetid;

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
                    case Packet.RequestPing:
                        {
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
                    case Packet.RequestClientInfo:
                        {
                            BinaryWriter writer = player.BeginSend(Packet.ResponseClientInfo);

                            if (player.ProtocolVersion == reader.ReadInt32())
                            {
                                player.State = NTcpProtocol.ConnectionState.connected;
                                writer.Write(true);
                                player.EndSend();
                            }
                            else
                            {
                                writer.Write(false);
                                player.EndSend();
                                RemovePlayer(player);
                            }

                            break;
                        }
                    case Packet.TextChat:
                        {
                            string text = reader.ReadString();
                            Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + ": " + text);
                            for (int i = 0; i < player.ConnectedChannels.size; i++)
                            {
                                NChannel channel = player.ConnectedChannels[i];

                                for (int e = 0; e < channel.Players.size; e++)
                                {
                                    NTcpPlayer NextPlayer = channel.Players[e];
                                    if (NextPlayer != player)
                                    {
                                        BinaryWriter writer = NextPlayer.BeginSend(Packet.TextChat);
                                        writer.Write(text);
                                        NextPlayer.EndSend();
                                    }
                                }
                            }
                            break;
                        }
                    
                    case Packet.RequestCreateObject:
                        {
                            NetworkObject obj = (NetworkObject)Converters.ConvertByteArrayToObject(reader.ReadByteArrayEx());
                            ClientRequestCreateObject(obj, player);
                            break;
                        }
                }
            }           
        }
    }
}
