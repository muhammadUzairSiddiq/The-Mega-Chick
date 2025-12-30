using UnityEngine;

/// <summary>
/// Configuration for match flow and timing.
/// Why ScriptableObject? So designers can tweak values without touching code.
/// </summary>
[CreateAssetMenu(fileName = "MatchConfig", menuName = "Mega Chick/Configs/Match Config")]
public class MatchConfig : ScriptableObject
{
    [Header("Match Flow")]
    [Tooltip("Countdown duration before match starts (in seconds)")]
    [Range(1, 10)]
    public int countdownSeconds = 3;
    
    [Tooltip("Default round duration in seconds (0 = no time limit)")]
    [Min(0)]
    public int defaultRoundSeconds = 300;
    
    [Tooltip("Minimum players required to start a match")]
    [Range(1, 8)]
    public int minPlayersToStart = 2;
    
    [Header("Match Settings")]
    [Tooltip("Enable rematch option after results screen")]
    public bool allowRematch = true;
    
    [Tooltip("How long to show results screen before returning to lobby (seconds)")]
    [Min(0)]
    public float resultsDisplayDuration = 10f;
    
    [Tooltip("Auto-start match when all players ready (if false, master must click Start)")]
    public bool autoStartWhenReady = false;
    
    [Header("Respawn")]
    [Tooltip("Respawn delay range (0.5-4s configurable per mode)")]
    [Range(0.5f, 4f)]
    public float respawnDelay = 2f;
}

