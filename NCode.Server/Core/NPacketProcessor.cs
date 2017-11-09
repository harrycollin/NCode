using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NCode.Core;
using NCode.Core.Entity;
using NCode.Core.Protocols;
using NCode.Server.Systems.Channel;
using static NCode.Core.Utilities.Tools;
using Buffer = NCode.Core.Buffer;

namespace NCode.Server.Core
{
    public sealed class NPacketProcessor
    {
        /// <summary>
        ///     A dictionary of custom packet listeners. The key is the packet.
        /// </summary>
        private readonly Dictionary<Packet, OnPacket> _packetHandlers = new Dictionary<Packet, OnPacket>();

        public TNUdpProtocol MainUdp;

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
            var packetType = (Packet) reader.ReadByte();

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
                        BinaryWriter writer = player.BeginSend(Packet.ResponseClientSetup);

                        if (NGameServer.ServerProtocolVersion == protocolVersion)
                        {
                            //Send information about the other players including self.
                            foreach (var p in NPlayer.PlayerDictionary.Values)
                            {
                                writer.WriteObject(p.PlayerInfo);
                                player.EndSend();
                            }

                            writer = player.BeginSend()
                            PrintInfo($"Player {player.ClientId} has connected.");
                        }
                        else
                        {
                            player.BeginSend(Packet.Disconnect);
                            player.EndSend();
                            Print($"Player {player.ClientId} has failed to connect.");
                        }
                        player.EndSend();
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
                        entity = (NNetworkEntity) reader.ReadObject();
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
                        entity = (NNetworkEntity) reader.ReadObject();
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
                        entity = (Guid) reader.ReadObject();
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
                        entity = (Guid) reader.ReadObject();
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

        private void ProcessForwardPacket(NPlayer player, Buffer buffer, Packet packet, BinaryReader reader,
            bool reliable)
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
                    else if (i.IsPlayerUdpConnected) MainUdp.Send(buffer, i.UdpEndpoint);
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
                        else if (otherPlayer.IsPlayerUdpConnected) MainUdp.Send(buffer, otherPlayer.UdpEndpoint);
                    }
                }
            }
        }
    }
}
