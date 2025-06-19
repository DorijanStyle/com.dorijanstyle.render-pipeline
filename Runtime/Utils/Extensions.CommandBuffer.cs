using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class Extensions
{
    private static BuiltinShaders shaders = new BuiltinShaders();
    
    public static void DrawUtils(this CommandBuffer cmd, UtilsPass pass)
    {
        cmd.DrawProcedural(Matrix4x4.identity, shaders.utilsMaterial, (int)pass, MeshTopology.Triangles, 3, 1);
    }
    
    public static void DrawUtils(this CommandBuffer cmd, UtilsPass pass, MaterialPropertyBlock properties)
    {
        cmd.DrawProcedural(Matrix4x4.identity, shaders.utilsMaterial, (int)pass, MeshTopology.Triangles, 3, 1, properties);
    }
    
    public static void DrawUtils(this RasterCommandBuffer cmd, UtilsPass pass)
    {
        cmd.DrawProcedural(Matrix4x4.identity, shaders.utilsMaterial, (int)pass, MeshTopology.Triangles, 3, 1);
    }
    
    public static void DrawUtils(this RasterCommandBuffer cmd, UtilsPass pass, MaterialPropertyBlock properties)
    {
        cmd.DrawProcedural(Matrix4x4.identity, shaders.utilsMaterial, (int)pass, MeshTopology.Triangles, 3, 1, properties);
    }

    public static ComputeShader GetUtilsCompute(this CommandBuffer cmd)
    {
        return shaders.utilsCompute;
    }

    public static int GetUtilsKernel(this CommandBuffer cmd, UtilsKernel kernel)
    {
        return shaders.kernels[kernel];
    }
}