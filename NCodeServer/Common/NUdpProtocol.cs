using System.Net.Sockets;

namespace NCode
{
    public class NUdpProtocol
    {
        Socket thisSocket;
        int thisPort;

        public void Start(int port)
        {
            thisPort = port;
            thisSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        }
    }
}
