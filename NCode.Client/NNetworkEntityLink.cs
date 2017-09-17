using System;
using System.Collections.Generic;
using System.Reflection;
using NCode.Client;
using UnityEngine;

namespace NCode.Core.Client
{
    public class NNetworkEntityLink : MonoBehaviour
    {
        #region Private

        private Dictionary<int, CachedFunc> _rfcList = new Dictionary<int, CachedFunc>();

        #endregion

        #region Public

        public readonly Guid Guid;
        public readonly string GuidString;
        public bool IsMine;
        public bool RebuildMethodList = true;

        #endregion

        public NNetworkEntityLink(Guid linkedEntityGuid)
        {
            Guid = linkedEntityGuid;
            GuidString = linkedEntityGuid.ToString();
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
                        CachedFunc ent = new CachedFunc();
                        ent.obj = monoBehaviour;
                        ent.func = method;

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

