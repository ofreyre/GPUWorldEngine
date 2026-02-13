using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PerlinNoiseSettings
{
	public float scale = 0.1f;
	[Range(0, 1)]public float persistance = .6f;
	public float lacunarity = 2;
	public int seed;


#if UNITY_EDITOR

	public void OnValidate()
	{
		scale = Mathf.Max(scale, 0.01f);
		//lacunarity = Mathf.Max(lacunarity, 1);
		//persistance = Mathf.Clamp01(persistance);
	}
#endif
}
