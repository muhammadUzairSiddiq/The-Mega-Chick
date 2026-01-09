using UnityEngine;

/// <summary>
/// AI Bot Controller - separate from PlayerController for AI-controlled players.
/// Uses same movement system but with AI input instead of player input.
/// 
/// WHY SEPARATE?
/// - PlayerController = Human input (WASD)
/// - AIBotController = AI input (pathfinding, decision making)
/// - Both use same MovementConfig and PlayerAnimatorController
/// - Easy to customize AI behavior without touching player code
/// 
/// USAGE:
/// - Add to bot/NPC GameObjects instead of PlayerController
/// - AI logic handles input, this handles movement/animations
/// - Works universally for all game modes
/// </summary>
public class AIBotController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private MovementConfig movementConfig;
    
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private PlayerAnimatorController animatorController;
    
    [Header("AI Input")]
    [Tooltip("AI-controlled movement input (set by AI logic)")]
    [SerializeField] private Vector2 aiMoveInput;
    
    [Tooltip("AI-controlled jump input (set by AI logic)")]
    [SerializeField] private bool aiJumpInput;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    
    // State
    private bool isGrounded;
    private bool wasGrounded;
    private bool isJumping = false;
    
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        
        if (rb == null)
        {
            Debug.LogError("[AIBotController] Rigidbody component required!");
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
    }
    
    private void Start()
    {
        // Apply gravity scale
        if (rb != null && movementConfig != null)
        {
            rb.useGravity = false; // We'll handle gravity manually
        }
        
        // Set idle animation on spawn
        if (animatorController != null)
        {
            animatorController.SetIdle();
        }
        
        wasGrounded = true;
    }
    
    private void Update()
    {
        // Check ground
        CheckGrounded();
        
        // Handle jump
        if (aiJumpInput && isGrounded)
        {
            Jump();
            isJumping = true;
        }
        
        // Reset jump flag when grounded
        if (isGrounded && wasGrounded)
        {
            isJumping = false;
        }
        wasGrounded = isGrounded;
        
        // Update animations
        if (animatorController != null)
        {
            bool justJumped = false; // AI doesn't track justJumped separately
            bool isSprinting = false; // AI doesn't sprint
            bool isInJumpState = isJumping; // Use isJumping as jump state
            bool jumpHeld = isJumping; // AI holds jump while jumping
            animatorController.UpdateAnimations(aiMoveInput, isGrounded, justJumped, isSprinting, isInJumpState, jumpHeld);
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
    /// Set AI movement input (called by AI logic).
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        aiMoveInput = input;
    }
    
    /// <summary>
    /// Set AI jump input (called by AI logic).
    /// </summary>
    public void SetJumpInput(bool jump)
    {
        aiJumpInput = jump;
    }
    
    /// <summary>
    /// Get current movement input (for AI logic).
    /// </summary>
    public Vector2 GetMoveInput()
    {
        return aiMoveInput;
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
        
        isGrounded = Physics.CheckSphere(
            groundCheck.position,
            movementConfig.groundCheckRadius,
            movementConfig.groundLayerMask
        );
    }
    
    /// <summary>
    /// Apply gravity manually.
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
    /// Apply movement based on AI input.
    /// </summary>
    private void ApplyMovement()
    {
        if (movementConfig == null) return;
        
        Vector3 moveDirection = new Vector3(aiMoveInput.x, 0f, aiMoveInput.y).normalized;
        
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
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || movementConfig == null) return;
        
        // Ground check gizmo
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, movementConfig.groundCheckRadius);
        }
    }
}

