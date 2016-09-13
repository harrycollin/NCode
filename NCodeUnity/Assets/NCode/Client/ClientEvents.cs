using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NCode.Core.Client
{
    public class ClientEvents
    {
        /// <summary>
        /// When the player is connected and verified (setup) with the server.
        /// </summary>
        public OnConnect onConnect;
        public delegate void OnConnect();

        /// <summary>
        /// When the player disconnects from the server.
        /// </summary>
        public OnDisconnect onDisconnect;
        public delegate void OnDisconnect();

        /// <summary>
        /// When the player has had a response from the join channel request.
        /// </summary>
        public OnJoinChannel onJoinChannel;
        public delegate void OnJoinChannel(int ID, bool Successul);

        /// <summary>
        /// When the player has had a response from the leave channel request.
        /// </summary>
        public OnLeaveChannel onLeaveChannel;
        public delegate void OnLeaveChannel(int ID, bool Successul);

        /// <summary>
        /// A dictionary of custom packet listeners. The key is the packet. 
        /// </summary>
        public Dictionary<Packet, OnPacket> packetHandlers = new Dictionary<Packet, OnPacket>();
        public delegate void OnPacket(Packet response, BinaryReader reader);


        public OnRFC onRFC;
        public delegate void OnRFC(int channelID, Guid guid, int RFCID, params object[] parameters);

        /// <summary>
        /// When the player receives an update for an network object.
        /// </summary>
        public OnObjectUpdate onObjectUpdate;
        public delegate void OnObjectUpdate(NetworkObject obj);

        public OnDestroyObject onDestroyObject;
        public delegate void OnDestroyObject(Guid guid);
    }
}
