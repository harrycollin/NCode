using UnityEngine;
using System.Collections;
using NCode;

public class RollerBallScene : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        if(NClientManager.Connect("127.0.0.1", 5127))
        {
            StartCoroutine(i());
        }
    }
    IEnumerator i()
    {
        yield return new WaitForSeconds(1);
        NClientManager.JoinChannel(10);
        yield return new WaitForSeconds(1);
        Spawn();

    }


    public void Spawn()
    {
        Tools.Print("Spa");
        NClientManager.CreateNewObject(10, 1, false, new Vector3(1, 1, 1), new Quaternion(0, 0, 0, 0));   
    }
}
