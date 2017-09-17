using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NCode.Core.Utilities;
using NCode.Server.Core;

namespace NCode.Server.Systems.Channel
{
    public class NChannel : IPlayerManagementSystem
    {
#region STATIC VARS
        //Keeps track of the current number of channels. Used to assign a channelID when a new channel is created. 
        public static int ChannelCount;

        //Static list of all channels 
        public static System.Collections.Generic.List<NChannel> Channels = new System.Collections.Generic.List<NChannel>();

#endregion

        /// <summary>
        /// The unique channel ID. This is unique for each channel and automatically assigned on creation. This cannot be changed. 
        /// </summary>
        public readonly int ID;

        

        public System.Collections.Generic.List<NPlayer> Players = new System.Collections.Generic.List<NPlayer>(); 

        private NChannel()
        {
            ChannelCount++;
            ID = ChannelCount;
            lock (Channels)
            {
                Channels.Add(this);
            }
            NCoreEvents.playerDisconnected += RemovePlayer;
        }

        ~NChannel()
        {
            ChannelCount--;
        }

        public bool AddPlayer(ref NPlayer player)
        {
            lock (Players)
            {
                if (!Players.Contains(player))
                {
                    Players.Add(player);
                    return true;
                }
                return false;
            }
        }

        public void RemovePlayer(NPlayer player)
        {
            Tools.Print($"Player {player.ClientId} has been removed from channel {ID}.");
        }

        public static int CreateChannel()
        {
            return new NChannel().ID;
        }

        public static bool CloseChannel(int channelId)
        {
            lock (Channels)
            {
                foreach (var channel in Channels)
                {
                    if (channel.ID == channelId)
                    {
                        Channels.Remove(channel);
                        return true;
                    }
                }
            }
            return false;
        }

        
    }
}
