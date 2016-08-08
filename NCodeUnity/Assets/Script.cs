using UnityEngine;
using System.Collections;
using NCode;
using UnityEngine.UI;

public class Script : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        NClientManager.Connect("127.0.0.1", 5127);
	}
}
