Shader "MaskTexture/AlphaMask" 
{
    Properties {
        _MainTex ("Mask Texture (A)", 2D) = "white" {}
        _Texture ("Texture (RGBA) ", 2D) = "white" {}
    }

    SubShader 
    {
		LOD 200

		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"DisableBatching" = "True"
		}

        //Stencil
        //{
        //    Ref 1
        //    Comp notequal
        //    Pass Keep
        //}

        Pass
        {
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }
			//ColorMask RGB
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMaterial AmbientAndDiffuse

            SetTexture [_MainTex] 
            {
                Combine texture
            }
            SetTexture [_Texture] 
            {
                Combine previous * texture
            }
        }
    }
}