using UnityEngine;

/// <summary>
/// Configuration for Photon networking settings.
/// Why separate? Network settings are technical and might need
/// different values for testing vs production.
/// </summary>
[CreateAssetMenu(fileName = "NetworkConfig", menuName = "Mega Chick/Configs/Network Config")]
public class NetworkConfig : ScriptableObject
{
    [Header("Photon Settings")]
    [Tooltip("Send rate (updates per second). Higher = smoother but more bandwidth")]
    [Range(10, 30)]
    public int sendRate = 20;
    
    [Tooltip("Serialization rate (how often to sync data). Lower = less bandwidth")]
    [Range(5, 20)]
    public int serializationRate = 10;
    
    [Header("Interpolation")]
    [Tooltip("Interpolation factor for smooth movement (0 = no interpolation, 1 = full)")]
    [Range(0f, 1f)]
    public float interpolationFactor = 0.1f;
    
    [Tooltip("Lerp speed for position sync (higher = snappier, lower = smoother)")]
    [Range(1f, 50f)]
    public float positionLerpSpeed = 10f;
    
    [Header("Room Settings")]
    [Tooltip("Maximum players per room")]
    [Range(2, 8)]
    public int maxPlayersPerRoom = 8;
    
    [Tooltip("Room code length (4-8 characters recommended)")]
    [Range(4, 8)]
    public int roomCodeLength = 6;
    
    [Tooltip("Room visibility (true = public, false = private/friends only)")]
    public bool isVisible = true;
    
    [Tooltip("Room open for joining (false = room full)")]
    public bool isOpen = true;
    
    [Header("Debug")]
    [Tooltip("Show network debug info in UI")]
    public bool showDebugInfo = false;
    
    [Tooltip("Log all network events (verbose, use for debugging only)")]
    public bool verboseLogging = false;
}

