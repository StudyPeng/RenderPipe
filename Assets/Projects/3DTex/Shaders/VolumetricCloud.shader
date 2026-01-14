Shader "Common/Volumetric-Cloud"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _Transform ("Transform", Vector) = (0, 0, 0, 0)
        _Radius ("Radius", Float) = 0.5
        _MaxDistance ("Max Distance", Float) = 100
        _Epsilon("_Epsilon", Range(0.00001, 0.01)) = 0.01
    }
    SubShader
    {
        Pass
        {
            Name "Volumetric-Cloud"

            Tags
            {
                "RenderType"="Opaque"
                "Queue"="Opaque"
                "RenderPipeline"="UniversalPipeline"
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "../../2DUI/Shader/Noise.hlsl"

            struct Attributes
            {
                float4 vertex : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Transform;
            float _Radius;
            float _Epsilon;
            float _MaxDistance;

            Varyings vert(Attributes i)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(i.vertex.xyz);
                o.positionWS = TransformObjectToWorld(i.vertex.xyz);
                return o;
            }

            float sdf_sphere(float3 p, float r)
            {
                return length(p) - r;
            }

            bool raymarching(float3 ori, float3 dir, out float3 hitPos)
            {
                float t = 0;
                for (int i = 0; i < 1028; ++i)
                {
                    float3 p = ori + dir * t;
                    float inner_dist = sdf_sphere(p, _Radius);
                    if (inner_dist < _Epsilon)
                    {
                        hitPos = p;
                        return true;
                    }

                    t += inner_dist;

                    if (t > _MaxDistance) break;
                }
                hitPos = 0;
                return false;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float3 origin = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1)).xyz;
                float3 dir = normalize(mul(unity_WorldToObject, i.positionWS - _WorldSpaceCameraPos));
                float3 hitPos = 0;
                if (raymarching(origin, dir, hitPos))
                {
                    return 1;
                }
                
                return 0;
            }
            ENDHLSL
        }
    }
}