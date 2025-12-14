Shader "LiteRP/Unlit-Alpha-Test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Pass
        {
            Name "Unlit-Alpha-Test"
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off Lighting Off ZWrite Off
            
            Tags
            {
                "RenderType"="Transparent"
                "Queue"="Transparent"
                "IgnoreProjector"="True"
                "RenderPipeline"="SRPDefaultUnlit"
                "LightMode"="SRPDefaultUnlit"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.vertex.xyz);
                o.texcoord = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
                return float4(c.xyz * _Color.xyz, _Color.w);
            }
            ENDHLSL
        }
    }
}