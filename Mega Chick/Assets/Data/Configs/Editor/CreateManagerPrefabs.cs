#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor script to automatically create all manager prefabs with proper references.
/// Why? Eliminates manual setup errors, ensures consistency.
/// </summary>
public class CreateManagerPrefabs : EditorWindow
{
    [MenuItem("Mega Chick/Create All Manager Prefabs")]
    public static void CreateAllManagerPrefabs()
    {
        string prefabPath = "Assets/Prefabs/Managers";
        
        // Create folder structure
        if (!Directory.Exists(prefabPath))
        {
            Directory.CreateDirectory(prefabPath);
            AssetDatabase.Refresh();
            Debug.Log($"✅ Created folder: {prefabPath}");
        }
        
        // Find required assets
        NetworkConfig networkConfig = FindAsset<NetworkConfig>("NetworkConfig");
        MatchConfig matchConfig = FindAsset<MatchConfig>("MatchConfig");
        GameObject playerPrefab = FindPlayerPrefab(); // Changed this line
        
        // Validate assets
        if (networkConfig == null)
        {
            Debug.LogError("❌ NetworkConfig asset not found! Please create it first using 'Mega Chick/Create Default Configs'");
            EditorUtility.DisplayDialog("Error", "NetworkConfig asset not found!\n\nPlease create it first using:\nMega Chick → Create Default Configs", "OK");
            return;
        }
        
        if (matchConfig == null)
        {
            Debug.LogError("❌ MatchConfig asset not found! Please create it first using 'Mega Chick/Create Default Configs'");
            EditorUtility.DisplayDialog("Error", "MatchConfig asset not found!\n\nPlease create it first using:\nMega Chick → Create Default Configs", "OK");
            return;
        }
        
        if (playerPrefab == null)
        {
            Debug.LogWarning("⚠️ Player prefab not found! SpawnManager will be created without player prefab reference. You can assign it manually later.");
        }
        
        // Create NetworkBootstrap prefab
        CreateNetworkBootstrapPrefab(prefabPath, networkConfig);
        
        // Create RoomManager prefab
        CreateRoomManagerPrefab(prefabPath, networkConfig);
        
        // Create MatchFlowController prefab
        CreateMatchFlowControllerPrefab(prefabPath, matchConfig);
        
        // Create SpawnManager prefab
        CreateSpawnManagerPrefab(prefabPath, playerPrefab);
        
        // Create CharacterSelectionManager prefab
        CreateCharacterSelectionManagerPrefab(prefabPath);
        
        // Create CharacterLoader prefab
        CreateCharacterLoaderPrefab(prefabPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✅ All manager prefabs created successfully!");
        EditorUtility.DisplayDialog("Success", 
            "All manager prefabs have been created!\n\n" +
            "Location: Assets/Prefabs/Managers/\n\n" +
            "All references have been automatically assigned:\n" +
            "• NetworkBootstrap → NetworkConfig\n" +
            "• RoomManager → NetworkConfig\n" +
            "• MatchFlowController → MatchConfig\n" +
            "• SpawnManager → Player Prefab\n" +
            "• CharacterSelectionManager → (auto-loaded CharacterData assets)\n" +
            "• CharacterLoader → (ready to drag into game scenes)", 
            "OK");
    }
    
    private static void CreateNetworkBootstrapPrefab(string path, NetworkConfig networkConfig)
    {
        string prefabPath = $"{path}/NetworkBootstrap.prefab";
        
        // Check if prefab already exists
        if (File.Exists(prefabPath))
        {
            Debug.Log($"⏭️  NetworkBootstrap prefab already exists, updating references...");
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                NetworkBootstrap bootstrap = existingPrefab.GetComponent<NetworkBootstrap>();
                if (bootstrap != null)
                {
                    SerializedObject serializedObject = new SerializedObject(bootstrap);
                    SerializedProperty configProperty = serializedObject.FindProperty("networkConfig");
                    if (configProperty != null)
                    {
                        configProperty.objectReferenceValue = networkConfig;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(existingPrefab);
                        Debug.Log($"✅ Updated NetworkBootstrap prefab references");
                        return;
                    }
                }
            }
        }
        
        // Create new GameObject
        GameObject go = new GameObject("NetworkBootstrap");
        NetworkBootstrap bootstrapComponent = go.AddComponent<NetworkBootstrap>();
        
        // Assign config using SerializedObject (to access private serialized fields)
        SerializedObject so = new SerializedObject(bootstrapComponent);
        so.FindProperty("networkConfig").objectReferenceValue = networkConfig;
        so.ApplyModifiedProperties();
        
        // Create prefab
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
        
        Debug.Log($"✅ Created NetworkBootstrap prefab at {prefabPath}");
    }
    
