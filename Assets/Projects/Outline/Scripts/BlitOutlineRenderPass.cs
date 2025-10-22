using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace Projects.Outline.Scripts
{
    public class BlitOutlineRenderPass : ScriptableRenderPass
    {
        private class PassData
        {
        }

        private BlitOutlineRenderFeature.BlitOutlineSettings _settings;

        public BlitOutlineRenderPass(BlitOutlineRenderFeature.BlitOutlineSettings settings)
        {
            _settings = settings;
        }
        
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
            context.cmd.ClearRenderTarget(RTClearFlags.None, Color.black, 1, 1);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Blit Outline Pass";
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
                UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                var colorDesc = resourceData.activeColorTexture;
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }
}