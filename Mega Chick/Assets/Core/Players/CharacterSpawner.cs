#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// Spawns complete character prefabs based on character selection from lobby.
/// This spawns the character prefab directly instead of a generic player prefab.
/// 
/// SETUP INSTRUCTIONS:
/// 1. Add this script to a GameObject in your game scene (or use the prefab)
/// 2. Ensure character prefabs in CharacterData have all required components:
///    - PhotonView (for networking)
///    - PlayerController (for movement)
///    - PlayerNetworkSync (for position synchronization)
///    - Rigidbody, Collider, etc. (as needed)
/// 3. Character prefabs should be in Resources folder OR assigned directly in CharacterData
/// 4. Enable "Use Character Prefabs" in SpawnManager to use this system
/// 
/// USAGE:
/// - Automatic: SpawnManager will use this if "Use Character Prefabs" is enabled
/// - Manual: Call CharacterSpawner.Instance.SpawnCharacter(player, position, rotation)
/// 
/// The character selection is automatically retrieved from Photon player properties
/// (set in lobby via CharacterSelectionManager).
/// </summary>
public class CharacterSpawner : MonoBehaviourPunCallbacks
{
    public static CharacterSpawner Instance { get; private set; }
    
    [Header("Spawn Configuration")]
    [Tooltip("If true, character prefabs must be in Resources folder with name matching CharacterData.characterPrefab name")]
    [SerializeField] private bool useResourcesFolder = true;
    
    [Header("Fallback")]
    [Tooltip("Fallback player prefab if character prefab is not found or character not selected")]
    [SerializeField] private GameObject fallbackPlayerPrefab;
    
    [Header("Debug")]
    [SerializeField] private bool logSpawnEvents = true;
    
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
    
    /// <summary>
    /// Spawn character for a player at the specified position and rotation.
    /// Uses character selection from lobby (stored in Photon player properties).
    /// </summary>
    public GameObject SpawnCharacter(Player player, Vector3 position, Quaternion rotation)
    {
        if (player == null)
        {
            Log("Cannot spawn: Player is null!");
            return null;
        }
        
        // Get character data from selection manager
        CharacterData characterData = null;
        if (CharacterSelectionManager.Instance != null)
        {
            characterData = CharacterSelectionManager.Instance.GetCharacterData(player);
        }
        
        // Get character prefab
        GameObject characterPrefab = GetCharacterPrefab(characterData, player);
        
        if (characterPrefab == null)
        {
            Log($"Failed to get character prefab for player {player.ActorNumber}. Using fallback.");
            characterPrefab = fallbackPlayerPrefab;
        }
        
        if (characterPrefab == null)
        {
            Log($"ERROR: No character prefab or fallback available for player {player.ActorNumber}!");
            return null;
        }
        
        // Spawn via Photon Network
        GameObject spawnedCharacter = PhotonNetwork.Instantiate(
            characterPrefab.name,
            position,
            rotation,
            0,
            new object[] { player.ActorNumber }
        );
        
        if (spawnedCharacter != null)
        {
            // Apply character stats if character data is available
            if (characterData != null)
            {
                ApplyCharacterStats(spawnedCharacter, characterData);
            }
            
            Log($"✅ Spawned character '{characterPrefab.name}' for player {player.ActorNumber} at {position}");
        }
        else
        {
            Log($"❌ Failed to spawn character for player {player.ActorNumber}!");
        }
        
        return spawnedCharacter;
    }
    
