using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI for countdown display (3..2..1..GO).
/// Why separate? UI logic isolated from match flow.
/// </summary>
public class CountdownUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject countdownPanel;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private float textScaleMultiplier = 1.5f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip countdownSound;
    [SerializeField] private AudioClip goSound;
    
    private int lastDisplayedNumber = -1;
    private Coroutine scaleAnimation;
    
    private void OnEnable()
    {
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.OnStateChanged += OnMatchStateChanged;
        }
    }
    
    private void OnDisable()
    {
        if (MatchFlowController.Instance != null)
        {
            MatchFlowController.Instance.OnStateChanged -= OnMatchStateChanged;
        }
        
        if (scaleAnimation != null)
        {
            StopCoroutine(scaleAnimation);
        }
    }
    
    private void Update()
    {
        if (MatchFlowController.Instance == null) return;
        
        if (MatchFlowController.Instance.GetCurrentState() == MatchState.Countdown)
        {
            UpdateCountdownDisplay();
        }
    }
    
    private void OnMatchStateChanged(MatchState newState)
    {
        if (newState == MatchState.Countdown)
        {
            ShowCountdown();
        }
        else
        {
            HideCountdown();
        }
    }
    
    private void ShowCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(true);
        }
        lastDisplayedNumber = -1;
    }
    
    private void HideCountdown()
    {
        if (countdownPanel != null)
        {
            countdownPanel.SetActive(false);
        }
        if (countdownText != null)
        {
            countdownText.text = "";
        }
    }
    
    private void UpdateCountdownDisplay()
    {
        float remaining = MatchFlowController.Instance.GetStateRemainingTime();
        
        if (remaining <= 0f)
        {
            if (countdownText != null)
            {
                countdownText.text = "GO!";
                countdownText.transform.localScale = Vector3.one * textScaleMultiplier;
            }
            PlaySound(goSound);
            return;
        }
        
        int displayNumber = Mathf.CeilToInt(remaining);
        
        // Only update when number changes (avoid spam)
        if (displayNumber != lastDisplayedNumber)
        {
            lastDisplayedNumber = displayNumber;
            
            if (countdownText != null)
            {
                countdownText.text = displayNumber.ToString();
                
                // Animate scale (simple bounce effect)
                if (scaleAnimation != null)
                {
                    StopCoroutine(scaleAnimation);
                }
                scaleAnimation = StartCoroutine(AnimateCountdownText());
            }
            
            PlaySound(countdownSound);
        }
    }
    
    private IEnumerator AnimateCountdownText()
    {
        if (countdownText == null) yield break;
        
        Vector3 startScale = Vector3.one * textScaleMultiplier * 1.3f;
        Vector3 endScale = Vector3.one * textScaleMultiplier;
        float duration = 0.3f;
        float elapsed = 0f;
        
        countdownText.transform.localScale = startScale;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Ease out back curve approximation
            t = 1f - Mathf.Pow(1 - t, 3f);
            countdownText.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        
        countdownText.transform.localScale = endScale;
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
