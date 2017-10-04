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
    void Update()
    {
        //if (true) return;

        if (!GetComponent<NEntityLink>().IsMine) return;

        if (Input.GetKey(KeyCode.W))
        {
            gameObject.transform.position += Vector3.forward * 2 * Time.deltaTime; ;
        }
        if (Input.GetKey(KeyCode.S))
        {
            gameObject.transform.position += Vector3.back * 2 * Time.deltaTime; ;
        }
        if (Input.GetKey(KeyCode.A))
        {
            gameObject.transform.position += Vector3.left * 2 * Time.deltaTime; ;
        }
        if (Input.GetKey(KeyCode.D))
        {
            gameObject.transform.position += Vector3.right * 2 * Time.deltaTime; ;
        }
        GetComponent<NEntityLink>().SendRfc(1, Packet.ForwardToChannels, false, transform.position);
    }

    [RFC(1)]
    protected void SetPosition(Vector3 v)
    {
        gameObject.transform.position = v;
    }
}
