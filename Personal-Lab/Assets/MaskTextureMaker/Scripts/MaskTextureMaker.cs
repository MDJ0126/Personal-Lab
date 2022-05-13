using UnityEngine;
using UnityEngine.SceneManagement;

public class MaskTextureMaker : MonoBehaviour
{
    #region Initialize
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void CreateInstance()
    {
        GameObject go = new GameObject();
        go.AddComponent<MaskTextureMaker>();
        go.name = nameof(MaskTextureMaker);
        DontDestroyOnLoad(go);
    }
    #endregion

    private void Awake()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Single)
            MaskTextureData.Clear();
    }

    private void Update() => MaskTextureData.OnRequestSaftyUpdater();
}