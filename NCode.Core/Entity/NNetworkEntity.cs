using System;

namespace NCode.Core.Entity
{
    [Serializable]
    public class NNetworkEntity
    {
        #region Private vars

        private SpawnPriority _spawnPriority;
        private DespawnPriority _despawnPriority;

        #endregion

        #region Public properties

        /// <summary>
        /// This entity's guid.
        /// </summary>
        public readonly Guid Guid;

        public int Owner;

        /// <summary>
        /// Changes the spawn priority for this entity.
        /// </summary>
        public SpawnPriority SpawnPriority
        {
            get { return _spawnPriority; }
            set { _spawnPriority = value; }
        }

        /// <summary>
        /// Changes the despawn priority for this entity.
        /// </summary>
        public DespawnPriority DespawnPriority
        {
            get { return _despawnPriority; }
            set { _despawnPriority = value; }
        }

        

        #endregion

        public NNetworkEntity()
        {
            Guid = Guid.NewGuid();
        }

    }

    public enum SpawnPriority
    {
        Low,
        Normal,
        High,
        Instant,
    }

    public enum DespawnPriority
    {
        Low,
        Normal,
        High,
        Instant,
    }
}
