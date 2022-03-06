using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMaskedTexure : MonoBehaviour
{
    public MaskTextureData maskTextureData;
    public MeshRenderer meshRenderer;

    public void Start()
    {
        maskTextureData.RequestMaskTexture((texture2D) =>
        {
            meshRenderer.material.mainTexture = texture2D;
        });
    }
}
