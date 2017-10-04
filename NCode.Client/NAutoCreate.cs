using UnityEngine;

namespace NCode.Client
{
    public class NAutoCreate : MonoBehaviour
    {
        public int ChannelId;
        public int PrefabIndex;

        void Awake()
        {
            Instantiate();
        }

        void Instantiate()
        {
            NetworkManager.CreateEntity(ChannelId, PrefabIndex, Vector3.zero, Quaternion.identity);
        }
    }
}
