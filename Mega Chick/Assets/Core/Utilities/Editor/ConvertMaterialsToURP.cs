#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Utility to convert materials from Built-in shaders to URP shaders.
/// Why? Pink materials = missing/incompatible shaders. URP needs URP shaders.
/// </summary>
public class ConvertMaterialsToURP : EditorWindow
{
    private List<Material> materialsToConvert = new List<Material>();
    private Vector2 scrollPosition;
    
    [MenuItem("Mega Chick/Convert Materials to URP")]
    public static void ShowWindow()
    {
        GetWindow<ConvertMaterialsToURP>("Convert Materials to URP");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("Convert Materials to URP", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This will convert selected materials to use URP Lit shader.\n" +
            "Pink materials = missing/incompatible shaders.\n" +
            "Select materials in Project window, then click 'Find Selected Materials'.",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Find Selected Materials", GUILayout.Height(30)))
        {
            FindSelectedMaterials();
        }
        
        if (GUILayout.Button("Find All Materials in Project", GUILayout.Height(30)))
        {
            FindAllMaterials();
        }
        
        GUILayout.Space(10);
        
        if (materialsToConvert.Count > 0)
        {
            GUILayout.Label($"Found {materialsToConvert.Count} materials:", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var mat in materialsToConvert)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField(mat, typeof(Material), false);
                
                // Show current shader
                if (mat.shader != null)
                {
                    EditorGUILayout.LabelField(mat.shader.name, GUILayout.Width(200));
                }
                else
                {
                    EditorGUILayout.LabelField("MISSING SHADER", EditorStyles.boldLabel, GUILayout.Width(200));
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Convert All to URP Lit", GUILayout.Height(40)))
            {
                ConvertAllToURP();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No materials found. Select materials in Project window and click 'Find Selected Materials'.", MessageType.Warning);
        }
    }
    
    private void FindSelectedMaterials()
    {
        materialsToConvert.Clear();
        
        foreach (Object obj in Selection.objects)
        {
            if (obj is Material)
            {
                materialsToConvert.Add(obj as Material);
            }
        }
        
        Debug.Log($"Found {materialsToConvert.Count} selected materials.");
    }
    
    private void FindAllMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        materialsToConvert.Clear();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null)
            {
                materialsToConvert.Add(mat);
            }
        }
        
        Debug.Log($"Found {materialsToConvert.Count} materials in project.");
    }
    
    private void ConvertAllToURP()
    {
        int converted = 0;
        int failed = 0;
        
        // Find URP Lit shader
        Shader urpLitShader = Shader.Find("Universal Render Pipeline/Lit");
        
        if (urpLitShader == null)
        {
            EditorUtility.DisplayDialog("Error", 
                "URP Lit shader not found!\n\n" +
                "Make sure you're using URP and the shader is available.\n" +
                "Try: Shader.Find(\"Universal Render Pipeline/Lit\")",
                "OK");
            return;
        }
        
        foreach (Material mat in materialsToConvert)
        {
            try
            {
                // Store original properties
                Texture mainTex = null;
                Color mainColor = Color.white;
                
                // Try to get main texture (different shaders use different property names)
                if (mat.HasProperty("_MainTex"))
                    mainTex = mat.GetTexture("_MainTex");
                else if (mat.HasProperty("_BaseMap"))
                    mainTex = mat.GetTexture("_BaseMap");
                else if (mat.HasProperty("_BaseColorMap"))
                    mainTex = mat.GetTexture("_BaseColorMap");
                
                // Try to get main color
                if (mat.HasProperty("_Color"))
                    mainColor = mat.GetColor("_Color");
                else if (mat.HasProperty("_BaseColor"))
                    mainColor = mat.GetColor("_BaseColor");
                
                // Change shader
                mat.shader = urpLitShader;
                
                // Apply properties to URP shader
                if (mainTex != null)
                {
                    mat.SetTexture("_BaseMap", mainTex);
                }
                
                mat.SetColor("_BaseColor", mainColor);
                
                // Set default values
                mat.SetFloat("_Smoothness", 0.5f);
                mat.SetFloat("_Metallic", 0f);
                
                converted++;
                Debug.Log($"Converted: {mat.name}");
            }
            catch (System.Exception e)
            {
                failed++;
                Debug.LogError($"Failed to convert {mat.name}: {e.Message}");
            }
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("Conversion Complete",
            $"Converted: {converted}\n" +
            $"Failed: {failed}\n\n" +
            "Materials have been updated to URP Lit shader.",
            "OK");
        
        materialsToConvert.Clear();
    }
}
#endif

