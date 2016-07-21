using NCode;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NCodeRCON.RConClient
{
    public class ServerInstance
    {
        public string Name { get; set; }
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        public System.Collections.Generic.List<NPlayer> Players = new System.Collections.Generic.List<NPlayer>();
        public NRConClient client = new NRConClient();
        
         
    }
}
