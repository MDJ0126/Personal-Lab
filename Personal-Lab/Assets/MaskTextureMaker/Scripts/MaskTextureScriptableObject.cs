using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageName_MaskTextureData", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]
public class MaskTextureScriptableObject : ScriptableObject
{
    private static Dictionary<int, Texture2D> _maskedTextures = new Dictionary<int, Texture2D>();

    public Texture2D texture;
    public Texture2D maskTexture;
    public Vector2 ccoordinate;

    /// <summary>
    /// 마스크 텍스쳐 가져오기
    /// </summary>
    /// <param name="isRefresh"></param>
    /// <returns></returns>
    public Texture2D GetMaskTexture(bool isRefresh = false)
    {
        var instanceId = this.GetInstanceID();

        if (isRefresh)
            _maskedTextures.Remove(instanceId);

        if (!_maskedTextures.TryGetValue(instanceId, out var texture2D))
        {
            var copyTextuer = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, false);
            Graphics.CopyTexture(texture, copyTextuer);

            var copyMaskTextuer = new Texture2D(maskTexture.width, maskTexture.height, maskTexture.format, maskTexture.mipmapCount, false);
            Graphics.CopyTexture(maskTexture, copyMaskTextuer);

            texture2D = new Texture2D(maskTexture.width, maskTexture.height, TextureFormat.RGBA32, texture.mipmapCount, false);
            Graphics.CopyTexture(MakeMaskedTexture(copyTextuer, copyMaskTextuer, ccoordinate), texture2D);
            texture2D.name = "Masked Texture (Instance)";
            _maskedTextures.Add(instanceId, texture2D);
        }
        return texture2D;
    }

    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <param name="texture"></param>
    /// <param name="area"></param>
    /// <returns></returns>
    private Texture2D MakeMaskedTexture(Texture2D texture, Texture2D maskTexture, Vector2 coordinate)
    {
        Texture2D result = new Texture2D(Mathf.RoundToInt(maskTexture.width), Mathf.RoundToInt(maskTexture.height), TextureFormat.RGBA32, 1, false);

        var pixels = result.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            int maskX = i % Mathf.RoundToInt(maskTexture.width);
            int maskY = i / Mathf.RoundToInt(maskTexture.width);
            int x = Mathf.RoundToInt(coordinate.x) + maskX;
            int y = Mathf.RoundToInt(coordinate.y) + maskY;
            var texturePixel = texture.GetPixel(x, y);
            var maskPixel = maskTexture.GetPixel(maskX, maskY);

            if (texturePixel.a != 0f)
            {
                //texturePixel.r += maskPixel.r;
                //texturePixel.g += maskPixel.g;
                //texturePixel.b += maskPixel.b;
                texturePixel.a = maskPixel.a;
            }

            result.SetPixel(maskX, maskY, texturePixel);
        }
        result.Apply();
        return result;
    }
}
