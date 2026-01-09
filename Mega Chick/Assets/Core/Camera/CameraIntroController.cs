using UnityEngine;
using System.Collections;
using TMPro;
#if PUN_2_OR_NEWER
using Photon.Pun;
#endif

/// <summary>
/// Handles camera intro sequence: Wait → Move to Player → Countdown → Start Gameplay
/// Disables player input during intro sequence.
/// </summary>
public class CameraIntroController : MonoBehaviour
{
    public static CameraIntroController Instance { get; private set; }
    
    [Header("Camera Settings")]
    [Tooltip("Main camera (auto-finds if null)")]
    [SerializeField] private Camera mainCamera;
    
    [Tooltip("Target to follow (player)")]
    [SerializeField] private Transform target;
    
    [Header("Intro Timing")]
    [Tooltip("Initial wait time before camera starts moving (seconds)")]
    [SerializeField] private float initialWaitTime = 5f;
    
    [Tooltip("Time to move camera to player position (seconds)")]
    [SerializeField] private float cameraMoveTime = 3f;
    
    [Header("Camera Follow Settings")]
    [Tooltip("Offset from player position (X, Y, Z)")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 5f, -10f);
    
    [Tooltip("Smooth follow speed (higher = faster, lower = smoother)")]
    [SerializeField] private float followSmoothness = 5f;
    
    [Tooltip("Rotation smoothness")]
    [SerializeField] private float rotationSmoothness = 3f;
    
    [Header("Countdown")]
    [Tooltip("Countdown UI text (optional - will create if null)")]
    [SerializeField] private TextMeshProUGUI countdownText;
    
    [Tooltip("Countdown duration (3, 2, 1, Go)")]
    [SerializeField] private float countdownDuration = 4f;
    
    [Header("Debug")]
    [SerializeField] private bool logEvents = true;
    
