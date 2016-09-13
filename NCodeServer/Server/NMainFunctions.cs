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
        /// Can be used to stop the processing on the game server.
        /// </summary>
        public bool RunGameServer;

        /// <summary>
        /// Can be used to stop the processing on the game server.
        /// </summary>
        public bool RunRConServer;

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
        public System.Collections.Generic.List<NRConClient> RConClients = new System.Collections.Generic.List<NRConClient>();

        /// <summary>
        /// Contains all the players currently connected to the server
        /// </summary>
        public System.Collections.Generic.List<NTcpPlayer> PlayersList = new System.Collections.Generic.List<NTcpPlayer>();

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
        public NBuffer packet;

        /// <summary>
        /// Dictionary of active channels
        /// </summary>
        public Dictionary<int, NChannel> ActiveChannels = new Dictionary<int, NChannel>();

        /// <summary>
        /// A dictionary of all network objects
        /// </summary>
        public Dictionary<Guid, NetworkObject> NetworkObjects = new Dictionary<Guid, NetworkObject>();

        /// <summary>
        /// Adds the player to the PlayersList list.
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
                PlayersList.Add(player);
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
        /// Removes the player from the PlayersList list.
        /// </summary>
        /// <returns></returns>
        public bool RemovePlayer(NTcpPlayer client)
        {
            if (client != null)
            {               
                foreach(KeyValuePair<int, NChannel> i in ActiveChannels)
                {
                    if (ActiveChannels[i.Key].IsPlayerConnected(client))
                    {
                        ActiveChannels[i.Key].RemovePlayer(client);
                        Tools.Print("Player Removed from Channel:" + i.Key);
                    }
                }
                //onPlayerDisconnect(client);
                client.Disconnect();
                PlayersList.Remove(client);
                PlayerDictionary.Remove(client.ClientGUID);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes the player from the PlayersList list.
        /// </summary>
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
        /// Handles the setup of newly connected clients who have sent a 'RequestClientSetup' packet with their version.
        /// </summary>
        public void ClientSetup(BinaryReader reader, NTcpPlayer player)
        {
            BinaryWriter writer = player.BeginSend(Packet.ResponseClientSetup);
            if(reader.ReadInt32() == player.ProtocolVersion) //Correct protocol version
            {
                player.ClientGUID = Guid.NewGuid(); //Assign the Guid to the player.
                player.State = NTcpProtocol.ConnectionState.connected; //Change the player's connection status.
                PlayerDictionary.Add(player.ClientGUID, player); //Add the player to the player dictionary for quicker access that doesn't require iterations.

                writer.Write((byte)1); //Tell the client their client version is matched.
                writer.WriteObject(player.ClientGUID); //Let the client know what their Guid is. 
                writer.Write(UdpPort); //Let the client know which port to send udp packets to.              
                player.EndSend();


                Tools.Print("Sending GUID:" + player.ClientGUID.ToString());


                Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " connected."); //Log the event. 
            }
            else //Incorrect protocol version
            {
                writer.Write((byte)0); //Tell the client that their version is a mismatch.
                Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " attempted to connect with the wrong client version. Disconnecting and notififying."); //Log the event. 
                player.EndSend();
                RemovePlayer(player);
            }

        }

        /// <summary>
        /// Moves a network object from channelA to channelB
        /// </summary>
        /// <param name="channelA"></param>
        /// <param name="channelB"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public bool RequestTransferObject(int channelA, int channelB, Guid guid)
        {
            if(guid != Guid.Empty && ActiveChannels.ContainsKey(channelA) && ActiveChannels.ContainsKey(channelB))
            {
                if (ActiveChannels[channelB].AddObject(ActiveChannels[channelA].RemoveObjectForTransfer(guid)))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles a RFC 
        /// </summary>
        public void ReceiveRFC(NTcpPlayer player, BinaryReader reader, NBuffer packet)
        {
            Packet packetid = packet.packet;
            int channel = reader.ReadInt32();          
            packet.position = 0;
            byte[] bytes = packet.PacketData;
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
            if (player == null) return false;

            BinaryWriter writer = player.BeginSend(Packet.ResponseLeaveChannel);

            if (ID == 0)
            {
                RemovePlayer(player);
                return false;
            }

            writer.Write(ID);

            //Check to see if the channel exists
            if (!ActiveChannels.ContainsKey(ID))
            {
                writer.Write(false);
                player.EndSend();
            }

            //Remove the player from channel
            if (ActiveChannels[ID].IsPlayerConnected(player))
            {
                //Remove player from channel
                ActiveChannels[ID].RemovePlayer(player);
                //Remove from player's list of connected channels
                player.ConnectedChannels.Remove(ActiveChannels[ID]);
                Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " has left channel " + ID);
                writer.Write(true);
                player.EndSend();
                return true;
            }
            else
            {
                writer.Write(false);
                player.EndSend();
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
                for(int i = 0; i < PlayersList.Count; i++)
                {
                    if (PlayersList[i] != player)
                    {
                        BinaryWriter writer = player.BeginSend(Packet.PlayerUpdate);
                        NPlayer p = PlayersList[i].thisPlayer();
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
            for (int i = 0; i < PlayersList.Count; i++)
            {
                if (PlayersList[i] != player)
                {
                    BinaryWriter writer = PlayersList[i].BeginSend(Packet.PlayerUpdate);
                    writer.WriteObject(p);
                    writer.Write(false);
                    PlayersList[i].EndSend();
                }
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
        /// Returns a NTcpPlayer by their Guid.
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
                    if (player != null) { return player; } else { return null; }
                }
                catch (Exception e) { Tools.Print("Error accessing returning a player from 'GetPlayer(Guid)'.", Tools.MessageType.error, e); return null; }

            }
            else
            {
                Tools.Print("@GetPlayer(IPEndPoint) Parameter 'endPoint' is null", Tools.MessageType.error);
                return null;
            }
        }

        public void SendPlayerConnected(NTcpPlayer player)
        {
            NPlayer p = player.thisPlayer();
            for (int i = 0; i < PlayersList.Count; i++)
            {
                if (PlayersList[i] != player)
                {
                    BinaryWriter writer = PlayersList[i].BeginSend(Packet.PlayerUpdate);
                    writer.WriteObject(p);
                    writer.Write(true);
                    PlayersList[i].EndSend();
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
            //Does the channel exist?
            if (ActiveChannels.ContainsKey(obj.LastChannelID))
            {   
                //Is the player in that channel?
                if (ActiveChannels[obj.LastChannelID].IsPlayerConnected(player))
                {
                    //Adds the object to the channel if it isn't already in it.
                    if (ActiveChannels[obj.LastChannelID].AddObject(obj))
                    {
                        //Sends the update to all clients including the sender. 
                        for (int i = 0; i < ActiveChannels[obj.LastChannelID].Players.Count; i++)
                        {
                            BinaryWriter writer = ActiveChannels[obj.LastChannelID].Players[i].BeginSend(Packet.ClientObjectUpdate);
                            writer.WriteObject(obj);
                            ActiveChannels[obj.LastChannelID].Players[i].EndSend();
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

        public void DisconnectAllPlayers()
        {
            for (int i = 0; i < PlayersList.Count; i++)
            {
                BinaryWriter writer = PlayersList[i].BeginSend(Packet.ServerShutDown);
                PlayersList[i].EndSend();
                PlayersList[i].Disconnect();
            }
        }

        public void CloseAllChannels()
        {
            for(int i = 0; i < ActiveChannels.Count; i++)
            {
                ActiveChannels.Remove(ActiveChannels[i].ID);
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
                CloseAllChannels();
                MainTcp.Stop();
                RunGameServer = false;
                Tools.Print("Game Server shut down.");
                return true;
            }
            return false;
        }       
    }
}
