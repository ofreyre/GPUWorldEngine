using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

    public class PopupMessage : PopupWindowContent
    {
        string message;
        Vector2 size;
        public int buttonPressed;

        public PopupMessage(Vector2 size, string message)
        {
            this.size = size;
            this.message = message;
        }

        public override Vector2 GetWindowSize()
        {
            return size;
        }

        public override void OnGUI(Rect rect)
        {
            GUILayout.Label(message, EditorStyles.boldLabel);
        }
    }
