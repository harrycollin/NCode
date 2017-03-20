using System.Collections.Generic;
using NCode.Core.Protocols;

namespace NCode.Server
{ 
    
    public class NRConClient : NTcpProtocol
    {
        public Queue<byte[]> OnPlayerConnectPackets = new Queue<byte[]>();
        public Queue<byte[]> OnPlayerDisconnectPackets = new Queue<byte[]>();

    }
}
