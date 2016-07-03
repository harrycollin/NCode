using NCode.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NCode
{
    /// <summary>
    /// Inherited by all networked objects (physical).
    /// </summary>
    public class NetworkBehaviour : MonoBehaviour
    {
        public Guid guid;
        public NetworkObject networkObject;
        public List<int> channels = new List<int>();

        public void SetValues()
        {
            if (networkObject == null) return;
            guid = networkObject.GUID;
        }
    }
}
