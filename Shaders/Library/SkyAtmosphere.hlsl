#pragma once

#include "Common.hlsl"

TextureCube _SkyboxCubemap;
SamplerState sampler_SkyboxCubemap;

void SkyboxCubemapVS(
    in uint id : SV_VertexID,
    out float4 positionCS : SV_POSITION,
    out float3 viewWS : TEXCOORD0
    )
{
    float2 uv = float2((id << 1) & 2, id & 2);
    positionCS = float4(uv * 2.0 - 1.0, 0.0, 1.0);
    viewWS = mul((float3x3)_InvViewMatrix, mul(_InvProjectionMatrix, positionCS).xyz);
}

void SkyboxCubemapPS(
    in float4 positionCS : SV_POSITION,
    in float3 viewWS : TEXCOORD0,
    out float4 color : SV_TARGET)
{
    float3 view = normalize(viewWS);
    color = _SkyboxCubemap.SampleLevel(sampler_SkyboxCubemap, view, 0);
}