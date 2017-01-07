using NCode.Core.Protocols;
using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;

namespace NCode.Core.Client
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

        /// <summary>
        /// Disconnects from the remote server.
        /// </summary>
        public bool Disconnect()
        {
            if (IsSocketConnected)
            {
                TcpClient.Disconnect();
                onDisconnect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Begins the sending process
        /// </summary>
        public BinaryWriter BeginSend(Packet packet)
        {
            TempBuffer = Buffer.Create();
            return TempBuffer.BeginPacket(packet);
        }

        /// <summary>
        /// Ends the sending process
        /// </summary>
        public void EndSend(bool reliable)
        {
            TempBuffer.EndPacket();
            if (IsSocketConnected)
            {
                if (reliable || !IsUdpSetup || ServerUdpEndpoint == null || !UdpClient.isActive)
                {
                    TcpClient.SendTcpPacket(TempBuffer);
                }
                else UdpClient.Send(TempBuffer, ServerUdpEndpoint);
            }
        }

        public void ResponseClientSetup(BinaryReader reader)
        {
            if (reader.ReadByte() == 0)
            {
                TcpClient.Disconnect();
                Tools.Print("Server - Client version mismatch");
            }
            else
            {
                TcpClient.ClientGuid = (Guid)reader.ReadObject();           
                IPEndPoint remoteIp = TcpClient.socket.RemoteEndPoint as IPEndPoint;
                ServerUdpEndpoint = new IPEndPoint(remoteIp.Address, reader.ReadInt32());
                BinaryWriter writer = BeginSend(Packet.SetupUDP);
                writer.WriteObject(TcpClient.ClientGuid);
                EndSend(false);
                TcpClient.stage = TNTcpProtocol.Stage.Connected;
            }
        }

        /// <summary>
        /// Handles the arrival of an NetworkObject (either a new one or just an update)
        /// </summary>
        public bool ReceiveObject(NetworkObject obj)
        {
            if (obj == null) return false;
            if (obj.GUID == Guid.Empty) return false;
            if (NetworkedObjects.ContainsKey(obj.GUID))
            {
                NetworkedObjects[obj.GUID] = obj;
            }
            else
            {
                NetworkedObjects.Add(obj.GUID, obj);
                onObjectUpdate(obj);
                Tools.Print(obj.GUID.ToString());
            }
            
            Tools.Print(obj.GUID);
            return true;
        }
    }
}
