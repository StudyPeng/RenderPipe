Shader "Custom/NormalMap"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white"
        _DeltaScale ("Delta Scale", Float) = 0.01
        _HeightScale ("Height Scale", Float) = 0.01
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float3 vertexOS : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertexCS : SV_POSITION;
                float4 uv : TEXCOORD0;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            float _DeltaScale;
            float _HeightScale;

            float GetGray(float3 p)
            {
                return p.r * 0.2126 + p.g * 0.7152 + p.b * 0.0722;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.vertexCS = TransformObjectToHClip(input.vertexOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float2 du = float2(_MainTex_TexelSize.x * _DeltaScale, 0);
                float u1 = GetGray(_MainTex.Sample(sampler_MainTex, input.uv.xy - du).xyz);
                float u2 = GetGray(_MainTex.Sample(sampler_MainTex, input.uv.xy + du).xyz);
                float3 tu = float3(du.x, 0, _HeightScale * (u1 - u2));
                float2 dv = float2(0, _MainTex_TexelSize.y * _DeltaScale);
                float v1 = GetGray(_MainTex.Sample(sampler_MainTex, input.uv.xy - dv).xyz);
                float v2 = GetGray(_MainTex.Sample(sampler_MainTex, input.uv.xy + dv).xyz);
                float3 tv = float3(0, dv.y, _HeightScale * (v1 - v2));
                float3 normal = normalize(cross(tu, tv));
                float4 color = float4(normal * 0.5 + 0.5, 1);
                return color;
            }
            ENDHLSL
        }
    }
}