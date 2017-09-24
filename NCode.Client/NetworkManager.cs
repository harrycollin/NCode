using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NCode.Core;
using UnityEngine;

using NCode.Core.Entity;
using NCode.Core.Utilities;
using UnityEditor;

namespace NCode.Client
{
    /// <summary>
    /// Acts as a more friendly way to carry out network functions. Derives from monobehaviour so needs to have an instance of itself in-game. 
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {

        #region Privates

        /// <summary>
        /// The instance of this class. 
        /// </summary>
        private static NetworkManager _instance;

        /// <summary>
        /// The staticInstance of the MainClient. This is the core of the networking. 
        /// </summary>
        private readonly NMainClient _mainClient = new NMainClient();

        private static readonly Dictionary<Guid, NNetworkEntity> _networkEntityDictionary = new Dictionary<Guid, NNetworkEntity>();
        #endregion

        #region Publics

        /// <summary>
        /// Whether the player is connected and verified with the server.
        /// </summary>
        public static bool IsConnected { get { return Instance != null && Instance._mainClient.IsConnected; } }

        /// <summary>
        /// Whether a connect operation is in progress.
        /// </summary>
        public static bool IsTryingToConnect { get { return Instance != null && Instance._mainClient.IsTryingToConnect; } }

        /// <summary>
        /// Whether the client has setup a Udp connection with the remote server.
        /// </summary>
        public static bool IsUdpSetup { get { return Instance != null && Instance._mainClient.IsUdpSetup; } }

        /// <summary>
        /// Returns the client's Guid. 
        /// </summary>
        public static int ClientID { get { return Instance._mainClient.ClientID; } }

        #endregion

        #region Delegates

        /// <summary>
        /// Event triggered when the client setup is complete.
        /// </summary>
        public static ClientEvents.OnConnect OnConnect { get { return Instance != null ? Instance._mainClient.onConnect : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onConnect = value; } }

        /// <summary>
        /// Event triggered when the client has disconnected from the server. 
        /// </summary>
        public static ClientEvents.OnDisconnect OnDisconnect { get { return Instance != null ? Instance._mainClient.onDisconnect : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onDisconnect = value; } }

        /// <summary>
        /// Event triggered upon receiving an Network Object update.
        /// </summary>
        public static ClientEvents.OnCreateEntity OnCreateEntity { get { return Instance != null ? Instance._mainClient.onCreateEntity : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onCreateEntity = value; } }

        /// <summary>
        /// Event triggered upon receiving an Network Object update.
        /// </summary>
        public static ClientEvents.OnEntityUpdate OnEntityUpdate { get { return Instance != null ? Instance._mainClient.onEntityUpdate : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onEntityUpdate = value; } }

        /// <summary>
        /// Event triggered upon receiving an destroy command from the server.
        /// </summary>
        public static ClientEvents.OnDestroyEntity OnDestroyEntity { get { return Instance != null ? Instance._mainClient.onDestroyEntity : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onDestroyEntity = value; } }

        /// <summary>
        /// Event triggered upon receiving an Remote Function call.
        /// </summary>
        public static ClientEvents.OnRFC onRFC { get { return Instance != null ? Instance._mainClient.onRFC : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onRFC = value; } }

        #endregion

        private static NetworkManager Instance
        {
            get
            {
                if (_instance != null) return _instance;
                _instance = CreateInstance();
                return _instance;
            }
        }

        private static NetworkManager CreateInstance()
        {
            if (_instance != null) return null;
            GameObject newInstance = new GameObject("NetworkManager");
            DontDestroyOnLoad(newInstance);
            newInstance.AddComponent<NetworkManager>();
            return newInstance.GetComponent<NetworkManager>();
        }

        /// <summary>
        /// Start method from monobehaviour
        /// </summary>                     
        void Start()
        {
        }

        void Update()
        {
            if (Instance != null)
            {
                Instance._mainClient.ClientUpdate();
            }

        }

        /// <summary>
        /// Attempts to connect the client to the given server.
        /// </summary>
        public static bool Connect(string ip, int port)
        {
            if (Instance == null || Instance._mainClient.IsTryingToConnect || Instance._mainClient.IsConnected) return false;
            Instance._mainClient.Connect(IPAddress.Parse(ip), port);
            return true;
        }

        /// <summary>
        /// Disconnects the client from the server. 
        /// </summary>
        public static void Disconnect()
        {
            if (Instance._mainClient.IsSocketConnected)
            {
                Instance._mainClient.Disconnect();
            }
        }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>
        public static void SetPacketHandler(Packet packet, ClientEvents.OnPacket callback)
        {
            if (Instance != null)
            {
                Instance._mainClient.packetHandlers[packet] = callback;
            }
        }

        /// <summary>
        /// Begin sending a packet
        /// </summary>
        public static BinaryWriter BeginSend(Packet packet)
        {
            if (Instance != null && Instance._mainClient.IsSocketConnected)
            {
                return Instance._mainClient.BeginSend(packet);
            }
            return null;
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public static void EndSend(bool reliable)
        {
            if (Instance != null && Instance._mainClient.IsSocketConnected)
            {
                Instance._mainClient.EndSend(reliable);
            }
        }

        private void CreateEntity(NNetworkEntity entity)
        {
            if (!_networkEntityDictionary.ContainsKey(entity.Guid))
            {
                _networkEntityDictionary.Add(entity.Guid, entity);
                Instantiate((GameObject)AssetDatabase.LoadAssetAtPath(entity.PathToPrefab, typeof(GameObject)), NUnityTools.V3ToVector3(entity.position), NUnityTools.V4ToQuaternion(entity.rotation));
            }
        }

        private void UpdateEntity(NNetworkEntity entity)
        {
            if (_networkEntityDictionary.ContainsKey(entity.Guid))
            {
                _networkEntityDictionary[entity.Guid] = entity; 
            }
        }

        private void DestroyEntity(Guid entity)
        {
            if (_networkEntityDictionary.ContainsKey(entity))
            {
                _networkEntityDictionary.Remove(entity);
                Destroy(NEntityLink.Find(entity).gameObject);
            }
        }


        public static NNetworkEntity GetEntity(Guid guid)
        {
            if (_networkEntityDictionary.ContainsKey(guid))
            {
                return _networkEntityDictionary[guid];
            }
            return null;
        }

        /// <summary>
        /// The static method to find a execute an RFC on any NetworkBehaviour
        /// </summary>
        public static void FindAndExecute(Guid guid, int RFCID, params object[] parameters)
        {
            var obj = NEntityLink.Find(guid);
            if (obj != null)
                obj.ExecuteRfc(RFCID, parameters);
        }
  
        public static void Instantiate(GameObject gameObject, Vector3 Position, Quaternion Rotation)
        {
            if(AssetDatabase.GetAssetPath(gameObject) == null)
            {
                Core.Utilities.Tools.Print("Specified GameObject isn't a prefab.",null, MessageType.Error);
                return;
            }

            NNetworkEntity entity = new NNetworkEntity()
            {
                position = NUnityTools.Vector3ToV3(Position),
                rotation = NUnityTools.QuaternionToV4(Rotation),
                Owner = ClientID,
                PathToPrefab = AssetDatabase.GetAssetPath(gameObject)
            };

            GameObject obj = UnityEngine.Object.Instantiate(gameObject, Position, Rotation);
            obj.GetComponent<NEntityLink>().Guid = entity.Guid;

            BinaryWriter writer = BeginSend(Packet.CreateEntity);
            writer.WriteObject(entity);
            EndSend(true);

        }
    }
}