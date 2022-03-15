using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Masked Texture", menuName = "ScriptableObjects/Masked Texture")]
public class MaskTextureData : ScriptableObject
{
    public static Dictionary<int, Texture2D> maskedTextures = new Dictionary<int, Texture2D>();
    public static void Release() => maskedTextures.Clear();

    private enum WriteSpeed
    {
        Slow,
        Default,
        Fast,
    }

    public Texture2D texture;
    public Texture2D maskTexture;
    public Vector2 coordinate;
    [SerializeField]
    private WriteSpeed runTimeWriteSpeed = WriteSpeed.Default;

    public int InstanceId => GetInstanceID();

    private Texture2D GetReadableTexture2D(Texture2D source)
    {
        if (source != null)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        source.width,
                        source.height,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.sRGB);
            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableTexture = new Texture2D(source.width, source.height);
            readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableTexture;
        }
        return null;
    }

    /// <summary>
    /// 마스크 텍스쳐 가져오기
    /// </summary>
    /// <param name="isRefresh">갱신 여부</param>
    /// <returns></returns>
    public void RequestMaskTexture(Action<Texture2D> onFinished)
    {
        if (texture != null)
        {
            if (Application.isPlaying)
            {
                MaskTextureMaker.MaskedTextureMaker.Instance.RequestMaskTexture(this, onFinished);
            }
            else
            {
                Texture2D texture2D = null;
                if (maskedTextures.TryGetValue(InstanceId, out texture2D))
                {
                    if (texture2D == null)
                        maskedTextures.Remove(InstanceId);
                }

                if (!maskedTextures.TryGetValue(InstanceId, out texture2D))
                {
                    texture2D = MakeMaskedTexture();
                    maskedTextures.Add(InstanceId, texture2D);
                }
                onFinished?.Invoke(texture2D);
            }
        }
        else
            onFinished?.Invoke(null);
    }

    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public Texture2D MakeMaskedTexture()
    {
        var texture = GetReadableTexture2D(this.texture);
        var maskTexture = GetReadableTexture2D(this.maskTexture);
        if (maskTexture != null)
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
            result.name = $"Masked {name} Texture (Instance)";
            result.Apply();

            return result;
        }
        return texture;
    }

    private WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public IEnumerator MakeMaskedTextureAsyc(Action<Texture2D> onFinished, Action<string, Color> progressText)
    {
        var texture = GetReadableTexture2D(this.texture);
        var maskTexture = GetReadableTexture2D(this.maskTexture);

        if (maskTexture != null)
        {
            Texture2D result = new Texture2D(Mathf.RoundToInt(maskTexture.width), Mathf.RoundToInt(maskTexture.height), TextureFormat.RGBA32, 1, false);
            var pixels = result.GetPixels();

            float roofTime = GetWriteSpeed();
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
                if (roofTime < time)
                {
                    yield return WaitForEndOfFrame;
                    time = 0f;
                    startupTime = Time.realtimeSinceStartup;
#if UNITY_EDITOR
                    progressText.Invoke($"Masking '{name}'.. {((float)i / pixels.Length) * 100f:N0}%", Color.gray);
#endif
                }
            }
            result.name = $"Masked {name} Texture (Instance)";
            result.Apply();
            onFinished.Invoke(result);
        }
        else
            onFinished.Invoke(texture);
#if UNITY_EDITOR
        progressText.Invoke($"Masking '{name}'.. Complete!", Color.white);
        Debug.Log($"<color=cyan>Loaded Masking '{name}'</color>");
#endif
    }

    /// <summary>
    /// 읽기 속도
    /// </summary>
    /// <returns></returns>
    private float GetWriteSpeed()
    {
        float speed = 0.1f;
        switch (runTimeWriteSpeed)
        {
            case WriteSpeed.Slow:
                speed = Time.deltaTime * 0.2f;
                break;
            case WriteSpeed.Default:
                speed = Time.deltaTime * 0.5f;
                break;
            case WriteSpeed.Fast:
                speed = Time.deltaTime;
                break;
            default:
                break;
        }
        return speed;
    }

    public override string ToString()
    {
        return $"{name}";
    }
}
