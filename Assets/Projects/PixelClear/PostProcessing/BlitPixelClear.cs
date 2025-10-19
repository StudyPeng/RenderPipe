using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class BlitPixelClear : ScriptableRendererFeature
{
    public RenderPassEvent eventPass = RenderPassEvent.AfterRenderingPrePasses;
    public bool enable = true;
    private BlitPixelClearPass _blitPixelClearPass;

    /// <inheritdoc/>
    public override void Create()
    {
        _blitPixelClearPass = new BlitPixelClearPass();

        // Configures where the render pass should be injected.
        _blitPixelClearPass.renderPassEvent = eventPass;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (enable)
        {
            renderer.EnqueuePass(_blitPixelClearPass);
        }
    }
}