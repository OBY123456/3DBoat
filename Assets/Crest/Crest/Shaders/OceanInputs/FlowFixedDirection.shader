// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

Shader "Crest/Inputs/Flow/Fixed Direction"
{
	Properties
	{
		_Speed("Speed", Range(0.0, 10.0)) = 1.0
		_Direction("Direction", Range(0.0, 1.0)) = 0.0
	}

	SubShader
	{
		Pass
		{
			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"

			CBUFFER_START(UnityPerObject)
			float _Speed;
			float _Direction;
			CBUFFER_END

			struct Attributes
			{
				float3 positionOS : POSITION;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float2 vel : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;
				o.positionCS = TransformObjectToHClip(input.positionOS);
				o.vel = _Speed * float2(cos(_Direction * 6.283185), sin(_Direction * 6.283185));
				return o;
			}
			
			float2 Frag(Varyings input) : SV_Target
			{
				return input.vel;
			}
			ENDHLSL
		}
	}
}
