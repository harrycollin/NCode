using UnityEngine;
using SickDev.CommandSystem;
using DevConsole;
using NCode.Client;
using NCode.Core.Entity;
using System.Collections.Generic;
using System.Collections;
using NCode.Core.Utilities;

public class NCodeConsoleCommands : MonoBehaviour {

    static System.Collections.Generic.List<GameObject> networkObjects = new System.Collections.Generic.List<GameObject>();

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
    static void SpawnEntity(int channel, int id)
    {
        NetworkManager.CreateEntity(channel, NetworkManager.GetPrefab(id), Vector3.zero, Quaternion.identity);
    }

    [Command]
    static void SetEntityOwner(int newOwner)
    {
        selectedEntity.Owner = newOwner;
        NetworkManager.UpdateEntity(selectedEntity);
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

    [Command]
    static void RunTests()
    {

        Console.LogInfo("Starting automated test...");

        Console.Log("Joining channel 1.");
        NetworkManager.JoinChannel(1);


        Console.Log("Leaving channel 1.");
        NetworkManager.LeaveChannel(1);


        Console.Log("Joining channel 100.");
        NetworkManager.JoinChannel(100);


        Console.Log("Spawning Entity 0 in channel 100");
        NetworkManager.CreateEntity(100, NetworkManager.GetPrefab(0), Vector3.zero, Quaternion.identity);


        Console.Log("Leaving channel 100");
        NetworkManager.LeaveChannel(100);


        Console.Log("Joining channel 100.");
        NetworkManager.JoinChannel(100);

    }
}