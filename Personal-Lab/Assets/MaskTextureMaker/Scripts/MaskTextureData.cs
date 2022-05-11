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
        Force,
    }

    [Flags]
    public enum FlipMode
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
    }

    [SerializeField] private Texture2D texture;
    [SerializeField] private Texture2D maskTexture;
    [SerializeField] private Vector2 coordinate = Vector2.one;
    [SerializeField] private float scale = 1f;
    [SerializeField] private WriteSpeed runTimeWriteSpeed = WriteSpeed.Default;
    [SerializeField] private FlipMode flipMode = FlipMode.None;

    public int InstanceId => GetInstanceID();

    public bool IsAvailable => texture != null && maskTexture != null;

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
    public void RequestMaskTexture(Action<Texture2D> onFinished, bool isRefresh = false, bool isForceLoad = false)
    {
        if (texture != null)
        {
            if (isRefresh)
                maskedTextures.Remove(InstanceId);
            if (isForceLoad)
            {
                var enumerator = MakeMaskedTextureAsyc((resultTexture) =>
                {
                    if (maskedTextures.ContainsKey(InstanceId))
                        maskedTextures[InstanceId] = resultTexture;
                    else
                        maskedTextures.Add(InstanceId, resultTexture);
                    onFinished?.Invoke(resultTexture);
                },
                (progressText, color) =>
                {

                });
                while (enumerator.MoveNext()) { }
            }
            if (Application.isPlaying)
            {
                MaskedTextureMaker.Instance.RequestMaskTexture(this, onFinished);
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

                if (runTimeWriteSpeed != WriteSpeed.Force)
                {
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

                if (runTimeWriteSpeed != WriteSpeed.Force)
                {
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
    }

    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public IEnumerator MakeMaskedTextureAsyc(Action<Texture2D> onFinished, Action<string, Color> progressText)
    {
        var texture = GetReadableTexture2D(this.texture);
        var maskTexture = GetReadableTexture2D(this.maskTexture);

        if (IsAvailable)
        {
            Texture2D result = new Texture2D(Mathf.RoundToInt(maskTexture.width), Mathf.RoundToInt(maskTexture.height), TextureFormat.RGBA32, 1, false);
            result.wrapMode = texture.wrapMode;

#if UNITY_EDITOR
            result.alphaIsTransparency = true;  // 에디터에서만 사용 가능한 코드 (디버깅을 위해 강제 처리)
#endif
            if (Application.isPlaying && runTimeWriteSpeed != WriteSpeed.Force)
            {
                yield return FlipTexture2DAsyc(flipMode, texture);
            }
            else
            {
                var enumerator = FlipTexture2DAsyc(flipMode, texture);
                while (enumerator.MoveNext()) { }
            }

            var pixels = result.GetPixels();
            float roofTime = GetWriteSpeed();
            float time = 0f;
            float startupTime = Time.realtimeSinceStartup;

            int width = Mathf.RoundToInt(maskTexture.width);
            for (int i = 0; i < pixels.Length; i++)
            {
                int maskX = i % width;
                int maskY = i / width;
                var maskPixel = maskTexture.GetPixel(maskX, maskY);

                if (maskPixel.a != 0f)
                {
                    // 마스크 픽셀이 투명하지 않으면 원색으로 적용
                    int x = (int)((Mathf.RoundToInt(coordinate.x) + maskX) / scale);
                    int y = (int)((Mathf.RoundToInt(coordinate.y) + maskY) / scale);
                    var texturePixel = texture.GetPixel(x, y);
                    maskTexture.SetPixel(maskX, maskY, texturePixel);
                }

                if (runTimeWriteSpeed != WriteSpeed.Force)
                {
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
            var targetFrameRate = Application.targetFrameRate;
            if (targetFrameRate == -1)  // 설정된 값이 없는 경우 처리
                targetFrameRate = 30;

            float frameDeltaTime = 1f / targetFrameRate;    // == Time.DeltaTime도 의미가 같으나 이 경우 순간적으로 프레임 드랍이 생기면 값에 문제가 생김

            switch (runTimeWriteSpeed)
            {
                case WriteSpeed.Slow:
                    speed = frameDeltaTime * 0.1f;
                    break;
                case WriteSpeed.Default:
                    speed = frameDeltaTime * 0.5f;
                    break;
                case WriteSpeed.Fast:
                    speed = frameDeltaTime * 1f;
                    break;
                case WriteSpeed.Force:
                    //speed = frameDeltaTime * 100000f;
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