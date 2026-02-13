Shader "Unlit/Functions"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _Scale("Cell Length", Float) = 1
        _Smoothstep0("Smoothstep0", Float) = 0
        _Smoothstep1("Smoothstep1", Float) = 1
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

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
                float _Scale;
                float _Smoothstep0;
                float _Smoothstep1;
            CBUFFER_END

#ifndef dbg_math
#include "../Common/Shaders/Ease.cginc"
#endif

            float2 ValueFilter(float py, float y)
            {
                float d = abs(y - py);
                float h = fwidth(py) * 2;
                return smoothstep(0, h, d);
            }

            float SmoothStep(float2 p)
            {
                return ValueFilter(p.y, smoothstep(_Smoothstep0 * _Scale, _Smoothstep1 * _Scale, p.x) * _Scale);
            }

            float DrawCycle(float2 p, float width, float offset)
            {
                //float y = abs((abs(p.x + width / 2 + offset) % width) - width * 0.5) * 2 / width;
                return ValueFilter(p.y, Cycle(p.x, width, offset));
            }

            float DrawSmoothcycle(float2 p, float width, float offset)
            {
                //float y = abs((abs(p.x + width / 2 + offset) % width) - width * 0.5) * 2 / width;
                return ValueFilter(p.y, Smoothcycle(p.x, width, offset));
            }

            float SmoothStepComp(float2 p)
            {
                float s0 = 1 - smoothstep(1 - _Smoothstep1, 1 - _Smoothstep0, p.x);
                return ValueFilter(p.y, s0);
            }

            float SmoothStepInOut(float2 p)
            {
                float s0 = smoothstep(_Smoothstep0, _Smoothstep1, p.x);
                float s1 = SmoothStepComp(p.x);
                float y = min(s0, s1);
                return ValueFilter(p.y, y);
            }

            float DrawEaseInOut(float2 p)
            {

            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = mul(unity_ObjectToWorld, IN.positionOS);
                // The TRANSFORM_TEX macro performs the tiling and offset
                // transformation.
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                //float c = SmoothStep(IN.positionWS.xz);
                //float c = SmoothStepComp(IN.positionWS.xz);
                //float c = DrawCycle(float2(IN.positionWS.x,IN.positionWS.z), 0.5, 1.5);
                //float c = DrawSmoothcycle(float2(IN.positionWS.x,IN.positionWS.z), 0.5, 1.5);
                //float c = min(SmoothStep(IN.positionWS.xz), SmoothStepComp(IN.positionWS.xz));
                float c = SmoothStepInOut(IN.positionWS.xz);

                float4 color;
                color.xyz = float3(c, 0,0);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }
}
