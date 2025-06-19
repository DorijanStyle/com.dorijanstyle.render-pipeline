#pragma once

#include "Common.hlsl"
#include "SphericalHarmonics.hlsl"
#include "MonteCarlo.hlsl"
#include "BRDF.hlsl"

TextureCube _SourceCubemap;
SamplerState sampler_SourceCubemap;

#define THREAD_X 8
#define THREAD_Y 8
#define THREAD_GROUP (THREAD_X * THREAD_Y)

//////////////////////////////////////////////////
// ref: https://www.ppsloan.org/publications/StupidSH36.pdf + Unreal
RWStructuredBuffer<float4> OutIrradianceMapSH;
groupshared SphericalHarmonicsL2RGB IrradianceSHShared[THREAD_GROUP];


[numthreads(THREAD_X, THREAD_Y, 1)]
void ComputeIrradianceCS(uint3 id : SV_DispatchThreadId)
{
    const uint index = THREAD_X * id.y + id.x;
    const float2 uv = float2(id.xy + 0.5) / float2(THREAD_X, THREAD_Y);

    float3 H = UniformSampleSphere(uv);
    float pdf = rcp(4 * PI);
    float weight = rcp(pdf * THREAD_GROUP);

    // float surface = 4 * PI;
    // float weight = surface * rcp(THREAD_GROUP);
    
    // const float3 color = _SourceCubemap.Sample(sampler_SourceCubemap, H, 0).rgb;
    const float3 color = _SourceCubemap.SampleLevel(sampler_SourceCubemap, H, 4).rgb;
    
    SphericalHarmonicsL2 basis = SHBasisFunction3(H);
    IrradianceSHShared[index] = MulSH3(basis, color * weight);

    [unroll]
    for (uint i = THREAD_GROUP * 0.5; i > 1; i  = i >> 1)
    {
        GroupMemoryBarrierWithGroupSync();
        
        if (index < i)
        {
            IrradianceSHShared[index] = AddSH3(IrradianceSHShared[index], IrradianceSHShared[index + i]);
        }
        
    }
    GroupMemoryBarrierWithGroupSync();
    
    if (index < 1)
    {
        SphericalHarmonicsL2RGB irradiance = AddSH3(IrradianceSHShared[index], IrradianceSHShared[index + 1]);
        
        // Stupid Spherical Harmonics (SH) Tricks
        const float sqrtPI = sqrt(PI);
        const float C0 = 1.0 * rcp(2.0 * sqrtPI);
        const float C1 = sqrt(3.0) * rcp(3.0 * sqrtPI);
        const float C2 = sqrt(15.0) * rcp(8.0 * sqrtPI);
        const float C3 = sqrt(5.0) * rcp(16.0 * sqrtPI);
        const float C4 = 0.5 * C2;

        // cAr
        OutIrradianceMapSH[0].x = -C1 * irradiance.r.a0[3];
        OutIrradianceMapSH[0].y = -C1 * irradiance.r.a0[1];
        OutIrradianceMapSH[0].z =  C1 * irradiance.r.a0[2];
        OutIrradianceMapSH[0].w =  C0 * irradiance.r.a0[0] - C3 * irradiance.r.a1[2];

        // cAg
        OutIrradianceMapSH[1].x = -C1 * irradiance.g.a0[3];
        OutIrradianceMapSH[1].y = -C1 * irradiance.g.a0[1];
        OutIrradianceMapSH[1].z =  C1 * irradiance.g.a0[2];
        OutIrradianceMapSH[1].w =  C0 * irradiance.g.a0[0] - C3 * irradiance.g.a1[2];

        // cAb
        OutIrradianceMapSH[2].x = -C1 * irradiance.b.a0[3];
        OutIrradianceMapSH[2].y = -C1 * irradiance.b.a0[1];
        OutIrradianceMapSH[2].z =  C1 * irradiance.b.a0[2];
        OutIrradianceMapSH[2].w =  C0 * irradiance.b.a0[0] - C3 * irradiance.b.a1[2];

        // cBr
        OutIrradianceMapSH[3].x =  C2 * irradiance.r.a1[0];
        OutIrradianceMapSH[3].y = -C2 * irradiance.r.a1[1];
        OutIrradianceMapSH[3].z = 3.0 * C3 * irradiance.r.a1[2];
        OutIrradianceMapSH[3].w = -C2 * irradiance.r.a1[3];

        // cBg
        OutIrradianceMapSH[4].x =  C2 * irradiance.g.a1[0];
        OutIrradianceMapSH[4].y = -C2 * irradiance.g.a1[1];
        OutIrradianceMapSH[4].z = 3.0 * C3 * irradiance.g.a1[2];
        OutIrradianceMapSH[4].w = -C2 * irradiance.g.a1[3];

        // cBb
        OutIrradianceMapSH[5].x =  C2 * irradiance.b.a1[0];
        OutIrradianceMapSH[5].y = -C2 * irradiance.b.a1[1];
        OutIrradianceMapSH[5].z = 3.0 * C3 * irradiance.b.a1[2];
        OutIrradianceMapSH[5].w = -C2 * irradiance.b.a1[3];

        // cC
        OutIrradianceMapSH[6].x = C4 * irradiance.r.a2;
        OutIrradianceMapSH[6].y = C4 * irradiance.g.a2;
        OutIrradianceMapSH[6].z = C4 * irradiance.b.a2;
        OutIrradianceMapSH[6].w = 1.0;
    }
}


