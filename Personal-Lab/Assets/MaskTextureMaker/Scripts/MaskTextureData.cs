using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "Masked Texture", menuName = "ScriptableObjects/Masked Texture")]
public class MaskTextureData : ScriptableObject
{
    public static Dictionary<int, Texture2D> maskedTextures = new Dictionary<int, Texture2D>();
    public static void Clear() => maskedTextures.Clear();

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

    [SerializeField] public byte[] compressedRawData;
    private byte[] decompressedRawData;
    public int InstanceId => GetInstanceID();
    public bool IsAvailable => texture != null && maskTexture != null;

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
                    if (isRefresh || compressedRawData == null || compressedRawData.Length == 0)
                    {
                        // 로우 데이터 없으면, 처음부터 시작
                        CreateRawData(() =>
                        {
                            RequestTextureAsyc(() =>
                            {
                                if (maskedTextures.TryGetValue(InstanceId, out var texture))
                                    onFinished?.Invoke(texture);
                            });
                        });
                    }
                    else
                    {
                        // 보통 파일을 만들 때는, 로우 데이터가 이미 생성되어 있는 상태로 시작
                        RequestTextureAsyc(() =>
                        {
                            if (maskedTextures.TryGetValue(InstanceId, out var texture))
                                onFinished?.Invoke(texture);
                        });
                    }
                }
            }
            else
                onFinished?.Invoke(texture2D);
        }
        else
            onFinished?.Invoke(null);
    }

    /// <summary>
    /// 1. 원시 데이터 제작 (바이트 형식의 이미지 데이터 제작)
    /// </summary>
    public void CreateRawData(Action onFinished)
    {
        if (IsAvailable)
        {
            texture.wrapMode = texture.wrapMode;
#if UNITY_EDITOR
            texture.alphaIsTransparency = true;  // 에디터에서만 사용 가능한 코드 (디버깅을 위해 강제 처리)
#endif
            var textureRawData = new RawDataInfo(texture);
            var maskTextureRawData = new RawDataInfo(maskTexture);

            // 텍스쳐 제작 메세지큐에 등록
            StartCreateThread(new Message
            {
                maskTextureData = this,
                textureRawData = textureRawData,
                maskTextureRawData = maskTextureRawData,
                onFinished = onFinished,
            });
        }
    }

    /// <summary>
    /// 2. 로우 데이터 기반으로 마스킹 텍스쳐 제작
    /// </summary>
    /// <returns>마스킹 적용된 텍스쳐 반환</returns>
    public void RequestTextureAsyc(Action onFinished)
    {
        // 텍스쳐 제작 메세지큐에 등록
        requestMessageQueue.Enqueue(new Message
        {
            maskTextureData = this,
            onFinished = onFinished,
        });
    }

    /// <summary>
    /// 3. 런타임, 메모리에 텍스쳐 적용하기
    /// </summary>
    public void Build()
    {
        if (compressedRawData != null)
        {
            Texture2D buildedTexture2D = new Texture2D(maskTexture.width, maskTexture.height);
            buildedTexture2D.name = this.name;

            buildedTexture2D.LoadRawTextureData(decompressedRawData);
            buildedTexture2D.Apply();

            if (maskedTextures.TryGetValue(InstanceId, out var texture2D))
            {
                texture2D = buildedTexture2D;
            }
            else
                maskedTextures.Add(InstanceId, buildedTexture2D);
        }
    }

    #region Threading (Static)

    public static Queue<Message> completeCreateMessageQueue = new Queue<Message>();
    public static Queue<Message> requestMessageQueue = new Queue<Message>();
    public static Queue<Message> resultMessageQueue = new Queue<Message>();

    public class Message
    {
        public MaskTextureData maskTextureData;
        public RawDataInfo textureRawData;
        public RawDataInfo maskTextureRawData;
        public Action onFinished;
    }

    static Thread getTextureThread = null;

    static MaskTextureData()
    {
        getTextureThread?.Abort();
        getTextureThread = new Thread(() => GetTextureRun());
        getTextureThread.Start();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= OnCreateSaftyUpdater;
        UnityEditor.EditorApplication.update += OnCreateSaftyUpdater;
        UnityEditor.EditorApplication.update -= OnRequestSaftyUpdater;
        UnityEditor.EditorApplication.update += OnRequestSaftyUpdater;
#endif
    }

    private static Dictionary<int, Thread> _createThreads = new Dictionary<int, Thread>();

    /// <summary>
    /// 제작 쓰레드 실행
    /// </summary>
    /// <param name="msg"></param>
    private static void StartCreateThread(Message msg)
    {
        if (_createThreads.TryGetValue(msg.maskTextureData.InstanceId, out var thread))
        {
            thread.Abort();
        }
        else
        {
            _createThreads.Add(msg.maskTextureData.InstanceId, null);
        }
        _createThreads[msg.maskTextureData.InstanceId] = new Thread(() => CreateRun(msg));
        _createThreads[msg.maskTextureData.InstanceId].Start();
    }

    /// <summary>
    /// 제작 쓰레드 함수
    /// </summary>
    private static void CreateRun(Message msg)
    {
        Thread.Sleep(50);
        BuildRawData(msg);
        completeCreateMessageQueue.Enqueue(msg);
    }

    /// <summary>
    /// 제작 완료 업데이터 (메인 쓰레드)
    /// </summary>
    private static void OnCreateSaftyUpdater()
    {
        if (completeCreateMessageQueue.Count > 0)
        {
            var msg = completeCreateMessageQueue.Dequeue();
            _createThreads.Remove(msg.maskTextureData.InstanceId);
            msg.onFinished?.Invoke();
        }
    }

    /// <summary>
    /// 로우 데이터 기반으로 이미지 요청 콜백 처리 업데이터
    /// </summary>
    public static void GetTextureRun()
    {
        while (true)
        {
            if (requestMessageQueue.Count > 0)
            {
                var msg = requestMessageQueue.Dequeue();
                msg.maskTextureData.decompressedRawData = Decompresse(msg.maskTextureData.compressedRawData);
                resultMessageQueue.Enqueue(msg);
            }
            Thread.Sleep(50);
        }
    }

    /// <summary>
    /// 콜백 처리 업데이터 (메인 쓰레드)
    /// </summary>
    public static void OnRequestSaftyUpdater()
    {
        if (resultMessageQueue.Count > 0)
        {
            var msg = resultMessageQueue.Dequeue();
            msg.maskTextureData.Build();
            msg.onFinished?.Invoke();
        }
    }

    #endregion

    #region Build Mask Texture (Static)

    /// <summary>
    /// 이미지 바이트 데이터 (원시 데이터)
    /// </summary>
    public class RawDataInfo
    {
        private byte[] originalRawData;
        private byte[] changedRawData;
        public int width;
        public int height;
        public int Length;

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

        public RawDataInfo(Texture2D texture2D)
        {
            originalRawData = GetReadableTexture2D(texture2D).GetRawTextureData();
            IntializeRawData();
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
                changedRawData[index] = color[0];   // r
                changedRawData[index + 1] = color[1];   // g
                changedRawData[index + 2] = color[2];   // b
                changedRawData[index + 3] = color[3];   // a
            }
        }

        public void IntializeRawData()
        {
            changedRawData = new byte[originalRawData.Length];
        }

        public void Apply()
        {
            if (changedRawData == null) IntializeRawData();
            Array.Copy(changedRawData, originalRawData, originalRawData.Length);
        }
        public byte[] GetRawData() => changedRawData;
    }

    /// <summary>
    /// 텍스쳐 뒤집기
    /// </summary>
    /// <param name="flipMode"></param>
    /// <param name="source"></param>
    private static void FlipTexture2D(FlipMode flipMode, ref RawDataInfo source)
    {
        const int ROOFTIME = 1;
        TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
        DateTime time = DateTime.MinValue;

        // 가로로 뒤집기
        if ((flipMode & FlipMode.X) == FlipMode.X)
        {
            for (int i = 0; i < source.Length; i++)
            {
                int x = i % Mathf.RoundToInt(source.width);
                int y = i / Mathf.RoundToInt(source.width);
                var flipPixel = source.GetPixel(source.width - x, y);
                source.SetPixel(x, y, flipPixel);

                time = DateTime.Now - start;
                if (ROOFTIME <= time.Second)
                {
                    Thread.Sleep(50);
                    time = DateTime.MinValue;
                    start = new TimeSpan(DateTime.Now.Ticks);
                }
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

                time = DateTime.Now - start;
                if (ROOFTIME <= time.Second)
                {
                    Thread.Sleep(50);
                    time = DateTime.MinValue;
                    start = new TimeSpan(DateTime.Now.Ticks);
                }
            }
            source.Apply();
        }
    }


    /// <summary>
    /// 마스크 텍스쳐 제작
    /// </summary>
    /// <param name="msg"></param>
    private static void BuildRawData(Message msg)
    {
        const int ROOFTIME = 1;
        TimeSpan start = new TimeSpan(DateTime.Now.Ticks);
        DateTime time = DateTime.MinValue;

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

            time = DateTime.Now - start;
            if (ROOFTIME <= time.Second)
            {
                Thread.Sleep(50);
                time = DateTime.MinValue;
                start = new TimeSpan(DateTime.Now.Ticks);
            }
        }

        maskTextureData.compressedRawData = Compresse(maskTextureRawData.GetRawData());
    }

    #endregion

    #region Compresse Utils (Static)

    /// <summary>
    /// 압축
    /// </summary>
    private static byte[] Compresse(byte[] originByte)
    {
        byte[] compressedByte;
        using (MemoryStream ms = new MemoryStream())
        {
            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
            {
                ds.Write(originByte, 0, originByte.Length);
            }
            compressedByte = ms.ToArray();
        }
        return compressedByte;
    }

    /// <summary>
    /// 압축 풀기
    /// </summary>
    /// <param name="compressedByte"></param>
    private static byte[] Decompresse(byte[] compressedByte)
    {
        MemoryStream resultStream = new MemoryStream();
        using (MemoryStream ms = new MemoryStream(compressedByte))
        {
            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Decompress))
            {
                ds.CopyTo(resultStream);
                ds.Close();
            }
        }

        byte[] deCompressedByte = resultStream.ToArray();
        resultStream.Dispose();
        return deCompressedByte;
    }
    #endregion
}