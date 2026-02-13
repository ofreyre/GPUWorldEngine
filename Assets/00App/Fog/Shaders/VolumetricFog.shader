Shader "Unlit/Volumetric Fog"
{
    // The _BaseMap variable is visible in the Material's Inspector, as a field
    // called Base Map.
    Properties
    {
        _FogColor("Base Color", Color) = (1, 1, 1, 1)
        _BaseMap("Base Map", 3D) = "white"
        _TextureSize("Texture Size", Float) = 1
        _MinDistance("MinDistance", Float) = 1
        _MaxDistance("MaxDistance", Float) = 10
        _Scale("Scale", Vector) = (1,1,1,1)
        _Weights("Weights", Vector) = (0.5,0.25,0.125,0.0725)
        _StepSize("Step Size", Float) = 1
        _Wind("Wind", Vector) = (0.5,0,0.125,0)
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
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #define STEPS 100

            struct Attributes
            {
                float4 vertexOS   : POSITION;
            };

            struct Varyings
            {
                float4 vertexHCS        : SV_POSITION;
                float4 screenUV         : TEXCOORD0;
                float3 vertexWS         : TEXCOORD1;
                float3 rd               : TEXCOORD2;
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
                float3 _Scale;
                float4 _Weights;
                float _StepSize;
                half4 _FogColor;
                float3 _Wind;
                float _MaxDistance;
                float _MinDistance;

                //sampler2D _CameraOpaqueTexture;
                CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.vertexHCS = TransformObjectToHClip(IN.vertexOS.xyz);
                OUT.vertexWS = mul(unity_ObjectToWorld, IN.vertexOS).xyz;
                float3 ray = OUT.vertexWS - _WorldSpaceCameraPos;
                OUT.rd = ray;
                OUT.screenUV = ComputeScreenPos(OUT.vertexHCS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                uint2 screenUV = uint2(IN.screenUV.xy / IN.screenUV.w * _ScreenSize.xy);
                float normalDepth = LOAD_TEXTURE2D_LOD(_CameraDepthTexture, screenUV, 0).r;
                float depth = LinearEyeDepth(normalDepth, _ZBufferParams);
                
                if (_MinDistance >= depth)
                    discard;
                
                float camRayLength = length(IN.rd);
                float startDistance = max(_MinDistance, camRayLength);
                float totalDistance = min(_MaxDistance, depth) - startDistance;

                float3 rd = normalize(IN.rd);
                float3 p0 = IN.vertexWS + rd * (startDistance - camRayLength);


                float rayLength = (_MaxDistance - _MinDistance) / float(STEPS);
                uint steps = min(totalDistance / rayLength, STEPS);

                float fog = 0;
                float c = 0;
                float sampleDistance = 0;

                float b = 4;
                float a = (log(totalDistance) + b) / STEPS;

                [unroll(STEPS)]
                for (uint i = 0; i < steps; i++)
                {
                    float step = float(i) / float(STEPS);
                    sampleDistance = exp(a * i - b);
                    float3 sampleVector = rd * sampleDistance;

                    float3 p = p0 + sampleVector;
                    p *= _Scale;

                    float4 noise = SAMPLE_TEXTURE3D(_BaseMap, sampler_BaseMap, p);
                    float density = dot(noise, _Weights);
                    //if (density > 0)
                    {
                        density *= exp(3 * step - 5);
                        fog += density;
                        if (fog >= 1)
                        {
                            break;
                        }
                    }
                }

                //Hacer resplandor solar o de luna aca

                return half4(_FogColor.rgb, saturate(fog) * _FogColor.a);
            }
            ENDHLSL
        }
    }
}
