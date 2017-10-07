using UnityEngine;
using SickDev.CommandSystem;
using DevConsole;
using NCode.Client;
using NCode.Core.Entity;
using System.Collections.Generic;

public class NCodeConsoleCommands : MonoBehaviour {

    static List<GameObject> networkObjects = new List<GameObject>();

    static NNetworkEntity selectedEntity;

    [Command]
    static void Connect(string ip, int port)
    {
        NetworkManager.Connect(ip, port);
    }

    [Command]
    static void Disconnect()
    {
        NetworkManager.Disconnect();
    }

    [Command]
    static void JoinChannel(int id)
    {
        NetworkManager.JoinChannel(id);
    }

    [Command]
    static void LeaveChannel(int id)
    {
        NetworkManager.LeaveChannel(id);
    }

    [Command]
    static void SpawnEntity(int id)
    {
        NetworkManager.CreateEntity(1, NetworkManager.GetPrefab(id), Vector3.zero, Quaternion.identity);
    }

    [Command]
    static void ListConnectedChannels()
    {
        foreach(var i in NetworkManager.GetConnectedChannels())
        {
            Console.LogInfo(string.Format("Channel ID: {0}", i));
        }
    }

    [Command]
    static void ListEntities()
    {
        networkObjects.Clear();
        foreach (var go in FindObjectsOfType<GameObject>())
        {
            if (go.GetComponent<NEntityLink>()) networkObjects.Add(go);
        }

        for(int i = 0; i < networkObjects.Count; i++)
        {
            Console.LogInfo(string.Format("Index: {0}, Name: {1}", i, networkObjects[i].name));
        }
    }

    [Command]
    static void SelectEntity(int index)
    {
        selectedEntity = NetworkManager.GetEntity(networkObjects[index].GetComponent<NEntityLink>().Guid);
        Console.LogInfo(string.Format("Entity {0} has been selected!", index));
    }

    [Command]
    static void TransferEntity(int channela, int channelb)
    {
        NetworkManager.TransferEntity(selectedEntity.Guid, channela, channelb);
    }
}