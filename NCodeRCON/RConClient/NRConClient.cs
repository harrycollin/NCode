using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NCode
{
    public class NRConClient
    {

        public string RconPassword;
        public NTcpProtocol TcpProtocol = new NTcpProtocol();
        public bool Authenticated;

        Thread MainThread;
        NPacketContainer packet;

        public bool Connect(IPAddress ip, int port)
        {
            IPEndPoint ipend = new IPEndPoint(ip, port);
            try
            {
                TcpProtocol.Connect(ipend);              
            }
            catch (Exception e)
            {
                Tools.Print("Failed to connect", Tools.MessageType.error, e);
                return false;
            }
            TcpProtocol.BeginListening();
            MainThread = new Thread(MainThreadLoop);
            MainThread.Start();
            return true;
        }
        
        

        void MainThreadLoop()
        {
            for (;;)
            {
                if(TcpProtocol.NextPacket(out packet))
                {
                    ProcessPacket(packet);
                }
            }
        }

        void ProcessPacket(NPacketContainer packet)
        {
            BinaryReader reader = packet.BeginReading();
            Packet p = packet.packetid;

            switch (p)
            {
                case Packet.RConResponseAuthenticate:
                    {
                        if (reader.ReadBoolean())
                        {
                            Authenticated = true;
                        }
                        else
                        {
                            Authenticated = false;
                        }
                        break;
                    }
            }
        }

    }
}
