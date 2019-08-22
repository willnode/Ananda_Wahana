using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using SimpleFileBrowser;

public class UIVisualizer : MonoBehaviour
{
    [Serializable]
    public class TextUI
    {
        public Text index, datetime;
        public Text pitch, yaw, roll;
        public Text accX, accY, accZ;
        public Text magX, magY, magZ;
        public Text lat, lng, alt;
        public Text temp, pressure;
    }
    [Serializable]
    public class GraphUI
    {
        public UILineRenderer pitch, yaw, roll;
        public UILineRenderer accX, accY, accZ;
        public UILineRenderer magX, magY, magZ;
        public int samplesWidth = 10;
    }
    [Serializable]
    public class MapUI
    {
        public UILineRenderer graph;
        public RectTransform pinpoint;
        public Text distText;
    }


    public TextUI textUI = new TextUI();

    public GraphUI graphUI = new GraphUI();

    public MapUI mapUI = new MapUI();

    public UnityUITable.Table table;

    public UILineInfo orientation;


    public void ValidateUI ()
    {
        var manager = GetComponent<DataManager>();
        var data = manager.CurrentData;

        textUI.index.text = data.index.ToString();
        textUI.datetime.text = data.DateTime.ToLongTimeString();
        textUI.pitch.text = data.pitch.ToString("0.0°");
        textUI.yaw.text = data.yaw.ToString("0.0°");
        textUI.roll.text = data.roll.ToString("0.0°");
        textUI.accX.text = data.accX.ToString("0.0°");
        textUI.accY.text = data.accY.ToString("0.0°");
        textUI.accZ.text = data.accZ.ToString("0.0°");
        textUI.magX.text = data.magX.ToString("0.0°");
        textUI.magY.text = data.magY.ToString("0.0°");
        textUI.magZ.text = data.magZ.ToString("0.0°");
        textUI.lat.text = data.lat.ToString("0.000°");
        textUI.lng.text = data.lng.ToString("0.000°");
        textUI.alt.text = data.alt.ToString("0.0 m");
        textUI.temp.text = data.temp.ToString("0.0 °C");
        textUI.pressure.text = data.pressure.ToString("0.0 hPa");

        table.UpdateContent();
        table.SetSelected(data.index);

        void HandleGraph(UILineRenderer graph, Func<DroneUnit, float> fetchFunc)
        {
            var points = graph.Points;
            var length = Math.Min(graphUI.samplesWidth, manager.units.Count);
            var start = data.index - length;
            var end = data.index;

            if (points.Length != length)
                points = new Vector2[length];
            for (int i = start; i < end; i++)
            {
                points[i - start] = new Vector2((float)(i - start) / (graphUI.samplesWidth - 1),
                   Mathf.Repeat(fetchFunc(manager.units[i]), 360) / 360); 
            }
            graph.Points = points;
            graph.SetVerticesDirty();
        }

        HandleGraph(graphUI.pitch, (x) => x.pitch);
        HandleGraph(graphUI.yaw, (x) => x.yaw);
        HandleGraph(graphUI.roll, (x) => x.roll);
        HandleGraph(graphUI.accX, (x) => x.accX);
        HandleGraph(graphUI.accY, (x) => x.accY);
        HandleGraph(graphUI.accZ, (x) => x.accZ);
        HandleGraph(graphUI.magX, (x) => x.magX);
        HandleGraph(graphUI.magY, (x) => x.magY);
        HandleGraph(graphUI.magZ, (x) => x.magZ);

        {
            var points = mapUI.graph.Points;
            var length = Math.Min(graphUI.samplesWidth, manager.units.Count);
            var start = data.index - length;
            var end = data.index;

            if (points.Length != length)
                points = new Vector2[length];
            Vector2 rmin = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 rmax = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            for (int i = start; i < end; i++)
            {
                rmin = Vector2.Min(rmin, manager.units[i].LonLat);
                rmax = Vector2.Max(rmax, manager.units[i].LonLat);
            }
            Rect r = Rect.MinMaxRect(rmin.x, rmin.y, rmax.x, rmax.y);
            {
                // Minor adjustment
                var center = r.center;
                r.size = Vector2.one * Math.Max(r.height, r.width) * 1.1f;
                r.center = center;
            }
            for (int i = start; i < end; i++)
            {
                var lonlat = manager.units[i].LonLat;
                points[i - start] = new Vector2(Mathf.InverseLerp(r.xMin, r.xMax, lonlat.x), 
                    Mathf.InverseLerp(r.yMin, r.yMax, lonlat.y));
            }
            mapUI.graph.Points = points;
            mapUI.graph.SetVerticesDirty();
            mapUI.pinpoint.anchorMin = mapUI.pinpoint.anchorMax = points[end - start - 1];
            mapUI.distText.text = HaversineFunction(rmin.y, rmin.x, rmax.y, rmax.y).ToString("0.0m");
        }
    }

    static float HaversineFunction(float lat1, float lon1, float lat2, float lon2)
    {  // generally used geo measurement function
        var R = 6378.137f; // Radius of earth in KM
        
        var dLat = lat2 * Mathf.Deg2Rad - lat1 * Mathf.Deg2Rad;
        var dLon = lon2 * Mathf.Deg2Rad - lon1 * Mathf.Deg2Rad;
        var a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
        Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.Deg2Rad) *
        Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var d = R * c;
        return d * 1000; // meters
    }

    private int lastCurrentIdx = -1;

    // Update is called once per frame
    void Update()
    {
        var current = GetComponent<DataManager>().current;
        if (current != lastCurrentIdx)
        {
            lastCurrentIdx = current;
            ValidateUI();
        }
        else if (table.SelectedRow - 1 != current && table.SelectedRow != -1)
        {
            GetComponent<DataManager>().current = table.SelectedRow - 1;
        }
    }

    public void LoadFile ()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("JSON", ".json"));
        StartCoroutine(GoLoadFile());
    }
    public void SaveFile()
    {
        FileBrowser.SetFilters(false, new FileBrowser.Filter("JSON", ".json"));
        StartCoroutine(GoSaveFile());
    }

    IEnumerator GoLoadFile()
    {
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Open");

        if (FileBrowser.Result != null)
            GetComponent<DataManager>().Import(FileBrowser.Result);
    }


    IEnumerator GoSaveFile()
    {
        yield return FileBrowser.WaitForSaveDialog(false, null, "Save File", "Save");

        if (FileBrowser.Result != null)
            GetComponent<DataManager>().Export(System.IO.Path.HasExtension(FileBrowser.Result) ? FileBrowser.Result : FileBrowser.Result + ".json");
    }
}
