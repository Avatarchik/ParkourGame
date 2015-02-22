using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Tonemap : MonoBehaviour 
{
	public Shader shader;
	private Material filmic;
	
	public float ShoulderStrength = 0.15f;
	public float LinearStrength = 0.55f;
	public float LinearAngle = 0.1f;
	public float ToeStrength = 0.2f;
	public float ToeNumerator = 0.02f;
	public float ToeDenominator = 0.7f;
	public float Weight = 10.2f;
	public float Vignette = 0.5f;
	public float Sharpening = 1.1f;
	public float Exposure = 1.0f;
	public float Vibrance = 1.0f;
	public float Technicolor = 0.8f;
	public float Coc = 0.1f;
	public float Axial = 0.25f;
	public float Transverse = 0.25f;
	public float LetterBox = 0.85f;
	public Color Defog = Color.black;
	
	Material material
	{
		get
		{
			if(filmic == null)
			{
				filmic = new Material(shader);
				filmic.hideFlags = HideFlags.HideAndDontSave;	
			}
			return filmic;
		}
	}
	
	public void Start ()
	{
		CheckSystemRequirements();
		AutoApplyShader("Custom/Filmic Tonemap");
	}
	
	public void SetConstants ()
	{
		material.SetFloat("_A", ShoulderStrength);
		material.SetFloat("_B", LinearStrength);
		material.SetFloat("_C", LinearAngle);
		material.SetFloat("_D", ToeStrength);
		material.SetFloat("_E", ToeNumerator);
		material.SetFloat("_F", ToeDenominator);
		material.SetFloat("_W", Weight);
		material.SetFloat("_V", Vignette);
		material.SetFloat("_S", Sharpening);
		material.SetFloat("_X", Exposure);
		material.SetFloat("_I", Vibrance);
		material.SetColor("_DF", Defog);
		material.SetFloat("_T", Technicolor);
		material.SetFloat("_CT", Transverse);
		material.SetFloat("_CA", Axial);
		material.SetFloat("_O", Coc);
		material.SetFloat("_U", LetterBox);
		material.SetFloat("_Z", (1.0f - LetterBox) );
	}
	
	public void OnRenderImage (RenderTexture In, RenderTexture Output)
	{
		if(shader)
		{
			//GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;

			SetConstants(); 
			Graphics.Blit(In, Output, material);
		}
	}
	
	public void CheckSystemRequirements ()
	{
		if(!SystemInfo.supportsImageEffects)
		{
			enabled = false;
			return;
		}
	}
	
	public void AutoApplyShader (string a)
	{
		shader = Shader.Find(a);
	}
	
	public void OnDisable ()
	{
		if(material)
			DestroyImmediate(material);
	}
	
}
