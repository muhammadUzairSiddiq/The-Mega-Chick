using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple test UI for Match Flow Controller.
/// Why? Visual feedback to test state machine without building full UI.
/// </summary>
public class MatchFlowTestUI : MonoBehaviour
{
    [Header("State Display")]
    [SerializeField] private TextMeshProUGUI currentStateText;
    [SerializeField] private TextMeshProUGUI elapsedTimeText;
    [SerializeField] private TextMeshProUGUI remainingTimeText;
    
    [Header("Buttons")]
    [SerializeField] private Button startMatchButton;
    [SerializeField] private Button endMatchButton;
    [SerializeField] private Button returnToLobbyButton;
    
    [Header("Countdown Display")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    
    [Header("Timer Display")]
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    
    private void Start()
    {
        // Setup buttons
        if (startMatchButton != null)
            startMatchButton.onClick.AddListener(OnStartMatch);
        
        if (endMatchButton != null)
            endMatchButton.onClick.AddListener(OnEndMatch);
        
        if (returnToLobbyButton != null)
            returnToLobbyButton.onClick.AddListener(OnReturnToLobby);
        
        // Subscribe to state changes
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.OnStateChanged += OnStateChanged;
        }
        
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.OnStateChanged -= OnStateChanged;
        }
    }
    
    private void Update()
    {
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (MatchFlowController.Instance == null) return;
        
        MatchState currentState = MatchFlowController.Instance.GetCurrentState();
        float elapsed = MatchFlowController.Instance.GetStateElapsedTime();
        float remaining = MatchFlowController.Instance.GetStateRemainingTime();
        
        // Update state text
        if (currentStateText != null)
        {
            currentStateText.text = $"State: <color=yellow>{currentState}</color>";
        }
        
        // Update time displays
        if (elapsedTimeText != null)
        {
            elapsedTimeText.text = $"Elapsed: {elapsed:F1}s";
        }
        
        if (remainingTimeText != null)
        {
            if (remaining < 0f)
            {
                remainingTimeText.text = "Remaining: ∞";
            }
            else
            {
                remainingTimeText.text = $"Remaining: <color=cyan>{remaining:F1}s</color>";
            }
        }
        
        // Update countdown display
        if (currentState == MatchState.Countdown)
        {
            if (countdownPanel != null) countdownPanel.SetActive(true);
            if (countdownText != null)
            {
                if (remaining <= 0f)
                {
                    countdownText.text = "<size=100>GO!</size>";
                    countdownText.color = Color.green;
                }
                else
                {
                    int number = Mathf.CeilToInt(remaining);
                    countdownText.text = $"<size=100>{number}</size>";
                    countdownText.color = Color.white;
                }
            }
        }
        else
        {
            if (countdownPanel != null) countdownPanel.SetActive(false);
        }
        
        // Update timer display
        if (currentState == MatchState.Playing)
        {
            if (timerPanel != null) timerPanel.SetActive(true);
            if (timerText != null)
            {
                if (remaining < 0f)
                {
                    timerText.text = "∞";
                }
                else
                {
                    int minutes = Mathf.FloorToInt(remaining / 60f);
                    int seconds = Mathf.FloorToInt(remaining % 60f);
                    timerText.text = $"{minutes:00}:{seconds:00}";
                    
                    // Warning color
                    if (remaining <= 30f)
                    {
                        timerText.color = Color.red;
                    }
                    else
                    {
                        timerText.color = Color.white;
                    }
                }
            }
        }
        else
        {
            if (timerPanel != null) timerPanel.SetActive(false);
        }
        
        // Update button states
        if (startMatchButton != null)
        {
            startMatchButton.interactable = (currentState == MatchState.Lobby);
        }
        
        if (endMatchButton != null)
        {
            endMatchButton.interactable = (currentState == MatchState.Playing);
        }
        
        if (returnToLobbyButton != null)
        {
            returnToLobbyButton.interactable = (currentState == MatchState.Results);
        }
    }
    
    private void OnStateChanged(MatchState newState)
    {
        Debug.Log($"[MatchFlowTestUI] State changed to: {newState}");
        UpdateUI();
    }
    
    private void OnStartMatch()
    {
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.StartMatch();
        }
    }
    
    private void OnEndMatch()
    {
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.EndMatch();
        }
    }
    
    private void OnReturnToLobby()
    {
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.SetState(MatchState.Lobby);
        }
    }
}

