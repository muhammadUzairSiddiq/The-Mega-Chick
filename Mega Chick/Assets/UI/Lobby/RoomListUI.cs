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
    }
    
    private void OnEnable()
    {
        // Re-subscribe when enabled (in case it was disabled)
        SubscribeToRoomListUpdates();
        // Refresh when shown
        RefreshRoomList();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromRoomListUpdates();
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
        
        // Allow refresh even if not in lobby (we'll show current room if in one)
        // But prefer to be in lobby to see other rooms
        
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
    
    /// <summary>
    /// Called when room list is updated.
    /// </summary>
    private void OnRoomListUpdated(List<RoomInfo> roomList)
    {
        Log($"🎉 [EVENT] OnRoomListUpdated called with {roomList?.Count ?? 0} rooms");
        
        ClearRoomList();
        
        if (roomListParent == null)
        {
            Log("❌ [ERROR] roomListParent is NULL!");
            return;
        }
        
        if (roomEntryPrefab == null)
        {
            Log("❌ [ERROR] roomEntryPrefab is NULL! Cannot create room entries.");
            return;
        }
        
        // Get current room info (if in a room) - we want to show it even if Photon filters it out
        string currentRoomCode = "";
        bool isInRoom = false;
        if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom != null && RoomManager.Instance != null)
        {
            currentRoomCode = RoomManager.Instance.GetCurrentRoomCode();
            isInRoom = true;
            Log($"🏠 [CURRENT] Found current room: {currentRoomCode}");
        }
        
        // Combine room list with current room (if not already in list)
        List<RoomInfo> allRooms = new List<RoomInfo>();
        if (roomList != null)
        {
            allRooms.AddRange(roomList);
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
                    Log($"✅ Current room already in list: {currentRoomCode}");
                    break;
                }
            }
        }
        
        // Create room entry for each room in the list
        int createdCount = 0;
        foreach (RoomInfo room in allRooms)
        {
            if (room == null || room.RemovedFromList) 
            {
                Log($"⚠️ Skipping null or removed room");
                continue;
            }
            
            try
            {
                GameObject entry = Instantiate(roomEntryPrefab, roomListParent);
                roomEntries.Add(entry);
                SetupRoomEntry(entry, room, currentRoomCode);
                createdCount++;
                Log($"✅ Created entry for room: {room.Name}");
            }
            catch (System.Exception e)
            {
                Log($"❌ [ERROR] Failed to create room entry: {e.Message}");
            }
        }
        
        // If we're in a room but it's not in the Photon room list, create a manual entry for it
        if (isInRoom && !currentRoomInList && PhotonNetwork.CurrentRoom != null)
        {
            try
            {
                GameObject entry = Instantiate(roomEntryPrefab, roomListParent);
                roomEntries.Add(entry);
                SetupRoomEntryFromCurrentRoom(entry, PhotonNetwork.CurrentRoom, currentRoomCode);
                createdCount++;
                Log($"✅ Created entry for current room (not in Photon list): {currentRoomCode}");
            }
            catch (System.Exception e)
            {
                Log($"❌ [ERROR] Failed to create entry for current room: {e.Message}");
            }
        }
        
        if (createdCount == 0)
        {
            Log("⚠️ No room entries created (room list empty and not in a room)");
        }
        else
        {
            Log($"✅ Created {createdCount} room entries (total: {roomEntries.Count})");
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
        string roomNo = room.Name;
        int playerCount = room.PlayerCount;
        int maxPlayers = room.MaxPlayers;
        
        // Format: Room No | Room Name | Players (1/2) | Join Button
        if (allTexts.Length > 0)
        {
            string formattedText = $"{roomNo} | {roomName} | Players {playerCount}/{maxPlayers}";
            allTexts[0].text = formattedText;
        }
        
        // Join button - disabled for current room (you're already in it)
        bool isRoomOwner = (currentRoomCode == room.Name);
        if (allButtons.Length > 0)
        {
            Button joinButton = allButtons[0];
            joinButton.interactable = false; // Always disabled for current room
            
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
    /// Setup room entry UI: Room No | Room Name | Players (1/2) | Join Button
    /// </summary>
    private void SetupRoomEntry(GameObject entryObj, RoomInfo room, string currentRoomCode)
    {
        TextMeshProUGUI[] allTexts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
        Button[] allButtons = entryObj.GetComponentsInChildren<Button>();
        
        // Get room name (try to get from RoomManager if available)
        string roomName = room.Name;
        string roomNo = room.Name; // Room code is the room name in Photon
        
        // Try to get friendly room name if available
        // Note: We can't get friendly name from RoomInfo, so we'll use room code
        // Format: Room No | Room Name | Players (1/2) | Join Button
        if (allTexts.Length > 0)
        {
            string formattedText = $"{roomNo} | {roomName} | Players {room.PlayerCount}/{room.MaxPlayers}";
            allTexts[0].text = formattedText;
        }
        
        // Join button - only interactable if not the room owner
        bool isRoomOwner = (currentRoomCode == room.Name);
        if (allButtons.Length > 0)
        {
            Button joinButton = allButtons[0];
            joinButton.interactable = !isRoomOwner;
            
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
                    buttonImage.color = Color.green;
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

