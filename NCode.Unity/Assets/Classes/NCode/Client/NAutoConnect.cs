using System.Collections;
using System.Collections.Generic;
using NCode;
using NCode.Core.Client;
using UnityEngine;

public class NAutoConnect : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    NetworkManager.Connect("127.0.0.1", 5127);
	    Tools.Print("Yoo");
	}
}
