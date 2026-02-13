using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseBaker : MonoBehaviour
{
    [Header("Compute Shader")]
    [SerializeField] ComputeShader m_shader;
    [Header("Compute Shader Parameters")]
    [SerializeField] [Range(16, 1024)] int m_textureLength;
    [SerializeField] [Range(2, 512)] int m_cellLength;
    [SerializeField] float m_shapeNoiseFrequency;
    [SerializeField] float m_lowDetailNoiseFrequency;
    [SerializeField] float m_midDetailNoiseFrequency;
    [SerializeField] float m_highDetailNoiseFrequency;
    [SerializeField] [Range(0.0f, 1.0f)] float m_perlinWeight = 0.5f;
    [SerializeField] Vector3 m_perlinPeriod = new Vector3(4, 4, 4);

    [Header("Preview")]
    [SerializeField] Renderer m_preview;

    [HideInInspector][SerializeField] RenderTexture m_noise;
    int m_kernelHandle;

    void Start()
    {
        Bake();
    }

    void Update()
    {
        
    }

    void Bake()
    {
        m_noise = new RenderTexture(m_textureLength, m_textureLength, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        m_noise.enableRandomWrite = true;
        m_noise.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        m_noise.volumeDepth = m_textureLength;
        m_noise.Create();
        RenderTexture.active = m_noise;

        int m_kernelHandle = m_shader.FindKernel("ComputeNoise3D");

        m_shader.SetVector(
               "scale",
               new Vector4(
                   m_shapeNoiseFrequency,
                   m_lowDetailNoiseFrequency,
                   m_midDetailNoiseFrequency,
                   m_highDetailNoiseFrequency
                   )
               );
        m_shader.SetFloat("perlinWeight", m_perlinWeight);
        m_shader.SetVector("period", m_perlinPeriod);
        m_shader.SetInt("cellLength", m_cellLength);
        m_shader.SetTexture(m_kernelHandle, "noise", m_noise);
        //m_shader.Dispatch(m_kernelHandle, m_noise.width / 8, m_noise.height / 8, m_noise.volumeDepth / 8);

        StartCoroutine(Dispatch());
    }

    IEnumerator Dispatch()
    {
        for (int i = 0; i < 100; i++)
        {
            yield return null;
        }

        m_shader.Dispatch(m_kernelHandle, m_noise.width / 8, m_noise.height / 8, m_noise.volumeDepth / 8);
    }

    Material PreviewMaterial
    {
        get
        {
            if (m_preview == null)
            {
                return null;
            }

            Material material = m_preview.sharedMaterial;
            if (material.shader.name == "Shader Graphs/UnlitTexture3D")
            {
                return material;
            }

            return null;
        }
    }

#if UNITY_EDITOR

    void OnValidate()
    {
        int n = Mathf.Min( 10, Mathf.Max(4, (int)Mathf.Round(Mathf.Log10(m_textureLength) / Mathf.Log10(2)) ) );
        m_textureLength = (int)(Mathf.Pow(2, n));

        int m = Mathf.Min(9, Mathf.Max(1, (int)Mathf.Round(Mathf.Log10(m_cellLength) / Mathf.Log10(2))));
        if(m > n - 1)
        {
            m = n - 1;
        }
        m_cellLength = (int)(Mathf.Pow(2, m));

        Material mat = PreviewMaterial;
        if (mat)
        {
            mat.SetFloat("TextureLength", m_textureLength);
            //Bake();
            //mat.SetTexture("_Texture3D", m_noise);
        }
    }

#endif

}
