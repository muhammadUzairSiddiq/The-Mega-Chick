#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds GameModeSelectionManager to current scene if missing.
/// </summary>
public class AddGameModeSelectionManagerToScene
{
    [MenuItem("Mega Chick/Add GameModeSelectionManager to Scene")]
    public static void AddToScene()
    {
        // Check if already exists
        GameModeSelectionManager existing = Object.FindObjectOfType<GameModeSelectionManager>();
        if (existing != null)
        {
            Debug.Log("✅ GameModeSelectionManager already exists in scene!");
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("Already Exists",
                "GameModeSelectionManager already exists in the scene!\n\n" +
                "It has been selected in Hierarchy.",
                "OK");
            return;
        }
        
        // Try to load prefab first
        string prefabPath = "Assets/Prefabs/Managers/GameModeSelectionManager.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        GameObject managerObj;
        
        if (prefab != null)
        {
            // Instantiate prefab
            managerObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Debug.Log($"✅ Instantiated GameModeSelectionManager from prefab: {prefabPath}");
        }
        else
        {
            // Create new GameObject
            managerObj = new GameObject("GameModeSelectionManager");
            managerObj.AddComponent<GameModeSelectionManager>();
            Debug.Log("✅ Created new GameModeSelectionManager GameObject");
        }
        
        // Select it
        Selection.activeGameObject = managerObj;
        
        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        Debug.Log("✅ GameModeSelectionManager added to scene and selected!");
        EditorUtility.DisplayDialog("Success",
            "GameModeSelectionManager added to scene!\n\n" +
            "✅ GameObject created\n" +
            "✅ Selected in Hierarchy",
            "OK");
    }
}
#endif

