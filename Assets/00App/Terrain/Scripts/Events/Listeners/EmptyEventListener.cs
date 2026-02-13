using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EmptyEventListener : MonoBehaviour
{
    [SerializeField] EmptyEvent m_event;
    [SerializeField] UnityEvent m_listeners;

    private void Awake()
    {
        m_event.Register(Dispatch);
    }

    void Dispatch()
    {
        m_listeners.Invoke();
    }
}
