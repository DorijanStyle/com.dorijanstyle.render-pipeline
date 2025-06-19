using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

public class PostProcessPass
{
    private static readonly ProfilingSampler Sampler = new("Post Process Pass");

    private TextureHandle color;
    private TextureHandle depth;
    
    public static void Record(RenderGraph graph, FrameTextures textures, Tonemap tonemap = Tonemap.None, bool flip = false)
    {
        using var builder = graph.AddRasterRenderPass<PostProcessPass>(Sampler.name, out var pass, Sampler);

        pass.color = textures.color;
        pass.depth = textures.depth;
        
        builder.UseTexture(pass.color);
        builder.UseTexture(pass.depth);
        
        builder.AllowPassCulling(false);
        builder.SetRenderAttachment(textures.backBuffer, 0, AccessFlags.Write);
        builder.SetRenderFunc<PostProcessPass>((pass, context) =>
        {
            var cmd = context.cmd;
            
            MaterialPropertyBlock properties = new();
            properties.SetTexture("_SourceColorTexture", pass.color);
            properties.SetTexture("_SourceDepthTexture", pass.depth);
            properties.SetInt("_TonemapMode", (int)tonemap);
            
            cmd.DrawUtils(UtilsPass.DRAW_FINAL, properties);
        });
    }
}