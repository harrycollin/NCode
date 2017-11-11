using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using NCode.Core;
using NCode.Core.Protocols;
using NCode.Core.Utilities;
using static NCode.Server.Core.NServerEvents;
using static NCode.Core.Utilities.Tools;
using Buffer = NCode.Core.Buffer;

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
            PlayerID = _idIncrementor;
            ClientGuid = Guid.NewGuid();
            PlayerInfo = new NPlayerInfo(PlayerID, ClientGuid);
            _tcpProtocol.StartReceiving(tcpSocket);
        }

        
        #region public

        /// <summary>
        /// This player's information.
        /// </summary>
        public NPlayerInfo PlayerInfo { get; private set; }

        /// <summary>
        /// This player's UDP IPEndPoint
        /// </summary>
        public IPEndPoint UdpEndpoint;

        /// <summary>
        /// Begin sending a packet. 
        /// </summary>
        /// <param name="type">The packet type.</param>
        /// <returns>Returns a binary writer. Write any data to be sent with this. </returns>
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

        public void SendTcpPacket(Buffer buffer)
        {
            lock (_lock)
            {
                _tcpProtocol.SendTcpPacket(buffer);
            }
        }

        public bool IsPlayerSocketConnected => _tcpProtocol.IsSocketConnected;

        public bool IsPlayerTcpConnected => _tcpProtocol.IsConnected;

        public bool IsPlayerUdpConnected { get; set; } = false; //Find a better way of confirming this other than manually marking it.

        public bool IsPlayerSetupComplete => IsPlayerTcpConnected && IsPlayerUdpConnected;

        public AuthorizationLevel AuthorizationLevel { get; set; } = Core.AuthorizationLevel.None;

        /// <summary>
        /// This player's id. Generated in constructor.
        /// </summary>
        public readonly int PlayerID;

        /// <summary>
        /// This player's guid.
        /// </summary>
        public readonly Guid ClientGuid;

        /// <summary>
        /// The remote tcp IpEndPoint for this player.
        /// </summary>
        public IPEndPoint RemoteTcpEndPoint => _tcpProtocol.tcpEndPoint;

        public long LastReceiveTime => _tcpProtocol.lastReceivedTime;

        public long TimeoutTime => _tcpProtocol.timeoutTime;

        public bool NextPacket(out Buffer buffer)
        {
            lock (_lock)
            {
                if (_tcpProtocol.ReceivePacket(out buffer)) return true;
            }
            return false;
        }

        

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
            lock (PlayerDictionary)
            {
                var newPlayer = new NPlayer(socket);
                PlayerDictionary.Add(newPlayer.PlayerID, newPlayer);
                Print(newPlayer.RemoteTcpEndPoint + " connecting...");
            }
        }

        public static bool RemovePlayer(int playerId)
        {
            lock (PlayerDictionary)
            {
                if (!PlayerDictionary.ContainsKey(playerId)) return false;

                playerDisconnected?.Invoke(PlayerDictionary[playerId]);
                if (GetPlayer(playerId).IsPlayerUdpConnected)
                {
                    PlayerUdpEnpointDictionary.Remove(GetPlayer(playerId).UdpEndpoint);
                }
                PlayerDictionary.Remove(playerId);
              
                Print($"Player {playerId} has disconnected.");
                _idIncrementor--;
                return true;
            }
        }

        public static NPlayer GetPlayer(int playerId)
        {
            lock (PlayerDictionary)
            {
                return PlayerDictionary.ContainsKey(playerId) ? PlayerDictionary[playerId] : null;
            }
        }

        public static NPlayer GetPlayer(IPEndPoint playerUdpEndPoint)
        {
            lock (PlayerUdpEnpointDictionary)
            {
                return PlayerUdpEnpointDictionary.ContainsKey(playerUdpEndPoint) ? PlayerUdpEnpointDictionary[playerUdpEndPoint] : null;
            }
        }

        public static Dictionary<int, NPlayer> PlayerDictionary = new Dictionary<int, NPlayer>();
        public static Dictionary<IPEndPoint, NPlayer> PlayerUdpEnpointDictionary = new Dictionary<IPEndPoint, NPlayer>();

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

