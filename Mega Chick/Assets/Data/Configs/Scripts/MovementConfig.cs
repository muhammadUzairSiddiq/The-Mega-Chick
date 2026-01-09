using UnityEngine;

/// <summary>
/// Configuration for player movement and combat mechanics.
/// Separated from MatchConfig because movement is gameplay-specific,
/// not match-flow-specific. This allows different movement configs per mode.
/// </summary>
[CreateAssetMenu(fileName = "MovementConfig", menuName = "Mega Chick/Configs/Movement Config")]
public class MovementConfig : ScriptableObject
{
    [Header("Movement")]
    [Tooltip("Base movement speed (units per second)")]
    [Range(1f, 20f)]
    public float moveSpeed = 5f;
    
    [Tooltip("Rotation speed (degrees per second) for left/right turning")]
    [Range(30f, 360f)]
    public float rotationSpeed = 120f;
    
    [Header("Jump Settings")]
    [Tooltip("Force applied when jumping (controls jump height)")]
    [Range(3f, 15f)]
    public float jumpForce = 5f;
    
    [Tooltip("Jump total speed (how fast the jump completes) - lower = faster jump")]
    [Range(0.1f, 2f)]
    public float jumpTotalSpeed = 0.3f;
    
    [Tooltip("Jump height speed multiplier (affects vertical velocity) - lower = less high")]
    [Range(0.1f, 2f)]
    public float jumpHeightSpeed = 0.6f;
    
    [Tooltip("Gravity multiplier (1 = normal, 2 = double gravity)")]
    [Range(0.1f, 5f)]
    public float gravityScale = 1f;
    
    [Tooltip("Air control factor (0 = no control in air, 1 = full control)")]
    [Range(0f, 1f)]
    public float airControl = 0.5f;
    
    [Header("Combat")]
    [Tooltip("Knockback force applied on hit")]
    [Range(1f, 50f)]
    public float knockbackForce = 10f;
    
    [Tooltip("Cooldown between attacks in seconds")]
    [Range(0.1f, 2f)]
    public float attackCooldown = 0.5f;
    
    [Tooltip("Attack range/distance (how far player can hit)")]
    [Range(0.5f, 5f)]
    public float attackRange = 1.5f;
    
    [Tooltip("Attack damage (if we add health system later)")]
    [Min(0)]
    public int attackDamage = 1;
    
    [Header("Weapon Settings")]
    [Tooltip("Weapon knockback power (separate from basic attack)")]
    [Range(1f, 50f)]
    public float weaponKnockbackPower = 15f;
    
    [Tooltip("Weapon cooldown in seconds (3-6s recommended)")]
    [Range(3f, 6f)]
    public float weaponCooldown = 4f;
    
    [Header("Physics")]
    [Tooltip("Ground check distance (how far down to check for ground)")]
    [Range(0.01f, 1f)]
    public float groundCheckDistance = 0.1f;
    
    [Tooltip("Ground check radius (size of ground detection sphere)")]
    [Range(0.1f, 1f)]
    public float groundCheckRadius = 0.3f;
    
    [Tooltip("Layer mask for what counts as 'ground'")]
    public LayerMask groundLayerMask = 1; // Default layer
}

