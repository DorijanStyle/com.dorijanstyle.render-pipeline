using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

/*  TODO:
 *  - optimizirati renderanje IBL-a
 *  - HDR & auto exposure
 *  - bloom
 *  - lens flare
 *  - PBR sky
 *  - screen space reflectance (SSR)
 *  - clustered rendering + optimizacije (Wicked engine)
 *  - shadows
 *  - probe sustav
 *  - subsurface scattering (SSS)
 *  - transparent pass?
 *  - Materijal:
 *      - tekstura za albdeo
 *      - tekstura za metallic i roughness
 *      - tekstura za normalu
 * 
 *  Fututre:
 *  - volumetric lighting / light shafts
 *  - volumetric clouds
 *  - water
 */

public class StyleRenderPipeline : RenderPipeline
{
    public static Material utilsMaterial = CoreUtils.CreateEngineMaterial("Style/Utils");
    
    // tools
    StyleRenderPipelineAsset m_Settings;
    RenderGraph m_RenderGraph;

    RenderPipelineShaders m_Shaders;
    
    ComputeShader m_CubemapUtils;
    
    public StyleRenderPipeline(StyleRenderPipelineAsset settings)
    {
        m_Settings = settings;
        m_RenderGraph = new RenderGraph("FER Render Graph");

        m_Shaders = GraphicsSettings.GetRenderPipelineSettings<RenderPipelineShaders>();
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        throw new System.NotImplementedException("Rendering with camera array is not supported.");
    }
    
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        // DJ: SupportedRenderingFeatures affect UI
        
        //SupportedRenderingFeatures.active.rendersUIOverlay = cameras.Count > 0;
        SupportedRenderingFeatures.active.overridesEnvironmentLighting = true;

        // DJ: and GraphicsSettings affect actual rendering
        
        //GraphicsSettings.allConfiguredRenderPipelines
        //GraphicsSettings.cameraRelativeLightCulling
        //GraphicsSettings.cameraRelativeShadowCulling
        //GraphicsSettings.currentRenderPipeline
        //GraphicsSettings.currentRenderPipelineAssetType
        //GraphicsSettings.defaultGateFitMode
        //GraphicsSettings.defaultRenderingLayerMask
        //GraphicsSettings.defaultRenderPipeline
        //GraphicsSettings.disableBuiltinCustomRenderTextureUpdate
        //GraphicsSettings.isScriptableRenderPipelineEnabled
        //GraphicsSettings.lightProbeOutsideHullStrategy
        GraphicsSettings.lightsUseColorTemperature = false;
        GraphicsSettings.lightsUseLinearIntensity = true;
        //GraphicsSettings.logWhenShaderIsCompiled
        //GraphicsSettings.realtimeDirectRectangularAreaLights
        //GraphicsSettings.transparencySortAxis
        //GraphicsSettings.transparencySortMode
        GraphicsSettings.useScriptableRenderPipelineBatching = false;
        // GraphicsSettings.videoShadersIncludeMode = VideoShadersIncludeMode.Never;
        
        // DJ: and QualitySettings should be set by user
        QualitySettings.realtimeReflectionProbes = true;

        for(int i = 0; i < cameras.Count; i++)
        {
            var camera = cameras[i];

            if (camera.cameraType == CameraType.Preview || camera.cameraType == CameraType.Reflection)
            {
                continue;
            }

            // no way to set linear or space mode
            // when using Linear mode, camera texture is sRGB
            // when using Gamma mode, camera texture is uNorm 
            // if(camera.targetTexture != null)
            //     Debug.Log(camera.targetTexture.descriptor.graphicsFormat.ToString());
            
            CommandBuffer cmd = CommandBufferPool.Get();
            RenderSingleCamera(cmd, context, camera);
            CommandBufferPool.Release(cmd);
        }

