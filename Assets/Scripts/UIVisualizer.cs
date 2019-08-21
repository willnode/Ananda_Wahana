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
        public UILineRenderer lonlat;
        public int samplesWidth = 10;
    }

    public TextUI textUI = new TextUI();

    public GraphUI graphUI = new GraphUI();

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
            if (points.Length != graphUI.samplesWidth)
                points = new Vector2[graphUI.samplesWidth];
            for (int i = 0; i < graphUI.samplesWidth; i++)
            {
                var ii = data.index - graphUI.samplesWidth + i;
                points[i] = new Vector2((float)i / (graphUI.samplesWidth - 1),
                    ii < 0 ? 0 :
                   Mathf.Repeat(fetchFunc(manager.units[ii]), 360) / 360); 
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