    // State
    private bool isIntroActive = false;
    private bool isFollowing = false;
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;
    private Vector3 targetFollowPosition;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Auto-find camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Auto-find countdown text
        if (countdownText == null)
        {
            countdownText = FindObjectOfType<TextMeshProUGUI>();
            if (countdownText != null && !countdownText.name.Contains("Countdown"))
            {
                countdownText = null; // Don't use random text
            }
        }
    }
    
    private void Start()
    {
        Debug.Log("🟢 [CameraIntroController] ========== START CALLED ==========");
        Log("✅ [START] CameraIntroController ready - waiting for MatchFlowController to start intro with local player");
        
        // Check if MatchFlowController exists
        Debug.Log($"🔍 [CameraIntroController] Checking MatchFlowController.Instance... {(MatchFlowController.Instance != null ? "EXISTS" : "NULL")}");
        if (MatchFlowController.Instance == null)
        {
            Debug.LogError("❌ [CameraIntroController] MatchFlowController.Instance is NULL! Intro will not start automatically!");
            Debug.LogError("❌ [CameraIntroController] MatchFlowController must be in the scene or loaded from Lobby scene!");
            
            // FALLBACK: Try to find local player and start intro manually after delay
            Debug.Log("🔄 [CameraIntroController] FALLBACK: Will try to start intro manually in 3 seconds...");
            StartCoroutine(FallbackStartIntro());
        }
        else
        {
            Debug.Log($"✅ [CameraIntroController] MatchFlowController.Instance found! Current state: {MatchFlowController.Instance.GetCurrentState()}");
        }
        
        // Clear any manually assigned target - it will be set by MatchFlowController
        if (target != null)
        {
            Log($"⚠️ [START] Manually assigned target '{target.name}' will be ignored - MatchFlowController will find local player");
            target = null;
        }
        
        Debug.Log("🟢 [CameraIntroController] ========== START COMPLETE ==========");
    }
    
    /// <summary>
    /// Fallback: Start intro manually if MatchFlowController doesn't exist.
    /// </summary>
    private IEnumerator FallbackStartIntro()
    {
        Debug.Log("⏳ [CameraIntroController] FALLBACK: Waiting 3 seconds for players to spawn...");
        yield return new WaitForSeconds(3f);
        
        Debug.Log("🔍 [CameraIntroController] FALLBACK: Searching for local player...");
        GameObject localPlayer = FindLocalPlayerFallback();
        
        if (localPlayer != null)
        {
            Debug.Log($"✅ [CameraIntroController] FALLBACK: Found local player '{localPlayer.name}' - starting intro!");
            StartIntro(localPlayer.transform);
        }
        else
        {
            Debug.LogError("❌ [CameraIntroController] FALLBACK: Could not find local player! Intro will not start!");
        }
    }
    
    /// <summary>
    /// Find local player as fallback when MatchFlowController is not available.
    /// </summary>
    private GameObject FindLocalPlayerFallback()
    {
        Debug.Log("🔍 [CameraIntroController] FALLBACK: Searching for local player...");
        
        #if PUN_2_OR_NEWER
        Debug.Log("🔍 [CameraIntroController] FALLBACK: Searching via PhotonView.IsMine...");
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        Debug.Log($"🔍 [CameraIntroController] FALLBACK: Found {photonViews.Length} PhotonView(s)");
        
        foreach (var pv in photonViews)
        {
            if (pv.IsMine)
            {
                Debug.Log($"✅ [CameraIntroController] FALLBACK: Found local player via PhotonView: '{pv.gameObject.name}'");
                return pv.gameObject;
            }
        }
        #endif
        
        // Fallback: find by tag
        Debug.Log("🔍 [CameraIntroController] FALLBACK: Trying FindGameObjectsWithTag('Player')...");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Debug.Log($"🔍 [CameraIntroController] FALLBACK: Found {players.Length} GameObject(s) with 'Player' tag");
        if (players.Length > 0)
        {
            Debug.Log($"✅ [CameraIntroController] FALLBACK: Using first player: '{players[0].name}'");
            return players[0];
        }
        
        Debug.LogError("❌ [CameraIntroController] FALLBACK: No player found!");
        return null;
    }
    
    /// <summary>
    /// Start intro sequence.
    /// </summary>
    public void StartIntro(Transform playerTarget)
    {
        Log($"🚀 [START_INTRO] StartIntro() called with target: {(playerTarget != null ? playerTarget.name : "NULL")}");
        
        // Verify this is the local player in multiplayer
        #if PUN_2_OR_NEWER
        if (playerTarget != null)
        {
            PhotonView pv = playerTarget.GetComponent<PhotonView>();
            if (pv != null)
            {
                if (pv.IsMine)
                {
                    Log($"✅ [START_INTRO] Verified: Target '{playerTarget.name}' is LOCAL player (IsMine=true)");
                }
                else
                {
                    Log($"❌ [START_INTRO] ERROR: Target '{playerTarget.name}' is NOT local player (IsMine=false)! This will cause issues!");
                    return; // Don't start intro for non-local player
                }
            }
            else
            {
                Log($"⚠️ [START_INTRO] Warning: Target '{playerTarget.name}' has no PhotonView - cannot verify if local player");
            }
        }
        #endif
        
        if (isIntroActive)
        {
            Log("⚠️ [START_INTRO] Intro already active! Ignoring call.");
            return;
        }
        
        if (mainCamera == null)
        {
            Log("❌ [START_INTRO] ERROR: No camera assigned!");
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Log("❌ [START_INTRO] ERROR: Camera.main is also null!");
                return;
            }
            Log($"✅ [START_INTRO] Found camera: {mainCamera.name}");
        }
        
        if (playerTarget == null)
        {
            Log("❌ [START_INTRO] ERROR: No player target assigned!");
            return;
        }
        
        target = playerTarget;
        isIntroActive = true;
        Log($"✅ [START_INTRO] Target set: {target.name}, isIntroActive: {isIntroActive}");
        
        // Store initial camera position/rotation
        initialCameraPosition = mainCamera.transform.position;
        initialCameraRotation = mainCamera.transform.rotation;
        
        // Disable player input during intro
        Log("🚫 [START_INTRO] Disabling player input...");
        DisablePlayerInput();
        
        // Start intro sequence
        Log("▶️ [START_INTRO] Starting IntroSequence coroutine...");
        StartCoroutine(IntroSequence());
    }
    
    /// <summary>
    /// Main intro sequence coroutine.
    /// </summary>
    private IEnumerator IntroSequence()
    {
        Log("🎬 Starting intro sequence...");
        
        // STEP 1: Wait
        Log($"⏳ Step 1: Waiting {initialWaitTime} seconds...");
        yield return new WaitForSeconds(initialWaitTime);
        
        // STEP 2: Move camera to player
        Log("📹 Step 2: Moving camera to player...");
        yield return StartCoroutine(MoveCameraToPlayer());
        
        // STEP 3: Countdown
        Log("⏱️ Step 3: Starting countdown...");
        yield return StartCoroutine(ShowCountdown());
        
        // Intro complete - enable gameplay
        Log("✅ Intro complete! Enabling gameplay...");
        OnIntroComplete();
    }
    
    /// <summary>
    /// Move camera smoothly to player position.
    /// </summary>
    private IEnumerator MoveCameraToPlayer()
    {
        if (target == null) 
        {
            Log("ERROR: Target is null in MoveCameraToPlayer!");
            yield break;
        }
        
        if (mainCamera == null)
        {
            Log("ERROR: Camera is null in MoveCameraToPlayer!");
            yield break;
        }
        
        // Calculate target position (player position + offset relative to player's transform)
        Vector3 offsetWorld = target.TransformDirection(followOffset);
        targetFollowPosition = target.position + offsetWorld;
        Vector3 directionToPlayer = (target.position - targetFollowPosition).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        
        while (elapsed < cameraMoveTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / cameraMoveTime;
            
            // Update offset as player might move
            offsetWorld = target.TransformDirection(followOffset);
            Vector3 currentTargetPos = target.position + offsetWorld;
            
            // Smooth interpolation
            mainCamera.transform.position = Vector3.Lerp(startPos, currentTargetPos, t);
            directionToPlayer = (target.position - mainCamera.transform.position).normalized;
            targetRotation = Quaternion.LookRotation(directionToPlayer);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, targetRotation, t);
            
            yield return null;
        }
        
        // Ensure exact position
        offsetWorld = target.TransformDirection(followOffset);
        targetFollowPosition = target.position + offsetWorld;
        directionToPlayer = (target.position - targetFollowPosition).normalized;
        targetRotation = Quaternion.LookRotation(directionToPlayer);
        mainCamera.transform.position = targetFollowPosition;
        mainCamera.transform.rotation = targetRotation;
        
        // Start following
        isFollowing = true;
        Log("✅ Camera now following player");
    }
    
    /// <summary>
    /// Show countdown (3, 2, 1, Go).
    /// </summary>
    private IEnumerator ShowCountdown()
    {
        // Create countdown text if it doesn't exist
        if (countdownText == null)
        {
            CreateCountdownText();
        }
        
        if (countdownText == null)
        {
            Log("⚠️ ERROR: Could not create countdown text!");
            yield break;
        }
        
        GameObject textGameObject = countdownText.gameObject;
        Canvas canvas = textGameObject.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 999; // Ensure it's on top
        }
        
        textGameObject.SetActive(true);
        textGameObject.transform.localScale = Vector3.zero;
        
        // Countdown: 3, 2, 1, Go
        string[] countdown = { "3", "2", "1", "GO!" };
        float timePerNumber = countdownDuration / countdown.Length;
        
        for (int i = 0; i < countdown.Length; i++)
        {
            countdownText.text = countdown[i];
            textGameObject.transform.localScale = Vector3.zero;
            
            // Scale up animation
            float scaleTime = 0.3f;
            float elapsed = 0f;
            while (elapsed < scaleTime)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(0f, 1.5f, elapsed / scaleTime);
                textGameObject.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            
            // Hold
            yield return new WaitForSeconds(timePerNumber - scaleTime);
            
            // Scale down
            elapsed = 0f;
            while (elapsed < scaleTime)
            {
                elapsed += Time.deltaTime;
                float scale = Mathf.Lerp(1.5f, 0f, elapsed / scaleTime);
                textGameObject.transform.localScale = Vector3.one * scale;
                yield return null;
            }
        }
        
        textGameObject.SetActive(false);
    }
    
    /// <summary>
    /// Create countdown text UI if it doesn't exist.
    /// </summary>
    private void CreateCountdownText()
    {
        // Try to find Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // Create canvas
            GameObject canvasObj = new GameObject("CountdownCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999; // Ensure it's on top
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // Create text
        GameObject textObj = new GameObject("CountdownText");
        textObj.transform.SetParent(canvas.transform, false);
        
        countdownText = textObj.AddComponent<TextMeshProUGUI>();
        countdownText.text = "3";
        countdownText.fontSize = 120;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.color = Color.white;
        countdownText.fontStyle = FontStyles.Bold;
        
        // Center it
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(600, 300);
        
        // Add outline for visibility (TextMeshPro has built-in outline)
        countdownText.outlineWidth = 0.2f;
        countdownText.outlineColor = Color.black;
        
        textObj.SetActive(false);
    }
    
    /// <summary>
    /// Smooth camera follow (called in LateUpdate when following).
    /// </summary>
    private void LateUpdate()
    {
        if (!isFollowing || target == null || mainCamera == null) return;
        
        // Calculate target position (player position + offset relative to player's transform)
        Vector3 offsetWorld = target.TransformDirection(followOffset);
        targetFollowPosition = target.position + offsetWorld;
        
        // Smooth position
        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetFollowPosition,
            Time.deltaTime * followSmoothness
        );
        
        // Smooth rotation (look at player)
        Vector3 directionToPlayer = (target.position - mainCamera.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        mainCamera.transform.rotation = Quaternion.Slerp(
            mainCamera.transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSmoothness
        );
    }
    
    /// <summary>
    /// Called when intro completes - enable gameplay.
    /// </summary>
    private void OnIntroComplete()
    {
        isIntroActive = false;
        
        // Enable player input
        EnablePlayerInput();
        
        // Notify MatchFlowController that intro is complete
        if (MatchFlowController.Instance != null)
        {
            // MatchFlowController will handle state transition
        }
        
        Log("🎮 Gameplay enabled!");
    }
    
    /// <summary>
    /// Disable all player input during intro.
    /// </summary>
    private void DisablePlayerInput()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            if (player != null)
            {
                player.SetInputEnabled(false);
            }
        }
        Log("🚫 Player input disabled");
    }
    
    /// <summary>
    /// Enable all player input after intro.
    /// </summary>
    private void EnablePlayerInput()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            if (player != null)
            {
                player.SetInputEnabled(true);
            }
        }
        Log("✅ Player input enabled");
    }
    
    /// <summary>
    /// Check if intro is active.
    /// </summary>
    public bool IsIntroActive() => isIntroActive;
    
    private void Log(string message)
    {
        if (logEvents)
        {
            Debug.Log($"[CameraIntroController] {message}");
        }
    }
}

