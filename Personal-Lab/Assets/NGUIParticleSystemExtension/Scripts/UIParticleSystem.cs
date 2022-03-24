using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIParticleSystem : UITexture
{
    private List<Material> _materials = new List<Material>();

    public override void Invalidate(bool includeChildren)
    {
        base.Invalidate(includeChildren);
        Initialize();
    }

    private void Initialize()
    {
        if (Application.isPlaying)
        {
            // Create fake texture
            if (mainTexture == null)
            {
                var texture2D = new Texture2D(1, 1);
                texture2D.name = $"{this.gameObject.name} (UISpine Fake Texture)";
                mainTexture = texture2D;
                // 이미지가 작아서 패널 영역 안에 포함 안된다고 처리하는 경우가 있어서, 사이즈 1000x1000으로 임시 수정
                width = 1000;
                height = 1000;
            }

            var particleSystemRenderers = this.GetComponentsInChildren<ParticleSystemRenderer>(true);
            if (particleSystemRenderers != null)
            {
                _materials.Clear();

                for (int i = 0; i < particleSystemRenderers.Length; i++)
                {
                    particleSystemRenderers[i].sortingOrder = 0;
                    particleSystemRenderers[i].material.renderQueue = 3000;
                    _materials.Add(particleSystemRenderers[i].material);
                }
            }

            if (drawCall != null)
                UpdateDrawCall();
        }
    }

    protected override void OnUpdate()
    {
        base.OnUpdate();
        UpdateDrawCall();
    }

    /// <summary>
    /// Update all Renderqueue
    /// </summary>
    private void UpdateDrawCall()
    {
        if (Application.isPlaying)
        {
            int renderQueue = 3000;
            if (drawCall != null)
                renderQueue = drawCall.renderQueue;

            for (int i = 0; i < _materials.Count; i++)
            {
                _materials[i].renderQueue = renderQueue;
            }
        }
    }
}
