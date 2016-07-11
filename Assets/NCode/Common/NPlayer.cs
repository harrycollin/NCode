
namespace NCode
{
    public class NPlayer
    {
        public string ClientID = "0";

        public string SteamID
        {
            get
            {
#if UNITY_EDITOR

                return "123456789";
#endif
                return "987654321";
            }
            set
            {
                SteamID = value;
            }
        }


        public string Aliase { get; set; }
    }
}
