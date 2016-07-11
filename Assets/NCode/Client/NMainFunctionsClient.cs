using NCode.BaseClasses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace NCode
{
    /// <summary>
    /// Contains all properties and values used across the client. 
    /// </summary>
    public class NMainFunctionsClient
    {
        /// <summary>
        /// The Tcp protocol for this player
        /// </summary>
        public NTcpProtocol TcpClient = new NTcpProtocol();

        /// <summary>
        /// The current tick time of this client.
        /// </summary>
        public long ClientTime = 0;

        /// <summary>
        /// A temporary packet container. Used for extracting packets
        /// </summary>
        public NPacketContainer tempPacket;

        /// <summary>
        /// Whether the player is trying to connect to a server
        /// </summary>
        public bool isTryingToConnect { get { return TcpClient.isTryingToConnect; } }

        /// <summary>
        /// Whether the player is connected to a server (verified aswell)
        /// </summary>
        public bool isConnected { get { return TcpClient.isConnected; } }

        /// <summary>
        /// Whether the player's socket is connected to the server (Not nessecary verifed).
        /// </summary>
        public bool isSocketConnected { get { return TcpClient.isSocketConnected; } }

        /// <summary>
        /// A dictionary of custom packet listeners. The key is the packet. 
        /// </summary>
        public Dictionary<Packet, OnPacket> packetHandlers = new Dictionary<Packet, OnPacket>();
        public delegate void OnPacket(Packet response, BinaryReader reader);

        public OnRFC onRFC;
        public delegate void OnRFC(int channelID, Guid guid, int RFCID, params object[] parameters);

        /// <summary>
        /// The last time this client sent a ping.
        /// </summary>
        public long LastPingTime = 0;

        /// <summary>
        /// A dictionary of existing(spawned) networked objects
        /// </summary>
        public Dictionary<Guid, NetworkObject> SpawnedNetworkedObjects = new Dictionary<Guid, NetworkObject>();

        /// <summary>
        /// A dictionary of known networked objects
        /// </summary>
        public Dictionary<Guid, NetworkObject> NetworkedObjects = new Dictionary<Guid, NetworkObject>();

        /// <summary>
        /// A queue of recieved NetworkObjects that need to be spawned. Can't be done within the NMainClient as it doens't derive from MonoBehaviour.
        /// </summary>
        public Queue<NetworkObject> WaitingForSpawn = new Queue<NetworkObject>();

        /// <summary>
        /// Dictionary of cached RFCs for quick access.
        /// </summary>
        public Dictionary<int, CachedFunc> CachedRFCs = new Dictionary<int, CachedFunc>();
    }
}
