#pragma once

#include "Math.hlsl"

float2 Hammersley(uint i, uint N)
{
    float x = frac((float)i / N);
    float y = float(reversebits(i)) * 2.3283064365386963e-10;
    return float2(x, y);
}

float3 UniformSampleHemisphere(float2 Xi)
{
    float phi = 2 * PI * Xi.x;
    float cosTheta = Xi.y;
    float sinTheta = sqrt(1.0 - cosTheta * cosTheta);

    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;

    return H;
}

float3 UniformSampleSphere(float2 Xi)
{
    float phi = 2 * PI * Xi.x;
    float cosTheta = 1 - 2 * Xi.y;
    float sinTheta = sqrt(1 - cosTheta * cosTheta);

    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;

    return H;
}

float3 ImportanceSampleGGX(float2 Xi, float a2)
{
    float phi = 2 * PI * Xi.x;
    float cosTheta = sqrt((1.0 - Xi.y) / (1.0 + (a2 - 1.0) * Xi.y));
    float sinTheta = sqrt(1.0 - cosTheta * cosTheta);
    
    float3 H;
    H.x = cos(phi) * sinTheta;
    H.y = sin(phi) * sinTheta;
    H.z = cosTheta;
    
    return H;
}