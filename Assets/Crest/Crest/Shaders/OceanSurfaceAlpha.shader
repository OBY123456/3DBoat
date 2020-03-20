﻿// Crest Ocean System for LWRP

// Copyright 2019 Huw Bowles

// Renders alpha geometry overlaid on ocean surface. Samples the ocean shape texture in the vertex shader to track
// the surface. Requires the right texture to be assigned (see RenderAlphaOnSurface script).
Shader "Crest/Ocean Surface Alpha"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Alpha("Alpha Multiplier", Range(0.0, 1.0)) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeSrc("Src Blend Mode", Int) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendModeTgt("Tgt Blend Mode", Int) = 10
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }

		Pass
		{
			Blend [_BlendModeSrc] [_BlendModeTgt]

			ZWrite Off
			// Depth offset to stop intersection with water. "Factor" and "Units". typical seems to be (-1,-1). (-0.5,0) gives
			// pretty good results for me when alpha geometry is fairly well matched but fails when alpha geo is too low res.
			// the ludicrously large value below seems to work in most of my tests.
			Offset 0, -1000000

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag
			#pragma multi_compile_fog

			#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
			#include "OceanConstants.hlsl"

			sampler2D _MainTex;
			Texture2DArray _LD_TexArray_AnimatedWaves;
			SamplerState LODData_linear_clamp_sampler;

			CBUFFER_START(CrestPerObject)
			float3 _OceanCenterPosWorld;
			half _Alpha;
			float4 _MainTex_ST;

			float4 _GeomData;
			// MeshScaleLerp, FarNormalsWeight, LODIndex (debug)
			float3 _InstanceData;

			float4 _LD_Params[MAX_LOD_COUNT];
			float3 _LD_Pos_Scale[MAX_LOD_COUNT];
			float _LD_SliceIndex;
			float4 _LD_Params_Source[MAX_LOD_COUNT];
			float3 _LD_Pos_Scale_Source[MAX_LOD_COUNT];
			CBUFFER_END

			#include "OceanHelpers.hlsl"
			#include "OceanLODData.hlsl"

			struct Attributes
			{
				float3 positionOS : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 uv_fogFactor : TEXCOORD0;
			};

			Varyings Vert(Attributes input)
			{
				Varyings o;

				// move to world
				float3 worldPos;
				worldPos.xz = TransformObjectToWorld(input.positionOS).xz;
				worldPos.y = 0.0;

				// vertex snapping and lod transition
				float lodAlpha = ComputeLodAlpha(worldPos, _InstanceData.x);

				// sample shape textures - always lerp between 2 scales, so sample two textures

				// sample weights. params.z allows shape to be faded out (used on last lod to support pop-less scale transitions)
				float wt_smallerLod = (1.0 - lodAlpha) * _LD_Params[_LD_SliceIndex].z;
				float wt_biggerLod = (1.0 - wt_smallerLod) * _LD_Params[_LD_SliceIndex + 1].z;
				// sample displacement textures, add results to current world pos / normal / foam
				const float2 wxz = worldPos.xz;
				half foam = 0.0;
				half sss = 0.;
				SampleDisplacements(_LD_TexArray_AnimatedWaves, WorldToUV(wxz), wt_smallerLod, worldPos, sss);
				SampleDisplacements(_LD_TexArray_AnimatedWaves, WorldToUV_BiggerLod(wxz), wt_biggerLod, worldPos, sss);

				// move to sea level
				worldPos.y += _OceanCenterPosWorld.y;

				// view-projection
				o.positionCS = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));

				o.uv_fogFactor.xy = TRANSFORM_TEX(input.uv, _MainTex);

				o.uv_fogFactor.z = ComputeFogFactor(o.positionCS.z);

				return o;
			}

			real4 Frag(Varyings input) : SV_Target
			{
				real4 col = tex2D(_MainTex, input.uv_fogFactor.xy);

				col.rgb = MixFog(col.rgb, input.uv_fogFactor.z);

				col.a *= _Alpha;

				return col;
			}
			ENDHLSL
		}
	}
}
