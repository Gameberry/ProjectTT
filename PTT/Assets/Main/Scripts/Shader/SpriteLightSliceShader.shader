// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "GameBerry/SpriteLightSliceShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_UseEffect("UseEffect", Int) = 1
		_ApplyOnlyTintColor("ApplyOnlyTintColor", Int) = 0

		_GlowColor("GlowColor", Color) = (1,1,1,1)
		_GlowRange("GlowRange", Range(0,1)) = 0
		_GlowPower("GlowPower", Range(0,10)) = 0

		_SliceRange("SliceRange", Range(-1,1)) = 0

		_AnglePointX("AnglePointX", Range(0,1)) = 0
		_AnglePointY("AnglePointY", Range(0,1)) = 0

		_Angle("Angle", Range(0,360)) = 0



		_NormalMap("Normalmap", 2D) = "bump"{}

		_LightDir("LightDirection", Vector) = (1,0,0,0)

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

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
					//float3 normal : TEXCOORD1;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					//float3 normal : TEXCOORD1;
				};

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;

				int _UseEffect;
				int _ApplyOnlyTintColor;

				fixed4 _GlowColor;
				float _GlowRange;
				float _GlowPower;

				fixed4 _Color;

				float _SliceRange;

				float _AnglePointX;
				float _AnglePointY;

				float _Angle;


				sampler2D _NormalMap;
				float4 _LightDir;

				float4 _SpecCol;
				float _SpecPow;

				float4 _ShadowCol;
				float _ShadowPow;




				v2f vert(appdata_t IN)
				{
					v2f OUT;

					UNITY_INITIALIZE_OUTPUT(v2f, OUT);

					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;

					float4 inver = IN.vertex;

					if (_UseEffect == 1)
					{
						/*inver.x = inver.x * 2.0;
						inver.y = inver.y * 2.0;*/

						inver *= 2.0;
					}

					OUT.vertex = UnityObjectToClipPos(inver);

#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

					return OUT;
				}



				fixed4 SampleSpriteTexture(float2 operuv)
				{
					fixed4 color;

					if (_UseEffect == 1)
					{

						float3 pointpos;
						pointpos.x = _AnglePointX;
						pointpos.y = _AnglePointY;
						pointpos.z = 0;

						float3 uvvec;
						uvvec.x = operuv.x;
						uvvec.y = operuv.y;
						uvvec.z = 0;

						float radangle = _Angle * 0.0174532924;
						float3 otherPoint;
						otherPoint.x = cos(radangle) + pointpos.x;
						otherPoint.y = sin(radangle) + pointpos.y;
						otherPoint.z = 0;

						float3 otherdirvec = otherPoint - pointpos;
						otherdirvec = normalize(otherdirvec);

						float3 pointdir = uvvec - pointpos;

						//float3 tempvec = otherdirvec * dot(otherdirvec, pointdir);
						float3 projvec = pointdir - (dot(pointdir, otherdirvec) * otherdirvec);

						pointdir = normalize(pointdir);

						float3 crossvec;

						crossvec.x = otherdirvec.y * pointdir.z - otherdirvec.z * pointdir.y;
						crossvec.y = otherdirvec.z * pointdir.x - otherdirvec.x * pointdir.z;
						crossvec.z = otherdirvec.x * pointdir.y - otherdirvec.y * pointdir.x;


						if (crossvec.z < 0)
						{
							if (_Angle < 180)
								operuv += otherdirvec * _SliceRange;
							else
								operuv += otherdirvec * _SliceRange * -1;
						}


						float3 lightDir;
						lightDir.x = _LightDir.x;
						lightDir.y = _LightDir.y;
						lightDir.z = _LightDir.z;

						lightDir = normalize(lightDir);

						float3 cusnormal = UnpackNormal(tex2D(_NormalMap, operuv));

						float3 SpecColor;
						float ndot1 = saturate(dot(cusnormal, lightDir));
						SpecColor = ndot1 * _SpecCol * _SpecPow;

						float3 selectcolor;
						selectcolor.x = 1 - _ShadowCol.x;
						selectcolor.y = 1 - _ShadowCol.y;
						selectcolor.z = 1 - _ShadowCol.z;

						float3 ShadowColor;
						float ndot2 = saturate(dot(cusnormal, -lightDir));
						ShadowColor = ndot2 * selectcolor * _ShadowPow;


						color = tex2D(_MainTex, operuv);


						float alpha = color.a;

						float projveclength = length(projvec);


						color.rgb += SpecColor;
						color.rgb -= ShadowColor;


						if (projveclength < _GlowRange)
						{
							float powervalue = _GlowRange - projveclength;
							color = lerp(color, _GlowColor * _GlowPower, powervalue);
							color.a = alpha;
						}

						if (operuv.x < 0 || operuv.x > 1)
							color.a = 0;
						else if (operuv.y < 0 || operuv.y > 1)
							color.a = 0;
					}
					else
					{
						//color = tex2D(_MainTex, operuv);
						color = tex2D(_MainTex, operuv);
					}
					

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, operuv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float2 uv;
					if (_UseEffect == 1)
					{
						uv.x = (IN.texcoord.x * 2) - 0.5;
						uv.y = (IN.texcoord.y * 2) - 0.5;
					}
					else
					{
						uv = IN.texcoord;
					}
					
					fixed4 c = SampleSpriteTexture(uv) * IN.color;
					if(_ApplyOnlyTintColor == 1)
						c.rgb = IN.color.rgb;

					c.rgb *= c.a;

					return c;
				}

			ENDCG
			}
		}
}