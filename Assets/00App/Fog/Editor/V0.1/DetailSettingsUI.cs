using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Texture3DBaker
{
    public class DetailSettingsUI
    {
        public void Draw(TextureBakeStyles styles, TextureBakerSettings editorSettings)
        {
            editorSettings.scrollPos = EditorGUILayout.BeginScrollView(editorSettings.scrollPos);
            for (int i = 0; i < editorSettings.details.Length; i++)
            {
                if (editorSettings.details[i].visible)
                {
                    EditorGUILayout.BeginVertical(styles.pannelStyleDetail);
                    DetailSettings settings = editorSettings.details[i];

                    settings.foldout = EditorGUILayout.Foldout(settings.foldout, settings.detailName, true, styles.StyleFoldout1);
                    if (settings.foldout)
                    {
                        EditorGUILayout.BeginVertical(styles.pannelStyleTextureSize);
                        int newTextureSize = EditorGUILayout.IntSlider("Texture size", settings.TextureSize, 8, 1024 / (i + 1), styles.sliderWidth, styles.sliderWidth);
                        if (newTextureSize != settings.TextureSize)
                        {
                            settings.TextureSize = UtilsMath.UpperPower2(newTextureSize);
                        }
                        EditorGUILayout.EndVertical();

                        for (int j = 0; j < settings.channels.Length; j++)
                        {
                            if (editorSettings.renderColorChannels[j])
                            {
                                DrawMixed(styles, settings.channels[j], editorSettings.colorNames[j]);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void DrawMixed(TextureBakeStyles styles, MixNoiseSettings settings, string header)
        {
            EditorGUILayout.BeginVertical(styles.pannelStyleChannel);
            settings.foldOut = EditorGUILayout.Foldout(settings.foldOut, header, true, styles.StyleFoldout2);
            if (settings.foldOut)
            {
                EditorGUILayout.BeginVertical(styles.pannelStyleNoise);
                settings.perlinWeight = EditorGUILayout.Slider("Perlin Weight", settings.perlinWeight, 0, 1, styles.sliderWidth, styles.sliderWidth);
                EditorGUILayout.EndVertical();

                settings.perlin = DrawNoise(styles, settings.perlin, "Perlin");
                settings.cellular = DrawNoise(styles, settings.cellular, "Cellular");
            }
            EditorGUILayout.EndVertical();
        }

        NoiseSettings DrawNoise(TextureBakeStyles styles, NoiseSettings settings, string header)
        {
            EditorGUILayout.BeginVertical(styles.pannelStyleNoise);
            EditorGUILayout.LabelField(header, styles.styleHeader1);

            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = styles.fieldLabelWidth;
            settings.scale = EditorGUILayout.FloatField("Scale", settings.scale, styles.sliderWidth, styles.sliderWidth);
            EditorGUIUtility.labelWidth = prevLabelWidth;

            float numberOfCells = settings.period / settings.cellLength;
            int newNumberOfCells = EditorGUILayout.IntSlider("Cells", (int)numberOfCells, 1, (int)settings.period, styles.sliderWidth, styles.sliderWidth);
            if (numberOfCells != newNumberOfCells)
            {
                float newCellLength = settings.period / newNumberOfCells;
                settings.cellLength = newCellLength;

                //float numberOfCells1 = (settings.period / settings.cellLength);
                //Debug.Log(newNumberOfCells + "  " + newCellLength + "  " + numberOfCells1+"  "+((int)numberOfCells1));
            }

            prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = styles.fieldLabelWidth;

            settings.layers = EditorGUILayout.IntField("Layers", settings.layers, styles.sliderWidth, styles.sliderWidth);
            settings.persistance = EditorGUILayout.FloatField("Persistance", settings.persistance, styles.sliderWidth, styles.sliderWidth);
            settings.roughness = EditorGUILayout.FloatField("Roughness", settings.roughness, styles.sliderWidth, styles.sliderWidth);

            EditorGUIUtility.labelWidth = prevLabelWidth;
            EditorGUILayout.EndVertical();
            return settings;
        }
    }
}
