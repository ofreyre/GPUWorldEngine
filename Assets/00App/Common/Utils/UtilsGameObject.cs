using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public struct ComponentSearchData<T> where T:Component
{
    public T component;
    public Transform transform;
    public int level;
}

public static class UtilsGameObject
{
    public static List<ComponentSearchData<T>> GetComponents<T>(GameObject gobj) where T:Component
    {
        return GetComponents<T>(gobj.transform);

    }

    public static List<ComponentSearchData<T>> GetComponents<T>(Transform transform) where T : Component
    {
        List<ComponentSearchData<T>> components = new List<ComponentSearchData<T>>();
        GetComponents<T>(transform, components, 0);
        return components;
    }

    static void GetComponents<T>(Transform transform, List<ComponentSearchData<T>> components, int level) where T : Component
    {
        T c = transform.GetComponent<T>();
        if(c != null)
        {
            components.Add(new ComponentSearchData<T>
            {
                component = c,
                transform = transform,
                level = level
            });
        }
        level++;
        foreach (Transform t in transform)
        {
            GetComponents<T>(t, components, level);
        }
    }

#if UNITY_EDITOR
    public static List<T> GetAnimationClips<T>(GameObject gameObject) where T: Object
    {
        List<T> result = new List<T>();
        string path = AssetDatabase.GetAssetPath(gameObject);
        var assetRepresentationsAtPath = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);
        foreach (var assetRepresentation in assetRepresentationsAtPath)
        {
            var obj = assetRepresentation as T;

            if (obj != null)
            {
                result.Add(obj);
            }
        }
        return result;
    }
#endif
}
