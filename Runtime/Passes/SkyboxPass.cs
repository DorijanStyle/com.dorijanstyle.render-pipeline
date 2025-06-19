using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

public class SkyboxPass
{
    private static readonly ProfilingSampler Sampler = new("Skybox Pass");

    protected Cubemap cubemap;
    
    public static void RecordCubemap(RenderGraph graph, Cubemap cubemap, ref FrameTextures textures)
    {
        using var builder = graph.AddRasterRenderPass<SkyboxPass>(Sampler.name, out var pass, Sampler);

        pass.cubemap = textures.environment;
        
        builder.AllowPassCulling(false);
        builder.SetRenderAttachment(textures.color, 0, AccessFlags.Write);
        builder.SetRenderAttachmentDepth(textures.depth, AccessFlags.Write);
        builder.SetRenderFunc<SkyboxPass>((pass, context) =>
        {
            var cmd = context.cmd;
            
            MaterialPropertyBlock properties = new();
            properties.SetTexture("_SkyboxCubemap", pass.cubemap);
            
            // cmd.DrawProcedural(Matrix4x4.identity, pass.material, 0, MeshTopology.Triangles, 3, 1, properties);
            cmd.DrawUtils(UtilsPass.DRAW_CUBEMAP_SKYBOX, properties);
        });
    }
}