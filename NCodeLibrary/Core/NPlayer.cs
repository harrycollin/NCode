using System;

namespace NCode.Core
{
    [Serializable]
    public class NPlayer
    {
        public Guid ClientGUID;

        public string SteamID = "";

        public string Aliase { get; set; }

        public NPlayer thisPlayer()
        {
            NPlayer player = new NPlayer();
            player.SteamID = SteamID;
            player.ClientGUID = ClientGUID;
            player.Aliase = Aliase;
            return player;
        }
    }
}
