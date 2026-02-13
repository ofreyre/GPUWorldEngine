using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace DBG.VATEditor
{
    public class VATEditor : EditorWindow
    {
        [MenuItem("Tools/Bake animations to textures")]
        static void ShowEditor()
        {
            //create window
            VATEditor window = EditorWindow.GetWindow<VATEditor>();
            window.Show();
        }

        GameObject m_animationsGameObject;
        PopupWindowContent m_messagePopup;
        Baker m_baker;
        int m_fps = 16;

        private void Awake()
        {
            m_messagePopup = new PopupMessage(new Vector2(250, 250), "The assigned object don't have an Animator component");
            m_baker = new Baker();
        }

        void OnGUI()
        {
            GUILayout.BeginVertical();
            GameObject newGobj = (GameObject)EditorGUILayout.ObjectField("Animations Object", m_animationsGameObject, typeof(GameObject), false);
            m_fps = EditorGUILayout.IntSlider("FPS", m_fps, 8, 60);
            if (newGobj != null && newGobj != m_animationsGameObject)
            {
                if (newGobj.GetComponent<SkinnedMeshRenderer>() != null && newGobj.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                {
                    m_animationsGameObject = newGobj;
                }
                else
                {
                    PopupWindow.Show(new Rect((position.width - 250) * 0.5f, (position.height - 250) * 0.5f, 250, 250), m_messagePopup);
                }
            }

            if (m_animationsGameObject != null)
            {
                if (GUILayout.Button("Bake"))
                {
                    m_baker.Bake(m_animationsGameObject, m_fps);
                }
            }
            GUILayout.EndVertical();
        }
    }
}
