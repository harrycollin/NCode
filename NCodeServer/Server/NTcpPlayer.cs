using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode
{
    public class NTcpPlayer : NTcpProtocol 
    {
        public List<NChannel> ConnectedChannels = new List<NChannel>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsInChannel(NChannel channel)
        {
            for (int i = 0; i < ConnectedChannels.Count;)
            {
                if (ConnectedChannels[i].ID == channel.ID)
                {
                    return true;
                }
                // Add up afterwards otherwise you miss the first element in the list. C# Lists start at 0.
                i++;
            }
            return false;
        }

    }
}
