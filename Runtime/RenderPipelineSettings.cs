using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[SupportedOnRenderPipeline(typeof(StyleRenderPipelineAsset))]
public class RenderPipelineSettings : RenderPipelineGlobalSettings<RenderPipelineSettings, StyleRenderPipeline>
{
    [SerializeField] private RenderPipelineGraphicsSettingsContainer m_Settings = new();
    protected override List<IRenderPipelineGraphicsSettings> settingsList => m_Settings.settingsList;

    #if UNITY_EDITOR
    internal static RenderPipelineSettings Ensure(bool canCreateNewAsset = true)
    {
        var currentInstance = GraphicsSettings.GetSettingsForRenderPipeline<StyleRenderPipeline>() as RenderPipelineSettings;
        var defaultName = "StylePipelineGlobalSettings";
        var defaultPath = $"Assets/{defaultName}.asset";

        if (RenderPipelineGlobalSettingsUtils.TryEnsure<RenderPipelineSettings, StyleRenderPipeline>(ref currentInstance, defaultPath, canCreateNewAsset))
        {
            return currentInstance;
        }
        
        
        return null;
    }


    public override void Initialize(RenderPipelineGlobalSettings source = null) { }
    #endif
}
