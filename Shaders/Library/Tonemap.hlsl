#pragma once

#include "Common.hlsl"

// ref: https://bruop.github.io/tonemapping/
// ref: https://64.github.io/tonemapping/

float3 TonemapReinhard(float3 x)
{
    return x * rcp(1.0 + x);
}

float3 TonemapReinhardExtended(float3 x, float white)
{
    float3 n = x * (1.0 + x * rcp(white * white));
    return n * rcp(1.0 + x);
}

float3 TonemapACES(float3 x)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return saturate((x * (a * x + b)) * rcp(x * (c * x + d) + e));
}