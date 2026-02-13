using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Texture3DBaker
{
    [Serializable]
    public class DetailSettings
    {
        public int textureSize;
        public MixNoiseSettings[] channels = new MixNoiseSettings[] {
            new MixNoiseSettings(),new MixNoiseSettings(),new MixNoiseSettings(),new MixNoiseSettings()
        };

        //Editor state
        public string detailName;
        public bool foldout = true;
        public bool visible = true;
        public float y = 0;

        public int TextureSize
        {
            set
            {
                textureSize = value;
                for (int i = 0; i < channels.Length; i++)
                {
                    channels[i].period = value;
                }
            }

            get { return textureSize; }
        }


        NoiseSettings[] noiseSettings;
        float[] perlinWeight;

        public NoiseSettings[] NoiseSettings
        {
            get
            {
                if (noiseSettings == null || noiseSettings.Length == 0)
                {
                    noiseSettings = new NoiseSettings[channels.Length * 2];
                }
                for (int i = 0; i < channels.Length; i++)
                {
                    noiseSettings[i * 2] = channels[i].perlin;
                    noiseSettings[i * 2 + 1] = channels[i].cellular;
                }
                return noiseSettings;
            }
        }

        public float[] PerlinWeights
        {
            get
            {
                if (perlinWeight == null || perlinWeight.Length == 0)
                {
                    perlinWeight = new float[channels.Length];
                }
                for (int i = 0; i < channels.Length; i++)
                {
                    perlinWeight[i] = channels[i].perlinWeight;
                }
                return perlinWeight;
            }
        }

        public void GetPerlinWeights(bool[] visibility, ref float[] pw)
        {
            for (int i = 0; i < channels.Length; i++)
            {
                pw[i] = channels[i].perlinWeight + ((visibility[i] ? 1 : 0) - 1) * 2;
            }
        }
    }
}
