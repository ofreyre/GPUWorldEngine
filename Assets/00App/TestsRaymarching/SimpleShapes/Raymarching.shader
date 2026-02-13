Shader "Unlit/Raymarching"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #include "MarchingSphere.hlsl"
            #include "MarchingCube.hlsl"
            #include "MarchingCuber.hlsl"
            #include "MarchingCubef.hlsl"
            #include "MarchingTorus.hlsl"

            struct Attributes
            {
                float4 vertexOS   : POSITION;
                float4 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 vertexHCS        : SV_POSITION;
                float3 vertexWS         : TEXCOORD1;
                //float3 normalWS         : TEXCOORD2;
            };

            // This macro declares _BaseMap as a Texture2D object.
            TEXTURE3D(_BaseMap);
            // This macro declares the sampler for the _BaseMap texture.
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                // The following line declares the _BaseMap_ST variable, so that you
                // can use the _BaseMap variable in the fragment shader. The _ST 
                // suffix is necessary for the tiling and offset function to work.
            CBUFFER_END
                       
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.vertexHCS = TransformObjectToHClip(IN.vertexOS.xyz);
                OUT.vertexWS = mul(unity_ObjectToWorld, IN.vertexOS).xyz;
                //OUT.normalWS = mul(unity_ObjectToWorld, IN.normalOS).xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 p = IN.vertexWS;
                p.x += 3;
                float4 c = sphereColor(p, normalize(IN.vertexWS - _WorldSpaceCameraPos), float3(1, 0, 0));
                p.x -= 1;
                float4 c1 = cubeColor(p, normalize(IN.vertexWS - _WorldSpaceCameraPos), float3(1, 0, 0));
                p.x -= 1;
                float4 c2 = cuberColor(p, normalize(IN.vertexWS - _WorldSpaceCameraPos), float3(1, 0, 0));
                p.x -= 1;
                float4 c3 = cubefColor(p, normalize(IN.vertexWS - _WorldSpaceCameraPos), float3(1, 0, 0));
                p.x -= 1;
                float4 c4 = torusColor(p, normalize(IN.vertexWS - _WorldSpaceCameraPos), float3(1, 0, 0));
                return c + c1 + c2 + c3 + c4;
            }
            ENDHLSL
        }
    }
}
