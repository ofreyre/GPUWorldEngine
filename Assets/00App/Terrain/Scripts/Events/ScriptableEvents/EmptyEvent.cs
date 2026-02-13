using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenuAttribute(fileName = "RayDataEvent", menuName = "Scriptable Events / New EmptyEvent")]
public class EmptyEvent : ScriptableObject
{
    [SerializeField]Action m_listeners;

    public void Register(Action listener)
    {
        m_listeners += listener;
    }

    public void Unregister(Action listener)
    {
        m_listeners -= listener;
    }

    public void Dispatch()
    {
        if (m_listeners != null)
        {
            m_listeners.Invoke();
        }
    }
}
