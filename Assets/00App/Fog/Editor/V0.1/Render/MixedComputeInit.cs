using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Texture3DBaker
{
    public class MixedComputeInit : IRenderComputeInit
    {
        public virtual void Run(Computer computer)
        {
            TextureBakerSettings settings = computer.m_editorSettings;
            DetailSettings detail = computer.m_currentDetail;

            computer.m_compute.SetFloat("period", settings.SelectedTextureSize);
            computer.m_compute.SetVector("weight", GetWeight(settings, detail));


            GetSliceP(detail, out Vector4 scale, out Vector4 cellLength, out Vector4 layers,
            out Vector4 persistance, out Vector4 roughness);

            computer.m_compute.SetVector("scaleP", scale);
            computer.m_compute.SetVector("cellLengthP", cellLength);
            computer.m_compute.SetVector("layersP", layers);
            computer.m_compute.SetVector("persistanceP", persistance);
            computer.m_compute.SetVector("roughnessP", roughness);

            GetSliceC(detail,
            out Vector4 scaleC, out Vector4 cellLengthC, out Vector4 layersC,
            out Vector4 persistanceC, out Vector4 roughnessC);

            computer.m_compute.SetVector("scaleC", scaleC);
            computer.m_compute.SetVector("cellLengthC", cellLengthC);
            computer.m_compute.SetVector("layersC", layersC);
            computer.m_compute.SetVector("persistanceC", persistanceC);
            computer.m_compute.SetVector("roughnessC", roughnessC);
        }

        protected Vector4 GetWeight(TextureBakerSettings settings, DetailSettings detail)
        {
            float[] weights = settings.GetPerlinWeightsWithVisibility(detail);
            return new Vector4(weights[0], weights[1], weights[2], weights[3]);
        }

        protected void GetSliceP(DetailSettings detail,
            out Vector4 scale, out Vector4 cellLength, out Vector4 layers,
            out Vector4 persistance, out Vector4 roughness)
        {
            MixNoiseSettings[] channels = detail.channels;

            scale = new Vector4(channels[0].perlin.scale,
                channels[1].perlin.scale,
                channels[2].perlin.scale,
                channels[3].perlin.scale);

            cellLength = new Vector4(channels[0].perlin.cellLength,
                channels[1].perlin.cellLength,
                channels[2].perlin.cellLength,
                channels[3].perlin.cellLength);

            layers = new Vector4(channels[0].perlin.layers,
                channels[1].perlin.layers,
                channels[2].perlin.layers,
                channels[3].perlin.layers);

            persistance = new Vector4(channels[0].perlin.persistance,
                channels[1].perlin.persistance,
                channels[2].perlin.persistance,
                channels[3].perlin.persistance);

            roughness = new Vector4(channels[0].perlin.roughness,
                channels[1].perlin.roughness,
                channels[2].perlin.roughness,
                channels[3].perlin.roughness);
        }

        protected void GetSliceC(DetailSettings detail,
            out Vector4 scale, out Vector4 cellLength, out Vector4 layers,
            out Vector4 persistance, out Vector4 roughness)
        {
            MixNoiseSettings[] channels = detail.channels;

            scale = new Vector4(channels[0].cellular.scale,
                channels[1].cellular.scale,
                channels[2].cellular.scale,
                channels[3].cellular.scale);

            cellLength = new Vector4(channels[0].cellular.cellLength,
                channels[1].cellular.cellLength,
                channels[2].cellular.cellLength,
                channels[3].cellular.cellLength);

            layers = new Vector4(channels[0].cellular.layers,
                channels[1].cellular.layers,
                channels[2].cellular.layers,
                channels[3].cellular.layers);

            persistance = new Vector4(channels[0].cellular.persistance,
                channels[1].cellular.persistance,
                channels[2].cellular.persistance,
                channels[3].cellular.persistance);

            roughness = new Vector4(channels[0].cellular.roughness,
                channels[1].cellular.roughness,
                channels[2].cellular.roughness,
                channels[3].cellular.roughness);
        }
    }
}
