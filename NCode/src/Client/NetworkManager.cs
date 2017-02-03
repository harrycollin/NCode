using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using NCode.Utilities;
using NCode.Core;
using System.Threading;

namespace NCode.Core.Client
{
    /// <summary>
    /// Acts as a more friendly way to carry out network functions. Derives from monobehaviour so needs to have an instance of itself in-game. 
    /// </summary>
    public class NetworkManager : MonoBehaviour
    {
        /// <summary>
        /// The instance of this class. 
        /// </summary>
        public static NetworkManager instance = new NetworkManager();

        /// <summary>
        /// The staticInstance of the MainClient. This is the core of the networking. 
        /// </summary>
        [System.NonSerialized]
        NMainClient mainClient = new NMainClient();

        /// <summary>
        /// Event triggered when the client setup is complete.
        /// </summary>
        public static ClientEvents.OnConnect onConnect { get { return instance != null ? instance.mainClient.onConnect : null; } set { if (instance != null && Application.isPlaying) instance.mainClient.onConnect = value; } }

        /// <summary>
        /// Event triggered when the client has disconnected from the server. 
        /// </summary>
        public static ClientEvents.OnDisconnect onDisconnect { get { return instance != null ? instance.mainClient.onDisconnect : null; } set { if (instance != null && Application.isPlaying) instance.mainClient.onDisconnect = value; } }
       
        /// <summary>
        /// Event triggered upon receiving an Network Object update.
        /// </summary>
        public static ClientEvents.OnObjectUpdate onObjectUpdate { get { return instance != null ? instance.mainClient.onObjectUpdate : null; } set { if (instance != null && Application.isPlaying) instance.mainClient.onObjectUpdate = value; } }

        /// <summary>
        /// Event triggered upon receiving an destroy command from the server.
        /// </summary>
        public static ClientEvents.OnDestroyObject onDestroyObject { get { return instance != null ? instance.mainClient.onDestroyObject : null; } set { if (instance != null && Application.isPlaying) instance.mainClient.onDestroyObject = value; } }

        /// <summary>
        /// Event triggered upon receiving a positive response for a player spawn request.
        /// </summary>
        public static ClientEvents.OnSpawnPlayerResponse onSpawnPlayerResponse { get { return instance != null ? instance.mainClient.onSpawnPlayerResponse : null; } set { if (instance != null && Application.isPlaying) instance.mainClient.onSpawnPlayerResponse = value; } }
    
        /// <summary>
        /// Event triggered upon receiving an Remote Function call.
        /// </summary>
        public static ClientEvents.OnRFC onRFC { get { return instance != null ? instance.mainClient.onRFC : null; } set { if (instance != null && Application.isPlaying) instance.mainClient.onRFC = value; } }

        /// <summary>
        /// A static dictionary for all spawned NetworkObjects with their GameObject pair.
        /// </summary>
        static Dictionary<NetworkObject, GameObject> SpawnedNetworkObjects = new Dictionary<NetworkObject, GameObject>();

        /// <summary>
        /// Whether the player is connected and verified with the server.
        /// </summary>
        public static bool isConnected { get { return instance != null ? instance.mainClient.IsConnected : false; } }

        /// <summary>
        /// Whether a connect operation is in progress.
        /// </summary>
        public static bool isTryingToConnect { get { return instance != null ? instance.mainClient.IsTryingToConnect : false; } }

        /// <summary>
        /// Whether the client has setup a Udp connection with the remote server.
        /// </summary>
        public static bool isUdpSetup { get { return instance != null ? instance.mainClient.IsUdpSetup : false; } }
        
        /// <summary>
        /// The endpoint of the remote server if connected.
        /// </summary>
        public static EndPoint RemoteServerEndPoint { get { return instance != null && isConnected ? instance.mainClient.TcpClient.socket.RemoteEndPoint : null; } }

        /// <summary>
        /// Start method from monobehaviour
        /// </summary>                     
        public virtual void Start()
        {
            SetDelegates();          
        }
        
        /// <summary>
        /// Updates every frame. Meaning clients will get network updates processed at the rate of their framerate. 
        /// </summary>
        public virtual void Update()
        {
            if (instance != null && instance.mainClient.IsSocketConnected)
            {
                instance.mainClient.ClientUpdate();
            }
        }

        void SetDelegates()
        {
            onRFC += OnRFC;
            onDestroyObject += OnDestroyObject;
        }


        public static void CreateInstance()
        {
            if(MainThread)
        }

