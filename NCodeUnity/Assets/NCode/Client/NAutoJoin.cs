using UnityEngine;
using System.Collections;
using NCode;
using System;
using NCode.Core.Client;

public class NAutoJoin : MonoBehaviour {

    
    public string ServerIPAddress;
    public int ServerPort;
	// Use this for initialization
	void Start ()
    {
        try
        {
            NClientManager.CreateInstance();
            NClientManager.Connect(ServerIPAddress, ServerPort);
        }
        catch (Exception e)
        {
            Tools.Print("Failed to connect to server. Ensure there is always an staticInstance of NClientManager", Tools.MessageType.error, e);
        }
	}
}
