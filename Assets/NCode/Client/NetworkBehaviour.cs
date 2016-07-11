using NCode.BaseClasses;
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
    public class NetworkBehaviour : MonoBehaviour
    {
        //Global list of all NetworkObjects
        public static Dictionary<Guid, NetworkBehaviour> NetworkObjects = new Dictionary<Guid, NetworkBehaviour>();

        [System.NonSerialized]
        Dictionary<int, CachedFunc> RFCList = new Dictionary<int, CachedFunc>();
        [System.NonSerialized]
        public Guid guid;
        [System.NonSerialized]
        public string owner;
        [System.NonSerialized]
        public NetworkObject networkObject;
        [System.NonSerialized]
        public List<int> channels = new List<int>();
        public int MainChannel = 0;

              
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
        /// <param name="guid"></param>
        /// <returns></returns>
        static NetworkBehaviour Find(Guid guid)
        {
            if (NetworkObjects.ContainsKey(guid))
            {
                return NetworkObjects[guid];
            }
            return null;
        }


        //Local methods below//

        public void SetValues()
        {
            if (networkObject == null) return;
            guid = networkObject.GUID;
            owner = networkObject.owner;
        }

        /// Send an RFC from this NetworkBehaviour
        /// </summary>
        public void SendRFC(int rfcID, Packet target, bool reliable, params object[] objs)
        {
            BinaryWriter writer = NClientManager.BeginSend(target, reliable);
            writer.Write(MainChannel); //Channel ID
            writer.Write(guid); //Network Behaviour Guid
            writer.Write(rfcID); //RFC id
            writer.WriteObjectArrayEx(objs); //Parameters
            NClientManager.EndSend();
            Tools.Print("Sending RFC");
        }

        /// <summary>
        /// Executes an RFC on this NetworkBehaviour
        /// </summary>
        /// <returns></returns>
        public bool ExecuteRFC(int ID, params object[] parameters)
        {
            RebuildMethodList();
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
        public bool IsMine { get { return  owner == NClientManager.LocalPlayer().SteamID; }  }

        /// <summary>
        /// Finds RFCs on this object. Adds them to a local dictionary for quick access
        /// </summary>
        void RebuildMethodList()
        {
            RFCList.Clear();
            MonoBehaviour[] monoBehaviours = GetComponentsInChildren<MonoBehaviour>(true);

            for (int i = 0, imax = monoBehaviours.Length; i < imax; ++i)
            {
                MonoBehaviour monoBehaviour = monoBehaviours[i];
                System.Type type = monoBehaviour.GetType();

                MethodInfo[] methods = type.GetMethods(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance);

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
                            if (tnc.id < 256) RFCList[tnc.id] = ent;
                            else Debug.LogError("RFC IDs need to be between 1 and 255 (1 byte). If you need more, just don't specify an ID and use the function's name instead.");
                        }                      
                    }
                }
            }
        }
    }
}
