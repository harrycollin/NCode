using System.Net;
using System.IO;
using System;
using NCode.Utilities;
using NCode.Core.Protocols;

namespace NCode.Core.Client
{
    public class NMainClient : NMainFunctionsClient
    {
        /// <summary>
        /// Checks for queued packet and carries out routine functions. 
        /// </summary>
        public void ClientUpdate()
        {
            if (!TcpClient.isSocketConnected) { return; }

            //Set the client time
            ClientTime = DateTime.UtcNow.Ticks / 10000;

            //Temporary variables for Udp packets
            NBuffer tempUdpBuffer;
            IPEndPoint tempIP;

            //Process all Udp packets first.
            if (UdpClient.ReceivePacket(out tempUdpBuffer, out tempIP))
            {
                ProcessPacket(tempUdpBuffer, tempIP);
            }

            //Temp buffer for Tcp packets
            NBuffer tempTcpBuffer;

            //Process Tcp packets
            if (TcpClient.NextPacket(out tempTcpBuffer))
            {
                ProcessPacket(tempTcpBuffer);
            }

            //Check if we need to ping (failing to ping reguarly will result in the server disconnecting this player).
            if (LastPingTime + 3000 < ClientTime && TcpClient.State == NTcpProtocol.ConnectionState.connected)
            {
                LastPingTime = ClientTime;
                TcpClient.Ping();
            }
        }

        /// <summary>
        /// Processes queued packets. 
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        bool ProcessPacket(NBuffer packet, IPEndPoint source = null)
        {
            BinaryReader reader = packet.BeginReading();
            Packet p = packet.packet;

            //Filters out any packets that have custom handlers. 
            OnPacket callback;
            if (packetHandlers.TryGetValue(p, out callback) && callback != null)
            {
                callback(p, reader);
                return true;
            }

            switch (p)
            {
                case Packet.RFC:
                    {
                        int channelID = reader.ReadInt32();
                        Guid guid = reader.ReadGUID();
                        int RFCID = reader.ReadInt32();
                        object[] parameters = reader.ReadObjectArrayEx();
                        onRFC(channelID, guid, RFCID, parameters);
                        break;
                    }
                case Packet.ResponseJoinChannel:
                    {
                        int channelID = reader.ReadInt32();
                        if (channelID != 0)
                        {
                            if (reader.ReadBoolean())
                            {
                                if (!ConnectedChannels.Contains(channelID)) { ConnectedChannels.Add(channelID); }
                            }
                        }
                        break;
                    }
                case Packet.Ping:
                    {
                        TcpClient.LastReceiveTime = ClientTime;
                        TcpClient.PingInMs = reader.ReadInt32();
                        break;
                    }
                case Packet.ResponseClientSetup:
                    {
                        if (!reader.ReadBoolean())
                        {
                            TcpClient.Disconnect();
                            Tools.Print("Server - Client version mismatch");
                        }
                        else
                        {
                            TcpClient.State = NTcpProtocol.ConnectionState.connected;
                            IPEndPoint remoteIp = TcpClient.thisSocket.RemoteEndPoint as IPEndPoint;
                            ServerUdpEndpoint = new IPEndPoint(remoteIp.Address, reader.ReadInt32());
                            TcpClient.ClientGUID = (Guid)reader.ReadObject();

                            BinaryWriter writer = UdpClient.BeginSend(Packet.SetupUDP);
                            writer.WriteObject(TcpClient.ClientGUID);
                            Tools.Print("Reading GUID:" + TcpClient.ClientGUID.ToString());
                            UdpClient.EndSend(ServerUdpEndpoint);
                        }
                        break;
                    }
                case Packet.ClientObjectUpdate:
                    {
                        ReceiveObject((NetworkObject)reader.ReadObject());
                        break;
                    }
                case Packet.PlayerUpdate:
                    {
                        ReceivePlayerUpdate((NPlayer)reader.ReadObject(), reader.ReadBoolean());
                        break;
                    }
                case Packet.SetupUDP:
                    {
                        if (reader.ReadBoolean())
                        {
                            Tools.Print("UDP Setup!");
                        }
                        break;
                    }
                default:
                    {
                        Tools.Print("No defined Packet");
                        break;
                    }
            }
            return false;
        }
    }
}