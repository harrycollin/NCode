using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode.Core
{
    [Serializable]
    public class NPlayerInfo
    {
        public readonly int PlayerID;
        public readonly Guid PlayerGuid;
        public string PlayerAliase;
        public readonly List<int> ConnectedChannels = new List<int>();
        public bool SessionLeader;

        public NPlayerInfo(int playerID, Guid playerGuid)
        {
            PlayerID = playerID;
            PlayerGuid = playerGuid;
            PlayerAliase = $"Player: {PlayerID}";
        }
    }
}
