using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomNetworkBehaviour : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI chatText = null;
    [SerializeField] private TMP_InputField inputField = null;
    [SerializeField] private GameObject canvas = null;

    private static event Action<string> OnMessage;

    // client 连接时调用
    public override void OnStartAuthority()
    {
        canvas.SetActive(true);

        OnMessage += HandleNewMessage;
    }

    // client 退出时调用
    [ClientCallback]
    private void OnDestroy()
    {
        if(!hasAuthority) { return; }

        OnMessage -= HandleNewMessage;
    }

    // 有新消息时，更新文本框
    private void HandleNewMessage(string message)
    {
        Debug.Log("Text Updated");
        chatText.text += message;
    }

    // client 按按钮时发送消息
    [Client]
    public void Send()
    {
        if (string.IsNullOrWhiteSpace(inputField.text)) { return; }
        Debug.Log("Prepare to send message");
        CmdSendMessage(inputField.text);
        inputField.text = string.Empty;
    }

    [Command(requiresAuthority = false)]
    private void CmdSendMessage(string message)
    {
        // Validate message
        RpcHandleMessage($"[{connectionToClient.connectionId}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        OnMessage?.Invoke($"\n{message}");
        Debug.Log("Success send a message");
    }

}