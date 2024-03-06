using UnityEngine;
using System.Collections.Generic;
using System.Net.Sockets;
using System.IO;
using TMPro;

public class Client : MonoBehaviour
{
    [SerializeField] private string serverIp = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;
    [SerializeField] private TMP_InputField inputField; 
    
    private TcpClient tcpClient;

    public void ConnectToServer()
    {
        try
        {
            tcpClient = new TcpClient(serverIp, serverPort);
            Debug.Log("Connected to server");
        }
        catch (SocketException e)
        {
            Debug.Log("SocketException: " + e.ToString());
        }
    }

    public void SendNumbers()
    {
        if (tcpClient == null || !tcpClient.Connected) return;

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

        // Send numbers to the server
        using (NetworkStream stream = tcpClient.GetStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(numbers.Count);
                foreach (int number in numbers)
                {
                    writer.Write(number);
                }
            }
        }

        Debug.Log($"Sent numbers: {string.Join(", ", numbers)}");
    }

    private void OnApplicationQuit()
    {
        tcpClient?.Close();
    }
}