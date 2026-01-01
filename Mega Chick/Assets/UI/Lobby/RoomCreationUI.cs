#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI for creating rooms and showing room list.
/// Simplified - no room code join, just create and list.
/// </summary>
public class RoomCreationUI : MonoBehaviour
{
    [Header("Create Room UI")]
    [SerializeField] private GameObject createRoomPanel;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("Room List UI")]
    [SerializeField] private GameObject roomListPanel;
    [SerializeField] private Transform roomListParent;
    [SerializeField] private GameObject roomEntryPrefab;
    [SerializeField] private Button refreshButton;
    
    [Header("Debug")]
    [SerializeField] private bool logUIEvents = true;
    [SerializeField] private bool verboseLogging = true;
    
    private enum UIState { MainMenu, CreatingRoom, InRoom }
    private UIState currentState = UIState.MainMenu;
    private List<GameObject> roomEntries = new List<GameObject>();
    
    private void Start()
    {
        LogState("🔵 [INIT] RoomCreationUI starting...");
        
        // Wire buttons
        if (createRoomButton != null)
        {
            createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            LogState("✅ [WIRE] CreateRoomButton → OnCreateRoomClicked");
        }
        
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshRoomList);
            LogState("✅ [WIRE] RefreshButton → RefreshRoomList");
        }
        
        // Subscribe to events
        GameEventBus.OnJoinedRoom += OnJoinedRoom;
        GameEventBus.OnJoinedLobby += OnJoinedLobby;
        GameEventBus.OnConnectedToMaster += OnConnectedToMaster;
        GameEventBus.OnCreateRoomFailed += OnCreateRoomFailed;
        
        // Subscribe to room list updates
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomListUpdated += OnRoomListUpdated;
        }
        
        LogState("✅ [EVENT] Subscribed to network events");
        
        // Initialize UI
        SetState(UIState.MainMenu);
        ShowCreateRoomPanel();
        
        LogState("✅ [INIT] RoomCreationUI initialized");
    }
    
    private void OnDestroy()
    {
        GameEventBus.OnJoinedRoom -= OnJoinedRoom;
        GameEventBus.OnJoinedLobby -= OnJoinedLobby;
        GameEventBus.OnConnectedToMaster -= OnConnectedToMaster;
        GameEventBus.OnCreateRoomFailed -= OnCreateRoomFailed;
        
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomListUpdated -= OnRoomListUpdated;
        }
    }
    
    /// <summary>
    /// Show create room panel.
    /// </summary>
    public void ShowCreateRoomPanel()
    {
        LogState("🔄 [UI] Showing CreateRoomPanel");
        
        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(true);
            LogState("✅ [UI] CreateRoomPanel activated");
        }
        
        if (roomListPanel != null)
        {
            roomListPanel.SetActive(false);
            LogState("✅ [UI] RoomListPanel deactivated");
        }
        
        UpdateStatus("Ready to create room");
        SetState(UIState.MainMenu);
    }
    
    /// <summary>
    /// Show room list panel.
    /// </summary>
    public void ShowRoomListPanel()
    {
        LogState("🔄 [UI] Showing RoomListPanel");
        
        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(false);
        }
        
        if (roomListPanel != null)
        {
            roomListPanel.SetActive(true);
        }
        
        RefreshRoomList();
    }
    
    /// <summary>
    /// Create room button clicked.
    /// </summary>
    private void OnCreateRoomClicked()
    {
        LogState("🖱️ [CLICK] Create Room button clicked");
        
        if (RoomManager.Instance == null)
        {
            LogState("❌ [ERROR] RoomManager.Instance is NULL!");
            SetState(UIState.MainMenu);
            UpdateStatus("RoomManager not found!");
            return;
        }
        
        bool isConnected = NetworkBootstrap.Instance != null && NetworkBootstrap.Instance.IsConnected();
        LogState($"🔌 [NETWORK] Connection status: {(isConnected ? "CONNECTED ✅" : "NOT CONNECTED ❌")}");
        
        if (!isConnected)
        {
            LogState("❌ [ERROR] Not connected to Photon!");
            UpdateStatus("Not connected to Photon!");
            return;
        }
        
        LogState("🚀 [ACTION] Creating room...");
        SetState(UIState.CreatingRoom);
        UpdateStatus("Creating room...");
        RoomManager.Instance.CreateRoom();
        LogState("✅ [ACTION] CreateRoom() called");
    }
    
    /// <summary>
    /// Join room from room list.
    /// </summary>
    public void JoinRoom(string roomName)
    {
        LogState($"🖱️ [CLICK] Join room: {roomName}");
        
        if (RoomManager.Instance == null)
        {
            LogState("❌ [ERROR] RoomManager.Instance is NULL!");
            UpdateStatus("RoomManager not found!");
            return;
        }
        
        LogState($"🚀 [ACTION] Joining room '{roomName}'...");
        UpdateStatus($"Joining room {roomName}...");
        RoomManager.Instance.JoinRoom(roomName);
    }
    
    /// <summary>
    /// Refresh room list.
    /// </summary>
    public void RefreshRoomList()
    {
        LogState("🔄 [ACTION] Refreshing room list...");
        
        if (!PhotonNetwork.InLobby)
        {
            LogState("⚠️ [WARN] Not in lobby! Cannot refresh room list.");
            UpdateStatus("Not in lobby!");
            return;
        }
        
        // Clear existing entries
        ClearRoomList();
        
        // Get room list from RoomManager
        // Note: Room list is automatically updated by Photon when in lobby via OnRoomListUpdate callback
        if (RoomManager.Instance != null)
        {
            List<RoomInfo> rooms = RoomManager.Instance.GetRoomList();
            OnRoomListUpdated(rooms);
        }
        else
        {
            LogState("⚠️ [WARN] RoomManager.Instance is NULL! Cannot get room list.");
            UpdateStatus("RoomManager not found!");
        }
        
        UpdateStatus("Refreshing room list...");
    }
    
    /// <summary>
    /// Called when room list is updated.
    /// Format: Room No | Room Name | Players (1/2) | Join Button
    /// </summary>
    private void OnRoomListUpdated(List<RoomInfo> roomList)
    {
        LogState($"🎉 [EVENT] Room list updated! {roomList.Count} rooms available");
        
        ClearRoomList();
        
        if (roomListParent == null || roomEntryPrefab == null)
        {
            LogState("⚠️ [WARN] Room list parent or prefab not assigned!");
            return;
        }
        
        // Get current room code (if in a room)
        string currentRoomCode = "";
        if (PhotonNetwork.InRoom && RoomManager.Instance != null)
        {
            currentRoomCode = RoomManager.Instance.GetCurrentRoomCode();
        }
        
        // Create room entry for each room
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList) continue; // Skip removed rooms
            
            GameObject entry = Instantiate(roomEntryPrefab, roomListParent);
            roomEntries.Add(entry);
            
            // Setup room entry UI: Room No | Room Name | Players (1/2) | Join Button
            TextMeshProUGUI[] allTexts = entry.GetComponentsInChildren<TextMeshProUGUI>();
            Button[] allButtons = entry.GetComponentsInChildren<Button>();
            
            // Format: Room No | Room Name | Players (1/2)
            string roomNo = room.Name;
            string roomName = room.Name; // We'll use room code as name for now
            if (allTexts.Length > 0)
            {
                allTexts[0].text = $"{roomNo} | {roomName} | Players {room.PlayerCount}/{room.MaxPlayers}";
            }
            
            // Join button - only interactable if not the room owner
            bool isRoomOwner = (currentRoomCode == room.Name);
            if (allButtons.Length > 0)
            {
                Button joinButton = allButtons[0];
                joinButton.interactable = !isRoomOwner;
                
                // Change button color
                Image buttonImage = joinButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isRoomOwner ? Color.grey : Color.green;
                }
                
                // Wire join button
                string roomNameToJoin = room.Name;
                joinButton.onClick.RemoveAllListeners();
                joinButton.onClick.AddListener(() => JoinRoom(roomNameToJoin));
                LogState($"✅ [WIRE] Join button wired for room: {roomNameToJoin} (Owner: {isRoomOwner})");
            }
        }
        
        LogState($"✅ [UI] Created {roomEntries.Count} room entries");
    }
    
    /// <summary>
    /// Clear room list UI.
    /// </summary>
    private void ClearRoomList()
    {
        foreach (GameObject entry in roomEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        roomEntries.Clear();
    }
    
    /// <summary>
    /// Called when successfully joined a room.
    /// </summary>
    private void OnJoinedRoom()
    {
        LogState("🎉 [EVENT] OnJoinedRoom fired!");
        
        if (RoomManager.Instance != null)
        {
            string roomCode = RoomManager.Instance.GetCurrentRoomCode();
            int playerCount = RoomManager.Instance.GetPlayerCount();
            int maxPlayers = RoomManager.Instance.GetMaxPlayers();
            bool isMaster = RoomManager.Instance.IsMasterClient();
            
            LogState($"🏠 [ROOM] Room Code: {roomCode}");
            LogState($"👥 [ROOM] Players: {playerCount}/{maxPlayers}");
            LogState($"👑 [ROOM] Is Master Client: {(isMaster ? "YES ✅" : "NO")}");
            
            UpdateStatus($"Joined room: {roomCode} ({playerCount}/{maxPlayers} players)");
        }
        
        // Hide room creation UI - move to character selection
        if (createRoomPanel != null)
        {
            createRoomPanel.SetActive(false);
            LogState("✅ [UI] CreateRoomPanel deactivated");
        }
        else
        {
            LogState("⚠️ [WARN] createRoomPanel is NULL!");
        }
        
        if (roomListPanel != null)
        {
            roomListPanel.SetActive(false);
            LogState("✅ [UI] RoomListPanel deactivated");
        }
        
        // Deactivate entire RoomCreationUI GameObject
        gameObject.SetActive(false);
        LogState("✅ [UI] RoomCreationUI GameObject deactivated");
        
        // FORCE SHOW CHARACTER SELECTION - Use coroutine to ensure it happens after frame
        StartCoroutine(DelayedShowCharacterSelection());
        
        SetState(UIState.InRoom);
        LogState("✅ [STATE] Now in room - character selection should be available");
    }
    
    /// <summary>
    /// Called when joined lobby (ready to create/join rooms).
    /// </summary>
    private void OnJoinedLobby()
    {
        LogState("🎉 [EVENT] OnJoinedLobby fired!");
        LogState("✅ [STATE] Ready to create/join rooms");
        SetState(UIState.MainMenu);
        ShowCreateRoomPanel();
        UpdateStatus("Connected! Ready to create or join room.");
    }
    
    /// <summary>
    /// Called when connected to Photon master server.
    /// </summary>
    private void OnConnectedToMaster()
    {
        LogState("🎉 [EVENT] OnConnectedToMaster fired!");
        LogState("✅ [NETWORK] Connected to Photon Master Server");
    }
    
    /// <summary>
    /// Called when create room fails.
    /// </summary>
    private void OnCreateRoomFailed(string errorMessage)
    {
        LogState($"❌ [EVENT] OnCreateRoomFailed fired!");
        LogState($"❌ [ERROR] {errorMessage}");
        SetState(UIState.MainMenu);
        UpdateStatus($"❌ Failed to create room: {errorMessage}");
    }
    
    /// <summary>
    /// Update status text.
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        if (verboseLogging)
        {
            LogState($"[STATUS] {message}");
        }
    }
    
    /// <summary>
    /// Set current UI state and log transition.
    /// </summary>
    private void SetState(UIState newState)
    {
        if (currentState != newState)
        {
            LogState($"🔄 [STATE] {currentState} → {newState}");
            currentState = newState;
        }
    }
    
    /// <summary>
    /// Delayed show character selection (ensures LobbyManager is ready).
    /// </summary>
    private IEnumerator DelayedShowCharacterSelection()
    {
        yield return null; // Wait one frame
        
        // FORCE SHOW CHARACTER SELECTION
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.ForceShowCharacterSelection();
            LogState("✅ [ACTION] Requested LobbyManager to show character selection");
        }
        else
        {
            LogState("❌ [ERROR] LobbyManager.Instance is NULL!");
            // Try to find it
            LobbyManager found = FindObjectOfType<LobbyManager>();
            if (found != null)
            {
                found.ForceShowCharacterSelection();
                LogState("✅ [FIX] Found LobbyManager and called ForceShowCharacterSelection");
            }
            else
            {
                LogState("❌ [ERROR] Could not find LobbyManager in scene!");
            }
        }
    }
    
    /// <summary>
    /// Log with state information.
    /// </summary>
    private void LogState(string message)
    {
        if (logUIEvents)
        {
            string statePrefix = $"[{currentState}]";
            Debug.Log($"[RoomCreationUI] {statePrefix} {message}");
        }
    }
}
#else
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomCreationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI statusText;
    
    private void Start()
    {
        if (statusText != null)
        {
            statusText.text = "Photon PUN2 not installed!";
        }
    }
}
#endif
