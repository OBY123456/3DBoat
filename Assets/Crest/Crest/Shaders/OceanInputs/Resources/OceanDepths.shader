// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

// Renders ocean depth - signed distance from sea level to sea floor
Shader "Crest/Inputs/Depth/Ocean Depth From Geometry"
{
	SubShader
	{
		Pass
		{
			BlendOp Min

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
		
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

			CBUFFER_START(UnityPerObject)
			float4 _LD_Params_0;
			float4 _LD_Params_1;
			float3 _LD_Pos_Scale_0;
			float3 _LD_Pos_Scale_1;
			float3 _GeomData;
			float3 _OceanCenterPosWorld;
			CBUFFER_END

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float depth : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = TransformObjectToHClip(input.positionOS);

				float altitude = TransformObjectToWorld(input.positionOS).y;

				o.depth = _OceanCenterPosWorld.y - altitude;

				return o;
			}

			float Frag(Varyings input) : SV_Target
			{
				return input.depth;
			}
			ENDHLSL
		}
	}
}
