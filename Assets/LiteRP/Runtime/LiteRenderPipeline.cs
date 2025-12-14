using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiteRP.Runtime
{
    public class LiteRenderPipeline : RenderPipeline
    {
        /// <summary>
        /// cameras contains multiple type.
        /// </summary>
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
        {
            BeginContextRendering(context, cameras);
            for (int i = 0; i < cameras.Count; ++i)
            {
                Camera camera = cameras[i];
                RenderCamera(context, camera);    
            }
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);
            // Get culling params & results.
            if (!camera.TryGetCullingParameters(out var cullingParams))
            {
                Debug.LogWarning($"Camera may not found or get culling param failed.");
                return;
            }
            CullingResults cullResults = context.Cull(ref cullingParams);
            // Get command buffer.
            CommandBuffer cmd = CommandBufferPool.Get(camera.name);
            // Set camera property.
            context.SetupCameraProperties(camera);
            // Clear render target
            cmd.ClearRenderTarget(true, true, CoreUtils.ConvertSRGBToActiveColorSpace(camera.backgroundColor));
            // IF Render skybox
            bool clearSkybox = camera.clearFlags == CameraClearFlags.Skybox;
            bool clearDepth =  camera.clearFlags != CameraClearFlags.Nothing;
            bool clearColor =  camera.clearFlags == CameraClearFlags.Color;
            if (clearSkybox)
            {
                RendererList skyboxList = context.CreateSkyboxRendererList(camera);
                cmd.DrawRendererList(skyboxList);
            }

            // Rendering Settings
            SortingSettings sortingSettings = new SortingSettings(camera);
            DrawingSettings drawingSettings = new DrawingSettings(new ShaderTagId("SRPDefaultUnlit"), sortingSettings);
            FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            RendererListParams rendererListParams = new RendererListParams(cullResults, drawingSettings, filteringSettings);
            RendererList renderList = context.CreateRendererList(ref rendererListParams);
            // Draw rendering list
            cmd.DrawRendererList(renderList);
            // Commit command buffer
            context.ExecuteCommandBuffer(cmd);
            // Release command buffer
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            // Submit CONTEXT
            context.Submit();
            
            EndCameraRendering(context, camera);
        }
    }
}