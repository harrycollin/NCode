using UnityEngine;
using System.Collections;

public class VehicleCamConfig : MonoBehaviour {

    public float Height = 6f;
    public float Distance = 10f;
    CameraManager camManager;


    void Start()
    {
        camManager = FindObjectOfType<CameraManager>();
    }

    public void GetIn()    
    {
        camManager.GetInVehicle(transform);
        camManager.Height = Height;
        camManager.Distance = Distance;
    }
    public void GetOut()
    {
        camManager.GetOutVehicle();
    }
}
