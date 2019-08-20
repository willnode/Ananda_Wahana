using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DroneVisualizer : MonoBehaviour
{
    public Transform drone;

    Quaternion targetRot = Quaternion.identity, currentRot = Quaternion.identity;

    public void ValidateDrone()
    {
        var data = GetComponent<DataManager>().CurrentData;

        targetRot = Quaternion.Euler(data.YawPitchRoll);
    }

    private int lastCurrentIdx = -1;

    // Update is called once per frame
    void Update()
    {
        if (GetComponent<DataManager>().current != lastCurrentIdx)
        {
            lastCurrentIdx = GetComponent<DataManager>().current;
            ValidateDrone();
        }
        // Transisi smooth ke targetRot
        currentRot = Quaternion.Slerp(currentRot, targetRot, Time.deltaTime * 3);
        drone.rotation = currentRot;
    }
}
