#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // For Hashtable

/// <summary>
/// Manages game mode selection for all players, storing choices in Photon player properties.
/// Similar to CharacterSelectionManager but for game modes.
/// </summary>
public class GameModeSelectionManager : MonoBehaviourPunCallbacks
{
    public static GameModeSelectionManager Instance { get; private set; }
    
    [Header("Debug")]
    [SerializeField] private bool logSelectionEvents = true;
    
    // Photon property key
    private const string GAME_MODE_INDEX_KEY = "GameModeIndex";
    
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
    /// Select game mode for local player and update Photon properties.
    /// </summary>
    public void SelectGameMode(int modeIndex)
    {
        Log($"🖱️ [ACTION] Attempting to select game mode index: {modeIndex}");
        
        // Store in Photon player properties
        Hashtable props = new Hashtable();
        props[GAME_MODE_INDEX_KEY] = modeIndex;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        
        Log($"✅ [SELECT] Local player selected game mode index: {modeIndex}");
    }
    
    /// <summary>
    /// Get selected game mode index for a player.
    /// </summary>
    public int GetSelectedGameModeIndex(Player player)
    {
        if (player == null)
        {
            Log("❌ [ERROR] GetSelectedGameModeIndex called with NULL player!");
            return 0;
        }
        
        if (player.CustomProperties.ContainsKey(GAME_MODE_INDEX_KEY))
        {
            int index = (int)player.CustomProperties[GAME_MODE_INDEX_KEY];
            Log($"🔍 [GET] Player '{player.NickName}' selected game mode index: {index}");
            return index;
        }
        Log($"⚠️ [GET] Player '{player.NickName}' has no game mode selected, returning default index 0.");
        return 0; // Default to first mode
    }
    
    /// <summary>
    /// Get selected game mode name for a player (requires GameModeSelectionUI to get mode names).
    /// </summary>
    public string GetSelectedGameModeName(Player player)
    {
        int index = GetSelectedGameModeIndex(player);
        
        // Try to get mode name from GameModeSelectionUI
        GameModeSelectionUI ui = FindObjectOfType<GameModeSelectionUI>();
        if (ui != null)
        {
            // Use reflection to get gameModes list
            var gameModesField = typeof(GameModeSelectionUI).GetField("gameModes", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var gameModes = gameModesField?.GetValue(ui) as System.Collections.Generic.List<GameModeSelectionUI.GameMode>;
            
            if (gameModes != null && index >= 0 && index < gameModes.Count)
            {
                string modeName = gameModes[index].modeName;
                Log($"✅ [GET] Player '{player.NickName}' game mode: {modeName} (index {index})");
                return modeName;
            }
            else
            {
                Log($"⚠️ [GET] Invalid game mode index {index} for player '{player.NickName}' (gameModes.Count = {gameModes?.Count ?? 0})");
            }
        }
        else
        {
            Log($"⚠️ [GET] GameModeSelectionUI not found in scene!");
        }
        
        // Fallback: return index as string
        return $"Mode {index}";
    }
    
    // Photon callbacks
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(GAME_MODE_INDEX_KEY))
        {
            int newIndex = (int)changedProps[GAME_MODE_INDEX_KEY];
            Log($"🎉 [EVENT] OnPlayerPropertiesUpdate: Player '{targetPlayer.NickName}' changed game mode to index {newIndex}");
        }
    }
    
    private void Log(string message)
    {
        if (logSelectionEvents)
        {
            Debug.Log($"[GameModeSelectionManager] {message}");
        }
    }
}
#else
// Placeholder for when PUN is not installed
using UnityEngine;
using Photon.Realtime;
public class GameModeSelectionManager : MonoBehaviour
{
    public static GameModeSelectionManager Instance { get; private set; }
    private void Awake() { if (Instance == null) Instance = this; else Destroy(gameObject); }
    public void SelectGameMode(int modeIndex) { Debug.Log($"[GameModeSelectionManager] (Offline) Selected game mode index: {modeIndex}"); }
    public int GetSelectedGameModeIndex(object player) => 0;
    public string GetSelectedGameModeName(object player) => "Mode 0";
}
#endif

