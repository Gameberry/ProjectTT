Shader "GameBerry/SpriteSimpleAngleSliceShader"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_EdgeTex("Edge Texture", 2D) = "white" {}
		_EdgeRange("GlowRange", Range(0,1)) = 0
		_EdgeSpeed("EdgeSpeed", Range(-5,5)) = 0

		_AfterColor("AfterColor", Color) = (1,1,1,1)

		_AnglePointX("AnglePointX", Range(0,1)) = 0
		_AnglePointY("AnglePointY", Range(0,1)) = 0
		_Angle("Angle", Range(0,360)) = 0

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
				sampler2D _EdgeTex;
				

				float _AlphaSplitEnabled;
				float _EdgeRange;
				float _EdgeSpeed;

				fixed4 _AfterColor;

				float _AnglePointX;
				float _AnglePointY;

				float _Angle;

				v2f vert(appdata_t IN)
				{
					v2f OUT;

					UNITY_INITIALIZE_OUTPUT(v2f, OUT);

					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color/* * _Color*/;

					float4 inver = IN.vertex;

					OUT.vertex = UnityObjectToClipPos(inver);

#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
#endif

					return OUT;
				}



				fixed4 SampleSpriteTexture(float2 uv, fixed4 incolor)
				{
					float2 operuv = uv;

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

					float3 projvec = pointdir - (dot(pointdir, otherdirvec) * otherdirvec);

					pointdir = normalize(pointdir);

					float3 crossvec;

					crossvec.x = otherdirvec.y * pointdir.z - otherdirvec.z * pointdir.y;
					crossvec.y = otherdirvec.z * pointdir.x - otherdirvec.x * pointdir.z;
					crossvec.z = otherdirvec.x * pointdir.y - otherdirvec.y * pointdir.x;


						
					fixed4 color;
					color = tex2D(_MainTex, operuv);

					float alpha = color.a;

					if (crossvec.z > 0)
					{
						color.xyz = _AfterColor.xyz;
					}
					else
					{
						color.xyz = incolor.xyz;

						float projveclength = length(projvec);

						if (projveclength < _EdgeRange)
						{
							float2 edgeuv;
							edgeuv.x = projveclength / _EdgeRange;
							edgeuv.y = operuv.y + (_Time.y * _EdgeSpeed);

							while (edgeuv.y > 1)
								edgeuv.y -= 1;

							fixed4 edgecolor = tex2D(_EdgeTex, edgeuv);

							color = lerp(incolor, _AfterColor, edgecolor.a);
							color.a = alpha;

						}

					}


					

					if (operuv.x < 0 || operuv.x > 1)
						color.a = 0;
					else if (operuv.y < 0 || operuv.y > 1)
						color.a = 0;

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord, IN.color)/* * IN.color*/;

					c.rgb *= c.a;

					return c;
				}

			ENDCG
			}
		}
}
