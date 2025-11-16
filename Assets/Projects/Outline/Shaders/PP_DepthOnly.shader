Shader "Common/PP_DepthOnly"
{
    SubShader
    {
        Pass
        {
            Name "PP_DepthOnly"

            Tags
            {
                "LightMode" = "DepthOnly"
            }
            
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask R

            HLSLPROGRAM

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            ENDHLSL
        }
    }
}