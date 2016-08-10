using UnityEngine;
using System.Collections;
using NCode;
using System;

public class NAutoJoin : MonoBehaviour {

    
    public string ServerIPAddress;
    public int ServerPort;
	// Use this for initialization
	void Start ()
    {
        try
        {
            NClientManager.Connect(ServerIPAddress, ServerPort);
        }catch(Exception e)
        {
            Tools.Print("Failed to connect to server. Ensure there is always an instance of NClientManager", Tools.MessageType.error, e);
        }
	}
}
