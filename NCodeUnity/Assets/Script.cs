using UnityEngine;
using System.Collections;
using NCode;
using UnityEngine.UI;
using NCode.Core.Client;

public class Script : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        NClientManager.CreateInstance();
        NClientManager.Connect("127.0.0.1", 5127);
	}
}
