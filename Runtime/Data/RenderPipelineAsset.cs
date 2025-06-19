using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

public enum Tonemap
{
    None,
    Reinhard,
    ReinhardExtended,
    ACES
}

[CreateAssetMenu(menuName = "Rendering/Render Pipeline")]
public class FERRenderPipelineAsset : RenderPipelineAsset<FERRenderPipeline>, IRenderGraphEnabledRenderPipeline
{
    /// <summary>
    /// ...
    /// </summary>
    
    public Cubemap environment;

    public Tonemap tonemap = Tonemap.None;
    
    
    protected override RenderPipeline CreatePipeline()
    {
        return new FERRenderPipeline(this);
    }

    // TODO: raahhh NEW KEYWORD?!?!? 
    public bool isImmediateModeSupported => false;
}
