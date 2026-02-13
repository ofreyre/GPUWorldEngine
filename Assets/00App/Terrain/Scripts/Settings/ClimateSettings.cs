using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ClimateSettings : PerlinNoiseSettings
{
    [Header("Temperature")]
    public float m_latitudeDecreaser;
    public float m_heightDecreaser;
}
