using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiteRP.Runtime
{
    public class LiteRenderPipeline : RenderPipeline
    {
        private RenderGraph m_RenderGraph;
        private LiteRenderGraphRecorder m_GraphRecorder;
        private ContextContainer m_ContextContainer;

        public LiteRenderPipeline()
        {
            InitRenderGraph();
        }

        protected override void Dispose(bool disposing)
        {
            CleanupRenderGraph();
            base.Dispose(disposing);
        }

        private void InitRenderGraph()
        {
            m_RenderGraph = new RenderGraph("LiteRPRenderGraph");
            m_GraphRecorder = new LiteRenderGraphRecorder();
            m_ContextContainer = new ContextContainer();
        }

        private void CleanupRenderGraph()
        {
            m_ContextContainer?.Dispose();
            m_RenderGraph.Cleanup();
            m_RenderGraph = null;
            m_GraphRecorder = null;
            m_ContextContainer = null;
        }

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
            m_RenderGraph.EndFrame();
            EndContextRendering(context, cameras);
        }

        private void RenderCamera(ScriptableRenderContext context, Camera camera)
        {
            BeginCameraRendering(context, camera);
            if (!PrepareFrameData(context, camera)) return;
            
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
            RecordAndExecute(context, camera, cmd);

            // Commit command buffer
            context.ExecuteCommandBuffer(cmd);
            // Release command buffer
            cmd.Clear();
            CommandBufferPool.Release(cmd);
            // Submit CONTEXT
            context.Submit();

            EndCameraRendering(context, camera);
        }

        private bool PrepareFrameData(ScriptableRenderContext context, Camera camera)
        {
            return true;
        }

        private void RecordAndExecute(ScriptableRenderContext context, Camera renderCamera, CommandBuffer cmd)
        {
            RenderGraphParameters renderGraphParameters = new RenderGraphParameters
            {
                executionId = renderCamera.GetEntityId(),
                commandBuffer = cmd,
                scriptableRenderContext = context,
                currentFrameIndex = Time.frameCount
            };
            m_RenderGraph.BeginRecording(renderGraphParameters);
            m_GraphRecorder.RecordRenderGraph(m_RenderGraph, m_ContextContainer);
            m_RenderGraph.EndRecordingAndExecute();
        }
    }
}