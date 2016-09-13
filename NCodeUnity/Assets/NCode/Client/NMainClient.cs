using System.Net;
using System.IO;
using System;
using NCode.Utilities;
using NCode.Core.Protocols;
using UnityEngine;

namespace NCode.Core.Client
{
    public class NMainClient : NMainFunctionsClient
    {
        /// <summary>
        /// Checks for queued packet and carries out routine functions. 
        /// </summary>
        public void ClientUpdate()
        {
            if (!TcpClient.isSocketConnected) { onDisconnect(); return; }

            //Set the client time
            ClientTime = DateTime.UtcNow.Ticks / 10000;

            //Temporary variables for packets
            NBuffer tempBuffer;
            IPEndPoint tempIP;

            bool keepProcessing = true;
       
            //Process all Udp packets first.
            while (keepProcessing && UdpClient.ReceivePacket(out tempBuffer, out tempIP))
            {
                keepProcessing = ProcessPacket(tempBuffer, tempIP);
            }

            //Process Tcp packets
            while (keepProcessing && TcpClient.NextPacket(out tempBuffer))
            {               
                ProcessPacket(tempBuffer);               
            }

            //Check if we need to ping (failing to ping reguarly will result in the server disconnecting this player).
            if (LastPingTime + 3000 < ClientTime && TcpClient.State == NTcpProtocol.ConnectionState.connected)
            {
                LastPingTime = ClientTime;
                BinaryWriter writer = TcpClient.BeginSend(Packet.Ping);
                TcpClient.EndSend();
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
                        //int channelID = reader.ReadInt32();
                        //Guid guid = reader.ReadGUID();
                        //int RFCID = reader.ReadInt32();
                        //object[] parameters = reader.ReadObjectArrayEx();
                        onRFC(reader.ReadInt32(), (Guid)reader.ReadObject(), reader.ReadInt32(), reader.ReadObjectArrayEx());
                        break;
                    }
                case Packet.ResponseJoinChannel:
                    {
                        ResponseJoinChannel(reader);
                        break;
                    }
                case Packet.ResponseLeaveChannel:
                    {
                        ResponseLeaveChannel(reader);
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
                        ResponseClientSetup(reader);
                        break;
                    }
                case Packet.ClientObjectUpdate:
                    {
                        ReceiveObject((NetworkObject)reader.ReadObject());
                        break;
                    }
                case Packet.ResponseDestroyObject:
                    {
                        onDestroyObject((Guid)reader.ReadObject());
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
            reader = null;
            return false;
        }
    }
}