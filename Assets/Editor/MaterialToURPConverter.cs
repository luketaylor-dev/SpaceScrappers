using UnityEngine;
using UnityEditor;
using System.Linq;

public class MaterialToURPConverter : EditorWindow
{
    [MenuItem("Tools/Convert Materials to URP")]
    public static void ConvertMaterialsToURP()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int convertedCount = 0;
        int skippedCount = 0;

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null) continue;

            Shader shader = material.shader;
            if (shader == null) continue;

            string shaderName = shader.name;

            if (shaderName.Contains("Standard") || shaderName.Contains("Legacy Shaders"))
            {
                Shader urpShader = null;

                if (shaderName.Contains("Standard") || shaderName.Contains("Diffuse"))
                {
                    urpShader = Shader.Find("Universal Render Pipeline/Lit");
                }
                else if (shaderName.Contains("Unlit"))
                {
                    urpShader = Shader.Find("Universal Render Pipeline/Unlit");
                }
                else if (shaderName.Contains("Transparent"))
                {
                    urpShader = Shader.Find("Universal Render Pipeline/Lit");
                }
                else if (shaderName.Contains("Specular"))
                {
                    urpShader = Shader.Find("Universal Render Pipeline/Lit");
                }
                else
                {
                    urpShader = Shader.Find("Universal Render Pipeline/Lit");
                }

                if (urpShader != null)
                {
                    material.shader = urpShader;
                    EditorUtility.SetDirty(material);
                    convertedCount++;
                    Debug.Log($"Converted: {path}");
                }
                else
                {
                    Debug.LogWarning($"Could not find URP shader for: {path} (Shader: {shaderName})");
                    skippedCount++;
                }
            }
            else if (!shaderName.Contains("Universal Render Pipeline") && !shaderName.Contains("Hidden"))
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null)
                {
                    material.shader = urpShader;
                    EditorUtility.SetDirty(material);
                    convertedCount++;
                    Debug.Log($"Converted: {path}");
                }
            }
            else
            {
                skippedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Material Conversion Complete",
            $"Converted {convertedCount} materials to URP.\nSkipped {skippedCount} materials.",
            "OK");

        Debug.Log($"Material conversion complete. Converted: {convertedCount}, Skipped: {skippedCount}");
    }
}





