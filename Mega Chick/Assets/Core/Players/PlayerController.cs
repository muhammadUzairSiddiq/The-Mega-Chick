using UnityEngine;

/// <summary>
/// Base player controller - handles input and movement.
/// Why separate from network? Local input handling, smooth movement.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private MovementConfig movementConfig;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundCheck;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    
    // Input
    private Vector2 moveInput;
    private bool jumpInput;
    private bool attackInput;
    
    // State
    private bool isGrounded;
    private float lastAttackTime;
    
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        
        if (rb == null)
        {
            Debug.LogError("[PlayerController] Rigidbody component required!");
        }
    }
    
    private void Start()
    {
        // Apply gravity scale
        if (rb != null && movementConfig != null)
        {
            rb.useGravity = false; // We'll handle gravity manually
        }
    }
    
    private void Update()
    {
        // Get input (will be replaced with Input System later)
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        jumpInput = Input.GetKeyDown(KeyCode.Space);
        attackInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return);
        
        // Check ground
        CheckGrounded();
        
        // Handle input
        if (jumpInput && isGrounded)
        {
            Jump();
        }
        
        if (attackInput)
        {
            TryAttack();
        }
    }
    
    private void FixedUpdate()
    {
        if (rb == null || movementConfig == null) return;
        
        // Apply gravity
        ApplyGravity();
        
        // Apply movement
        ApplyMovement();
    }
    
    /// <summary>
    /// Check if player is grounded.
    /// </summary>
    private void CheckGrounded()
    {
        if (groundCheck == null || movementConfig == null)
        {
            isGrounded = false;
            return;
        }
        
        float checkDistance = movementConfig.groundCheckDistance + movementConfig.groundCheckRadius;
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            movementConfig.groundCheckRadius,
            movementConfig.groundLayerMask
        );
    }
    
    /// <summary>
    /// Apply gravity manually (for gravity scale control).
    /// </summary>
    private void ApplyGravity()
    {
        if (movementConfig == null) return;
        
        if (!isGrounded)
        {
            rb.linearVelocity += Vector3.down * Physics.gravity.magnitude * movementConfig.gravityScale * Time.fixedDeltaTime;
        }
    }
    
    /// <summary>
    /// Apply movement based on input.
    /// </summary>
    private void ApplyMovement()
    {
        if (movementConfig == null) return;
        
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        // Air control factor
        float controlFactor = isGrounded ? 1f : movementConfig.airControl;
        
        // Calculate movement
        Vector3 movement = moveDirection * movementConfig.moveSpeed * controlFactor;
        
        // Apply horizontal movement (preserve Y velocity for jumping/falling)
        rb.linearVelocity = new Vector3(movement.x, rb.linearVelocity.y, movement.z);
        
        // Face movement direction
        if (moveDirection.magnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }
    
    /// <summary>
    /// Jump action.
    /// </summary>
    private void Jump()
    {
        if (movementConfig == null) return;
        
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, movementConfig.jumpForce, rb.linearVelocity.z);
    }
    
    /// <summary>
    /// Try to attack (with cooldown).
    /// </summary>
    private void TryAttack()
    {
        if (movementConfig == null) return;
        
        if (Time.time - lastAttackTime < movementConfig.attackCooldown)
        {
            return; // On cooldown
        }
        
        lastAttackTime = Time.time;
        PerformAttack();
    }
    
    /// <summary>
    /// Perform attack - check for hits and apply knockback.
    /// </summary>
    private void PerformAttack()
    {
        if (movementConfig == null) return;
        
        // Overlap sphere in front of player
        Vector3 attackPosition = transform.position + transform.forward * movementConfig.attackRange;
        Collider[] hits = Physics.OverlapSphere(attackPosition, movementConfig.attackRange);
        
        foreach (Collider hit in hits)
        {
            // Don't hit self
            if (hit.transform == transform) continue;
            
            // Check if it's a player
            PlayerController otherPlayer = hit.GetComponent<PlayerController>();
            if (otherPlayer != null)
            {
                ApplyKnockback(otherPlayer);
            }
        }
    }
    
    /// <summary>
    /// Apply knockback to another player.
    /// </summary>
    private void ApplyKnockback(PlayerController target)
    {
        if (movementConfig == null || target == null) return;
        
        Vector3 knockbackDirection = (target.transform.position - transform.position).normalized;
        knockbackDirection.y = 0.5f; // Add upward component
        
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            targetRb.AddForce(knockbackDirection * movementConfig.knockbackForce, ForceMode.Impulse);
        }
        
        Debug.Log($"[PlayerController] {gameObject.name} hit {target.gameObject.name}!");
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || movementConfig == null) return;
        
        // Ground check gizmo
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, movementConfig.groundCheckRadius);
        }
        
        // Attack range gizmo
        Gizmos.color = Color.yellow;
        Vector3 attackPos = transform.position + transform.forward * movementConfig.attackRange;
        Gizmos.DrawWireSphere(attackPos, movementConfig.attackRange);
    }
}

