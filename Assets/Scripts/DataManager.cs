﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.IO.Ports;
//using System.IO.

public class DataManager : MonoBehaviour
{
    /// <summary>
    /// Daftar data 
    /// </summary>
    public List<DroneUnit> units = new List<DroneUnit>();

    /// <summary>
    /// Index data yang ditampilkan sekarang
    /// </summary>
    [NonSerialized]
    public int current = -1;

    public DroneUnit CurrentData => current < 0 ? new DroneUnit(0, DateTime.Now) : units[current];

    public void PushData(DroneUnit unit)
    {
        unit.index = NextIndex;
        units.Add(unit);
        current++;
    }

    public int NextIndex => units.Count + 1;

    public bool fakeData = true;
     
    void Start()
    {
    }

    void OnEnable()
    {
        current = units.Count - 1;
        if (fakeData)
            StartCoroutine(AddFakeData());
    }

    IEnumerator AddFakeData()
    {
        while (fakeData)
        {
            yield return new WaitForSeconds(2);
            PushData(DroneUnitSerializer.Random(NextIndex, CurrentData));
        }
    }

    public void Clear()
    {
        units.Clear();
        current = -1;
    }


    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Fullscreen(bool goFull)
    {
        Screen.fullScreen = goFull;
    }

    public void Import(string file)
    {
        var text = File.ReadAllText(file);
        JsonUtility.FromJsonOverwrite(text, this);
    }

    public void Export(string file)
    {
        File.WriteAllText(file, JsonUtility.ToJson(this));
    }

}

[Serializable]
public struct DroneUnit
{
    // Unix Time: Selisih detik dari 1 Januari 1970 UTC
    public int index;
    public long unixTime;
    public float pitch, yaw, roll;
    public float accX, accY, accZ;
    public float angle;
    public double lat, lng;
    public float alt, temp, pressure;

    public Vector3 YawPitchRoll => new Vector3(pitch, yaw, roll);
    public Vector3 Acceleration => new Vector3(accX, accY, accZ);
    public Vector3 LonLat => new Vector2((float)lng, (float)lat);

    public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;

    public DroneUnit(int idx, DateTime time)
    {
        index = idx;
        unixTime = new DateTimeOffset(time).ToUnixTimeSeconds();
        pitch = 0;
        yaw = 0;
        roll = 0;
        accX = 0;
        accY = 0;
        accZ = 0;
        angle = 0;
        lat = 0;
        lng = 0;
        alt = 0;
        temp = 0;
        pressure = 0;
    }

}

public static class DroneUnitSerializer
{
    public static DroneUnit Parse(string line)
    {
        var units = line.Trim(' ', '\r', '\n', '\t').Replace(" ", "").Split(',');
        var unit = new DroneUnit
        {
            index = int.Parse(units[0]),
            unixTime = long.Parse(units[1]),
            pitch = float.Parse(units[2]),
            yaw = float.Parse(units[3]),
            roll = float.Parse(units[4]),
            accX = float.Parse(units[5]),
            accY = float.Parse(units[6]),
            accZ = float.Parse(units[7]),
            angle = float.Parse(units[8]),
            lat = double.Parse(units[9]),
            lng = double.Parse(units[10]),
            alt = float.Parse(units[11]),
            temp = float.Parse(units[12]),
            pressure = float.Parse(units[13])
        };
        return unit;
    }

    public static string Stringify(DroneUnit unit)
    {
        return string.Join(",", new string[] {
            unit.index.ToString(), unit.unixTime.ToString(),
            unit.pitch.ToString(), unit.yaw.ToString(), unit.roll.ToString(),
            unit.accX.ToString(), unit.accY.ToString(), unit.accZ.ToString(),
            unit.angle.ToString(),
            unit.lat.ToString(), unit.lng.ToString(), unit.alt.ToString(),
            unit.temp.ToString(), unit.pressure.ToString()
        });
    }

    /// <summary> Generate Fake Data </summary>
    public static DroneUnit Random(int idx, DroneUnit rndseed = new DroneUnit())
    {
        float randDegree(float last)
        {
            return last + UnityEngine.Random.Range(-1f, 1f) * 45;
        }
        float randDist(float last)
        {
            return last + UnityEngine.Random.Range(-1f, 1f) * .1f;
        }

        var unit = new DroneUnit
        {
            index = idx,
            unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            pitch = randDegree(rndseed.pitch),
            yaw = randDegree(rndseed.yaw),
            roll = randDegree(rndseed.roll),
            accX = randDegree(rndseed.accX),
            accY = randDegree(rndseed.accY),
            accZ = randDegree(rndseed.accZ),
            angle = randDegree(rndseed.angle),
            lat = randDegree((float)rndseed.lat),
            lng = randDegree((float)rndseed.lng),
            alt = randDist(rndseed.alt),
            temp = randDist(rndseed.pitch),
            pressure = randDist(rndseed.pitch)
        };
        return unit;
    }
}