using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Masked Texture", menuName = "ScriptableObjects/Masked Texture")]
public class MaskTextureData : ScriptableObject
{
    public static Dictionary<int, Texture2D> maskedTextures = new Dictionary<int, Texture2D>();
    public static void Release() => maskedTextures.Clear();

    public enum WriteSpeed
    {
        Slow,
        Default,
        Fast,
    }

    [Flags]
    public enum FlipMode
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
    }

    public Texture2D texture;
    public Texture2D maskTexture;
    public Vector2 coordinate = Vector2.one;
    [Range(0.1f, 5f)]
    public float scale = 1f;
    public WriteSpeed runTimeWriteSpeed = WriteSpeed.Default;
    public FlipMode flipMode = FlipMode.None;

    public int InstanceId => GetInstanceID();

    private WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

    /// <summary>
    /// 읽기 가능한 임시 텍스쳐 생성
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
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
            readableTexture.wrapMode = source.wrapMode;
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
    public void RequestMaskTexture(Action<Texture2D> onFinished, bool isRefresh = false)
    {
        if (texture != null)
        {
            if (isRefresh)
                maskedTextures.Remove(InstanceId);

            if (Application.isPlaying)
            {
                MaskTextureMaker.MaskedTextureMaker.Instance.RequestMaskTexture(this, onFinished);
            }
            else
            {
#if UNITY_EDITOR
                Texture2D texture2D = null;

                if (maskedTextures.TryGetValue(InstanceId, out texture2D))
                {
                    if (texture2D == null)
                        maskedTextures.Remove(InstanceId);
                }

                if (!maskedTextures.TryGetValue(InstanceId, out texture2D))
                {
                    EditorCoroutine.StartCoroutine(MakeMaskedTextureAsyc((resultTexture) =>
                    {
                        if (maskedTextures.ContainsKey(InstanceId))
                            maskedTextures[InstanceId] = resultTexture;
                        else
                            maskedTextures.Add(InstanceId, resultTexture);
                        onFinished?.Invoke(resultTexture);
                    },
                    (progressText, color) =>
                    {

                    }));
                }
                else
                    onFinished?.Invoke(texture2D);
#endif
            }
        }
        else
            onFinished?.Invoke(null);
    }

    /// <summary>
    /// 텍스쳐 뒤집기
    /// </summary>
    /// <param name="flipMode"></param>
    /// <param name="source"></param>
    private IEnumerator FlipTexture2DAsyc(FlipMode flipMode, Texture2D source)
    {
        float roofTime = GetWriteSpeed();
        float time = 0f;
        float startupTime = Time.realtimeSinceStartup;

        // 가로로 뒤집기
        if ((flipMode & FlipMode.X) == FlipMode.X)
        {
            var pixels = source.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % Mathf.RoundToInt(source.width);
                int y = i / Mathf.RoundToInt(source.width);
                var flipPixel = pixels[(source.width - x - 1) + (source.width * y)];
                source.SetPixel(x, y, flipPixel);

                time = Time.realtimeSinceStartup - startupTime;
                if (roofTime < time)
                {
                    yield return WaitForEndOfFrame;
                    time = 0f;
                    startupTime = Time.realtimeSinceStartup;
                }
            }
        }

        // 세로로 뒤집기
        if ((flipMode & FlipMode.Y) == FlipMode.Y)
        {
            var pixels = source.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % Mathf.RoundToInt(source.width);
                int y = i / Mathf.RoundToInt(source.width);
                var flipPixel = pixels[x + (source.width * (source.height - y - 1))];
                source.SetPixel(x, y, flipPixel);

                time = Time.realtimeSinceStartup - startupTime;
                if (roofTime < time)
                {
                    yield return WaitForEndOfFrame;
                    time = 0f;
                    startupTime = Time.realtimeSinceStartup;
                }
            }
        }
    }

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
#if UNITY_EDITOR
            var enumerator = FlipTexture2DAsyc(flipMode, texture);
            while (enumerator.MoveNext())
            {
                //yield return WaitForEndOfFrame;
            }
#else
            yield return FlipTexture2DAsyc(flipMode, texture);
#endif
            Texture2D result = new Texture2D(Mathf.RoundToInt(maskTexture.width), Mathf.RoundToInt(maskTexture.height), TextureFormat.RGBA32, 1, false);
            var pixels = result.GetPixels();

            float roofTime = GetWriteSpeed();
            float time = 0f;
            float startupTime = Time.realtimeSinceStartup;
            for (int i = 0; i < pixels.Length; i++)
            {
                int maskX = i % Mathf.RoundToInt(maskTexture.width);
                int maskY = i / Mathf.RoundToInt(maskTexture.width);
                int x = (int)((Mathf.RoundToInt(coordinate.x) + maskX) / scale);
                int y = (int)((Mathf.RoundToInt(coordinate.y) + maskY) / scale);
                var texturePixel = texture.GetPixel(x, y);
                var maskPixel = maskTexture.GetPixel(maskX, maskY);

                if (maskPixel.a == 0f)
                    texturePixel.a = 0f;

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
        if (Application.isPlaying)
            Debug.Log($"<color=cyan>Loaded Masking '{name}'</color>");
#endif
    }

    /// <summary>
    /// 읽기 속도
    /// </summary>
    /// <returns></returns>
    private float GetWriteSpeed()
    {
        float speed = 1f;
        if (Application.isPlaying)
        {
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
        }
        return speed;
    }

    public override string ToString()
    {
        return $"{name}";
    }

    #region ## EditorCoroutine ##
    private class EditorCoroutine
    {
#if UNITY_EDITOR
        public static EditorCoroutine StartCoroutine(IEnumerator _routine)
        {
            EditorCoroutine coroutine = new EditorCoroutine(_routine);
            coroutine.Start();
            return coroutine;
        }

        readonly IEnumerator routine;
        private EditorCoroutine(IEnumerator _routine) => routine = _routine;

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                UnityEditor.EditorApplication.update -= Update;
        }

        private void Start()
        {
            UnityEditor.EditorApplication.update += Update;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }
        private void Stop()
        {
            UnityEditor.EditorApplication.update -= Update;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void Update()
        {
            if (!routine.MoveNext()) Stop();
        }
#endif
    }
    #endregion
}