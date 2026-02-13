using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Texture3DBaker
{
    [Serializable]
    public class MixNoiseSettings
    {
        public string noiseName;
        public float perlinWeight = 128;
        public NoiseSettings perlin = new NoiseSettings
        {
            scale = 1,
            cellLength = 0.47f,
            period = 8.5f,
            layers = 4,
            persistance = 1.58f,
            roughness = 0.32f
        };

        public NoiseSettings cellular = new NoiseSettings
        {
            scale = 0.47f,
            cellLength = 1,
            period = 8.5f,
            layers = 4,
            persistance = 1.58f,
            roughness = 0.4f
        };

        //Editor state
        public bool foldOut = true;

        public int period
        {
            set
            {
                perlin.period = value;
                cellular.period = value;
            }
        }
    }
}