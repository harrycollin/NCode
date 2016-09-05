using NCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NCodeRCON.RConClient
{
    public class ServerInstance : NRConClient
    {
        public string Name { get; set; }
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public string Password { get; set; }
        
        
        public void Start()
        {
            BeginSend(Packet.RConStartGameServer);
            EndSend();
        }
        public void Stop()
        {
            if (!TcpProtocol.isSocketConnected) { return; }
            BinaryWriter writer = BeginSend(Packet.RConStopGameServer);
            EndSend();
        }
        public void Shutdown()
        {
            BinaryWriter writer = BeginSend(Packet.RConShutdownServer);
            writer.Write(2);
            EndSend();
        }
    }
}
