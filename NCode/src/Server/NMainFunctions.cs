using NCode.Utilities;
using NCode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCode.Core.Protocols;
using NCode.Core.BaseClasses;
using System.Collections.Concurrent;
using NCode.Server;
using System.Diagnostics;

namespace NCode
{
    /// <summary>
    /// A parent of the MainThread. Contains all nessecary methods and properties to prevent clutter in the MainThread.
    /// </summary>
    public class NMainFunctions
    {
        /// <summary>
        /// Main Tcp protocol for the server
        /// </summary>
        public TcpListener MainTcp;    

        /// <summary>
        /// The main Tcp port for the server. This is used when connecting
        /// </summary>
        public int TcpListenPort;

        /// <summary>
        /// Main Udp protocol for the server
        /// </summary>
        public TNUdpProtocol MainUdp;

        /// <summary>
        /// Main Udp port for the server. Used by the UdpProtocols on the clients
        /// </summary>
        public int UdpPort;

        /// <summary>
        /// Lock for GameServer threads
        /// </summary>
        public object GameServerThreadLock = 0;      

        /// <summary>
        /// The main thread for processing data.
        /// </summary>
        public Thread MainThread;

        /// <summary>
        /// The thread used to process udp packets
        /// </summary>
        public Thread UdpThread;


        public Thread PlayerSorting;

        /// <summary>
        /// Can be used to stop the processing on the game server.
        /// </summary>
        public bool RunGameServer;

        /// <summary>
        /// Time in ticks 
        /// </summary>
        public long TickTime = 0;

        /// <summary>
        /// Contains all the players currently connected to the server
        /// </summary>
        public Utilities.List<NTcpPlayer> MainPlayerList = new Utilities.List<NTcpPlayer>();

        /// <summary>
        /// A dictionary will all players in it for quick access.
        /// </summary>
        public Dictionary<Guid, NTcpPlayer> PlayerDictionary = new Dictionary<Guid, NTcpPlayer>();

        /// <summary>
        /// Dictionary containing player's Udp Endpoints
        /// </summary>
        public Dictionary<IPEndPoint, NTcpPlayer> PlayerUdpEPDictionary = new Dictionary<IPEndPoint, NTcpPlayer>();  

        /// <summary>
        /// Temp packet for processing
        /// </summary>
        public Core.Buffer packet;   

        /// <summary>
        /// A dictionary of all network objects
        /// </summary>
        public Dictionary<Guid, NetworkObject> NetworkObjects = new Dictionary<Guid, NetworkObject>();

        /// <summary>
        /// Whether the server should run in single channel mode.
        /// </summary>
        public bool SingleChannelMode = false;

        public long UdpThreadTimeInMs = 0;

        public long MainThreadTimeInMs = 0;

        public long MainThreadIterationsPerSecond = 0;

        public long UdpThreadIterationsPerSecond = 0;

        /// <summary>
        /// Adds the player to the MainPlayerList list.
        /// </summary>
        /// <returns></returns>
        public bool AddPlayer(Socket client)
        {
            if (client == null) return false;
            NTcpPlayer player = new NTcpPlayer();
            player.StartReceiving(client);
            player.stage = TNTcpProtocol.Stage.Verifying;
            MainPlayerList.Add(player);
            Console.WriteLine(client.RemoteEndPoint + " connecting...");
            return true;
        }     

        /// <summary>
        /// Removes the player from the MainPlayerList list.
        /// </summary>
        /// <returns></returns>
        public bool RemovePlayer(NTcpPlayer client, bool timeOut = false)
        {
            if (client == null) return false;
            //We must stop tracking the player before we remove them from any other list. This is to ensure we don't have someone trying to send packets to them.
            NObjectTracker.StopTrackingPlayer(client);    
            
            lock (MainPlayerList)
            {
                MainPlayerList.Remove(client);
            }
            lock (PlayerDictionary)
            {
                PlayerDictionary.Remove(client.ClientGuid);
            }

            client.Disconnect();
            if (timeOut) Tools.Print(client.tcpEndPoint + " has timed out.");
            else Tools.Print(client.tcpEndPoint + " has disconnected.");
            return true;
        }

       
        /// <summary>
        /// Handles the setup of newly connected clients who have sent a 'RequestClientSetup' packet with their version.
        /// </summary>
        public void ClientSetup(BinaryReader reader, NTcpPlayer player)
        {
            BinaryWriter writer = player.BeginSend(Packet.ResponseClientSetup);
            if(reader.ReadInt32() == player.ProtocolVersion) //Correct protocol version
            {
                player.ClientGuid = Guid.NewGuid(); //Assign the ThisGuid to the player.
                player.stage = TNTcpProtocol.Stage.Connected; //Change the player's connection status.

                lock (PlayerDictionary)
                {
                    PlayerDictionary.Add(player.ClientGuid, player); //Add the player to the player dictionary for quicker access that doesn't require iterations.
                }
                writer.Write((byte)1); //Tell the client their client version is matched.
                writer.WriteObject(player.ClientGuid); //Let the client know what their ThisGuid is. 
                writer.Write(UdpPort); //Let the client know which port to send udp packets to.              
                player.EndSend();


                Tools.Print("Sending GUID:" + player.ClientGuid.ToString());


                Tools.Print(player.socket.RemoteEndPoint.ToString() + " connected."); //Log the event. 
            }
            else //Incorrect protocol version
            {
                writer.Write((byte)0); //Tell the client that their version is a mismatch.
                Tools.Print(player.socket.RemoteEndPoint.ToString() + " attempted to connect with the wrong client version. Disconnecting and notififying."); //Log the event. 
                player.EndSend();
                RemovePlayer(player);
            }

        }

