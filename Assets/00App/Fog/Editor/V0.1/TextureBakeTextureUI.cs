using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace Texture3DBaker
{
    public class TextureBakeTextureUI
    {
        TextureBakerSettings editorSettings;
        TextureBakeStyles styles;

        public TextureBakeTextureUI(TextureBakerSettings editorSettings, TextureBakeStyles styles)
        {
            this.editorSettings = editorSettings;
            this.styles = styles;
        }

        public void Draw(Texture2D texture)
        {
            if (texture != null)
            {
                float width = editorSettings.windowRect.width - styles.settingsWidth - 40;
                float height = editorSettings.windowRect.height - 20;
                float length = Mathf.Min(width, height);
                float x = styles.settingsWidth + 20 + (width - length) * 0.5f;
                float y = 10 + (height - length) * 0.5f;
                EditorGUI.DrawPreviewTexture(new Rect(x, y, length, length), texture);
            }
        }
    }
}
