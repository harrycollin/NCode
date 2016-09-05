using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using NCode.Utilities;
using NCode.Core;

namespace NCode.Core.Client
{
    /// <summary>
    /// Acts as a more friendly way to carry out network functions. Derives from monobehaviour so needs to have an instance of itself in-game. 
    /// </summary>
    public sealed class NClientManager : MonoBehaviour
    {
        /// <summary>
        /// The instance of this class. 
        /// </summary>
        static NClientManager staticInstance;

        /// <summary>
        /// The staticInstance of the MainClient. This is the core of the networking. 
        /// </summary>
        [System.NonSerialized]
        NMainClient mainClient = new NMainClient();

        /// <summary>
        /// A temporary packet to write to. 
        /// </summary>
        static NBuffer tempPacket;


        public static ClientEvents.OnConnect onConnect { get { return staticInstance != null ? staticInstance.mainClient.onConnect : null; } set { if (staticInstance != null && Application.isEditor) staticInstance.mainClient.onConnect = value; } }
        
        public static ClientEvents.OnDisconnect onDisconnect { get { return staticInstance != null ? staticInstance.mainClient.onDisconnect : null; } set { if (staticInstance != null && Application.isEditor) staticInstance.mainClient.onDisconnect = value; } }

        public static ClientEvents.OnJoinChannel onJoinChannel { get { return staticInstance != null ? staticInstance.mainClient.onJoinChannel : null; } set { if (staticInstance != null && Application.isEditor) staticInstance.mainClient.onJoinChannel = value; } }

        public static ClientEvents.OnLeaveChannel onLeaveChannel { get { return staticInstance != null ? staticInstance.mainClient.onLeaveChannel : null; } set { if (staticInstance != null && Application.isEditor) staticInstance.mainClient.onLeaveChannel = value; } }


        public static ClientEvents.OnRFC onRFC { get { return staticInstance != null ? staticInstance.mainClient.onRFC : null; } set { if (staticInstance != null && Application.isEditor) staticInstance.mainClient.onRFC = value; } }

        /// <summary>
        /// Whether a connect operation is in progress.
        /// </summary>
        public static bool isTryingToConnect { get { return staticInstance != null ? staticInstance.mainClient.isTryingToConnect : false; } }
        /// <summary>
        /// Whether the player is connected and verified with the server.
        /// </summary>
        public static bool isConnected { get { return staticInstance != null ? staticInstance.mainClient.isConnected : false; } }
        /// <summary>
        /// The endpoint of the remote server if connected.
        /// </summary>
        public static EndPoint RemoteServerEndPoint { get { return staticInstance != null && isConnected ? staticInstance.mainClient.TcpClient.thisSocket.RemoteEndPoint : null; } }

        /// <summary>
        /// Start method from monobehaviour
        /// </summary>                     
        void Start()
        {
            SetDelegates();
        }
        
        /// <summary>
        /// Updates every frame. Meaning clients will get network updates processed at the rate of their framerate. 
        /// </summary>
        void Update()
        {
            if (staticInstance != null && staticInstance.mainClient.isSocketConnected)
            {
                staticInstance.mainClient.ClientUpdate();
                //Dequeues and spawns the network object
                if (staticInstance.mainClient.WaitingForSpawn.Count > 0)
                {
                    SpawnQueuedObject(staticInstance.mainClient.WaitingForSpawn.Dequeue());
                }
            }
        }

        void SetDelegates()
        {
            staticInstance.mainClient.onRFC = OnRFC;
        }


        public static void CreateInstance()
        {
            if(staticInstance == null)
            {
                GameObject newInstance = new GameObject("NetworkManager");
                DontDestroyOnLoad(newInstance);
                staticInstance = newInstance.AddComponent<NClientManager>();
                newInstance.GetComponent<NClientManager>().enabled = true;
            }
        }

