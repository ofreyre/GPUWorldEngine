using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RayData = RaysCollisionComputer.RayData;

[CreateAssetMenuAttribute(fileName = "RayDataEvent", menuName = "Scriptable Events / New RayDataEvent")]
public class RayDataEvent : ScriptableObject
{
    Action<RayData> m_listeners;

    public void Register(Action<RayData> listener)
    {
        m_listeners += listener;
    }

    public void Unregister(Action<RayData> listener)
    {
        m_listeners -= listener;
    }

    public void Dispatch(RayData rayData)
    {
        if(m_listeners != null)
        {
            m_listeners.Invoke(rayData);
        }
    }
}
