using System;
using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using TMPro;

public class Client : MonoBehaviour
{
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;
    [SerializeField] private TMP_InputField inputField; 
    
    private TcpClient tcpClient;

    public delegate void SpotUpdateDelegate();
    public static event SpotUpdateDelegate SpotUpdateEvent;
    
    private List<int> localIDs = new List<int>();

    public void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient(serverIp, serverPort);
            Debug.Log("Connected to server");
            StartListening();
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e.ToString());
        }
    }
    
    private void StartListening()
    {
        Thread listeningThread = new Thread(() =>
        {
            using (NetworkStream stream = tcpClient.GetStream())
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    try
                    {
                        while (tcpClient.Connected)
                        {
                            byte messageType = reader.ReadByte();
                            switch (messageType)
                            {
                                case 1: // 同步列表
                                    int count = reader.ReadInt32();
                                    localIDs.Clear();
                                    for (int i = 0; i < count; i++)
                                    {
                                        localIDs.Add(reader.ReadInt32());
                                    }

                                    // UpdateSelectedScenicSpotIDList(localIDs);
                                    Debug.Log($"Received IDs: {string.Join(", ", localIDs)}");
                                    break;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error: {e.Message}");
                    }
                }
            }
        });
        listeningThread.Start();
    }

    public void TestSend()
    {
        // Parse numbers from input field
        string[] numbersStr = inputField.text.Split(',');
        List<int> numbers = new List<int>();
        foreach (string numberStr in numbersStr)
        {
            if (int.TryParse(numberStr.Trim(), out int number))
            {
                numbers.Add(number);
            }
        }

        SendNumbers(numbers);
    }

    public void UpdateLocalIDs(List<int> newIDs)
    {
        localIDs = newIDs;
        SendNumbers(localIDs);
    }

    private void SendNumbers(List<int> numbers)
    {
        if (tcpClient == null || !tcpClient.Connected) return;
        // Send numbers to the server
        using (NetworkStream stream = tcpClient.GetStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((byte)0); // Message type: Update request
                writer.Write(numbers.Count);
                foreach (int number in numbers)
                {
                    writer.Write(number);
                }
            }
        }
        Debug.Log($"Sent numbers: {string.Join(", ", numbers)}");
    }
    
    private void UpdateSelectedScenicSpotIDList(List<int> idList)
    {
        SpotUpdateEvent?.Invoke(); // 触发 AR 部分更新按钮状态和列表
    }

    private void OnApplicationQuit()
    {
        tcpClient?.Close();
    }
}