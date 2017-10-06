using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCode.Core.Utilities;
using NCode.Server.Core;
using System.Collections.Generic;
using NCode.Core.Entity;
using System.IO;
using NCode.Core;

namespace NCode.Server.Systems.Channel
{
    public class NChannel : IPlayerManagementSystem
    {
#region STATIC VARS
        //Keeps track of the current number of channels. Used to assign a channelID when a new channel is created. 
        public static int ChannelCount;

        //Static list of all channels 
        public static Dictionary<int, NChannel> Channels = new Dictionary<int, NChannel>();

        public static readonly uint MaxChannels = 1000;

#endregion

        /// <summary>
        /// The unique channel ID. This is unique for each channel and automatically assigned on creation. This cannot be changed. 
        /// </summary>
        public readonly int ID;
                
        /// <summary>
        /// A list of players by their ID's
        /// </summary>
        private readonly System.Collections.Generic.List<int> Players = new System.Collections.Generic.List<int>();

        private readonly System.Collections.Generic.List<Guid> Entities = new System.Collections.Generic.List<Guid>();

        private readonly System.Collections.Generic.List<NCode.Core.Buffer> PersistantPackets = new System.Collections.Generic.List<NCode.Core.Buffer>();

        private NChannel(int ChannelID = -1)
        {
            ChannelCount++;

            if (ChannelID != -1)
            {
                if (!Channels.ContainsKey(ChannelID))
                {
                    ID = ChannelID;
                    Channels.Add(ID, this);
                    Tools.Print($"Channel {ID} has been created.", Tools.MessageType.Notification);

                }
                else
                {
                    Tools.Print($"Couldn't create a channel with the ID {ChannelID} as it already exists.", Tools.MessageType.Error);
                }
            }
            else if (ChannelID == -1)
            {
                for (var i = 0; i < MaxChannels; i++)
                {
                    if (!Channels.ContainsKey(i))
                    {
                        ID = i;
                        Channels.Add(i, this);
                        Tools.Print($"Channel {ID} has been created.", Tools.MessageType.Notification);
                        break;
                    }
                }
            }
            else
            {
                Tools.Print("A new channel couldn't be created. Max channels reached", Tools.MessageType.Error);
            }          
            NServerEvents.playerDisconnected += LeaveChannel;
        }
     
        ~NChannel()
        {
            ChannelCount--;
        }

        public static bool JoinChannel(NPlayer player, int ChannelID)
        {
            if (Channels.ContainsKey(ChannelID))
            {
                if (!Channels[ChannelID].JoinChannel(player))
                {
                    return true;
                }
            }
            else
            {
                NChannel newChannel = new NChannel(ChannelID);
                newChannel.JoinChannel(player);
            }
            return false;
        }

        public static bool LeaveChannel(NPlayer player, int ChannelID)
        {
            if (Channels.ContainsKey(ChannelID))
            {
                if (Channels[ChannelID].Players.Contains(player.ClientId))
                {
                    Channels[ChannelID].LeaveChannel(player);
                    return true;
                }
                else
                {
                    Tools.Print("STR_CHANNEL_PLAYERLEAVENULLCHANNEL", null, player.ClientGuid, ChannelID);
                }
            }
            return false;
        }

        public bool JoinChannel(NPlayer player)
        {
            lock (Players)
            {
                if (!Players.Contains(player.ClientId))
                {
                    Players.Add(player.ClientId);

                    //Send each object in the channel to the newly joined player. 
                    foreach(var entity in Entities)
                    {
                        BinaryWriter writer = player.BeginSend(Packet.CreateEntity);
                        writer.WriteObject(NEntityCache.GetEntity(entity));
                        player.EndSend();
                        Tools.Print($"Entity: {entity} has been sent to Player {player.ClientId}.", null);
                    }
                    Tools.Print($"Player:{player.ClientId} has joined Channel {ID}.");


                    return true;
                }
                Tools.Print("EC1003", Tools.MessageType.Error, null, player.ClientId, ID);
                return false;
            }
        }

