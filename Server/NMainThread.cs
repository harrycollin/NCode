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

        public void Start(int port)
        {
            if (StartListening(port))
            {
                MainThread = new Thread(MainThreadLoop);
                MainThread.Start();
            }
        }

        public bool StartListening(int TcpPort)
        {
            lock (Lock)
            {
                if (MainTcp != null)
                {
                    MainTcp.Stop();
                    MainTcp = null;
                }

                TcpListenPort = TcpPort;

                if (TcpPort != 0)
                {
                    try
                    {
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

        void MainThreadLoop()
        {
            Tools.Print("Starting main thread.");
            for (;;)
            {

                lock (Lock)
                {
                    IPEndPoint IP;
                    TickTime = DateTime.UtcNow.Ticks / 10000;

                    //Add pending connections
                    while (MainTcp != null && MainTcp.Pending())
                    {
                        Socket socket = MainTcp.AcceptSocket();
                        try
                        {
                            IPEndPoint remoteClient = (IPEndPoint)socket.RemoteEndPoint;
                            if (remoteClient == null)
                            {
                                socket.Close(); socket = null; 
                            }
                            else
                            {
                                AddPlayer(socket);
                            }
                        }
                        catch (Exception e)
                        {
                            Tools.Print("@MainThreadLoop - add pending connection", Tools.MessageType.error, e);
                        }                                             
                    }

                    for(int i = 0; i < MainPlayers.Count;)
                    {
                        NTcpPlayer player = MainPlayers[i];

                        // Remove disconnected players
                        if (!player.isSocketConnected)
                        {
                            RemovePlayer(player);
                            continue;
                        }

                        // Process up to 100 packets from this player's InQueue at a time.
                        for(int e = 0; e < 100 && player.NextPacket(out packet); e++)
                        {
                            ProcessPlayerPacket(player, packet);
                            continue;
                        }
                       
                        // If the player doesn't send any packets in a while, disconnect him
                        if (player.Timeout > 0 && player.LastReceiveTime + player.Timeout < TickTime)
                        {
                             RemovePlayer(player);
                             continue;
                        }                                              
                        i++;
                    }
                }
            }
        }

        void ProcessPlayerPacket(NTcpPlayer player, NPacketContainer packet)
        {
            BinaryReader reader = packet.BeginReading();
            
            Packet p = packet.packet;

            switch (p)
            {
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
                case Packet.Broadcast:
                    {
                        for (int i = 0; i < player.ConnectedChannels.size; i++)
                        {
                            NChannel channel = player.ConnectedChannels[i];

                            for (int e = 0; e < channel.Players.size; e++)
                            {
                                NTcpPlayer NextPlayer = channel.Players[e];
                                Tools.Print("Sending Broadcast");
                                BinaryWriter writer = NextPlayer.BeginSend(Packet.Broadcast);
                                writer.Write(reader.ReadString());
                                NextPlayer.EndSend();
                            }
                        }
                        break;
                    }
                case Packet.TextChat:
                    {
                        string text = reader.ReadString();
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