        /// <summary>
        /// Sends the specified client an update containing all players. 
        /// </summary>
        public void SendClientPlayerUpdate(NTcpPlayer player)
        {
            if (player == null || !player.isConnected) return;
            for(int i = 0; i < MainPlayerList.Count; i++)
            {
                if (MainPlayerList[i] != player)
                {
                    BinaryWriter writer = player.BeginSend(Packet.PlayerUpdate);
                    NPlayer p = MainPlayerList[i].ThisPlayer;
                    writer.WriteObject(p);
                    writer.Write(true); //The client is still connected
                    player.EndSend();
                }
            }
        }

        public void SendPlayerDisconnected(NTcpPlayer player)
        {
            NPlayer p = player.ThisPlayer;
            for (int i = 0; i < MainPlayerList.Count; i++)
            {
                if (MainPlayerList[i] == player) continue;
                BinaryWriter writer = MainPlayerList[i].BeginSend(Packet.PlayerUpdate);
                writer.WriteObject(p);
                writer.Write(false);
                MainPlayerList[i].EndSend();
            }
        }

        /// <summary>
        /// Returns a NTcpPlayer by their Udp EndPoint.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public NTcpPlayer GetPlayer(IPEndPoint endPoint)
        {
            if(endPoint != null)
            {
                NTcpPlayer player = null;
                try
                {
                    if(PlayerUdpEPDictionary.ContainsKey(endPoint))
                    player = PlayerUdpEPDictionary[endPoint];
                     return player;
                }
                catch (Exception e) { Tools.Print("Error accessing returning a player from 'GetPlayer(IPEndPoint)'.", Tools.MessageType.error, e); return null; }

            }
            else
            {
                Tools.Print("@GetPlayer(IPEndPoint) Parameter 'endPoint' is null", Tools.MessageType.error);
                return null;
            }
        }

        /// <summary>
        /// Returns a NTcpPlayer by their ThisGuid.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns></returns>
        public NTcpPlayer GetPlayer(Guid guid)
        {
            Tools.Print("Sending GUID:" + guid.ToString());

            if (guid != null)
            {
                NTcpPlayer player = null;
                try
                {
                    if (PlayerDictionary.ContainsKey(guid))
                    {
                        player = PlayerDictionary[guid];
                    }
                    else
                    {
                        Tools.Print("GetPlayer Dictionary null");
                    }
                    return player ?? null;
                }
                catch (Exception e) { Tools.Print("Error accessing returning a player from 'GetPlayer(ThisGuid)'.", Tools.MessageType.error, e); return null; }

            }
            else
            {
                Tools.Print("@GetPlayer(IPEndPoint) Parameter 'endPoint' is null", Tools.MessageType.error);
                return null;
            }
        }

        public void SendPlayerConnected(NTcpPlayer player)
        {
            NPlayer p = player.ThisPlayer;
            for (int i = 0; i < MainPlayerList.Count; i++)
            {
                if (MainPlayerList[i] != player)
                {
                    BinaryWriter writer = MainPlayerList[i].BeginSend(Packet.PlayerUpdate);
                    writer.WriteObject(p);
                    writer.Write(true);
                    MainPlayerList[i].EndSend();
                }
            }
        }

        public void DisconnectAllPlayers()
        {
            for (int i = 0; i < MainPlayerList.Count; i++)
            {
                //BinaryWriter writer = MainPlayerList[i].BeginSend(Packet.ServerShutDown);
                MainPlayerList[i].EndSend();
                MainPlayerList[i].Disconnect();
            }
        }

        /// <summary>
        /// Safely stops the game server.
        /// </summary>
        /// <returns></returns>
        public bool StopGameServer()
        {
            if(MainTcp != null)
            {
                DisconnectAllPlayers();
                MainTcp.Stop();
                RunGameServer = false;
                Tools.Print("Game Server shut down.");
                return true;
            }
            return false;
        }

        public void ProcessPlayerSpawn(NTcpPlayer player, NetworkObject playerObject)
        {
            //Channel channel = GetCurrentChannel(playerObject.position);
            if (!NetworkObjects.ContainsKey(playerObject.GUID))
            {
                lock (NetworkObjects)
                {
                    player.playerObject = playerObject;
                    //NetworkObjects.Add(playerObject.GUID, playerObject);
                    NObjectTracker.StartTrackingPlayer(player);
                    BinaryWriter writer = player.BeginSend(Packet.ResponseSpawnPlayerObject);
                    writer.Write(true);
                    writer.WriteObject(playerObject);
                    player.EndSend();
                    Tools.Print("Player sorted");

                }
            }
        }

        public void UpdateView()
        {
            Process proc = Process.GetCurrentProcess();

            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, 1);
            Console.Write("Memory Usage (MegaBytes):" + proc.PrivateMemorySize64 / 1048576);
         
            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, 3);
            Console.Write("Main Thread (Milliseconds):" + MainThreadTimeInMs);

            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, 4);
            Console.Write("UDP  Thread (Milliseconds):" + UdpThreadTimeInMs);

            Console.SetCursorPosition(Console.WindowWidth / 2 + 1, 6);
            Console.Write("Connected Players:" + MainPlayerList.Count);

            Console.SetCursorPosition(2, 2);
        }
    }
}
