using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Masked Texture", menuName = "ScriptableObjects/Masked Texture")]
public class MaskTextureData : ScriptableObject
{
    public static Dictionary<int, Texture2D> maskedTextures = new Dictionary<int, Texture2D>();

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
    [SerializeField] private FlipMode flipMode = FlipMode.None;

    public int InstanceId => GetInstanceID();

    public bool IsAvailable => texture != null && maskTexture != null;

    private WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();

    private byte[] resultData;

    private Texture2D result;

    public class RawData
    {
        private Texture2D texture2D;
        private byte[] rawData;
        public byte[] resultData;
        public int width;
        public int height;
        public int Length;

        public RawData(Texture2D texture2D)
        {
            this.texture2D = texture2D;
            rawData = texture2D.GetRawTextureData();
            resultData = texture2D.GetRawTextureData();
            width = Mathf.RoundToInt(texture2D.width);
            height = Mathf.RoundToInt(texture2D.height);
            Length = rawData.Length / 4;
        }

        public byte[] GetPixel(int x, int y)
        {
            int index = x * 4 + y * width * 4;
            if (index > 0 && index < rawData.Length)
            {
                byte r = rawData[index];
                byte g = rawData[index + 1];
                byte b = rawData[index + 2];
                byte a = rawData[index + 3];
                return new byte[] { r , g, b, a };
            }
            return null;
        }

        public void SetPixel(int x, int y, byte[] color)
        {
            if (color != null && color.Length == 4)
            {
                int index = x * 4 + y * width * 4;
                resultData[index] = color[0];
                resultData[index + 1] = color[1];
                resultData[index + 2] = color[2];
                resultData[index + 3] = color[3];
            }
        }

        public void Build()
        {
            Array.Copy(resultData, rawData, rawData.Length);
        }
    }

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
        if (makeUpdateThread == null)
        {
            makeUpdateThread = new Thread(() => MakeUpdater());
            makeUpdateThread.Start();
        }

        if (IsAvailable)
        {
            if (isRefresh || !maskedTextures.TryGetValue(InstanceId, out var texture2D) || texture2D == null)
            {
                maskedTextures.Remove(InstanceId);
                if (!maskedTextures.ContainsKey(InstanceId))
                {
                    MakeMaskedTextureAsyc(() =>
                    {
                        onFinished?.Invoke(maskedTextures[InstanceId]);
                    });
                }
            }
            else
                onFinished?.Invoke(texture2D);
        }
        else
            onFinished?.Invoke(null);
    }

    /// <summary>
    /// 대상 텍스쳐를 영역만큼 잘라서 반환
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public void MakeMaskedTextureAsyc(Action onFinished)
    {
        if (IsAvailable)
        {
            texture.wrapMode = texture.wrapMode;
#if UNITY_EDITOR
            texture.alphaIsTransparency = true;  // 에디터에서만 사용 가능한 코드 (디버깅을 위해 강제 처리)
#endif
            var textureRawData = new RawData(GetReadableTexture2D(texture));
            var maskTextureRawData = new RawData(GetReadableTexture2D(maskTexture));
            
            makeMessageQueue.Enqueue(new MakeMessage
            {
                maskTextureData = this,
                textureRawData = textureRawData,
                maskTextureRawData = maskTextureRawData,
                onFinished = onFinished,
            });
        }
    }

    public void Build()
    {
        if (result == null)
        {
            result = new Texture2D(maskTexture.width, maskTexture.height);
            result.name = this.name;
        }

        result.LoadRawTextureData(resultData);
        result.Apply();

        if (maskedTextures.TryGetValue(InstanceId, out var texture2D))
        {
            texture2D = result;
        }
        else
            maskedTextures.Add(InstanceId, result);
    }

    #region Static Method

    public static Queue<MakeMessage> makeMessageQueue = new Queue<MakeMessage>();
    public static Queue<ResultMessage> resultMessageQueue = new Queue<ResultMessage>();

    public struct MakeMessage
    {
        public MaskTextureData maskTextureData;
        public RawData textureRawData;
        public RawData maskTextureRawData;
        public Action onFinished;
    }

    public struct ResultMessage
    {
        public MaskTextureData maskTextureData;
        public Action onFinished;
    }

    static MaskTextureData()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= OnUpdater;
        UnityEditor.EditorApplication.update += OnUpdater;
#endif
    }

    public static void OnUpdater()
    {
        if (resultMessageQueue.Count > 0)
        {
            var message = resultMessageQueue.Dequeue();
            message.maskTextureData.Build();
            message.onFinished.Invoke();
        }
    }

    private static Thread makeUpdateThread = null;

    public static void ReleaseThread()
    {
        if (makeUpdateThread != null)
        {
            makeUpdateThread.Abort();
            makeUpdateThread = null;
        }
    }

    private static void MakeUpdater()
    {
        while (true)
        {
            if (makeMessageQueue.Count > 0)
            {
                Make(makeMessageQueue.Dequeue());
            }
            else
                Thread.Sleep(100);
        }
    }

    /// <summary>
    /// 텍스쳐 뒤집기
    /// </summary>
    /// <param name="flipMode"></param>
    /// <param name="source"></param>
    private static void FlipTexture2D(FlipMode flipMode, ref RawData source)
    {
        // 가로로 뒤집기
        if ((flipMode & FlipMode.X) == FlipMode.X)
        {
            for (int i = 0; i < source.Length; i++)
            {
                int x = i % Mathf.RoundToInt(source.width);
                int y = i / Mathf.RoundToInt(source.width);
                var flipPixel = source.GetPixel(source.width - x, y);
                source.SetPixel(x, y, flipPixel);
            }
            source.Build();
        }

        // 세로로 뒤집기
        if ((flipMode & FlipMode.Y) == FlipMode.Y)
        {
            for (int i = 0; i < source.Length; i++)
            {
                int x = i % Mathf.RoundToInt(source.width);
                int y = i / Mathf.RoundToInt(source.width);
                var flipPixel = source.GetPixel(x, source.height - y);
                source.SetPixel(x, y, flipPixel);
            }
            source.Build();
        }
    }

    /// <summary>
    /// 마스크 텍스쳐 제작
    /// </summary>
    /// <param name="requestMessage"></param>
    private static void Make(MakeMessage requestMessage)
    {
        var maskTextureData = requestMessage.maskTextureData;
        var textureRawData = requestMessage.textureRawData;
        var maskTextureRawData = requestMessage.maskTextureRawData;
        var onFinished = requestMessage.onFinished;

        FlipTexture2D(maskTextureData.flipMode, ref textureRawData);
        for (int i = 0; i < maskTextureRawData.Length; i++)
        {
            int maskX = i % maskTextureRawData.width;
            int maskY = i / maskTextureRawData.width;
            var maskPixel = maskTextureRawData.GetPixel(maskX, maskY);

            if (maskPixel != null && maskPixel[3] != 0)
            {
                int x = (int)((Mathf.RoundToInt(maskTextureData.coordinate.x) + maskX) / maskTextureData.scale);
                int y = (int)((Mathf.RoundToInt(maskTextureData.coordinate.y) + maskY) / maskTextureData.scale);
                var texturePixel = textureRawData.GetPixel(x, y);

                // 마스크 픽셀이 투명하지 않으면 원색으로 적용
                maskTextureRawData.SetPixel(maskX, maskY, texturePixel);
            }
        }
        maskTextureData.resultData = maskTextureRawData.resultData;
        resultMessageQueue.Enqueue(new ResultMessage { maskTextureData = maskTextureData, onFinished = onFinished });
    }

    #endregion
}