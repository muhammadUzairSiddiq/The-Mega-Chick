#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Photon.Pun;

/// <summary>
/// Auto-setup Photon configuration.
/// </summary>
public class AutoSetupPhoton : EditorWindow
{
    [MenuItem("Mega Chick/Auto Setup Photon")]
    public static void SetupPhoton()
    {
        ServerSettings settings = PhotonNetwork.PhotonServerSettings;
        
        if (settings == null)
        {
            Debug.LogError("❌ PhotonServerSettings not found!");
            EditorUtility.DisplayDialog("Error", "PhotonServerSettings not found!\n\nPlease install Photon PUN2 from Asset Store first.", "OK");
            return;
        }
        
        // Enable lobby statistics (required for room operations)
        settings.AppSettings.EnableLobbyStatistics = true;
        
        // Set network logging
        settings.AppSettings.NetworkLogging = ExitGames.Client.Photon.DebugLevel.INFO;
        
        // Enable run in background
        settings.RunInBackground = true;
        
        EditorUtility.SetDirty(settings);
        AssetDatabase.SaveAssets();
        
        Debug.Log("✅ Photon auto-setup complete!");
        Debug.Log($"   App ID: {settings.AppSettings.AppIdRealtime}");
        Debug.Log($"   Lobby Statistics: {settings.AppSettings.EnableLobbyStatistics}");
        
        EditorUtility.DisplayDialog("Photon Setup Complete",
            "Photon configuration updated!\n\n" +
            "✅ Lobby Statistics enabled\n" +
            "✅ Network logging enabled\n" +
            "✅ Run in background enabled\n\n" +
            "Ready to use!",
            "OK");
    }
}
#endif

