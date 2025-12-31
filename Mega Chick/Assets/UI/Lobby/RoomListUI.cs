#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

/// <summary>
/// Separate panel for displaying available rooms to join.
/// Format: Room No | Room Name | Players (1/2) | Join Button
/// </summary>
public class RoomListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform roomListParent;
    [SerializeField] private GameObject roomEntryPrefab;
    
    [Header("Debug")]
    [SerializeField] private bool logUIEvents = true;
    
    private List<GameObject> roomEntries = new List<GameObject>();
    private bool isSubscribed = false;
    
    private void Awake()
    {
        // Subscribe in Awake so it works even if GameObject is inactive
        SubscribeToRoomListUpdates();
        
        // Subscribe to lobby join event
        GameEventBus.OnJoinedLobby += OnJoinedLobby;
        
        // Subscribe to room join event - refresh when room is created/joined
        GameEventBus.OnJoinedRoom += OnJoinedRoom;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromRoomListUpdates();
        GameEventBus.OnJoinedLobby -= OnJoinedLobby;
        GameEventBus.OnJoinedRoom -= OnJoinedRoom;
    }
    
    /// <summary>
    /// Called when joined lobby - refresh room list.
    /// </summary>
    private void OnJoinedLobby()
    {
        Log("🎉 [EVENT] OnJoinedLobby - refreshing room list...");
        RefreshRoomList();
    }
    
    /// <summary>
    /// Called when joined room - refresh room list to show the room.
    /// </summary>
    private void OnJoinedRoom()
    {
        Log("🎉 [EVENT] OnJoinedRoom - refreshing room list to show current room...");
        // Small delay to ensure room properties are set
        StartCoroutine(DelayedRefreshAfterJoin());
    }
    
    private System.Collections.IEnumerator DelayedRefreshAfterJoin()
    {
        yield return new WaitForSeconds(0.3f); // Wait for room properties to be set
        RefreshRoomList();
    }
    
    private void Start()
    {
        Log("🔵 [START] RoomListUI Start() called");
        // Auto-refresh on start if active
        if (gameObject.activeInHierarchy)
        {
            Log("✅ [START] Panel is active, refreshing room list...");
            StartCoroutine(DelayedRefresh());
        }
        else
        {
            Log("⚠️ [START] Panel is inactive, will refresh when enabled");
        }
    }
    
    private void OnEnable()
    {
        Log("🔵 [ENABLE] RoomListUI OnEnable() called");
        // Re-subscribe when enabled (in case it was disabled)
        SubscribeToRoomListUpdates();
        // Refresh when shown - with a small delay to ensure everything is ready
        StartCoroutine(DelayedRefresh());
    }
    
    private System.Collections.IEnumerator DelayedRefresh()
    {
        yield return new WaitForSeconds(0.2f); // Small delay to ensure Photon is ready
        Log("⏰ [DELAY] Delayed refresh triggered");
        RefreshRoomList();
    }
    
    private void OnDisable()
    {
        UnsubscribeFromRoomListUpdates();
    }
    
    private void SubscribeToRoomListUpdates()
    {
        if (!isSubscribed && RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomListUpdated += OnRoomListUpdated;
            isSubscribed = true;
            Log("✅ Subscribed to RoomManager.OnRoomListUpdated");
        }
    }
    
    private void UnsubscribeFromRoomListUpdates()
    {
        if (isSubscribed && RoomManager.Instance != null)
        {
            RoomManager.Instance.OnRoomListUpdated -= OnRoomListUpdated;
            isSubscribed = false;
            Log("✅ Unsubscribed from RoomManager.OnRoomListUpdated");
        }
    }
    
    /// <summary>
    /// Show this panel and refresh room list.
    /// </summary>
    public void Show()
    {
        gameObject.SetActive(true);
        RefreshRoomList();
    }
    
    /// <summary>
    /// Hide this panel.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Refresh room list.
    /// </summary>
    public void RefreshRoomList()
    {
        Log($"🔄 [REFRESH] Refreshing room list... InLobby: {PhotonNetwork.InLobby}, InRoom: {PhotonNetwork.InRoom}");
        Log($"🔍 [DEBUG] roomListParent: {(roomListParent != null ? roomListParent.name : "NULL")}");
        Log($"🔍 [DEBUG] roomEntryPrefab: {(roomEntryPrefab != null ? roomEntryPrefab.name : "NULL")}");
        
        // Check if prefab is assigned
        if (roomEntryPrefab == null)
        {
            Log("❌ [ERROR] roomEntryPrefab is NULL! Cannot create room entries.");
            Log("💡 [TIP] Make sure RoomEntryPrefab.prefab exists in Assets/Prefabs/UI/");
            return;
        }
        
        // Check if parent is assigned
        if (roomListParent == null)
        {
            Log("❌ [ERROR] roomListParent is NULL! Cannot create room entries.");
            return;
        }
        
        // Ensure we're in lobby to see rooms (Photon requirement)
        if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            Log("⚠️ [WARN] Not in lobby and not in room! Joining lobby...");
            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinLobby();
                // Wait for lobby join, then refresh
                StartCoroutine(WaitForLobbyAndRefresh());
                return;
            }
            else
            {
                Log("❌ [ERROR] Not connected! Cannot join lobby.");
                return;
            }
        }
        
        // Ensure we're subscribed
        SubscribeToRoomListUpdates();
        
        if (RoomManager.Instance != null)
        {
            List<RoomInfo> rooms = RoomManager.Instance.GetRoomList();
            Log($"📋 [ROOMS] Got {rooms.Count} rooms from RoomManager");
            
            // If not in lobby but in a room, still show the current room
            if (!PhotonNetwork.InLobby && PhotonNetwork.InRoom)
            {
                Log("⚠️ Not in lobby, but in a room - will show current room only");
            }
            
            OnRoomListUpdated(rooms);
        }
        else
        {
            Log("❌ RoomManager.Instance is NULL!");
        }
    }
    
    private System.Collections.IEnumerator WaitForLobbyAndRefresh()
    {
        // Wait up to 2 seconds for lobby join
        float timeout = 2f;
        float elapsed = 0f;
        
        while (!PhotonNetwork.InLobby && elapsed < timeout)
        {
            yield return new WaitForSeconds(0.1f);
            elapsed += 0.1f;
        }
        
        if (PhotonNetwork.InLobby)
        {
            Log("✅ Joined lobby, refreshing room list...");
            RefreshRoomList();
        }
        else
        {
            Log("❌ Failed to join lobby within timeout");
        }
    }
    
    /// <summary>
    /// Called when room list is updated.
    /// </summary>
    private void OnRoomListUpdated(List<RoomInfo> roomList)
    {
        Log($"🎉 [EVENT] OnRoomListUpdated called with {roomList?.Count ?? 0} rooms");
        Log($"🔍 [DEBUG] Panel active: {gameObject.activeInHierarchy}");
        Log($"🔍 [DEBUG] Panel enabled: {enabled}");
        
        ClearRoomList();
        
        if (roomListParent == null)
        {
            Log("❌ [ERROR] roomListParent is NULL!");
            Log("💡 [TIP] Make sure Content GameObject exists under Viewport in RoomListScrollView");
            return;
        }
        
        if (roomEntryPrefab == null)
        {
            Log("❌ [ERROR] roomEntryPrefab is NULL! Cannot create room entries.");
            Log("💡 [TIP] Assign RoomEntryPrefab in the Inspector or run 'Step 6 UI Setup → Complete Setup'");
            return;
        }
        
        Log($"✅ [CHECK] All references valid - roomListParent: {roomListParent.name}, prefab: {roomEntryPrefab.name}");
        
        // Get current room info (if in a room) - we want to show it even if Photon filters it out
        string currentRoomCode = "";
        bool isInRoom = false;
        Room currentRoom = null;
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && RoomManager.Instance != null)
        {
            currentRoom = PhotonNetwork.CurrentRoom;
            currentRoomCode = RoomManager.Instance.GetCurrentRoomCode();
            isInRoom = true;
            Log($"🏠 [CURRENT] Found current room: {currentRoomCode} (Players: {currentRoom.PlayerCount}/{currentRoom.MaxPlayers})");
        }
        
        // Combine room list with current room (if not already in list)
        List<RoomInfo> allRooms = new List<RoomInfo>();
        if (roomList != null)
        {
            allRooms.AddRange(roomList);
            Log($"📋 [LIST] Photon room list has {roomList.Count} rooms");
        }
        
        // Check if current room is already in the list
        bool currentRoomInList = false;
        if (isInRoom && !string.IsNullOrEmpty(currentRoomCode))
        {
            foreach (RoomInfo room in allRooms)
            {
                if (room != null && room.Name == currentRoomCode)
                {
                    currentRoomInList = true;
                    Log($"✅ Current room already in Photon list: {currentRoomCode}");
                    break;
                }
            }
            
            if (!currentRoomInList)
            {
                Log($"⚠️ [NOTE] Current room {currentRoomCode} is NOT in Photon room list (this is normal - Photon excludes rooms you're in)");
            }
        }
        
        // Create room entry for each room in the list
        int createdCount = 0;
        Log($"📝 [CREATE] Attempting to create entries for {allRooms.Count} rooms...");
        
        foreach (RoomInfo room in allRooms)
        {
            if (room == null || room.RemovedFromList) 
            {
                Log($"⚠️ Skipping null or removed room");
                continue;
            }
            
            try
            {
                Log($"🔨 [CREATE] Creating entry for room: {room.Name} (Players: {room.PlayerCount}/{room.MaxPlayers})");
                GameObject entry = Instantiate(roomEntryPrefab, roomListParent);
                if (entry == null)
                {
                    Log($"❌ [ERROR] Instantiate returned NULL for room: {room.Name}");
                    continue;
                }
                
                roomEntries.Add(entry);
                Log($"✅ [CREATE] Instantiated GameObject: {entry.name}");
                
                SetupRoomEntry(entry, room, currentRoomCode);
                createdCount++;
                Log($"✅ [CREATE] Successfully created entry for room: {room.Name}");
            }
            catch (System.Exception e)
            {
                Log($"❌ [ERROR] Failed to create room entry: {e.Message}");
                Log($"❌ [ERROR] Stack trace: {e.StackTrace}");
            }
        }
        
        Log($"📊 [SUMMARY] Created {createdCount} room entries out of {allRooms.Count} rooms");
        
        // ALWAYS show current room if we're in one (Photon excludes it from room list)
        if (isInRoom && currentRoom != null)
        {
            // Always add current room to the list, even if it's in Photon's list
            // This ensures the room creator sees their own room
            try
            {
                Log($"🔨 [CREATE] Creating entry for current room: {currentRoomCode}");
                GameObject entry = Instantiate(roomEntryPrefab, roomListParent);
                if (entry == null)
                {
                    Log($"❌ [ERROR] Instantiate returned NULL for current room");
                }
                else
                {
                    roomEntries.Add(entry);
                    SetupRoomEntryFromCurrentRoom(entry, currentRoom, currentRoomCode);
                    createdCount++;
                    Log($"✅ [CREATE] Created entry for current room: {currentRoomCode}");
                }
            }
            catch (System.Exception e)
            {
                Log($"❌ [ERROR] Failed to create entry for current room: {e.Message}");
                Log($"❌ [ERROR] Stack trace: {e.StackTrace}");
            }
        }
        
        if (createdCount == 0)
        {
            Log("⚠️ [WARN] No room entries created!");
            Log($"💡 [TIP] Reasons could be:");
            Log($"   - No rooms available (create a room or wait for others)");
            Log($"   - Not in lobby (Photon requirement to see rooms)");
            Log($"   - All rooms were filtered out (removed/full/closed)");
            Log($"   - Prefab instantiation failed (check console for errors)");
            
            // Only show test entry if truly no rooms AND not in a room
            if (allRooms.Count == 0 && !isInRoom)
            {
                Log("💡 [TIP] Try creating a room to see it in the list!");
            }
        }
        else
        {
            Log($"✅ [SUCCESS] Created {createdCount} room entries (total: {roomEntries.Count})");
        }
        
        // Force layout update
        if (roomListParent != null)
        {
            UnityEngine.UI.LayoutGroup layout = roomListParent.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (layout != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(roomListParent as RectTransform);
                Log("✅ [LAYOUT] Forced layout rebuild");
            }
        }
    }
    
    /// <summary>
    /// Setup room entry from current Room (when not in Photon room list).
    /// </summary>
    private void SetupRoomEntryFromCurrentRoom(GameObject entryObj, Room room, string currentRoomCode)
    {
        TextMeshProUGUI[] allTexts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
        Button[] allButtons = entryObj.GetComponentsInChildren<Button>();
        
        // Get room info
        string roomName = room.Name;
        string friendlyRoomName = roomName;
        if (RoomManager.Instance != null)
        {
            friendlyRoomName = RoomManager.Instance.GetCurrentRoomName();
        }
        
        // Get host name (master client)
        string hostName = "Unknown";
        if (room.CustomProperties.ContainsKey("HostName"))
        {
            hostName = room.CustomProperties["HostName"].ToString();
        }
        else if (PhotonNetwork.MasterClient != null)
        {
            hostName = PhotonNetwork.MasterClient.NickName;
            if (string.IsNullOrEmpty(hostName))
            {
                hostName = $"Player {PhotonNetwork.MasterClient.ActorNumber}";
            }
        }
        
        // Get game mode from room properties
        string gameModeName = "Unknown";
        if (room.CustomProperties.ContainsKey("GameModeName"))
        {
            gameModeName = room.CustomProperties["GameModeName"].ToString();
        }
        else if (room.CustomProperties.ContainsKey("GameModeIndex"))
        {
            int modeIndex = (int)room.CustomProperties["GameModeIndex"];
            string[] modeNames = { "Race", "FFA", "Hunter", "Zone", "Carry" };
            if (modeIndex >= 0 && modeIndex < modeNames.Length)
            {
                gameModeName = modeNames[modeIndex];
            }
        }
        
        int playerCount = room.PlayerCount;
        int maxPlayers = room.MaxPlayers;
        
        // Format: Room Name | Host: HostName | Game Mode | Players (1/2)
        if (allTexts.Length > 0)
        {
            allTexts[0].text = friendlyRoomName;
        }
        if (allTexts.Length > 1)
        {
            allTexts[1].text = $"Host: {hostName}";
        }
        if (allTexts.Length > 2)
        {
            allTexts[2].text = gameModeName;
        }
        if (allTexts.Length > 3)
        {
            allTexts[3].text = $"Players {playerCount}/{maxPlayers}";
        }
        else if (allTexts.Length > 0)
        {
            allTexts[0].text = $"{friendlyRoomName} | Host: {hostName} | {gameModeName} | {playerCount}/{maxPlayers}";
        }
        
        // Join button - disabled for current room (you're already in it)
        if (allButtons.Length > 0)
        {
            Button joinButton = allButtons[0];
            joinButton.interactable = false; // Always disabled for current room
            
            // Set button text to game mode name
            TextMeshProUGUI buttonText = joinButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = gameModeName;
            }
            
            // Change button color
            Image buttonImage = joinButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = Color.grey; // Grey (disabled - you're in this room)
            }
            
            // Don't wire the button - you can't join a room you're already in
            Log($"✅ [WIRE] Join button disabled for current room: {roomName} (You are already in this room)");
        }
    }
    
    /// <summary>
    /// Setup room entry UI: Room Name | Host | Game Mode | Players (1/2) | Join Button
    /// </summary>
    private void SetupRoomEntry(GameObject entryObj, RoomInfo room, string currentRoomCode)
    {
        TextMeshProUGUI[] allTexts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
        Button[] allButtons = entryObj.GetComponentsInChildren<Button>();
        
        // Get room info
        string roomName = room.Name;
        string friendlyRoomName = roomName; // Default to room code
        
        // Try to get friendly room name from RoomManager
        if (RoomManager.Instance != null)
        {
            // Check if this is the current room
            if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.Name == roomName)
            {
                friendlyRoomName = RoomManager.Instance.GetCurrentRoomName();
            }
        }
        
        // Get host name (master client)
        string hostName = "Unknown";
        if (room.CustomProperties.ContainsKey("HostName"))
        {
            hostName = room.CustomProperties["HostName"].ToString();
        }
        else if (room.masterClientId > 0)
        {
            // Try to get from player list if available
            hostName = $"Player {room.masterClientId}";
        }
        
        // Get game mode from room properties
        string gameModeName = "Unknown";
        if (room.CustomProperties.ContainsKey("GameModeName"))
        {
            gameModeName = room.CustomProperties["GameModeName"].ToString();
        }
        else if (room.CustomProperties.ContainsKey("GameModeIndex"))
        {
            // Fallback: convert index to name
            int modeIndex = (int)room.CustomProperties["GameModeIndex"];
            string[] modeNames = { "Race", "FFA", "Hunter", "Zone", "Carry" };
            if (modeIndex >= 0 && modeIndex < modeNames.Length)
            {
                gameModeName = modeNames[modeIndex];
            }
        }
        
        // Format: Room Name | Host: HostName | Game Mode | Players (1/2)
        // Try to find text elements by name first, then by index
        TextMeshProUGUI roomNameText = null;
        TextMeshProUGUI hostText = null;
        TextMeshProUGUI gameModeText = null;
        TextMeshProUGUI playerCountText = null;
        
        // Find by name
        foreach (TextMeshProUGUI text in allTexts)
        {
            string name = text.name.ToLower();
            if (name.Contains("roomname"))
                roomNameText = text;
            else if (name.Contains("host"))
                hostText = text;
            else if (name.Contains("gamemode") || name.Contains("mode"))
                gameModeText = text;
            else if (name.Contains("playercount") || name.Contains("player"))
                playerCountText = text;
        }
        
        // Fallback to index if not found by name
        if (roomNameText == null && allTexts.Length > 0) roomNameText = allTexts[0];
        if (hostText == null && allTexts.Length > 1) hostText = allTexts[1];
        if (gameModeText == null && allTexts.Length > 2) gameModeText = allTexts[2];
        if (playerCountText == null && allTexts.Length > 3) playerCountText = allTexts[3];
        
        // Set text values
        if (roomNameText != null) roomNameText.text = friendlyRoomName;
        if (hostText != null) hostText.text = $"Host: {hostName}";
        if (gameModeText != null) gameModeText.text = gameModeName;
        if (playerCountText != null) playerCountText.text = $"{room.PlayerCount}/{room.MaxPlayers}";
        
        // Fallback: if no texts found, combine in first text (like PlayerListUI)
        if (allTexts.Length > 0 && roomNameText == null && hostText == null && gameModeText == null && playerCountText == null)
        {
            allTexts[0].text = $"{friendlyRoomName} | Host: {hostName} | {gameModeName} | {room.PlayerCount}/{room.MaxPlayers}";
        }
        
        // Join button - only interactable if not the room owner
        bool isRoomOwner = (currentRoomCode == room.Name);
        if (allButtons.Length > 0)
        {
            Button joinButton = allButtons[0];
            joinButton.interactable = !isRoomOwner;
            
            // Set button text to game mode name
            TextMeshProUGUI buttonText = joinButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = gameModeName;
            }
            
            // Change button color based on state
            Image buttonImage = joinButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                if (isRoomOwner)
                {
                    buttonImage.color = Color.grey;
                }
                else
                {
                    buttonImage.color = new Color(0.5f, 0.2f, 0.8f); // Purple like in screenshot
                }
            }
            
            // Wire join button - only wire if not owner (owner can't join their own room)
            string roomNameToJoin = room.Name;
            joinButton.onClick.RemoveAllListeners();
            if (!isRoomOwner)
            {
                joinButton.onClick.AddListener(() => JoinRoom(roomNameToJoin));
                Log($"✅ [WIRE] Join button wired for room: {roomNameToJoin} (Can join)");
            }
            else
            {
                Log($"✅ [WIRE] Join button disabled for room: {roomNameToJoin} (You are the owner)");
            }
        }
    }
    
    /// <summary>
    /// Join a room by name.
    /// </summary>
    private void JoinRoom(string roomName)
    {
        Log($"🖱️ [CLICK] Join room: {roomName}");
        
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.JoinRoom(roomName);
        }
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
    
    private void Log(string message)
    {
        if (logUIEvents)
        {
            Debug.Log($"[RoomListUI] {message}");
        }
    }
}
#endif

