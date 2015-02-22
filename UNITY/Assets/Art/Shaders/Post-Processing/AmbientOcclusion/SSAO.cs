using UnityEngine;
using PostFXEngine;
using System.Collections;

[ExecuteInEditMode]
public class SSAO : MonoBehaviour 
{
    public Material material;
    public Shader ssaoShader;
    public Camera cam;
    public Texture2D JitterTex;

	// Use this for initialization
	private void Awake() 
    {
        material = Shading.GetMaterial(material, ssaoShader);
        cam = GetComponent<Camera>();
	}

    protected void SetConstants()
    {
        cam.depthTextureMode |= DepthTextureMode.DepthNormals;
        material.SetMatrix("_InverseProj", cam.projectionMatrix.inverse);
        material.SetTexture("_Jitter", JitterTex);
    }

    protected void CalculateAO(RenderTexture input, RenderTexture output)
    {
        SetConstants();
        Graphics.Blit(input, output, material);
    }

    private void OnRenderImage(RenderTexture Input, RenderTexture Output) 
    {
        CalculateAO(Input, Output);
	}

    private void OnDisable()
    {
      //  DestroyImmediate(material);
    }
}
