#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

/// <summary>
/// Manages player spawning for all game modes.
/// Why separate? Spawn logic differs per mode (race vs arena).
/// Master Client controls spawn assignments.
/// </summary>
public class SpawnManager : MonoBehaviourPunCallbacks
{
    public static SpawnManager Instance { get; private set; }
    
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> raceSpawnPoints = new List<Transform>();
    [SerializeField] private List<Transform> arenaSpawnPoints = new List<Transform>();
    
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Debug")]
    [SerializeField] private bool logSpawnEvents = true;
    
    private Dictionary<int, Transform> playerSpawnAssignments = new Dictionary<int, Transform>();
    
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
    /// Spawn all players for current mode.
    /// Called by Master Client when match starts.
    /// </summary>
    public void SpawnAllPlayers(MatchState matchState)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Log("Only Master Client can spawn players!");
            return;
        }
        
        if (playerPrefab == null)
        {
            Log("Player prefab not assigned!");
            return;
        }
        
        List<Transform> spawnPoints = GetSpawnPointsForMode(matchState);
        
        if (spawnPoints.Count == 0)
        {
            Log("No spawn points available!");
            return;
        }
        
        // Get all players in room
        Photon.Realtime.Player[] players = PhotonNetwork.PlayerList;
        
        Log($"Spawning {players.Length} players...");
        
        // Assign spawn points and spawn
        for (int i = 0; i < players.Length; i++)
        {
            Transform spawnPoint = GetSpawnPoint(spawnPoints, i);
            SpawnPlayer(players[i], spawnPoint);
        }
    }
    
    /// <summary>
    /// Get spawn points for current mode.
    /// Why separate? Race uses ordered spawns, arena uses random.
    /// </summary>
    private List<Transform> GetSpawnPointsForMode(MatchState matchState)
    {
        switch (matchState)
        {
            case MatchState.Playing:
                // Check if we're in race or arena mode (for now, default to race)
                // TODO: Get current mode from ModeManager
                return raceSpawnPoints.Count > 0 ? raceSpawnPoints : arenaSpawnPoints;
            default:
                return raceSpawnPoints;
        }
    }
    
    /// <summary>
    /// Get spawn point for player index.
    /// For race: ordered by index.
    /// For arena: random (avoid immediate overlap).
    /// </summary>
    private Transform GetSpawnPoint(List<Transform> spawnPoints, int playerIndex)
    {
        if (spawnPoints.Count == 0) return null;
        
        // For race mode: use index-based (ordered start positions)
        // For arena: use random (will implement later)
        int spawnIndex = playerIndex % spawnPoints.Count;
        return spawnPoints[spawnIndex];
    }
    
    /// <summary>
    /// Spawn a single player at spawn point.
    /// </summary>
    private void SpawnPlayer(Photon.Realtime.Player player, Transform spawnPoint)
    {
        if (spawnPoint == null)
        {
            Log($"No spawn point for player {player.ActorNumber}!");
            return;
        }
        
        // Store spawn assignment
        playerSpawnAssignments[player.ActorNumber] = spawnPoint;
        
        // Spawn via Photon (all clients will instantiate)
        GameObject playerObj = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoint.position,
            spawnPoint.rotation,
            0,
            new object[] { player.ActorNumber }
        );
        
        // Apply character selection
        ApplyCharacterToPlayer(playerObj, player);
        
        Log($"Spawned player {player.ActorNumber} at {spawnPoint.position}");
    }
    
    /// <summary>
    /// Get spawn point for player (for respawn).
    /// </summary>
    public Transform GetPlayerSpawnPoint(int actorNumber)
    {
        if (playerSpawnAssignments.ContainsKey(actorNumber))
        {
            return playerSpawnAssignments[actorNumber];
        }
        return null;
    }
    
    /// <summary>
    /// Respawn player at their spawn point (or last checkpoint for race).
    /// </summary>
    public void RespawnPlayer(int actorNumber, Transform customSpawnPoint = null)
    {
        Transform spawnPoint = customSpawnPoint ?? GetPlayerSpawnPoint(actorNumber);
        
        if (spawnPoint == null)
        {
            Log($"No spawn point found for player {actorNumber}!");
            return;
        }
        
        // Find player object
        GameObject playerObj = FindPlayerObject(actorNumber);
        if (playerObj != null)
        {
            // Reset position
            playerObj.transform.position = spawnPoint.position;
            playerObj.transform.rotation = spawnPoint.rotation;
            
            // Reset velocity if rigidbody
            var rb = playerObj.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            Log($"Respawned player {actorNumber} at {spawnPoint.position}");
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
    
    /// <summary>
    /// Apply character selection to spawned player.
    /// </summary>
    private void ApplyCharacterToPlayer(GameObject playerObj, Photon.Realtime.Player player)
    {
        if (CharacterSelectionManager.Instance == null) return;
        
        CharacterData characterData = CharacterSelectionManager.Instance.GetCharacterData(player);
        if (characterData == null) return;
        
        PlayerVisual playerVisual = playerObj.GetComponent<PlayerVisual>();
        if (playerVisual != null)
        {
            playerVisual.SetCharacter(characterData);
        }
    }
    
    /// <summary>
    /// Clear all spawn assignments (when match ends).
    /// </summary>
    public void ClearSpawnAssignments()
    {
        playerSpawnAssignments.Clear();
    }
    
    private void Log(string message)
    {
        if (logSpawnEvents)
        {
            Debug.Log($"[SpawnManager] {message}");
        }
    }
}
#else
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Photon not installed - stub implementation.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    
    [SerializeField] private List<Transform> raceSpawnPoints = new List<Transform>();
    [SerializeField] private GameObject playerPrefab;
    
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
        
        Debug.LogWarning("[SpawnManager] Photon not installed!");
    }
    
    public void SpawnAllPlayers(MatchState matchState) => Debug.LogWarning("Photon not installed!");
    public Transform GetPlayerSpawnPoint(int actorNumber) => null;
    public void RespawnPlayer(int actorNumber, Transform customSpawnPoint = null) { }
    public void ClearSpawnAssignments() { }
}
#endif

