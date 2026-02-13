using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectChangeStateDispatcher : MonoBehaviour
{
    [SerializeField] UnityEvent m_OnAwake;
    [SerializeField] UnityEvent m_OnStart;
    [SerializeField] UnityEvent m_OnDestroy;
    [SerializeField] UnityEvent m_OnDisable;

    private void Awake()
    {
        m_OnAwake?.Invoke();
    }

    private void Start()
    {
        m_OnStart?.Invoke();
    }

    private void OnDisable()
    {
        m_OnDisable?.Invoke();
    }

    private void OnDestroy()
    {
        m_OnDestroy?.Invoke();
    }
}
