using UnityEngine;

/// <summary>
/// Kill volume - triggers KO when player enters.
/// Why separate component? Reusable, easy to place in scene.
/// </summary>
public class KillVolume : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool respawnOnKO = true;
    [SerializeField] private float respawnDelay = 2f;
    
    [Header("Debug")]
    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.3f);
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if it's a player
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            OnPlayerEnteredKillVolume(player);
        }
    }
    
    /// <summary>
    /// Handle player entering kill volume.
    /// </summary>
    private void OnPlayerEnteredKillVolume(PlayerController player)
    {
        Debug.Log($"[KillVolume] Player {player.name} entered kill volume!");
        
        // Trigger KO event
        int actorNumber = GetPlayerActorNumber(player);
        if (actorNumber > 0)
        {
            GameEventBus.FirePlayerKOed(actorNumber);
        }
        
        // Handle respawn if enabled
        if (respawnOnKO)
        {
            StartCoroutine(RespawnPlayerDelayed(player, respawnDelay));
        }
    }
    
    /// <summary>
    /// Get player actor number (for networking).
    /// </summary>
    private int GetPlayerActorNumber(PlayerController player)
    {
#if PUN_2_OR_NEWER
        Photon.Pun.PhotonView pv = player.GetComponent<Photon.Pun.PhotonView>();
        if (pv != null && pv.Owner != null)
        {
            return pv.Owner.ActorNumber;
        }
#endif
        return 0;
    }
    
    /// <summary>
    /// Respawn player after delay.
    /// </summary>
    private System.Collections.IEnumerator RespawnPlayerDelayed(PlayerController player, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (player != null && SpawnManager.Instance != null)
        {
            int actorNumber = GetPlayerActorNumber(player);
            if (actorNumber > 0)
            {
                SpawnManager.Instance.RespawnPlayer(actorNumber);
                GameEventBus.FirePlayerRespawned(actorNumber);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;
        
        Gizmos.color = gizmoColor;
        
        if (col is BoxCollider)
        {
            BoxCollider box = col as BoxCollider;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider)
        {
            SphereCollider sphere = col as SphereCollider;
            Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
        }
    }
}

