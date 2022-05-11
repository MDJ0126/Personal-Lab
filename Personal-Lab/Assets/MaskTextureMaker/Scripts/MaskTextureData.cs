using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

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
    private byte[] rawData;
    private Texture2D buildedTexture2D;

    /// <summary>
    /// 마스크 텍스쳐 가져오기
    /// </summary>
    /// <param name="isRefresh">갱신 여부</param>
    /// <returns></returns>
    public void RequestMaskTexture(Action<Texture2D> onFinished, bool isRefresh = false)
    {
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
    /// 마스킹 텍스쳐 제작
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

            // 텍스쳐 제작 메세지큐에 등록
            StartThread(new MakeMessage
            {
                maskTextureData = this,
                textureRawData = textureRawData,
                maskTextureRawData = maskTextureRawData,
                onFinished = onFinished,
            });
        }
        else
            onFinished?.Invoke();
    }

    /// <summary>
    /// 텍스쳐 적용하기
    /// </summary>
    public void Build()
    {
        if (rawData != null)
        {
            if (buildedTexture2D == null)
            {
                buildedTexture2D = new Texture2D(maskTexture.width, maskTexture.height);
                buildedTexture2D.name = this.name;
            }

            buildedTexture2D.LoadRawTextureData(rawData);
            buildedTexture2D.Apply();

            if (maskedTextures.TryGetValue(InstanceId, out var texture2D))
            {
                texture2D = buildedTexture2D;
            }
            else
                maskedTextures.Add(InstanceId, buildedTexture2D);
        }
    }

    #region Static Method

    /// <summary>
    /// 이미지 바이트 데이터 (원시 데이터)
    /// </summary>
    public class RawData
    {
        private byte[] originalRawData;
        private byte[] changedRawData;
        public int width;
        public int height;
        public int Length;

        public RawData(Texture2D texture2D)
        {
            originalRawData = texture2D.GetRawTextureData();
            changedRawData = texture2D.GetRawTextureData();
            width = Mathf.RoundToInt(texture2D.width);
            height = Mathf.RoundToInt(texture2D.height);
            Length = originalRawData.Length / 4;
        }

        public byte[] GetPixel(int x, int y)
        {
            int index = x * 4 + y * width * 4;
            if (index > 0 && index < originalRawData.Length)
            {
                byte r = originalRawData[index];
                byte g = originalRawData[index + 1];
                byte b = originalRawData[index + 2];
                byte a = originalRawData[index + 3];
                return new byte[] { r, g, b, a };
            }
            return null;
        }

        public void SetPixel(int x, int y, byte[] color)
        {
            if (color != null && color.Length == 4)
            {
                int index = x * 4 + y * width * 4;
                changedRawData[index]     = color[0];   // r
                changedRawData[index + 1] = color[1];   // g
                changedRawData[index + 2] = color[2];   // b
                changedRawData[index + 3] = color[3];   // a
            }
        }

        public void Apply() => Array.Copy(changedRawData, originalRawData, originalRawData.Length);
        public byte[] GetRawData() => changedRawData;
    }

    public static Queue<MakeMessage> makeMessageQueue = new Queue<MakeMessage>();
    public static Queue<ResultMessage> resultMessageQueue = new Queue<ResultMessage>();

    public class MakeMessage
    {
        public MaskTextureData maskTextureData;
        public RawData textureRawData;
        public RawData maskTextureRawData;
        public Action onFinished;
    }

    public class ResultMessage
    {
        public MakeMessage completeMaskMessage;
    }

    static MaskTextureData()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= OnUpdater;
        UnityEditor.EditorApplication.update += OnUpdater;
#endif
    }

    /// <summary>
    /// 완성된 이미지 콜백 처리 업데이터
    /// </summary>
    public static void OnUpdater()
    {
        if (resultMessageQueue.Count > 0)
        {
            var msg = resultMessageQueue.Dequeue();
            msg.completeMaskMessage.maskTextureData.Build();
            msg.completeMaskMessage.onFinished.Invoke();
        }
    }

    //초기 실행 가능한 쓰레드 4개
    //최대 실행 가능한 쓰레드 4개
    private static Semaphore semaphore = new Semaphore(4, 4);

    /// <summary>
    /// 쓰레드 시작
    /// </summary>
    public static void StartThread(MakeMessage msg)
    {
        makeMessageQueue.Enqueue(msg);
        new Thread(() => Run()).Start();
    }

    /// <summary>
    /// 쓰레드 함수
    /// </summary>
    private static void Run()
    {
        semaphore.WaitOne();
        Make(makeMessageQueue.Dequeue());
        semaphore.Release();
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
            source.Apply();
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
            source.Apply();
        }
    }

    /// <summary>
    /// 마스크 텍스쳐 제작
    /// </summary>
    /// <param name="msg"></param>
    private static void Make(MakeMessage msg)
    {
        var maskTextureData = msg.maskTextureData;
        var textureRawData = msg.textureRawData;
        var maskTextureRawData = msg.maskTextureRawData;

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
        maskTextureData.rawData = maskTextureRawData.GetRawData();
        resultMessageQueue.Enqueue(new ResultMessage { completeMaskMessage = msg});
    }

    #endregion
}