        /// <summary>
        /// Attempts to connect the client to the given server.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static bool Connect(string ip, int port)
        {
            if(staticInstance != null && !staticInstance.mainClient.isTryingToConnect && !staticInstance.mainClient.isConnected)
            {
                if(staticInstance.mainClient.Connect(new IPEndPoint(IPAddress.Parse(ip), port)))
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
            if (staticInstance.mainClient.isSocketConnected)
            {
                staticInstance.mainClient.TcpClient.Disconnect();
            }
        }



        /// <summary>
        /// Handles RFCs that have just been received.
        /// </summary>
        void OnRFC(int channelID, Guid guid, int RFCID, params object[] parameters)
        {
            NetworkBehaviour.FindAndExecute(guid, RFCID, parameters);
        }

        [System.Obsolete]
        static void SpawnQueuedObject(NetworkObject obj)
        {
            Vector3 pos = NUnityTools.StringToVector3(obj.position);
            Quaternion rot = NUnityTools.StringToQuaternion(obj.rotation);
            if (obj != null)
            {
                GameObject go = (GameObject)Instantiate(ClientConfig.GetPrefabByID(obj.prefabid), pos, rot);
                Tools.Print(obj.position);
                go.transform.position = pos;
                go.transform.rotation = rot;
                if(go.GetComponent<NetworkBehaviour>() == null) { go.AddComponent<NetworkBehaviour>(); }
                go.GetComponent<NetworkBehaviour>().networkObject = obj;
                staticInstance.mainClient.SpawnedNetworkedObjects.Add(obj.GUID, obj);
                NetworkBehaviour.NetworkObjects.Add(obj.GUID, go.GetComponent<NetworkBehaviour>());
            }
            else { Tools.Print("@SpawnQueuedObject - Object is null", Tools.MessageType.error); }
        }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>
        public static void SetPacketHandler(Packet packet, NMainFunctionsClient.OnPacket callback)
        {
            if (staticInstance != null)
            {
                staticInstance.mainClient.packetHandlers[packet] = callback;
            }
        }

        /// <summary>
        /// Begin sending a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static BinaryWriter BeginSend(Packet packet, bool reliable)
        {     
            if (staticInstance != null && staticInstance.mainClient.isSocketConnected )
            {
                if (reliable)
                    return staticInstance.mainClient.TcpClient.BeginSend(packet);
                else
                    return staticInstance.mainClient.UdpClient.BeginSend(packet);
            }     
            else
            {
                Tools.Print("@NClientManager.BeginSend - Either the MainClient staticInstance is null OR the staticInstance isn't connected and verified", Tools.MessageType.warning);
            }
            return null;
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public static void EndSend(bool reliable)
        {
            if(staticInstance != null && staticInstance.mainClient.isSocketConnected)
            {
                if (reliable)
                {
                    staticInstance.mainClient.TcpClient.EndSend();
                }
                else
                {
                    staticInstance.mainClient.UdpClient.EndSend(staticInstance.mainClient.ServerUdpEndpoint);
                }
            }
        }

        /// <summary>
        /// Sends a request to join the given channel. 
        /// </summary>
        public static void JoinChannel(int ID)
        {
            if(ID > 0 && staticInstance.mainClient.isConnected)
            {
                BinaryWriter writer = BeginSend(Packet.RequestJoinChannel, true);
                writer.Write(ID);
                EndSend(true);
            }
        }

        /// <summary>
        /// Sends a request to leave a given channel.
        /// </summary>
        public static void LeaveChannel(int ID)
        {
            Tools.Print("Leave" + ID);
            if (ID > 0)
            {
                BinaryWriter writer = BeginSend(Packet.RequestLeaveChannel, true);
                writer.Write(ID);
                EndSend(true);
            }
        }

        
        /// <summary>
        /// Creates a new NetworkObject and sends it to the server for proccessing. 
        /// </summary>
        public static void CreateNewObject(int channelID, int PrefabID, bool Persistant, Vector3 position, Quaternion rotation)
        {            
            BinaryWriter writer = BeginSend(Packet.RequestCreateObject, true);
            NetworkObject TempObject = new NetworkObject();
            TempObject.LastChannelID = channelID;
            TempObject.prefabid = PrefabID;
            TempObject.Persistant = Persistant;
            TempObject.GUID = Guid.NewGuid();
            TempObject.position = NUnityTools.Vector3ToString(position);
            TempObject.rotation = NUnityTools.QuaternionToString(rotation);
            TempObject.owner = staticInstance.mainClient.TcpClient.SteamID;
            TempObject.NetworkOwnerGUID = staticInstance.mainClient.TcpClient.ClientGUID;
            writer.WriteObject(TempObject);
            EndSend(true);
        }

        /// <summary>
        /// Returns the local player
        /// </summary>
        /// <returns></returns>
        public static NPlayer LocalPlayer()
        {
            return staticInstance.mainClient.TcpClient;
        }

        /// <summary>
        /// Sends a destroy request to the server for the given object (by guid). 
        /// Does pre-checks to see if object is valid and exists on the client. 
        /// </summary>
        /// <param name="guid"></param>
        public static void DestroyObject(Guid guid)
        {
            if(guid != Guid.Empty && DoesObjectExist(guid))
            {
                BinaryWriter writer = BeginSend(Packet.RequestDestroyObject, true);
                writer.WriteObject(guid);
                EndSend(true);
            }
        }

        /// <summary>
        /// Good for checking if a given object is valid (exists on this client). 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool DoesObjectExist(Guid guid)
        {
            if (staticInstance.mainClient.NetworkedObjects.ContainsKey(guid)) { return true; }
            return false;
        }

        

        /// <summary>
        /// Config for the client. Will add functionality for custom config to be loaded. 
        /// </summary>
        public static class ClientConfig
        {
            public static GameObject GetPrefabByID(int ID)
            {
                GameObject g = new GameObject();

                switch (ID)
                {
                    case 1:
                        {
                            return (GameObject)Resources.Load("Player", typeof(GameObject)); 
                        }
                }

                return g;
            }
        }
    }
}
