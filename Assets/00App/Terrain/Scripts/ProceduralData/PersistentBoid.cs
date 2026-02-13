using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DBG.Utils.Persistence;

[Serializable]
public class PersistentBoid
{
    public int type;
    public float r;
    public float maxSpeed;
    public float maxForce;
    public float seekWeight;
    public S_Vector3 position;
    public S_Vector3 eulerAngles;
    public S_Vector3 localScale;

    public BoidsInitComputer.Cyllinder collider;

    public float stamina;

    public static PersistentBoid GetPersistentBoid(Boid boid)
    {
        return new PersistentBoid
        {
            type = boid.type,
            r = boid.r,
            maxSpeed = boid.maxSpeed,
            maxForce = boid.maxForce,
            seekWeight = boid.seekWeight,
            collider = boid.m_collider,
            stamina = boid.m_stamina,
            //position = boid.transform.position,
            position = boid.m_position,
            //eulerAngles = boid.transform.eulerAngles,
            eulerAngles = boid.m_eulerAngles,
            //localScale = boid.transform.localScale
            localScale = boid.m_localScale
        };
    }
}
