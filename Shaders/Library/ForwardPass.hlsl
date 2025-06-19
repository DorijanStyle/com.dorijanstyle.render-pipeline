#pragma once

#include "Lighting.hlsl"

cbuffer UnityPerMaterial
{
    float4 _BaseColor;
    float _Roughness;
    float _Metallic;
};

struct Attributes
{
    float4 positionOS : POSITION;
    float3 normalOS   : NORMAL;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float3 positionWS : TEXCOORD0;
    float3 normalWS   : TEXCOORD1;
};

void ForwardVS(in Attributes input, out Varyings output)
{
    float4 positionWS = TransformObjectToWorld(input.positionOS);
    float4 positionVS = mul(_ViewMatrix, positionWS);
    float4 positionCS = mul(_ProjectionMatrix, positionVS);

    output.positionCS = positionCS;
    output.positionWS = positionWS.xyz;
    output.normalWS = TransformNormalToWorld(input.normalOS);
}

// TODO: remove
float3 _LightDirection;
float3 _LightColor;

Texture2D _BRDFLut;
SamplerState sampler_BRDFLut;

void ForwardPS(in Varyings input, out float3 color : SV_TARGET)
{
    float3 albedo = _BaseColor.rgb;
    float roughness = max(0.07, _Roughness);
    float metallic = _Metallic;

    float3 N = normalize(input.normalWS);

    #if 1
    float3 V = normalize(_CameraPosition - input.positionWS);
    #else
    float3 V = normalize(float3(_ViewMatrix._31, _ViewMatrix._32, _ViewMatrix._33));
    #endif
    
    float3 L = _LightDirection;
    float3 H = normalize(V + L);

    float NoV = saturate(abs(dot(N, V)) + 1e-5); // removes artifacts on edges
    float NoL = saturate(dot(N, L));
    float NoH = saturate(dot(N, H));
    float VoH = saturate(dot(V, H));

    float a2 = pow(roughness, 4);
    float3 f0 = lerp(0.04, albedo, metallic);
    
    float2 dfg = _BRDFLut.SampleLevel(sampler_BRDFLut, float2(NoV, roughness), 0);

    color = 0.0;
    
    // direct
    {
        float3 diffuse = DiffuseLambert(albedo) * (1.0 - metallic);
        // diffuse = diffuse * (1.0 - f0); // more accurate but not important

        float D = DistributionGGX(a2, NoH);
        float Vis = VisibilitySmithGGXCorrelatedFast(a2, NoV, NoL);
        float3 F = FresnelSchlick(f0, VoH);

        float3 specular = (D * Vis) * F;
        specular = specular * (1.0 + f0 * (rcp(dfg.x) - 1.0));
        
        color += (diffuse + specular) * _LightColor * NoL;
    }
    // color += IntegrateBRDF(albedo, roughness, metallic, N, V, L, _LightColor);

    // ambient
    {
        float3 R = reflect(-V, N);
        
        // fersnel
        float3 diffuse = Irradiance(input.normalWS) * DiffuseLambert(albedo) * (1.0 - metallic);
        float3 specular = _SkyReflection.SampleLevel(sampler_SkyReflection, R, roughness * 8);
        specular = specular * (f0 * dfg.x + dfg.y);

        color += (diffuse + specular);
    }
}
