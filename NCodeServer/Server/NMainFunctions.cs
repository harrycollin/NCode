using NCode.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        /// Main Tcp protocol for the server
        /// </summary>
        public TcpListener RConTcp;

        /// <summary>
        /// The main Tcp port for the server. This is used when connecting
        /// </summary>
        public int TcpListenPort;

        /// <summary>
        /// Main Udp protocol for the server
        /// </summary>
        public NUdpProtocol MainUdp;

        /// <summary>
        /// Main Udp port for the server. Used by the UdpProtocols on the clients
        /// </summary>
        public int UdpPort;

        /// <summary>
        /// Lock for GameServer threads
        /// </summary>
        public object GameServerThreadLock = 0;

        /// <summary>
        /// Lock for RconServer threads
        /// </summary>
        public object RConServerThreadLock = 0;

        /// <summary>
        /// The main thread for processing data.
        /// </summary>
        public Thread MainThread;

        /// <summary>
        /// The main thread for processing data.
        /// </summary>
        public Thread RConThread;

        /// <summary>
        /// Time in ticks 
        /// </summary>
        public long TickTime = 0;

        /// <summary>
        /// Whether the server will be using RCon.
        /// </summary>
        public bool RConActive;

        /// <summary>
        /// The RCon password
        /// </summary>
        public string RConPassword;

        /// <summary>
        /// List of any RCon Clients
        /// </summary>
        public List<NRConClient> RConClients = new List<NRConClient>();

        /// <summary>
        /// Contains all the players currently connected to the server
        /// </summary>
        public List<NTcpPlayer> MainPlayers = new List<NTcpPlayer>();

        /// <summary>
        /// Temp packet for processing
        /// </summary>
        public NPacketContainer packet;

        /// <summary>
        /// Dictionary of active channels
        /// </summary>
        public Dictionary<int, NChannel> ActiveChannels = new Dictionary<int, NChannel>();

        /// <summary>
        /// A dictionary of all network objects
        /// </summary>
        public Dictionary<Guid, NetworkObject> NetworkObjects = new Dictionary<Guid, NetworkObject>();

        public OnPacket onPacket;
        public delegate void OnPacket(NTcpPlayer player, NPacketContainer packet);

        public OnPlayerConnect onPlayerConnect;
        public delegate void OnPlayerConnect(NTcpPlayer player);

        public OnPlayerDisconnect onPlayerDisconnect;
        public delegate void OnPlayerDisconnect(NTcpPlayer player);

        public OnCreateChannel onCreateChannel;
        public delegate void OnCreateChannel(NChannel channel);

        public OnCloseChannel onCloseChannel;
        public delegate void OnCloseChannel(NChannel channel);

        public OnPlayerJoinChannel onPlayerJoinChannel;
        public delegate void OnPlayerJoinChannel(NTcpPlayer player, NChannel channel, bool result);

        public OnPlayerLeaveChannel onPlayerLeaveChannel;
        public delegate void OnPlayerLeaveChannel(NTcpPlayer player, NChannel channel, bool result);

        /// <summary>
        /// Adds the player to the MainPlayers list.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool AddPlayer(Socket client)
        {
            if (client != null)
            {
                NTcpPlayer player = new NTcpPlayer();
                player.thisSocket = client;
                player.BeginListening();
                player.State = NTcpProtocol.ConnectionState.verifying;
                MainPlayers.Add(player);
                //onPlayerConnect(player);
                player = null;
                client = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds the rconclient to the RConClients list.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool AddRConClient(Socket client)
        {
            if (client != null)
            {
                NRConClient rconclient = new NRConClient();
                rconclient.thisSocket = client;
                rconclient.BeginListening();
                rconclient.State = NTcpProtocol.ConnectionState.verifying;
                RConClients.Add(rconclient);
                rconclient = null;
                client = null;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the player from the MainPlayers list.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool RemovePlayer(NTcpPlayer client)
        {
            if (client != null)
            {

                for (int i = 0; i < ActiveChannels.Count; i++)
                {
                    if (ActiveChannels.ContainsKey(i))
                     {
                        if (ActiveChannels[i].IsPlayerConnected(client))
                        {
                            ActiveChannels[i].RemovePlayer(client);
                        }
                    }
                }
                //onPlayerDisconnect(client);
                client.Disconnect();
                MainPlayers.Remove(client);
                return true;
            }
            return false;
        }
        /// <summary>
        /// Removes the player from the MainPlayers list.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool RemoveRConClient(NRConClient client)
        {
            if (client != null)
            {             
                client.Disconnect();
                RConClients.Remove(client);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles a RFC 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reader"></param>
        /// <param name="packet"></param>
        public void ReceiveRFC(NTcpPlayer player, BinaryReader reader, NPacketContainer packet)
        {
            Packet packetid = packet.packetid;
            int channel = reader.ReadInt32();          
            packet.position = 0;
            byte[] bytes = packet.packetData;
            for (int i = 0; i < ActiveChannels[channel].Players.Count; i++)
            {            
                if (ActiveChannels[channel].Players[i] != player)
                {
                    BinaryWriter writer = ActiveChannels[channel].Players[i].BeginSend(packetid);
                    writer.Write(bytes);
                    ActiveChannels[channel].Players[i].EndSend();
                }
            }
        }


        /// <summary>
        /// Handles the creation/joining of a channel. Creates a new channel if specified channel doesn't exist.s
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool JoinChannel(int ID, NTcpPlayer player)
        {               
            //Check for null before begin writing       
            if (player == null) return false;
            //Begin sending the response packet
            BinaryWriter writer = player.BeginSend(Packet.ResponseJoinChannel);
            //Writer the channel ID we're talking about
            writer.Write(ID);
            //If the channel ID is 0. Send a false response, end the send process and return false. 
            if (ID == 0)
            {
                writer.Write(false); player.EndSend(); return false;
            }          
            //Does the channel actively exist
            if (ActiveChannels.ContainsKey(ID))
            {
                //Make sure the player isn't already in this channel
                if (!ActiveChannels[ID].IsPlayerConnected(player))
                {
                    //Make sure the player limit hasn't been reached
                    if (ActiveChannels[ID].PlayerLimit > ActiveChannels[ID].Players.Count)
                    {
                        //Add player
                        ActiveChannels[ID].AddPlayer(player);
                        //Add the channel to the player's list of connected channels
                        player.ConnectedChannels.Add(ActiveChannels[ID]);   
                                        
                        writer.Write(true);
                        player.EndSend();
                        Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " has joined channel " + ID);
                        return true;
                    }
                    else //The channel is full
                    {
                        writer.Write(false);
                        player.EndSend();
                        return false;
                    }
                }
                else //The player is already connected to this channel
                {
                    writer.Write(false);
                    player.EndSend();
                    return false;
                }
            }
            //Create the channel as it's not active
            else
            {
                //Make a new channel
                NChannel channel = new NChannel();
                //Set the ID
                channel.ID = ID;
                //Add the player
                channel.AddPlayer(player);
                //Add channel to active channels
                ActiveChannels.Add(ID, channel);
                //Add the channel to the player's list of connected channels
                player.ConnectedChannels.Add(ActiveChannels[ID]);

                Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " has created channel " + ID);

                writer.Write(true);
                player.EndSend();
                return true;
            }          
        }

        /// <summary>
        /// Removes the player from the specified channel. Closes the channel if is empty.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool LeaveChannel(int ID, NTcpPlayer player)
        {
            //Check for null values
            if (ID == 0 || player == null) return false;
            //Check to see if the channel exists
            if (!ActiveChannels.ContainsKey(ID)) return false;
            //Remove the player from channel
            if (ActiveChannels[ID].IsPlayerConnected(player))
            {
                ActiveChannels[ID].RemovePlayer(player);
                //Remove from player's list of connected channels
                player.ConnectedChannels.Remove(ActiveChannels[ID]);
                Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " has left channel " + ID);
                            return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates a new channel. Used by server on startup
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool CreateChannel(int ID)
        {
            if (ActiveChannels.ContainsKey(ID)) return false;
            NChannel newChannel = new NChannel();
            newChannel.ID = ID;
            ActiveChannels.Add(ID, newChannel);
            return true;
        }

        /// <summary>
        /// Sends the specified client an update containing all players. 
        /// </summary>
        public void SendClientPlayerUpdate(NTcpPlayer player)
        {
            if(player != null && player.isConnected)
            {
                for(int i = 0; i < MainPlayers.Count; i++)
                {
                    if (MainPlayers[i] != player)
                    {
                        BinaryWriter writer = player.BeginSend(Packet.PlayerUpdate);
                        NPlayer p = MainPlayers[i].thisPlayer();
                        writer.WriteObject(p);
                        writer.Write(true); //The client is still connected
                        player.EndSend();
                    }
                }
            }
        }

        public void SendPlayerDisconnected(NTcpPlayer player)
        {
            NPlayer p = player.thisPlayer();
            for (int i = 0; i < MainPlayers.Count; i++)
            {
                if (MainPlayers[i] != player)
                {
                    BinaryWriter writer = MainPlayers[i].BeginSend(Packet.PlayerUpdate);
                    writer.WriteObject(p);
                    writer.Write(false);
                    MainPlayers[i].EndSend();
                }
            }
        }

        public void SendPlayerConnected(NTcpPlayer player)
        {
            NPlayer p = player.thisPlayer();
            for (int i = 0; i < MainPlayers.Count; i++)
            {
                if (MainPlayers[i] != player)
                {
                    BinaryWriter writer = MainPlayers[i].BeginSend(Packet.PlayerUpdate);
                    writer.WriteObject(p);
                    writer.Write(true);
                    MainPlayers[i].EndSend();
                }
            }
        }

        /// <summary>
        /// Processes the request for Network Object creation, returns with a Update to the clients if accepted
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool ClientRequestCreateObject(NetworkObject obj, NTcpPlayer player)
        {
            if (obj == null || obj.LastChannelID == 0) return false;
            if (ActiveChannels.ContainsKey(obj.LastChannelID))
            {
                if (ActiveChannels[obj.LastChannelID].IsPlayerConnected(player))
                {
                    if (ActiveChannels[obj.LastChannelID].AddObject(obj))
                    {
                        for(int i = 0; i < ActiveChannels[obj.LastChannelID].Players.size;)
                        {
                            BinaryWriter writer = ActiveChannels[obj.LastChannelID].Players[i].BeginSend(Packet.ClientObjectUpdate);
                            writer.WriteByteArray(Converters.ConvertObjectToByteArray(obj));
                            ActiveChannels[obj.LastChannelID].Players[i].EndSend();
                            i++;
                        }
                        return true;
                    }
                }
                else
                {
                    Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " tried to create an object in channel " + obj.LastChannelID + "yet isn't in the channel. Object creation failed.", Tools.MessageType.error);
                }
            }
            return false;
        }

        


    }
}
