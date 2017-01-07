using NCode.Core;
using NCode.Core.BaseClasses;
using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCode.Server
{
    public class NObjectTracker
    {
        static bool RunThreads = false;

        static Thread RadialObjectSortingThread;
        static Thread RadialPlayerSortingThread;
        static Thread ObjectSyncManagerThread;

        public static Dictionary<Guid, NetworkObject> NetworkObjects = new Dictionary<Guid, NetworkObject>();
        public static Utilities.List<NTcpPlayer> MainPlayerList = new Utilities.List<NTcpPlayer>();

        public static void StartTrackingTheads()
        {
            RadialPlayerSortingThread = new Thread(RadialPlayerSorting);
            RadialObjectSortingThread = new Thread(RadialObjectSorting);
            ObjectSyncManagerThread = new Thread(ClientUpdater);
            RadialObjectSortingThread.Start();
            RadialPlayerSortingThread.Start();
            ObjectSyncManagerThread.Start();
            RunThreads = true;
        }

        public static void StopTrackingTheads()
        {
            RunThreads = false;
        }

        public static void StartTrackingPlayer(NTcpPlayer player)
        {
            MainPlayerList.Add(player);
            if (player.playerObject != null)
                lock (NetworkObjects)
                {
                    NetworkObjects.Add(player.playerObject.GUID, player.playerObject);
                }
        }

        public static void StopTrackingPlayer(NTcpPlayer player)
        {
            if(player != null)
            {
                if (MainPlayerList.Contains(player))
                {
                    lock (MainPlayerList)
                    {
                        Utilities.List<Guid> NetworkObjectstToBeDestroyed = new Utilities.List<Guid>();
                        lock (NetworkObjects)
                        {
                            foreach (KeyValuePair<Guid, NetworkObject> i in NetworkObjects)
                            {
                                if (i.Value.NetworkOwnerGUID == player.ClientGuid && !i.Value.Persistant)
                                {
                                    NetworkObjectstToBeDestroyed.Add(i.Key);
                                }
                            }
                            foreach (Guid i in NetworkObjectstToBeDestroyed)
                            {
                                NetworkObjects.Remove(i);
                            }
                        }
                        foreach (NTcpPlayer ii in MainPlayerList)
                        {
                            //Remove the disconnected player from everyones sync list.
                            if (ii.isPlayerInSync(player.ClientGuid))
                            {
                                ii.RemoveSyncPlayer(player.ClientGuid);
                            }
                            foreach (Guid iii in NetworkObjectstToBeDestroyed)
                            {
                                if (ii.isObjectInSync(iii))
                                {
                                    ii.EnqueueToDestroy(iii);
                                }
                            }
                        }

                        MainPlayerList.Remove(player);
                    }
                }
            }
        }

        static void ClientUpdater()
        {
            while (RunThreads)
            {
                lock (MainPlayerList) {
                    for (int i = 0; i < MainPlayerList.Count; i++)
                    {
                        if (MainPlayerList[i] != null && MainPlayerList[i].IsPlayerSetupComplete)
                        {
                            while (MainPlayerList[i].ObjectsToSendCount() > 0)
                            {
                                BinaryWriter writer = MainPlayerList[i].BeginSend(Packet.ClientObjectUpdate);
                                writer.WriteObject(NetworkObjects[MainPlayerList[i].DequeueObjectsToSend()]);
                                MainPlayerList[i].EndSend();
                                Tools.Print("Sent ^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
                            }
                            while (MainPlayerList[i].ObjectsToDestroyCount() > 0)
                            {
                                BinaryWriter writer = MainPlayerList[i].BeginSend(Packet.ResponseDestroyObject);
                                writer.WriteObject(MainPlayerList[i].DequeueObjectsToDestroy());
                                MainPlayerList[i].EndSend();
                            }
                        }
                    }
                }
            }
        }

        static void RadialObjectSorting()
        {
            while (RunThreads)
            {
                Utilities.List<NTcpPlayer> playerListCopy;
                lock (MainPlayerList) playerListCopy = MainPlayerList;
                lock (playerListCopy)
                for (int i = 0; i < playerListCopy.Count; i++)
                {
                    if (playerListCopy[i] != null && playerListCopy[i].playerObject != null)
                    {
                        lock (playerListCopy[i])
                        {
                            lock (NetworkObjects)
                            {
                                foreach (KeyValuePair<Guid, NetworkObject> ii in NetworkObjects)
                                {
                                    NTcpPlayer player = MainPlayerList[i];
                                    NetworkObject obj = ii.Value;
                                    if (player.playerObject.GUID == obj.GUID) continue;

                                    float distance = CalculateDistance3D(player.playerObject.position, obj.position);

                                    if (distance < player.Radius && !playerListCopy[i].isObjectInSync(obj.GUID))
                                    {                                    
                                            MainPlayerList[i].AddSyncingObject(obj.GUID);
                                            MainPlayerList[i].EnqueueToSend(obj.GUID);                                   
                                    }
                                    else if (distance > playerListCopy[i].Radius && playerListCopy[i].isObjectInSync(obj.GUID))
                                    {                                     
                                            MainPlayerList[i].RemoveSyncingObject(obj.GUID);
                                            MainPlayerList[i].EnqueueToDestroy(obj.GUID);                                  
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static void RadialPlayerSorting()
        {
            while(RunThreads)
            {
                Utilities.List<NTcpPlayer> playerListCopy;
                lock (MainPlayerList) playerListCopy = MainPlayerList;
                lock (playerListCopy)
                {
                    for (int i = 0; i < playerListCopy.size; i++)
                    {
                        Utilities.List<Guid> PlayersToAdd = new Utilities.List<Guid>();
                        Utilities.List<Guid> PlayersToRemove = new Utilities.List<Guid>();

                        for (int ii = 0; ii < playerListCopy.size; ii++)
                        {
                            if (playerListCopy[i] == null || playerListCopy[i].playerObject == null || !playerListCopy[i].IsPlayerSetupComplete) break;
                            if (playerListCopy[ii] == null || playerListCopy[ii].playerObject == null || !playerListCopy[ii].IsPlayerSetupComplete) continue;
                            if (playerListCopy[i].ClientGuid == playerListCopy[ii].ClientGuid) continue;

                            NetworkObject player1 = playerListCopy[i].playerObject;
                            NetworkObject player2 = playerListCopy[ii].playerObject;

                            float distance = CalculateDistance3D(player1.position, player2.position);

                            if (distance < playerListCopy[i].Radius && !playerListCopy[i].isPlayerInSync(playerListCopy[ii].ClientGuid))
                            {
                                PlayersToAdd.Add(playerListCopy[ii].ClientGuid);
                            }
                            else if (distance > playerListCopy[i].Radius && playerListCopy[i].isPlayerInSync(playerListCopy[ii].ClientGuid))
                            {
                                PlayersToRemove.Add(playerListCopy[ii].ClientGuid);

                            }
                        }

                        foreach (Guid adding in PlayersToAdd)
                        {
                            MainPlayerList[i].AddSyncPlayer(adding);
                        }
                        foreach (Guid removing in PlayersToRemove)
                        {
                            MainPlayerList[i].RemoveSyncPlayer(removing);
                        }
                    }
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Returns the distance between two V3 positions. 
        /// </summary>
        private static float CalculateDistance3D(V3 pos1, V3 pos2)
        {
            return (float)Math.Sqrt((pos1.X - pos2.X) * (pos1.X - pos2.X) + (pos1.Y - pos2.Y) * (pos1.Y - pos2.Y) + (pos1.Z - pos2.Z) * (pos1.Z - pos2.Z));
        }
    }
}
