// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

Shader "Crest/Inputs/Animated Waves/Wave Particle"
{
	Properties
	{
		_Amplitude( "Amplitude", float ) = 1
		_Radius( "Radius", float) = 3
	}

	SubShader
	{
		Tags { "DisableBatching" = "True" }

		Pass
		{
			Blend One One
		
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			
			CBUFFER_START(UnityPerObject)
			float _Radius;
			float _Amplitude;
			// TODO add this for all ocean inputs?
			float _Weight;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 worldOffsetScaledXZ : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = TransformObjectToHClip(input.positionOS);

				float3 worldPos = TransformObjectToWorld(input.positionOS);
				float3 centerPos = unity_ObjectToWorld._m03_m13_m23;
				o.worldOffsetScaledXZ = worldPos.xz - centerPos.xz;

				// shape is symmetric around center with known radius - fix the vert positions to perfectly wrap the shape.
				o.worldOffsetScaledXZ = sign(o.worldOffsetScaledXZ);
				float4 newWorldPos = float4(centerPos, 1.);
				newWorldPos.xz += o.worldOffsetScaledXZ * _Radius;
				o.positionCS = mul(UNITY_MATRIX_VP, newWorldPos);

				return o;
			}

			float4 Frag(Varyings input) : SV_Target
			{
				// power 4 smoothstep - no normalize needed
				// credit goes to stubbe's shadertoy: https://www.shadertoy.com/view/4ldSD2
				float r2 = dot( input.worldOffsetScaledXZ, input.worldOffsetScaledXZ);
				if( r2 > 1.0 )
					return (float4)0.0;

				r2 = 1.0 - r2;

				float y = r2 * r2 * _Amplitude;

				return float4(0.0, y * _Weight, 0.0, 0.0);
			}

			ENDHLSL
		}
	}
}
