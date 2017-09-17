using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NCode.Core;
using NCode.Core.Entity;
using UnityEngine;

namespace NCode.Client
{
    /// <summary>
    /// Inherited by all networked objects (physical).
    /// </summary>
    public sealed class NetworkBehaviour : MonoBehaviour
    {
        //Global list of all NetworkObjects
        public static Dictionary<Guid, NetworkBehaviour> NetworkObjects = new Dictionary<Guid, NetworkBehaviour>();

        //Local variables
        private bool rebuildMethodList = true;

        public Guid guid => Entity.Guid;

        public int Owner
        {
            get { return Entity.Owner; }
            set { Entity.Owner = value; } 
        }
       
        public NNetworkEntity Entity;

        public string ObjectGuid;

        /// <summary>
        /// Checks if this NetworkBehaviour is owned by this client.
        /// </summary>
        /// <returns></returns>
        public bool IsMine => Owner == NetworkManager.ClientID;

        void Start()
        {
            ObjectGuid = guid.ToString();
        }

        [System.NonSerialized]
        public List<int> ConnectedChannels = new List<int>();
        
        //A local dictionary of cached RFCs
        private Dictionary<int, CachedFunc> RFCList = new Dictionary<int, CachedFunc>();


        /// <summary>
        /// The static method to find a execute an RFC on any NetworkBehaviour
        /// </summary>
        public static void FindAndExecute(Guid guid, int RFCID, params object[] parameters)
        {
            NetworkBehaviour obj = NetworkBehaviour.Find(guid);
            if(obj != null)
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
            if (NetworkManager.IsConnected)
            {
                BinaryWriter writer = NetworkManager.BeginSend(target);
                writer.WriteObject(guid); //Network Behaviour Guid
                writer.Write(rfcID); //RFC id
                writer.WriteObjectArrayEx(objs); //Parameters
                NetworkManager.EndSend(reliable);
            }
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
