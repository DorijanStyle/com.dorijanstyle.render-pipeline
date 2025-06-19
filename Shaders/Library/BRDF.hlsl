#pragma once

#include "Math.hlsl"

//////////////////////////////////////////
half3 DiffuseLambert(half3 color)
{
    return color * rcp(PI);
}

//////////////////////////////////////////
float DistributionGGX(float a2, float NoH)
{
    float d = (NoH * a2 - NoH) * NoH + 1;
    return a2 * rcp(PI * d * d);
}

float DistributionBeckmann(float a2, float NoH)
{
    float NoH2 = NoH * NoH;
    return exp((NoH2 - 1) * rcp(a2 * NoH2)) * rcp(PI * a2 * NoH2 * NoH2);
}

//////////////////////////////////////////
float VisibilitySmithGGX(float a2, float NoV, float NoL)
{
    float smithV = NoL * sqrt(NoV * (NoV - NoV * a2) + a2);
    float smithL = NoV * sqrt(NoL * (NoL - NoL * a2) + a2);
    return 0.5 * rcp(smithV * smithL);
}

float VisibilitySmithGGXCorrelatedFast(float a2, float NoV, float NoL)
{
    float a = sqrt(a2);
    float smithV = NoL * (NoV * (1.0 - a) + a);
    float smithL = NoV * (NoL * (1.0 - a) + a);
    return 0.5 * rcp(smithV + smithL);
}

//////////////////////////////////////////
float3 FresnelSchlick(float3 f0, float3 f90, float VoH)
{
    return f0 + (f90 - f0) * pow(1.0 - VoH, 5);
}

float3 FresnelSchlick(float3 f0, float VoH) // TODO: rethink
{
    float t = pow(1.0 - VoH, 5.0);
    return f0 + (1 - f0) * t;
    // float t = pow(1.0 - VoH, 5);
    // float3 f90 = saturate(50.0 * f0.g);
    // return f90 * t + (1 - t) * f0;
}
//////////////////////////////////////////
// ref: https://www.unrealengine.com/en-US/blog/physically-based-shading-on-mobile?utm_source=chatgpt.com
float3 EnvBRDFApproximation(half3 f0, float roughness, float NoV)
{
    const float4 c0 = float4(-1.0, -0.0275, -0.572, 0.022);
    const float4 c1 = float4(1.0, 0.0425, 1.04, -0.04);
    float4 r = roughness * c0 + c1;
    float a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
    float2 AB = float2(-1.04, 1.04) * a004 + r.zw;
    return f0 * AB.x + AB.y;
}

float EnvBRDFApproximationNonmetal(float roughness, float NoV)
{
    const float2 c0 = float2(-1.0, -0.0275);
    const float2 c1 = float2(1.0, 0.0425);
    float2 r = roughness * c0 + c1;
    return min( r.x * r.x, exp2( -9.28 * NoV ) ) * r.x + r.y;
}

float3 EnvBRDF(float3 f0, float roughness, float NoV)
{
    
}