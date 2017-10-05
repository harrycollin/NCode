using UnityEngine;
using UnityEngine.SceneManagement;

namespace NCode.Client
{
    public class NAutoJoin : MonoBehaviour{

        public int SceneIndex;
        public string ServerIpAddress;
        public int ServerPort;
        public int ChannelId;


        // Use this for initialization
        void Start ()
        {
            NetworkManager.OnConnect += JoinAndLoad;
            NetworkManager.Connect(ServerIpAddress, ServerPort);
        }

        void JoinAndLoad()
        {
            SceneManager.LoadScene(SceneIndex);
            NetworkManager.JoinChannel(ChannelId);
        }
    }
}
