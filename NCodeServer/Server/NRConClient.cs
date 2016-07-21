using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NCode;
namespace NCode
{ 
    
    public class NRConClient : NTcpProtocol
    {
        public Queue<byte[]> OnPlayerConnectPackets = new Queue<byte[]>();
        public Queue<byte[]> OnPlayerDisconnectPackets = new Queue<byte[]>();

    }
}
