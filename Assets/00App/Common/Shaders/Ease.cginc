#ifndef dbg_ease
  #define dbg_ease

#ifndef dbg_math
#include "Math.cginc"
#endif

float easeIn(float interpolator){
	return interpolator * interpolator;
}

float easeOut(float interpolator)
{
    return 1 - easeIn(1 - interpolator);
}

float easeInOut(float interpolator){
	float easeInValue = easeIn(interpolator);
	float easeOutValue = easeOut(interpolator);
	return lerp(easeInValue, easeOutValue, interpolator);
}

struct CurveKeyframe
{
    float time;
    float tangent;
    float value;
};

// http://en.wikipedia.org/wiki/Cubic_Hermite_spline
float CubicHermiteSpline(float t, CurveKeyframe keyframe0, CurveKeyframe keyframe1)
{
    float dt = keyframe1.time - keyframe0.time;
    t = (t - keyframe0.time) / dt;

    float m0 = keyframe0.tangent * dt;
    float m1 = keyframe1.tangent * dt;

    float t2 = t * t;
    float t3 = t2 * t;

    float a = 2 * t3 - 3 * t2 + 1;
    float b = t3 - 2 * t2 + t;
    float c = t3 - t2;
    float d = -2 * t3 + 3 * t2;

    return a * keyframe0.value + b * m0 + c * m1 + d * keyframe1.value;
}

StructuredBuffer<CurveKeyframe> CurveKeyframes;
int CubicHermiteSplineFramesCount;

float CubicHermiteSplines(float t)
{
    for(int i=0;i<CubicHermiteSplineFramesCount;i++)
    {
        if(t < CurveKeyframes[i].time)
        {
            return CubicHermiteSpline(t,
                CurveKeyframes[clamp(i-1, 0, CubicHermiteSplineFramesCount - 1)],
                CurveKeyframes[i]
            );
        }
    }
    return CurveKeyframes[CubicHermiteSplineFramesCount - 1].value;
}

float Smoothcycle(float v, float width, float offset)
{
    return easeInOut(abs((abs(v + width / 2 + offset) % width) - width * 0.5) * 2 / width);
}

#endif




