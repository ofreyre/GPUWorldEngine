#ifndef dbg_math
#define dbg_math

float modulo(float dividend, float divisor)
{
    float positiveDivident = dividend % divisor + divisor;
    return positiveDivident % divisor;
}

float2 modulo2(float2 dividend, float2 divisor)
{
    float2 positiveDivident = dividend % divisor + divisor;
    return positiveDivident % divisor;
}

float3 modulo3(float3 dividend, float3 divisor){
    float3 positiveDivident = dividend % divisor + divisor;
    return positiveDivident % divisor;
}

float Cycle(float v, float width, float offset)
{
    return abs((abs(v + width / 2 + offset) % width) - width * 0.5) * 2 / width;
}

#endif