        /// <summary>
        /// Attempts to connect the client to the given server.
        /// </summary>
        public static bool Connect(string ip, int port)
        {
            Tools.Print("Connected");

            if (instance != null && !instance.mainClient.IsTryingToConnect && !instance.mainClient.IsConnected)
            {
                if(instance.mainClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Disconnects the client from the server. 
        /// </summary>
        public static void Disconnect()
        {
            if (instance.mainClient.IsSocketConnected)
            {
                instance.mainClient.TcpClient.Disconnect();
            }
        }

        /// <summary>
        /// Destroys the specified Object providing it exists. 
        /// </summary>
        void OnDestroyObject(Guid guid)
        {
            if (instance.mainClient.NetworkedObjects.ContainsKey(guid))
            {
                if (SpawnedNetworkObjects.ContainsKey(instance.mainClient.NetworkedObjects[guid]))           
                {
                    Destroy(SpawnedNetworkObjects[instance.mainClient.NetworkedObjects[guid]]);
                    SpawnedNetworkObjects.Remove(instance.mainClient.NetworkedObjects[guid]);
                    NetworkBehaviour.NetworkObjects.Remove(guid);
                }
                instance.mainClient.NetworkedObjects.Remove(guid);
            }
        }

        /// <summary>
        /// Handles RFCs that have just been received.
        /// </summary>
        void OnRFC(Guid guid, int RFCID, params object[] parameters)
        {
            NetworkBehaviour.FindAndExecute(guid, RFCID, parameters);
        }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>
        public static void SetPacketHandler(Packet packet, NMainFunctionsClient.OnPacket callback)
        {
            if (instance != null)
            {
                instance.mainClient.packetHandlers[packet] = callback;
            }
        }

        /// <summary>
        /// Begin sending a packet
        /// </summary>

        public static BinaryWriter BeginSend(Packet packet)
        {     
            if (instance != null && instance.mainClient.IsSocketConnected )
            {
                return instance.mainClient.BeginSend(packet);
            }     
            else
            {
                //Tools.Print("@NClientManager.BeginSend - Either the MainClient staticInstance is null OR the staticInstance isn't connected and verified", Tools.MessageType.warning);
            }
            return null;
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public static void EndSend(bool reliable)
        {
            if(instance != null && instance.mainClient.IsSocketConnected)
            {
                instance.mainClient.EndSend(reliable);
            }
        }
  
        /// <summary>
        /// Creates a new NetworkObject and sends it to the server for proccessing. 
        /// </summary>
        public static void CreateNewObject(int PrefabID, bool Persistant, Vector3 position, Quaternion rotation)
        {
            NetworkObject TempObject = new NetworkObject(PrefabID, Persistant, SpawnPriority.High);
            TempObject.position = NUnityTools.Vector3ToV3(position);
            TempObject.rotation = NUnityTools.QuaternionToV4(rotation);
            TempObject.NetworkOwnerGUID = instance.mainClient.TcpClient.ClientGuid;

            BinaryWriter writer = BeginSend(Packet.RequestCreateObject);
            writer.WriteObject(TempObject);
            EndSend(true);
        }

        /// <summary>
        /// Returns the local player
        /// </summary>
        public static NPlayer LocalPlayer()
        {
            return instance.mainClient.TcpClient;
        }

        /// <summary>
        /// Returns the Guid of the client.
        /// </summary>
        public static Guid ClientGuid()
        {
            return LocalPlayer().ClientGuid;
        }

        

        public static void SpawnPlayer(Vector3 position, Quaternion rotation)
        {
            NetworkObject playerObject = new NetworkObject(1, false, SpawnPriority.High);
            playerObject.position = NUnityTools.Vector3ToV3(position);
            playerObject.rotation = NUnityTools.QuaternionToV4(rotation);
            playerObject.NetworkOwnerGUID = instance.mainClient.TcpClient.ClientGuid;

            BinaryWriter writer = BeginSend(Packet.RequestSpawnPlayerObject);
            writer.WriteObject(playerObject);
            EndSend(true);
        }

        /// <summary>
        /// Sends a destroy request to the server for the given object (by guid). 
        /// Does pre-checks to see if object is valid and exists on the client. 
        public static void DestroyObject(Guid guid)
        {
            if(guid != Guid.Empty && DoesObjectExist(guid))
            {
                BinaryWriter writer = BeginSend(Packet.RequestDestroyObject);
                writer.WriteObject(guid);
                EndSend(true);
            }
        }

        /// <summary>
        /// Good for checking if a given object is valid (exists on this client). 
        /// </summary>
        public static bool DoesObjectExist(Guid guid)
        {
            if (instance.mainClient.NetworkedObjects.ContainsKey(guid)) { return true; }
            return false;
        }

        
       
    }
}
