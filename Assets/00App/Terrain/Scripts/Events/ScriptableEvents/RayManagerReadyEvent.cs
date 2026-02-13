using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenuAttribute(fileName = "RayManagerReadyEvent", menuName = "Scriptable Events / New RayManagerReadyEvent")]
public class RayManagerReadyEvent : ScriptableObject
{
    Action<bool> m_listeners;

    public void Register(Action<bool> listener)
    {
        m_listeners += listener;
    }

    public void Unregister(Action<bool> listener)
    {
        m_listeners -= listener;
    }

    public void Dispatch(bool rayData)
    {
        if (m_listeners != null)
        {
            m_listeners.Invoke(rayData);
        }
    }
}