    /// <summary>
    /// Get character prefab from character data.
    /// </summary>
    private GameObject GetCharacterPrefab(CharacterData characterData, Player player)
    {
        if (characterData == null)
        {
            Log($"No character data for player {player.ActorNumber}");
            return null;
        }
        
        if (characterData.characterPrefab == null)
        {
            Log($"Character '{characterData.characterName}' has no prefab assigned!");
            return null;
        }
        
        // If using Resources folder, try to load from Resources first
        if (useResourcesFolder)
        {
            GameObject resourcePrefab = Resources.Load<GameObject>(characterData.characterPrefab.name);
            if (resourcePrefab != null)
            {
                Log($"Loaded character prefab '{characterData.characterPrefab.name}' from Resources");
                return resourcePrefab;
            }
        }
        
        // Use prefab directly from CharacterData
        Log($"Using character prefab '{characterData.characterPrefab.name}' from CharacterData");
        return characterData.characterPrefab;
    }
    
    /// <summary>
    /// Apply character stats (speed, jump, etc.) to spawned character.
    /// </summary>
    private void ApplyCharacterStats(GameObject characterObj, CharacterData characterData)
    {
        if (characterObj == null || characterData == null) return;
        
        // Apply stats to PlayerController if it exists
        PlayerController playerController = characterObj.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Note: PlayerController should handle stat multipliers via MovementConfig
            // This is a placeholder - adjust based on your PlayerController implementation
            Log($"Applied character stats to {characterData.characterName}");
        }
        
        // Store character data reference in PlayerVisual if it exists
        PlayerVisual playerVisual = characterObj.GetComponent<PlayerVisual>();
        if (playerVisual != null)
        {
            playerVisual.SetCharacter(characterData);
        }
    }
    
    /// <summary>
    /// Spawn character for local player (called when entering game scene).
    /// </summary>
    public void SpawnLocalPlayer(Vector3 position, Quaternion rotation)
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
        {
            Log("Cannot spawn local player: Not connected to Photon!");
            return;
        }
        
        SpawnCharacter(PhotonNetwork.LocalPlayer, position, rotation);
    }
    
    /// <summary>
    /// Spawn character for all players in room.
    /// </summary>
    public void SpawnAllPlayers(Transform[] spawnPoints)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Log("Only Master Client can spawn all players!");
            return;
        }
        
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Log("No spawn points provided!");
            return;
        }
        
        Player[] players = PhotonNetwork.PlayerList;
        Log($"Spawning {players.Length} players...");
        
        for (int i = 0; i < players.Length; i++)
        {
            Transform spawnPoint = spawnPoints[i % spawnPoints.Length];
            if (spawnPoint != null)
            {
                SpawnCharacter(players[i], spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
    
    /// <summary>
    /// Respawn a character at a specific position.
    /// </summary>
    public void RespawnCharacter(int actorNumber, Vector3 position, Quaternion rotation)
    {
        // Find existing player object and destroy it
        GameObject existingPlayer = FindPlayerObject(actorNumber);
        if (existingPlayer != null)
        {
            PhotonNetwork.Destroy(existingPlayer);
        }
        
        // Find player by actor number
        Player player = null;
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.ActorNumber == actorNumber)
            {
                player = p;
                break;
            }
        }
        
        if (player != null)
        {
            SpawnCharacter(player, position, rotation);
        }
        else
        {
            Log($"Cannot respawn: Player with actor number {actorNumber} not found!");
        }
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
    
    private void Log(string message)
    {
        if (logSpawnEvents)
        {
            Debug.Log($"[CharacterSpawner] {message}");
        }
    }
}
#else
using UnityEngine;

/// <summary>
/// Photon not installed - stub implementation.
/// </summary>
public class CharacterSpawner : MonoBehaviour
{
    public static CharacterSpawner Instance { get; private set; }
    
    [SerializeField] private GameObject fallbackPlayerPrefab;
    
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
        
        Debug.LogWarning("[CharacterSpawner] Photon not installed!");
    }
    
    public GameObject SpawnCharacter(object player, Vector3 position, Quaternion rotation) => null;
    public void SpawnLocalPlayer(Vector3 position, Quaternion rotation) { }
    public void SpawnAllPlayers(Transform[] spawnPoints) { }
    public void RespawnCharacter(int actorNumber, Vector3 position, Quaternion rotation) { }
}
#endif

