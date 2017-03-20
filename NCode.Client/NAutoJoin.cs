using UnityEngine;
using UnityEngine.SceneManagement;

namespace NCode.Client
{
    public class NAutoJoin : MonoBehaviour {

        public int SceneIndex;
        public string ServerIPAddress;
        public int ServerPort;

        // Use this for initialization
        void Start ()
        {       
            NetworkManager.CreateInstance();
            NetworkManager.onConnect += JoinAndLoad;
            NetworkManager.Connect(ServerIPAddress, ServerPort);
        }

        void JoinAndLoad()
        {
            SceneManager.LoadScene(SceneIndex);
        }
    }
}
