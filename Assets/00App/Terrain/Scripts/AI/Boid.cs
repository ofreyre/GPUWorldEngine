using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoidBase = BoidsComputer.BoidBase;
using BoidResult = BoidsComputer.BoidResult;
using AnimatedModel;

public class Boid : MonoBehaviour
{
    public int type;
    public float r;
    public float maxSpeed;
    public float maxForce;
    public float seekWeight;

    public Vector2 v;
    public Vector2 a;
    public Vector2Int gridCoords;
    public Vector2Int chunkCoords;
    public Chunk m_chunk;

    public BoidsInitComputer.Cyllinder m_collider;

    public float m_stamina;

    public Vector3 m_position;
    public Vector3 m_eulerAngles;
    public Vector3 m_localScale;
    public ModelAnimator m_animator;
    

    public void SetProperties(float r, float maxSpeed, float maxForce, float seekWeight)
    {
        this.r = r;
        this.maxSpeed = maxSpeed;
        this.maxForce = maxForce;
        this.seekWeight = seekWeight;
        m_localScale = transform.localScale;
    }

    public virtual BoidBase boidBase
    {
        get
        {
            //Debug.Log("get baseV = " + v);
            return new BoidBase
            {
                pos = transform.position,
                r = r,
                v = v,
                maxSpeed = maxSpeed,
                maxForce = maxForce,
                seekWeight = seekWeight
            };
        }
    }

    public virtual BoidResult boidResult
    {
        get
        {
            return new BoidResult
            {
                //pos = transform.position,
                pos = m_position,
                v = v,
                gridCoords = gridCoords,
                chunkCoords = chunkCoords
            };
        }

        set
        {
            m_position = value.pos;
            transform.position = value.pos;
            v = value.v;
            gridCoords = value.gridCoords;
            chunkCoords = value.chunkCoords;
            transform.LookAt(value.pos + new Vector3(v.x, 0, v.y), Vector3.up);
            m_eulerAngles = transform.eulerAngles;
        }
    }

    public BoidsInitComputer.BoidSettingsResult boidSettingsResult
    {
        set
        {
            transform.position = value.pos;
            r = value.r;
            transform.localScale = new Vector3(value.scale, value.scale, value.scale);
            maxSpeed = value.maxSpeed;
            maxForce = value.maxForce;
            seekWeight = value.seekWeight;
            gridCoords = value.gridPos;
            chunkCoords = value.chunkCoords;
            m_stamina = value.stamina;
        }
    }

    public void ApplyDamage(float value)
    {
        m_stamina -= value;
        if(m_stamina <= 0)
        {
            //Debug.Log("***************** Boid.ApplyDamage: remove " + m_chunk.m_boids.IndexOf(this)+ "  chunk "+m_chunk.m_position);
            m_chunk.RemoveBoid(this);
            Destroy(gameObject);
        }
    }

    public static Boid GetBoid(PersistentBoid persistentBoid, BoidSettings[] boidsSettings)
    {
        GameObject gobj = GameObject.Instantiate(boidsSettings[persistentBoid.type].prefab);
        //gobj.transform.SetParent(m_gameObject.transform);
        Boid boid = gobj.AddComponent<Boid>();
        boid.r = persistentBoid.r;
        boid.maxSpeed = persistentBoid.maxSpeed;
        boid.maxForce = persistentBoid.maxForce;
        boid.seekWeight = persistentBoid.seekWeight;
        boid.m_stamina = persistentBoid.stamina;
        boid.transform.position = persistentBoid.position;
        boid.transform.eulerAngles = persistentBoid.eulerAngles;
        boid.transform.localScale = persistentBoid.localScale;
        boid.m_collider = persistentBoid.collider;

        return boid;
    }
}