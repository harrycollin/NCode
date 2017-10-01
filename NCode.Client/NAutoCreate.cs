using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NCode.Client
{
    public class NAutoCreate : MonoBehaviour
    {
        public GameObject Prefab;

        void Awake()
        {
            NetworkManager.Instantiate(0, Vector3.zero, Quaternion.identity);
        }
    }
}
