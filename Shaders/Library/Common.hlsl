#pragma once

#include "Math.hlsl"

// Uniforms

cbuffer UnityPerFrame : register(b0)
{
    float4x4 _ViewMatrix;
    float4x4 _InvViewMatrix;
    float4x4 _ProjectionMatrix;
    float4x4 _InvProjectionMatrix;

    float3 _CameraPosition;
};

cbuffer UnityPerDraw : register(b1)
{
    #define _ModelMatrix    unity_ObjectToWorld
    #define _InvModelMatrix unity_WorldToObject
    
    float4x4 _ModelMatrix;
    float4x4 _InvModelMatrix;

    float4 unity_LODFade;
    float4 unity_WorldTransformParams;
};

// Functions with uniforms
float4 TransformObjectToWorld(in float4 position)
{
    return mul(_ModelMatrix, position);
}

float4 TransformObjectToClip(in float4 position)
{
    return mul(_ProjectionMatrix, mul(_ViewMatrix, mul(_ModelMatrix, position)));
}

float3 TransformNormalToWorld(in float3 normal)
{
    return mul(float4(normal, 0.0f), _InvModelMatrix).xyz;
}

float2 GetFullscreenTriangleUV(uint id)
{
    return float2((id << 1) & 2, id & 2);
}