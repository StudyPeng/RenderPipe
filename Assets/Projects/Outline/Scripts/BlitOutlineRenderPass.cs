using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Projects.Outline.Scripts
{
    public class BlitOutlineRenderPass : ScriptableRenderPass
    {
        private class BlitOutlinePassData
        {
            public TextureHandle Source;
            public TextureHandle Copy;
            public TextureHandle TemporaryTex;
            public Material Mat;
        }

        private readonly BlitOutlineRenderFeature.BlitOutlineSettings m_Settings;
        private static readonly int ms_DepthTexProperty = Shader.PropertyToID("_SecTex");
        private static readonly int ms_ColorTexProperty = Shader.PropertyToID("_ThirdTex");
        private Material m_OutlineMat;

        public BlitOutlineRenderPass(BlitOutlineRenderFeature.BlitOutlineSettings settings)
        {
            m_Settings = settings;
            renderPassEvent = m_Settings.PassEvent + 1;
        }

        public void Setup(Material temporaryMat)
        {
            m_OutlineMat = temporaryMat;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string name = "PP Outline ";

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            RenderTextureDescriptor textureDesc = cameraData.cameraTargetDescriptor;
            textureDesc.depthBufferBits = 0;
            TextureHandle colorCopyTex = UniversalRenderer.CreateRenderGraphTexture(renderGraph, 
                textureDesc, "_ColorCopyTex", true, FilterMode.Bilinear, TextureWrapMode.Clamp);

            using (var copyBuilder =
                   renderGraph.AddRasterRenderPass<BlitOutlinePassData>(name + " Copy Pass", out var passData))
            {
                passData.Source = resourceData.cameraColor;
                passData.Copy = colorCopyTex;
                copyBuilder.UseTexture(passData.Source);
                copyBuilder.SetRenderAttachment(passData.Copy, 0);
                copyBuilder.SetRenderFunc((BlitOutlinePassData copyData, RasterGraphContext context) =>
                {
                    Vector4 scaleBias = new Vector4(1, 1, 0, 0);
                    Blitter.BlitTexture(context.cmd, copyData.Source, scaleBias, 0f, false);
                });
            }

            // TODO: First pass is working now.
            // Some reason make second pass is not working.
            // Maybe colorCopyTex framebuffer is null? or other.
            using (var executeBuilder =
                   renderGraph.AddRasterRenderPass<BlitOutlinePassData>(name + " Execute Pass", out var passData))
            {
                passData.Mat = m_OutlineMat;
                executeBuilder.SetInputAttachment(colorCopyTex, 0);
                executeBuilder.SetRenderAttachment(resourceData.cameraColor, 0);
                // executeBuilder.AllowPassCulling(false);
                executeBuilder.SetRenderFunc((BlitOutlinePassData executeData, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, Vector2.one, executeData.Mat, 0);
                });
            }
            
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose()
        {
        }
    }
}