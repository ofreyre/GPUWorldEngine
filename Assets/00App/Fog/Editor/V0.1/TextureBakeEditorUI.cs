using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Texture3DBaker
{

    public class TextureBakeEditorUI
    {
        public void DrawGlobal(TextureBakeStyles styles, TextureBakerSettings esitorSettings, Action<Action<Texture>> Bake, Action<Texture> BakeReadyHandler)
        {
            EditorGUILayout.BeginHorizontal(styles.pannelStyleDetail);
            DrawDetailsVisibility(styles, esitorSettings);
            DrawChannelsVisibility(styles, esitorSettings);
            EditorGUILayout.EndHorizontal();
            DrawY(styles, esitorSettings);
            DrawButtons(styles, esitorSettings, Bake, BakeReadyHandler);

        }

        void DrawDetailsVisibility(TextureBakeStyles styles, TextureBakerSettings editorSettings)
        {
            //Details
            //EditorGUILayout.BeginVertical(GUILayout.Width(90));
            EditorGUILayout.BeginVertical(styles.pannelStyleVisibility);
            EditorGUILayout.LabelField("Show Details", styles.styleHeader1);

            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;
            for (int i = 0; i < editorSettings.details.Length; i++)
            {
                bool newVisible = EditorGUILayout.Toggle(editorSettings.details[i].detailName, editorSettings.details[i].visible
                    , GUILayout.Width(20), GUILayout.MinWidth(20), GUILayout.MaxWidth(20));
                if (newVisible != editorSettings.details[i].visible)
                {
                    editorSettings.SelectedDetailIndex = i;
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = prevLabelWidth;
        }

        void DrawChannelsVisibility(TextureBakeStyles styles, TextureBakerSettings esitorSettings)
        {
            float prevLabelWidth = EditorGUIUtility.labelWidth;

            EditorGUILayout.BeginVertical(styles.pannelStyleVisibility);
            EditorGUILayout.LabelField("Render Colors", styles.styleHeader1);
            EditorGUILayout.BeginHorizontal();

            EditorGUIUtility.labelWidth = 10;
            for (int i = 0; i < esitorSettings.renderColorChannels.Length; i++)
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(20));
                EditorGUILayout.LabelField(esitorSettings.colorNames[i], GUILayout.Width(20));
                esitorSettings.renderColorChannels[i] = EditorGUILayout.Toggle(esitorSettings.renderColorChannels[i], GUILayout.Width(20));
                EditorGUILayout.EndVertical();
            }
            EditorGUIUtility.labelWidth = prevLabelWidth;

            //EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUIUtility.labelWidth = prevLabelWidth;
        }

        void DrawY(TextureBakeStyles styles, TextureBakerSettings esitorSettings)
        {
            EditorGUILayout.BeginHorizontal(styles.pannelStyleDetail);
            DetailSettings ds = esitorSettings.SelectedDetail;
            ds.y = EditorGUILayout.Slider("Y", esitorSettings.SelectedY, 0, (float)ds.textureSize, styles.sliderWidth, styles.sliderWidth);
            EditorGUILayout.EndHorizontal();
        }

        void DrawButtons(TextureBakeStyles styles, TextureBakerSettings editorSettings, Action<Action<Texture>> BakeHandler, Action<Texture> BakeReadyHandler)
        {
            EditorGUILayout.BeginHorizontal(styles.pannelStyleDetail);
            /*if (GUILayout.Button("Render"))
            {
                RenderHandler.Invoke();
            }*/

            if (GUILayout.Button("Bake"))
            {
                BakeHandler.Invoke(BakeReadyHandler);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