        m_RenderGraph.EndFrame();
    }

    protected override void Dispose(bool disposing)
    {
        m_RenderGraph.Cleanup(); m_RenderGraph = null;
    }
    
    void RenderSingleCamera(CommandBuffer cmd, ScriptableRenderContext context, Camera camera)
    {
        if (!camera.TryGetCullingParameters(out var cullingParameters)) return;
        var cullingResult = context.Cull(ref cullingParameters);

        var parameters = new RenderGraphParameters()
        {
            executionName = "Rendering Camera", // TODO: change
            currentFrameIndex =  Time.frameCount,
            scriptableRenderContext = context,
            commandBuffer = cmd,
        };
        
        
        m_RenderGraph.BeginRecording(parameters);
        
        FrameTextures frameTextures = new FrameTextures(m_RenderGraph, camera, m_Settings.environment);
        
        // Setup //
        LightPass.Record(m_RenderGraph, cullingResult);

        EnvironmentPass.RecordAmbient(m_RenderGraph, frameTextures);
        EnvironmentPass.RecordReflection(m_RenderGraph, frameTextures);
        EnvironmentPass.RecordIntegrateBRDF(m_RenderGraph, frameTextures);
        
        // Uniforms for drawing to the screen
        SetShaderUniforms(cmd, context, camera);
        cmd.SetGlobalInt("_Debug", (int)m_Settings.debug);
        
        // Rendering /
        OpaquePass.Record(m_RenderGraph, camera, cullingResult, frameTextures);
        SkyboxPass.RecordCubemap(m_RenderGraph, m_Settings.environment, ref frameTextures);
        
        GizmoPass.Record(m_RenderGraph, camera, frameTextures);
        PostProcessPass.Record(m_RenderGraph, frameTextures, m_Settings.tonemap);
        
        m_RenderGraph.EndRecordingAndExecute();
        context.ExecuteAndClearCommandBuffer(cmd);
        context.Submit();
        
        GL.invertCulling = false;
    }

    void SetShaderUniforms(CommandBuffer cmd, ScriptableRenderContext context, Camera camera)
    {
        // Flush commands
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        
        // Needs to be called to setup editor stuff (grid, overlay, gizmos, etc...),
        // also setups native rendering shader variables - but im overriding that
        #if UNITY_EDITOR
        context.SetupCameraProperties(camera);
        #endif
        
        // Normal transform system
        Matrix4x4 projection = camera.projectionMatrix;
        Matrix4x4 view = camera.worldToCameraMatrix;
        Vector3 cameraPosition = camera.transform.position;
        
        // GPU system
        Matrix4x4 projectionGPU = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
        // Matrix4x4 projectionGPU = GL.GetGPUProjectionMatrix(camera.projectionMatrix, camera.cameraType == CameraType.SceneView);
        Matrix4x4 viewGPU = view;
        
        if (camera.cameraType == CameraType.Game)
        {
            projectionGPU.m11 *= -1.0f;
        }
        
        GL.invertCulling = camera.cameraType == CameraType.Game;
        
        // Send
        cmd.SetGlobalMatrix("_ViewMatrix", viewGPU);
        cmd.SetGlobalMatrix("_InvViewMatrix", viewGPU.inverse);
        cmd.SetGlobalMatrix("_ProjectionMatrix", projectionGPU);
        cmd.SetGlobalMatrix("_InvProjectionMatrix", projectionGPU.inverse);
        cmd.SetGlobalVector("_CameraPosition", cameraPosition);
        context.ExecuteAndClearCommandBuffer(cmd);
    }

    public static TextureHandle CreateColorBuffer(RenderGraph graph, Camera camera)
    {
        TextureDesc desc = new TextureDesc(camera.pixelWidth, camera.pixelHeight, false, false);
        desc.name = "Color Buffer";
        desc.clearBuffer = true;
        desc.colorFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.HDR);
        desc.clearBuffer = true;
        desc.clearColor = camera.backgroundColor;

        // desc.bindTextureMS = true;
        desc.msaaSamples = MSAASamples.MSAA8x;
        
        return graph.CreateTexture(desc);
    }
    
    public static TextureHandle CreateDepthBuffer(RenderGraph graph, Camera camera)
    {
        TextureDesc desc = new TextureDesc(camera.pixelWidth, camera.pixelHeight, false, false);
        desc.name = "Depth Buffer";
        desc.clearBuffer = true;
        desc.colorFormat = SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil);
        desc.clearBuffer = true;
        
        desc.msaaSamples = MSAASamples.MSAA8x;
        
        return graph.CreateTexture(desc);
    }
}
