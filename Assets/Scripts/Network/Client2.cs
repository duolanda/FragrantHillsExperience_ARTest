using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Client2 : MonoBehaviour
{
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;
    [SerializeField] private TMP_InputField inputField; 

    public delegate void SpotUpdateDelegate(List<int> idList);
    public static event SpotUpdateDelegate SpotUpdateEvent;
    
    private Socket tcpClient;
    private List<int> localIDs = new List<int>();

    public void ConnectToServer()
    {
        try
        {
            IPHostEntry host = Dns.GetHostEntry(serverIp);
            IPAddress ipAddress = host.AddressList[0];
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, serverPort);

            tcpClient = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            tcpClient.Connect(remoteEP);

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
            try
            {
                while (tcpClient.Connected)
                {
                    byte[] bytes = new byte[1024];
                    int bytesRec = tcpClient.Receive(bytes);
                    string data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    // Process received data...
                    ReceiveData(data);
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
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

    private void ReceiveData(string data)
    {
        data = data.Replace("<EOF>", "");
        localIDs = data.Split(',').Select(int.Parse).ToList();
        // UpdateSelectedScenicSpotIDList(localIDs);
        Debug.Log($"Received IDs: {string.Join(", ", localIDs)}");
    }
    
    public void UpdateLocalIDs(List<int> newIDs)
    {
        localIDs = newIDs;
        SendNumbers(localIDs);
    }
    
    private void SendNumbers(List<int> numbers)
    {
        if (tcpClient == null || !tcpClient.Connected) return;
        string msg = string.Join(",", numbers) + "<EOF>";
        byte[] byteData = Encoding.ASCII.GetBytes(msg);
        tcpClient.Send(byteData);
        Debug.Log($"Sent numbers: {string.Join(", ", numbers)}");
    }
    
    private void UpdateSelectedScenicSpotIDList(List<int> idList)
    {
        SpotUpdateEvent?.Invoke(idList); // 触发 AR 部分更新按钮状态和列表
    }

    private void OnApplicationQuit()
    {
        tcpClient?.Close();
    }
}
