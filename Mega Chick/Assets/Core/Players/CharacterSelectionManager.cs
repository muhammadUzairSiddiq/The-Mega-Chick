#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

/// <summary>
/// Manages character selection for all players.
/// Why separate? Character selection is lobby-specific, not gameplay.
/// Stores selection in Photon player properties.
/// </summary>
public class CharacterSelectionManager : MonoBehaviourPunCallbacks
{
    public static CharacterSelectionManager Instance { get; private set; }
    
    [Header("Available Characters")]
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    
    [Header("Debug")]
    [SerializeField] private bool logSelectionEvents = true;
    
    // Photon property keys
    private const string CHARACTER_INDEX_KEY = "CharacterIndex";
    
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
        // Auto-load characters if list is empty (in Start to ensure it runs)
        if (availableCharacters == null || availableCharacters.Count == 0)
        {
            AutoLoadCharacters();
        }
    }
    
    /// <summary>
    /// Automatically load all CharacterData assets from project.
    /// </summary>
    private void AutoLoadCharacters()
    {
#if UNITY_EDITOR
        // Editor: Use AssetDatabase
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterData");
        availableCharacters = new List<CharacterData>();
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            CharacterData charData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (charData != null)
            {
                availableCharacters.Add(charData);
                Log($"Auto-loaded: {charData.characterName}");
            }
        }
        
        if (availableCharacters.Count > 0)
        {
            Log($"✅ Auto-loaded {availableCharacters.Count} characters!");
        }
        else
        {
            Log("⚠️ No CharacterData assets found! Create them in 'Assets/Data/Configs/Character Data/'");
        }
#else
        // Runtime: Use Resources (if you put characters in Resources folder)
        CharacterData[] chars = Resources.LoadAll<CharacterData>("CharacterData");
        availableCharacters = new List<CharacterData>(chars);
        if (availableCharacters.Count > 0)
        {
            Log($"✅ Loaded {availableCharacters.Count} characters from Resources!");
        }
#endif
    }
    
    /// <summary>
    /// Select character for local player.
    /// </summary>
    public void SelectCharacter(int characterIndex)
    {
        if (characterIndex < 0 || characterIndex >= availableCharacters.Count)
        {
            Log($"Invalid character index: {characterIndex}");
            return;
        }
        
        if (availableCharacters[characterIndex] == null)
        {
            Log($"Character at index {characterIndex} is null!");
            return;
        }
        
        if (!availableCharacters[characterIndex].isUnlocked)
        {
            Log($"Character {availableCharacters[characterIndex].characterName} is locked!");
            return;
        }
        
        // Store in Photon player properties
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props[CHARACTER_INDEX_KEY] = characterIndex;
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        
        Log($"Selected character: {availableCharacters[characterIndex].characterName}");
    }
    
    /// <summary>
    /// Get selected character index for a player.
    /// </summary>
    public int GetSelectedCharacterIndex(Player player)
    {
        if (player.CustomProperties.ContainsKey(CHARACTER_INDEX_KEY))
        {
            return (int)player.CustomProperties[CHARACTER_INDEX_KEY];
        }
        return 0; // Default to first character
    }
    
    /// <summary>
    /// Get character data for a player.
    /// </summary>
    public CharacterData GetCharacterData(Player player)
    {
        int index = GetSelectedCharacterIndex(player);
        if (index >= 0 && index < availableCharacters.Count)
        {
            return availableCharacters[index];
        }
        return availableCharacters.Count > 0 ? availableCharacters[0] : null;
    }
    
    /// <summary>
    /// Get character data by index.
    /// </summary>
    public CharacterData GetCharacterData(int index)
    {
        if (index >= 0 && index < availableCharacters.Count)
        {
            return availableCharacters[index];
        }
        return null;
    }
    
    /// <summary>
    /// Get all available characters.
    /// </summary>
    public List<CharacterData> GetAvailableCharacters()
    {
        return new List<CharacterData>(availableCharacters);
    }
    
    /// <summary>
    /// Get unlocked characters only.
    /// </summary>
    public List<CharacterData> GetUnlockedCharacters()
    {
        List<CharacterData> unlocked = new List<CharacterData>();
        foreach (var character in availableCharacters)
        {
            if (character != null && character.isUnlocked)
            {
                unlocked.Add(character);
            }
        }
        return unlocked;
    }
    
    // Photon callbacks
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey(CHARACTER_INDEX_KEY))
        {
            int newIndex = (int)changedProps[CHARACTER_INDEX_KEY];
            Log($"Player {targetPlayer.ActorNumber} changed character to index {newIndex}");
            // Fire event for UI update
            GameEventBus.FirePlayerEnteredRoom(targetPlayer.ActorNumber.ToString());
        }
    }
    
    private void Log(string message)
    {
        if (logSelectionEvents)
        {
            Debug.Log($"[CharacterSelectionManager] {message}");
        }
    }
}
#else
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Photon not installed - stub.
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance { get; private set; }
    
    [SerializeField] private List<CharacterData> availableCharacters = new List<CharacterData>();
    
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
        
        Debug.LogWarning("[CharacterSelectionManager] Photon not installed!");
    }
    
    public void SelectCharacter(int index) { }
    public int GetSelectedCharacterIndex(object player) => 0;
    public CharacterData GetCharacterData(object player) => null;
    public CharacterData GetCharacterData(int index) => null;
    public List<CharacterData> GetAvailableCharacters() => new List<CharacterData>();
    public List<CharacterData> GetUnlockedCharacters() => new List<CharacterData>();
}
#endif

