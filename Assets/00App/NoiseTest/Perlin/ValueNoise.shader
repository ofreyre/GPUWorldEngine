Shader "Unlit/ValueNoise"
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

            float RND(float3 value, float mutator = 0.546)
            {
                float random = frac(sin(value + mutator) * 143758.5453);
                return random;
            }

            float RND2_1(float2 p, float2 dotp = float2(7.3412, 47.7597))
            {
                return frac(
                    sin(dot(p, dotp)) * 143758.5453
                );
            }

            float RND3_1(float3 p, float3 dotp = float3(7.3412, 47.7597, 27.4913))
            {
                return frac(
                    sin(dot(p, dotp)) * 143758.5453
                );
            }

            float easeIn(float x)
            {
                return x * x;
            }

            float easeOut(float x)
            {
                return 1 - easeIn(1 - x);
            }

            float easeInOut(float x)
            {
                return lerp(easeIn(x), easeOut(x), x);
            }

            float2 ValueFilter(float py, float y, float cellLength)
            {
                float d = abs(y - py);
                float h = fwidth(py);
                return smoothstep(0, h, d);
            }

            float Value(float p, float cellLength)
            {
                float c = p / cellLength;
                float2 y0 = RND(floor(c));
                float2 y1 = RND(ceil(c));
                float f = frac(c);
                float interpolation = easeInOut(f);
                return lerp(y0, y1, interpolation);
            }

            float ValueNoise(float2 p, float cellLength)
            {
                float y = Value(p, cellLength);
                return ValueFilter(p.y, y, cellLength);
            }

            float ValueNoise2(float2 p, float cellLength)
            {
                float2 c = p / cellLength;
                float2 f = frac(p / cellLength);

                float c00 = RND2_1(float2(floor(c.x), floor(c.y)));
                float c10 = RND2_1(float2(ceil(c.x), floor(c.y)));
                float c01 = RND2_1(float2(floor(c.x), ceil(c.y)));
                float c11 = RND2_1(float2(ceil(c.x), ceil(c.y)));

                float interpolationX = easeInOut(frac(f.x));
                float interpolationY = easeInOut(frac(f.y));

                float c_0 = lerp(c00, c10, interpolationX);
                float c_1 = lerp(c01, c11, interpolationX);

                return lerp(c_0, c_1, interpolationY);
            }

            float ValueNoise3(float3 p, float cellLength)
            {
                float3 c = floor(p / cellLength);
                float3 f = frac(p / cellLength);

                float interpolationX = easeInOut(f.x);
                float interpolationY = easeInOut(f.y);
                float interpolationZ = easeInOut(f.z);

                float nZ[2];
                [unroll(2)]
                for (int z = 0; z <= 1; z++)
                {
                    float nY[2];
                    [unroll(2)]
                    for (int y = 0; y <= 1; y++)
                    {
                        float nX[2];
                        [unroll(2)]
                        for (int x = 0; x <= 1; x++)
                        {
                            nX[x] = RND3_1(c + float3(x, y, z));
                        }
                        nY[y] = lerp(nX[0], nX[1], interpolationX);
                    }
                    nZ[z] = lerp(nY[0], nY[1], interpolationY);
                }
                return lerp(nZ[0], nZ[1], interpolationZ);
            }


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
                float cellLength = 1;              

                float c = ValueNoise(IN.positionWS.xz, cellLength);
                //float c = ValueNoise2(IN.positionWS.xz, cellLength);
                //float c = ValueNoise3(IN.positionWS, cellLength);
                float4 color;
                color.xyz = c;
                color.a = 1;
                return color;
            }
            ENDHLSL
        }
    }
}
