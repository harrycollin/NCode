
using NCode.Core;
using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode
{
    public class NChannel
    {
        public Dictionary<Guid, NetworkObject> channelObjects = new Dictionary<Guid, NetworkObject>();
        /// <summary>
        /// Acts as a slave to all objects that need owners and will act as a slave to sync them until a reliable owner can be found.
        /// </summary>
        public NTcpPlayer ChannelSlave;
        public System.Collections.Generic.List<NTcpPlayer> Players = new System.Collections.Generic.List<NTcpPlayer>();
        public int ID = 0;
        public int PlayerLimit = 300;

        public bool AddPlayer(NTcpPlayer player)
        {
            if(!IsPlayerConnected(player) && Players.Count < PlayerLimit)
            {
                Players.Add(player);
                foreach (KeyValuePair<Guid, NetworkObject> i in channelObjects)
                {
                    BinaryWriter writer = player.BeginSend(Packet.ClientObjectUpdate);
                    writer.WriteObject(i.Value);
                    player.EndSend();
                }
            }
            return false;
        }

        public bool RemovePlayer(NTcpPlayer player)
        {
            if (IsPlayerConnected(player))
            {
                //Remove all none persistent objects in the channel for this player
                int count = 0;

                foreach (KeyValuePair<Guid, NetworkObject> i in channelObjects)
                {
                    if(i.Value.NetworkOwnerGUID == player.ClientGUID && !i.Value.Persistant)
                    {
                        count++;
                        channelObjects.Remove(i.Key);
                    }
                }
                Tools.Print("Removed " + count.ToString() + " Network objects");
                //Remove the player
                Players.Remove(player);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns bool on whether the player is in this channel
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool IsPlayerConnected(NTcpPlayer player)
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
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool DoesObjectExist(Guid guid)
        {
            if(guid != null)
            {
                return channelObjects.ContainsKey(guid);
            }
            return false;
        }

        /// <summary>
        /// Removes a network object from this channel and returns it for new allocation.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public NetworkObject RemoveObjectForTransfer(Guid guid)
        {
            if(guid != null)
            {
                if (channelObjects.ContainsKey(guid))
                {
                    NetworkObject obj = channelObjects[guid];
                    channelObjects.Remove(guid);
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a Network Object to the channel.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool AddObject(NetworkObject obj)
        {
            if(obj != null && obj.GUID != null && !DoesObjectExist(obj.GUID))
            {
                channelObjects.Add(obj.GUID, obj);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a object from the channel (Use transfer object when moving to other channels)
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool RemoveObject(Guid guid)
        {
            if(guid != null && DoesObjectExist(guid))
            {
                channelObjects.Remove(guid);
                return true;
            }
            return false;
        }

    }
}
