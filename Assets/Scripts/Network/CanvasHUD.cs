using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class CanvasHUD : MonoBehaviour
{
    public GameObject PanelStart;
    public GameObject PanelStop;
    public GameObject Chat;
    
    public Button buttonHost, buttonServer, buttonClient, buttonStop;

    public TMP_InputField inputFieldAddress;

    public TextMeshProUGUI serverText;
    public TextMeshProUGUI clientText;

    
    private void Start()
    {
        //如果在启动场景前手动修改了 network manager 地址，更新 canvas 文本
        if (NetworkManager.singleton.networkAddress != "localhost") { inputFieldAddress.text = NetworkManager.singleton.networkAddress; }
        
        //在输入框值变化的时候调用方法
        inputFieldAddress.onValueChanged.AddListener(delegate { ValueChangeCheck(); });
        
        //确保 inspector 中的 button 都填上了
        buttonHost.onClick.AddListener(ButtonHost);
        buttonServer.onClick.AddListener(ButtonServer);
        buttonClient.onClick.AddListener(ButtonClient);
        buttonStop.onClick.AddListener(ButtonStop);

        //更新 canvas，每次修改的时候都需要手动调用
        SetupCanvas();
    }

    // 输入框内容变化时调用
    public void ValueChangeCheck()
    {
        NetworkManager.singleton.networkAddress = inputFieldAddress.text;
    }

    public void ButtonHost()
    {
        NetworkManager.singleton.StartHost();
        SetupCanvas();
    }

    public void ButtonServer()
    {
        NetworkManager.singleton.StartServer();
        SetupCanvas();
    }

    public void ButtonClient()
    {
        // NetworkManager.singleton.networkAddress = "192.168.1.26"; //路由器绑定的电脑地址
        NetworkManager.singleton.StartClient();
        SetupCanvas();
    }

    public void ButtonStop()
    {
        // stop host if host mode
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        // stop client if client-only
        else if (NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopClient();
        }
        // stop server if server-only
        else if (NetworkServer.active)
        {
            NetworkManager.singleton.StopServer();
        }

        SetupCanvas();
    }

    public void SetupCanvas()
    {
        // 处理大部分可能会变化的 UI

        if (!NetworkClient.isConnected && !NetworkServer.active)
        {
            if (NetworkClient.active)
            {
                PanelStart.SetActive(false);
                Chat.gameObject.SetActive(true);
                PanelStop.SetActive(true);
                clientText.text = "Connecting to " + NetworkManager.singleton.networkAddress + "..";
            }
            else
            {
                PanelStart.SetActive(true);
                Chat.gameObject.SetActive(false);
                PanelStop.SetActive(false);
            }
        }
        else
        {
            PanelStart.SetActive(false);
            Chat.gameObject.SetActive(true);
            PanelStop.SetActive(true);

            // server / client status message
            if (NetworkServer.active)
            {
                serverText.text = "Server: active. Transport: " + Transport.active;
                // Note, older mirror versions use: Transport.activeTransport
            }
            if (NetworkClient.isConnected)
            {
                clientText.text = "Client: address=" + NetworkManager.singleton.networkAddress;
            }
        }
    }
}
