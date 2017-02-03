using System;
using System.Collections;
using NCode.Core.BaseClasses;
using System.Collections.Generic;

namespace NCode.Core
{
    /// <summary>
    /// The lowest point of inheritance when it comes to anything player orientated. 
    /// </summary>
    [Serializable]
    public class NPlayer
    {
        /// <summary>
        /// This player's authorization level. 
        /// </summary>
        private AuthorizationLevel _authorizationLevel = Core.AuthorizationLevel.None;

        /// <summary>
        /// Whether this player is connected on Tcp.
        /// </summary>
        private bool _isPlayerTcpConnected = false;

        /// <summary>
        /// Whether this player is connected on Udp.
        /// </summary>
        private bool _isPlayerUdpConnected = false;

        /// <summary>
        /// Whether this player's setup with the server is complete.
        /// </summary>
        private bool _isPlayerSetupComplete = false;

        /// <summary>
        /// This player's ThisGuid. Generated in constructor.
        /// </summary>
        public Guid ClientGuid = Guid.Empty;

        public string SteamId = null;

        public string Aliase = null;

        private static object _mLock = new int();

        /// <summary>
        /// This player's position buffer. Used by the server to determine in-game combat accuracy & anti-cheat.
        /// </summary>
        private List<KeyValuePair<DateTime, V3>> _positionBuffer = new List<KeyValuePair<DateTime, V3>>();

        
        public AuthorizationLevel AuthorizationLevel
        {
            get
            {
                return _authorizationLevel;
            }

            set
            {
                _authorizationLevel = value;
            }
        }

        public NPlayer ThisPlayer
        {
            get
            {
                NPlayer player = new NPlayer
                {
                    SteamId = SteamId,
                    ClientGuid = ClientGuid,
                    Aliase = Aliase
                };
                return player;
            }
        }

        public bool IsPlayerTcpConnected
        {
            get
            {
                return _isPlayerTcpConnected;
            }

            set
            {
                _isPlayerTcpConnected = value;
            }
        }

        public bool IsPlayerUdpConnected
        {
            get
            {
                return _isPlayerUdpConnected;
            }

            set
            {
                _isPlayerUdpConnected = value;
            }
        }

        public bool IsPlayerSetupComplete
        {
            get
            {
                return _isPlayerSetupComplete;
            }

            set
            {
                _isPlayerSetupComplete = value;
            }
        }
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
