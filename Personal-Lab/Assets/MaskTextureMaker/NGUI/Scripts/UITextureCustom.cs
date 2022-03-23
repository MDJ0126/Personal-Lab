using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Texture Custom")]
public class UITextureCustom : UITexture
{
    private static Texture2D clearTexture = null;

    [HideInInspector] [SerializeField] Object mTextureObject = null;

    public MaskTextureData maskTextureScriptableObject
    {
        get
        {
            return mTextureObject as MaskTextureData;
        }
        set
        {
            if (!mTextureObject.Equals(value))
            {
                mTextureObject = value;
                mainTexture = GetClearTexture();
            }

            value.RequestMaskTexture((texture2D) =>
            {
                if (mTextureObject.Equals(value))
                    mainTexture = texture2D;
            });
        }
    }

    protected override void OnStart()
    {
        base.OnStart();
        mainTexture = GetClearTexture();
        if (maskTextureScriptableObject != null)
        {
            maskTextureScriptableObject.RequestMaskTexture((texture2D) =>
            {
                mainTexture = texture2D;
            });
        }
    }

    private Texture2D GetClearTexture()
    {
        if (clearTexture == null)
        {
            clearTexture = new Texture2D(2, 2);
            var pixels = clearTexture.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % Mathf.RoundToInt(clearTexture.width);
                int y = i / Mathf.RoundToInt(clearTexture.width);
                clearTexture.SetPixel(x, y, Color.clear);
            }
            clearTexture.Apply();
        }
        return clearTexture;
    }
}