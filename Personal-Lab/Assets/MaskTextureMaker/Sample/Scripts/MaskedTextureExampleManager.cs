using UnityEngine;

namespace MaskTextureMaker
{
    public class MaskedTextureExampleManager : MonoBehaviour
    {
        public MaskTextureScriptableObject maskTextureScriptableObject1;
        public MaskTextureScriptableObject maskTextureScriptableObject2;
        public UITexture uiTexture1;
        public UITexture uiTexture2;

        private void Awake()
        {
            uiTexture1.mainTexture = maskTextureScriptableObject1.GetMaskTexture();
            uiTexture1.MakePixelPerfect();

            uiTexture2.mainTexture = maskTextureScriptableObject2.GetMaskTexture();
            uiTexture2.MakePixelPerfect();
        }
    }
}