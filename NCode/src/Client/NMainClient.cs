using System.Net;
using System.IO;
using System;
using NCode.Utilities;
using NCode.Core.Protocols;
using UnityEngine;
using System.Threading;

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
                Buffer tempBuffer;
                IPEndPoint tempIP;

                bool keepProcessing = true;

                //Process all Udp packets first.
                while (keepProcessing && UdpClient.ReceivePacket(out tempBuffer, out tempIP))
                {
                    keepProcessing = ProcessPacket(tempBuffer, tempIP);
                    tempBuffer.Recycle();

                }

                //Process Tcp packets
                while (keepProcessing && TcpClient.ReceivePacket(out tempBuffer))
                {
                    ProcessPacket(tempBuffer, null);
                    tempBuffer.Recycle();
                }

                //Check if we need to ping (failing to ping reguarly will result in the server disconnecting this player).
                if (LastPingTime + 3000 < ClientTime && TcpClient.stage == TNTcpProtocol.Stage.Connected)
                {
                    LastPingTime = ClientTime;
                    BinaryWriter writer = TcpClient.BeginSend(Packet.Ping);
                    TcpClient.EndSend();
                }
            
        }

        /// <summary>
        /// Processes queued packets. 
        /// </summary>
        bool ProcessPacket(Buffer packet, IPEndPoint source)
        {
            BinaryReader reader = packet.BeginReading();
            int packetID = reader.ReadByte();
            Packet response = (Packet)packetID;
            
            //Filters out any packets that have custom handlers. 
            OnPacket callback;

            if (packetHandlers.TryGetValue(response, out callback) && callback != null)
            {
                callback(response, reader);
                return true;
            }
            Tools.Print(response.ToString());
            switch (response)
            {
                case Packet.ForwardToChannels:
                    {                     
                        Guid guid = (Guid)reader.ReadObject();
                        int RFCID = reader.ReadInt32();
                        object[] parameters = reader.ReadObjectArrayEx();
                        onRFC(guid, RFCID, parameters);
                        break;
                    }               
                case Packet.Ping:
                    {
                        TcpClient.lastReceivedTime = ClientTime;
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
                        Guid guid = (Guid)reader.ReadObject();
                        Tools.Print(guid);
                        onDestroyObject(guid);
                        break;
                    }
                
                case Packet.ResponseSpawnPlayerObject:
                    {
                        Tools.Print("Response for spawn");
                        if (reader.ReadBoolean())
                        {
                            onSpawnPlayerResponse((NetworkObject)reader.ReadObject());
                        }
                        break;
                    }
                case Packet.SetupUDP:
                    {
                        if (reader.ReadBoolean())
                        {
                            Tools.Print("UDP Setup!");
                            onConnect();

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