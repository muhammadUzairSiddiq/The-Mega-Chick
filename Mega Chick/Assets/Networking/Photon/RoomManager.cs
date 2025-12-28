#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

/// <summary>
/// Manages room creation, joining, and room code generation.
/// Why separate from NetworkBootstrap? Single responsibility - rooms vs connection.
/// </summary>
public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance { get; private set; }
    
    [Header("Config")]
    [SerializeField] private NetworkConfig networkConfig;
    
    [Header("Debug")]
    [SerializeField] private bool logRoomEvents = true;
    
    private string currentRoomCode;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// Generate a random room code.
    /// Why? Simple room code system for easy joining.
    /// </summary>
    public string GenerateRoomCode()
    {
        int length = networkConfig != null ? networkConfig.roomCodeLength : 6;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code = "";
        
        for (int i = 0; i < length; i++)
        {
            code += chars[Random.Range(0, chars.Length)];
        }
        
        return code;
    }
    
    /// <summary>
    /// Create a new room with generated code.
    /// </summary>
    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Log("Not connected! Connect first.");
            return;
        }
        
        if (PhotonNetwork.InRoom)
        {
            Log("Already in a room!");
            return;
        }
        
        currentRoomCode = GenerateRoomCode();
        Log($"Creating room: {currentRoomCode}");
        
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = networkConfig != null ? (byte)networkConfig.maxPlayersPerRoom : (byte)8,
            IsVisible = networkConfig != null ? networkConfig.isVisible : true,
            IsOpen = networkConfig != null ? networkConfig.isOpen : true
        };
        
        PhotonNetwork.CreateRoom(currentRoomCode, roomOptions);
    }
    
    /// <summary>
    /// Join a room by code.
    /// </summary>
    public void JoinRoom(string roomCode)
    {
        if (string.IsNullOrEmpty(roomCode))
        {
            Log("Room code is empty!");
            return;
        }
        
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Log("Not connected! Connect first.");
            return;
        }
        
        if (PhotonNetwork.InRoom)
        {
            Log("Already in a room! Leave first.");
            return;
        }
        
        Log($"Joining room: {roomCode}");
        PhotonNetwork.JoinRoom(roomCode);
    }
    
    /// <summary>
    /// Leave current room.
    /// </summary>
    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    
    public bool IsMasterClient() => PhotonNetwork.IsMasterClient;
    public string GetCurrentRoomCode() => currentRoomCode;
    public int GetPlayerCount() => PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
    public int GetMaxPlayers() => PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.MaxPlayers : 0;
    public List<Player> GetPlayerList() => new List<Player>(PhotonNetwork.PlayerList);
    
    // Photon Callbacks
    public override void OnCreatedRoom()
    {
        Log($"Room created: {PhotonNetwork.CurrentRoom.Name}");
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Log($"Failed to create room: {message}");
    }
    
    public override void OnJoinedRoom()
    {
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        Log($"Joined room: {currentRoomCode} ({GetPlayerCount()}/{GetMaxPlayers()} players)");
        GameEventBus.FireJoinedRoom();
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Log($"Failed to join room: {message}");
    }
    
    public override void OnLeftRoom()
    {
        Log("Left room");
        currentRoomCode = null;
        GameEventBus.FireLeftRoom();
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Log($"Player entered: {newPlayer.ActorNumber}");
        GameEventBus.FirePlayerEnteredRoom(newPlayer.ActorNumber.ToString());
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Log($"Player left: {otherPlayer.ActorNumber}");
        GameEventBus.FirePlayerLeftRoom(otherPlayer.ActorNumber.ToString());
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Log($"Master client switched to: {newMasterClient.ActorNumber}");
    }
    
    private void Log(string message)
    {
        if (logRoomEvents)
        {
            Debug.Log($"[RoomManager] {message}");
        }
    }
}
#else
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Photon PUN2 is not installed. Install from Unity Asset Store.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    
    [Header("Config")]
    [SerializeField] private NetworkConfig networkConfig;
    
    private string currentRoomCode;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        Debug.LogWarning("[RoomManager] Photon PUN2 is not installed!");
    }
    
    public string GenerateRoomCode() => "XXXXXX";
    public void CreateRoom() => Debug.LogWarning("Photon not installed!");
    public void JoinRoom(string code) => Debug.LogWarning("Photon not installed!");
    public void LeaveRoom() { }
    public bool IsMasterClient() => false;
    public string GetCurrentRoomCode() => currentRoomCode;
    public int GetPlayerCount() => 0;
    public int GetMaxPlayers() => 8;
    public List<object> GetPlayerList() => new List<object>();
}
#endif
