using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleMaskedTexure : MonoBehaviour
{
    public MaskTextureData maskTextureData;
    public MeshRenderer meshRenderer;

    public void Start()
    {
        meshRenderer.material = maskTextureData.GetMeterial();
    }
}
