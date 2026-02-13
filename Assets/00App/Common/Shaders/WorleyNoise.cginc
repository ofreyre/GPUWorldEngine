#ifndef dbg_worleynoise
  #define dbg_worleynoise



#ifndef dbg_whitenoise
  #include "WhiteNoise.cginc"
#endif

float WorleyNoise2D(float2 p, float scale, float cellLength)
{
    float2 st = p / cellLength;

   // Scale
    st *= scale;

   // Tile the space
    float2 i_st = floor(st);
    float2 f_st = frac(st);

    float m_dist = 1.0; // minimum distance
    
        [unroll]
        for (int y = -1; y <= 1; y++)
    {

        [unroll]
        for (int x = -1; x <= 1; x++)
        {

            // Neighbor place in the grid

            float2 neighbor = float2(float(x), float(y));

            // Random position from current + neighbor place in the grid

            float2 rndPoint = rand2dTo2d(i_st + neighbor);

	        // Vector between the pixel and the point

            float2 diff = neighbor + rndPoint - f_st;

            // Distance to the point
            float dist = length(diff);

            // Keep the closer distance
            m_dist = min(m_dist, dist);
        }
    }

    // Invert min distance (distance field)
    return 1 - m_dist;
}

float WorleyNoise3D (float3 p, float scale, float cellLength)
{
   float3 st = p/cellLength;

   // Scale
   st *= scale;

   // Tile the space
   float3 i_st = floor(st);
   float3 f_st = frac(st);

   float m_dist = 1.0;  // minimum distance

   [unroll]
   for(int z= -1; z <= 1; z++) {

        [unroll]
        for(int y= -1; y <= 1; y++) {

            [unroll]
            for(int x= -1; x <= 1; x++) {

                // Neighbor place in the grid

                float3 neighbor = float3(float(x), float(y), float(z));

                // Random position from current + neighbor place in the grid

                float3 rndPoint = rand3dTo3d(i_st + neighbor);

	            // Vector between the pixel and the point

                float3 diff = neighbor + rndPoint - f_st;

                // Distance to the point
                float dist = length(diff);

                // Keep the closer distance
                m_dist = min(m_dist, dist);
            }
        }
    }

    // Invert min distance (distance field)
    return 1 - m_dist;
}



#endif




