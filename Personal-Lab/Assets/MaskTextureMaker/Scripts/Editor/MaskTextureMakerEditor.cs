using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

[CustomEditor(typeof(MaskTextureMaker))]
public class MaskedTextureMakerEditor : Editor
{
    private void OnEnable()
    {
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

    private long totalMemoryUsage = 0;
    public override void OnInspectorGUI()
    {
        GUIStyle boldStyle = new GUIStyle("WhiteLabel");
        boldStyle.fontStyle = FontStyle.Bold;
        //base.OnInspectorGUI();
        var loadedDatas = MaskTextureData.maskedTextures;
        if (loadedDatas != null)
        {
            if (loadedDatas.Count > 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField($"Memory Usage: {GetMemoryString(totalMemoryUsage)}", boldStyle);

                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField($"Loaded Mask Texture2D List (Count: {loadedDatas.Count})");

                totalMemoryUsage = 0;
                int index = 1;
                var enumerator = loadedDatas.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    var instanceId = enumerator.Current.Key;
                    var texture2D = enumerator.Current.Value;
                    EditorGUILayout.BeginHorizontal("HelpBox");

                    string path = AssetDatabase.GetAssetPath(instanceId);
                    var maskTextureData = AssetDatabase.LoadAssetAtPath<MaskTextureData>(path);
                    bool isButtonClick = false;
                    if (maskTextureData != null)
                    {
                        if (maskTextureData.IsAvailable)
                        {
                            if (texture2D != null)
                            {
                                long memory = Profiler.GetRuntimeMemorySizeLong(texture2D);
                                totalMemoryUsage += memory;
                                isButtonClick = GUILayout.Button($"{index}. {maskTextureData.name} ({GetMemoryString(memory)})", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f));
                            }
                            else
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                isButtonClick = GUILayout.Button($"{index}. {maskTextureData.name} Loading...", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f));
                                EditorGUI.EndDisabledGroup();
                            }
                        }
                        else
                        {
                            GUI.backgroundColor = Color.red;
                            isButtonClick = GUILayout.Button($"{index}. {maskTextureData.name} (Could not load)", GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.4f));
                            GUI.backgroundColor = Color.white;
                        }
                    }

                    if (isButtonClick)
                    {
                        MaskTextureDataEditor.previousSelection = target;
                        Selection.activeObject = maskTextureData;
                    }

                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.ObjectField(texture2D, typeof(Texture2D), false, GUILayout.Width(EditorGUIUtility.currentViewWidth * 0.5f));
                    EditorGUI.EndDisabledGroup();

                    ++index;
                    EditorGUILayout.EndHorizontal();
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
