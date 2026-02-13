using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public struct NoiseSettings
{
    public float scale;
    public float cellLength;
    public float period;
    public int layers;
    public float persistance;
    public float roughness;

    public NoiseSettings(float scale, float cellLength, float period, int layers, float persistance, float roughness)
    {
        this.scale = scale;
        this.cellLength = cellLength;
        this.period = period;
        this.layers = layers;
        this.persistance = persistance;
        this.roughness = roughness;
    }

    public override string ToString()
    {
        return "scale = " + scale + "  cellLength = " + cellLength +
            "  period = " + period + "  layers = " + layers + "  persistance = " + persistance +
            "  roughness = " + roughness;
    }
};
