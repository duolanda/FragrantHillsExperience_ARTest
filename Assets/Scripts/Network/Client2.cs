using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class Client2 : MonoBehaviour
{
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;
    private Socket tcpClient;
    private List<int> localIDs = new List<int>();

    private void Start()
    {
        ConnectToServer();
    }

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
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error: {e.Message}");
            }
        });
        listeningThread.Start();
    }

    private void SendNumbers(List<int> numbers)
    {
        if (tcpClient == null || !tcpClient.Connected) return;
        string msg = string.Join(",", numbers) + "<EOF>";
        byte[] byteData = Encoding.ASCII.GetBytes(msg);
        tcpClient.Send(byteData);
        Debug.Log($"Sent numbers: {string.Join(", ", numbers)}");
    }

    private void OnApplicationQuit()
    {
        tcpClient?.Close();
    }
}
