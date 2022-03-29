using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Particle Panel Mask")]
[RequireComponent(typeof(UIPanel))]
public class UIParticlePanelMask : MonoBehaviour
{
    private Vector3 mCachedPosition;
    private Vector2 mCachedScale;

    private UIPanel mPanel = null;
    [SerializeField] private SpriteMask mSpriteMask = null;


    private void Awake()
    {
        mPanel = GetComponent<UIPanel>();
    }

    private bool IsAvailable => mPanel != null;

    private void Start()
    {
        if (IsAvailable)
        {
            // 패널 아래 있는 모든 파티클 강제로 마스킹 세팅
            ChangeAllVisibleInsideMask();
        }
    }

    private void Update()
    {
        if (IsAvailable)
        {
            // 파티클 영역 오브젝트 생성
            if (mSpriteMask == null)
                CreateSpriteMask();

            // 앵커 변화가 있는 경우 앵커 반영 처리
            if (IsChangedAnchor())
                UpdateMaskAnchor();
        }
    }

    /// <summary>
    /// 앵커에 변화가 있었는지 체크
    /// </summary>
    /// <returns></returns>
    private bool IsChangedAnchor()
    {
        Vector3 newPosition;
        Vector2 newScale;
        if (mPanel.clipping == UIDrawCall.Clipping.SoftClip)
        {
            newPosition = new Vector3(mPanel.transform.localPosition.x + mPanel.finalClipRegion.x, mPanel.transform.localPosition.y + mPanel.finalClipRegion.y, 0f);
            newScale = new Vector2(mPanel.baseClipRegion.z, mPanel.baseClipRegion.w);
        }
        else
        {
            newPosition = Vector3.zero;
            newScale = new Vector2(Screen.width, Screen.height);
        }

        if (mCachedPosition != newPosition || mCachedScale != newScale)
        {
            mCachedPosition = newPosition;
            mCachedScale = newScale;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 앵커 업데이트
    /// </summary>
    private void UpdateMaskAnchor()
    {
        mSpriteMask.transform.localPosition = mCachedPosition;
        mSpriteMask.transform.localScale = mCachedScale;
    }

    /// <summary>
    /// 모든 자식 오브젝트 파티클 마스킹 영역에서만 작동하도록 설정
    /// </summary>
    private void ChangeAllVisibleInsideMask()
    {
        var particles = this.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i].maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
        }
    }

    /// <summary>
    /// Sprite Mask 오브젝트 생성
    /// </summary>
    private void CreateSpriteMask()
    {
        if (mSpriteMask == null)
        {
            // 오브젝트 생성
            GameObject go = new GameObject($"Particl Clipping Mask (Target : {this.name})");
            go.transform.SetParent(this.transform.parent);
            go.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
            go.layer = mPanel.gameObject.layer;
            mSpriteMask = go.AddComponent<SpriteMask>();

            Texture2D texture2D = new Texture2D(100, 100);
            Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
            mSpriteMask.sprite = sprite;

            IsChangedAnchor();
            UpdateMaskAnchor();
        }
    }
}
