using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ParticlesStopEvent : MonoBehaviour
{
    [SerializeField] UnityEvent m_onStop;

    private void OnParticleSystemStopped()
    {
        m_onStop.Invoke();
    }
}
