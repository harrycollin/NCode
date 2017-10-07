using System;
using System.Collections.Generic;
using System.IO;
using NCode.Core;
using NCode.Core.BaseClasses;
using NCode.Core.TypeLibrary;
using NCode.Core.Utilities;
using NCode.Server.Core;

namespace NCode.Server
{
    [Obsolete("Use NChannel instead")]
    public class Channel
    {
        public static Core.Utilities.List<Channel> Channels = new Core.Utilities.List<Channel>();
        public static Dictionary<int, Channel> ChannelDictionary = new Dictionary<int, Channel>();

        public static int ChannelCount { get { return Channel.Channels.size; } }

        //Channel Range
        public NVector3 point00; //Bottom Left
        public NVector3 point01; //Top Left
        public NVector3 point10; //Bottom Right
        public NVector3 point11; //Top Right

        public int ID = 0;
        public Dictionary<Guid, NetworkObject> ChannelObjects = new Dictionary<Guid, NetworkObject>(); 
        public Core.Utilities.List<Core.NPlayer> Players = new Core.Utilities.List<Core.NPlayer>();


        /// <summary>
        /// Transfers an object from channelA to channelB
        /// </summary>
        public static bool TransferObject(NetworkObject networkObject, Channel channelA, Channel channelB)
        {
            if(channelA.IsObjectConnected(networkObject.GUID) && !channelB.IsObjectConnected(networkObject.GUID))
            {
                channelB.AddObject(channelA.RemoveObjectForTransfer(networkObject.GUID));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a player from all channels
        /// </summary>
        public static void DisconnectPlayer(Core.NPlayer player)
        {
            foreach(Channel channel in Channels)
            {
                if (channel.IsPlayerConnected(player))
                {
                    channel.RemovePlayer(player);
                }
            }
        }

        /// <summary>
        /// Adds a channel to the list of channels
        /// </summary>
        public static void AddChannel(Channel channel)
        {
            if (!ChannelDictionary.ContainsKey(channel.ID))
            {
                Channels.Add(channel);
                ChannelDictionary.Add(channel.ID, channel);
            }
        }

        /// <summary>
        /// Globally updates an object in any channel that it is connected to.
        /// </summary>
        public static void UpdateChannelObject(NetworkObject networkObject)
        {
            //Here we are caching the players we already sent the object update to.
            Core.Utilities.List<Guid> PlayersWeHaveSentTo = new Core.Utilities.List<Guid>(); 
            for(int i = 0; i < ChannelCount; i++)
            {
                if (Channels[i].IsObjectConnected(networkObject.GUID))
                {
                    for(int ii = 0; ii < Channels[i].Players.size; ii++)
                    {
                        //If we already sent it to them then there is no need to send it again.
                        if (PlayersWeHaveSentTo.Contains(Channels[i].Players[ii].ClientGuid)) continue;
                        BinaryWriter writer = Channels[i].Players[ii].BeginSend(Packet.ClientObjectUpdate);
                        writer.WriteObject(networkObject);
                        Channels[i].Players[ii].EndSend();
                        PlayersWeHaveSentTo.Add(Channels[i].Players[ii].ClientGuid);
                    }
                }
            }
            PlayersWeHaveSentTo.Clear();
            PlayersWeHaveSentTo = null;
        }

        /// <summary>
        /// Returns a list of channels that a player is connected to.
        /// </summary>
        public static Core.Utilities.List<Channel> GetConnectedChannels (Core.NPlayer player)
        {
            Core.Utilities.List<Channel> channels = new Core.Utilities.List<Channel>();
            for(int i = 0; i < Channels.size; i++)
            {
                if (Channels[i].IsPlayerConnected(player))
                {
                    channels.Add(Channels[i]);
                }
            }
            return channels;
        }

        /// <summary>
        /// Adds a player to this channel providing they aren't already connected.
        /// </summary>
        /// <returns></returns>
        public bool AddPlayer(Core.NPlayer player)
        {
            if(!IsPlayerConnected(player))
            {
                
                Players.Add(player); //Add the player to the list
                if (player.playerObject != null) AddObject(player.playerObject);

                Tools.Print(player.ClientGuid + " has connected to channel " + ID);

                foreach(KeyValuePair<Guid, NetworkObject> i in ChannelObjects)
                {
                    if (!i.Value.Persistant && i.Value.NetworkOwnerGUID == player.ClientGuid) continue;
                    BinaryWriter writer = player.BeginSend(Packet.ClientObjectUpdate);
                    writer.WriteObject(i.Value);
                    player.EndSend();
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a player from this channel providing they're connected.
        /// </summary>
        /// <returns></returns>
        public bool RemovePlayer(Core.NPlayer player)
        {
            if (IsPlayerConnected(player))
            {
                //Remove all none persistent objects in the channel for this player
                int count = 0;
                //Create a list to store the objects that need removing. You can't modify the dictionary whilst it is iterating.
                Core.Utilities.List<KeyValuePair<Guid, NetworkObject>> objectsToRemove = new Core.Utilities.List<KeyValuePair<Guid, NetworkObject>>();

                lock (ChannelObjects)
                {
                    //Find the objects to remove.
                    foreach (KeyValuePair<Guid, NetworkObject> i in ChannelObjects)
                    {
                        if (i.Value.NetworkOwnerGUID == player.ClientGuid && !i.Value.Persistant)
                        {
                            count++;
                            objectsToRemove.Add(i);
                        }
                    }

                    //Remove them without iterating the dictionary
                    for(int i = 0; i < objectsToRemove.Count; i++)
                    {
                        for(int p = 0; p < Players.Count; p++)
                        {
                            BinaryWriter writer = Players[p].BeginSend(Packet.ResponseDestroyObject);
                            writer.WriteObject(objectsToRemove[i].Key);
                            Tools.Print("Destroying " + objectsToRemove[i].Key);
                            Players[p].EndSend();
                        }
                        ChannelObjects.Remove(objectsToRemove[i].Key);
                    }

                }
                Tools.Print("Removed " + count.ToString() + " Network objects");
                //Remove the player
                lock (Players)
                {
                    Players.Remove(player);
                }
                Tools.Print(player.ClientGuid + " disconnected from channel " + ID);

                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns bool on whether the player is in this channel.
        /// </summary>
        public bool IsPlayerConnected(Core.NPlayer player)
        {
            for(int i = 0; i < Players.Count;)
            {
                if (Players.Contains(player)) { return true; }

                // Add up afterwards otherwise you miss the first element in the list. C# Lists start at 0.
                i++;
            }
            return false;
        }

        /// <summary>
        /// Checks to see if the object already exists in this channel
        /// </summary>
        public bool IsObjectConnected(Guid guid)
        {
            if(guid != null)
            {
                return ChannelObjects.ContainsKey(guid);
            }
            return false;
        }

        /// <summary>
        /// Removes a network object from this channel and returns it for new allocation.
        /// </summary>
        NetworkObject RemoveObjectForTransfer(Guid guid)
        {
            if(guid != null)
            {
                if (ChannelObjects.ContainsKey(guid))
                {
                    NetworkObject obj = new NetworkObject();
                    obj = ChannelObjects[guid];
                    ChannelObjects.Remove(guid);
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a Network Object to the channel and updates all connected players.
        /// </summary>
        /// <returns></returns>
        public bool AddObject(NetworkObject obj)
        {
            if(obj != null && obj.GUID != Guid.Empty && !IsObjectConnected(obj.GUID))
            {
                Tools.Print(obj.GUID.ToString() + " was added to Channel:" + ID);
                lock (ChannelObjects) { 
                ChannelObjects.Add(obj.GUID, obj);
                for(int i = 0; i < Players.Count; i++)
                {
                    if (!obj.Persistant && obj.NetworkOwnerGUID == Players[i].ClientGuid) continue;
                    BinaryWriter writer = Players[i].BeginSend(Packet.ClientObjectUpdate);
                    writer.WriteObject(obj);
                    Players[i].EndSend();
                    Tools.Print(obj.GUID + " was sent to " + Players[i].ClientGuid);
                }
                return true;}
            }
            return false;
        }

        /// <summary>
        /// Removes a object from the channel and updates all connnected players
        /// </summary>
        public bool RemoveObject(Guid guid)
        {
            if(guid != null && IsObjectConnected(guid))
            {
                ChannelObjects.Remove(guid);
                for (int i = 0; i < Players.Count; i++)
                {
                    BinaryWriter writer = Players[i].BeginSend(Packet.ResponseDestroyObject);
                    writer.WriteObject(guid);
                    Players[i].EndSend();
                }
                return true;
            }
            return false;
        }

    }
}
