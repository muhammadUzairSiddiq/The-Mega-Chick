using UnityEngine;

/// <summary>
/// Universal animator controller for all game modes.
/// Handles animation parameters based on movement input.
/// Works with character animator controllers that use integer animation parameter.
/// 
/// ANIMATION MAPPING:
/// - Idle: animation = 0 (default)
/// - W (Forward Run): animation = 1
/// - A (Left Run): animation = 2
/// - D (Right Run): animation = 3
/// - S (Back Run): animation = 4
/// - Space (Jump): animation = 5
/// 
/// This system is universal - works in Race, Arena, and all game modes.
/// </summary>
public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Animator")]
    [Tooltip("Animator component (auto-found if not assigned)")]
    [SerializeField] private Animator animator;
    
    [Header("Animation IDs")]
    [Tooltip("Animation ID for idle state")]
    [SerializeField] private int idleAnimationID = 1; // Default idle animation ID
    
    [Tooltip("Animation ID for forward run (W)")]
    [SerializeField] private int forwardRunAnimationID = 1;
    
    [Tooltip("Animation ID for left run (A)")]
    [SerializeField] private int leftRunAnimationID = 2;
    
    [Tooltip("Animation ID for right run (D)")]
    [SerializeField] private int rightRunAnimationID = 3;
    
    [Tooltip("Animation ID for back run (S) - NOT USED")]
    [SerializeField] private int backRunAnimationID = 4;
    
    [Tooltip("Animation ID for jump (Space)")]
    [SerializeField] private int jumpAnimationID = 5;
    
    [Tooltip("Animation ID for sprint (SHIFT + W)")]
    [SerializeField] private int sprintAnimationID = 18;
    
    [Header("Settings")]
    [Tooltip("Smooth transition time for animation changes")]
    [SerializeField] private float animationTransitionTime = 0.1f;
    
    private const string ANIMATION_PARAMETER = "animation";
    private int currentAnimationID = 0;
    private float jumpAnimationTimer = 0f;
    private const float JUMP_ANIMATION_DURATION = 0.2f; // How long to show jump animation (shorter - doesn't block other animations)
    
    private void Awake()
    {
        // Auto-find animator if not assigned
        if (animator == null)
        {
            // Try to find animator on this object first (character model has animator on itself)
            animator = GetComponent<Animator>();
            
            // If not found, try children
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
            
            if (animator == null)
            {
                Debug.LogWarning($"[PlayerAnimatorController] No Animator found on {gameObject.name} or children!");
            }
        }
    }
    
    private void Start()
    {
        // Set idle animation on spawn IMMEDIATELY
        SetIdle();
    }
    
    private void OnEnable()
    {
        // Also set idle when component is enabled (in case character loads after Start)
        if (animator != null)
        {
            SetIdle();
        }
    }
    
    private void Update()
    {
        // Update jump animation timer
        if (jumpAnimationTimer > 0f)
        {
            jumpAnimationTimer -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Update animations based on movement input.
    /// NEW LOGIC:
    /// - W + Space (hold) = Jump animation while space held, then idle/run when released
    /// - Space (press) = Jump animation
    /// - Hold WASD = Run animation
    /// - SHIFT + W = Sprint animation
    /// - No input = Idle animation
    /// </summary>
    public void UpdateAnimations(Vector2 moveInput, bool isGrounded, bool justJumped, bool isSprinting = false, bool isInJumpState = false, bool jumpHeld = false)
    {
        if (animator == null) return;
        
        // Start jump animation timer when jump is pressed
        if (justJumped)
        {
            jumpAnimationTimer = JUMP_ANIMATION_DURATION;
        }
        
        // NEW: If space is held, show jump animation (W + Space hold = jump animation)
        if (jumpHeld)
        {
            SetAnimation(jumpAnimationID);
            return; // Exit early - jump animation while space is held
        }
        
        // If in jump state (in air) or jump animation timer active, show jump animation
        if (isInJumpState || jumpAnimationTimer > 0f)
        {
            // Show jump animation - this is the ONLY animation during jump state
            SetAnimation(jumpAnimationID);
            return; // Exit early - no other animations while jumping
        }
        
        // Only show these animations when GROUNDED (not in jump state) and space not held
        if (isSprinting && moveInput.y > 0.1f)
        {
            // SHIFT + W = Sprint animation
            SetAnimation(sprintAnimationID);
        }
        else if (moveInput.magnitude > 0.1f)
        {
            // Any WASD key HOLD = run animation (same for all directions)
            SetAnimation(forwardRunAnimationID);
        }
        else
        {
            // No WASD pressed - IDLE
            SetAnimation(idleAnimationID);
        }
    }
    
    /// <summary>
    /// Set idle animation.
    /// </summary>
    public void SetIdle()
    {
        SetAnimation(idleAnimationID);
    }
    
    /// <summary>
    /// Clear jump animation timer (called when player lands).
    /// </summary>
    public void ClearJumpAnimation()
    {
        jumpAnimationTimer = 0f;
    }
    
    /// <summary>
    /// Set specific animation by ID.
    /// </summary>
    private void SetAnimation(int animationID)
    {
        if (animator == null) return;
        
        if (currentAnimationID != animationID)
        {
            currentAnimationID = animationID;
            animator.SetInteger(ANIMATION_PARAMETER, animationID);
        }
    }
    
    /// <summary>
    /// Get current animation ID.
    /// </summary>
    public int GetCurrentAnimationID()
    {
        return currentAnimationID;
    }
    
    /// <summary>
    /// Check if animator is available.
    /// </summary>
    public bool IsAnimatorAvailable()
    {
        return animator != null;
    }
}

