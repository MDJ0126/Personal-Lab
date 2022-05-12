using UnityEngine;

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

    private void Update() => MaskTextureData.OnRequestSaftyUpdater();
}