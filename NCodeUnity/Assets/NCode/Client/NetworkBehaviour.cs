
using NCode.Core;
using NCode.Core.Client;
using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace NCode
{
    /// <summary>
    /// Inherited by all networked objects (physical).
    /// </summary>
    public sealed class NetworkBehaviour : MonoBehaviour
    {
        //Global list of all NetworkObjects
        public static Dictionary<Guid, NetworkBehaviour> NetworkObjects = new Dictionary<Guid, NetworkBehaviour>();

        //Local variables
        public bool rebuildMethodList = true;

        public Guid guid
        {
            get { return networkObject.GUID; }
            set { networkObject.GUID = value; }
        }
        public string owner
        {
            get { return networkObject.owner; }
            set { networkObject.owner = value; }
        }     
        public Guid NetworkOwner
        {
            get { return networkObject.NetworkOwnerGUID; }
            set { networkObject.NetworkOwnerGUID = value; } 
        }
        public int LastChannelID
        {
            get { return networkObject.LastChannelID; }
            set { networkObject.LastChannelID = value; }
        }

        public NetworkObject networkObject;

        [System.NonSerialized]
        public System.Collections.Generic.List<int> ConnectedChannels = new System.Collections.Generic.List<int>();
        
        //A local dictionary of cached RFCs
        private Dictionary<int, CachedFunc> RFCList = new Dictionary<int, CachedFunc>();


        /// <summary>
        /// The static method to find a execute an RFC on any NetworkBehaviour
        /// </summary>
        public static void FindAndExecute(Guid guid, int RFCID, params object[] parameters)
        {
            NetworkBehaviour obj = NetworkBehaviour.Find(guid);
            obj.ExecuteRFC(RFCID, parameters);
        }

        /// <summary>
        /// Finds a NetworkBehaviour by GUID
        /// </summary>
        /// <returns></returns>
        private static NetworkBehaviour Find(Guid guid)
        {
            if (NetworkObjects.ContainsKey(guid))
            {
                return NetworkObjects[guid];
            }
            return null;
        }

        /// Send an RFC from this NetworkBehaviour
        /// </summary>
        public void SendRFC(int rfcID, Packet target, bool reliable, params object[] objs)
        {
            BinaryWriter writer = NClientManager.BeginSend(target, reliable);
            writer.Write(networkObject.LastChannelID); //Channel ID
            writer.Write(guid); //Network Behaviour Guid
            writer.Write(rfcID); //RFC id
            writer.WriteObjectArrayEx(objs); //Parameters
            NClientManager.EndSend(reliable);
        }

        /// <summary>
        /// Executes an RFC on this NetworkBehaviour
        /// </summary>
        /// <returns></returns>
        public bool ExecuteRFC(int ID, params object[] parameters)
        {
            if(rebuildMethodList) RebuildMethodList();
            CachedFunc fnc;
            if (RFCList.TryGetValue(ID, out fnc))
            {
                if (fnc.parameters == null)
                    fnc.parameters = fnc.func.GetParameters();
                try
                {
                    fnc.func.Invoke(fnc.obj, parameters);
                    return true;
                }
                catch (System.Exception ex)
                {
                    if (ex.GetType() == typeof(System.NullReferenceException)) return false;
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if this NetworkBehaviour is owned by this client.
        /// </summary>
        /// <returns></returns>
        public bool IsMine { get { return NetworkOwner == NClientManager.LocalPlayer().ClientGUID; }  }

        /// <summary>
        /// Finds RFCs on this object. Adds them to a local dictionary for quick access
        /// </summary>
        private void RebuildMethodList()
        {
            rebuildMethodList = false;
            RFCList.Clear();

            //Get all monobehaviour objects attached to this object
            MonoBehaviour[] monoBehaviours = GetComponentsInChildren<MonoBehaviour>(true);

            //Iterate the classes
            for (int i = 0, imax = monoBehaviours.Length; i < imax; ++i)
            {
                MonoBehaviour monoBehaviour = monoBehaviours[i];
                System.Type type = monoBehaviour.GetType();

                //Get all methods 
                MethodInfo[] methods = type.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

                //Iterate the methods
                for (int b = 0, bmax = methods.Length; b < bmax; ++b)
                {
                    MethodInfo method = methods[b];

     
                    if (method.IsDefined(typeof(RFC), true))
                    {
                        CachedFunc ent = new CachedFunc();
                        ent.obj = monoBehaviour;
                        ent.func = method;

                        RFC tnc = (RFC)ent.func.GetCustomAttributes(typeof(RFC), true)[0];

                        if (tnc.id > 0)
                        {
                            if (tnc.id < 256) RFCList.Add(tnc.id, ent);
                            else Debug.LogError("RFC IDs need to be between 1 and 255 (1 byte). If you need more, just don't specify an ID and use the function's name instead.");
                        }                      
                    }
                }
            }
        }
    }
}
