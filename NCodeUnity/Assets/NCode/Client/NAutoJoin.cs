using UnityEngine;
using System.Collections;
using NCode;
using System;
using NCode.Core.Client;
using UnityEngine.SceneManagement;

public class NAutoJoin : MonoBehaviour {

    public int SceneIndex;
    public string ServerIPAddress;
    public int ServerPort;
	// Use this for initialization
	void Start ()
    {
        try
        {
            NClientManager.CreateInstance();
            NClientManager.Connect(ServerIPAddress, ServerPort);
            NClientManager.onConnect += JoinAndLoad;
        }
        catch (Exception e)
        {
            Tools.Print("Failed to connect to server. Ensure there is always an staticInstance of NClientManager", Tools.MessageType.error, e);
        }
	}

    void JoinAndLoad()
    {
        NClientManager.JoinChannel(10);
        SceneManager.LoadScene(SceneIndex);
    }
}
