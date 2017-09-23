using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using NCode.Core;
using UnityEngine;

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

        /// <summary>
        /// The entire list of all network entities
        /// </summary>
        private static System.Collections.Generic.List<NEntityLink> _networkEntityList = new System.Collections.Generic.List<NEntityLink>();

        private static Dictionary<Guid, NEntityLink> _networkEntityDictionary = new Dictionary<Guid, NEntityLink>();

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
        public static ClientEvents.OnObjectUpdate OnObjectUpdate { get { return Instance != null ? Instance._mainClient.onObjectUpdate : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onObjectUpdate = value; } }

        /// <summary>
        /// Event triggered upon receiving an destroy command from the server.
        /// </summary>
        public static ClientEvents.OnDestroyObject OnDestroyObject { get { return Instance != null ? Instance._mainClient.onDestroyObject : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onDestroyObject = value; } }

        /// <summary>
        /// Event triggered upon receiving a positive response for a player spawn request.
        /// </summary>
        public static ClientEvents.OnSpawnPlayerResponse OnSpawnPlayerResponse { get { return Instance != null ? Instance._mainClient.onSpawnPlayerResponse : null; } set { if (Instance != null && Application.isPlaying) Instance._mainClient.onSpawnPlayerResponse = value; } }

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

        /// <summary>
        /// The static method to find a execute an RFC on any NetworkBehaviour
        /// </summary>
        public static void FindAndExecute(Guid guid, int RFCID, params object[] parameters)
        {
            var obj = Find(guid);
            if (obj != null)
                obj.ExecuteRfc(RFCID, parameters);
        }

        /// <summary>
        /// Finds a NetworkBehaviour by GUID
        /// </summary>
        /// <returns></returns>
        private static NEntityLink Find(Guid guid)
        {
            if (_networkEntityDictionary.ContainsKey(guid))
            {
                return _networkEntityDictionary[guid];
            }
            return null;
        }
    }
}