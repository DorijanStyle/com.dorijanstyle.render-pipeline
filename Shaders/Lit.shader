Shader "Style/Lit"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 1.0)
        _Roughness("Roughness", Range(0, 1)) = 0.0
        _Metallic("Metallic", Range(0, 1)) = 0.0
        // emissive
        // emissive intensity
        // ambient occlusion
    }
    
    SubShader
    {
        Pass
        {
            Name "Forward Shading"
            Tags { "LightMode" = "Forward" }
            
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex ForwardVS
            #pragma fragment ForwardPS

            #include "Library/ForwardPass.hlsl"
            ENDHLSL
        }
    }
}
