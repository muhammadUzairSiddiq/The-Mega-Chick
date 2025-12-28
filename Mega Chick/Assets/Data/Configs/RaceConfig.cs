using UnityEngine;

/// <summary>
/// Configuration specific to Race mode.
/// Why separate? Each mode might have unique rules.
/// This keeps mode-specific configs isolated.
/// </summary>
[CreateAssetMenu(fileName = "RaceConfig", menuName = "Mega Chick/Configs/Race Config")]
public class RaceConfig : ScriptableObject
{
    [Header("Checkpoints")]
    [Tooltip("Require all checkpoints before finishing (true) or just reach finish (false)")]
    public bool checkpointRequired = true;
    
    [Tooltip("Allow skipping checkpoints (for shortcuts - might be disabled)")]
    public bool allowCheckpointSkip = false;
    
    [Header("Scoring")]
    [Tooltip("Points for first place")]
    [Min(0)]
    public int pointsFirst = 10;
    
    [Tooltip("Points for second place")]
    [Min(0)]
    public int pointsSecond = 7;
    
    [Tooltip("Points for third place")]
    [Min(0)]
    public int pointsThird = 5;
    
    [Tooltip("Points for other placements")]
    [Min(0)]
    public int pointsOthers = 3;
    
    [Header("Race Settings")]
    [Tooltip("Race time limit in seconds (0 = no limit)")]
    [Min(0)]
    public float raceTimeLimit = 0f;
    
    [Tooltip("Respawn at last checkpoint (true) or start line (false)")]
    public bool respawnAtLastCheckpoint = true;
    
    [Tooltip("Allow respawn during race (false = one life)")]
    public bool allowRespawn = true;
    
    [Header("Placement Rules")]
    [Tooltip("How to rank players who didn't finish: by checkpoint progress, then distance")]
    public bool rankByProgress = true;
    
    [Tooltip("Minimum checkpoint progress to qualify for ranking (0-1, 1 = must finish)")]
    [Range(0f, 1f)]
    public float minProgressToRank = 0f;
}

