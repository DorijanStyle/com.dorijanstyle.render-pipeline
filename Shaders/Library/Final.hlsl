#pragma once

#include "Common.hlsl"
#include "Tonemap.hlsl"

Texture2D _SourceColorTexture;
Texture2D _SourceDepthTexture;

int _TonemapMode;

SamplerState sampler_point_clamp;

void FinalVS(
    in uint id : SV_VertexID,
    out float4 positionCS : SV_POSITION,
    out float2 uv : TEXCOORD0)
{
    uv = GetFullscreenTriangleUV(id);
    positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);

    uv.y = 1.0 - uv.y;
}

void FinalPS(
    in float4 positionCS : SV_POSITION,
    in float2 uv : TEXCOORD0,
    out float3 color : SV_TARGET,
    out float depth : SV_Depth)
{
    color = _SourceColorTexture.SampleLevel(sampler_point_clamp, uv, 0).rgb;
    depth = _SourceDepthTexture.SampleLevel(sampler_point_clamp, uv, 0).r;

    switch (_TonemapMode)
    {
    case 0: break;
    case 1: color = TonemapReinhard(color); break;
    case 2: color = TonemapReinhardExtended(color, 10); break;
    case 3: color = TonemapACES(color); break;
    }
    
    depth = 1.0;
}