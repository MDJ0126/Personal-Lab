using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaskTextureScriptableObject)), CanEditMultipleObjects]
public class MaskTextureScriptableObjectEditor : Editor
{
    private MaskTextureScriptableObject _maskTextureScriptableObject = null;

    private Texture2D _previewTexture2D = null;

    private void OnEnable()
    {
        _maskTextureScriptableObject = (MaskTextureScriptableObject)target;
        SetPreviewMask();
    }

    private void SetPreviewMask()
    {
        _previewTexture2D = _maskTextureScriptableObject.GetMaskTexture(true);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUI.changed)
        {
            SetPreviewMask();
        }
    }

    public override bool HasPreviewGUI() => true;
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (_previewTexture2D != null)
        {
            var texture = _previewTexture2D;
            var mask = _maskTextureScriptableObject.maskTexture;

            Rect outerRect = r;
            outerRect.width = mask.width;
            outerRect.height = mask.height;

            if (mask.width > 0)
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

            // Size label
            string infoText = "Masked Texture Example";
            EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), infoText);

            // Size label
            string sizeText = string.Format("Masked Size: {0}x{1}", Mathf.RoundToInt(mask.width), Mathf.RoundToInt(mask.height));
            EditorGUI.DropShadowLabel(GUILayoutUtility.GetRect(Screen.width, 18f), sizeText);
        }
    }

    /// <summary>
    /// 체커 만들기
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