using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerRotateMouse
{
    [SerializeField] Transform m_moveTransform;
    [SerializeField] Transform m_seeTransform;
    [SerializeField] Transform m_gun;
    [SerializeField] Camera m_camera; 
    [SerializeField] float m_sensitivity = 1;
    float rotationX;

    public void Update(float deltaTime)
    {
        float deltaX = Input.GetAxis("Mouse X") * m_sensitivity * deltaTime;
        float deltaY = Input.GetAxis("Mouse Y") * m_sensitivity * deltaTime;
        rotationX -= deltaY;
        m_moveTransform.Rotate(Vector3.up * deltaX);
        m_seeTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        Vector3 destination = m_camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 1));
        //m_gun.LookAt(destination);
    }
}
