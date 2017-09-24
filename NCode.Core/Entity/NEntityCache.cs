using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            }
        }

        public static NNetworkEntity GetEntity(Guid nNetworkEntityGuid)
        {
            lock (Dictionary)
            {
                return Dictionary.ContainsKey(nNetworkEntityGuid) ? Dictionary[nNetworkEntityGuid] : null;
            }
        }

        public static List<KeyValuePair<Guid, NNetworkEntity>> GetList()
        {
            lock (Dictionary)
            {
                return Dictionary.ToList();
            }
        }
    }
}
