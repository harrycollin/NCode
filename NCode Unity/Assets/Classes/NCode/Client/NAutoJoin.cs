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
        NetworkManager.CreateInstance();
        NetworkManager.onConnect += JoinAndLoad;
        NetworkManager.Connect(ServerIPAddress, ServerPort);
	}

    void JoinAndLoad()
    {
        SceneManager.LoadScene(SceneIndex);
    }
}
