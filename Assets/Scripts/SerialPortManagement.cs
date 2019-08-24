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

    public Text portConnectBtn;

    public Selectable[] DisableIfDisconnected;

    public Selectable[] DisableIfConnected;

    public int[] baudRates = new int[0];

    string[] ports;

    SerialPort serialPort = new SerialPort();

    public bool IsConnected => serialPort != null && serialPort.IsOpen;

    void UpdateStatus(string status)
    {
        portStatus.text = status;
    }

    public void UpdateUIStates()
    {
        var connected = IsConnected;
        foreach (var item in DisableIfConnected)
            item.interactable = !connected;
        foreach (var item in DisableIfDisconnected)
            item.interactable = connected;
        portConnectBtn.text = connected ? "DISCONNECT" : "CONNECT";
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
            serialPort = new SerialPort(ports[portChoices.value], baudRates[baudrateChoices.value], Parity.None, 8, StopBits.One);

            serialPort.DataReceived += SerialPort_DataReceived;
            serialPort.ErrorReceived += SerialPort_ErrorReceived;
            serialPort.PinChanged += SerialPort_PinChanged;

            try
            {
                serialPort.Open();
                UpdateStatus("<color=lime>Koneksi OK</color>");
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                UpdateUIStates();
            }
            catch (Exception e)
            {
                UpdateStatus("<color=red>Koneksi Gagal</color> ("+e.Message.Trim()+")");
                UpdateUIStates();
            }
        }
    }

    public void SendCommand(string cmd)
    {
        if (IsConnected)
        {
            serialPort.WriteLine(cmd);
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
            UpdateUIStates();
        }
    }

    void Update()
    {
        while (IsConnected && serialPort.BytesToRead > 0)
            SerialPort_DataReceived(null, null);
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
        UpdateUIStates();
    }

    void OnDisable()
    {
        if (IsConnected)
            Disconnect();
    }
}

