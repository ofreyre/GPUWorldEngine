using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetSaverUI
{
    public string SaveTexture(Object asset, string windowTitle, string path, string fileName)
    {
        path = EditorUtility.SaveFilePanel(windowTitle,
            path == "" || path == "" ?
            Application.dataPath : path,
            fileName, "");
        if (path == "")
        {
            return "";
        }

        int i = Application.dataPath.Length - "Assets".Length;
        try
        {
            string pathWithNoFilename = Path.GetDirectoryName(path);
            path = path.Substring(i);
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return pathWithNoFilename;
        }
        catch
        {
            return "";
        }
    }
}
