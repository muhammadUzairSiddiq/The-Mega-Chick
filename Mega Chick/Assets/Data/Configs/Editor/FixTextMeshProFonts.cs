#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// Fixes all TextMeshPro components in the scene that have corrupted/missing fonts.
/// </summary>
public class FixTextMeshProFonts : EditorWindow
{
    [MenuItem("Mega Chick/Fix TextMeshPro Fonts in Scene")]
    public static void FixFontsInScene()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        int fixedCount = 0;
        
        TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont == null)
        {
            // Try to find any TMP font
            string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                string fontPath = AssetDatabase.GUIDToAssetPath(fontGuids[0]);
                defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            }
        }
        
        if (defaultFont == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "Could not find any TextMeshPro font asset!\n\n" +
                "Please import TextMeshPro Essentials first:\n" +
                "Window > TextMeshPro > Import TMP Essential Resources",
                "OK");
            return;
        }
        
        // Find default material for the font
        Material defaultMaterial = null;
        if (defaultFont != null && defaultFont.material != null)
        {
            defaultMaterial = defaultFont.material;
        }
        else
        {
            // Try to find material preset
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");
            foreach (string guid in materialGuids)
            {
                string matPath = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                if (mat != null && mat.name.Contains("SDF") && mat.name.Contains(defaultFont.name))
                {
                    defaultMaterial = mat;
                    break;
                }
            }
        }
        
        foreach (TextMeshProUGUI text in allTexts)
        {
            bool needsFix = false;
            
            // Check if font is missing or using fallback
            if (text.font == null || text.font.name.Contains("Fallback"))
            {
                text.font = defaultFont;
                needsFix = true;
            }
            
            // Check if material/shared material is missing
            if (text.fontSharedMaterial == null || (defaultFont != null && text.fontSharedMaterial != defaultFont.material))
            {
                if (defaultFont != null && defaultFont.material != null)
                {
                    text.fontSharedMaterial = defaultFont.material;
                    needsFix = true;
                }
                else if (defaultMaterial != null)
                {
                    text.fontSharedMaterial = defaultMaterial;
                    needsFix = true;
                }
            }
            
            // Always force update to refresh the text
            text.ForceMeshUpdate();
            
            if (needsFix)
            {
                fixedCount++;
            }
            
            // Mark object as dirty (this will mark the scene as dirty automatically)
            EditorUtility.SetDirty(text);
            if (text.gameObject.scene.IsValid())
            {
                EditorUtility.SetDirty(text.gameObject);
            }
        }
        
        EditorUtility.DisplayDialog("Font Fix Complete", 
            $"Fixed {fixedCount} TextMeshPro component(s)!\n\n" +
            $"Assigned font: {defaultFont.name}",
            "OK");
        
        Debug.Log($"✅ Fixed {fixedCount} TextMeshPro component(s) with font: {defaultFont.name}");
    }
    
    [MenuItem("Mega Chick/Fix TextMeshPro Fonts in Scene", true)]
    public static bool ValidateFixFonts()
    {
        return Application.isPlaying == false;
    }
}
#endif

