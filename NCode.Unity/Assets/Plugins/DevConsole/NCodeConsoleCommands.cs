using UnityEngine;
using SickDev.CommandSystem;
using DevConsole;
using NCode.Client;

public class NCodeConsoleCommands : MonoBehaviour {

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
}