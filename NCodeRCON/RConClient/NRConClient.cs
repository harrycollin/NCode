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
        /// <summary>
        /// The number of players on the remote server
        /// </summary>
        public int PlayerCount { get { return Players.Count; } }
        /// <summary>
        /// A list of the remote server'
        /// </summary>
        public System.Collections.Generic.List<NPlayer> Players = new System.Collections.Generic.List<NPlayer>();
        /// <summary>
        /// The RCon server's password
        /// </summary>
        public string RconPassword;
        /// <summary>
        /// Standard TCP Protocol
        /// </summary>
        public NTcpProtocol TcpProtocol = new NTcpProtocol();
        /// <summary>
        /// Whether this RConClient has been authenticated.
        /// </summary>
        public bool Authenticated;
        /// <summary>
        /// The UTC time in ms of this client.
        /// </summary>
        public long ClientTime = 0;
        /// <summary>
        /// The last time this client sent a ping.
        /// </summary>
        public long LastPingTime = 0;

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
                ClientTime = DateTime.UtcNow.Ticks / 10000;

                if (LastPingTime + 3000 < ClientTime && TcpProtocol.State == NTcpProtocol.ConnectionState.connected)
                {
                    LastPingTime = ClientTime;
                    BinaryWriter writer = TcpProtocol.BeginSend(Packet.Ping);
                    TcpProtocol.EndSend();
                }

                if (TcpProtocol.NextPacket(out packet))
                {
                    ProcessPacket(packet);
                }

                Thread.Sleep(0);
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
                            Authenticated = true;                      
                        else                        
                            Authenticated = false;                       
                        break;
                    }
            }
        }

        public BinaryWriter BeginSend(Packet p)
        {
            return TcpProtocol.BeginSend(p);
        }

        public void EndSend()
        {
            TcpProtocol.EndSend();
        }

    }
}
