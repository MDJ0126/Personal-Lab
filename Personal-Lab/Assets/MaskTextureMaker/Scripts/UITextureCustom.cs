using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Texture Custom")]
public class UITextureCustom : UITexture
{
	[HideInInspector] [SerializeField] Object mTextureObject = null;
}