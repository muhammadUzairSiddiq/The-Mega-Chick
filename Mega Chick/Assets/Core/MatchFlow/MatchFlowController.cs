#if PUN_2_OR_NEWER
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

/// <summary>
/// Master-client authoritative match flow controller.
/// Why Master Client? Single source of truth prevents desync.
/// Why state machine? Clear transitions, impossible invalid states.
/// </summary>
public class MatchFlowController : MonoBehaviourPunCallbacks
{
    public static MatchFlowController Instance { get; private set; }
    
    [Header("Config")]
    [SerializeField] private MatchConfig matchConfig;
    
    [Header("Debug")]
    [SerializeField] private bool logStateChanges = true;
    
    // Current state
    private MatchState currentState = MatchState.MainMenu;
    private float stateStartTime;
    private float stateDuration;
    
    // Events
    public event System.Action<MatchState> OnStateChanged;
    
    // Photon Event Codes (for syncing state)
    private const byte STATE_CHANGE_EVENT = 1;
    
    private void Awake()
    {
        // CRITICAL: Log immediately to verify script is running
        Debug.Log("🔵 [MatchFlowController] ========== AWAKE CALLED ==========");
        
        try
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("✅ [MatchFlowController] Instance created and set to DontDestroyOnLoad");
            }
            else
            {
                Debug.Log("⚠️ [MatchFlowController] Instance already exists, destroying duplicate");
                Destroy(gameObject);
                return;
            }
            
            // Register custom event
            if (PhotonNetwork.NetworkingClient != null)
            {
                PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
                Debug.Log("✅ [MatchFlowController] Photon event handler registered");
            }
            else
            {
                Debug.LogError("❌ [MatchFlowController] PhotonNetwork.NetworkingClient is NULL!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ [MatchFlowController] ERROR in Awake: {e.Message}\n{e.StackTrace}");
        }
        
