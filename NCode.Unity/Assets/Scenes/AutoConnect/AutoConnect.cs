using NCode.Core.Client;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoConnect : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        NetworkManager.Connect("127.0.0.1", 5127);	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
