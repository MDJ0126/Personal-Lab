using UnityEditor;
using UnityEngine;
using static MaskTextureData;

[CustomEditor(typeof(MaskTextureData)), CanEditMultipleObjects]
public class MaskTextureDataEditor : Editor
{
    private MaskTextureData data = null;

    private Texture2D previewTexture2D = null;

    private int SelectedObjectCount => targets.Length;
    private bool IsSingleSelectionObject => SelectedObjectCount == 1;

    SerializedProperty texture;
    SerializedProperty maskTexture;
    SerializedProperty coordinate;
    SerializedProperty scale;
    SerializedProperty flipMode;

    public static Object previousSelection = null;

    private void OnEnable()
    {
        data = target as MaskTextureData;
        texture = serializedObject.FindProperty("texture");
        maskTexture = serializedObject.FindProperty("maskTexture");
        coordinate = serializedObject.FindProperty("coordinate");
        scale = serializedObject.FindProperty("scale");
        flipMode = serializedObject.FindProperty("flipMode");
        
        SetPreviewTexture();
        Undo.undoRedoPerformed = UndoRedoPerformed;
    }

    private void UndoRedoPerformed()
    {
        data.CreateRawData(() =>
        {
            SetPreviewTexture();
        });
    }

    private void SetPreviewTexture()
    {
        if (IsSingleSelectionObject)
        {
            bool isRefresh = !Application.isPlaying;
            data.RequestMaskTexture((texture2D) =>
            {
                previewTexture2D = texture2D;
                Repaint();
            }, isRefresh: isRefresh);
        }
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        serializedObject.Update();

        if (IsSingleSelectionObject)
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("MaskTextureData", data, typeof(MaskTextureData), false);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.LabelField($"Multiple data was selected. ({SelectedObjectCount} Datas)", new GUIStyle("WhiteLabel"));
        }

        EditorGUILayout.BeginVertical("Box");
        EditorGUILayout.ObjectField(texture, typeof(Texture));
        EditorGUILayout.ObjectField(maskTexture, typeof(Texture));

        if (texture.objectReferenceValue != null && maskTexture.objectReferenceValue != null)
        {
            EditorGUILayout.Space(10f);

            EditorGUILayout.BeginVertical("HelpBox");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Optional", new GUIStyle("WhiteLabel"));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(coordinate);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Slider(scale, 0.1f, 5f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (IsSingleSelectionObject)
            {
                EditorGUILayout.PrefixLabel("Flip");
                bool isFlipX = ((FlipMode)flipMode.intValue & FlipMode.X) == FlipMode.X;
                isFlipX = EditorGUILayout.Toggle(isFlipX, GUILayout.Width(15f));
                EditorGUILayout.LabelField("X", GUILayout.Width(20f));

                bool isFlipY = ((FlipMode)flipMode.intValue & FlipMode.Y) == FlipMode.Y;
                isFlipY = EditorGUILayout.Toggle(isFlipY, GUILayout.Width(15f));
                EditorGUILayout.LabelField("Y", GUILayout.Width(20f));
                if (GUI.changed)
                {
                    if (isFlipX)
                        flipMode.intValue |= (int)FlipMode.X;
                    else
                        flipMode.intValue &= ~(int)FlipMode.X;

                    if (isFlipY)
                        flipMode.intValue |= (int)FlipMode.Y;
                    else
                        flipMode.intValue &= ~(int)FlipMode.Y;
                }
            }
            else
                EditorGUILayout.PropertyField(flipMode, new GUIContent("Flip"));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndVertical();

        // Undo
        if (previousSelection != null)
        {
            EditorGUILayout.Space(3f);
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button($"<< Return to '{previousSelection.name}'"))
            {
                GameObject go = previousSelection as GameObject;
                if (go != null)
                {
                    Selection.activeGameObject = go;
                }
                else
                {
                    Selection.activeObject = previousSelection;
                }
                previousSelection = null;
            }
            GUI.backgroundColor = Color.white;
        }

        if (GUI.changed)
        {
            serializedObject.ApplyModifiedProperties();
            data.CreateRawData(() =>
            {
                serializedObject.ApplyModifiedProperties();
                SetPreviewTexture();
            });
        }
    }
    
    public override bool HasPreviewGUI() => IsSingleSelectionObject;
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (previewTexture2D != null)
        {
            var texture = previewTexture2D;
            var mask = maskTexture.objectReferenceValue as Texture2D;

            Rect outerRect = r;
            if (mask != null)
            {
                outerRect.width = mask.width;
                outerRect.height = mask.height;
            }
            else
            {
                outerRect.width = texture.width;
                outerRect.height = texture.height;
            }

            if (outerRect.width > 0)
            {
                float f = r.width / outerRect.width;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (r.height > outerRect.height)
            {
                outerRect.y += (r.height - outerRect.height) * 0.5f;
            }
            else if (outerRect.height > r.height)
            {
                float f = r.height / outerRect.height;
                outerRect.width *= f;
                outerRect.height *= f;
            }

            if (r.width > outerRect.width) outerRect.x += (r.width - outerRect.width) * 0.5f;

            DrawTiledTexture(outerRect, CreateCheckerTex(new Color(0.1f, 0.1f, 0.1f, 0.5f), new Color(0.2f, 0.2f, 0.2f, 0.5f)));

            GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);

            if (mask != null)
            {
                // Title label
                string infoText = "Masked Texture Example";
                EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight), infoText);

                // Size label
                string sizeText = string.Format("Masked Size: {0}x{1}", Mathf.RoundToInt(mask.width), Mathf.RoundToInt(mask.height));
                EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight), sizeText);
            }
            else
                EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, EditorGUIUtility.singleLineHeight), "Need mask texture.");
        }
    }

    /// <summary>
    /// Create a checker-background texture
    /// </summary>
    /// <param name="c0"></param>
    /// <param name="c1"></param>
    /// <returns></returns>
    private Texture2D CreateCheckerTex(Color c0, Color c1)
    {
        Texture2D tex = new Texture2D(16, 16);
        tex.name = "[Generated] Checker Texture";
        tex.hideFlags = HideFlags.DontSave;

        for (int y = 0; y < 8; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c1);
        for (int y = 8; y < 16; ++y) for (int x = 0; x < 8; ++x) tex.SetPixel(x, y, c0);
        for (int y = 0; y < 8; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c0);
        for (int y = 8; y < 16; ++y) for (int x = 8; x < 16; ++x) tex.SetPixel(x, y, c1);

        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;
        return tex;
    }

    /// <summary>
    /// Draws the tiled texture. Like GUI.DrawTexture() but tiled instead of stretched.
    /// </summary>
    private void DrawTiledTexture(Rect rect, Texture tex)
    {
        GUI.BeginGroup(rect);
        {
            int width = Mathf.RoundToInt(rect.width);
            int height = Mathf.RoundToInt(rect.height);

            for (int y = 0; y < height; y += tex.height)
            {
                for (int x = 0; x < width; x += tex.width)
                {
                    GUI.DrawTexture(new Rect(x, y, tex.width, tex.height), tex);
                }
            }
        }
        GUI.EndGroup();
    }
}