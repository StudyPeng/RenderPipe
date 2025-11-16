using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Projects.Outline.Scripts
{
    public class BlitOutlineRenderPass : ScriptableRenderPass
    {
        private class BlitOutlinePassData
        {
            public TextureHandle DepthAttach;
            public TextureHandle ColorAttach;
            public TextureHandle Dest;
        }

        private readonly BlitOutlineRenderFeature.BlitOutlineSettings m_Settings;
        private static readonly int ms_DepthTexProperty = Shader.PropertyToID("_QuadDepth");
        private static readonly int ms_CopyColorTex = Shader.PropertyToID("_CopyColorTex");
        private static readonly int ms_ObjectsTex = Shader.PropertyToID("_ObjectsTex");
        private readonly Material m_OutlineMat;
        private readonly Material m_DepthOnlyMat;
        private readonly Material m_UFloatMat;

        private readonly List<ShaderTagId> m_ShaderTagIds = new()
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("Transparent"),
        };

        public BlitOutlineRenderPass(BlitOutlineRenderFeature.BlitOutlineSettings settings)
        {
            m_Settings = settings;
            m_OutlineMat = m_Settings.OutlineMat;
            m_DepthOnlyMat = m_Settings.DepthMat;
            m_UFloatMat = m_Settings.UFloatMat;
            renderPassEvent = m_Settings.PassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string name = "PP Outline";
            if (m_OutlineMat == null || m_DepthOnlyMat == null || m_UFloatMat == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalLightData lightData = frameData.Get<UniversalLightData>();
            // Create depth texture
            RenderTextureDescriptor depthDesc = CreateDrawDescriptor(cameraData);
            TextureHandle depthTex = UniversalRenderer.CreateRenderGraphTexture(renderGraph, depthDesc,
                "_PP_OutlineDepthTexture", false, FilterMode.Bilinear);
            // Create color texture
            RenderTextureDescriptor colorDesc = CreateColorDescriptor(cameraData);
            TextureHandle colorTex = UniversalRenderer.CreateRenderGraphTexture(renderGraph, colorDesc,
                "_PP_OutlineColorTexture", false, FilterMode.Bilinear);

            using (var renderBuilder = renderGraph.AddRasterRenderPass<BlitOutlinePassData>($"{name} Draw Objects", out var passData))
            {
                SortingCriteria sortingCriteria = SortingCriteria.CommonOpaque;
                uint renderLayerMask = 1 << 1;
                FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.all, -1, renderLayerMask);
                DrawingSettings drawSettings = CreateDrawingSettings(m_ShaderTagIds, renderingData, cameraData,
                    lightData, sortingCriteria);
                RendererListParams rendererListParams = new RendererListParams(renderingData.cullResults, drawSettings, filteringSettings);
                RendererListHandle rendererList = renderGraph.CreateRendererList(rendererListParams);
                renderBuilder.AllowPassCulling(false);
                renderBuilder.SetRenderAttachment(colorTex, 0);
                renderBuilder.UseRendererList(rendererList);
                renderBuilder.SetRenderFunc((BlitOutlinePassData data, RasterGraphContext ctx) =>
                {
                    ctx.cmd.DrawRendererList(rendererList);
                });
            }

            using (var colorBuilder = renderGraph.AddRasterRenderPass<BlitOutlinePassData>($"{name} Color Build", out var passData))
            {
                passData.ColorAttach = colorTex;
                passData.Dest = depthTex;
                colorBuilder.UseTexture(passData.ColorAttach);
                colorBuilder.SetRenderAttachment(passData.Dest, 0);
                colorBuilder.AllowPassCulling(false);
                colorBuilder.SetRenderFunc((BlitOutlinePassData data, RasterGraphContext ctx) =>
                {
                    m_UFloatMat.SetTexture(ms_ObjectsTex, passData.ColorAttach);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, m_UFloatMat, 0, MeshTopology.Triangles, 3);
                });
            }

            using (var copyBuilder = renderGraph.AddRasterRenderPass<BlitOutlinePassData>($"{name} Copy Color", out var passData))
            {
                passData.ColorAttach = resourceData.activeColorTexture;
                copyBuilder.UseTexture(resourceData.activeColorTexture);
                copyBuilder.SetRenderAttachment(colorTex, 0);
                copyBuilder.SetRenderFunc((BlitOutlinePassData data, RasterGraphContext ctx) =>
                {
                    Blitter.BlitTexture(ctx.cmd, data.ColorAttach, Vector2.one, 0, false);
                });
            }

            using (var executeBuilder = renderGraph.AddRasterRenderPass<BlitOutlinePassData>($"{name} Blit", out var passData))
            {
                passData.ColorAttach = colorTex;
                passData.DepthAttach = depthTex;
                passData.Dest = resourceData.activeColorTexture;
                executeBuilder.UseTexture(depthTex);
                executeBuilder.UseTexture(colorTex);
                executeBuilder.SetRenderAttachment(passData.Dest, 0);
                executeBuilder.AllowPassCulling(false);
                executeBuilder.SetRenderFunc((BlitOutlinePassData data, RasterGraphContext ctx) =>
                {
                    m_OutlineMat.SetTexture(ms_CopyColorTex, data.ColorAttach);
                    m_OutlineMat.SetTexture(ms_DepthTexProperty, data.DepthAttach);
                    ctx.cmd.DrawProcedural(Matrix4x4.identity, m_OutlineMat, 0, MeshTopology.Triangles, 3);
                });
            }
        }

        public RenderTextureDescriptor CreateColorDescriptor(UniversalCameraData cameraData)
        {
            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            desc.msaaSamples = 1;
            desc.stencilFormat = GraphicsFormat.None;
            return desc;
        }

        public RenderTextureDescriptor CreateDrawDescriptor(UniversalCameraData cameraData)
        {
            RenderTextureDescriptor desc = new RenderTextureDescriptor
            {
                width = cameraData.cameraTargetDescriptor.width,
                height = cameraData.cameraTargetDescriptor.height,
                graphicsFormat = GraphicsFormat.R32_SFloat,
                depthStencilFormat = GraphicsFormat.None,
                depthBufferBits = 0,
                msaaSamples = 1,
                autoGenerateMips = false,
                sRGB = false,
                dimension = TextureDimension.Tex2D,
                volumeDepth = 1,
                enableRandomWrite = false,
                memoryless = RenderTextureMemoryless.None
            };
            return desc;
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }

        public void Dispose()
        {
        }
    }
}