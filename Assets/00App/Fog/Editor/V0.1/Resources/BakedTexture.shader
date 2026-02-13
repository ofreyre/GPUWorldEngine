Shader "Unlit/BakedTexture"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _BaseMap("Base Map", 3D) = "white"
        _TextureSize("Texture Size", Float) = 1
        _Scale("Scale", Float) = 1
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
                float4 _BaseMap_ST;
                float4 _TextureSize;
                float _Scale;
            CBUFFER_END
                       

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.vertexHCS = TransformObjectToHClip(IN.vertexOS.xyz);
                OUT.vertexWS = mul(unity_ObjectToWorld, IN.vertexOS).xyz;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 color = SAMPLE_TEXTURE3D(_BaseMap, sampler_BaseMap, IN.vertexWS * _Scale);
                return color;
            }
            ENDHLSL
        }
    }
}
