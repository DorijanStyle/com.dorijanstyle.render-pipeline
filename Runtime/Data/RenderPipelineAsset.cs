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

public enum DEBUG
{
    None,
    DIFFUSE,
    SPECULAR,
    DIRECT,
    IBL_IRRADIANCE,
    IBL_RADIANCE,
    INDIRECT
}

[CreateAssetMenu(menuName = "Rendering/Render Pipeline")]
public class StyleRenderPipelineAsset : RenderPipelineAsset<StyleRenderPipeline>, IRenderGraphEnabledRenderPipeline
{
    public Cubemap environment;
    
    public Tonemap tonemap = Tonemap.None;
    public DEBUG debug = DEBUG.None;
    
    
    protected override RenderPipeline CreatePipeline()
    {
        return new StyleRenderPipeline(this);
    }

    protected override void EnsureGlobalSettings()
    {
        base.EnsureGlobalSettings();
        
        #if UNITY_EDITOR
        RenderPipelineSettings.Ensure();
        #endif
    }

    public bool isImmediateModeSupported => false;
}
