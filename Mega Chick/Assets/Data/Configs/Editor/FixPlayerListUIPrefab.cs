#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

/// <summary>
/// Fix PlayerListUI prefab assignment in scene.
/// </summary>
public class FixPlayerListUIPrefab : EditorWindow
{
    [MenuItem("Mega Chick/Fix PlayerListUI Prefab Assignment")]
    public static void FixPrefab()
    {
        Debug.Log("🚀 Fixing PlayerListUI prefab assignment...");
        
        PlayerListUI playerListUI = Object.FindObjectOfType<PlayerListUI>(true);
        if (playerListUI == null)
        {
            Debug.LogError("❌ PlayerListUI not found in scene!");
            EditorUtility.DisplayDialog("Error", "PlayerListUI not found in scene!", "OK");
            return;
        }
        
        // Load PlayerEntry prefab
        GameObject playerEntryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/PlayerEntry.prefab");
        if (playerEntryPrefab == null)
        {
            Debug.LogError("❌ PlayerEntry.prefab not found at Assets/Prefabs/UI/PlayerEntry.prefab!");
            EditorUtility.DisplayDialog("Error", "PlayerEntry.prefab not found! Please run 'Mega Chick/Step 6 UI Setup/Complete Setup' first.", "OK");
            return;
        }
        
        // Assign prefab using reflection
        var playerEntryPrefabField = typeof(PlayerListUI).GetField("playerEntryPrefab", 
            BindingFlags.NonPublic | BindingFlags.Instance);
        playerEntryPrefabField?.SetValue(playerListUI, playerEntryPrefab);
        
        EditorUtility.SetDirty(playerListUI);
        Debug.Log("✅ PlayerEntry prefab assigned to PlayerListUI!");
        EditorUtility.DisplayDialog("Success", "PlayerEntry prefab assigned to PlayerListUI!", "OK");
    }
}
#endif

