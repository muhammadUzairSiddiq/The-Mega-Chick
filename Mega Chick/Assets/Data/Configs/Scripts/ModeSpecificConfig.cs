using UnityEngine;

/// <summary>
/// Configuration for mode-specific settings.
/// Contains settings for FFA, Zone, Hunter modes.
/// </summary>
[CreateAssetMenu(fileName = "ModeSpecificConfig", menuName = "Mega Chick/Configs/Mode Specific Config")]
public class ModeSpecificConfig : ScriptableObject
{
    [Header("FFA Mode (Chick Rumble)")]
    [Tooltip("KO credit window in seconds (last-hit tracker)")]
    [Range(1f, 10f)]
    public float koCreditWindow = 5f;
    
    [Tooltip("Penalty for self-KO (-1 point, off by default)")]
    public bool enableSelfKOPenalty = false;
    
    [Tooltip("Self-KO penalty points")]
    [Min(0)]
    public int selfKOPenalty = 1;
    
    [Header("Zone Control Mode")]
    [Tooltip("Zone tick interval in seconds")]
    [Range(0.5f, 5f)]
    public float zoneTickInterval = 1f;
    
    [Header("Hunter Mode (King Mega Chick)")]
    [Tooltip("Hunter duration for 2 players (seconds)")]
    [Range(5f, 30f)]
    public float hunterDuration2Players = 20f;
    
    [Tooltip("Hunter duration for 3-4 players (seconds)")]
    [Range(5f, 30f)]
    public float hunterDuration3to4Players = 15f;
    
    [Tooltip("Hunter duration for 5-8 players (seconds)")]
    [Range(5f, 30f)]
    public float hunterDuration5to8Players = 10f;
    
    [Header("Hunter Buffs (Menu Configurable On/Off)")]
    [Tooltip("Hunter gets higher knockback")]
    public bool hunterHigherKnockback = true;
    
    [Tooltip("Hunter gets speed boost")]
    public bool hunterSpeedBoost = true;
    
    [Tooltip("Hunter gets larger attack hitbox")]
    public bool hunterLargerHitbox = true;
    
    [Tooltip("Hunter invincible to knockback")]
    public bool hunterInvincibleToKnockback = true;
}

