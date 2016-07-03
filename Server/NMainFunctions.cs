using NCode.BaseClasses;
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
        public NUdpProtocol MainUdp;

        /// <summary>
        /// Main Udp port for the server. Used by the UdpProtocols on the clients
        /// </summary>
        public int UdpPort;

        /// <summary>
        /// Lock for this threads
        /// </summary>
        public object Lock = 0;

        /// <summary>
        /// The main thread for processing data.
        /// </summary>
        public Thread MainThread;

        /// <summary>
        /// Time in ticks 
        /// </summary>
        public long TickTime = 0;

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

        public Dictionary<Guid, NetworkObject> NetworkObjects = new Dictionary<Guid, NetworkObject>();

        /// <summary>
        /// Adds the player to the MainPlayers list.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public bool AddPlayer(Socket client)
        {
            if(client != null)
            {
                NTcpPlayer player = new NTcpPlayer();            
                player.thisSocket = client;
                player.BeginListening();
                player.State = NTcpProtocol.ConnectionState.verifying;
                MainPlayers.Add(player);

                Tools.Print(client.RemoteEndPoint.ToString() + " connected.");
                player = null;
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
                client.Disconnect();                
                MainPlayers.Remove(client);
                return true;
            }
            return false;
        }

        public void RequestClientInfo(NTcpPlayer player)
        {
            BinaryWriter writer = player.BeginSend(Packet.RequestClientInfo);
            player.EndSend();
        }

        /// <summary>
        /// Handles the creation/joining of a channel. Creates a new channel if specified channel doesn't exist.s
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool JoinChannel(int ID, NTcpPlayer player)
        {
            if (player == null) return false;
            if (ID == 0) return false;
            //Does the channel actively exist
            if (ActiveChannels.ContainsKey(ID))
            {
                //Make sure the player isn't already in this channel
                if (!ActiveChannels[ID].IsPlayerConnected(player))
                {
                    //Make sure the player limit hasn't been reached
                    if(ActiveChannels[ID].PlayerLimit > ActiveChannels[ID].Players.Count)
                    {
                        //Add player
                        ActiveChannels[ID].AddPlayer(player);
                        //Add the channel to the player's list of connected channels
                        player.ConnectedChannels.Add(ActiveChannels[ID]);

                        Tools.Print(player.thisSocket.RemoteEndPoint.ToString() + " has joined channel " + ID);

                    }
                }
            }
            //Create the channel
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
            }
            return true;
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
            ActiveChannels[ID].RemovePlayer(player);
            //Remove from player's list of connected channels
            player.ConnectedChannels.Remove(ActiveChannels[ID]);
            //Close channel if it's empty
            if (ActiveChannels[ID].Players.Count == 0)
            {
                ActiveChannels.Remove(ID);
                Tools.Print("Channel " + ID + " has been removed");
            }
            return true;
        }

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
                            writer.WriteByteArrayEx(Converters.ConvertObjectToByteArray(obj));
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
