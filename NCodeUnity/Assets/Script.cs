using UnityEngine;
using System.Collections;
using NCode;
using UnityEngine.UI;

public class Script : MonoBehaviour
{
	// Use this for initialization
	void Start ()
    {
        NClientManager.Connect("82.21.29.175", 5127);
	}
}
