using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Masked Texture", menuName = "ScriptableObjects/Masked Texture")]
public class MaskTextureData : ScriptableObject
{
    public static Dictionary<string, Texture2D> maskedTextures = new Dictionary<string, Texture2D>();

    public static void Release() => maskedTextures.Clear();

    [HideInInspector]
    public string fileName;
    public Texture2D texture;
    public Texture2D maskTexture;
    public Vector2 coordinate;

    private Texture2D GetReadableTexture()
    {
        var copyTextuer = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, false);
        Graphics.CopyTexture(texture, copyTextuer);
        return copyTextuer;
    }

    private Texture2D GetReadableMaskTexture()
    {
        var copyMaskTextuer = new Texture2D(maskTexture.width, maskTexture.height, maskTexture.format, maskTexture.mipmapCount, false);
        Graphics.CopyTexture(maskTexture, copyMaskTextuer);
        return copyMaskTextuer;
    }

    /// <summary>
    /// 마스크 텍스쳐 가져오기
    /// </summary>
    /// <param name="isRefresh">갱신 여부</param>
    /// <returns></returns>
    public void RequestMaskTexture(Action<Texture2D> onFinished, bool isRefresh = false)
    {
        if (texture != null && maskTexture != null)
        {
            if (Application.isPlaying)
            {
                MaskedTextureMaker.Instance.RequestMaskTexture(this, onFinished);
            }
            else
            {
                if (isRefresh)
                    maskedTextures.Remove(this.fileName);

                Texture2D texture2D = null;
                if (maskedTextures.TryGetValue(this.fileName, out texture2D))
                {
                    if (texture2D == null)
                        maskedTextures.Remove(this.fileName);
                }

                if (!maskedTextures.TryGetValue(this.fileName, out texture2D))
                {
                    texture2D = MakeMaskedTexture();
                    maskedTextures.Add(this.fileName, texture2D);
                }
                onFinished?.Invoke(texture2D);
            }
        }
    }

    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public Texture2D MakeMaskedTexture()
    {
        var texture = GetReadableTexture();
        var maskTexture = GetReadableMaskTexture();

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
        result.name = "Masked Texture (Instance)";
        result.Apply();

        return result;
    }

    private WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public IEnumerator MakeMaskedTextureAsyc(Action<Texture2D> onFinished, Action<string, Color> progressText)
    {
        var texture = GetReadableTexture();
        var maskTexture = GetReadableMaskTexture();

        Texture2D result = new Texture2D(Mathf.RoundToInt(maskTexture.width), Mathf.RoundToInt(maskTexture.height), TextureFormat.RGBA32, 1, false);
        var pixels = result.GetPixels();

        float roopTime = Time.deltaTime;
        float time = 0f;
        float startupTime = Time.realtimeSinceStartup;
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

            time = Time.realtimeSinceStartup - startupTime;
            if (roopTime < time)
            {
                time = 0f;
                startupTime = Time.realtimeSinceStartup;
                progressText.Invoke($"Masking '{this.fileName}'.. {((float)i / pixels.Length) * 100f:N0}%", Color.gray);
                yield return WaitForEndOfFrame;
            }
        }
        result.name = "Masked Texture (Instance)";
        result.Apply();
        onFinished.Invoke(result);
        progressText.Invoke($"Masking '{this.fileName}'.. Complete!", Color.white);
        Debug.Log($"<color=cyan>Loaded Masking '{this.fileName}'</color>");
    }
}
