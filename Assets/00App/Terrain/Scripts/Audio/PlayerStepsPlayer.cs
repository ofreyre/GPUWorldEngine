using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DBG.Audio;

public class PlayerStepsPlayer : MonoBehaviour
{
    [SerializeField] Boid m_player;
    [SerializeField] float m_stepDistance = 0.5f;
    [SerializeField] AudioQeue m_steps;
    float m_distance;

    void Start()
    {
        
    }

    void Update()
    {
        m_distance += m_player.v.magnitude;
        if(m_distance > m_stepDistance)
        {
            AudioManager.instance.PlayQueue(m_steps);
        }
    }
}
