using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaskTextureData)), CanEditMultipleObjects]
public class MaskTextureDataEditor : Editor
{
    private MaskTextureData _maskTextureData = null;

    private Texture2D _previewTexture2D = null;

    private void OnEnable()
    {
        _maskTextureData = (MaskTextureData)target;
        SetData();
    }

    private void SetData()
    {
        _maskTextureData.RequestMaskTexture((texture2D) => 
        {
            _previewTexture2D = texture2D;
        }, true);
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.ObjectField("MaskTextureData", _maskTextureData, typeof(MaskTextureData), false);
        EditorGUI.EndDisabledGroup();

        base.OnInspectorGUI();
        if (GUI.changed)
        {
            SetData();
        }
    }

    public override bool HasPreviewGUI() => true;
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (_previewTexture2D != null)
        {
            var texture = _previewTexture2D;
            var mask = _maskTextureData.maskTexture;

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
                EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), infoText);

                // Size label
                string sizeText = string.Format("Masked Size: {0}x{1}", Mathf.RoundToInt(mask.width), Mathf.RoundToInt(mask.height));
                EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), sizeText);
            }
            else
                EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), "Need mask texture.");
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