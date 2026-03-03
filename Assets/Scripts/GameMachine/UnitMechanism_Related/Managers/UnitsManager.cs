using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UnitsManager : MonoBehaviour
{
    public static UnitsManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject testSoldier;

    public GameObject testVehicle;

}
