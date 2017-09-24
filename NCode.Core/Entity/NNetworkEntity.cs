using NCode.Core.TypeLibrary;
using System;

namespace NCode.Core.Entity
{
    [Serializable]
    public class NNetworkEntity
    {
        /// <summary>
        /// This entity's guid.
        /// </summary>
        public readonly Guid Guid;

        public int Owner;

        public string PathToPrefab;

        public NVector3 position;

        public NVector4 rotation;

        public NNetworkEntity()
        {
            Guid = Guid.NewGuid();
        }
    }
}
