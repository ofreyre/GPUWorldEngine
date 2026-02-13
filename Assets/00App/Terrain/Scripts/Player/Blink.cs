using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blink : MonoBehaviour
{
    [SerializeField] GameObject m_gameObject;
    [SerializeField] float m_minOn;
    [SerializeField] float m_maxOn;
    [SerializeField] float m_minOff;
    [SerializeField] float m_maxOff;
    float m_nextT;

    void Update()
    {
        if(m_gameObject.activeSelf)
        {
            if(Time.time > m_nextT)
            {
                m_gameObject.SetActive(false);
                m_nextT = Time.time + Random.Range(m_minOff, m_maxOff);
            }
        }
        else
        {
            if (Time.time > m_nextT)
            {
                m_gameObject.SetActive(true);
                m_nextT = Time.time + Random.Range(m_minOn, m_maxOn);
            }
        }
    }
}
