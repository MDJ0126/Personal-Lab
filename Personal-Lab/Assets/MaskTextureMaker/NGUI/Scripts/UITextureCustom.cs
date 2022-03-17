using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Texture Custom")]
public class UITextureCustom : UITexture
{
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
                if (mTextureObject != null)
                {
                    mainTexture = null;
                    value.RequestMaskTexture((texture2D) =>
                    {
                        mainTexture = texture2D;
                    });
                }
            }
        }
    }

    protected override void OnStart()
    {
        base.OnStart();
        mainTexture = null;
        if (maskTextureScriptableObject != null)
        {
            maskTextureScriptableObject.RequestMaskTexture((texture2D) =>
            {
                mainTexture = texture2D;
            });
        }
    }
}