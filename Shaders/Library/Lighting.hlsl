#pragma once

#include "Common.hlsl"
#include "BRDF.hlsl"

float3 IntegrateBRDF(float3 albedo, float roughness, float metallic, float3 N, float3 V, float3 L, float3 C)
{
    // Calculate
    float3 H = normalize(V + L);

    float NoV = abs(dot(N, V));
    float NoL = saturate(dot(N, L));
    float NoH = saturate(dot(N, H));
    float VoH = saturate(dot(V, H));

    float a2 = pow(roughness, 4);
    float3 f0 = lerp(0.04, albedo, metallic);

    // BRDF
    float D = DistributionGGX(a2, NoH);
    float Vis = VisibilitySmithGGXCorrelatedFast(a2, NoV, NoL);
    float3 F = FresnelSchlick(f0, 1.0, VoH);

    float3 specular = (D * Vis) * F;
    float3 diffuse = DiffuseLambert(albedo) * (1.0 - metallic); // * (1.0 - F);
    
    return (specular + diffuse) * C * NoL;
}

//////////////////////////////////////////

StructuredBuffer<float4> _SkyIrradiance;
float3 Irradiance(float3 normal)
{
    float4 A = float4(normal, 1.0);
    float3 x1, x2, x3;

    x1.r = dot(_SkyIrradiance[0], A);
    x1.g = dot(_SkyIrradiance[1], A);
    x1.b = dot(_SkyIrradiance[2], A);

    float4 B = A.xyzz * A.yzzx;
    x2.r = dot(_SkyIrradiance[3], B);
    x2.g = dot(_SkyIrradiance[4], B);
    x2.b = dot(_SkyIrradiance[5], B);

    float vC = A.x * A.x - A.y * A.y;
    x3 = _SkyIrradiance[6].xyz * vC;
    
    return x1 + x2 + x3;
}

TextureCube _SkyReflection;
SamplerState sampler_SkyReflection;

float3 SpecularIBL()
{
    
}

float3 IntegrateIBL()
{
    
}