#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Fixes TextMeshPro materials that were incorrectly converted to URP Lit.
/// TextMeshPro requires specific shaders (TextMeshPro/Distance Field), not URP Lit.
/// </summary>
public class FixTextMeshProMaterials : EditorWindow
{
    [MenuItem("Mega Chick/Fix TextMeshPro Materials (URP Fix)")]
    public static void FixMaterials()
    {
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        int fixedCount = 0;
        int skippedCount = 0;
        
        Dictionary<TMP_FontAsset, Material> fontMaterialCache = new Dictionary<TMP_FontAsset, Material>();
        
        foreach (TextMeshProUGUI text in allTexts)
        {
            if (text.font == null)
            {
                skippedCount++;
                continue;
            }
            
            // Check if material is using wrong shader (must be TextMeshPro/Distance Field)
            Material currentMaterial = text.fontSharedMaterial;
            bool needsFix = false;
            
            if (currentMaterial != null)
            {
                string shaderName = currentMaterial.shader.name;
                // TextMeshPro materials MUST use TextMeshPro/Distance Field (not HDRP, not URP Lit, etc.)
                if (shaderName != "TextMeshPro/Distance Field" && shaderName != "TextMeshPro/Mobile/Distance Field")
                {
                    needsFix = true;
                    Debug.Log($"⚠️ Found TextMeshPro with wrong shader: {shaderName} on {text.gameObject.name} - needs TextMeshPro/Distance Field");
                }
            }
            else
            {
                needsFix = true;
            }
            
            if (needsFix)
            {
                Material correctMaterial = null;
                
                // Check cache first
                if (fontMaterialCache.ContainsKey(text.font))
                {
                    correctMaterial = fontMaterialCache[text.font];
                }
                else
                {
                    // Try to get material from font asset
                    if (text.font.material != null)
                    {
                        correctMaterial = text.font.material;
                    }
                    else
                    {
                        // Try to find material with font name that uses correct shader
                        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
                        foreach (string guid in materialGuids)
                        {
                            string matPath = AssetDatabase.GUIDToAssetPath(guid);
                            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
                            if (mat != null && (mat.shader.name == "TextMeshPro/Distance Field" || mat.shader.name == "TextMeshPro/Mobile/Distance Field"))
                            {
                                // Check if material name matches font name
                                if (mat.name.Contains(text.font.name) || 
                                    (text.font.name.Contains("LiberationSans") && mat.name.Contains("LiberationSans")))
                                {
                                    correctMaterial = mat;
                                    break;
                                }
                            }
                        }
                        
                        // If still not found, create a new material with correct shader
                        if (correctMaterial == null)
                        {
                            correctMaterial = CreateTextMeshProMaterial(text.font);
                        }
                    }
                    
                    // Cache the material
                    if (correctMaterial != null)
                    {
                        fontMaterialCache[text.font] = correctMaterial;
                    }
                }
                
                if (correctMaterial != null)
                {
                    text.fontSharedMaterial = correctMaterial;
                    text.ForceMeshUpdate();
                    fixedCount++;
                    
                    EditorUtility.SetDirty(text);
                    if (text.gameObject.scene.IsValid())
                    {
                        EditorUtility.SetDirty(text.gameObject);
                    }
                    
                    Debug.Log($"✅ Fixed material for {text.gameObject.name} - Assigned: {correctMaterial.name}");
                }
                else
                {
                    Debug.LogWarning($"❌ Could not find/create correct material for {text.gameObject.name} with font: {text.font.name}");
                }
            }
        }
        
        string message = $"Fixed {fixedCount} TextMeshPro material(s)!\n\n";
        if (skippedCount > 0)
        {
            message += $"Skipped {skippedCount} component(s) with no font assigned.";
        }
        
        EditorUtility.DisplayDialog("Material Fix Complete", message, "OK");
        Debug.Log($"✅ Fixed {fixedCount} TextMeshPro material(s). Skipped {skippedCount}.");
    }
    
    /// <summary>
    /// Creates a new TextMeshPro material with the correct shader.
    /// </summary>
    private static Material CreateTextMeshProMaterial(TMP_FontAsset font)
    {
        // Use TextMeshPro/Distance Field shader (works with URP)
        Shader tmpShader = Shader.Find("TextMeshPro/Distance Field");
        if (tmpShader == null)
        {
            // Fallback to mobile version
            tmpShader = Shader.Find("TextMeshPro/Mobile/Distance Field");
        }
        if (tmpShader == null)
        {
            Debug.LogError("❌ Could not find TextMeshPro/Distance Field shader! Please ensure TextMeshPro is properly imported.");
            return null;
        }
        
        // Create new material
        Material newMaterial = new Material(tmpShader);
        newMaterial.name = $"{font.name} Material";
        
        // Set font atlas texture
        if (font.atlasTexture != null)
        {
            newMaterial.SetTexture("_MainTex", font.atlasTexture);
        }
        
        // Set default TMP material properties
        newMaterial.SetFloat("_TextureWidth", font.atlasWidth);
        newMaterial.SetFloat("_TextureHeight", font.atlasHeight);
        newMaterial.SetFloat("_GradientScale", font.atlasPadding + 1);
        newMaterial.SetFloat("_WeightNormal", font.normalStyle);
        newMaterial.SetFloat("_WeightBold", font.boldStyle);
        
        // Save material to project
        string materialPath = $"Assets/Materials/{newMaterial.name}.mat";
        if (!System.IO.Directory.Exists("Assets/Materials"))
        {
            System.IO.Directory.CreateDirectory("Assets/Materials");
        }
        
        AssetDatabase.CreateAsset(newMaterial, materialPath);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"✅ Created new TextMeshPro material: {materialPath}");
        return newMaterial;
    }
    
    [MenuItem("Mega Chick/Fix TextMeshPro Materials (URP Fix)", true)]
    public static bool ValidateFixMaterials()
    {
        return Application.isPlaying == false;
    }
}
#endif

