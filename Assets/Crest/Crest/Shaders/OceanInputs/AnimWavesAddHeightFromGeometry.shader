// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

// This adds the height from the geometry. This allows setting the water height to some level for rivers etc, but still
// getting the waves added on top.

Shader "Crest/Inputs/Animated Waves/Add Water Height From Geometry"
{
	Properties
	{
		[Enum(BlendOp)] _BlendOp("Blend Op", Int) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeSrc("Src Blend Mode", Int) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeTgt("Tgt Blend Mode", Int) = 1
		[Enum(ColorWriteMask)] _ColorWriteMask("Color Write Mask", Int) = 15
	}

 	SubShader
	{
 		Pass
		{
			BlendOp [_BlendOp]
			Blend [_BlendModeSrc] [_BlendModeTgt]
			ColorMask [_ColorWriteMask]

 			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"

			CBUFFER_START(UnityPerObject)
			float4 _LD_Params_0;
			float4 _LD_Params_1;
			float3 _LD_Pos_Scale_0;
			float3 _LD_Pos_Scale_1;
			float3 _GeomData;
			float3 _OceanCenterPosWorld;
			CBUFFER_END

			#include "../OceanLODData.hlsl"

 			struct Attributes
			{
				float3 positionOS : POSITION;
			};

 			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 worldPos : TEXCOORD0;
			};

 			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = TransformObjectToHClip(input.positionOS);
				o.worldPos = TransformObjectToWorld(input.positionOS);
				return o;
			}

 			half4 Frag(Varyings input) : SV_Target
			{
				// Write displacement to get from sea level of ocean to the y value of this geometry
				float addHeight = input.worldPos.y - _OceanCenterPosWorld.y;
				return half4(0.0, addHeight, 0.0, 1.0);
			}
			ENDHLSL
		}
	}
}
