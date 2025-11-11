Shader "GameBerry/SpriteLightShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		[PerRendererData]_NormalMap("NormalMap", 2d) = "bump"{}

		_LightDir("LightDirection", Vector) = (1,0,0)

		_SpecCol("Specular Color", Color) = (1,1,1,1)
		_SpecPow("Specular Power", Range(0,3)) = 1

		_ShadowCol("Shadow Color", Color) = (0,0,0,1)
		_ShadowPow("ShadowPow Power", Range(0,1)) = 1

		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			fixed4 _Color;

			sampler2D _NormalMap;

			float4 _SpecCol;
			float _SpecPow;

			float4 _ShadowCol;
			float _ShadowPow;

			float3 _LightDir;

			float _AlphaSplitEnabled;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float3 normal : TEXCOORD1;
			};

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;

				float4 n = tex2D(_NormalMap, IN.texcoord);

				OUT.normal = IN.normal;

				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap(OUT.vertex);
				#endif

				return OUT;
			}

			fixed4 SampleSpriteTexture(float2 uv)
			{
				fixed4 color = tex2D(_MainTex, uv);

				float3 lightDir;
				lightDir.x = _LightDir.x;
				lightDir.y = _LightDir.y;
				lightDir.z = _LightDir.z;

				_LightDir = normalize(_LightDir);

				float3 cusnormal = UnpackNormal(tex2D(_NormalMap, uv));

				float3 SpecColor;
				float ndot1 = saturate(dot(cusnormal, _LightDir));
				SpecColor = ndot1 * _SpecCol * _SpecPow;

				float3 selectcolor;
				selectcolor.x = 1 - _ShadowCol.x;
				selectcolor.y = 1 - _ShadowCol.y;
				selectcolor.z = 1 - _ShadowCol.z;

				float3 ShadowColor;
				float ndot2 = saturate(dot(cusnormal, -_LightDir));
				ShadowColor = ndot2 * selectcolor * _ShadowPow;

				color.rgb += SpecColor;
				color.rgb -= ShadowColor;

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D(_AlphaTex, uv).r;
#endif 
				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
				c.rgb *= c.a;

				return c;
			}
		ENDCG
		}
	}
}