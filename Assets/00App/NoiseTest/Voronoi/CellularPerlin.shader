Shader "Unlit/Cellular Perlin"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _BaseMap("Base Map", 2D) = "white"

        _PerlinWeight("Perlin Weight", Float) = 1

        _PerlinScale("Perlin Scale", Float) = 1
        _PerlinCellLength("Perlin Cell Length", Float) = 1
        _PerlinPeriod("Perlin Period", Float) = 1
        _PerlinLayers("Perlin Layers", Integer) = 1
        _PerlinPersistance("Perlin Persistance", Float) = 1
        _PerlinRoughness("Perlin Roughness", Float) = 1

        _CellularScale("Cellular Scale", Float) = 1
        _CellularCellLength("Cellular Cell Length", Float) = 1
        _CellularPeriod("Cellular Period", Float) = 1
        _CellularLayers("Cellular Layers", Integer) = 1
        _CellularPersistance("Cellular Persistance", Float) = 1
        _CellularRoughness("Cellular Roughness", Float) = 1

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

                float _PerlinWeight;

                float _PerlinScale;
                float _PerlinCellLength;
                float _PerlinPeriod;
                uint _PerlinLayers;
                float _PerlinPersistance;
                float _PerlinRoughness;

                float _CellularScale;
                float _CellularCellLength;
                float _CellularPeriod;
                uint _CellularLayers;
                float _CellularPersistance;
                float _CellularRoughness;
            CBUFFER_END

#ifndef dbg_math
#include "../../Common/Shaders/PerlinWorley.cginc"
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
                /*
                float c = PerlinWorley3D(IN.positionWS,
                            _PerlinWeight, _PerlinScale, _PerlinCellLength, _PerlinPeriod, _PerlinLayers, _PerlinPersistance, _PerlinRoughness,
                            _CellularScale, _CellularCellLength, _CellularPeriod, _CellularLayers, _CellularPersistance, _CellularRoughness
                        );
                */

                
                float cP = PerlinNoise3Tiledlayered(IN.positionWS, _PerlinScale, _PerlinCellLength, _PerlinPeriod, _PerlinLayers, _PerlinPersistance, _PerlinRoughness);
                float cC = Cellular3dtiledlayered(IN.positionWS, _CellularScale, _CellularCellLength, _CellularPeriod, _CellularLayers, _CellularPersistance, _CellularRoughness);
                float cm = _PerlinWeight * cP + (1 - cC) * (1 - _PerlinWeight);
                

                //float c = 1 - Cellular3dtiledlayered(IN.positionWS + _CellularPeriod, _CellularScale, _CellularCellLength, _CellularPeriod, _CellularLayers, _CellularPersistance, _CellularRoughness);

                float4 color;
                //color.xyz = c;
                //color.xyz = cP;
                //color.xyz = (1-cC);
                color.xyz = cm;
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }
}
