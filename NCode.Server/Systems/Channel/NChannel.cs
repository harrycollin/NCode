using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode.Server.Systems.Channel
{
    public sealed class NChannel
    {
#region STATIC 
        //Keeps track of the current number of channels. Used to assign a channelID when a new channel is created. 
        public static int ChannelCount;
#endregion

        public readonly int ID = -1;

        NChannel()
        {
            ChannelCount++;
            ID = ChannelCount;
        }

        ~NChannel()
        {
            ChannelCount--;
        }

    }
}
