using UnityEngine;

namespace NCode.Client
{
    public class NAutoCreate : MonoBehaviour
    {
        public int ChannelId;
        public GameObject NetworkPrefab;

        void Awake()
        {
            Instantiate();
        }

        void Instantiate()
        {
            NetworkManager.CreateEntity(ChannelId, NetworkPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
