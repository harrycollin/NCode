using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using NCode.Core;
using UnityEngine;
using NCode.Core.Entity;
using NCode.Core.Utilities;
using static NCode.Client.NUnityTools;
using static NCode.Core.Utilities.Tools;


namespace NCode.Client
{
    /// <summary>
    /// Acts as a more friendly way to carry out network functions. Derives from monobehaviour so needs to have an instance of itself in-game. 
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {

        #region Privates

        /// <summary>
        /// The instance of this class. This allows us to use the built in Start, Awake and Update methods in Monobehaviour.
        /// </summary>
        private static NetworkManager _instance;

        /// <summary>
        /// The staticInstance of the MainClient. This is the core of the networking. 
        /// </summary>
        private readonly NMainClient _client = new NMainClient();


        private static readonly Dictionary<Guid, NNetworkEntity> NetworkEntityDictionary = new Dictionary<Guid, NNetworkEntity>();
       
        #endregion

        #region Publics

        /// <summary>
        /// The array of prefabs that can be instantiated by 
        /// </summary>
        public GameObject[] NetworkPrefabs;

        /// <summary>
        /// Whether the player is connected and verified with the server.
        /// </summary>
        public static bool IsConnected => Instance != null && Instance._client.IsConnected;

        /// <summary>
        /// Whether a connect operation is in progress.
        /// </summary>
        public static bool IsTryingToConnect => Instance != null && Instance._client.IsTryingToConnect;

        /// <summary>
        /// Whether the client has setup a Udp connection with the remote server.
        /// </summary>
        public static bool IsUdpSetup => Instance != null && Instance._client.IsUdpSetup;

        /// <summary>
        /// Returns the client's Guid. 
        /// </summary>
        public static int ClientId => Instance._client.ClientId;

        #endregion

        #region Delegates

        /// <summary>
        /// Event triggered when the client setup is complete.
        /// </summary>
        public static NClientEvents.OnConnect OnConnect { get => Instance?._client.onConnect; set { if (Instance != null && Application.isPlaying) Instance._client.onConnect = value; } }

        /// <summary>
        /// Event triggered when the client has disconnected from the server. 
        /// </summary>
        public static NClientEvents.OnDisconnect OnDisconnect { get => Instance?._client.onDisconnect; set { if (Instance != null && Application.isPlaying) Instance._client.onDisconnect = value; } }

        /// <summary>
        /// Event triggered when a player joins the server
        /// </summary>
        public static NClientEvents.OnPlayerConnect OnPlayerConnect { get => Instance?._client.onPlayerConnect; set { if (Instance != null && Application.isPlaying) Instance._client.onPlayerConnect = value; } }

        /// <summary>
        /// Event triggered when a player's information get's updated
        /// </summary>
        public static NClientEvents.OnUpdatePlayerInfo OnUpdatePlayerInfo { get => Instance?._client.onUpdatePlayerInfo; set { if (Instance != null && Application.isPlaying) Instance._client.onUpdatePlayerInfo = value; } }

        /// <summary>
        /// Event triggered when a player disconnects from the server
        /// </summary>
        public static NClientEvents.OnPlayerDisconnect OnPlayerDisconnect { get => Instance?._client.onPlayerDisconnect; set { if (Instance != null && Application.isPlaying) Instance._client.onPlayerDisconnect = value; } }

        /// <summary>
        /// Event triggered upon receiving an Network Object update.
        /// </summary>
        public static NClientEvents.OnCreateEntity OnCreateEntity { get => Instance?._client.onCreateEntity; set { if (Instance != null && Application.isPlaying) Instance._client.onCreateEntity = value; } }

        /// <summary>
        /// Event triggered upon receiving an Network Object update.
        /// </summary>
        public static NClientEvents.OnEntityUpdate OnEntityUpdate { get => Instance?._client.onEntityUpdate; set { if (Instance != null && Application.isPlaying) Instance._client.onEntityUpdate = value; } }

        /// <summary>
        /// Event triggered upon receiving an destroy command from the server.
        /// </summary>
        public static NClientEvents.OnDestroyEntity OnDestroyEntity { get => Instance?._client.onDestroyEntity; set { if (Instance != null && Application.isPlaying) { Instance._client.onDestroyEntity = value; } } }

        /// <summary>
        /// Event triggered upon receiving an Remote Function call.
        /// </summary>
        public static NClientEvents.OnRemoteFunctionCall OnRemoteFunctionCall { get => Instance?._client.onRemoteFunctionCall; set { if (Instance != null && Application.isPlaying) Instance._client.onRemoteFunctionCall = value; } }

        #endregion

        private static NetworkManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = CreateInstance();
                return null;
            }
            set { _instance = value; }
        }

        private static NetworkManager CreateInstance()
        {
            var newInstance = new GameObject("Network Manager");
            DontDestroyOnLoad(newInstance);
            return newInstance.AddComponent<NetworkManager>();
        }


        void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }

        }

        /// <summary>
        /// Start method from monobehaviour
        /// </summary>                     
        void Start()
        {
            OnRemoteFunctionCall += FindAndExecute;
            OnCreateEntity += EventCreateEntity;
            OnEntityUpdate += EventUpdateEntity;
            OnDestroyEntity += EventDestroyEntity;
        }

        void Update()
        {

            if (!Application.isPlaying)
            {
                Disconnect();
            }

            if (Instance != null)
            {
                Instance._client.ClientUpdate();
            }
        }

        /// <summary>
        /// Attempts to connect the client to the given server.
        /// </summary>
        public static bool Connect()
        {
            if (Instance == null || Instance._client.IsTryingToConnect || Instance._client.IsConnected) return false;
            Instance._client.Connect(IPAddress.Parse("127.0.0.1"), 5127);
            return true;
        }

        /// <summary>
        /// Attempts to connect the client to the given server.
        /// </summary>
        public static bool Connect(string ip, int port)
        {
            if (Instance == null || Instance._client.IsTryingToConnect || Instance._client.IsConnected) return false;
            Instance._client.Connect(IPAddress.Parse(ip), port);
            return true;
        }

        /// <summary>
        /// Disconnects the client from the server. 
        /// </summary>
        public static void Disconnect()
        {
            if (Instance._client.IsSocketConnected)
            {
                Instance._client.Disconnect();
            }
        }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>
        public static void SetPacketHandler(Packet packet, NClientEvents.OnPacket callback)
        {
            if (Instance != null)
            {
                Instance._client.packetHandlers[packet] = callback;
            }
        }

        /// <summary>
        /// Begin sending a packet
        /// </summary>
        public static BinaryWriter BeginSend(Packet packet)
        {
            if (Instance != null && Instance._client.IsSocketConnected)
            {
                return Instance._client.BeginSend(packet);
            }
            return null;
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public static void EndSend(bool reliable)
        {
            if (Instance != null && Instance._client.IsSocketConnected)
            {
                Instance._client.EndSend(reliable);
            }
        }

        private void EventCreateEntity(NNetworkEntity entity)
        {
            if (NetworkEntityDictionary.ContainsKey(entity.Guid)) return;
            NetworkEntityDictionary.Add(entity.Guid, entity);
            var go = Instantiate(GetPrefab(entity.PrefabIndex), V3ToVector3(entity.Position), V4ToQuaternion(entity.Rotation));
            go.GetComponent<NEntityLink>().Initialize(entity.Guid);
        }

        private void EventUpdateEntity(NNetworkEntity entity)
        {
            if (NetworkEntityDictionary.ContainsKey(entity.Guid))
            {
                NetworkEntityDictionary[entity.Guid] = entity; 
            }
        }

        private void EventDestroyEntity(Guid entity)
        {
            Tools.Print(entity.ToString());

            if (NetworkEntityDictionary.ContainsKey(entity))
            {
                NetworkEntityDictionary.Remove(entity);
                NEntityLink.Destroy(entity);
            }
        }


        public static NNetworkEntity GetEntity(Guid guid)
        {
            return NetworkEntityDictionary.ContainsKey(guid) ? NetworkEntityDictionary[guid] : null;
        }

        public static void JoinChannel(int id)
        {
            var writer = BeginSend(Packet.JoinChannel);
            writer.Write(id);
            EndSend(true);
        }


        public static void LeaveChannel(int id)
        {
            var writer = BeginSend(Packet.LeaveChannel);
            writer.Write(id);
            EndSend(true);
        }

        /// <summary>
        /// The static method to find a execute an RFC on any EntityLink
        /// </summary>
        public static void FindAndExecute(Guid guid, int rfcid, params object[] parameters)
        {
            var obj = NEntityLink.Find(guid);
            if (obj != null)
                obj.ExecuteRfc(rfcid, parameters);
        }     

        public static void CreateEntity(int channelId, GameObject gameObject, Vector3 position, Quaternion rotation)
        {        
            if(Instance == null) return;
            
            var entity = new NNetworkEntity
            {
                Position = Vector3ToV3(position),
                Rotation = QuaternionToV4(rotation),
                Owner = ClientId,
                PrefabIndex = GetIndexOf(gameObject)
            };

            var writer = BeginSend(Packet.CreateEntity);
            writer.Write(channelId);
            writer.WriteObject(entity);
            EndSend(true);
        }

        public static void UpdateEntity(NNetworkEntity updatedEntity)
        {
            if (Instance == null) return;

            if (updatedEntity == null)
            {
                PrintError("Can't update Entity! Provided Entity is null.");
                return;
            }

            if (!NetworkEntityDictionary.ContainsKey(updatedEntity.Guid))
            {
                PrintError("Can't update Entity! Provided Entity doesn't exist in Entity dictionary.");
                return;
            }

            NetworkEntityDictionary[updatedEntity.Guid] = updatedEntity;

            var writer = BeginSend(Packet.UpdateEntity);
            writer.WriteObject(updatedEntity);
            EndSend(true);
        }

        public static void DestroyEntity(Guid entityGuid)
        {
            if (Instance == null) return;

            if (NetworkEntityDictionary[entityGuid] != null)
            {
                var writer = BeginSend(Packet.DestroyEntity);
                writer.WriteObject(entityGuid);
                EndSend(true);
            }

        }

        public static void TransferEntity(Guid entity, int ChannelA, int ChannelB)
        {
            if (Instance == null) return;

            if (NetworkEntityDictionary.ContainsKey(entity))
            {
                var writer = BeginSend(Packet.TransferEntity);
                writer.WriteObject(entity);
                writer.Write(ChannelA);
                writer.Write(ChannelB);
                EndSend(true);
            }

        }

        public static System.Collections.Generic.List<int> GetConnectedChannels()
        {
            return Instance._client.ConnectedChannels;
        }


        public static GameObject GetPrefab(int index)
        {
            if (Instance.NetworkPrefabs[index] != null)
            {
                return Instance.NetworkPrefabs[index];
            }
            return null;
        }

        public static int GetIndexOf(GameObject networkPrefab)
        {
            if (networkPrefab != null && Instance != null && Instance.NetworkPrefabs != null)
            {
                for (int i = 0; i < Instance.NetworkPrefabs.Length; ++i)
                {
                    if (Instance.NetworkPrefabs[i] == networkPrefab) return i;
                }
            }
            return -1;
        }
    }
}