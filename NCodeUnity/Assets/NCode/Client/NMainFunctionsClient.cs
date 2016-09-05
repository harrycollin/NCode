using NCode.Core.Protocols;
using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace NCode.Core.Client
{
    /// <summary>
    /// Contains all properties and values used across the client. 
    /// </summary>
    public class NMainFunctionsClient : ClientEvents
    {
        /// <summary>
        /// The Tcp protocol for this player.
        /// </summary>
        public NTcpProtocol TcpClient = new NTcpProtocol();

        /// <summary>
        /// The Udp protocol for this player.
        /// </summary>
        public NUdpProtocol UdpClient = new NUdpProtocol();

        /// <summary>
        /// The current tick time of this client.
        /// </summary>
        public long ClientTime = 0;

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
        /// Whether the Udp Client is setup
        /// </summary>
        public bool isUdpSetup { get { return UdpClient.isActive; } }

        /// <summary>
        /// The endpoint for udp traffic on the server.
        /// </summary>
        public IPEndPoint ServerUdpEndpoint;    

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
        /// Currently connected channels
        /// </summary>
        public System.Collections.Generic.List<int> ConnectedChannels = new System.Collections.Generic.List<int>();


        /// <summary>
        /// A dictionary containing all connected players. 
        /// </summary>
        public Dictionary<Guid, NPlayer> ConnectedPlayers = new Dictionary<Guid, NPlayer>();



        /// <summary>
        /// Starts the connection process 
        /// </summary>
        public bool Connect(IPEndPoint ip)
        {
            if (TcpClient.Connect(ip))
            {
                if (UdpClient.Start(UnityEngine.Random.Range(10000, 50000)))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Disconnect()
        {
            if (TcpClient.Disconnect())
            {
                onDisconnect();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the responseJoinChannel packet.
        /// </summary>
        public void ResponseJoinChannel(BinaryReader reader)
        {
            int channelID = reader.ReadInt32();
            if (channelID != 0)
            {
                if (reader.ReadBoolean())
                {
                    if (!ConnectedChannels.Contains(channelID)) { ConnectedChannels.Add(channelID); onJoinChannel(channelID, true); }
                }
                else
                {
                    onJoinChannel(channelID, false);
                }
            }
        }

        /// <summary>
        /// Handles the ResponseLeaveChannel packet.
        /// </summary>
        public void ResponseLeaveChannel(BinaryReader reader)
        {
            int channelID = reader.ReadInt32();
            if (channelID != 0)
            {
                if (reader.ReadBoolean())
                {
                    if (ConnectedChannels.Contains(channelID)) { ConnectedChannels.Remove(channelID); onLeaveChannel(channelID, true); }
                }
                else
                {
                    onLeaveChannel(channelID, false);
                }
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
                TcpClient.ClientGUID = (Guid)reader.ReadObject();           
                IPEndPoint remoteIp = TcpClient.thisSocket.RemoteEndPoint as IPEndPoint;
                ServerUdpEndpoint = new IPEndPoint(remoteIp.Address, reader.ReadInt32());

                TcpClient.State = NTcpProtocol.ConnectionState.connected;

                BinaryWriter writer = UdpClient.BeginSend(Packet.SetupUDP);
                writer.WriteObject(TcpClient.ClientGUID);
                UdpClient.EndSend(ServerUdpEndpoint);

                onConnect();
            }
        }

        /// <summary>
        /// Handles the arrival of an NetworkObject (either a new one or just an update)
        /// </summary>
        public bool ReceiveObject(NetworkObject obj)
        {
            if (obj == null) return false;
            if (obj.GUID == Guid.Empty) return false;
            if (obj.LastChannelID == 0) return false;
            if (NetworkedObjects.ContainsKey(obj.GUID))
            {
                NetworkedObjects[obj.GUID] = obj;
            }
            else
            {
                NetworkedObjects.Add(obj.GUID, obj);
                Tools.Print(obj.GUID.ToString());
            }

            onObjectUpdate(obj);
            return true;
        }

        public bool ReceivePlayerUpdate(NPlayer player, bool Removing)
        {
            if (ConnectedPlayers.ContainsKey(player.ClientGUID))
            {
                if (Removing)
                {
                    ConnectedPlayers.Remove(player.ClientGUID);
                    Tools.Print(player.ClientGUID + " disconneced");
                    return true;
                }
                else
                {
                    ConnectedPlayers.Remove(player.ClientGUID);
                    ConnectedPlayers.Add(player.ClientGUID, player);
                    Tools.Print(player.ClientGUID + "'s information has been updated");

                    return true;
                }
            }
            else
            {
                ConnectedPlayers.Add(player.ClientGUID, player);
                Tools.Print(player.ClientGUID + " connected");

                return true;
            }
        }
    }
}
