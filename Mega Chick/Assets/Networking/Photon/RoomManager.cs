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
    [SerializeField] private bool verboseLogging = true;
    
    private string currentRoomCode;
    private string currentRoomName;
    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    
    // Event for room list updates
    public System.Action<List<RoomInfo>> OnRoomListUpdated;
    
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
    /// Generate a friendly room name (different from room code).
    /// </summary>
    public string GenerateRoomName()
    {
        string[] adjectives = { "Epic", "Mega", "Super", "Ultra", "Awesome", "Cool", "Fast", "Wild", "Crazy", "Fun" };
        string[] nouns = { "Arena", "Battle", "Match", "Game", "Room", "Lobby", "Zone", "Stage", "Ring", "Field" };
        
        string adjective = adjectives[Random.Range(0, adjectives.Length)];
        string noun = nouns[Random.Range(0, nouns.Length)];
        int number = Random.Range(1, 100);
        
        return $"{adjective} {noun} {number}";
    }
    
    /// <summary>
    /// Create a new room with generated code.
    /// </summary>
    public void CreateRoom()
    {
        LogState("🚀 [ACTION] CreateRoom() called");
        
        bool isConnected = PhotonNetwork.IsConnectedAndReady;
        LogState($"🔌 [CHECK] IsConnectedAndReady: {isConnected}");
        
        if (!isConnected)
        {
            LogState("❌ [ERROR] Not connected! Connect first.");
            return;
        }
        
        bool inRoom = PhotonNetwork.InRoom;
        LogState($"🏠 [CHECK] Already in room: {inRoom}");
        
        if (inRoom)
        {
            LogState("⚠️ [WARN] Already in a room! Leaving first...");
            PhotonNetwork.LeaveRoom();
            // Wait for OnLeftRoom callback before creating new room
            return;
        }
        
        currentRoomCode = GenerateRoomCode();
        currentRoomName = GenerateRoomName();
        LogState($"🔤 [GENERATE] Room code: {currentRoomCode}, Room name: {currentRoomName}");
        
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = networkConfig != null ? (byte)networkConfig.maxPlayersPerRoom : (byte)8,
            IsVisible = networkConfig != null ? networkConfig.isVisible : true,
            IsOpen = networkConfig != null ? networkConfig.isOpen : true
        };
        
        LogState($"⚙️ [OPTIONS] MaxPlayers: {roomOptions.MaxPlayers} | Visible: {roomOptions.IsVisible} | Open: {roomOptions.IsOpen}");
        LogState($"🚀 [PHOTON] Calling PhotonNetwork.CreateRoom({currentRoomCode})...");
        
        bool success = PhotonNetwork.CreateRoom(currentRoomCode, roomOptions);
        LogState($"✅ [PHOTON] CreateRoom returned: {success}");
    }
    
    /// <summary>
    /// Join a room by code.
    /// </summary>
    public void JoinRoom(string roomCode)
    {
        LogState("🚀 [ACTION] JoinRoom() called");
        LogState($"📝 [INPUT] Room code: '{roomCode}'");
        
        if (string.IsNullOrEmpty(roomCode))
        {
            LogState("❌ [ERROR] Room code is empty!");
            return;
        }
        
        bool isConnected = PhotonNetwork.IsConnectedAndReady;
        LogState($"🔌 [CHECK] IsConnectedAndReady: {isConnected}");
        
        if (!isConnected)
        {
            LogState("❌ [ERROR] Not connected! Connect first.");
            return;
        }
        
        bool inRoom = PhotonNetwork.InRoom;
        LogState($"🏠 [CHECK] Already in room: {inRoom}");
        
        if (inRoom)
        {
            LogState("⚠️ [WARN] Already in a room! Leaving first...");
            PhotonNetwork.LeaveRoom();
            // Wait for OnLeftRoom callback
            return;
        }
        
        LogState($"🚀 [PHOTON] Calling PhotonNetwork.JoinRoom({roomCode})...");
        bool success = PhotonNetwork.JoinRoom(roomCode);
        LogState($"✅ [PHOTON] JoinRoom returned: {success}");
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
    public string GetCurrentRoomName() => currentRoomName;
    public int GetPlayerCount() => PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;
    public int GetMaxPlayers() => PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.MaxPlayers : 0;
    public List<Player> GetPlayerList() => new List<Player>(PhotonNetwork.PlayerList);
    
    // Photon Callbacks
    public override void OnCreatedRoom()
    {
        string roomName = PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.Name : "UNKNOWN";
        LogState($"🎉 [CALLBACK] OnCreatedRoom fired!");
        LogState($"🏠 [ROOM] Room created: {roomName}");
        LogState($"👑 [ROOM] Is Master Client: {PhotonNetwork.IsMasterClient}");
        LogState($"👥 [ROOM] Player count: {GetPlayerCount()}/{GetMaxPlayers()}");
        
        // NOTE: OnCreatedRoom fires BEFORE OnJoinedRoom
        // The creator is automatically in the room, OnJoinedRoom will fire next
        LogState("⏳ [WAIT] Waiting for OnJoinedRoom callback (creator auto-joins)...");
    }
    
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        LogState($"❌ [CALLBACK] OnCreateRoomFailed fired!");
        LogState($"❌ [ERROR] Return Code: {returnCode}");
        LogState($"❌ [ERROR] Message: {message}");
        GameEventBus.FireCreateRoomFailed(message);
    }
    
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            LogState("❌ [ERROR] CurrentRoom is NULL in OnJoinedRoom!");
            return;
        }
        
        currentRoomCode = PhotonNetwork.CurrentRoom.Name;
        // If room name not set, generate one
        if (string.IsNullOrEmpty(currentRoomName))
        {
            currentRoomName = GenerateRoomName();
        }
        int playerCount = GetPlayerCount();
        int maxPlayers = GetMaxPlayers();
        bool isMaster = PhotonNetwork.IsMasterClient;
        
        LogState($"🎉 [CALLBACK] OnJoinedRoom fired!");
        LogState($"🏠 [ROOM] Room Code: {currentRoomCode}");
        LogState($"👥 [ROOM] Players: {playerCount}/{maxPlayers}");
        LogState($"👑 [ROOM] Is Master Client: {(isMaster ? "YES ✅" : "NO")}");
        LogState($"✅ [ROOM] Successfully joined room!");
        
        GameEventBus.FireJoinedRoom();
    }
    
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LogState($"❌ [CALLBACK] OnJoinRoomFailed fired!");
        LogState($"❌ [ERROR] Return Code: {returnCode}");
        LogState($"❌ [ERROR] Message: {message}");
        
        // Common error codes:
        // 32760 = Room not found
        // 32758 = Room full
        // 32757 = Room closed
        
        string errorMsg = "";
        switch (returnCode)
        {
            case 32760:
                errorMsg = "Room not found! Check the room code.";
                break;
            case 32758:
                errorMsg = "Room is full!";
                break;
            case 32757:
                errorMsg = "Room is closed!";
                break;
            default:
                errorMsg = $"Failed to join: {message}";
                break;
        }
        
        LogState($"❌ [ERROR] {errorMsg}");
        GameEventBus.FireJoinRoomFailed(errorMsg);
    }
    
    public override void OnLeftRoom()
    {
        LogState("🎉 [CALLBACK] OnLeftRoom fired!");
        LogState("🏠 [ROOM] Left room");
        currentRoomCode = null;
        currentRoomName = null;
        GameEventBus.FireLeftRoom();
    }
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        LogState($"🎉 [CALLBACK] OnPlayerEnteredRoom fired!");
        LogState($"👤 [PLAYER] Player entered: {newPlayer.ActorNumber} ({newPlayer.NickName})");
        GameEventBus.FirePlayerEnteredRoom(newPlayer.ActorNumber.ToString());
    }
    
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        LogState($"🎉 [CALLBACK] OnPlayerLeftRoom fired!");
        LogState($"👤 [PLAYER] Player left: {otherPlayer.ActorNumber} ({otherPlayer.NickName})");
        GameEventBus.FirePlayerLeftRoom(otherPlayer.ActorNumber.ToString());
    }
    
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        LogState($"🎉 [CALLBACK] OnMasterClientSwitched fired!");
        LogState($"👑 [MASTER] Master client switched to: {newMasterClient.ActorNumber} ({newMasterClient.NickName})");
    }
    
    /// <summary>
    /// Get cached room list.
    /// </summary>
    public List<RoomInfo> GetRoomList()
    {
        return new List<RoomInfo>(cachedRoomList);
    }
    
    /// <summary>
    /// Photon callback when room list updates.
    /// </summary>
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        LogState($"🎉 [CALLBACK] OnRoomListUpdate fired! {roomList.Count} rooms");
        cachedRoomList = roomList;
        OnRoomListUpdated?.Invoke(roomList);
    }
    
    private void LogState(string message)
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
