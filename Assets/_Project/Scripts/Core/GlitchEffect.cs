using UnityEngine;

[RequireComponent(typeof(Camera))]
public class GlitchEffect : MonoBehaviour
{
    [Header("Glitch Settings")]
    public Shader glitchShader;
    private Material glitchMaterial;

    [Range(0f, 0.1f)]
    public float intensity = 0.02f;
    
    public Vector2 blockSize = new Vector2(10f, 10f);
    
    [Range(0f, 10f)]
    public float speed = 1f;
    
    [Range(0f, 0.05f)]
    public float colorShiftIntensity = 0.01f;

    [Header("Debug")]
    public bool enableEffect = true;

    private void OnEnable()
    {
        CreateMaterial();
    }

    private void CreateMaterial()
    {
        if (glitchShader == null)
        {
            glitchShader = Shader.Find("Custom/GlitchEffect");
        }
        
        if (glitchShader != null && glitchShader.isSupported)
        {
            if (glitchMaterial == null || glitchMaterial.shader != glitchShader)
            {
                if (glitchMaterial != null)
                {
                    DestroyImmediate(glitchMaterial);
                }
                glitchMaterial = new Material(glitchShader);
                glitchMaterial.hideFlags = HideFlags.HideAndDontSave;
            }
            Debug.Log("Glitch effect material created successfully.");
        }
        else
        {
            if (glitchShader == null)
            {
                Debug.LogError("Glitch shader not found! Please assign it manually in Inspector.");
            }
            else
            {
                Debug.LogError("Glitch shader is not supported on this platform!");
            }
        }
    }

    private void Start()
    {
        CreateMaterial();
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!enableEffect || glitchMaterial == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        glitchMaterial.SetFloat("_Intensity", intensity);
        glitchMaterial.SetVector("_BlockSize", blockSize);
        glitchMaterial.SetFloat("_Speed", speed);
        glitchMaterial.SetFloat("_ColorShiftIntensity", colorShiftIntensity);
        
        Graphics.Blit(source, destination, glitchMaterial);
    }

    private void OnDestroy()
    {
        if (glitchMaterial != null)
        {
            DestroyImmediate(glitchMaterial);
        }
    }
}

