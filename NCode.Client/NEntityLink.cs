using System;
using System.Collections.Generic;
using System.Reflection;
using NCode.Core;
using UnityEngine;
using NCode.Core.Utilities;

namespace NCode.Client
{
    public class NEntityLink : MonoBehaviour
    {
        #region Private

        private readonly Dictionary<int, CachedFunc> _rfcList = new Dictionary<int, CachedFunc>();

        #endregion

        #region Public

        public Guid Guid;

        [SerializeField]
        public string GuidString;

        [SerializeField]
        public int Owner
        {
            get { return NetworkManager.GetEntity(Guid).Owner; }
            set
            {
                NetworkManager.GetEntity(Guid).Owner = value;
                
            }
        }
        
        [SerializeField]
        public bool IsMine => Owner == NetworkManager.ClientID;
        public bool RebuildMethodList = true;

        //Global list of all NetworkObjects
        public static Dictionary<Guid, NEntityLink> NetworkObjects = new Dictionary<Guid, NEntityLink>();

        #endregion


        public void Initialize(Guid guid)
        {
            Guid = guid;
            GuidString = guid.ToString();
            NetworkObjects.Add(guid, this);
            Tools.Print($"{NetworkObjects.Count} is the count of NObjects");
        }

        /// Finds a NetworkBehaviour by GUID
        /// </summary>
        /// <returns></returns>
        public static NEntityLink Find(Guid guid)
        {
            if (NetworkObjects.ContainsKey(guid))
            {
                Tools.Print("Object FOund");
                return NetworkObjects[guid];
            }
            return null;
        }

        /// Finds a NetworkBehaviour by GUID
        /// </summary>
        /// <returns></returns>
        public static bool Destroy(Guid guid)
        {
            if (NetworkObjects.ContainsKey(guid))
            {
                Destroy(NetworkObjects[guid].gameObject);
                NetworkObjects.Remove(guid);
                return true;
            }
            return false;
        }

        public void SendRfc(int rfcID, Packet target, bool reliable, params object[] objs)
        {
            if (!NetworkManager.IsConnected) return;

            var writer = NetworkManager.BeginSend(target);
            writer.WriteObject(Guid); //Network Behaviour Guid
            writer.Write(rfcID); //RFC id
            writer.WriteObjectArrayEx(objs); //Parameters
            NetworkManager.EndSend(reliable);
        }

        /// <summary>
        /// Executes an RFC on this NetworkBehaviour
        /// </summary>
        /// <returns></returns>
        public bool ExecuteRfc(int ID, params object[] parameters)
        {
            if (RebuildMethodList) RebuildRfcList();
            CachedFunc fnc;
            if (_rfcList.TryGetValue(ID, out fnc))
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
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds RFCs on this object. Adds them to a local dictionary for quick access
        /// </summary>
        private void RebuildRfcList()
        {
            RebuildMethodList = false;
            _rfcList.Clear();

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
                        var ent = new CachedFunc
                        {
                            obj = monoBehaviour,
                            func = method
                        };

                        RFC tnc = (RFC)ent.func.GetCustomAttributes(typeof(RFC), true)[0];

                        if (tnc.id > 0)
                        {
                            if (tnc.id < 256) _rfcList.Add(tnc.id, ent);
                            else Debug.LogError("RFC IDs need to be between 1 and 255 (1 byte). If you need more, just don't specify an ID and use the function's name instead.");
                        }
                    }
                }
            }
        }
    }
}

