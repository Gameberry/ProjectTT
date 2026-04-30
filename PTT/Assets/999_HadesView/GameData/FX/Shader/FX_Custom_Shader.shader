

Shader "Shader/FX/Custom" 
{
	Properties {

       	[Enum(SrcAlpha,5)] _Blend ("Blend mode", Float) = 5
		[Space(20)]
		[Enum(One,1,OneMinus,10)] _Blend2("Blend mode subset", float) = 1

		//////////////////////////소프트 파티클 옵션 ///////////////////////////////////
		[Toggle] _USESOFTPARTICLES("Use Soft Particles", float) = 0
    	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		//////////////////////////텍스쳐 옵션 //////////////////////////////////////////
		[HDR]_MainTex ("Particle Texture", 2D) = "white" {}
		_DistortionTex ("Distortion Texture", 2D) = "white" {}
        _MaskTex ("--- Mask Texture ---", 2D) = "white" {}

	
		//////////////////////////메인텍스쳐 컨트롤 옵션/////////////////////////////////
		_Opacity ("Opacity", Range (0,1)) = 1
        
		[Toggle] _AlphaCut ("Use CutOff?", float) = 0
		_Cutoff ("Cut off", Range(0, 1)) = 0.5
		_EdgeWidth ("Edge Width", Range(0, 1)) = 0.05
		[HDR] _EdgeColor ("Edge Color", Color) = (1,1,1,1)

		[HDR]_TintColor ("Tint Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Multiplier ("Color Multiplier", Float) = 1.0
		_USpeed ("U Speed", Float) = 0.0
		_VSpeed ("V Speed", Float) = 0.0
	
		////////////////////////////디스토션 텍스쳐 컨트롤 옵션//////////////////////////
	
		_DistortionUSpeed ("Distortion U Speed", Float) = 0.0
		_DistortionVSpeed ("Distortion V Speed", Float) = 0.0
	
		_UDistortionFactor ("Distortion Factor(U)", Float) = 0.0
		_VDistortionFactor ("Distortion Factor(V)", Float) = 0.0

        _StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	Category {
        
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}

		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
		Blend [_Blend] [_Blend2]
		ColorMask RGB
		Cull Off Lighting Off ZWrite Off
		ColorMask [_ColorMask]


		SubShader {

			Pass {
			
				HLSLPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_instancing
				#pragma multi_compile _USESOFTPARTICLES_OFF _USESOFTPARTICLES_ON 
				#pragma shader_feature _ALPHACUT_OFF _ALPHACUT_ON
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				
				CBUFFER_START(UnityPerMaterial)
				TEXTURE2D(_MainTex);
				SAMPLER(sampler_MainTex);
				float4 _MainTex_ST;
				TEXTURE2D(_DistortionTex);
				SAMPLER(sampler_DistortionTex);
				float4 _DistortionTex_ST;	
				
	     float _Cutoff;
       		 	float _EdgeWidth;
				float4 _EdgeColor;

				float4 _TintColor;
				float _Multiplier;
	
				float _USpeed;
				float _VSpeed;
	
				float _DistortionUSpeed;
				float _DistortionVSpeed;

				float _UDistortionFactor;
				float _VDistortionFactor;

				float _Opacity;
				TEXTURE2D(_MaskTex);
				SAMPLER(sampler_MaskTex);
				float4 _MaskTex_ST;

				TEXTURE2D(_CameraDepthTexture);
				SAMPLER(sampler_CameraDepthTexture);
				
				//UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            	float _InvFade;
				CBUFFER_END

				struct appdata_t {
					float4 vertex : POSITION;
					float4 color : COLOR;
					float3 texcoord : TEXCOORD0;	
				};


				struct v2f {
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float2 texcoord : TEXCOORD0;
					float2 texcoord1 : TEXCOORD1;
					float2 texcoord2 : TEXCOORD2;

					//#ifdef SOFTPARTICLES_ON
                	float4 projPos : TEXCOORD3;
                	//#endif

					float CustomData1 : TEXCOORD4;
					float CustomData2 : TEXCOORD5;

				};
				

				v2f vert (appdata_t v)
				{
					v2f o;

					o.vertex = TransformObjectToHClip(v.vertex.xyz);
					
					///////// 메인 텍스쳐////////
					float2 uv = TRANSFORM_TEX(v.texcoord,_MainTex);			
					const float2 center = float2(0.5f, 0.5f);
					const float deg2rad = 0.01745328f;
					
					float2 _uv = uv - center;
					
					float uoffset = _USpeed * _Time.y;
					float voffset = _VSpeed * _Time.y; // UV 스피드 사용


					uv.x = uv.x + uoffset; // UV 오프셋 사용
					uv.y = uv.y + voffset; // UV 오프셋 사용	


					////////////// 디스토션 텍스쳐 ///////////////
					float2 uv2 = TRANSFORM_TEX(v.texcoord, _DistortionTex);

					_uv = uv2 - center;

					uoffset = _DistortionUSpeed * _Time.y; 
					voffset = _DistortionVSpeed * _Time.y; 

					uv2.x = uv2.x + uoffset;
					uv2.y = uv2.y + voffset;

					o.texcoord = uv;
					o.texcoord1 = uv2 * 2.0f - 0.5f;
					o.texcoord2 = TRANSFORM_TEX(v.texcoord, _MaskTex);	
					
					#if _USESOFTPARTICLES_ON
						//#ifdef SOFTPARTICLES_ON
                			o.projPos = ComputeScreenPos (o.vertex);
                		//COMPUTE_EYEDEPTH(o.projPos.z);
                		//#endif
					#endif

				
					o.CustomData1 = v.texcoord.z;	//! CustomData1로 입력된 Z값을 전달
					o.CustomData2 = v.texcoord.x;	//! CustomData2로 입력된 X값을 전달

					o.color = v.color;
				    
					return o;
				}
				            
				float4 frag (v2f i) : SV_Target
				{

					float4 distortionTexColor = SAMPLE_TEXTURE2D(_DistortionTex, sampler_DistortionTex,i.texcoord1);
					float4 maskTex = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.texcoord2);
					float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex,i.texcoord + float2(distortionTexColor.r * _UDistortionFactor, distortionTexColor.r * _VDistortionFactor));


					#if _USESOFTPARTICLES_ON
				
							float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.projPos.xy / i.projPos.w), _ZBufferParams);                
							float thisZ = LinearEyeDepth(i.projPos.z/ i.projPos.w, _ZBufferParams);               
							float fade = saturate (_InvFade * (sceneDepth-thisZ)); 

							i.color.a *= fade;
					#endif	
					
					float fCustomData1 = i.CustomData1; //! uv.z로 받은 CustomData를 변수를 만들어서 할당
					float4 color = saturate(mainTex * i.color) * _TintColor* _Multiplier *maskTex * _Opacity;
					

					/// 컷오프 ///
					#if _ALPHACUT_ON
						clip(distortionTexColor * mainTex.r >= _Cutoff * (_EdgeWidth + 0.7) ? 1:-1);
					#endif
					//color.a = ceil(color.r + color.g - fCustomData1) * i.color.a;
					color.a *= saturate(ceil(distortionTexColor.r - fCustomData1) + ceil(distortionTexColor.g - fCustomData1)-0.02f) * i.color.a;	// 원래 커스텀 데이터 알파

					return color;

				}
				ENDHLSL
			}	
		}	
	}
}
