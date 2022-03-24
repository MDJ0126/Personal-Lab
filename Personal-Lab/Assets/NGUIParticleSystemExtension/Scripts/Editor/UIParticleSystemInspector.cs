//-------------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2019 Tasharen Entertainment Inc
//-------------------------------------------------

using UnityEngine;
using UnityEditor;

/// <summary>
/// Inspector class used to edit UITextureCustomInspector.
/// </summary>

[CanEditMultipleObjects]
[CustomEditor(typeof(UIParticleSystem), true)]
public class UIParticleSystemInspector : Editor
{
    UIParticleSystem mParticleSystem;

    private void OnEnable()
    {
        mParticleSystem = target as UIParticleSystem;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        GUILayout.Space(3f);

        var isPrefab = NGUIEditorTools.IsPrefab(mParticleSystem.gameObject) && !NGUIEditorTools.IsPrefabInstance(mParticleSystem.gameObject);
        if (NGUIEditorTools.DrawHeader("Only Depth"))
        {
            NGUIEditorTools.BeginContents();
            DrawDepth(serializedObject, mParticleSystem, isPrefab);
            NGUIEditorTools.EndContents();
        }
    }

    /// <summary>
    /// Draw widget's depth.
    /// </summary>

    private void DrawDepth(SerializedObject so, UIWidget w, bool isPrefab)
    {
        if (isPrefab) return;

        GUILayout.Space(2f);
        GUILayout.BeginHorizontal();
        {
            EditorGUILayout.PrefixLabel("Depth");

            if (GUILayout.Button("Back", GUILayout.MinWidth(46f)))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    UIWidget pw = go.GetComponent<UIWidget>();
                    if (pw != null) pw.depth = w.depth - 1;
                }
            }

            NGUIEditorTools.DrawProperty("", so, "mDepth", GUILayout.MinWidth(20f));

            if (GUILayout.Button("Forward", GUILayout.MinWidth(60f)))
            {
                foreach (GameObject go in Selection.gameObjects)
                {
                    UIWidget pw = go.GetComponent<UIWidget>();
                    if (pw != null) pw.depth = w.depth + 1;
                }
            }
        }
        GUILayout.EndHorizontal();

        int matchingDepths = 1;

        UIPanel p = w.panel;

        if (p != null)
        {
            for (int i = 0, imax = p.widgets.Count; i < imax; ++i)
            {
                UIWidget pw = p.widgets[i];
                if (pw != w && pw.depth == w.depth)
                    ++matchingDepths;
            }
        }

        if (matchingDepths > 1)
        {
            EditorGUILayout.HelpBox(matchingDepths + " widgets are sharing the depth value of " + w.depth, MessageType.Info);
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
