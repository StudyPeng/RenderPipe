Shader "Unlit/PP_BlitOutline"
{
    Properties
    {
        _QuadDepth ("Depth Tex", 2D) = "black" {}
        _CopyColorTex ("Color Tex", 2D) = "black" {}
        _SampleScale ("Sample Range", Float) = 0.01
        _Threshold ("Threshold", Float) = 0.01
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "RenderType"="Opaque"
                "Queue"="Geometry"
                "RenderPipeline"="UniversalPipeline"
                "LightMode"="UniversalForwardOnly"
            }

            Blend One Zero
            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define U float2( 0,  1)
            #define D float2( 0, -1)
            #define L float2(-1,  0)
            #define R float2( 1,  0)

            struct Attributes
            {
                uint vertexID : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 texcoord : TEXCOORD0;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_QuadDepth);
            SAMPLER(sampler_QuadDepth);
            float4 _QuadDepth_TexelSize;
            TEXTURE2D(_CopyColorTex);
            SAMPLER(sampler_CopyColorTex);
            float4 _OutlineColor;
            float _SampleScale;
            float _Threshold;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
                float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);
                output.positionCS = pos;
                output.texcoord = uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 c = SAMPLE_TEXTURE2D(_CopyColorTex, sampler_CopyColorTex, input.texcoord);
                float2 dx = float2(_QuadDepth_TexelSize.x * _SampleScale, 0);
                float2 dy = float2(0, _QuadDepth_TexelSize.y * _SampleScale);
                float px = SAMPLE_TEXTURE2D(_QuadDepth, sampler_QuadDepth, input.texcoord + dx);
                float nx = SAMPLE_TEXTURE2D(_QuadDepth, sampler_QuadDepth, input.texcoord - dx);
                float py = SAMPLE_TEXTURE2D(_QuadDepth, sampler_QuadDepth, input.texcoord + dy);
                float ny = SAMPLE_TEXTURE2D(_QuadDepth, sampler_QuadDepth, input.texcoord - dy);
                float edge = abs(px - nx) + abs(py - ny);
                if (edge > _Threshold) return _OutlineColor;
                return c;
            }
            ENDHLSL
        }
    }
}