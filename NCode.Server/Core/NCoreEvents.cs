using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NCode.Server.Core
{
    public static class NCoreEvents
    {
        public delegate void PlayerConnected(NPlayer player);
        public static PlayerConnected playerConnected;

        public delegate void PlayerDisconnected(NPlayer player);
        public static PlayerDisconnected playerDisconnected;


    }
}
