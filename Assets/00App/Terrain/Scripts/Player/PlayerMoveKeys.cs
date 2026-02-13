using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerMoveKeys
{
    [SerializeField] Transform m_moveTransform;
    Vector2 m_seekVelocity;

    public bool GetMoveDirection(out Vector3 value)
    {
        bool down = false;

        int axisX = 0;
        int axisY = 0;
        if(Input.GetKey(KeyCode.D))
        {
            down = true;
            axisX = 1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            down = true;
            axisX = -1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            down = true;
            axisY = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            down = true;
            axisY = -1;
        }

        value = m_moveTransform.right * axisX + m_moveTransform.forward * axisY;

        return down;
    }

    public Vector2 GetSeek(float maxForce, float maxSpeed)
    {
        Vector3 moveDirection;
        bool keyDown = GetMoveDirection(out moveDirection);
        if (keyDown)
        {
            Vector2 direction = (new Vector2(moveDirection.x, moveDirection.z)).normalized * maxSpeed;
            Vector2 steer = (direction - m_seekVelocity).normalized * maxForce;
            m_seekVelocity += steer;
        }
        else
        {
            if (m_seekVelocity.magnitude > 0.1f)
            {
                m_seekVelocity *= 0.5f;
            }
            else
            {
                m_seekVelocity = Vector3.zero;
            }
        }

        return m_seekVelocity;
    }
}
