using UnityEngine;
using System.Collections;

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
    [SerializeField] private PlayerAnimatorController animatorController;
    
    /// <summary>
    /// Set animator controller (called by PlayerVisual when character loads).
    /// </summary>
    public void SetAnimatorController(PlayerAnimatorController controller)
    {
        animatorController = controller;
    }
    
    [Header("Camera")]
    [Tooltip("Camera reference for camera-relative movement (auto-finds Main Camera if null)")]
    [SerializeField] private Camera playerCamera;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    
    // Input
    private Vector2 moveInput;
    private bool jumpInput;
    private bool jumpHeld;
    private bool attackInput;
    
    // State
    private bool isGrounded;
    private bool wasGrounded; // Track previous grounded state
    private float lastAttackTime;
    private float gravityDisabledTimer = 0f;
    private const float GRAVITY_DISABLE_DURATION = 0.3f; // Quick jump - 0.3 seconds
    private bool isSprinting = false;
    private bool isInJumpState = false; // Track if player is in jump/air state
    private const float ROTATION_SPEED = 5f; // Smooth human-like rotation speed
    private const float SPRINT_SPEED_MULTIPLIER = 1.5f; // Sprint is 1.5x normal speed
    private const float FALL_GRAVITY_MULTIPLIER = 2.5f; // Faster fall for realistic jump
    private bool inputEnabled = false; // Control input during intro sequence - DISABLED BY DEFAULT
    
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
        
        // Auto-find animator controller
        if (animatorController == null)
        {
            animatorController = GetComponent<PlayerAnimatorController>();
            if (animatorController == null)
            {
                animatorController = GetComponentInChildren<PlayerAnimatorController>();
            }
        }
        
        // Auto-find camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }
    
    private void Start()
    {
        InitializePlayer();
    }
    
    /// <summary>
    /// Initialize player - called on Start and when character loads.
    /// </summary>
    private void InitializePlayer()
    {
        // Apply gravity scale
        if (rb != null && movementConfig != null)
        {
            // ENABLE Unity's automatic gravity (default)
            rb.useGravity = true;
            
            // FREEZE ALL ROTATION (X, Y, Z) - prevent any rotation
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            
            // CRITICAL: Ensure character starts with zero velocity
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            // Ensure rigidbody is not kinematic
            rb.isKinematic = false;
            
            Debug.Log($"[PlayerController] ✅ Initialized rigidbody - Gravity: {rb.useGravity}, Constraints: {rb.constraints}");
        }
        else
        {
            Debug.LogWarning($"[PlayerController] ⚠️ Cannot initialize - rb: {rb != null}, config: {movementConfig != null}");
        }
        
        // Initialize grounded state
        wasGrounded = true;
        isInJumpState = false;
        
        // Set idle animation on spawn (ID=1)
        if (animatorController != null)
        {
            animatorController.SetIdle();
        }
        else
        {
            // Try to find animator controller if not found yet (character might load later)
            StartCoroutine(DelayedIdleSet());
        }
    }
    
    /// <summary>
    /// Called when character model is loaded - re-initialize if needed.
    /// </summary>
    public void OnCharacterLoaded()
    {
        // Re-initialize player when character loads (ensures gravity/controls work)
        InitializePlayer();
    }
    
    /// <summary>
    /// Enable/disable player input (used during intro sequence).
    /// </summary>
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
        {
            // Reset inputs when disabled
            moveInput = Vector2.zero;
            isSprinting = false;
        }
    }
    
    /// <summary>
    /// Check if input is enabled.
    /// </summary>
    public bool IsInputEnabled() => inputEnabled;
    
    /// <summary>
    /// Delayed idle set for when animator loads after Start.
    /// </summary>
    private IEnumerator DelayedIdleSet()
    {
        yield return new WaitForSeconds(0.1f); // Wait a bit for character to load
        if (animatorController == null)
        {
            animatorController = GetComponentInChildren<PlayerAnimatorController>();
        }
        if (animatorController != null)
        {
            animatorController.SetIdle();
        }
    }
    
    private void Update()
    {
        // Re-find animator controller if not found (character might load later)
        if (animatorController == null)
        {
            animatorController = GetComponentInChildren<PlayerAnimatorController>();
        }
        
        // Update gravity disable timer
        if (gravityDisabledTimer > 0f)
        {
            gravityDisabledTimer -= Time.deltaTime;
            if (gravityDisabledTimer <= 0f)
            {
                // Re-enable gravity after jump period
                if (rb != null)
                {
                    rb.useGravity = true;
                }
            }
        }
        
        // Get input (WASD + Space + Shift) - ONLY if input is enabled
        if (inputEnabled)
        {
            moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            // REMOVE S key (backward) - clamp Y to 0 minimum (only forward movement)
            if (moveInput.y < 0f)
            {
                moveInput.y = 0f;
            }
            
            // Sprint check: SHIFT + W (forward)
            isSprinting = Input.GetKey(KeyCode.LeftShift) && moveInput.y > 0.1f;
            
            jumpInput = Input.GetKeyDown(KeyCode.Space);
            jumpHeld = Input.GetKey(KeyCode.Space);
            attackInput = Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return);
        }
        else
        {
            // Input disabled (during intro) - zero all inputs
            moveInput = Vector2.zero;
            isSprinting = false;
            jumpInput = false;
            jumpHeld = false;
            attackInput = false;
        }
        
        // Check ground
        CheckGrounded();
        
        // Track jump state - player is in air if not grounded
        if (!isGrounded)
        {
            isInJumpState = true; // In air = jump state
        }
        else if (isGrounded)
        {
            // Grounded - exit jump state and clear jump animation timer
            if (wasGrounded == false)
            {
                // Just landed - exit jump state
                isInJumpState = false;
                // Clear jump animation timer when landing
                if (animatorController != null)
                {
                    animatorController.ClearJumpAnimation();
                }
            }
            else
            {
                // Already grounded - make sure jump state is false
                isInJumpState = false;
            }
        }
        wasGrounded = isGrounded;
        
        // Handle jump (only when grounded)
        bool justJumped = false;
        if (jumpInput && isGrounded)
        {
            Jump();
            justJumped = true;
            isInJumpState = true; // Enter jump state
        }
        
        // Update animations: Only jump animation during jump state, otherwise run/idle/sprint
        if (animatorController != null)
        {
            animatorController.UpdateAnimations(moveInput, isGrounded, justJumped, isSprinting, isInJumpState);
        }
        
        // Handle attack
        if (attackInput)
        {
            TryAttack();
        }
    }
    
    private void FixedUpdate()
    {
        if (rb == null || movementConfig == null) return;
        
        // Apply enhanced gravity when falling (more realistic jump)
        ApplyEnhancedGravity();
        
        // Apply movement (Unity handles gravity automatically, except during jump)
        ApplyMovement();
    }
    
    /// <summary>
    /// Apply enhanced gravity when falling for more realistic jump.
    /// </summary>
    private void ApplyEnhancedGravity()
    {
        if (rb == null) return;
        
        // If player is falling (negative Y velocity) and not in jump disable period, increase gravity
        if (rb.linearVelocity.y < 0 && gravityDisabledTimer <= 0f)
        {
            // Apply extra gravity for faster fall
            rb.linearVelocity += Vector3.down * (Physics.gravity.magnitude * (FALL_GRAVITY_MULTIPLIER - 1f)) * Time.fixedDeltaTime;
        }
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
    /// Apply movement based on input (camera-relative WASD).
    /// CRITICAL: Movement ONLY affects X and Z axes, NEVER Y.
    /// </summary>
    private void ApplyMovement()
    {
        if (movementConfig == null || rb == null) return;
        
        // Get current Y velocity (for jump/gravity) - PRESERVE THIS
        float currentYVelocity = rb.linearVelocity.y;
        
        // If no input, only preserve Y velocity, zero X and Z
        if (moveInput.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, currentYVelocity, 0);
            return;
        }
        
        // Get camera forward and right vectors
        Vector3 cameraForward = Vector3.forward;
        Vector3 cameraRight = Vector3.right;
        
        if (playerCamera != null)
        {
            cameraForward = playerCamera.transform.forward;
            cameraRight = playerCamera.transform.right;
        }
        
        // CRITICAL: Flatten to ground plane (remove Y component completely)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        // Normalize after flattening
        float forwardMag = cameraForward.magnitude;
        float rightMag = cameraRight.magnitude;
        
        if (forwardMag > 0.01f)
        {
            cameraForward /= forwardMag; // Normalize manually
        }
        else
        {
            cameraForward = Vector3.forward;
        }
        
        if (rightMag > 0.01f)
        {
            cameraRight /= rightMag; // Normalize manually
        }
        else
        {
            cameraRight = Vector3.right;
        }
        
        // Calculate movement direction (horizontal only) - NO Y COMPONENT
        Vector3 moveDirection = (cameraForward * moveInput.y + cameraRight * moveInput.x);
        moveDirection.y = 0f; // Force Y to zero BEFORE normalizing
        
        // Normalize horizontal direction
        float moveMag = moveDirection.magnitude;
        if (moveMag > 0.1f)
        {
            moveDirection /= moveMag; // Normalize
            moveDirection.y = 0f; // Force Y to zero AGAIN after normalize
        }
        else
        {
            // No movement
            rb.linearVelocity = new Vector3(0, currentYVelocity, 0);
            return;
        }
        
        // Air control factor
        float controlFactor = isGrounded ? 1f : movementConfig.airControl;
        
        // Calculate horizontal velocity (X and Z ONLY, Y is ZERO)
        float baseSpeed = movementConfig.moveSpeed;
        // Apply sprint multiplier if sprinting
        if (isSprinting && isGrounded)
        {
            baseSpeed *= SPRINT_SPEED_MULTIPLIER;
        }
        float moveSpeed = baseSpeed * controlFactor;
        
        // Smooth movement - use smooth damping for human-like feel
        Vector3 targetVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            0f, // Y is ALWAYS zero
            moveDirection.z * moveSpeed
        );
        
        // Smoothly interpolate to target velocity (only horizontal)
        Vector3 currentHorizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        Vector3 smoothVelocity = Vector3.Lerp(currentHorizontalVel, targetVelocity, Time.fixedDeltaTime * 8f);
        
        // CRITICAL: Apply ONLY horizontal movement, preserve Y velocity from gravity/jump
        rb.linearVelocity = new Vector3(smoothVelocity.x, currentYVelocity, smoothVelocity.z);
        
        // Face movement direction (only if moving) - SMOOTH HUMAN-LIKE ROTATION
        if (moveMag > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * ROTATION_SPEED);
        }
    }
    
    /// <summary>
    /// Jump action - apply upward force and disable gravity temporarily.
    /// </summary>
    private void Jump()
    {
        if (movementConfig == null || rb == null) return;
        
        // Apply jump force
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, movementConfig.jumpForce, rb.linearVelocity.z);
        
        // DISABLE gravity for 1-2 seconds during jump
        rb.useGravity = false;
        gravityDisabledTimer = GRAVITY_DISABLE_DURATION;
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

