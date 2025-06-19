using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

public class OpaquePass
{
    static readonly ProfilingSampler Sampler = new ("Opaque Pass");
    static readonly ShaderTagId[] ShaderTags = {
        new ("Forward")
    };

    protected RendererListHandle list;

    public static void Record(RenderGraph graph, Camera camera, CullingResults results, FrameTextures textures)
    {
        using var builder = graph.AddRasterRenderPass<OpaquePass>(Sampler.name, out var pass, Sampler);
        
        RendererListDesc desc = new RendererListDesc(ShaderTags, results, camera);
        desc.sortingCriteria = SortingCriteria.CommonOpaque;
        desc.renderQueueRange = RenderQueueRange.opaque;
        desc.rendererConfiguration = PerObjectData.None;
        
        pass.list = graph.CreateRendererList(desc);

        builder.UseRendererList(pass.list);
        builder.SetRenderAttachment(textures.color, 0, AccessFlags.Write);
        builder.SetRenderAttachmentDepth(textures.depth, AccessFlags.Write);
        builder.SetRenderFunc<OpaquePass>((pass, context) =>
        {
            var cmd = context.cmd;
            
            // Izbrisi ovo kasnije
            cmd.ClearRenderTarget(true, true, camera.backgroundColor);
            cmd.DrawRendererList(pass.list);
            // context.renderContext.ExecuteAndClearCommandBuffer(cmd);
        });
    }
}
