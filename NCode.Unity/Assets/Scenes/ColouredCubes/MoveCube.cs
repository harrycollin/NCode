using System.Collections;
using System.Collections.Generic;
using NCode.Client;
using NCode.Core;
using UnityEngine;

[RequireComponent(typeof(NEntityLink))]
public class MoveCube : MonoBehaviour
{


    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(!GetComponent<NEntityLink>().IsMine) return;

        if (Input.GetKeyDown(KeyCode.W))
        {
            gameObject.transform.position += Vector3.forward * 2;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            gameObject.transform.position += Vector3.back * 2;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            gameObject.transform.position += Vector3.left * 2;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            gameObject.transform.position += Vector3.right * 2;
        }
        GetComponent<NEntityLink>().SendRfc(1, Packet.ForwardToAll, false, gameObject.transform.position);
    }

    [RFC(1)]
    protected void SetPosition(Vector3 v)
    {
        gameObject.transform.position = v;
    }
}
