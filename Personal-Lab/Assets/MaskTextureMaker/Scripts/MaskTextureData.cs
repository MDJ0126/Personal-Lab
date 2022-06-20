using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "Masked Texture", menuName = "ScriptableObjects/Masked Texture")]
public class MaskTextureData : ScriptableObject
{
    private static Shader _shader = null;
    private static Shader Shader
    {
        get
        {
            if (_shader == null)
                _shader = Resources.Load<Shader>("Shaders/AlphaMask");
            return _shader;
        }
    }

    [Flags]
    public enum FlipMode
    {
        None = 0,
        X = 1 << 0,
        Y = 1 << 1,
    }

    [HideInInspector][SerializeField] public Texture texture;
    [HideInInspector][SerializeField] public Texture maskTexture;
    [HideInInspector][SerializeField] public Vector2 offset = Vector2.zero;
    [HideInInspector][SerializeField] public float scale = 1f;
    [HideInInspector][SerializeField] public FlipMode flipMode = FlipMode.None;

    public bool IsAvailable => texture != null && maskTexture != null;

    private Material _mat = null;
    private Material Material
    {
        get
        {
            if (_mat == null)
                _mat = new Material(Shader);
            return _mat;
        }
    }

    public Material GetMeterial()
    {
        Material.SetTexture("_MainTex", maskTexture);
        Material.SetTexture("_Texture", texture);
        Material.SetTextureOffset("_Texture", GetOffset());
        Material.SetTextureScale("_Texture", GetScale());
        return Material;
    }

    public Texture2D GetTexture()
    {
        Material.SetTexture("_MainTex", maskTexture);
        Material.SetTexture("_Texture", texture);
        Material.SetTextureOffset("_Texture", GetOffset());
        Material.SetTextureScale("_Texture", GetScale());

        RenderTexture renderTex = RenderTexture.GetTemporary(
                    maskTexture.width,
                    maskTexture.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.sRGB);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        GL.Clear(true, true, Color.clear);
        Graphics.Blit(null, renderTex, Material);
        Texture2D readableTexture = new Texture2D(maskTexture.width, maskTexture.height);
        readableTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableTexture.wrapMode = maskTexture.wrapMode;
        readableTexture.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableTexture;
    }

    private Vector2 GetOffset()
    {
        Vector2 result = offset;
        if ((flipMode & FlipMode.X) == FlipMode.X)
        {
            result *= new Vector2(-1, 1);
            result += new Vector2(1, 0);
        }
        if ((flipMode & FlipMode.Y) == FlipMode.Y)
        {
            result *= new Vector2(1, -1);
            result += new Vector2(0, 1);
        }
        return result;
    }

    private Vector2 GetScale()
    {
        Vector2 result = new Vector2((1 / scale) * maskTexture.width / texture.width, (1 / scale) * maskTexture.height / texture.height);
        if ((flipMode & FlipMode.X) == FlipMode.X)
        {
            result *= new Vector2(-1, 1);
        }
        if ((flipMode & FlipMode.Y) == FlipMode.Y)
        {
            result *= new Vector2(1, -1);
        }
        return result;
    }
}