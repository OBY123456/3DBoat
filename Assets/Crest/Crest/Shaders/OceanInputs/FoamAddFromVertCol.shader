// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

Shader "Crest/Inputs/Foam/Add From Vert Colours"
{
	Properties
	{
		_Strength("Strength", float) = 1
	}

	SubShader
	{
		Blend One One

		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"

			CBUFFER_START(UnityPerObject)
			float _Strength;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
				float4 col : COLOR0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 col : COLOR0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = TransformObjectToHClip(input.positionOS);
				o.col = input.col;
				return o;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				return _Strength * input.col.x;
			}
			ENDHLSL
		}
	}
}
