using NCode.Core.TypeLibrary;
using System;

namespace NCode.Core.Entity
{
    [Serializable]
    public class NNetworkEntity
    {
        public NNetworkEntity()
        {
            Guid = Guid.NewGuid();
        }

        /// <summary>
        /// This entity's guid. Set only once. Can't be changed.
        /// </summary>
        public Guid Guid { get; }

        public int Owner { get; set; }

        public NVector3 Position { get; set; }

        public NVector4 Rotation { get; set; }

        public string PathToPrefab;

        public int PrefabIndex;

       
    }
}
