using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using NCode.Core.BaseClasses;
using NCode.Core.Protocols;
using Buffer = NCode.Core.Buffer;

namespace NCode.Client
{
    /// <summary>
    /// Contains all properties and values used across the client. 
    /// </summary>
    public class NMainFunctionsClient : ClientEvents
    {

        public Thread MainThread;


        public Buffer TempBuffer;

        /// <summary>
        /// The Tcp protocol for this player.
        /// </summary>
        public TNTcpProtocol TcpClient = new TNTcpProtocol();

        /// <summary>
        /// The Udp protocol for this player.
        /// </summary>
        public TNUdpProtocol UdpClient = new TNUdpProtocol();

        /// <summary>
        /// The current tick time of this client.
        /// </summary>
        public long ClientTime = 0;

        /// <summary>
        /// Whether the player is trying to connect to a server
        /// </summary>
        public bool IsTryingToConnect => TcpClient.isTryingToConnect;

        /// <summary>
        /// Whether the player is connected to a server (verified aswell)
        /// </summary>
        public bool IsConnected => TcpClient.isConnected;

        /// <summary>
        /// Whether the player's socket is connected to the server (Not nessecary verifed).
        /// </summary>
        public bool IsSocketConnected => TcpClient.isSocketConnected;

        /// <summary>
        /// Whether the Udp Client is setup
        /// </summary>
        public bool IsUdpSetup => UdpClient.isActive;

        /// <summary>
        /// The endpoint for udp traffic on the server.
        /// </summary>
        public IPEndPoint ServerUdpEndpoint;    

        /// <summary>
        /// The last time this client sent a ping.
        /// </summary>
        public long LastPingTime = 0;

        /// <summary>
        /// A dictionary of known networked objects
        /// </summary>
        public Dictionary<Guid, NetworkObject> NetworkedObjects = new Dictionary<Guid, NetworkObject>();

        /// <summary>
        /// Starts the connection process 
        /// </summary>
        public bool Connect(IPEndPoint ip)
        {
            TcpClient.Connect(ip);
            UdpClient.Start(UnityEngine.Random.Range(10000, 50000));
            return true;
        }

       

        

        
    }
}
