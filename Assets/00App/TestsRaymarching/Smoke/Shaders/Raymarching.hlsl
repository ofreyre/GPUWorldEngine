//#ifndef TERRAINALBEDO_INCLUDE
//#define TERRAINALBEDO_INCLUDE

float Distance(float3 p, float r)
{
	//p.y = p.y / y;

	return length(p) - r;
}

float Distance1(float3 p, float perlinC, float r, float offsetY, float perlinR)
{
	float r1 = 0.2 + r * p.y * perlinR / 10;
	float3 p1 = float3(cos(p.y + offsetY) * r1 + perlinC, p.y, sin(p.y + offsetY) * r1 + perlinC);
	return length(p - p1) - r1;
}

float3 GetNormal(float normalOffset, float3 p, float y)
{
	float3 offset = float3(normalOffset, 0 , 0);
	return normalize(
		float3(
			Distance(p + offset.xyy, y) - Distance(p - offset.xyy, y),
			Distance(p + offset.yxy, y) - Distance(p - offset.yxy, y),
			Distance(p + offset.yyx, y) - Distance(p - offset.yyx, y)
		)
	);
} 

float3 GetNormal1(float normalOffset, float3 p, float perlinC, float r, float offsetY, float perlinR)
{
	float3 offset = float3(normalOffset, 0 , 0);
	return normalize(
		float3(
			Distance1(p + offset.xyy,perlinC , r, offsetY, perlinR) - Distance1(p - offset.xyy,perlinC , r, offsetY, perlinR),
			Distance1(p + offset.yxy,perlinC , r, offsetY, perlinR) - Distance1(p - offset.yxy,perlinC , r, offsetY, perlinR),
			Distance1(p + offset.yyx,perlinC , r, offsetY, perlinR) - Distance1(p - offset.yyx,perlinC , r, offsetY, perlinR)
		)
	);
}

float3 Map(float x, float p0, float p1, float q0, float q1)
{
	return q1 + (x-p0) * (q1 - q0) / (p1 - p0);
}


void Raymarch_float(float hitDistance, float maxDistance, float normalOffset, float3 rayOrigin
				, float3 rayDirection, UnityTexture2D perlin, float opacity
				, out float3 albedo, out float3 normal, out float alpha)
{
	const int STEPS = 40;
	float totalDistance = 0;
	alpha = 0;
	for(int i=0; i < STEPS; i++)
	{
		float3 p = rayOrigin + totalDistance * rayDirection;

		float t = -_Time.y;
		float perlinValue = tex2D(perlin, float2(0,p.y / 30 + t/20)).x ;
		float perlinC = (perlinValue - 0.3) * p.y * 0.5;
		float perlinR = perlinValue * 10;
		float distance = Distance1(p, perlinC, 1, t, perlinR);

		if(distance < hitDistance)
		{
			if(p.y < 0)
				break;

			albedo = float3(1,1,1);
			normal = GetNormal1(normalOffset, p, perlinC, 1, t, perlinR);
			float perlinAlpha = 0.1 + tex2D(perlin, float2(p.x,p.z)  / 20 - t/200).x ;
			//alpha = 0.01 * (5 - perlinValue * 5) * (10 - p.y);

			float cos = abs(dot(normal,rayDirection));
			if(cos > 0.1)
				cos = 1;
			else
				cos = Map(cos, 0.05, 0, 1, 0.01);
			alpha = opacity * perlinAlpha * (10 - p.y) * cos;
			//alpha = cos;
			
        	break;

		}
		else
		{
			totalDistance += distance;
		}

		if (totalDistance > maxDistance)
        {
        	break;
        }
	}
}
