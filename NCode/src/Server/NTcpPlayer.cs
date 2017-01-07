using NCode;
using NCode.Core.BaseClasses;
using NCode.Core.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NCode
{
    public class NTcpPlayer : TNTcpProtocol 
    {
        public float Radius = 300;

        Utilities.List<Guid> ObjectsSyncingWith = new Utilities.List<Guid>();
        Utilities.List<Guid> PlayersSyncingWith = new Utilities.List<Guid>();

        Queue<Guid> ObjectsToSend = new Queue<Guid>();
        Queue<Guid> ObjectsToDestroy = new Queue<Guid>();

        public IPEndPoint UdpEndpoint;
        public NetworkObject playerObject;


        public int ObjectsToSendCount()
        {
            return ObjectsToSend.Count;
        }

        public int ObjectsToDestroyCount()
        {
            return ObjectsToDestroy.Count;
        }

        public int PlayerSyncingWithCount()
        {
            return PlayersSyncingWith.Count;
        }

        public int ObjectsSyncingWithCount()
        {
            return ObjectsSyncingWith.Count;
        }

        /// <summary>
        /// Enqueue a NetworkObject to be destroyed
        /// </summary>
        public void EnqueueToDestroy(Guid guid)
        {
            lock (ObjectsToDestroy)
            {
                ObjectsToDestroy.Enqueue(guid);
            }
        }

        /// <summary>
        /// Dequeue a NetworkObject to be destroyed
        /// </summary>
        public Guid DequeueObjectsToDestroy()
        {
            lock (ObjectsToDestroy)
            {
                return ObjectsToDestroy.Dequeue();
            }
        }

        /// <summary>
        /// Enqueues a NetworkObject to be sent
        /// </summary>
        public void EnqueueToSend(Guid guid)
        {
            lock (ObjectsToSend)
            {
                ObjectsToSend.Enqueue(guid);
            }
        }

        /// <summary>
        /// Dequeues a NetworkObject to be sent
        /// </summary>
        public Guid DequeueObjectsToSend()
        {
            lock (ObjectsToSend)
            {
                return ObjectsToSend.Dequeue();
            }
        }


        /// <summary>
        /// Adds a player to this player's sync list
        /// </summary>
        public void AddSyncPlayer(Guid guid)
        {
            lock (PlayersSyncingWith)
            {
                if (!PlayersSyncingWith.Contains(guid))
                {
                    PlayersSyncingWith.Add(guid);
                }
            }
        }

        /// <summary>
        /// Removes a player from this player's sync list
        /// </summary>
        public void RemoveSyncPlayer(Guid guid)
        {
            lock (PlayersSyncingWith)
            {
                if (PlayersSyncingWith.Contains(guid))
                {
                    PlayersSyncingWith.Remove(guid);
                }
            }
        }

        public void AddSyncingObject(Guid guid)
        {
            lock (ObjectsSyncingWith)
            {
                if (!ObjectsSyncingWith.Contains(guid))
                    ObjectsSyncingWith.Add(guid);
            }
        }

        public void RemoveSyncingObject(Guid guid)
        {
            lock (ObjectsSyncingWith)
            {
                if (ObjectsSyncingWith.Contains(guid))
                    ObjectsSyncingWith.Remove(guid);
            }
        }

        public bool isObjectInSync(Guid guid)
        {
            if (ObjectsSyncingWith.Contains(guid))
                return true;
            return false;
        }

        public bool isPlayerInSync(Guid guid)
        {
            if (PlayersSyncingWith.Contains(guid))
                return true;
            return false;
        }

        public Utilities.List<Guid> SyncingPlayers()
        {
            return PlayersSyncingWith;
        }
    }
}
