using UnityEngine;

/// <summary>
/// Configuration for player respawn mechanics.
/// Separated because respawn rules might differ per mode:
/// - Race: respawn at checkpoint
/// - Arena: respawn at random spawn point
/// </summary>
[CreateAssetMenu(fileName = "RespawnConfig", menuName = "Mega Chick/Configs/Respawn Config")]
public class RespawnConfig : ScriptableObject
{
    [Header("Respawn Timing")]
    [Tooltip("Delay before respawning after KO (seconds)")]
    [Range(0f, 10f)]
    public float respawnDelay = 2f;
    
    [Tooltip("Invulnerability duration after respawn (seconds)")]
    [Range(0f, 10f)]
    public float invulnerableSeconds = 3f;
    
    [Header("Respawn Settings")]
    [Tooltip("Show respawn countdown to player")]
    public bool showRespawnCountdown = true;
    
    [Tooltip("Fade in duration after respawn (for visual effect)")]
    [Range(0f, 3f)]
    public float fadeInDuration = 1f;
    
    [Tooltip("Can player cancel respawn? (useful for spectate mode)")]
    public bool allowCancelRespawn = false;
    
    [Header("Visual Feedback")]
    [Tooltip("Flash effect during invulnerability")]
    public bool flashDuringInvulnerability = true;
    
    [Tooltip("Invulnerability flash interval (seconds)")]
    [Range(0.1f, 1f)]
    public float flashInterval = 0.2f;
}

