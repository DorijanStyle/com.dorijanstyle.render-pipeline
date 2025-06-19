using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

public class GizmoPass
{
    private static readonly ProfilingSampler Sampler = new("Gizmo Pass");

    private RendererListHandle list;
    
    public static void Record(RenderGraph graph, Camera camera, FrameTextures textures)
    {
        
        using var builder = graph.AddRasterRenderPass<GizmoPass>(Sampler.name, out var pass, Sampler);

        pass.list = graph.CreateGizmoRendererList(camera, GizmoSubset.PostImageEffects);
        
        builder.UseRendererList(pass.list);
        builder.AllowPassCulling(true);
        builder.SetRenderAttachment(textures.color, 0, AccessFlags.Write);
        builder.SetRenderAttachmentDepth(textures.depth, AccessFlags.ReadWrite);
        builder.SetRenderFunc<GizmoPass>((pass, context) =>
        {
            var cmd = context.cmd;
            
            cmd.DrawRendererList(pass.list);
        });
    }
}