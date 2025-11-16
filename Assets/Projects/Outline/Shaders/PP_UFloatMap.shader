Shader "Common/PP_UFloat"
{
    Properties
    {
        _ObjectsTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Name "PP_UFloat"

            Tags
            {
                "RenderType"="Opaque"
                "Queue"="Opaque"
                "RenderPipeline"="UniversalPipeline"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            TEXTURE2D(_ObjectsTex);
            SAMPLER(sampler_ObjectsTex);
            float4 _ObjectsTex_ST;

            Varyings vert(Attributes v)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);
                o.positionCS = GetFullScreenTriangleVertexPosition(v.vertexID);
                o.texcoord = GetFullScreenTriangleTexCoord(v.vertexID);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 col = SAMPLE_TEXTURE2D(_ObjectsTex, sampler_ObjectsTex, i.texcoord);
                return 1 - step(col, 0);
            }
            ENDHLSL
        }
    }
}