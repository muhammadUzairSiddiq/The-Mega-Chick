#if PUN_2_OR_NEWER
using UnityEngine;
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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Register custom event
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
    }
    
    private void OnDestroy()
    {
        if (PhotonNetwork.NetworkingClient != null)
        {
            PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        }
    }
    
    /// <summary>
    /// Set new match state (Master Client only).
    /// Why Master only? Prevents conflicts, single authority.
    /// </summary>
    public void SetState(MatchState newState, float duration = 0f)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            Log("Only Master Client can change match state!");
            return;
        }
        
        if (currentState == newState)
        {
            Log($"Already in state: {newState}");
            return;
        }
        
        ExitState(currentState);
        currentState = newState;
        stateStartTime = (float)PhotonNetwork.Time;
        stateDuration = duration;
        
        EnterState(newState);
        
        // Broadcast state change to all clients
        BroadcastStateChange(newState, stateStartTime, stateDuration);
        
        OnStateChanged?.Invoke(newState);
        GameEventBus.FireMatchStateChanged();
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
        Log($"Entering state: {state}");
        
        switch (state)
        {
            case MatchState.Lobby:
                // Enable lobby UI, character selection
                break;
                
            case MatchState.Countdown:
                GameEventBus.FireCountdownStarted();
                // Disable player input, show countdown UI
                break;
                
            case MatchState.Playing:
                GameEventBus.FireMatchStarted();
                // Enable player input, start gameplay
                break;
                
            case MatchState.Results:
                GameEventBus.FireMatchEnded();
                // Show results screen
                break;
        }
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
            switch (currentState)
            {
                case MatchState.Countdown:
                    // Countdown finished, start playing
                    int roundDuration = matchConfig != null ? matchConfig.defaultRoundSeconds : 300;
                    SetState(MatchState.Playing, roundDuration);
                    break;
                    
                case MatchState.Playing:
                    // Match time expired, show results
                    EndMatch();
                    break;
                    
                case MatchState.Results:
                    // Results shown, return to lobby
                    SetState(MatchState.Lobby);
                    break;
            }
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

