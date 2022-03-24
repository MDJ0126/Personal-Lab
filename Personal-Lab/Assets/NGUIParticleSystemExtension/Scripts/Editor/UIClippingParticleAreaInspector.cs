using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIClippingParticleArea))]
public class UIClippingParticleAreaInspector : Editor
{
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();

        GUIStyle boldStyle = new GUIStyle("WhiteLabel");
        boldStyle.fontStyle = FontStyle.Bold;

        SerializedProperty spriteMask = serializedObject.FindProperty("mSpriteMask");

        EditorGUILayout.LabelField("Clipping Area", boldStyle);
        EditorGUILayout.BeginHorizontal("HelpBox");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField(spriteMask, new GUIContent(""));
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();
    }
}
