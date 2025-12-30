#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

/// <summary>
/// Adds CharacterSelectionManager to current scene if missing.
/// </summary>
public class AddCharacterSelectionManagerToScene
{
    [MenuItem("Mega Chick/Add CharacterSelectionManager to Scene")]
    public static void AddToScene()
    {
        // Check if already exists
        CharacterSelectionManager existing = Object.FindObjectOfType<CharacterSelectionManager>();
        if (existing != null)
        {
            Debug.Log("✅ CharacterSelectionManager already exists in scene!");
            Selection.activeGameObject = existing.gameObject;
            EditorUtility.DisplayDialog("Already Exists",
                "CharacterSelectionManager already exists in the scene!\n\n" +
                "It has been selected in Hierarchy.",
                "OK");
            return;
        }
        
        // Try to load prefab first
        string prefabPath = "Assets/Prefabs/Managers/CharacterSelectionManager.prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        GameObject managerObj;
        
        if (prefab != null)
        {
            // Instantiate prefab
            managerObj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            Debug.Log($"✅ Instantiated CharacterSelectionManager from prefab: {prefabPath}");
        }
        else
        {
            // Create new GameObject
            managerObj = new GameObject("CharacterSelectionManager");
            managerObj.AddComponent<CharacterSelectionManager>();
            Debug.Log("✅ Created new CharacterSelectionManager GameObject");
        }
        
        // Auto-load characters
        CharacterSelectionManager manager = managerObj.GetComponent<CharacterSelectionManager>();
        if (manager != null)
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterData");
            if (guids.Length > 0)
            {
                SerializedObject serializedManager = new SerializedObject(manager);
                SerializedProperty availableCharactersProp = serializedManager.FindProperty("availableCharacters");
                
                if (availableCharactersProp != null)
                {
                    availableCharactersProp.arraySize = guids.Length;
                    for (int i = 0; i < guids.Length; i++)
                    {
                        string charPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                        CharacterData charData = AssetDatabase.LoadAssetAtPath<CharacterData>(charPath);
                        availableCharactersProp.GetArrayElementAtIndex(i).objectReferenceValue = charData;
                    }
                    serializedManager.ApplyModifiedProperties();
                    Debug.Log($"✅ Auto-loaded {guids.Length} CharacterData assets!");
                }
            }
        }
        
        // Select it
        Selection.activeGameObject = managerObj;
        
        // Mark scene as dirty
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        Debug.Log("✅ CharacterSelectionManager added to scene and selected!");
        EditorUtility.DisplayDialog("Success",
            "CharacterSelectionManager added to scene!\n\n" +
            "✅ GameObject created\n" +
            "✅ Character data auto-loaded\n" +
            "✅ Selected in Hierarchy\n\n" +
            "Check Inspector to verify character data is loaded.",
            "OK");
    }
}
#endif

