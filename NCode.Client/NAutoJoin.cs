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
            NetworkManager.Connect(ServerIPAddress, ServerPort);
            JoinAndLoad();
        }

        void JoinAndLoad()
        {
            SceneManager.LoadSceneAsync(SceneIndex);
        }
    }
}
