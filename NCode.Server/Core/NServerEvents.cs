using NCode.Core.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode.Server.Core
{
    public static class NServerEvents
    {
        public delegate void PlayerConnected(NPlayer player);
        public static PlayerConnected playerConnected;

        public delegate void PlayerDisconnected(NPlayer player);
        public static PlayerDisconnected playerDisconnected;

        public delegate void EntityUpdated(NNetworkEntity entity);
        public static EntityUpdated entityUpdated;


    }
}
