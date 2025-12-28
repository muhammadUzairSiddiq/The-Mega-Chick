using UnityEngine;
using TMPro;

/// <summary>
/// Displays match timer in HUD.
/// Why separate? HUD components isolated, easy to show/hide.
/// </summary>
public class MatchTimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private string format = "mm:ss"; // or "ss" for seconds only
    
    [Header("Warning Settings")]
    [SerializeField] private bool showWarning = true;
    [SerializeField] private float warningTime = 30f; // Show warning when < 30 seconds
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    
    private void Update()
    {
        if (MatchFlowController.Instance == null) return;
        
        MatchState currentState = MatchFlowController.Instance.GetCurrentState();
        
        // Only show timer during Playing state
        if (currentState == MatchState.Playing)
        {
            UpdateTimer();
        }
        else
        {
            HideTimer();
        }
    }
    
    private void UpdateTimer()
    {
        float remaining = MatchFlowController.Instance.GetStateRemainingTime();
        
        if (remaining < 0f)
        {
            // No time limit
            if (timerText != null)
            {
                timerText.text = "";
            }
            return;
        }
        
        if (timerText != null)
        {
            // Format time
            int minutes = Mathf.FloorToInt(remaining / 60f);
            int seconds = Mathf.FloorToInt(remaining % 60f);
            
            if (format == "mm:ss")
            {
                timerText.text = $"{minutes:00}:{seconds:00}";
            }
            else
            {
                timerText.text = Mathf.CeilToInt(remaining).ToString();
            }
            
            // Warning color
            if (showWarning && remaining <= warningTime)
            {
                timerText.color = warningColor;
                
                // Optional: pulse effect
                if (remaining <= 10f)
                {
                    float pulse = Mathf.Sin(Time.time * 5f) * 0.5f + 0.5f;
                    timerText.color = Color.Lerp(normalColor, warningColor, pulse);
                }
            }
            else
            {
                timerText.color = normalColor;
            }
        }
    }
    
    private void HideTimer()
    {
        if (timerText != null)
        {
            timerText.text = "";
        }
    }
}

