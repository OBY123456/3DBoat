// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

// Push water under the geometry. Needs to be rendered into all LODs - set Octave Wave length to 0.

Shader "Crest/Inputs/Animated Waves/Push Water Under Convex Hull"
{
 	SubShader
	{
 		Pass
		{
			BlendOp Min
			Cull Front

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
				// Write displacement to get from sea level of ocean to the y value of this geometry.

				// Write large XZ components - using min blending so this should not affect them.
				
				return half4(10000.0, input.worldPos.y - _OceanCenterPosWorld.y, 10000.0, 1.0);
			}
			ENDHLSL
		}
	}
}
