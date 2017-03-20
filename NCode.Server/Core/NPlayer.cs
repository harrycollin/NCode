using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NCode.Core;
using NCode.Core.Protocols;
using NCode.Core.TypeLibrary;
using NCode.Core.Utilities;

namespace NCode.Server.Core
{
    /// <summary>
    /// Standard player class for any players on the server.
    /// </summary>
    public class NPlayer
    {
        public NPlayer(Socket tcpSocket)
        {
            _idIncrementor++;
            ClientId = _idIncrementor;
            ClientGuid = new Guid();
            _tcpProtocol.StartReceiving(tcpSocket);
        }

        ~NPlayer()
        {
            _idIncrementor--;
        }

        #region public

        public IPEndPoint UdpEndpoint;


        public BinaryWriter BeginSend(Packet type)
        {
            lock (_lock)
            {
                return _tcpProtocol.BeginSend(type);
            }
        }

        public void EndSend()
        {
            lock (_lock)
            {
                _tcpProtocol.EndSend();
            }
        }

        public bool IsPlayerTcpConnected => _tcpProtocol.isConnected;

        public bool IsPlayerUdpConnected { get; set; } = false; //Find a better way of confirming this other than manually marking it.

        public bool IsPlayerSetupComplete => IsPlayerTcpConnected && IsPlayerUdpConnected;

        public AuthorizationLevel AuthorizationLevel { get; set; } = Core.AuthorizationLevel.None;

        /// <summary>
        /// This player's id. Generated in constructor.
        /// </summary>
        public readonly int ClientId;

        /// <summary>
        /// This player's guid.
        /// </summary>
        public readonly Guid ClientGuid;

        /// <summary>
        /// The remote tcp IpEndPoint for this player.
        /// </summary>
        public IPEndPoint RemoteTcpEndPoint => _tcpProtocol.tcpEndPoint;

        #endregion

        #region private     

        private object _lock = new object();

        /// <summary>
        /// Incremented each time another player joins. This will 
        /// </summary>
        private static int _idIncrementor = 0;

        /// <summary>
        /// The TCP protocol used in this version.
        /// </summary>
        private readonly TNTcpProtocol _tcpProtocol = new TNTcpProtocol();

        #endregion

        #region public static

        public static void AddPlayer(Socket socket)
        {
            NPlayer newPlayer = new NPlayer(socket);
            PlayerIdDictionary.AddOrUpdate(newPlayer.ClientId, newPlayer);
            Console.WriteLine(newPlayer.RemoteTcpEndPoint + " connecting...");
        }

        public static bool RemovePlayer(int playerId)
        {
            if (!PlayerIdDictionary.Exists(playerId)) return false;
            PlayerIdDictionary.Remove(playerId);
            return true;
        }


        public static readonly SynchronizedCache<int, NPlayer> PlayerIdDictionary = new SynchronizedCache<int, NPlayer>();

        #endregion

        #region private static


        #endregion
    }

    public enum AuthorizationLevel
    {
        None,
        Basic,
        Moderate,
        Full,
        Custom
    }

}

