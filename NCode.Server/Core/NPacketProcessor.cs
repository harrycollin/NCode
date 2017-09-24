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
            Tools.Print($"Packet {packetType.ToString()}");
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
                                BinaryWriter writer = otherPlayer.BeginSend(Packet.CreateEntity);
                                writer.WriteObject(entity);
                                otherPlayer.EndSend();
                            }
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
                case Packet.ForwardToAll:
                    {
                        foreach(var otherPlayer in NPlayer.PlayerDictionary.Values.ToList())
                        {
                            if (otherPlayer == player) continue;
                            if (reliable)
                            {
                                otherPlayer.SendTcpPacket(buffer);
                            }
                            else
                            {
                                var udpEndpoint = NPlayer.PlayerUdpEnpointDictionary.FirstOrDefault(x => x.Value == otherPlayer).Key;
                                mainUdp.Send(buffer, udpEndpoint);
                            }
                        }                  
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
    }
}
