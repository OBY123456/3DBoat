// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

// This writes straight into the displacement texture and sets the water height to the y value of the geometry.

Shader "Crest/Inputs/Animated Waves/Set Water Height To Geometry"
{
	Properties
	{
		[Enum(ColorWriteMask)] _ColorWriteMask("Color Write Mask", Int) = 15
	}

 	SubShader
	{
 		Pass
		{
			Blend Off
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
				float height = input.worldPos.y - _OceanCenterPosWorld.y;
				return half4(0.0, height, 0.0, 0.0);
			}
			ENDHLSL
		}
	}
}
