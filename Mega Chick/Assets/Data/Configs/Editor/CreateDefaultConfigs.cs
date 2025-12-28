#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility to automatically create default config assets.
/// Why this? Makes it easy to create all configs with one click instead of manually.
/// </summary>
public class CreateDefaultConfigs : EditorWindow
{
    [MenuItem("Mega Chick/Create Default Configs")]
    public static void CreateAllConfigs()
    {
        string configPath = "Assets/Data/Configs";
        
        // Ensure directory exists
        if (!Directory.Exists(configPath))
        {
            Directory.CreateDirectory(configPath);
            AssetDatabase.Refresh();
        }
        
        // Create MatchConfig
        CreateConfigIfNotExists<MatchConfig>(configPath, "MatchConfig");
        
        // Create MovementConfig
        CreateConfigIfNotExists<MovementConfig>(configPath, "MovementConfig");
        
        // Create RespawnConfig
        CreateConfigIfNotExists<RespawnConfig>(configPath, "RespawnConfig");
        
        // Create NetworkConfig
        CreateConfigIfNotExists<NetworkConfig>(configPath, "NetworkConfig");
        
        // Create RaceConfig
        CreateConfigIfNotExists<RaceConfig>(configPath, "RaceConfig");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("✅ Default configs created successfully in Assets/Data/Configs/");
        EditorUtility.DisplayDialog("Success", 
            "All default configs have been created!\n\n" +
            "Check Assets/Data/Configs/ folder.", 
            "OK");
    }
    
    private static void CreateConfigIfNotExists<T>(string path, string name) where T : ScriptableObject
    {
        string assetPath = $"{path}/{name}.asset";
        
        if (File.Exists(assetPath))
        {
            Debug.Log($"⏭️  {name} already exists, skipping...");
            return;
        }
        
        T config = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(config, assetPath);
        Debug.Log($"✅ Created {name}");
    }
}
#endif

