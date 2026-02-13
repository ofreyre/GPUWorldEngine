Shader "Unlit/Voronoi"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _BaseMap("Base Map", 2D) = "white"
        _CellLength("Cell Length", Float) = 1
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"            

            struct Attributes
            {
                float4 positionOS   : POSITION;
                // The uv variable contains the UV coordinate on the texture for the
                // given vertex.
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS    : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float3 positionWS       : TEXCOORD1;
            };

            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE2D(_BaseMap);
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float4 _BaseMap_ST;
                float _CellLength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

#ifndef dbg_math
#include "../Common/Shaders/Ease.cginc"
#endif

#ifndef dbg_whitenoise
#include "../Common/Shaders/WhiteNoise.cginc"
#endif

            float Voronoid2d(float2 p, float cellLength)
            {
                float2 cp = floor(p / cellLength);
                float d = 9999999;
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        float2 cell = cp + float2(j, i);
                        float2 v = cell + (rand2dTo2d(cell) * 2 - 1);
                        float dist = distance(p.xy, v);
                        d = min(d, dist);
                    }
                }

                return d;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 n = Voronoid2d(IN.positionWS.xz, _CellLength);
                
                half4 color = half4(0,0,0,1);
                color.xyz += n;

                /*
                color.rg = frac(sin(p.xy * 3));
                color.b = frac(sin(p.x * 2 + p.y* 3));
                color = abs(color - abs(sin(80.0 * d) * 0.5) * 0.5);
                color += 1. - step(.02, d);
                */

                return color;
            }
            ENDHLSL
        }
    }
}
