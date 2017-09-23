using System;

namespace NCode.Core.Entity
{
    [Serializable]
    public class NNetworkEntity
    {
        #region Public properties

        /// <summary>
        /// This entity's guid.
        /// </summary>
        public readonly Guid Guid;

        public int Owner;

        #endregion

        public NNetworkEntity()
        {
            Guid = Guid.NewGuid();
        }
    }
}
