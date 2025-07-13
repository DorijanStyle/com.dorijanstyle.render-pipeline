using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

public readonly ref struct FrameTextures
{
    public readonly TextureHandle color;
    public readonly TextureHandle depth;
    public readonly TextureHandle depthResolved;
    public readonly TextureHandle backBuffer;
    public readonly Cubemap environment;

    public FrameTextures(RenderGraph graph, Camera camera, Cubemap cubemap)
    {
        color = StyleRenderPipeline.CreateColorBuffer(graph, camera);
        depth = StyleRenderPipeline.CreateDepthBuffer(graph, camera);
        depthResolved = TextureHandle.nullHandle;
        
        backBuffer = graph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget);
        environment = cubemap;
    }
}

public enum UtilsPass
{
    DRAW_CUBEMAP_SKYBOX,
    DRAW_FINAL
}

public enum UtilsKernel
{
    COMPUTE_IRRADIANCE_MAP,
    FILTER_ENVIRONEMNT_MAP,
    INTEGRATE_BRDF
}

public class BuiltinShaders
{
    public readonly Material utilsMaterial;
    public readonly ComputeShader utilsCompute;
    public readonly Dictionary<UtilsKernel, int> kernels; 
    
    public BuiltinShaders()
    {
        utilsMaterial = CoreUtils.CreateEngineMaterial("Style/Utils");
        utilsCompute = (ComputeShader)AssetDatabase.LoadAssetAtPath("Packages/com.dorijanstyle.render-pipeline/Shaders/Utils.compute", typeof(ComputeShader));
        
        kernels = new Dictionary<UtilsKernel, int>();
        kernels.Add(UtilsKernel.COMPUTE_IRRADIANCE_MAP, utilsCompute.FindKernel("ComputeIrradianceCS"));
        kernels.Add(UtilsKernel.FILTER_ENVIRONEMNT_MAP, utilsCompute.FindKernel("FilterEnvionemntCS"));
        kernels.Add(UtilsKernel.INTEGRATE_BRDF, utilsCompute.FindKernel("IntegrateBRDFCS"));
    }
}

[Serializable]
[SupportedOnRenderPipeline(typeof(StyleRenderPipelineAsset))]
public class RenderPipelineShaders : IRenderPipelineResources
{
    public int version { get; }
    public bool isAvailableInPlayerBuild => true;

    [ResourcePath("Shaders/Utils.shader")]
    public Shader test;
}