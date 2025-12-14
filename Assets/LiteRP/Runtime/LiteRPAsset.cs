using LiteRP.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "RenderPipelineAsset", menuName = "LiteRP/Pipeline Asset")]
public class LiteRPAsset : RenderPipelineAsset<LiteRenderPipeline>
{
    protected override RenderPipeline CreatePipeline()
    {
        return new LiteRenderPipeline();
    }
}
