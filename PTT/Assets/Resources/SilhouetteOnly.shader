Shader "Custom/SilhouetteOnly"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _SilColor ("Silhouette Color", Color) = (1, 1, 1, 0.4)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Stencil
        {
            Ref 1
            Comp Equal     // 스텐실이 1인 곳(나무 뒤)에서만 그림
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _SilColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);
                clip(tex.a - 0.01);
                fixed4 col = _SilColor;
                col.a *= tex.a; // 스프라이트 외곽 알파 유지
                return col;
            }
            ENDCG
        }
    }
}