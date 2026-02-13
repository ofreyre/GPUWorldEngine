using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;


namespace Texture3DBaker
{
    public class TextureBakerEditor : EditorWindow
    {
        [MenuItem("Tools/Bake noise to texture")]
        static void ShowEditor()
        {
            //create window
            TextureBakerEditor window = EditorWindow.GetWindow<TextureBakerEditor>();
            window.Show();
        }

        TextureBakerSettings editorSettings;
        ITextureComputer renderer;
        ITextureComputer baker;

        TextureBakeEditorUI editorUI;
        DetailSettingsUI detailsUI;
        TextureBakeTextureUI textureUI;
        AssetSaverUI saverUI;

        TextureBakeStyles styles;
        Texture renderTexture;

        private void Awake()
        {
            if (styles == null)
            {
                styles = new TextureBakeStyles();
            }

            if (editorSettings == null)
            {
                editorSettings = AssetDatabase.LoadAssetAtPath<TextureBakerSettings>("Assets/00App/Fog/Editor/v0.1/Resources/EditorSettings.asset");
                if (editorSettings == null)
                {
                    editorSettings = CreateInstance<TextureBakerSettings>();
                    AssetDatabase.CreateAsset(editorSettings, "Assets/00App/Fog/Editor/v0.1/Resources/EditorSettings.asset");
                    AssetDatabase.SaveAssets();
                }

            }

            if (editorUI == null)
                editorUI = new TextureBakeEditorUI();

            if (detailsUI == null)
                detailsUI = new DetailSettingsUI();

            if (textureUI == null)
                textureUI = new TextureBakeTextureUI(editorSettings, styles);

            if (renderer == null)
                renderer = new RendererMixed(editorSettings, editorSettings.computeRender, editorSettings.computeRenderKernel);

            if (baker == null)
                baker = new Baker(editorSettings, editorSettings.computeBake, editorSettings.computeBakeKernel);

            if (saverUI == null)
                saverUI = new AssetSaverUI();
        }


        void OnGUI()
        {
            float prevLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = styles.labelWidth;
            float prevfieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = styles.fieldWidth;

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical(GUILayout.Width(styles.settingsWidth));
            editorSettings.windowRect = position;
            editorUI.DrawGlobal(styles, editorSettings, baker.Run, SaveTexture);
            detailsUI.Draw(styles, editorSettings);
            GUILayout.EndVertical();

            renderer.Run(RenderReady);
            textureUI.Draw((Texture2D)renderTexture);
            GUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = prevLabelWidth;
            EditorGUIUtility.fieldWidth = prevfieldWidth;
        }

        void RenderReady(Texture texture)
        {
            renderTexture = texture;
        }

        void SaveTexture(Texture texture)
        {
            Texture3D tex = (Texture3D)texture;
            bool isHeigh = editorSettings.SelectedDetailIndex == 0;
            string path = saverUI.SaveTexture(tex, "Save Texture3D", editorSettings.lastSavePath, isHeigh ? "HeighDetails" : "LowDetails");
            
            EditorUtility.FocusProjectWindow();

            if (path != null && path != "")
            {
                editorSettings.lastSavePath = path;
                EditorUtility.SetDirty(editorSettings);
            }
        }

        public static bool AllFilled
        {
            get { return true; }
        }

        private void OnDestroy()
        {
            EditorUtility.SetDirty(editorSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            renderer.Release();
            baker.Release();
        }
    }
}