        public void LeaveChannel(NPlayer player)
        {
            lock (Players)
            {
                if (Players.Contains(player.ClientId))
                {
                    Players.Remove(player.ClientId);

                    //Send each object in the channel to the newly joined player. 
                    foreach (var entity in Entities)
                    {
                        BinaryWriter writer = player.BeginSend(Packet.DestroyEntity);
                        writer.WriteObject(entity);
                        player.EndSend();
                        Tools.Print($"Entity: {entity} has been removed from Player {player.ClientId}.", null);
                    }

                    Tools.Print($"Player:{player.ClientId} has left Channel {ID}.");

                }
            }
        }

        public bool HasPlayer(NPlayer player)
        {
            if (Players.Contains(player.ClientId))
            {
                return true;
            }
            return false;
        }

        public bool HasEntity(NNetworkEntity entity)
        {
            if (Entities.Contains(entity.Guid))
            {
                return true;
            }
            return false;
        }

        public bool HasEntity(Guid entity)
        {
            if (Entities.Contains(entity))
            {
                return true;
            }
            return false;
        }

        public bool AddEntity(Guid entity)
        {
            if (!Entities.Contains(entity))
            {
                Entities.Add(entity);
                foreach(var player in Players)
                {
                    BinaryWriter writer = NPlayer.GetPlayer(player).BeginSend(Packet.CreateEntity);
                    writer.WriteObject(NEntityCache.GetEntity(entity));
                    NPlayer.GetPlayer(player).EndSend();
                }
                Tools.Print($"Entity:{entity} has joined Channel {ID}.");

                return true;
            }
            Tools.Print($"Channel {ID} already contains Entity: {entity}.");
            return false;
        }

        public bool RemoveEntity(Guid entity)
        {
            if (Entities.Contains(entity))
            {
                Entities.Remove(entity);
                foreach (var player in Players)
                {
                    BinaryWriter writer = NPlayer.GetPlayer(player).BeginSend(Packet.DestroyEntity);
                    writer.WriteObject(entity);
                    NPlayer.GetPlayer(player).EndSend();
                }
                return true;
            }
            return false;
        }

        public static bool TransferEntity(Guid entity, NChannel channelA, NChannel channelB)
        {
            if (entity != null && channelA != null && channelB != null)
            {
                if (channelA.HasEntity(entity))
                {
                    if (!channelB.HasEntity(entity))
                    {
                        channelA.RemoveEntity(entity);
                        channelB.AddEntity(entity);
                        Tools.Print($"Entity:{entity} has been transfered from Channel {channelA.ID} to Channel {channelB.ID}.");
                        return true;
                    }
                    else
                    {
                        Tools.Print($"Can't transfer Entity:{entity} from Channel {channelA.ID} to Channel {channelB.ID}. Channel {channelB.ID} already contains Entity: {entity}.", Tools.MessageType.Error);
                    }
                }
                else
                {
                    Tools.Print($"Can't transfer Entity:{entity} from Channel {channelA.ID} to Channel {channelB.ID}. Channel {channelA.ID} doesn't contain Entity: {entity}.", Tools.MessageType.Error);
                }
            }
            else
            {
                Tools.Print($"Can't carry out Entity channel transfer. One of the parameters is null", Tools.MessageType.Error);
            }
            return false;
        }

        public static int CreateChannel()
        {
            return new NChannel().ID;
        }

        public static bool CloseChannel(int channelId)
        {
            lock (Channels)
            {
                if (Channels.ContainsKey(channelId))
                {
                    lock (Channels)
                    {
                        Channels.Remove(channelId); 
                        Tools.Print("STR_CHANNEL_CLOSED", Tools.MessageType.Warning, null, channelId);
                        return true;
                    }
                }

            }
            return false;
        }

        public System.Collections.Generic.List<NPlayer> GetPlayers()
        {
            System.Collections.Generic.List<NPlayer> players = new System.Collections.Generic.List<NPlayer>();

            foreach(var ID in Players)
            {
                players.Add(NPlayer.GetPlayer(ID));
            }
            return players;
        }
    }
}
