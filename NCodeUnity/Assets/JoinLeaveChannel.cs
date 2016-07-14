using UnityEngine;
using System.Collections;
using NCode;

public class JoinLeaveChannel : MonoBehaviour {

    void Awake()
    {
        StartCoroutine(PeriodicCheck());
    }

    IEnumerator PeriodicCheck()
    {
        for (;;)
        {
            yield return new WaitForSeconds(0.1f);
            foreach (WorldChannel i in FindObjectsOfType<WorldChannel>())
            {
                if (Vector3.Distance(transform.position, i.transform.position) < i.JoinDistance)
                {
                    NClientManager.JoinChannel(i.ID);
                }
                else if (Vector3.Distance(transform.position, i.transform.position) > i.LeaveDistance)
                {
                    NClientManager.LeaveChannel(i.ID);
                }
            }
        }
    }
}
