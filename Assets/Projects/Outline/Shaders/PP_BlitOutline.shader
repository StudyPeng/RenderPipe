Shader "Unlit/PP_BlitOutline"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _SecTex ("Depth Tex", 2D) = "black" {}
        _ThirdTex ("Color Tex", 2D) = "black" {}
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
                "LightMode" = "UniversalForward"
            }

            Blend One Zero
            ZWrite Off
            ZTest LEqual
            Cull Back

            HLSLPROGRAM
            #pragma enable_d3d11_debug_symbols
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_SecTex);
            SAMPLER(sampler_SecTex);
            SAMPLER(sampler_BlitTexture);
            FRAMEBUFFER_INPUT_X_HALF(0);
            // FRAMEBUFFER_INPUT_X_HALF(1);

            v2f vert(appdata IN)
            {
                v2f OUT;
                OUT.vertex = TransformObjectToHClip(IN.vertex.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                float4 depth = LOAD_FRAMEBUFFER_X_INPUT(0, IN.uv.xy);
                // float4 color = LOAD_FRAMEBUFFER_X_INPUT(1, IN.uv.xy);
                // float depth = SAMPLE_DEPTH_TEXTURE(_SecTex, sampler_SecTex, IN.uv);
                float blitColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv);
                return blitColor;
            }
            ENDHLSL
        }
    }
}