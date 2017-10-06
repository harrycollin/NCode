using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NCode.Core;
using Buffer = NCode.Core.Buffer;
using NCode.Core.Utilities;
using NCode.Core.Entity;
using NCode.Core.Protocols;
using NCode.Server.Systems.Channel;
using static NCode.Core.Utilities.Tools;

namespace NCode.Server.Core
{
    public sealed class NPacketProcessor
    {
        /// <summary>
        /// A dictionary of custom packet listeners. The key is the packet. 
        /// </summary>
        private readonly Dictionary<Packet, OnPacket> _packetHandlers = new Dictionary<Packet, OnPacket>();

        public TNUdpProtocol mainUdp;

        public delegate void OnPacket(Packet response, BinaryReader reader);

        public void AddCustomHandler(Packet packet, OnPacket handler)
        {
            _packetHandlers.Add(packet, handler);
        }

        public bool ProcessPacket(NPlayer player, Buffer buffer, bool reliable)
        {
            //Begin reading the packet. Returns the BinaryReader loaded with the memorystream to be read. 
            BinaryReader reader = buffer.BeginReading();

            //Identifies the packet.
            Packet packetType = (Packet) reader.ReadByte();
            
            if (packetType == 0) return true;
            //Filters out any packets that have custom handlers. 
            OnPacket callback;

            if (_packetHandlers.TryGetValue(packetType, out callback) && callback != null)
            {
                callback(packetType, reader);
                return true;
            }
            //Tools.Print($"Packet {packetType.ToString()}");
            switch (packetType)
            {

#if NCODE_SERVER
                case Packet.Ping:
                {
                    
                    break;
                }
                case Packet.RequestClientSetup:
                {
                    try
                    {
                        int protocolVersion = reader.ReadInt32();
                        BinaryWriter writer = player.BeginSend(Packet.ResponseClientSetup);

                        if (NGameServer.ServerProtocolVersion == protocolVersion)
                        {
                            writer.Write(player.ClientId);
                            writer.Write(NGameServer.UdpListenPort);
                            Tools.Print($"Player {player.ClientId} has connected.");
                        }
                        else
                        {
                            writer.Write(-1);
                            Tools.Print($"Player {player.ClientId} has failed to connect.");
                        }
                        player.EndSend();
                    }
                    catch (Exception e)
                    {
                        Tools.Print($"Cannot parse {packetType} packet to type 'int'. Client setup failed.", Tools.MessageType.Error, e);
                        throw;
                    }
                    break;
                }
                case Packet.CreateEntity:
                    {
                        int channelID = -1;
                        NNetworkEntity entity = null;
                        try
                        {
                            channelID = reader.ReadInt32();
                            entity = (NNetworkEntity)reader.ReadObject();
                        }
                        catch (NullReferenceException e)
                        {
                            Tools.Print("Null reference exception.", MessageType.Error);
                        }

                        if (entity != null)
                        {
                            NEntityCache.AddOrUpdate(entity);
                            NChannel.Channels[channelID].AddEntity(entity.Guid);
                        }
                        break;
                    }
#endif

                case Packet.UpdateEntity:
                    {
                        NNetworkEntity entity = null;
                        try
                        {
                            entity = (NNetworkEntity)reader.ReadObject();
                        }
                        catch (NullReferenceException) { }

                        if (entity != null)
                        {
                            NEntityCache.AddOrUpdate(entity);
                            foreach (var otherPlayer in NPlayer.PlayerDictionary.Values.ToList())
                            {
                                if (otherPlayer == player) continue;
                                BinaryWriter writer = otherPlayer.BeginSend(Packet.UpdateEntity);
                                writer.WriteObject(entity);
                                otherPlayer.EndSend();
                            }
                        }
                        break;
                    }
                case Packet.DestroyEntity:
                    {
                        Guid entity = Guid.Empty;
                        try
                        {
                            entity = (Guid)reader.ReadObject();
                        }
                        catch (NullReferenceException) { }

                        if(entity != null)
                        {
                            NEntityCache.Remove(entity);
                            foreach(var otherPlayer in NPlayer.PlayerDictionary.Values.ToList())
                            {
                                if (otherPlayer == player) continue;
                                BinaryWriter writer = otherPlayer.BeginSend(Packet.DestroyEntity);
                                writer.WriteObject(entity);
                                otherPlayer.EndSend();
                            }
                        }

                        break;
                    }
                case Packet.TransferEntity:
                    {
                        Guid entity = Guid.Empty;
                        int channelA = 0;
                        int channelB = 0;

                        try
                        {
                            entity = (Guid)reader.ReadObject();
                            channelA = (int)reader.ReadInt32();
                            channelB = (int)reader.ReadInt32();
                        }
                        catch (EndOfStreamException endOfStream)
                        {
                            Print($"Exception reading {packetType}.", MessageType.Error, endOfStream);
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
                        Tools.Print($"Packet with the ID:{packetType} has not been defined for processing.", Tools.MessageType.Error);
                        return false;
                    }
            }
            return true;
        }

        void ProcessForwardPacket(NPlayer player, Buffer buffer, Packet packet, BinaryReader reader, bool reliable)
        {
            int start = buffer.position - 5;
            buffer.position = start;

            //Simply send the packet to all players
            if (packet == Packet.ForwardToAll)
            {
                foreach(var i in NPlayer.PlayerDictionary.Values.ToList())
                {
                    if (i == player) continue;
                    if (reliable) i.SendTcpPacket(buffer);
                    else if (i.IsPlayerUdpConnected) mainUdp.Send(buffer, i.UdpEndpoint);
                    
                }
            }
            //More work is needed if it is to all connected channels
            else if (packet == Packet.ForwardToChannels)
            {
                System.Collections.Generic.List<NPlayer> alreadySentTo = new System.Collections.Generic.List<NPlayer>();

                foreach(var channel in NChannel.Channels.Values.ToList())
                {
                    if (!channel.HasPlayer(player)) continue;

                    foreach(var otherPlayer in channel.GetPlayers())
                    {
                        if (otherPlayer == player) continue;
                        if (alreadySentTo.Contains(otherPlayer)) continue;
                        alreadySentTo.Add(otherPlayer);
                
                        if (reliable) otherPlayer.SendTcpPacket(buffer);
                        else if (otherPlayer.IsPlayerUdpConnected) mainUdp.Send(buffer, otherPlayer.UdpEndpoint);
                       
                    }
                }
            }
        }
    }
}
