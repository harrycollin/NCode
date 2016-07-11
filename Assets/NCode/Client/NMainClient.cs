using UnityEngine;
using System.Collections;
using System.Net;
using NCode;
using System.IO;
using System.Threading;
using System;
using System.Collections.Generic;
using NCode.BaseClasses;
using NCode.Utilities;

public class NMainClient : NMainFunctionsClient
{
    /// <summary>
    /// Starts the connection process 
    /// </summary>
    /// <param name="ip"></param>
    public void Start(IPEndPoint ip)
    {
        if (TcpClient.Connect(ip))
        {
            
        }
    }

    /// <summary>
    /// Checks for queued packet and carries out routine functions. 
    /// </summary>
    public void ClientUpdate()
    {
        if (!TcpClient.isSocketConnected) { return; }

        ClientTime = DateTime.UtcNow.Ticks / 10000;

        if (TcpClient.NextPacket(out tempPacket))
        {
            ProcessPacket(tempPacket);
        }
        if (LastPingTime + 3000 < ClientTime)
        {
            LastPingTime = ClientTime;
            BinaryWriter writer = TcpClient.BeginSend(Packet.RequestPing);
            writer.Write(ClientTime);
            TcpClient.EndSend();
        }
        Thread.Sleep(1);

    }

    /// <summary>
    /// Processes queued packets. 
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    bool ProcessPacket(NPacketContainer packet)
    {
        BinaryReader reader = packet.BeginReading();
        Packet p = packet.packetid;

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
                    Tools.Print("Packet.RFC");

                    int channelID = reader.ReadInt32();
                    Guid guid = ((Guid)Converters.ConvertByteArrayToObject(reader.ReadByteArrayEx()));
                    int RFCID = reader.ReadInt32(); 
                    object[] parameters = reader.ReadObjectArrayEx();

                    onRFC(channelID, guid, RFCID, parameters);
                    break;
                }
            case Packet.ResponsePing:
                {
                    TcpClient.LastReceiveTime = ClientTime;            
                    TcpClient.Ping = reader.ReadInt32();
                    break;
                }
            case Packet.ResponseClientInfo:
                {
                    if (!reader.ReadBoolean())
                    {
                        TcpClient.Disconnect();
                        Tools.Print("Server - Client version mismatch");
                    }
                    break;
                }
            case Packet.ClientObjectUpdate:
                {
                    ReceiveObject((NetworkObject)Converters.ConvertByteArrayToObject(reader.ReadByteArrayEx()));
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

    public bool ReceiveObject(NetworkObject obj)
    {
        if (obj == null) return false;
        if (obj.GUID == null) return false;
        if (obj.LastChannelID == 0) return false;
        if (NetworkedObjects.ContainsKey(obj.GUID))
        {
            NetworkedObjects[obj.GUID] = obj;
        }
        else
        {
            NetworkedObjects.Add(obj.GUID, obj);
            Tools.Print(obj.GUID.ToString());
        }

        WaitingForSpawn.Enqueue(obj);
        return true;
    }

    void ProcessRFC(NPacketContainer container)
    {
        if (container == null) return;
        CachedFunc cachedFnc;
        Guid ObjectGuid;
        int RFCID;

        BinaryReader reader = container.BeginReading();

        //ObjectGuid = reader.ReadGUID();
        RFCID = reader.ReadInt32();

        
        
    }
}
