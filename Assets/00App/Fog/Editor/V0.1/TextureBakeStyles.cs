using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Texture3DBaker
{
    public class TextureBakeStyles
    {
        public float settingsWidth = 250;
        public float labelWidth = 80;
        public float fieldLabelWidth = 157;
        public float fieldWidth = 40;
        public GUILayoutOption sliderWidth = GUILayout.Width(195);
        public GUIStyle pannelStyleVisibility = new GUIStyle();
        public GUIStyle pannelStyleNoise = new GUIStyle();
        public GUIStyle pannelStyleDetail = new GUIStyle();
        public GUIStyle pannelStyleChannel = new GUIStyle();
        public GUIStyle pannelStyleTextureSize = new GUIStyle();

        public GUIStyle styleHeader1 = new GUIStyle();
        public GUIStyle styleFoldout1;
        public GUIStyle styleFoldout2;

        public Texture2D defaultTexture;

        public GUIStyle StyleFoldout1
        {
            get
            {
                if (styleFoldout1 == null)
                {
                    styleFoldout1 = new GUIStyle(EditorStyles.foldout);
                    styleFoldout1.alignment = TextAnchor.MiddleCenter;
                    styleFoldout1.fontStyle = FontStyle.Bold;
                    styleFoldout1.normal.textColor = new Color(1, 1, 1, 1);
                    styleFoldout1.fixedWidth = 200;
                    styleFoldout1.fontSize = 14;
                }
                return styleFoldout1;
            }
        }

        public GUIStyle StyleFoldout2
        {
            get
            {
                if (styleFoldout2 == null)
                {
                    styleFoldout2 = new GUIStyle(EditorStyles.foldout);
                    styleFoldout2.alignment = TextAnchor.MiddleLeft;
                    styleFoldout2.fontStyle = FontStyle.Bold;
                    styleFoldout2.normal.textColor = new Color(1, 1, 1, 1);
                    styleFoldout2.fixedWidth = 195;
                    styleFoldout2.fontSize = 12;
                }
                return styleFoldout2;
            }
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();

            return result;
        }

        public TextureBakeStyles()
        {
            pannelStyleVisibility.normal.background = MakeTex(1, 1, new Color(0.1f, 0.1f, 0.1f, 1f));
            pannelStyleVisibility.fixedWidth = 100;
            pannelStyleVisibility.margin = new RectOffset(5, 5, 5, 5);

            pannelStyleNoise.normal.background = pannelStyleVisibility.normal.background;
            pannelStyleNoise.fixedWidth = 205;
            pannelStyleNoise.margin = new RectOffset(5, 5, 5, 5);

            pannelStyleDetail.normal.background = MakeTex(1, 1, new Color(0.18f, 0.18f, 0.18f, 1f));
            pannelStyleDetail.fixedWidth = 225;
            pannelStyleDetail.margin = new RectOffset(5, 5, 5, 5);

            pannelStyleChannel.normal.background = MakeTex(1, 1, new Color(0.14f, 0.14f, 0.14f, 1f));
            pannelStyleChannel.fixedWidth = 215;
            pannelStyleChannel.margin = new RectOffset(5, 5, 5, 5);

            pannelStyleTextureSize.normal.background = pannelStyleVisibility.normal.background;
            pannelStyleTextureSize.fixedWidth = 215;
            pannelStyleTextureSize.margin = new RectOffset(5, 5, 5, 5);

            styleHeader1.alignment = TextAnchor.MiddleCenter;
            styleHeader1.fontStyle = FontStyle.Bold;
            styleHeader1.normal.textColor = new Color(1, 1, 1, 1);

            defaultTexture = MakeTex(1, 1, new Color(0.14f, 0.14f, 0.14f, 1f));
        }
    }
}
