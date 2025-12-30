#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

/// <summary>
/// Displays list of players in the room with their character selections.
/// Why separate? Player list is a distinct UI component, reusable.
/// </summary>
public class PlayerListUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform playerListParent;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private TextMeshProUGUI roomNameText; // Room name/title
    [SerializeField] private TextMeshProUGUI statusText; // "Joined!" or status message
    [SerializeField] private TextMeshProUGUI titleText; // "AVAILABLE ROOM!" title
    
    [Header("Debug")]
    [SerializeField] private bool logUIEvents = true;
    
    private List<GameObject> playerEntries = new List<GameObject>();
    
    private void Start()
    {
        // Subscribe to events
        GameEventBus.OnJoinedRoom += RefreshPlayerList;
        GameEventBus.OnPlayerEnteredRoom += OnPlayerEnteredRoom;
        GameEventBus.OnPlayerLeftRoom += OnPlayerLeftRoom;
        
        // Initial refresh
        RefreshPlayerList();
    }
    
    private void OnDestroy()
    {
        GameEventBus.OnJoinedRoom -= RefreshPlayerList;
        GameEventBus.OnPlayerEnteredRoom -= OnPlayerEnteredRoom;
        GameEventBus.OnPlayerLeftRoom -= OnPlayerLeftRoom;
    }
    
    /// <summary>
    /// Refresh entire player list.
    /// </summary>
    public void RefreshPlayerList()
    {
        if (!PhotonNetwork.InRoom)
        {
            ClearPlayerList();
            return;
        }
        
        // Clear existing entries
        ClearPlayerList();
        
        // Get all players
        Player[] players = PhotonNetwork.PlayerList;
        
        // Create entry for each player
        foreach (Player player in players)
        {
            CreatePlayerEntry(player);
        }
        
        // Update player count
        UpdatePlayerCount();
        
        // Update room code and name
        UpdateRoomCode();
        UpdateRoomName();
        UpdateStatus();
    }
    
    /// <summary>
    /// Create player entry UI.
    /// </summary>
    private void CreatePlayerEntry(Player player)
    {
        if (playerEntryPrefab == null || playerListParent == null)
        {
            Log("Player entry prefab or parent not assigned!");
            return;
        }
        
        GameObject entryObj = Instantiate(playerEntryPrefab, playerListParent);
        playerEntries.Add(entryObj);
        
        // Get character data
        CharacterData characterData = null;
        if (CharacterSelectionManager.Instance != null)
        {
            characterData = CharacterSelectionManager.Instance.GetCharacterData(player);
        }
        
        // Setup entry UI
        SetupPlayerEntry(entryObj, player, characterData);
    }
    
    /// <summary>
    /// Setup player entry UI elements.
    /// Format: Room No/Name | Player Name | Character Name | Game Mode
    /// </summary>
    private void SetupPlayerEntry(GameObject entryObj, Player player, CharacterData characterData)
    {
        // Get all UI components
        TextMeshProUGUI[] allTexts = entryObj.GetComponentsInChildren<TextMeshProUGUI>();
        Image[] allImages = entryObj.GetComponentsInChildren<Image>();
        
        // Get room info
        string roomNo = "";
        string roomName = "";
        if (PhotonNetwork.InRoom && RoomManager.Instance != null)
        {
            roomNo = RoomManager.Instance.GetCurrentRoomCode();
            roomName = RoomManager.Instance.GetCurrentRoomName();
            if (string.IsNullOrEmpty(roomName))
            {
                roomName = roomNo;
            }
        }
        
        // Get player name
        string playerName = string.IsNullOrEmpty(player.NickName) ? $"Player {player.ActorNumber}" : player.NickName;
        
        // Get character name
        string characterName = characterData != null ? characterData.characterName : "No Character";
        
        // Get game mode name
        string gameModeName = "No Mode";
        if (GameModeSelectionManager.Instance != null)
        {
            gameModeName = GameModeSelectionManager.Instance.GetSelectedGameModeName(player);
        }
        
        // Format: Room No/Name | Player Name | Character Name | Game Mode
        // First text component gets the full formatted string
        if (allTexts.Length > 0)
        {
            string formattedText = $"{roomNo}/{roomName} | {playerName} | {characterName} | {gameModeName}";
            allTexts[0].text = formattedText;
            
            // Highlight local player
            if (player.IsLocal)
            {
                allTexts[0].color = Color.yellow;
            }
            else
            {
                allTexts[0].color = Color.white;
            }
        }
        
        // Character icon (second image, skip first which is usually background)
        if (allImages.Length > 1 && characterData != null && characterData.icon != null)
        {
            allImages[1].sprite = characterData.icon;
        }
        
        // Master client indicator
        if (player.IsMasterClient)
        {
            // Add master client indicator (star, crown, etc.)
            Transform masterIndicator = entryObj.transform.Find("MasterIndicator");
            if (masterIndicator != null)
            {
                masterIndicator.gameObject.SetActive(true);
            }
        }
    }
    
    /// <summary>
    /// Clear all player entries.
    /// </summary>
    private void ClearPlayerList()
    {
        foreach (GameObject entry in playerEntries)
        {
            if (entry != null)
            {
                Destroy(entry);
            }
        }
        playerEntries.Clear();
    }
    
    /// <summary>
    /// Update player count display - HIDDEN (user doesn't want titles).
    /// </summary>
    private void UpdatePlayerCount()
    {
        // Hide player count text - user doesn't want it
        if (playerCountText != null)
        {
            playerCountText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update room code display - HIDDEN (user doesn't want titles).
    /// </summary>
    private void UpdateRoomCode()
    {
        // Hide room code text - user doesn't want it
        if (roomCodeText != null)
        {
            roomCodeText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update room name display - HIDDEN (user doesn't want titles).
    /// </summary>
    private void UpdateRoomName()
    {
        // Hide title text - user doesn't want it
        if (titleText != null)
        {
            titleText.gameObject.SetActive(false);
        }
        
        if (roomNameText != null)
        {
            roomNameText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Update status text - HIDDEN (user doesn't want titles).
    /// </summary>
    private void UpdateStatus()
    {
        // Hide status text - user doesn't want it
        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Called when a player enters the room.
    /// </summary>
    private void OnPlayerEnteredRoom(string actorNumber)
    {
        RefreshPlayerList();
    }
    
    /// <summary>
    /// Called when a player leaves the room.
    /// </summary>
    private void OnPlayerLeftRoom(string actorNumber)
    {
        RefreshPlayerList();
    }
    
    private void Log(string message)
    {
        if (logUIEvents)
        {
            Debug.Log($"[PlayerListUI] {message}");
        }
    }
}
#else
using UnityEngine;
using TMPro;

/// <summary>
/// Photon not installed - stub.
/// </summary>
public class PlayerListUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerCountText;
    
    private void Start()
    {
        if (playerCountText != null)
        {
            playerCountText.text = "Photon PUN2 not installed!";
        }
    }
}
#endif