//////////////////////////////////////////////////
RWTexture2DArray<float3> _ReflectionMap;
uint _MipIndex;
uint _MipCount;

float3 GetCubemapVector(float2 uv, int face)
{
    if (face == 0)
    {
        return float3(1.0, -uv.y, -uv.x);
    }
    else if (face == 1)
    {
        return float3(-1.0, -uv.y, uv.x);
    }
    else if (face == 2)
    {
        return float3(uv.x, 1.0, uv.y);
    }
    else if (face == 3)
    {
        return float3(uv.x, -1.0, -uv.y);
    }
    else if (face == 4)
    {
        return float3(uv.x, -uv.y, 1.0);
    }
    else
    {
        return float3(-uv.x, -uv.y, -1.0);
    }
}

float3x3 GetOrthonormalBasis(float3 tangentZ)
{
    // TODO: postoje brzi nacini
    float3 tangentY = abs(tangentZ.z) < 0.999 ? float3(0.0, 0.0, 1.0) : float3(1.0, 0.0, 0.0);
    float3 tangentX = normalize(cross(tangentY, tangentZ));
    tangentY = cross(tangentZ, tangentX);

    return float3x3(tangentX, tangentY, tangentZ);
}

[numthreads(THREAD_X, THREAD_Y, 1)]
void FilterEnvionemntCS(uint3 id : SV_DispatchThreadId)
{
    uint width, height, slices;
    _ReflectionMap.GetDimensions(width, height, slices);
    
    float2 uv = (id.xy + 0.5) / float2(width, height);
    uv = uv * 2.0 - 1.0;

    float3 N = normalize(GetCubemapVector(uv, id.z));

    if (_MipIndex == 0)
    {
        _ReflectionMap[id] = _SourceCubemap.SampleLevel(sampler_SourceCubemap, N, 0).rgb;   
        return;
    }

    float roughness = float(_MipIndex) * rcp(_MipCount - 1);
    float solidAngleByTexel = 4.0 * PI * rcp(6 * roughness * roughness);
    float3x3 tangentToWorld = GetOrthonormalBasis(N);

    uint nbSamples = 64;

    float3 color = 0.0;
    float weight = 0.0;

    [loop]
    for (uint i = 0; i < nbSamples; i++)
    {
        float2 Xi = Hammersley(i, nbSamples);
        float3 H = ImportanceSampleGGX(Xi, pow(roughness, 4));
        float3 L = 2.0 * H.z * H - float3(0, 0, 1);
        
        float NoL = L.z;
        float NoH = H.z;
        if (NoL > 0.0)
        {
            L = mul(L, tangentToWorld);
            
            float pdf = DistributionGGX(pow(roughness, 4), NoH) * 0.25;
            float solidAngleBySample = 1.0 * rcp(nbSamples * pdf);
            float mip = 1.0 + log2(solidAngleBySample / solidAngleByTexel);
            
            color += _SourceCubemap.SampleLevel(sampler_SourceCubemap, L, mip).rgb * NoL;
            weight += NoL;
        }
    }

    _ReflectionMap[id] = color / weight;
}

//////////////////////////////////////////////////
RWTexture2D<float2> _BRDFLut;

[numthreads(THREAD_X, THREAD_Y, 1)]
void IntegrateBRDFCS(uint3 id : SV_DispatchThreadId)
{
    uint width, height;
    _BRDFLut.GetDimensions(width, height);

    float2 uv = (id.xy + 0.5) / float2(width, height);

    float NoV = uv.x;
    float roughness = uv.y;

    float3 V;
    V.x = sqrt(1.0 - NoV * NoV);
    V.y = 0.0;
    V.z = NoV;

    float A = 0.0;
    float B = 0.0;

    uint nbSamples = 128;

    [loop]
    for (uint i = 0; i < nbSamples; i++)
    {
        float2 Xi = Hammersley(i, nbSamples);
        float3 H = ImportanceSampleGGX(Xi, pow(roughness, 4));
        float3 L = 2 * dot(V, H) * H - V;

        float NoL = saturate(L.z);
        float NoH = saturate(H.z);
        float VoH = saturate(dot(V, H));

        if (NoL > 0.0)
        {
            float Vis = VisibilitySmithGGXCorrelatedFast(pow(roughness, 4), NoV, NoL);

            // pdf = D * NoH / (4 * VoH)
            // BRDF = D * G * F / (4 * NoL * NoV) = D * V * F
            // BRDF / pdf = G * F * VoH / (NoH * NoL * NoV) = D * V * F
            
            Vis = Vis * VoH * NoL * 4 * rcp(NoH);
            float F = pow(1.0 - VoH, 5.0);

            A += (1.0 - F) * Vis;
            B += F * Vis;
        }
    }

    _BRDFLut[id.xy] = float2(A, B) / nbSamples;
}