using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.RenderGraphModule;

public class LightPass
{
    private static readonly ProfilingSampler Sampler = new("Light Pass");

    public static void Record(RenderGraph graph, CullingResults results)
    {
        using var builder = graph.AddRenderPass<LightPass>(Sampler.name, out var pass, Sampler);
        
        builder.AllowPassCulling(false);
        builder.SetRenderFunc<LightPass>((pass, context) =>
        {
            var cmd = context.cmd;
            
            Light light = RenderSettings.sun;
            cmd.SetGlobalVector("_LightDirection", -light.transform.forward);
            cmd.SetGlobalVector("_LightColor", light.color * light.intensity);
            
            context.renderContext.ExecuteAndClearCommandBuffer(cmd);
        });
    }
}