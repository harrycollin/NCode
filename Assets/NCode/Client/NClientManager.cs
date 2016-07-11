using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using NCode.BaseClasses;
using NCode.Utilities;

namespace NCode
{
    /// <summary>
    /// Acts as a more friendly way to carry out network fucntions. Derives from monobehaviour so needs to have an instance of itself in-game. 
    /// </summary>
    public sealed class NClientManager : MonoBehaviour
    {
        /// <summary>
        /// The instance of the MainClient. This is the core of the networking. 
        /// </summary>
        static NMainClient instance = new NMainClient();

        void SetDelegates()
        {
            instance.onRFC = OnRFC;

        }

        /// <summary>
        /// Attempts to connect the client to the given server.
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public static void Connect(string ip, int port)
        {
            if(instance != null && !instance.isTryingToConnect && !instance.isConnected)
            {
                instance.Start(new IPEndPoint(IPAddress.Parse(ip), port));
            }
        }

        /// <summary>
        /// Disconnects the client from the server. 
        /// </summary>
        public static void Disconnect()
        {
            if (instance.isSocketConnected)
            {
                instance.TcpClient.Disconnect();
            }
        }

        void Awake()
        {
            SetDelegates();
        }

        /// <summary>
        /// Updates every frame. Meaning clients will get network updates processed at the rate of their framerate. 
        /// </summary>
        void Update()
        {
            if (instance != null && instance.isSocketConnected)
            {
                instance.ClientUpdate();
                //Dequeues and spawns the network object
                if (instance.WaitingForSpawn.Count > 0)
                {
                    SpawnQueuedObject(instance.WaitingForSpawn.Dequeue());
                }
            }
        }

        void OnRFC(int channelID, Guid guid, int RFCID, params object[] parameters)
        {
            Tools.Print("OnRFC");
            NetworkBehaviour.FindAndExecute(guid, RFCID, parameters);

        }

        static void SpawnQueuedObject(NetworkObject obj)
        {
            Vector3 pos = Converters.StringToVector3(obj.position);
            Quaternion rot = Converters.StringToQuaternion(obj.rotation);
            if (obj != null)
            {
                GameObject go = (GameObject)Instantiate(ClientConfig.GetPrefabByID(obj.prefabid), pos, rot);
                Tools.Print(obj.position);
                go.transform.position = pos;
                go.transform.rotation = rot;
                if(go.GetComponent<NetworkBehaviour>() == null) { go.AddComponent<NetworkBehaviour>(); }
                go.GetComponent<NetworkBehaviour>().networkObject = obj;
                go.GetComponent<NetworkBehaviour>().SetValues();
                instance.SpawnedNetworkedObjects.Add(obj.GUID, obj);
                NetworkBehaviour.NetworkObjects.Add(obj.GUID, go.GetComponent<NetworkBehaviour>());
            }
            else { Tools.Print("@SpawnQueuedObject - Object is null", Tools.MessageType.error); }
        }

        /// <summary>
        /// Set the following function to handle this type of packets.
        /// </summary>
        public static void SetPacketHandler(Packet packet, NMainFunctionsClient.OnPacket callback)
        {
            if (instance != null)
            {
                instance.packetHandlers[packet] = callback;
            }
        }

        /// <summary>
        /// Begin sending a packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public static BinaryWriter BeginSend(Packet packet, bool reliable)
        {
            if (instance != null && instance.isSocketConnected)
            {
                return instance.TcpClient.BeginSend(packet);
            }
            else { Tools.Print("@NClientManager.BeginSend - Either the MainClient instance is null OR the instance isn't connected and verified", Tools.MessageType.warning); }
            return null;
        }

        /// <summary>
        /// End the sending process
        /// </summary>
        public static void EndSend()
        {
            instance.TcpClient.EndSend();
        }

        /// <summary>
        /// Sends a request to join the given channel. 
        /// </summary>
        /// <param name="ID"></param>
        public static void JoinChannel(int ID)
        {
            if(ID > 0)
            {
                Tools.Print(ID.ToString());
                BinaryWriter writer = BeginSend(Packet.RequestJoinChannel, true);
                writer.Write(ID);
                EndSend();
            }
        }

        /// <summary>
        /// Sends a request to leave a given channel.
        /// </summary>
        /// <param name="ID"></param>
        public static void LeaveChannel(int ID)
        {
            if (ID > 0)
            {
                BinaryWriter writer = BeginSend(Packet.RequestLeaveChannel, true);
                writer.Write(ID);
                EndSend();
            }
        }

        
        /// <summary>
        /// Creates a new NetworkObject and sends it to the server for proccessing. 
        /// </summary>
        /// <param name="PrefabID"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public static void CreateNewObject(int channelID, int PrefabID, Vector3 position, Quaternion rotation)
        {            
            BinaryWriter writer = BeginSend(Packet.RequestCreateObject, true);

            NetworkObject TempObject = new NetworkObject();
            TempObject.LastChannelID = channelID;
            TempObject.prefabid = PrefabID;
            TempObject.GUID = Generate.GenerateGUID();
            TempObject.position = Converters.Vector3ToString(position);
            TempObject.rotation = Converters.QuaternionToString(rotation);
            TempObject.owner = instance.TcpClient.SteamID;

            writer.WriteByteArrayEx(Converters.ConvertObjectToByteArray(TempObject));
            EndSend();
            
        }

        /// <summary>
        /// Returns the local player
        /// </summary>
        /// <returns></returns>
        public static NPlayer LocalPlayer()
        {
            return instance.TcpClient;
        }

        /// <summary>
        /// Sends a destroy request to the server for the given object (by guid). 
        /// Does pre-checks to see if object is valid and exists on the client. 
        /// </summary>
        /// <param name="guid"></param>
        public static void DestroyObject(Guid guid)
        {
            if(guid != null && DoesObjectExist(guid))
            {
                BinaryWriter writer = BeginSend(Packet.RequestDestroyObject, true);
                writer.WriteByteArrayEx(Converters.ConvertObjectToByteArray(guid));
                EndSend();
            }
        }

        /// <summary>
        /// Good for checking if a given object is valid (exists on this client). 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool DoesObjectExist(Guid guid)
        {
            if (instance.NetworkedObjects.ContainsKey(guid)) { return true; }
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
