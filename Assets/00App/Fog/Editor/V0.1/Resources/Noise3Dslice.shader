Shader "Unlit/Noise3Dslice"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _Period("Period", Float) = 128
        _Weight("Weight", Vector) = (.5, .5, .5, .5)

        _ScaleP("ScaleP", Vector) = (1, 1, 1, 1)
        _CellLengthP("CellLengthP", Vector) = (6.4, 6.4, 6.4, 6.4)
        _LayersP("LayersP", Vector) = (4, 4, 4, 4)
        _PersistanceP("PersistanceP", Vector) = (1.5, 1.5, 1.5, 1.5)
        _RoughnessP("RoughnessP", Vector) = (0.5, 0.5, 0.5, 0.5)
        
        _ScaleC("ScaleC", Vector) = (1, 1, 1, 1)
        _CellLengthC("CellLengthC", Vector) = (6.4, 6.4, 6.4, 6.4)
        _LayersC("LayersC", Vector) = (4, 4, 4, 4)
        _PersistanceC("PersistanceC", Vector) = (1.5, 1.5, 1.5, 1.5)
        _RoughnessC("RroughnessC", Vector) = (0.5, 0.5, 0.5, 0.5)
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

            #define MAX_STEP_COUNT 128
            #define EPSILON 0.00001f

            struct Attributes
            {
                float4 vertexOS   : POSITION;
            };

            struct Varyings
            {
                float4 vertexHCS        : SV_POSITION;
                float3 vertexWS         : TEXCOORD1;
            };

            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE3D(_BaseMap);
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.

                float _Period;
                float4 _Weight;

                float4 _ScaleP;
                float4 _CellLengthP;
                float4 _LayersP;
                float4 _PersistanceP;
                float4 _RoughnessP;

                float4 _ScaleC;
                float4 _CellLengthC;
                float4 _LayersC;
                float4 _PersistanceC;
                float4 _RoughnessC;
            CBUFFER_END
                       
            #include "E:\Public\UnityOscar\ShaderExperiments\Assets\00App\Fog\Editor\V0.1\Resources\PerlinCellular3Tiled.hlsl"

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.vertexHCS = TransformObjectToHClip(IN.vertexOS.xyz);
                OUT.vertexWS = mul(unity_ObjectToWorld, IN.vertexOS).xyz;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {

                float4 color = Nose(IN.vertexWS).r;
                return color;
            }
            ENDHLSL
        }
    }
}
