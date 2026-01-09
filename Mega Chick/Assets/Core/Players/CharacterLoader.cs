#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

/// <summary>
/// Generic character loader for any game scene.
/// Automatically loads character models on Player prefabs based on lobby selection.
/// Works in Race, Arena, or any game mode scene.
/// 
/// SETUP:
/// 1. Drag CharacterLoader prefab into any game scene (Race, Arena, etc.)
/// 2. Ensure CharacterSelectionManager is also in the scene (or use DontDestroyOnLoad)
/// 3. That's it! Characters will auto-load when players spawn
/// 
/// HOW IT WORKS:
/// - Gets character selection from Photon player properties (set in lobby)
/// - If no character selected (direct scene open), uses first character as default
/// - Automatically loads character model on Player prefab via PlayerVisual
/// 
/// NO MANUAL SETUP REQUIRED - Just drag and drop!
/// </summary>
public class CharacterLoader : MonoBehaviourPunCallbacks
{
    public static CharacterLoader Instance { get; private set; }
    
    [Header("Auto-Load Settings")]
    [Tooltip("Automatically load characters when players spawn")]
    [SerializeField] private bool autoLoadOnSpawn = true;
    
    [Tooltip("Delay before loading character (allows Player prefab to fully initialize)")]
    [SerializeField] private float loadDelay = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool logLoadEvents = true;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    private void Start()
    {
        // Ensure default character is set for local player if not selected
        EnsureDefaultCharacter();
        
        // Load character for existing players (if scene opened directly)
        StartCoroutine(LoadCharactersForExistingPlayers());
    }
    
    /// <summary>
    /// Ensure local player has a default character selected if none chosen.
    /// </summary>
    private void EnsureDefaultCharacter()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            Log("Not connected to Photon - skipping default character setup");
            return;
        }
        
        if (CharacterSelectionManager.Instance == null)
        {
            Log("CharacterSelectionManager not found - characters may not load!");
            return;
        }
        
        // Check if local player has character selected
        int selectedIndex = CharacterSelectionManager.Instance.GetSelectedCharacterIndex(PhotonNetwork.LocalPlayer);
        
        // If no character selected (index 0 might be valid, but check if it's actually set)
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("CharacterIndex"))
        {
            // Set default to first character (index 0)
            CharacterSelectionManager.Instance.SelectCharacter(0);
            Log("Set default character (first in list) for local player");
        }
    }
    
    /// <summary>
    /// Load characters for players that already exist in scene.
    /// </summary>
    private IEnumerator LoadCharactersForExistingPlayers()
    {
        yield return new WaitForSeconds(loadDelay);
        
        if (!PhotonNetwork.IsConnected)
        {
            yield break;
        }
        
        // Find all Player objects in scene
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (var pv in photonViews)
        {
            if (pv.gameObject.name.Contains("Player") && pv.Owner != null)
            {
                LoadCharacterForPlayer(pv.gameObject, pv.Owner);
            }
        }
    }
    
    /// <summary>
    /// Called when a player enters the room.
    /// </summary>
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        
        if (autoLoadOnSpawn)
        {
            // Wait a bit for player to spawn, then load character
            StartCoroutine(LoadCharacterForPlayerDelayed(newPlayer));
        }
    }
    
    /// <summary>
    /// Delayed character load for newly joined player.
    /// </summary>
    private IEnumerator LoadCharacterForPlayerDelayed(Player player)
    {
        yield return new WaitForSeconds(loadDelay);
        
        // Find player object
        GameObject playerObj = FindPlayerObject(player.ActorNumber);
        if (playerObj != null)
        {
            LoadCharacterForPlayer(playerObj, player);
        }
    }
    
    /// <summary>
    /// Load character model for a player object.
    /// </summary>
    public void LoadCharacterForPlayer(GameObject playerObj, Player player)
    {
        if (playerObj == null || player == null)
        {
            Log("Cannot load character: Player object or player is null");
            return;
        }
        
        if (CharacterSelectionManager.Instance == null)
        {
            Log("CharacterSelectionManager not found!");
            return;
        }
        
        // Get character data for this player
        CharacterData characterData = CharacterSelectionManager.Instance.GetCharacterData(player);
        
        if (characterData == null)
        {
            Log($"No character data found for player {player.ActorNumber} - using default");
            // Try to get first character as fallback
            var availableChars = CharacterSelectionManager.Instance.GetAvailableCharacters();
            if (availableChars.Count > 0)
            {
                characterData = availableChars[0];
            }
        }
        
        if (characterData == null)
        {
            Log($"ERROR: No characters available at all!");
            return;
        }
        
        // Get or add PlayerVisual component
        PlayerVisual playerVisual = playerObj.GetComponent<PlayerVisual>();
        if (playerVisual == null)
        {
            Log($"PlayerVisual not found on {playerObj.name} - adding it");
            playerVisual = playerObj.AddComponent<PlayerVisual>();
        }
        
        // Load character model
        playerVisual.SetCharacter(characterData);
        
        Log($"✅ Loaded character '{characterData.characterName}' for player {player.ActorNumber}");
    }
    
    /// <summary>
    /// Find player GameObject by actor number.
    /// </summary>
    private GameObject FindPlayerObject(int actorNumber)
    {
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        foreach (var pv in photonViews)
        {
            if (pv.Owner != null && pv.Owner.ActorNumber == actorNumber)
            {
                return pv.gameObject;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Manually trigger character load for all players in scene.
    /// Useful if characters didn't load automatically.
    /// </summary>
    [ContextMenu("Load All Characters Now")]
    public void LoadAllCharactersNow()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Log("Not connected to Photon!");
            return;
        }
        
        Player[] players = PhotonNetwork.PlayerList;
        foreach (Player player in players)
        {
            GameObject playerObj = FindPlayerObject(player.ActorNumber);
            if (playerObj != null)
            {
                LoadCharacterForPlayer(playerObj, player);
            }
        }
        
        Log($"Loaded characters for {players.Length} players");
    }
    
    private void Log(string message)
    {
        if (logLoadEvents)
        {
            Debug.Log($"[CharacterLoader] {message}");
        }
    }
}
#else
using UnityEngine;

/// <summary>
/// Photon not installed - stub.
/// </summary>
public class CharacterLoader : MonoBehaviour
{
    public static CharacterLoader Instance { get; private set; }
    
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
        
        Debug.LogWarning("[CharacterLoader] Photon not installed!");
    }
}
#endif

