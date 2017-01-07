using NCode.Core.BaseClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NCode
{
    /// <summary>
    /// A base class which can be inherited to provide the fundementals for saving etc. 
    /// This is not for attaching to GameObjects and is simply used as a container to save data. 
    /// These are sent to the server for saving and should be serializable. 
    /// </summary>
    [Serializable]
    public class NetworkObject
    {
        public NetworkObject(int prefabID, bool persistant, SpawnPriority priority,string Owner = null)
        {
            owner = Owner;
            prefabid = prefabID;
            Persistant = persistant;
            spawnPriority = priority;
            GUID = Guid.NewGuid();
        }
        public NetworkObject()
        {
            GUID = Guid.NewGuid();
        }

        /// <summary>
        /// The Permanent GUID of this object.
        /// </summary>
        public Guid GUID { get; }
   
        /// <summary>
        /// The current network owner's GUID.
        /// </summary>
        public Guid NetworkOwnerGUID { get; set; }

        /// <summary>
        /// Should this be de-spawned when the NetworkOwner leaves the channel.
        /// </summary>
        public bool Persistant { get; set; }
       

        /// <summary>
        /// The spawn priority of this object. This should be used appropriately.
        /// </summary>
        public SpawnPriority spawnPriority { get; set; }
        
        /// <summary>
        /// The 
        /// </summary>
        public int prefabid { get; set; }

        /// <summary>
        /// The world position of this object.
        /// </summary>
        public V3 position { get; set; }

        /// <summary>
        /// The rotation of this object.
        /// </summary>
        public V4 rotation { get; set; }

        /// <summary>
        /// This objects name. This will be readable and used in inventory labbeling.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// The owner of this object. This ises a permanent ID solution such as a SteamID64. 
        /// </summary>
        public string owner { get; set; }
    }

    public enum SpawnPriority
    {
        Low,
        Medium,
        High,
    } 
}