        Debug.Log("🔵 [MatchFlowController] ========== AWAKE COMPLETE ==========");
    }
    
    private void Start()
    {
        Debug.Log("🟢 [MatchFlowController] ========== START CALLED ==========");
        Debug.Log($"🟢 [MatchFlowController] START called - Current state: {currentState}");
        Debug.Log($"🟢 [MatchFlowController] IsMasterClient: {PhotonNetwork.IsMasterClient}");
        Debug.Log($"🟢 [MatchFlowController] IsConnected: {PhotonNetwork.IsConnected}");
        
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        Debug.Log($"🟢 [MatchFlowController] Current scene: {currentScene}");
        Debug.Log($"🟢 [MatchFlowController] Scene name check: '{currentScene}' == 'Race'? {currentScene == "Race"}");
        Debug.Log($"🟢 [MatchFlowController] PhotonNetwork.IsConnected: {PhotonNetwork.IsConnected}");
        
        // Auto-transition to Playing state when Race scene loads
        // This works for both: direct scene load AND loading from lobby
        if (currentScene == "Race" && PhotonNetwork.IsConnected)
        {
            Debug.Log($"🟢 [MatchFlowController] ✅ Race scene detected AND connected! Auto-transitioning to Playing state...");
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log("🟢 [MatchFlowController] ✅ Is Master Client - will transition to Playing state");
                // Wait a frame to ensure everything is initialized
                StartCoroutine(DelayedStateTransition());
            }
            else
            {
                Debug.Log("🟢 [MatchFlowController] ⚠️ Not Master Client - waiting for state sync from Master");
            }
        }
        else
        {
            Debug.LogWarning($"🟢 [MatchFlowController] ⚠️ Conditions not met: scene='{currentScene}', connected={PhotonNetwork.IsConnected}");
        }
        
        Debug.Log("🟢 [MatchFlowController] ========== START COMPLETE ==========");
    }
    
    /// <summary>
    /// Delayed state transition to ensure all systems are initialized.
    /// </summary>
    private IEnumerator DelayedStateTransition()
    {
        Debug.Log("⏳ [MatchFlowController] ========== DELAYED STATE TRANSITION START ==========");
        Debug.Log("⏳ [MatchFlowController] Waiting 1 second for scene to fully load...");
        yield return new WaitForSeconds(1f);
        
        Debug.Log($"⏳ [MatchFlowController] Current state check: {currentState} != Playing? {currentState != MatchState.Playing}");
        if (currentState != MatchState.Playing)
        {
            Debug.Log($"🟢 [MatchFlowController] ✅ Transitioning from {currentState} to Playing state");
            int roundDuration = matchConfig != null ? matchConfig.defaultRoundSeconds : 300;
            Debug.Log($"🟢 [MatchFlowController] Calling SetState(Playing, {roundDuration})...");
            SetState(MatchState.Playing, roundDuration);
            Debug.Log($"🟢 [MatchFlowController] SetState() call completed. New state: {currentState}");
        }
        else
        {
            Debug.Log("🟢 [MatchFlowController] ⚠️ Already in Playing state - skipping transition");
        }
        
        Debug.Log("⏳ [MatchFlowController] ========== DELAYED STATE TRANSITION COMPLETE ==========");
    }
    
    private void OnDestroy()
    {
        if (PhotonNetwork.NetworkingClient != null)
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        }
    }
    
    /// <summary>
    /// Called when enabled - subscribe to scene loaded event.
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Debug.Log("🟢 [MatchFlowController] OnEnable - subscribed to sceneLoaded event");
    }
    
    /// <summary>
    /// Called when disabled - unsubscribe from scene loaded event.
    /// </summary>
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Called when a new scene is loaded (Unity callback).
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"📦 [MatchFlowController] ========== ON SCENE LOADED ==========");
        HandleSceneLoaded();
    }
    
    /// <summary>
    /// Handle scene loaded - check if Race scene and transition to Playing.
    /// </summary>
    private void HandleSceneLoaded()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"📦 [MatchFlowController] Scene name: {sceneName}");
        Debug.Log($"📦 [MatchFlowController] IsConnected: {PhotonNetwork.IsConnected}");
        Debug.Log($"📦 [MatchFlowController] IsMasterClient: {PhotonNetwork.IsMasterClient}");
        Debug.Log($"📦 [MatchFlowController] Current state: {currentState}");
        
        // Auto-transition to Playing when Race scene loads
        if (sceneName == "Race" && PhotonNetwork.IsConnected)
        {
            Debug.Log($"📦 [MatchFlowController] ✅ Race scene loaded AND connected! Auto-transitioning to Playing...");
            StartCoroutine(DelayedStateTransitionAfterSceneLoad());
        }
        else
        {
            Debug.LogWarning($"📦 [MatchFlowController] ⚠️ Conditions not met: scene='{sceneName}', connected={PhotonNetwork.IsConnected}");
        }
    }
    
    /// <summary>
    /// Delayed state transition after scene loads.
    /// </summary>
    private IEnumerator DelayedStateTransitionAfterSceneLoad()
    {
        Debug.Log("⏳ [MatchFlowController] Waiting 2 seconds after scene load for everything to initialize...");
        yield return new WaitForSeconds(2f);
        
        if (currentState != MatchState.Playing)
        {
            Debug.Log($"🟢 [MatchFlowController] Transitioning from {currentState} to Playing state (after scene load)");
            int roundDuration = matchConfig != null ? matchConfig.defaultRoundSeconds : 300;
            SetState(MatchState.Playing, roundDuration);
        }
    }
    
    /// <summary>
    /// Set new match state (Master Client only).
    /// Why Master only? Prevents conflicts, single authority.
    /// </summary>
    public void SetState(MatchState newState, float duration = 0f)
    {
        Debug.Log($"🎯 [MatchFlowController] ========== SET STATE CALLED ==========");
        Debug.Log($"🎯 [MatchFlowController] SetState({newState}, {duration}) called");
        Debug.Log($"🎯 [MatchFlowController] IsMasterClient: {PhotonNetwork.IsMasterClient}");
        Debug.Log($"🎯 [MatchFlowController] Current state: {currentState}");
        
        if (!PhotonNetwork.IsMasterClient)
        {
            Debug.LogWarning("⚠️ [MatchFlowController] Only Master Client can change match state!");
            Log("Only Master Client can change match state!");
            return;
        }
        
        if (currentState == newState)
        {
            Debug.LogWarning($"⚠️ [MatchFlowController] Already in state: {newState}");
            Log($"Already in state: {newState}");
            return;
        }
        
        Debug.Log($"🎯 [MatchFlowController] Exiting state: {currentState}");
        ExitState(currentState);
        
        currentState = newState;
        stateStartTime = (float)PhotonNetwork.Time;
        stateDuration = duration;
        Debug.Log($"🎯 [MatchFlowController] State changed to: {newState}");
        
        Debug.Log($"🎯 [MatchFlowController] Entering state: {newState}");
        EnterState(newState);
        
        // Broadcast state change to all clients
        Debug.Log($"🎯 [MatchFlowController] Broadcasting state change to all clients...");
        BroadcastStateChange(newState, stateStartTime, stateDuration);
        
        OnStateChanged?.Invoke(newState);
        GameEventBus.FireMatchStateChanged();
        
        Debug.Log($"🎯 [MatchFlowController] ========== SET STATE COMPLETE ==========");
    }
    
    /// <summary>
    /// Broadcast state change to all clients via Photon event.
    /// Why Photon event? Reliable, synced, all clients receive it.
    /// </summary>
    private void BroadcastStateChange(MatchState state, float startTime, float duration)
    {
        object[] content = new object[] { (int)state, startTime, duration };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(STATE_CHANGE_EVENT, content, raiseEventOptions, SendOptions.SendReliable);
    }
    
    /// <summary>
    /// Handle Photon events (state changes from Master).
    /// </summary>
    private void OnPhotonEvent(EventData photonEvent)
    {
        if (photonEvent.Code == STATE_CHANGE_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;
            MatchState newState = (MatchState)(int)data[0];
            float startTime = (float)data[1];
            float duration = (float)data[2];
            
            // Only apply if we're not Master (Master already set it)
            if (!PhotonNetwork.IsMasterClient)
            {
                ExitState(currentState);
                currentState = newState;
                stateStartTime = startTime;
                stateDuration = duration;
                EnterState(newState);
                OnStateChanged?.Invoke(newState);
                GameEventBus.FireMatchStateChanged();
            }
        }
    }
    
    /// <summary>
    /// Enter state - called when state becomes active.
    /// </summary>
    private void EnterState(MatchState state)
    {
        Debug.Log($"🎯 [MatchFlowController] ========== ENTER STATE: {state} ==========");
        Log($"Entering state: {state}");
        
        switch (state)
        {
            case MatchState.Lobby:
                Debug.Log("📋 [MatchFlowController] Lobby state - Enable lobby UI, character selection");
                // Enable lobby UI, character selection
                break;
                
            case MatchState.Countdown:
                Debug.Log("⏱️ [MatchFlowController] Countdown state - Firing countdown started event");
                GameEventBus.FireCountdownStarted();
                // Disable player input, show countdown UI
                break;
                
            case MatchState.Playing:
                Debug.Log("🎮 [MatchFlowController] ========== PLAYING STATE ENTERED ==========");
                Debug.Log("🎮 [MatchFlowController] Firing match started event...");
                GameEventBus.FireMatchStarted();
                
                // Spawn players when match starts playing
                Debug.Log($"🎮 [MatchFlowController] Checking SpawnManager.Instance... {(SpawnManager.Instance != null ? "FOUND" : "NULL")}");
                if (SpawnManager.Instance != null)
                {
                    Debug.Log("🎮 [MatchFlowController] Calling SpawnManager.SpawnAllPlayers()...");
                    SpawnManager.Instance.SpawnAllPlayers(MatchState.Playing);
                }
                else
                {
                    Debug.LogError("❌ [MatchFlowController] SpawnManager.Instance is NULL! Players won't spawn!");
                }
                
                // Start camera intro sequence LOCALLY for each player (not synced)
                // Each player runs their own intro independently
                Debug.Log("🎮 [MatchFlowController] Starting camera intro LOCALLY for this player...");
                StartCoroutine(StartCameraIntroLocal());
                Debug.Log("🎮 [MatchFlowController] StartCameraIntroLocal coroutine started");
                break;
                
            case MatchState.Results:
                Debug.Log("🏁 [MatchFlowController] Results state - Firing match ended event");
                GameEventBus.FireMatchEnded();
                // Show results screen
                break;
        }
        
        Debug.Log($"✅ [MatchFlowController] EnterState({state}) completed");
    }
    
    /// <summary>
    /// Exit state - called when leaving a state.
    /// </summary>
    private void ExitState(MatchState state)
    {
        Log($"Exiting state: {state}");
        // Cleanup if needed
    }
    
    /// <summary>
    /// Get current state.
    /// </summary>
    public MatchState GetCurrentState() => currentState;
    
    /// <summary>
    /// Get elapsed time in current state (synced across clients).
    /// Why PhotonNetwork.Time? Server time, synced across all clients.
    /// </summary>
    public float GetStateElapsedTime()
    {
        if (!PhotonNetwork.IsConnected)
            return Time.time - stateStartTime;
            
        return (float)PhotonNetwork.Time - stateStartTime;
    }
    
    /// <summary>
    /// Get remaining time in current state.
    /// </summary>
    public float GetStateRemainingTime()
    {
        if (stateDuration <= 0f) return -1f; // No time limit
        return stateDuration - GetStateElapsedTime();
    }
    
    /// <summary>
    /// Check if state has duration and is expired.
    /// </summary>
    public bool IsStateExpired()
    {
        if (stateDuration <= 0f) return false;
        return GetStateElapsedTime() >= stateDuration;
    }
    
    /// <summary>
    /// Start match from lobby (Master Client only).
    /// </summary>
    public void StartMatch()
    {
        if (currentState != MatchState.Lobby)
        {
            Log("Can only start match from Lobby!");
            return;
        }
        
        int countdownSeconds = matchConfig != null ? matchConfig.countdownSeconds : 3;
        SetState(MatchState.Countdown, countdownSeconds);
    }
    
    /// <summary>
    /// End match and show results (Master Client only).
    /// </summary>
    public void EndMatch()
    {
        if (currentState != MatchState.Playing)
        {
            Log("Can only end match from Playing state!");
            return;
        }
        
        float resultsDuration = matchConfig != null ? matchConfig.resultsDisplayDuration : 10f;
        SetState(MatchState.Results, resultsDuration);
    }
    
    private void Update()
    {
        // Master Client checks for state expiration
        if (PhotonNetwork.IsMasterClient && IsStateExpired())
        {
            Debug.Log($"⏰ [MatchFlowController] State expired! Current state: {currentState}");
            switch (currentState)
            {
                case MatchState.Countdown:
                    // Countdown finished, start playing
                    Debug.Log("⏰ [MatchFlowController] Countdown expired - transitioning to Playing state");
                    int roundDuration = matchConfig != null ? matchConfig.defaultRoundSeconds : 300;
                    SetState(MatchState.Playing, roundDuration);
                    break;
                    
                case MatchState.Playing:
                    // Match time expired, show results
                    Debug.Log("⏰ [MatchFlowController] Match time expired - ending match");
                    EndMatch();
                    break;
                    
                case MatchState.Results:
                    // Results shown, return to lobby
                    Debug.Log("⏰ [MatchFlowController] Results time expired - returning to Lobby");
                    SetState(MatchState.Lobby);
                    break;
            }
        }
    }
    
    /// <summary>
    /// Start camera intro sequence LOCALLY for each player (runs independently on each client).
    /// </summary>
    private IEnumerator StartCameraIntroLocal()
    {
        Debug.Log("🎬 [MatchFlowController] ========== START CAMERA INTRO LOCAL ==========");
        Debug.Log("🎬 [MatchFlowController] Waiting 1 second for players to spawn...");
        yield return new WaitForSeconds(1f);
        
        Debug.Log($"🎬 [MatchFlowController] CameraIntroController.Instance: {(CameraIntroController.Instance != null ? "EXISTS" : "NULL")}");
        if (CameraIntroController.Instance != null)
        {
            Debug.Log("🎬 [MatchFlowController] Searching for LOCAL player...");
            GameObject localPlayer = FindLocalPlayer();
            int retries = 0;
            while (localPlayer == null && retries < 10)
            {
                Debug.Log($"⏳ [MatchFlowController] Local player not found, retry {retries + 1}/10...");
                yield return new WaitForSeconds(0.5f);
                localPlayer = FindLocalPlayer();
                retries++;
            }
            
            if (localPlayer != null)
            {
                Debug.Log($"✅ [MatchFlowController] Local player found: '{localPlayer.name}' - starting intro LOCALLY!");
                
                #if PUN_2_OR_NEWER
                PhotonView pv = localPlayer.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    Debug.Log($"✅ [MatchFlowController] Verified: '{localPlayer.name}' is LOCAL player (IsMine=true)");
                }
                #endif
                
                CameraIntroController.Instance.StartIntro(localPlayer.transform);
            }
            else
            {
                Debug.LogError("❌ [MatchFlowController] Local player not found after retries - cannot start camera intro");
                EnablePlayerInputImmediately();
            }
        }
        else
        {
            Debug.LogError("❌ [MatchFlowController] CameraIntroController.Instance is NULL - skipping intro");
            EnablePlayerInputImmediately();
        }
    }
    
    /// <summary>
    /// Delayed camera intro start (waits for players to spawn).
    /// </summary>
    private IEnumerator StartCameraIntroDelayed()
    {
        Log("⏳ [START_CAMERA_INTRO] Waiting 0.5s for players to spawn...");
        // Wait a bit for players to spawn
        yield return new WaitForSeconds(0.5f);
        
        if (CameraIntroController.Instance != null)
        {
            Log("✅ [START_CAMERA_INTRO] CameraIntroController.Instance found");
            
            // Find local player (retry if not found)
            Log("🔍 [START_CAMERA_INTRO] Searching for LOCAL player...");
            GameObject localPlayer = FindLocalPlayer();
            int retries = 0;
            while (localPlayer == null && retries < 10)
            {
                Log($"⏳ [START_CAMERA_INTRO] Local player not found, retry {retries + 1}/10...");
                yield return new WaitForSeconds(0.5f);
                localPlayer = FindLocalPlayer();
                retries++;
            }
            
            if (localPlayer != null)
            {
                Log($"✅ [START_CAMERA_INTRO] Local player found: '{localPlayer.name}' - starting intro...");
                
                // Verify it's actually the local player
                #if PUN_2_OR_NEWER
                PhotonView pv = localPlayer.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    Log($"✅ [START_CAMERA_INTRO] Verified: '{localPlayer.name}' is LOCAL player (IsMine=true)");
                }
                else if (pv != null)
                {
                    Log($"❌ [START_CAMERA_INTRO] ERROR: '{localPlayer.name}' is NOT local player (IsMine=false)! This is a bug!");
                }
                #endif
                
                CameraIntroController.Instance.StartIntro(localPlayer.transform);
            }
            else
            {
                Log("❌ [START_CAMERA_INTRO] Local player not found after retries - cannot start camera intro");
                EnablePlayerInputImmediately();
            }
        }
        else
        {
            Log("❌ [START_CAMERA_INTRO] CameraIntroController.Instance is NULL - skipping intro");
            // If no intro controller, enable input immediately
            EnablePlayerInputImmediately();
        }
    }
    
    /// <summary>
    /// Find local player GameObject using PhotonView.IsMine.
    /// </summary>
    private GameObject FindLocalPlayer()
    {
#if PUN_2_OR_NEWER
        Log("🔍 [FIND_LOCAL_PLAYER] Searching for local player via PhotonView.IsMine...");
        PhotonView[] photonViews = FindObjectsOfType<PhotonView>();
        Log($"🔍 [FIND_LOCAL_PLAYER] Found {photonViews.Length} PhotonView(s) in scene");
        
        foreach (var pv in photonViews)
        {
            if (pv.IsMine)
            {
                Log($"🔍 [FIND_LOCAL_PLAYER] Found PhotonView with IsMine=true: '{pv.gameObject.name}'");
                
                // Check if it's a player object
                if (pv.gameObject.name.Contains("Player") || pv.gameObject.GetComponent<PlayerController>() != null)
                {
                    Log($"✅ [FIND_LOCAL_PLAYER] Local player found: '{pv.gameObject.name}' (IsMine=true)");
                    return pv.gameObject;
                }
                else
                {
                    Log($"⚠️ [FIND_LOCAL_PLAYER] PhotonView '{pv.gameObject.name}' is mine but not a player object");
                }
            }
        }
        
        Log("⚠️ [FIND_LOCAL_PLAYER] No local player found via PhotonView.IsMine");
#else
        Log("⚠️ [FIND_LOCAL_PLAYER] Photon not available - using fallback");
#endif
        
        // Fallback: find any Player object (should not happen in multiplayer)
        Log("🔍 [FIND_LOCAL_PLAYER] Using fallback: FindGameObjectsWithTag('Player')...");
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Log($"🔍 [FIND_LOCAL_PLAYER] Found {players.Length} GameObject(s) with 'Player' tag");
        if (players.Length > 0)
        {
            Log($"⚠️ [FIND_LOCAL_PLAYER] WARNING: Using fallback player '{players[0].name}' - this may not be the local player!");
            return players[0];
        }
        
        // Last resort: find by name
        Log("🔍 [FIND_LOCAL_PLAYER] Last resort: GameObject.Find('Player')...");
        GameObject found = GameObject.Find("Player");
        if (found != null)
        {
            Log($"⚠️ [FIND_LOCAL_PLAYER] WARNING: Found player by name '{found.name}' - this may not be the local player!");
        }
        else
        {
            Log("❌ [FIND_LOCAL_PLAYER] No player found by any method!");
        }
        return found;
    }
    
    /// <summary>
    /// Enable player input immediately (fallback if no intro controller).
    /// </summary>
    private void EnablePlayerInputImmediately()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (var player in players)
        {
            player.SetInputEnabled(true);
        }
    }
    
    private void Log(string message)
    {
        if (logStateChanges)
        {
            Debug.Log($"[MatchFlowController] {message}");
        }
    }
}
#else
using UnityEngine;

/// <summary>
/// Photon not installed - stub implementation.
/// </summary>
public class MatchFlowController : MonoBehaviour
{
    public static MatchFlowController Instance { get; private set; }
    
    [Header("Config")]
    [SerializeField] private MatchConfig matchConfig;
    
    private MatchState currentState = MatchState.MainMenu;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        Debug.LogWarning("[MatchFlowController] Photon not installed!");
    }
    
    public void SetState(MatchState newState, float duration = 0f)
    {
        currentState = newState;
        Debug.Log($"State changed to: {newState}");
    }
    
    public MatchState GetCurrentState() => currentState;
    public float GetStateElapsedTime() => 0f;
    public float GetStateRemainingTime() => -1f;
    public bool IsStateExpired() => false;
    public void StartMatch() => Debug.LogWarning("Photon not installed!");
    public void EndMatch() => Debug.LogWarning("Photon not installed!");
}
#endif


