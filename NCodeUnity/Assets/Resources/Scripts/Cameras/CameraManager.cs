using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

    public Camera VehicleCam;
    public float Height = 6f;
    public float Distance = 10f;
    public Transform VehicleCamTarget;

    public void GetInVehicle(Transform target)
    {
        VehicleCam.enabled = true;
        VehicleCam.GetComponent<CarCamera>().playerCar = target;
        VehicleCam.GetComponent<CarCamera>().height = Height;
        VehicleCam.GetComponent<CarCamera>().distance = Distance;

    }
    public void GetOutVehicle()
    {
        VehicleCam.enabled = false;
        VehicleCam.GetComponent<CarCamera>().playerCar = null;
    }
}
