#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// Automatically loads all CharacterData assets into CharacterSelectionManager.
/// Why? Saves time, prevents missing references.
/// </summary>
public class AutoLoadCharacterData : EditorWindow
{
    [MenuItem("Mega Chick/Auto Load Character Data")]
    public static void LoadCharacterData()
    {
        Debug.Log("🚀 Starting Auto Load Character Data...");
        
        // Find all CharacterData assets
        string[] guids = AssetDatabase.FindAssets("t:CharacterData");
        
        if (guids.Length == 0)
        {
            Debug.LogWarning("⚠️ No CharacterData assets found! Create them first in 'Assets/Data/Configs/Character Data/'");
            EditorUtility.DisplayDialog("No Character Data Found",
                "No CharacterData assets found!\n\n" +
                "Please create CharacterData assets first:\n" +
                "Right-click in Project → Create → Mega Chick → Character Data",
                "OK");
            return;
        }
        
        Debug.Log($"📋 Found {guids.Length} CharacterData assets");
        
        // Load all CharacterData assets
        CharacterData[] characterDataArray = new CharacterData[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            characterDataArray[i] = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            Debug.Log($"  ✅ Loaded: {characterDataArray[i].characterName} from {path}");
        }
        
        // Find CharacterSelectionManager in scene first
        CharacterSelectionManager manager = FindObjectOfType<CharacterSelectionManager>();
        bool isPrefab = false;
        
        if (manager == null)
        {
            Debug.LogWarning("⚠️ CharacterSelectionManager not found in scene! Looking for prefab...");
            
            // Try to find prefab
            string[] managerGuids = AssetDatabase.FindAssets("CharacterSelectionManager t:Prefab");
            if (managerGuids.Length > 0)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(managerGuids[0]);
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                manager = prefab.GetComponent<CharacterSelectionManager>();
                isPrefab = true;
                
                if (manager != null)
                {
                    Debug.Log($"✅ Found CharacterSelectionManager in prefab: {prefabPath}");
                }
            }
        }
        else
        {
            Debug.Log("✅ Found CharacterSelectionManager in scene!");
        }
        
        if (manager == null)
        {
            EditorUtility.DisplayDialog("CharacterSelectionManager Not Found",
                "CharacterSelectionManager not found!\n\n" +
                "Please:\n" +
                "1. Run 'Mega Chick → Create Manager Prefabs'\n" +
                "2. Add CharacterSelectionManager prefab to scene\n" +
                "3. Run this script again",
                "OK");
            return;
        }
        
        // Assign character data using SerializedObject
        SerializedObject serializedManager = new SerializedObject(manager);
        SerializedProperty availableCharactersProp = serializedManager.FindProperty("availableCharacters");
        
        if (availableCharactersProp == null)
        {
            Debug.LogError("❌ Could not find 'availableCharacters' property!");
            return;
        }
        
        // Clear existing list
        availableCharactersProp.arraySize = 0;
        
        // Add all character data
        availableCharactersProp.arraySize = characterDataArray.Length;
        for (int i = 0; i < characterDataArray.Length; i++)
        {
            availableCharactersProp.GetArrayElementAtIndex(i).objectReferenceValue = characterDataArray[i];
        }
        
        serializedManager.ApplyModifiedProperties();
        
        // Mark as dirty and save
        EditorUtility.SetDirty(manager);
        
        if (isPrefab)
        {
            // If it's a prefab, save the prefab
            PrefabUtility.SaveAsPrefabAsset(manager.gameObject, AssetDatabase.GetAssetPath(manager.gameObject));
            Debug.Log("✅ Saved prefab with character data!");
        }
        else if (PrefabUtility.IsPartOfPrefabInstance(manager))
        {
            // If it's a prefab instance in scene, apply to prefab
            PrefabUtility.ApplyPrefabInstance(manager.gameObject, InteractionMode.AutomatedAction);
            Debug.Log("✅ Applied changes to prefab instance!");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ Successfully loaded {characterDataArray.Length} characters into CharacterSelectionManager!");
        Debug.Log($"📋 Characters: {string.Join(", ", characterDataArray.Select(c => c.characterName))}");
        
        EditorUtility.DisplayDialog("Success",
            $"Successfully loaded {characterDataArray.Length} characters!\n\n" +
            $"Characters: {string.Join(", ", characterDataArray.Select(c => c.characterName))}\n\n" +
            $"Check CharacterSelectionManager in Inspector to verify.\n\n" +
            $"If in scene, make sure CharacterSelectionManager GameObject is active!",
            "OK");
    }
}
#endif

