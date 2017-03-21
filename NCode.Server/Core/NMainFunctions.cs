using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NCode.Core;
using NCode.Core.Protocols;
using NCode.Core.Utilities;
using NCode.Server.Core;
using Buffer = NCode.Core.Buffer;

namespace NCode.Server
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
        public NCode.Core.Utilities.List<NPlayer> MainPlayerList = new NCode.Core.Utilities.List<NPlayer>();

        /// <summary>
        /// A dictionary will all players in it for quick access.
        /// </summary>
        public Dictionary<Guid, Core.NPlayer> PlayerDictionary = new Dictionary<Guid, Core.NPlayer>();

        /// <summary>
        /// Dictionary containing player's Udp Endpoints
        /// </summary>
        public Dictionary<IPEndPoint, Core.NPlayer> PlayerUdpEPDictionary = new Dictionary<IPEndPoint, Core.NPlayer>();  

        /// <summary>
        /// Temp packet for processing
        /// </summary>
        public Buffer packet;   
    }
}
