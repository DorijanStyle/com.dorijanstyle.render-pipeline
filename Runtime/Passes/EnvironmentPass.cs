using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

public class EnvironmentPass
{
    private static readonly ProfilingSampler AmbientSampler = new("Environment Ambient Pass");
    private static readonly ProfilingSampler ReflectionSampler = new("Environment Reflection Pass");
    private static readonly ProfilingSampler BRDFSampler = new("Integrate BRDF Pass");
    
    private Cubemap cubemap;
    private BufferHandle irradianceBuffer;
    private TextureHandle prefilteredTextureArray;
    private TextureHandle reflectionCubemap;
    private TextureHandle brdfLUT;
    
    // TODO: makni hardkodirani mip iz shadera i postavi ga ovdje
    public static void RecordAmbient(RenderGraph graph, FrameTextures textures)
    {
        using var builder = graph.AddRenderPass<EnvironmentPass>(AmbientSampler.name, out var pass, AmbientSampler);

        pass.cubemap = textures.environment;
        pass.irradianceBuffer = builder.WriteBuffer(graph.CreateBuffer(new BufferDesc()
        {
            name = "Sky Irradiance",
            count = 7,
            stride = sizeof(float) * 4,
            usageFlags = GraphicsBuffer.UsageFlags.None,
            target = GraphicsBuffer.Target.Structured
        }));
        
        builder.AllowPassCulling(false);
        builder.SetRenderFunc<EnvironmentPass>((pass, context) =>
        {
            var cmd = context.cmd;
            var shader = cmd.GetUtilsCompute();
            int kernel = cmd.GetUtilsKernel(UtilsKernel.COMPUTE_IRRADIANCE_MAP);
            
            shader.GetKernelThreadGroupSizes(kernel, out uint groupSizeX, out uint groupSizeY, out uint groupSizeZ);

            cmd.SetComputeTextureParam(shader, kernel, "_SourceCubemap", pass.cubemap);
            cmd.SetComputeBufferParam(shader, kernel, "OutIrradianceMapSH", pass.irradianceBuffer);
            cmd.DispatchCompute(shader, kernel, (int)groupSizeX, (int)groupSizeY, (int)groupSizeZ);
            context.renderContext.ExecuteCommandBuffer(cmd); 
            
            cmd.SetGlobalBuffer("_SkyIrradiance", pass.irradianceBuffer);
            context.renderContext.ExecuteAndClearCommandBuffer(cmd);
        });
    }

    public static void RecordReflection(RenderGraph graph, FrameTextures textures)
    {
        using var builder = graph.AddRenderPass<EnvironmentPass>(ReflectionSampler.name, out var pass);

        TextureDesc desc = new TextureDesc(256, 256)
        {
            name = "Sky Reflection",
            dimension = TextureDimension.Cube,
            format = GraphicsFormat.B10G11R11_UFloatPack32,
            autoGenerateMips = false,
            useMipMap = true,
            enableRandomWrite = true,
            filterMode = FilterMode.Trilinear,
        };
        
        pass.cubemap = textures.environment;
        pass.reflectionCubemap = builder.WriteTexture(graph.CreateTexture(desc));

        desc.name = "Sky Reflection Array";
        desc.dimension = TextureDimension.Tex2DArray;
        desc.slices = 6;
        pass.prefilteredTextureArray = builder.WriteTexture(graph.CreateTexture(desc));
        
        builder.AllowPassCulling(false);
        builder.SetRenderFunc<EnvironmentPass>((pass, context) =>
        {
            var cmd = context.cmd;
            var shader = cmd.GetUtilsCompute();
            int kernel = cmd.GetUtilsKernel(UtilsKernel.FILTER_ENVIRONEMNT_MAP);
            
            int mipCount = CoreUtils.GetMipCount(256);
            
            shader.GetKernelThreadGroupSizes(kernel, out uint groupSizeX, out uint groupSizeY, out uint groupSizeZ);

            cmd.SetComputeTextureParam(shader, kernel, "_SourceCubemap", pass.cubemap);
            cmd.SetComputeIntParam(shader, "_MipCount", mipCount);
            context.renderContext.ExecuteAndClearCommandBuffer(cmd);
            

            for (int i = 0; i < mipCount; i++)
            {
                cmd.SetComputeIntParam(shader, "_MipIndex", i);
                cmd.SetComputeTextureParam(shader, kernel, "_ReflectionMap", pass.prefilteredTextureArray, i);
                    
                cmd.DispatchCompute(shader, kernel, (int)(256 / groupSizeX), (int)(256 / groupSizeY), 6);
                context.renderContext.ExecuteAndClearCommandBuffer(cmd);
            }
            
            // stitch cubemap
            Graphics.CopyTexture(pass.prefilteredTextureArray, 0, pass.reflectionCubemap, 0);
            Graphics.CopyTexture(pass.prefilteredTextureArray, 1, pass.reflectionCubemap, 1);
            Graphics.CopyTexture(pass.prefilteredTextureArray, 2, pass.reflectionCubemap, 2);
            Graphics.CopyTexture(pass.prefilteredTextureArray, 3, pass.reflectionCubemap, 3);
            Graphics.CopyTexture(pass.prefilteredTextureArray, 4, pass.reflectionCubemap, 4);
            Graphics.CopyTexture(pass.prefilteredTextureArray, 5, pass.reflectionCubemap, 5);
            
            cmd.SetGlobalTexture("_SkyReflection", pass.reflectionCubemap);
            context.renderContext.ExecuteAndClearCommandBuffer(cmd);
        });
    }

    public static void RecordIntegrateBRDF(RenderGraph graph, FrameTextures textures)
    {
        using var builder = graph.AddRenderPass<EnvironmentPass>(BRDFSampler.name, out var pass, BRDFSampler);

        const int size = 256;
        
        pass.brdfLUT = builder.WriteTexture(graph.CreateTexture(new TextureDesc(size, size)
        {
            name = "BRDF LUT",
            autoGenerateMips = false,
            useMipMap = false,
            dimension = TextureDimension.Tex2D,
            format = GraphicsFormat.R16G16_SFloat,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp, // mora biti
            enableRandomWrite = true
        }));
        
        builder.AllowPassCulling(false);
        builder.SetRenderFunc<EnvironmentPass>((pass, context) =>
        {
            var cmd = context.cmd;
            var shader = cmd.GetUtilsCompute();
            int kernel = cmd.GetUtilsKernel(UtilsKernel.INTEGRATE_BRDF);
            
            shader.GetKernelThreadGroupSizes(kernel, out uint groupSizeX, out uint groupSizeY, out uint groupSizeZ);
            
            cmd.SetComputeTextureParam(shader, kernel, "_BRDFLut", pass.brdfLUT);
            cmd.DispatchCompute(shader, kernel, size / (int)groupSizeX, size / (int)groupSizeY, 1);
            context.renderContext.ExecuteAndClearCommandBuffer(cmd);
            
            cmd.SetGlobalTexture("_BRDFLut", pass.brdfLUT);
            context.renderContext.ExecuteAndClearCommandBuffer(cmd);
        });
    }
}