using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkConnectionManager : MonoBehaviour
{
    [Header("Connection Settings")]
    [SerializeField] private string serverIP = "192.168.1.100";
    [SerializeField] private ushort serverPort = 7777;
    [SerializeField] private bool autoStartInEditor = true;
    [SerializeField] private bool autoStartInBuild = true;
    [SerializeField] private ConnectionMode connectionMode = ConnectionMode.Auto;

    private enum ConnectionMode
    {
        Auto,
        Host,
        Client,
        Server
    }

    private void Start()
    {
        ConfigureTransport();
        
        if (ShouldAutoStart())
        {
            StartConnection();
        }
    }

    private void ConfigureTransport()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is UnityTransport transport)
        {
            if (connectionMode == ConnectionMode.Client || (connectionMode == ConnectionMode.Auto && !Application.isEditor))
            {
                transport.ConnectionData.Address = serverIP;
                transport.ConnectionData.Port = serverPort;
                Debug.Log($"Configured transport for client connection to {serverIP}:{serverPort}");
            }
            else
            {
                transport.ConnectionData.ServerListenAddress = "0.0.0.0";
                transport.ConnectionData.Port = serverPort;
                Debug.Log($"Configured transport for host/server on port {serverPort}");
            }
        }
        else
        {
            Debug.LogError("UnityTransport not found on NetworkManager!");
        }
    }

    private bool ShouldAutoStart()
    {
        if (Application.isEditor)
        {
            return autoStartInEditor;
        }
        else
        {
            return autoStartInBuild;
        }
    }

    private void StartConnection()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("NetworkManager is already running!");
            return;
        }

        ConnectionMode mode = connectionMode;
        if (mode == ConnectionMode.Auto)
        {
            mode = Application.isEditor ? ConnectionMode.Host : ConnectionMode.Client;
        }

        switch (mode)
        {
            case ConnectionMode.Host:
                if (NetworkManager.Singleton.StartHost())
                {
                    Debug.Log("Started as Host");
                }
                else
                {
                    Debug.LogError("Failed to start as Host");
                }
                break;

            case ConnectionMode.Client:
                if (NetworkManager.Singleton.StartClient())
                {
                    Debug.Log($"Started as Client connecting to {serverIP}:{serverPort}");
                }
                else
                {
                    Debug.LogError("Failed to start as Client");
                }
                break;

            case ConnectionMode.Server:
                if (NetworkManager.Singleton.StartServer())
                {
                    Debug.Log("Started as Server");
                }
                else
                {
                    Debug.LogError("Failed to start as Server");
                }
                break;
        }
    }

    public void SetServerIP(string ip)
    {
        serverIP = ip;
        ConfigureTransport();
    }

    public void SetServerPort(ushort port)
    {
        serverPort = port;
        ConfigureTransport();
    }

    public void StartAsHost()
    {
        connectionMode = ConnectionMode.Host;
        ConfigureTransport();
        StartConnection();
    }

    public void StartAsClient()
    {
        connectionMode = ConnectionMode.Client;
        ConfigureTransport();
        StartConnection();
    }
}
