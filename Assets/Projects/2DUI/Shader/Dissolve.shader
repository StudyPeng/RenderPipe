Shader "Common/Unlit-Dissolve"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [KeywordEnum(White, Simple, Gradient, Voronoi)] _Noise ("Noise Type", Float) = 0
        _Offset ("UV Offset", Vector) = (0, 0, 0, 0)
        [Toggle] _Revert ("Revert Noise", Float) = 1
        _Amplitude ("Noise Amplitude", Float) = 1
        _Value ("Value", Range(0, 1)) = 1
        [MaterialToggle] _ZWrite("ZWrite", Float) = 0
        [HideInInspector] _Color ("Tint", Color) = (1,1,1,1)
        [HideInInspector] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _AlphaTex ("External Alpha", 2D) = "white" {}
    }
    SubShader
    {
        Pass
        {
            Tags
            {
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
                "RenderPipeline" = "UniversalPipeline"
            }

            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            Cull Off
            ZWrite [_ZWrite]

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Noise.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
                UNITY_SKINNED_VERTEX_INPUTS
            };

            struct Varyings
            {
                COMMON_2D_OUTPUTS
                half4 color : COLOR;
            };

            #pragma multi_compile_instancing
            #pragma multi_compile _ SKINNED_SPRITE
            #pragma shader_feature _NOISE_WHITE _NOISE_SIMPLE _NOISE_GRADIENT _NOISE_VORONOI

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _Offset;
            float _Power;
            float _Amplitude;
            float _Value;
            float _Revert;

            Varyings vert(Attributes input)
            {
                UNITY_SKINNED_VERTEX_COMPUTE(input);
                input.positionOS = float3(input.positionOS.xy * unity_SpriteProps.xy, input.positionOS.z);
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionCS = TransformObjectToHClip(input.positionOS);
                o.uv = input.uv;
                o.color = input.color * _Color * unity_SpriteColor;
                return o;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTex = input.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half2 offsetUV = input.uv + _Offset.xy;
                half n = 0;
                #ifdef _NOISE_WHITE
                n = WhiteNoise(offsetUV * _Amplitude);
                #elif _NOISE_SIMPLE
                n = SimpleNoise(offsetUV * _Amplitude);
                #elif _NOISE_GRADIENT
                n = GradientNoise(offsetUV * _Amplitude);
                #elif _NOISE_VORONOI
                n = VoronoiNoise(offsetUV * _Amplitude);
                #endif
                
                if (_Revert >= 0.5) n = 1 - n;
                float alpha = smoothstep(0, n, _Value);
                return half4(mainTex.x, mainTex.y, mainTex.z, alpha);
            }
            ENDHLSL
        }
    }
}