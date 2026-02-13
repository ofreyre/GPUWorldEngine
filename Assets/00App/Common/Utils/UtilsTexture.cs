using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UtilsTexture
{
    public static Texture2D MergeTextures2D(int rowTextures, int colTextures, Vector2Int texturesSize, Texture2D[] textures, TextureFormat textureFormat, bool mipmaps, FilterMode filterMode)
    {
        int width = colTextures * texturesSize.x;
        int height = rowTextures * texturesSize.y;
        Texture2D texture = new Texture2D(width, height, textureFormat, mipmaps);
        texture.filterMode = filterMode;
        for(int j=0;j< rowTextures;j++)
        {
            for (int i = 0; i < colTextures; i++)
            {
                int index = j * colTextures + i;
                if (index < textures.Length)
                {
                    texture.SetPixels(i * texturesSize.x, j * texturesSize.y, texturesSize.x, texturesSize.y, textures[index].GetPixels(), 0);
                }
            }
        }

        texture.Apply(true);

        return texture;
    }

    public static Texture2DArray ToTextureArray(int texturesWidth, int texturesHeight, Texture2D[] textures, TextureFormat format, bool mipmaps, FilterMode filter)
    {
        Texture2DArray textureArray = new Texture2DArray(texturesWidth, texturesHeight, textures.Length, format, mipmaps);
        textureArray.filterMode = filter;

        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }
}
