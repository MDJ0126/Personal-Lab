using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MaskTextureMaker
{
    public class MaskedTextureMaker : MonoBehaviour
    {
        public static bool isOnDebugGUI = false;

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

        #region Initialize
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GameObject go = new GameObject();
            go.AddComponent<MaskedTextureMaker>();
            go.name = $"Masked Texture Maker (Instance)";
            DontDestroyOnLoad(go);
        }
        #endregion

        private class Message
        {
            public MaskTextureData maskTextureData;
            public event Action<Texture2D> onFinished;
            public void InvokeOnFinished(Texture2D texture)
            {
                onFinished?.Invoke(texture);
            }
        }

        private Queue<Message> messageQueue = new Queue<Message>();
        private List<Message> makingList = new List<Message>();
        private string progressText = string.Empty;
        private Color color = Color.white;

        private void Awake()
        {
            MaskTextureData.Release();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode == LoadSceneMode.Single)
                MaskTextureData.Release();
        }

        private void Update()
        {
            if (messageQueue.Count > 0)
            {
                var message = messageQueue.Dequeue();
                var instanceId = message.maskTextureData.InstanceId;

                Texture2D texture2D = null;

                if (MaskTextureData.maskedTextures.TryGetValue(instanceId, out texture2D))
                {
                    if (texture2D == null)
                        MaskTextureData.maskedTextures.Remove(instanceId);
                }

                if (!MaskTextureData.maskedTextures.TryGetValue(instanceId, out texture2D))
                {
                    MaskTextureData.maskedTextures.Add(instanceId, texture2D);
                    makingList.Add(message);
                    StartCoroutine(message.maskTextureData.MakeMaskedTextureAsyc((resultTexture) =>
                    {
                        makingList.Remove(message);
                        if (MaskTextureData.maskedTextures.ContainsKey(instanceId))
                            MaskTextureData.maskedTextures[instanceId] = resultTexture;
                        message.InvokeOnFinished(resultTexture);
                    },
                    (progressText, color) =>
                    {
                        this.progressText = progressText;
                        this.color = color;
                    }));
                }
                else
                    message.InvokeOnFinished(texture2D);
            }
        }

        /// <summary>
        /// 마스크 이미지 제작 요청하기
        /// </summary>
        /// <param name="maskTextureData"></param>
        /// <param name="onFinished"></param>
        public void RequestMaskTexture(MaskTextureData maskTextureData, Action<Texture2D> onFinished)
        {
            // 완성된 이미지가 이미 존재하는 경우 바로 넘겨주기
            if (MaskTextureData.maskedTextures.TryGetValue(maskTextureData.InstanceId, out var texture2D))
            {
                onFinished?.Invoke(texture2D);
                return;
            }

            // 텍스쳐 제작 중에 있는지 체크
            var making = makingList.Find(item => item.maskTextureData.InstanceId == maskTextureData.InstanceId);
            if (making != null)
            {
                making.onFinished += onFinished;
                return;
            }

            // 이미 이미지 요청을 한 경우에는 이벤트 구독만 추가해준다.
            var enumerator = messageQueue.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current.maskTextureData.InstanceId == maskTextureData.InstanceId)
                {
                    enumerator.Current.onFinished += onFinished;
                    return;
                }
            }

            // 마스크 이미지 제작 요청
            Message message = new Message();
            message.maskTextureData = maskTextureData;
            message.onFinished += onFinished;
            messageQueue.Enqueue(message);
        }

#if UNITY_EDITOR

        private void OnGUI()
        {
            if (isOnDebugGUI)
            {
                GUIStyle style = new GUIStyle();
                style.fontStyle = FontStyle.Bold;
                style.fontSize = 14;
                DrawTextWithOutline(new Rect(10, 10, 300, 100), progressText, style, Color.black, color, 0.5f);
            }
        }

        /// <summary>
        /// Outline GUI Label
        /// https://answers.unity.com/questions/160285/text-with-outline.html
        /// </summary>
        /// <param name="centerRect"></param>
        /// <param name="text"></param>
        /// <param name="style"></param>
        /// <param name="borderColor"></param>
        /// <param name="innerColor"></param>
        /// <param name="borderWidth"></param>
        private void DrawTextWithOutline(Rect centerRect, string text, GUIStyle style, Color borderColor, Color innerColor, float borderWidth)
        {
            // assign the border color
            style.normal.textColor = borderColor;

            // draw an outline color copy to the left and up from original
            Rect modRect = centerRect;
            modRect.x -= borderWidth;
            modRect.y -= borderWidth;
            GUI.Label(modRect, text, style);


            // stamp copies from the top left corner to the top right corner
            while (modRect.x <= centerRect.x + borderWidth)
            {
                modRect.x++;
                GUI.Label(modRect, text, style);
            }

            // stamp copies from the top right corner to the bottom right corner
            while (modRect.y <= centerRect.y + borderWidth)
            {
                modRect.y++;
                GUI.Label(modRect, text, style);
            }

            // stamp copies from the bottom right corner to the bottom left corner
            while (modRect.x >= centerRect.x - borderWidth)
            {
                modRect.x--;
                GUI.Label(modRect, text, style);
            }

            // stamp copies from the bottom left corner to the top left corner
            while (modRect.y >= centerRect.y - borderWidth)
            {
                modRect.y--;
                GUI.Label(modRect, text, style);
            }

            // draw the inner color version in the center
            style.normal.textColor = innerColor;
            GUI.Label(centerRect, text, style);
        }
#endif
    }
}