#if PUN_2_OR_NEWER
using UnityEngine;
using Photon.Pun;

/// <summary>
/// Network synchronization for player position/rotation.
/// Why separate? Network sync isolated from gameplay logic.
/// Uses Photon Transform View for smooth interpolation.
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerNetworkSync : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Sync Settings")]
    [SerializeField] private bool syncPosition = true;
    [SerializeField] private bool syncRotation = true;
    [SerializeField] private float positionLerpSpeed = 10f;
    
    private Vector3 networkPosition;
    private Quaternion networkRotation;
    private Rigidbody rb;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    private void Update()
    {
        // Only interpolate if this is NOT our local player
        if (!photonView.IsMine)
        {
            InterpolatePosition();
            InterpolateRotation();
        }
    }
    
    /// <summary>
    /// Interpolate position for smooth movement.
    /// </summary>
    private void InterpolatePosition()
    {
        if (!syncPosition) return;
        
        if (rb != null)
        {
            // Use rigidbody for physics-based movement
            rb.position = Vector3.Lerp(rb.position, networkPosition, Time.deltaTime * positionLerpSpeed);
        }
        else
        {
            // Use transform for non-physics movement
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * positionLerpSpeed);
        }
    }
    
    /// <summary>
    /// Interpolate rotation for smooth turning.
    /// </summary>
    private void InterpolateRotation()
    {
        if (!syncRotation) return;
        
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * positionLerpSpeed);
    }
    
    /// <summary>
    /// Photon serialization - send/receive data.
    /// </summary>
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player - send our data
            if (syncPosition)
            {
                stream.SendNext(rb != null ? rb.position : transform.position);
            }
            if (syncRotation)
            {
                stream.SendNext(transform.rotation);
            }
        }
        else
        {
            // We're receiving data from another player
            if (syncPosition)
            {
                networkPosition = (Vector3)stream.ReceiveNext();
            }
            if (syncRotation)
            {
                networkRotation = (Quaternion)stream.ReceiveNext();
            }
        }
    }
}
#else
using UnityEngine;

/// <summary>
/// Photon not installed - stub.
/// </summary>
public class PlayerNetworkSync : MonoBehaviour
{
    private void Awake()
    {
        Debug.LogWarning("[PlayerNetworkSync] Photon not installed!");
    }
}
#endif

