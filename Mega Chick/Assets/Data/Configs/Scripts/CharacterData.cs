using UnityEngine;

/// <summary>
/// Character data - stores character info, prefab, abilities.
/// Why ScriptableObject? Easy to create multiple characters, designer-friendly.
/// Simple but extendable - can add more properties later.
/// </summary>
[CreateAssetMenu(fileName = "CharacterData", menuName = "Mega Chick/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Character name")]
    public string characterName = "Character";
    
    [Tooltip("Character description")]
    [TextArea(2, 4)]
    public string description = "A character description";
    
    [Tooltip("Character icon/sprite for selection UI")]
    public Sprite icon;
    
    [Header("Character Model")]
    [Tooltip("Character prefab/model to use")]
    public GameObject characterPrefab;
    
    [Header("Abilities/Stats")]
    [Tooltip("Speed multiplier (1 = normal, >1 = faster)")]
    [Range(0.5f, 2f)]
    public float speedMultiplier = 1f;
    
    [Tooltip("Jump power multiplier")]
    [Range(0.5f, 2f)]
    public float jumpMultiplier = 1f;
    
    [Tooltip("Knockback resistance (0 = no resistance, 1 = full resistance)")]
    [Range(0f, 1f)]
    public float knockbackResistance = 0f;
    
    [Header("Special Abilities")]
    [Tooltip("Special ability description (shown in selection UI)")]
    public string specialAbility = "None";
    
    [Tooltip("Is this character unlocked?")]
    public bool isUnlocked = true;
}

