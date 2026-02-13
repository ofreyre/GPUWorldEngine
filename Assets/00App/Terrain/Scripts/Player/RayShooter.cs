using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RayData = RaysCollisionComputer.RayData;

[Serializable]
public class RayShooter
{
    [SerializeField] Camera m_camera;
    [SerializeField] float m_shootRest = 0.3f;
    [SerializeField] float m_range = 10;
    [SerializeField] float m_damage = 1;
    [SerializeField] RayDataEvent m_rayDataEvent;
    [SerializeField] Transform m_rayOrigin;

    float m_shootT;

    public void Shoot()
    {
        float time = Time.time;
        if (m_shootT + m_shootRest < time)
        {
            m_shootT = time;
            RayData rayData = RayData.GetRayToScreenCenter(m_camera, m_rayOrigin.position, m_range, m_damage);
            m_rayDataEvent.Dispatch(rayData);
        }
    }
}
