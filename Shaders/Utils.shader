Shader "Style/Utils"
{
    // Pass order matters
    // Must be same as in Extensions.CommandBuffer.cs (UtilsPass)
    SubShader
    {
        /*Cubemap Skybox*/ Pass {
            Name "Cubemap Skybox"
            Cull Off
            ZWrite On
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex SkyboxCubemapVS
            #pragma fragment SkyboxCubemapPS
            
            #include "Library/SkyAtmosphere.hlsl"
            ENDHLSL
        }

        /*Final Blit*/ Pass
        {
            Name "Final"
            Cull Off
            ZWrite On
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex FinalVS
            #pragma fragment FinalPS

            #include "Library/Final.hlsl"
            ENDHLSL
        }
    }
}