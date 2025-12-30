#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Verify and fix GameModeSelectionUI buttons.
/// </summary>
public class VerifyGameModeButtons : EditorWindow
{
    [MenuItem("Mega Chick/Verify Game Mode Buttons")]
    public static void VerifyButtons()
    {
        Debug.Log("🔍 Verifying Game Mode Buttons...");
        
        GameModeSelectionUI ui = Object.FindObjectOfType<GameModeSelectionUI>(true);
        if (ui == null)
        {
            Debug.LogError("❌ GameModeSelectionUI not found in scene!");
            EditorUtility.DisplayDialog("Error", "GameModeSelectionUI not found in scene!", "OK");
            return;
        }
        
        // Use reflection to get modeButtonParent
        var modeButtonParentField = typeof(GameModeSelectionUI).GetField("modeButtonParent", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Transform buttonParent = modeButtonParentField?.GetValue(ui) as Transform;
        
        if (buttonParent == null)
        {
            Debug.LogError("❌ Mode button parent is NULL!");
            EditorUtility.DisplayDialog("Error", "Mode button parent is not assigned in GameModeSelectionUI!", "OK");
            return;
        }
        
        // Find all buttons
        Button[] buttons = buttonParent.GetComponentsInChildren<Button>(true);
        Debug.Log($"📋 Found {buttons.Length} buttons in ModeButtonParent");
        
        int modeButtonCount = 0;
        foreach (Button btn in buttons)
        {
            if (btn != null && btn.name.StartsWith("ModeButton_"))
            {
                modeButtonCount++;
                Debug.Log($"   ✅ {btn.name} - Active: {btn.gameObject.activeSelf}, Enabled: {btn.interactable}");
                
                // Ensure button is active
                if (!btn.gameObject.activeSelf)
                {
                    btn.gameObject.SetActive(true);
                    Debug.Log($"   🔧 Activated {btn.name}");
                }
            }
        }
        
        if (modeButtonCount == 0)
        {
            Debug.LogWarning("⚠️ No mode buttons found! Buttons might have been deleted.");
            Debug.LogWarning("💡 Solution: Run 'Mega Chick → Step 6 UI Setup → Complete Setup' to recreate buttons.");
            EditorUtility.DisplayDialog("No Buttons Found", 
                $"No mode buttons found in ModeButtonParent!\n\n" +
                $"Found {buttons.Length} total buttons, but none match 'ModeButton_*' pattern.\n\n" +
                $"Solution: Run 'Mega Chick → Step 6 UI Setup → Complete Setup' to recreate the buttons.",
                "OK");
        }
        else
        {
            Debug.Log($"✅ Found {modeButtonCount} mode buttons!");
            EditorUtility.DisplayDialog("Verification Complete", 
                $"Found {modeButtonCount} mode buttons in ModeButtonParent.\n\n" +
                $"All buttons are now active and should be visible.",
                "OK");
        }
        
        EditorUtility.SetDirty(ui);
    }
}
#endif

