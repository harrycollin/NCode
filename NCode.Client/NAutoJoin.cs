using UnityEngine;
using UnityEngine.SceneManagement;

namespace NCode.Client
{
    public class NAutoJoin : MonoBehaviour{

        public int SceneIndex;
        public string ServerIPAddress;
        public int ServerPort;
        public int ChannelID;


        // Use this for initialization
        void Start ()
        {
            NetworkManager.OnConnect += JoinAndLoad;
            NetworkManager.Connect(ServerIPAddress, ServerPort);
        }

        void JoinAndLoad()
        {
            NetworkManager.JoinChannel(ChannelID);
            SceneManager.LoadSceneAsync(SceneIndex);
        }
    }
}
