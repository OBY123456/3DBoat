// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles & Tom Read Cutting

// Draw cached depths into current frame ocean depth data
Shader "Crest/Inputs/Depth/Cached Depths"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}

	SubShader
	{
		Pass
		{
			// Min blending to take the min of all depths. Similar in spirit to zbuffer'd visibility when viewing from top down.
			// To confuse matters further, ocean depth is now more like 'sea floor altitude' - a height above a deep water value,
			// so values are increasing in Y and we need to take the MAX of all depths.
			BlendOp Min

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			#include "../../OceanInput.hlsl"
			#include "../../OceanLODData.hlsl"

			sampler2D _MainTex;

			CBUFFER_START(UnityPerObject)
			float4 _MainTex_ST;
			int _CurrentLodCount;
			float4x4 _SliceViewProjMatrices[MAX_LOD_COUNT];
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings output;
				output.position = TransformObjectToHClip(input.positionOS);
				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				return output;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				return half4(tex2D(_MainTex, input.uv).x, 0.0, 0.0, 0.0);
			}
			ENDHLSL
		}
	}
}
