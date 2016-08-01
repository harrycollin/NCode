using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCode
{
    public class NRConThread
    {
        Thread SendThread;
        NMainThreads ServerInstance;

        public void Start(NMainThreads instance)
        {
            ServerInstance = instance;
            ServerInstance.onPlayerConnect += OnPlayerConnect;
            ServerInstance.onPlayerDisconnect += OnPlayerDisconnect;

        }

        void OnPlayerConnect(NTcpPlayer player)
        {
            NPacketContainer p = new NPacketContainer();
            BinaryWriter writer = p.BeginWriting(Packet.RConPlayerConnect);
            writer.WriteObject(player);
            foreach(NRConClient i in ServerInstance.RConClients) { i.OnPlayerConnectPackets.Enqueue(p.EndWriting()); }
        }

        void OnPlayerDisconnect(NTcpPlayer player)
        {
            NPacketContainer p = new NPacketContainer();
            BinaryWriter writer = p.BeginWriting(Packet.RConPlayerDisconnect);
            writer.WriteObject(player);
            foreach (NRConClient i in ServerInstance.RConClients) { i.OnPlayerDisconnectPackets.Enqueue(p.EndWriting()); }
        }

        void Sender()
        {
            for (;;)
            {
                for(int i = 0; i < ServerInstance.RConClients.Count; i++)
                {
                    
                }
            }
        }
    }
}