    private static void CreateRoomManagerPrefab(string path, NetworkConfig networkConfig)
    {
        string prefabPath = $"{path}/RoomManager.prefab";
        
        if (File.Exists(prefabPath))
        {
            Debug.Log($"⏭️  RoomManager prefab already exists, updating references...");
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                RoomManager roomManager = existingPrefab.GetComponent<RoomManager>();
                if (roomManager != null)
                {
                    SerializedObject serializedObject = new SerializedObject(roomManager);
                    SerializedProperty configProperty = serializedObject.FindProperty("networkConfig");
                    if (configProperty != null)
                    {
                        configProperty.objectReferenceValue = networkConfig;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(existingPrefab);
                        Debug.Log($"✅ Updated RoomManager prefab references");
                        return;
                    }
                }
            }
        }
        
        GameObject go = new GameObject("RoomManager");
        RoomManager roomManagerComponent = go.AddComponent<RoomManager>();
        
        SerializedObject so = new SerializedObject(roomManagerComponent);
        so.FindProperty("networkConfig").objectReferenceValue = networkConfig;
        so.ApplyModifiedProperties();
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
        
        Debug.Log($"✅ Created RoomManager prefab at {prefabPath}");
    }
    
    private static void CreateMatchFlowControllerPrefab(string path, MatchConfig matchConfig)
    {
        string prefabPath = $"{path}/MatchFlowController.prefab";
        
        if (File.Exists(prefabPath))
        {
            Debug.Log($"⏭️  MatchFlowController prefab already exists, updating references...");
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                MatchFlowController controller = existingPrefab.GetComponent<MatchFlowController>();
                if (controller != null)
                {
                    SerializedObject serializedObject = new SerializedObject(controller);
                    SerializedProperty configProperty = serializedObject.FindProperty("matchConfig");
                    if (configProperty != null)
                    {
                        configProperty.objectReferenceValue = matchConfig;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(existingPrefab);
                        Debug.Log($"✅ Updated MatchFlowController prefab references");
                        return;
                    }
                }
            }
        }
        
        GameObject go = new GameObject("MatchFlowController");
        MatchFlowController controllerComponent = go.AddComponent<MatchFlowController>();
        
        SerializedObject so = new SerializedObject(controllerComponent);
        so.FindProperty("matchConfig").objectReferenceValue = matchConfig;
        so.ApplyModifiedProperties();
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
        
        Debug.Log($"✅ Created MatchFlowController prefab at {prefabPath}");
    }
    
    private static void CreateSpawnManagerPrefab(string path, GameObject playerPrefab)
    {
        string prefabPath = $"{path}/SpawnManager.prefab";
        
        if (File.Exists(prefabPath))
        {
            Debug.Log($"⏭️  SpawnManager prefab already exists, updating references...");
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existingPrefab != null)
            {
                SpawnManager spawnManager = existingPrefab.GetComponent<SpawnManager>();
                if (spawnManager != null)
                {
                    SerializedObject serializedObject = new SerializedObject(spawnManager);
                    SerializedProperty prefabProperty = serializedObject.FindProperty("playerPrefab");
                    if (prefabProperty != null && playerPrefab != null)
                    {
                        prefabProperty.objectReferenceValue = playerPrefab;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(existingPrefab);
                        Debug.Log($"✅ Updated SpawnManager prefab references");
                        return;
                    }
                }
            }
        }
        
        GameObject go = new GameObject("SpawnManager");
        SpawnManager spawnManagerComponent = go.AddComponent<SpawnManager>();
        
        SerializedObject so = new SerializedObject(spawnManagerComponent);
        if (playerPrefab != null)
        {
            so.FindProperty("playerPrefab").objectReferenceValue = playerPrefab;
        }
        so.ApplyModifiedProperties();
        
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        DestroyImmediate(go);
        
        Debug.Log($"✅ Created SpawnManager prefab at {prefabPath}");
    }
    
    private static void CreateCharacterSelectionManagerPrefab(string path)
    {
        string prefabPath = $"{path}/CharacterSelectionManager.prefab";
        
        GameObject prefab;
        CharacterSelectionManager manager;
        
        if (File.Exists(prefabPath))
        {
            Debug.Log($"⏭️  CharacterSelectionManager prefab already exists, updating character data...");
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            manager = prefab.GetComponent<CharacterSelectionManager>();
            if (manager == null)
            {
                manager = prefab.AddComponent<CharacterSelectionManager>();
            }
        }
        else
        {
            GameObject go = new GameObject("CharacterSelectionManager");
            manager = go.AddComponent<CharacterSelectionManager>();
            prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            DestroyImmediate(go);
        }
        
        // Auto-load CharacterData assets
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
                        Debug.Log($"   ✅ Loaded: {charData.characterName}");
                    }
                    serializedManager.ApplyModifiedProperties();
                    EditorUtility.SetDirty(prefab);
                    AssetDatabase.SaveAssets();
                    Debug.Log($"   ✅ Auto-loaded {guids.Length} CharacterData assets!");
                }
            }
            else
            {
                Debug.Log($"   ⚠️  No CharacterData assets found. Create them first!");
            }
        }
        
        Debug.Log($"✅ CharacterSelectionManager prefab ready at {prefabPath}");
    }
    
    private static void CreateCharacterLoaderPrefab(string path)
    {
        string prefabPath = $"{path}/CharacterLoader.prefab";
        
        GameObject prefab;
        CharacterLoader loader;
        
        if (File.Exists(prefabPath))
        {
            Debug.Log($"⏭️  CharacterLoader prefab already exists, updating...");
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            loader = prefab.GetComponent<CharacterLoader>();
            if (loader == null)
            {
                loader = prefab.AddComponent<CharacterLoader>();
            }
        }
        else
        {
            GameObject go = new GameObject("CharacterLoader");
            loader = go.AddComponent<CharacterLoader>();
            prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            DestroyImmediate(go);
        }
        
        Debug.Log($"✅ CharacterLoader prefab ready at {prefabPath}");
    }
    
    /// <summary>
    /// Find Player prefab specifically in Prefabs/Player folder.
    /// </summary>
    private static GameObject FindPlayerPrefab()
    {
        // Try exact path first
        string exactPath = "Assets/Prefabs/Player/Player.prefab";
        if (File.Exists(exactPath))
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>(exactPath);
        }
        
        // Fallback: search for prefabs with exact name "Player" (not containing "Player")
        string[] guids = AssetDatabase.FindAssets("t:Prefab Player");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            // Only match exact "Player" name, and prefer Prefabs/Player folder
            if (fileName == "Player")
            {
                if (path.Contains("Prefabs/Player"))
                {
                    return AssetDatabase.LoadAssetAtPath<GameObject>(path);
                }
            }
        }
        
        // Last resort: return first "Player" prefab found
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            if (fileName == "Player")
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Find asset by type and name pattern.
    /// </summary>
    private static T FindAsset<T>(string namePattern) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name} {namePattern}");
        
        if (guids.Length == 0)
        {
            // Try without name pattern
            guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        }
        
        if (guids.Length > 0)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }
        
        return null;
    }
}
#endif

