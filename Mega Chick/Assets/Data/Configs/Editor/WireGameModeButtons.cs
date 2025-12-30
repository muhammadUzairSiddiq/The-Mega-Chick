#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// Wire existing game mode buttons to GameModeSelectionUI.
/// </summary>
public class WireGameModeButtons
{
    [MenuItem("Mega Chick/Wire Game Mode Buttons")]
    public static void WireButtons()
    {
        Debug.Log("🚀 Wiring Game Mode Buttons...");
        
        GameModeSelectionUI ui = Object.FindObjectOfType<GameModeSelectionUI>();
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
            Debug.LogError("❌ Mode button parent not found!");
            EditorUtility.DisplayDialog("Error", "Mode button parent not assigned in GameModeSelectionUI!", "OK");
            return;
        }
        
        // Find all mode buttons
        Button[] buttons = buttonParent.GetComponentsInChildren<Button>();
        int wiredCount = 0;
        
        foreach (Button btn in buttons)
        {
            if (btn.name.StartsWith("ModeButton_"))
            {
                string modeName = btn.name.Replace("ModeButton_", "");
                
                // Find index in gameModes list
                var gameModesField = typeof(GameModeSelectionUI).GetField("gameModes", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var gameModes = gameModesField?.GetValue(ui) as System.Collections.Generic.List<GameModeSelectionUI.GameMode>;
                
                if (gameModes != null)
                {
                    int index = -1;
                    for (int i = 0; i < gameModes.Count; i++)
                    {
                        if (gameModes[i].modeName == modeName)
                        {
                            index = i;
                            break;
                        }
                    }
                    
                    if (index >= 0)
                    {
                        btn.onClick.RemoveAllListeners();
                        int capturedIndex = index; // Capture for lambda
                        btn.onClick.AddListener(() => {
                            Debug.Log($"[WireGameModeButtons] Button {btn.name} clicked, calling SelectMode({capturedIndex})");
                            ui.SelectMode(capturedIndex);
                        });
                        wiredCount++;
                        Debug.Log($"✅ Wired {btn.name} → SelectMode({index}) [{gameModes[index].modeName} → {gameModes[index].sceneName}]");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Mode '{modeName}' not found in gameModes list!");
                    }
                }
            }
        }
        
        Debug.Log($"✅ Wired {wiredCount} game mode buttons!");
        EditorUtility.DisplayDialog("Success", $"Wired {wiredCount} game mode buttons!\n\nButtons should now update description and load correct scenes.", "OK");
    }
}
#endif

