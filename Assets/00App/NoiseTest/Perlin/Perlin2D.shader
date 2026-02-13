Shader "Unlit/Perlin2D"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _BaseMap("Base Map", 2D) = "white"
        _CellLength("Cell Length", Float) = 1
        _Period("Period", Float) = 1
        _Layers("Layers", Integer) = 1
        _Persistance("Persistance", Float) = 0.5
        _Roughness("Roughness", Float) = 0.5
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
                float _Period;
                uint _Layers;
                float _Persistance;
                float _Roughness;
            CBUFFER_END

#ifndef dbg_perlinnoise
#include "../../Common/Shaders/PerlinNoise.cginc"
#endif
                       

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

            half4 frag(Varyings IN) : SV_Target
            {
                //float c = PerlinNoiseDraw(IN.positionWS.xz, cellLength);
                //float c = PerlinNoiseTiledDraw(IN.positionWS.xz, _CellLength, _Period);
                //float c = PerlinNoise2D(IN.positionWS.xz, _CellLength);
                //float c = PerlinNoise2DTiled(IN.positionWS.xz, _CellLength, _Period);
                //float c = PerlinNoise3(IN.positionWS, _CellLength);
                //float c = PerlinNoise3Tiled(IN.positionWS, _CellLength, _Period);
                float c = PerlinNoise3Tiledlayered(IN.positionWS, 1, _CellLength, _Period, _Layers, _Persistance, _Roughness);

                float4 color;
                color.xyz = c;
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }
}
