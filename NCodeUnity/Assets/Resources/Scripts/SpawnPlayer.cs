using UnityEngine;
using System.Collections;
using NCode.Core.Client;
using NCode;
using System.IO;
using NCode.Core;

public class SpawnPlayer : MonoBehaviour
{


    // Use this for initialization
    void Start()
    {
        NClientManager.CreateNewObject(10, 1, false, new Vector3(30, 2, 44), new Quaternion(0, 0, 0, 0));
        
    }
}
