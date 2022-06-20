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
			"RenderType" = "Transparent"    // ����ť 3000���� ����
			"Queue" = "Transparent"         // ����ť 3000 ���ؿ� �׸���.
			"IgnoreProjector" = "True"      // �������Ϳ� ���� ���� �ʰ� �����Ѵ�. (�������ʹ� �ٸ� Material�� '���̴� ��� ��' ������ �ִ� ���̴��� ���Ѵ�.)
			"DisableBatching" = "True"      // ��ο��� ��Ī ��Ȱ��ȭ ó��
            "PreviewType" = "Plane"         // �̸����� ǥ�ø� �÷����� ���̰��Ѵ�. (�����Ϳ� �������� �߿����� ����)
		}

        Pass
        {
		    Blend SrcAlpha OneMinusSrcAlpha
		    //ColorMaterial AmbientAndDiffuse

            SetTexture [_MainTex]   // = previous
            SetTexture [_Texture] 
            {
                Combine previous * texture  // ������ ���� ���Ѵ�. ��������� ���İ��� 0�� ��쿡�� ���ϸ� 0���� ó���Ǿ� �ȼ��� �� ���̰� ó���Ѵ�.
            }
        }
    }
}