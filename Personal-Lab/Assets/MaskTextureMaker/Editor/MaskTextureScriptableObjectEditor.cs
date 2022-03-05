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
        var backgroundTex = CreateCheckerTex(new Color(0.1f, 0.1f, 0.1f, 0.5f), new Color(0.2f, 0.2f, 0.2f, 0.5f));
        GUI.DrawTexture(r, backgroundTex);

        if (_previewTexture2D != null)
        {
            var texture = _previewTexture2D;
            var mask = _maskTextureScriptableObject.maskTexture;
            GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);
        }
    }

    /// <summary>
    /// 체크 만들기
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
        return tex;
    }
}