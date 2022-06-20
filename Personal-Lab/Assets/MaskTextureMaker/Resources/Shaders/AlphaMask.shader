Shader "MaskTexture/AlphaMask" 
{
    Properties {
        _MainTex ("Mask Texture (A)", 2D) = "white" {}
        _Texture ("Texture (RGBA) ", 2D) = "white" {}
    }

    SubShader 
    {
		LOD 100

		Tags
		{
			"RenderType" = "Transparent"    // 랜더큐 3000으로 세팅
			"Queue" = "Transparent"         // 랜더큐 3000 수준에 그린다.
			"IgnoreProjector" = "True"      // 프로젝터에 영향 받지 않게 설정한다. (프로젝터는 다른 Material에 '셰이더 계산 값' 영향을 주는 셰이더를 말한다.)
			"DisableBatching" = "True"      // 드로우콜 배칭 비활성화 처리
            "PreviewType" = "Plane"         // 미리보기 표시를 플랜으로 보이게한다. (에디터용 설정으로 중요하지 않음)
		}

        Pass
        {
		    Blend SrcAlpha OneMinusSrcAlpha
		    //ColorMaterial AmbientAndDiffuse

            SetTexture [_MainTex]   // = previous
            SetTexture [_Texture] 
            {
                Combine previous * texture  // 색상을 서로 곱한다. 결과적으로 알파값이 0인 경우에는 곱하면 0으로 처리되어 픽셀을 안 보이게 처리한다.
            }
        }
    }
}