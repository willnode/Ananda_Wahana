using System;
using System.Linq;
using System.IO.Ports;
using UnityEngine;
using UnityEngine.UI;

public class SerialPortManagement : MonoBehaviour
{
    public Dropdown portChoices;

    public Dropdown baudrateChoices;

    public Text portStatus;

    public int[] baudRates = new int[0];

    string[] ports;

    SerialPort serialPort = new SerialPort();

    public bool IsConnected => serialPort != null && serialPort.IsOpen;

    void UpdateStatus(string status)
    {
        portStatus.text = status;
    }

    public void ReloadPortLists()
    {
        ports = SerialPort.GetPortNames();
        portChoices.ClearOptions();
        portChoices.options.AddRange(ports.Select(x => new Dropdown.OptionData(x)));
        if (!IsConnected)
            UpdateStatus(ports.Length + " port ditemukan");
    }

    public void ReloadBaudRates()
    {
        baudrateChoices.ClearOptions();
        baudrateChoices.options.AddRange(baudRates.Select(x => new Dropdown.OptionData(x.ToString())));
    }

    public void ConnectOrDisconnect()
    {
        if (IsConnected) Disconnect();
        else Connect();
    }

    public void Connect()
    {
        if (portChoices.value >= 0 && baudrateChoices.value >= 0 && !IsConnected)
        {
            serialPort = new SerialPort(ports[portChoices.value], baudRates[baudrateChoices.value]);
            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
            serialPort.PinChanged += SerialPort_PinChanged;
            serialPort.Open();
            UpdateStatus("<color=green>Koneksi OK</color>");
        }
    }

    public void SendCommand(string cmd)
    {
        if (IsConnected)
        {
            serialPort.Write(cmd);
        }
    }

    private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
    {
        UpdateStatus("Pin Ganti (" + e.EventType + ")</color>");
    }

    private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
    {
        UpdateStatus("<color=red>Koneksi Error (" + e.EventType + ")</color>");
    }

    public void Disconnect()
    {
        if (IsConnected)
        {
            serialPort.Close();
            serialPort.Dispose();
            if (portStatus)
                UpdateStatus("Koneksi Diputus");
        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        var manager = GetComponent<DataManager>();
        var index = manager.NextIndex;
        var unit = DroneUnitSerializer.Parse(
            index + "," + 
            DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "," + serialPort.ReadLine()
            );

        manager.units.Add(unit);
        manager.current++;
    }

    void OnEnable()
    {
        ReloadPortLists();
        ReloadBaudRates();
    }

    void OnDisable()
    {
        if (IsConnected)
            Disconnect();
    }
}

