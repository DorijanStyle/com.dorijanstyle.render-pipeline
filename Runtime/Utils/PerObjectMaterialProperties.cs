using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{ 
    static int baseColorID = Shader.PropertyToID("_BaseColor");
    static int roughnessID = Shader.PropertyToID("_Roughness");
    static int metallicID = Shader.PropertyToID("_Metallic");
    
    static MaterialPropertyBlock materialBlock;
    
    [SerializeField] Color baseColor = Color.white;
    [SerializeField, Range(0, 1)] float roughness = 1.0f;
    [SerializeField, Range(0, 1)] float metallic = 0.0f;

    void OnValidate() // want be called in builds
    {
        if(materialBlock == null)
            materialBlock = new MaterialPropertyBlock();
        
        materialBlock.SetColor(baseColorID, baseColor);
        materialBlock.SetFloat(roughnessID, roughness);
        materialBlock.SetFloat(metallicID, metallic);
        
        GetComponent<Renderer>().SetPropertyBlock(materialBlock);
    }

    void Awake()
    {
        OnValidate();
    }
}