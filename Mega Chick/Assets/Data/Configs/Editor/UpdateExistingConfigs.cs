#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Updates existing config assets with new fields.
/// Why? Script only creates new assets, doesn't update existing ones.
/// </summary>
public class UpdateExistingConfigs : EditorWindow
{
    [MenuItem("Mega Chick/Update Existing Configs")]
    public static void UpdateConfigs()
    {
        string configPath = "Assets/Data/Configs";
        
        // Update MatchConfig
        MatchConfig matchConfig = AssetDatabase.LoadAssetAtPath<MatchConfig>($"{configPath}/MatchConfig.asset");
        if (matchConfig != null)
        {
            // New field: respawnDelay (default 2f if not set)
            SerializedObject so = new SerializedObject(matchConfig);
            SerializedProperty prop = so.FindProperty("respawnDelay");
            if (prop != null && prop.floatValue == 0f)
            {
                prop.floatValue = 2f;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(matchConfig);
            Debug.Log("✅ Updated MatchConfig");
        }
        
        // Update MovementConfig
        MovementConfig movementConfig = AssetDatabase.LoadAssetAtPath<MovementConfig>($"{configPath}/MovementConfig.asset");
        if (movementConfig != null)
        {
            // New fields: weaponKnockbackPower, weaponCooldown
            SerializedObject so = new SerializedObject(movementConfig);
            SerializedProperty prop1 = so.FindProperty("weaponKnockbackPower");
            SerializedProperty prop2 = so.FindProperty("weaponCooldown");
            if (prop1 != null && prop1.floatValue == 0f)
            {
                prop1.floatValue = 15f;
            }
            if (prop2 != null && prop2.floatValue == 0f)
            {
                prop2.floatValue = 4f;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(movementConfig);
            Debug.Log("✅ Updated MovementConfig");
        }
        
        // Update RespawnConfig
        RespawnConfig respawnConfig = AssetDatabase.LoadAssetAtPath<RespawnConfig>($"{configPath}/RespawnConfig.asset");
        if (respawnConfig != null)
        {
            // New field: spawnProtectionDuration
            SerializedObject so = new SerializedObject(respawnConfig);
            SerializedProperty prop = so.FindProperty("spawnProtectionDuration");
            if (prop != null && prop.floatValue == 0f)
            {
                prop.floatValue = 1f;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(respawnConfig);
            Debug.Log("✅ Updated RespawnConfig");
        }
        
        // Update RaceConfig
        RaceConfig raceConfig = AssetDatabase.LoadAssetAtPath<RaceConfig>($"{configPath}/RaceConfig.asset");
        if (raceConfig != null)
        {
            // New field: allPlayersTransformed
            SerializedObject so = new SerializedObject(raceConfig);
            SerializedProperty prop = so.FindProperty("allPlayersTransformed");
            if (prop != null)
            {
                prop.boolValue = true; // Default to true
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(raceConfig);
            Debug.Log("✅ Updated RaceConfig");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Update Complete",
            "Existing configs have been updated with new fields!\n\n" +
            "Check Assets/Data/Configs/ folder.",
            "OK");
    }
}
#endif

