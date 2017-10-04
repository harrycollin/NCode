using UnityEngine;
using SickDev.CommandSystem;
using DevConsole;
using NCode.Client;

public class Example : MonoBehaviour {
    [Command]
    static void TimeScale(float value) {
        Time.timeScale = value;
        Console.Log("Change successful", Color.green);
    }

    [Command]
    static void ShowTime() {
        Console.Log(Time.time.ToString());
    }

    [Command]
    static void SetGravity(Vector3 value) {
        Physics.gravity = value;
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
    static void SpawnEntity( int id)
    {
        NetworkManager.Instantiate(1, id, Vector3.zero, Quaternion.identity);
    }
}