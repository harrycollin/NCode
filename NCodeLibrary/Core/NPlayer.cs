using System;

namespace NCode.Core
{
    /// <summary>
    /// The lowest point of inheritance when it comes to anything player orientated. 
    /// </summary>
    [Serializable]
    public class NPlayer
    {
        public bool isPlayerTCPConnected = false;

        public bool isPlayerUDPConnected = false;

        public bool isPlayerSetupComplete = false;

        public Guid ClientGUID = Guid.Empty;

        public string SteamID = null;

        public string Aliase = null;

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
