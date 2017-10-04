using NCode.Core.Utilities;
using NCode.Server.Core;

namespace NCode.ClientServer
{
    public class NClientServer
    {
        private NGameServer _server;

        public void Start(string name, int tcpport, int udpport, int rconport, string password)
        {
            _server = new NGameServer(name, tcpport, udpport, rconport, password, true);
            _server.Start();
            Tools.Print($"Local server started on TCP port {tcpport}.");
        }

    }
}
