using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class BlitPixelClearPass : ScriptableRenderPass
{
    // This class stores the data needed by the RenderGraph pass.
    // It is passed as a parameter to the delegate function that executes the RenderGraph pass.
    private class PassData
    {
        public Color _cleanColor;
    }

    // This static method is passed as the RenderFunc delegate to the RenderGraph render pass.
    // It is used to execute draw commands.
    private static void ExecutePass(PassData data, RasterGraphContext context)
    {
        context.cmd.ClearRenderTarget(RTClearFlags.All, Color.white, 0, 0);
    }

    // RecordRenderGraph is where the RenderGraph handle can be accessed, through which render passes can be added to the graph.
    // FrameData is a context container through which URP resources can be accessed and managed.
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        const string _passName = "Blit Pixel Clear Pass";

        // This adds a raster render pass to the graph, specifying the name and the data type that will be passed to the ExecutePass function.
        using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
        {
            // Use this scope to set the required inputs and outputs of the pass and to
            // setup the passData with the required properties needed at pass execution time.

            // Make use of frameData to access resources and camera data through the dedicated containers.
            // Eg:
            // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            // Setup pass inputs and outputs through the builder interface.
            // Eg:
            // builder.UseTexture(sourceTexture);
            // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

            // This sets the render target of the pass to the active color texture. Change it to your own render target as needed.
            passData._cleanColor = Color.clear;
            builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.Write);

            // Assigns the ExecutePass function to the render pass delegate. This will be called by the render graph when executing the pass.
            builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
        }
    }

    // NOTE: This method is part of the compatibility rendering path, please use the Render Graph API above instead.
    // Cleanup any allocated resources that were created during the execution of this render pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
    }
}