using NCode.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static NCode.Core.Utilities.Tools;

namespace NCode.Core.Entity
{
    public static class NEntityCache
    {
        private static readonly Dictionary<Guid, NNetworkEntity> Dictionary = new Dictionary<Guid, NNetworkEntity>();

        public static void AddOrUpdate(NNetworkEntity entity)
        {
            lock (Dictionary)
            {
                if (!Dictionary.ContainsKey(entity.Guid))
                {
                    Dictionary.Add(entity.Guid, entity);
                    return;
                }
                Dictionary[entity.Guid] = entity;
            }
        }

        public static void Remove(Guid nNetworkEntityGuid)
        {
            lock (Dictionary)
            {
                if (!Dictionary.ContainsKey(nNetworkEntityGuid)) return;
                Dictionary.Remove(nNetworkEntityGuid);
                Print($"Entity: {nNetworkEntityGuid} has been destroyed.");
            }
        }

        public static NNetworkEntity GetEntity(Guid nNetworkEntityGuid)
        {
            lock (Dictionary)
            {
                return Dictionary.ContainsKey(nNetworkEntityGuid) ? Dictionary[nNetworkEntityGuid] : null;
            }
        }

        public static System.Collections.Generic.List<KeyValuePair<Guid, NNetworkEntity>> GetList()
        {
            lock (Dictionary)
            {
                return Dictionary.ToList();
            }
        }
    }
}
