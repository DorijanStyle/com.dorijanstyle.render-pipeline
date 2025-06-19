using UnityEngine.Rendering;

public static partial class Extensions
{
    public static void ExecuteAndClearCommandBuffer(this ScriptableRenderContext context, CommandBuffer cmd)
    {
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
    }

    public static void ExecuteAndReleaseCommandBuffer(this ScriptableRenderContext context, CommandBuffer cmd)
    {
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}