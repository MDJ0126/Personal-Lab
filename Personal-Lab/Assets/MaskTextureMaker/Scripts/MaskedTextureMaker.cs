using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MaskedTextureMaker : MonoBehaviour
{
    #region Singleton

    private static MaskedTextureMaker instance = null;
    public static MaskedTextureMaker Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<MaskedTextureMaker>();
                if (instance == null)
                {
                    Initialize();
                    instance = FindObjectOfType<MaskedTextureMaker>();
                }
            }
            return instance;
        }
    }

    public static bool IsLive => instance != null;

    #endregion

    #region RuntimeInitializeOnLoadMethod
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        GameObject go = new GameObject();
        go.AddComponent<MaskedTextureMaker>();
        go.name = $"Masked Texture Maker (Instance)";
        DontDestroyOnLoad(go);
    }
    #endregion

    private struct Message
    {
        public MaskTextureData maskTextureData;
        public event Action<Texture2D> onFinished;
        public void InvokeOnFinished(Texture2D texture)
        {
            onFinished?.Invoke(texture);
        }
    }

    private Queue<Message> requestMessageQueue = new Queue<Message>();

    private void Awake()
    {
        MaskTextureData.Release();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            MaskTextureData.Release();
    }

    private void OnEnable()
    {
        StartCoroutine("MakePreocesser");
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// 마스크 이미지 제작 요청하기
    /// </summary>
    /// <param name="maskTextureData"></param>
    /// <param name="onFinished"></param>
    public void RequestMaskTexture(MaskTextureData maskTextureData, Action<Texture2D> onFinished)
    {
        bool isRequested = false;

        // 이미 이미지 요청을 한 경우에는 이벤트 구독만 추가해준다.
        var enumerator = requestMessageQueue.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.maskTextureData.fileName == maskTextureData.fileName)
            {
                enumerator.Current.onFinished += onFinished;
                isRequested = true;
                break;
            }
        }

        // 마스크 이미지 제작 요청
        if (!isRequested)
        {
            Message message = new Message();
            message.maskTextureData = maskTextureData;
            message.onFinished += onFinished;
            requestMessageQueue.Enqueue(message);
        }
    }

    private WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    /// <summary>
    /// Make Preocesser Updater
    /// </summary>
    /// <returns></returns>
    private IEnumerator MakePreocesser()
    {
        while (true)
        {
            if (requestMessageQueue.Count > 0)
            {
                var message = requestMessageQueue.Dequeue();
                var name = message.maskTextureData.fileName;

                Texture2D texture2D = null;

                if (MaskTextureData.maskedTextures.TryGetValue(name, out texture2D))
                {
                    if (texture2D == null)
                        MaskTextureData.maskedTextures.Remove(name);
                }

                if (!MaskTextureData.maskedTextures.TryGetValue(name, out texture2D))
                {
                    texture2D = message.maskTextureData.MakeMaskedTexture();
                    MaskTextureData.maskedTextures.Add(name, texture2D);
                }
                message.InvokeOnFinished(texture2D);
            }

            // Delay
            yield return WaitForEndOfFrame;
        }
    }
}
