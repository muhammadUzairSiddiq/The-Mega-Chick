using UnityEngine;
using TMPro;

/// <summary>
/// Forces TextMeshPro components to refresh on Start to fix corrupted text.
/// Attach this to any GameObject with TextMeshPro that shows corrupted text.
/// </summary>
public class FixTextMeshProOnStart : MonoBehaviour
{
    [Header("Auto Fix Settings")]
    [Tooltip("Force update all TextMeshPro components on this GameObject and children")]
    [SerializeField] private bool fixOnStart = true;
    
    [Tooltip("Fix all TextMeshPro in scene (use sparingly)")]
    [SerializeField] private bool fixAllInScene = false;
    
    private void Start()
    {
        if (fixOnStart)
        {
            FixTextMeshProComponents();
        }
        
        if (fixAllInScene)
        {
            FixAllTextMeshProInScene();
        }
    }
    
    private void FixTextMeshProComponents()
    {
        TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.font == null)
            {
                // Try to assign default font
                if (TMP_Settings.defaultFontAsset != null)
                {
                    text.font = TMP_Settings.defaultFontAsset;
                }
            }
            
            // Force mesh update
            text.ForceMeshUpdate();
            text.UpdateMeshPadding();
        }
        
        Debug.Log($"✅ Fixed {texts.Length} TextMeshPro component(s) on {gameObject.name}");
    }
    
    private void FixAllTextMeshProInScene()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.font == null && TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
            text.ForceMeshUpdate();
            text.UpdateMeshPadding();
        }
        
        Debug.Log($"✅ Fixed {allTexts.Length} TextMeshPro component(s) in scene");
    }
}

