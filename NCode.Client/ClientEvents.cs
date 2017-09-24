using System;
using System.Collections.Generic;
using System.IO;
using NCode.Core;
using NCode.Core.Entity;

namespace NCode.Client
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
        /// A dictionary of custom packet listeners. The key is the packet. 
        /// </summary>
        public Dictionary<Packet, OnPacket> packetHandlers = new Dictionary<Packet, OnPacket>();
        public delegate void OnPacket(Packet response, BinaryReader reader);


        public OnRFC onRFC;
        public delegate void OnRFC(Guid guid, int RFCID, params object[] parameters);

        public OnCreateEntity onCreateEntity;
        public delegate void OnCreateEntity(NNetworkEntity entity);

        /// <summary>
        /// When the player receives an update for an network object.
        /// </summary>
        public OnEntityUpdate onEntityUpdate;
        public delegate void OnEntityUpdate(NNetworkEntity obj);

        public OnDestroyEntity onDestroyEntity;
        public delegate void OnDestroyEntity(Guid guid);

    }
}
