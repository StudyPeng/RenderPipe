using Projects.Outline.Scripts;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlitOutlineRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlitOutlineSettings
    {
        public Material OutlineMat;
        public Material DepthMat;
        public Material UFloatMat;
        public RenderPassEvent PassEvent = RenderPassEvent.AfterRenderingTransparents;
        [ColorUsage(true, true)] public Color Color;
    }
    
    public BlitOutlineSettings Settings;
    private BlitOutlineRenderPass m_OutlinePass;

    public override void Create()
    {
        name = "Outline";
        m_OutlinePass = new BlitOutlineRenderPass(Settings);
        m_OutlinePass.renderPassEvent = Settings.PassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (Settings.OutlineMat != null)
        {
            renderer.EnqueuePass(m_OutlinePass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        m_OutlinePass.Dispose();
    }
}
