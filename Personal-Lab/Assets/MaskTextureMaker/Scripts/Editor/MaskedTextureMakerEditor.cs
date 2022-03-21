using System;
using MaskTextureMaker;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[CustomEditor(typeof(MaskedTextureMaker))]
public class MaskedTextureMakerEditor : Editor
{
    GUIStyle boldStyle = new GUIStyle("WhiteLabel");

    private void OnEnable()
    {
        boldStyle.fontStyle = FontStyle.Bold;
        EditorApplication.update += OnUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnUpdate;
    }

    private void OnUpdate()
    {
        Repaint();
    }

    private long totalMemory = 0;
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        var loadedDatas = MaskTextureData.maskedTextures;
        if (loadedDatas != null)
        {

            if (loadedDatas.Count > 0)
            {
                EditorGUILayout.LabelField($"Total Memory: {GetMemoryString(totalMemory)}", boldStyle);
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField($"Loaded Mask Texture2D List (Count: {loadedDatas.Count})");

                totalMemory = 0;
                int index = 1;
                var enumerator = loadedDatas.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var instanceId = enumerator.Current.Key;
                    var texture2D = enumerator.Current.Value;
                    if (texture2D != null)
                    {
                        long memory = Profiler.GetRuntimeMemorySizeLong(texture2D);
                        totalMemory += memory;
                        EditorGUILayout.BeginHorizontal("HelpBox");
                        string path = AssetDatabase.GetAssetPath(instanceId);
                        var maskTextureData = AssetDatabase.LoadAssetAtPath<MaskTextureData>(path);
                        if (maskTextureData != null)
                        {
                            EditorGUILayout.PrefixLabel($"{index}. {maskTextureData.name} ({GetMemoryString(memory)})");
                            EditorGUILayout.ObjectField(texture2D, typeof(Texture2D), false);
                            EditorGUILayout.EndHorizontal();
                            ++index;
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                EditorGUILayout.LabelField($"No Loaded Mask Textures.", boldStyle);
            }
        }
    }

    private string GetMemoryString(long memory)
    {
        float temp = memory;
        string memoryStr = string.Empty;
        int pow = 0;
        while (temp > 1024f)
        {
            temp /= 1024f;
            pow++;
        }
        memoryStr = temp.ToString("F2");
        switch (pow)
        {
            case 1:
                memoryStr += "KB";
                break;
            case 2:
                memoryStr += "MB";
                break;
            case 3:
                memoryStr += "GB";
                break;
            case 4:
                memoryStr += "TB";
                break;
            default:
                memoryStr += "B";
                break;
        }
        return memoryStr;
    }
}
