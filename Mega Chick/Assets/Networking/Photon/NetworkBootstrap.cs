#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Handles Photon connection lifecycle.
/// Why separate? Connection logic isolated from room logic.
/// Singleton pattern - one instance manages connection.
/// </summary>
public class NetworkBootstrap : MonoBehaviourPunCallbacks
{
    public static NetworkBootstrap Instance { get; private set; }
    
    [Header("Config")]
    [SerializeField] private NetworkConfig networkConfig;
    
    [Header("Debug")]
    [SerializeField] private bool logConnectionEvents = true;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Apply network config
        if (networkConfig != null)
        {
            PhotonNetwork.SendRate = networkConfig.sendRate;
            PhotonNetwork.SerializationRate = networkConfig.serializationRate;
        }
    }
    
    /// <summary>
    /// Connect to Photon servers.
    /// Call this from UI button or game start.
    /// </summary>
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            Log("Already connected!");
            return;
        }
        
        Log("Connecting to Photon...");
        PhotonNetwork.ConnectUsingSettings();
    }
    
    /// <summary>
    /// Disconnect from Photon.
    /// </summary>
    public void Disconnect()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
    }
    
    public bool IsConnected() => PhotonNetwork.IsConnected;
    public bool IsInLobby() => PhotonNetwork.InLobby;
    public bool IsInRoom() => PhotonNetwork.InRoom;
    
    // Photon Callbacks
    public override void OnConnectedToMaster()
    {
        Log("Connected to Photon Master Server");
        GameEventBus.FireConnectedToMaster();
        
        // Auto-join lobby if configured
        if (PhotonNetwork.NetworkingClient.EnableLobbyStatistics)
        {
            PhotonNetwork.JoinLobby();
        }
    }
    
    public override void OnDisconnected(DisconnectCause cause)
    {
        Log($"Disconnected: {cause}");
        GameEventBus.FireDisconnected();
    }
    
    public override void OnJoinedLobby()
    {
        Log("Joined Lobby");
        GameEventBus.FireJoinedLobby();
    }
    
    public override void OnLeftLobby()
    {
        Log("Left Lobby");
        GameEventBus.FireLeftLobby();
    }
    
    private void Log(string message)
    {
        if (logConnectionEvents)
        {
            Debug.Log($"[NetworkBootstrap] {message}");
        }
    }
}
#else
using UnityEngine;

/// <summary>
/// Photon PUN2 is not installed. Install from Unity Asset Store.
/// </summary>
public class NetworkBootstrap : MonoBehaviour
{
    public static NetworkBootstrap Instance { get; private set; }
    
    [Header("Config")]
    [SerializeField] private NetworkConfig networkConfig;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        Debug.LogWarning("[NetworkBootstrap] Photon PUN2 is not installed! Please install from Unity Asset Store.");
    }
    
    public void Connect() => Debug.LogWarning("Photon not installed!");
    public void Disconnect() { }
    public bool IsConnected() => false;
    public bool IsInLobby() => false;
    public bool IsInRoom() => false;
}
#endif
