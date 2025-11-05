Shader "Common/Z_Test"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Name "Z_Test"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            ZTest LEqual
            
            Tags
            {
                "RenderType"="Transparent"
                "Queue"="Transparent"
                "RenderPipeline"="UniversalPipeline"
                "LightMode" = "UniversalForward"
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                return o;
            }

            float4 frag(Varyings i) : SV_Target
            {
                return float4(0.9, 0.6, 0.9, 0.25);
            }
            ENDHLSL
        }
    }
}