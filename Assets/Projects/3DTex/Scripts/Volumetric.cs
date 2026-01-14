using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class Volumetric : MonoBehaviour
{
    private RenderTexture m_VolumetricRT;

    void Start()
    {
        RenderTextureDescriptor desc = CreateDescriptor();
        m_VolumetricRT = new RenderTexture(desc);
        m_VolumetricRT.Create();
    }

    void Update()
    {
    }

    private RenderTextureDescriptor CreateDescriptor()
    {
        RenderTextureDescriptor desc = new RenderTextureDescriptor();
        desc.width = 128;
        desc.height = 128;
        desc.graphicsFormat = GraphicsFormat.R32_SFloat;
        desc.depthStencilFormat = GraphicsFormat.None;
        desc.depthBufferBits = 0;
        desc.volumeDepth = 64;
        desc.dimension = TextureDimension.Tex3D;
        desc.enableRandomWrite = true;
        desc.msaaSamples = 1;
        return desc;
    }
}