using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace Texture3DBaker
{
    public class TextureBakerSettings : ScriptableObject
    {
        public ComputeShader computeBake;
        public string computeBakeKernel = "Compute";
        public ComputeShader computeRender;
        public string computeRenderKernel = "Compute";
        public Shader renderShader;
        public DetailSettings[] details = new DetailSettings[] {
        new DetailSettings()
        {
            detailName = "Heigh Details",
            TextureSize = 128
        } ,
        new DetailSettings()
        {
            detailName = "Low Details",
            TextureSize = 64
        }
    };

        //Editor state
        public string[] colorNames = new string[] { "R", "G", "B", "A" };
        public bool[] renderColorChannels = { true, true, true, true };
        public Vector2 scrollPos;
        public Rect windowRect;
        public string lastSavePath;

        public float settingsWidth = 270;
        public float labelWidth = 100;
        public float fieldWidth = 20;
        public float sliderWidth = 100;

        float[] perlinWeightsWithVisibility;

        public int SelectedDetailIndex
        {
            get
            {
                for (int i = 0; i < details.Length; i++)
                {
                    if (details[i].visible) return i;
                }
                return 0;
            }
            set
            {
                for (int i = 0; i < details.Length; i++)
                {
                    details[i].visible = i == value;
                }
            }
        }

        public DetailSettings SelectedDetail { get { return details[SelectedDetailIndex]; } }

        public float[] GetPerlinWeightsWithVisibility(DetailSettings detailSettings)
        {
            if (perlinWeightsWithVisibility == null)
            {
                perlinWeightsWithVisibility = new float[renderColorChannels.Length];
            }
            detailSettings.GetPerlinWeights(renderColorChannels, ref perlinWeightsWithVisibility);
            return perlinWeightsWithVisibility;
        }

        public float SelectedY
        {
            get { return SelectedDetail.y; }
            set { SelectedDetail.y = value; }
        }

        public int SelectedTextureSize
        {
            get { return SelectedDetail.TextureSize; }
            set { SelectedDetail.TextureSize = value; }
        }
    }
}
