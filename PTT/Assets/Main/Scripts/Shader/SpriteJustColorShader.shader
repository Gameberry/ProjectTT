Shader "GameBerry/SpriteJustColorShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

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

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				fixed4 _Color;


				v2f vert(appdata_t IN)
				{
					v2f OUT;

					UNITY_INITIALIZE_OUTPUT(v2f, OUT);

					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;

					OUT.vertex = UnityObjectToClipPos(IN.vertex);

#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

					return OUT;
				}



				fixed4 SampleSpriteTexture(float2 uv)
				{
					float2 operuv = uv;
					fixed4 color;

					color = tex2D(_MainTex, operuv);

					#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, operuv).r;
					#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float2 uv;
					uv = IN.texcoord;

					fixed4 c = SampleSpriteTexture(uv) * IN.color;

					c.rgb = IN.color.rgb;

					c.rgb *= c.a;

					return c;
				}

			ENDCG
			}
		}
}