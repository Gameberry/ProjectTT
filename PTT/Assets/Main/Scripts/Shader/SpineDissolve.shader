Shader "Custom/SpineDissolve"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        _Color("Tint Color", Color) = (1,1,1,1)

        _NoiseTex("Noise Texture", 2D) = "gray" {}
        _DissolveAmount("Dissolve Amount", Range(0,1)) = 0
        _EdgeWidth("Edge Width", Range(0,0.5)) = 0.1
        _EdgeColor("Edge Color", Color) = (1,0.5,0,1)

        _AlphaCutoff("Alpha Cutoff", Range(0,1)) = 0.01

        // 🔹 밖에서 안으로 타들어가는 정도 (0이면 방향성 없음, 1이면 완전 바깥→안)
        _RadialStrength("Radial Strength", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Blend One OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _NoiseTex;
            float4 _Color;
            float _DissolveAmount;
            float _EdgeWidth;
            float4 _EdgeColor;
            float _AlphaCutoff;
            float _RadialStrength;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;   // Spine vertex color
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 0) 기본 색
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 1) 원본 알파 기준으로 거의 투명한 영역은 그냥 버림
                clip(col.a - _AlphaCutoff);

                // 2) 노이즈
                float noise = tex2D(_NoiseTex, i.uv).r;

                // 3) 밖에서 안으로 타들어가도록 하는 반경 값
                //   - center: (0.5, 0.5)
                //   - dist: 중심에서 얼마나 떨어져 있는지 (0=중심, ~0.7=모서리)
                float2 center = float2(0.5, 0.5);
                float dist = distance(i.uv, center);
                // 최대 거리(대각선 모서리까지)를 대략 0.707로 보고 정규화
                float distN = saturate(dist / 0.7071);

                // 4) 바깥쪽일수록 먼저 사라지도록 noise에 반경값을 섞어줌
                //    _RadialStrength 가 0이면 기존과 동일한 노이즈 디졸브,
                //    1 이상으로 올릴수록 "가장자리 우선" 효과 강해짐
                float radialBias = distN * _RadialStrength;

                // 최종 디졸브 기준값
                float dissolveMask = noise + radialBias;

                float threshold = _DissolveAmount;

                // 코어 부분 제거 (mask < threshold 인 영역들 제거)
                float diff = dissolveMask - threshold;
                clip(diff);

                // 5) 엣지 영역 연출
                float edge = saturate(diff / _EdgeWidth);   // 0~1
                float edgeFactor = 1.0 - edge;

                // 엣지 쪽은 EdgeColor, 안쪽은 원본색
                col.rgb = lerp(_EdgeColor.rgb, col.rgb, edge);

                return col;
            }
            ENDCG
        }
    }

    FallBack "Transparent/Diffuse"
}
