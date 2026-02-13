#ifndef dbg_lighting
#define dbg_lighting

float simpleLanbert(float3 n, float3 d)
{
    return dot(n, d);
}

#endif