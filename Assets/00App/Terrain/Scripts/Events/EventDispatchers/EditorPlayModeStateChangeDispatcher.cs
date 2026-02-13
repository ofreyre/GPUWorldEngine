using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EditorPlayModeStateChangeDispatcher : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] UnityEvent m_OnEnteredEditMode;
    [SerializeField] UnityEvent m_OnEnteredPlayMode;
    [SerializeField] UnityEvent m_OnExitingEditMode;
    [SerializeField] UnityEvent m_OnExitingPlayMode;

    // Start is called before the first frame update
    void Start()
    {
        EditorApplication.playModeStateChanged += Dispatch;
    }

    // Update is called once per frame
    void Dispatch(PlayModeStateChange state)
    {
        switch(state)
        {
            case PlayModeStateChange.EnteredEditMode:
                m_OnEnteredEditMode?.Invoke();
                break;
            case PlayModeStateChange.EnteredPlayMode:
                m_OnEnteredPlayMode?.Invoke();
                break;
            case PlayModeStateChange.ExitingEditMode:
                m_OnExitingEditMode?.Invoke();
                break;
            default:
                m_OnExitingPlayMode?.Invoke();
                break;
        }
    }
#endif
}
