Shader "Custom/PolarScan"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white"
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            float heart(float2 uv)
            {
                uv = uv - 0.5;
                float theta = atan2(uv.y, uv.x);
                float heartR = 0.3 * (1 - sin(theta) * sqrt(abs(cos(theta))) / (sin(theta) + 3));
                return length(uv) - heartR;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = 1 - float2(input.uv.x, input.uv.y);
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float phase = frac(_Time.x + 1);
                float heartDist = smoothstep(1-heart(input.uv), phase, 0.2);
                return float4(heartDist, heartDist, 0, 1);
            }
            ENDHLSL
        }
    }
}