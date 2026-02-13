using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayData = RaysCollisionComputer.RayData;
using System;
using RayCollision = RaysCollisionComputer.RayCollision;

using UnityEditor;

[Serializable]
public class RaysManager
{
    Pool m_shotsPool;
    Pool m_bloodPool;
    Pool m_treeParticlePool;
    List<RayData> raysData = new List<RayData>();
    List<Boid> m_boids = new List<Boid>();
    RaysCollisionComputer m_computer;
    TerrainSettings m_settings;
    Action m_callbackReady;
    Action<int> m_callbackBoidDamaged;

    public Transform m_testP1;
    int boidsLength;

    public RaysManager(TerrainSettings settings, Action callbackReady, Action<int> callbackBoidDamaged, Pool shotsPool, Pool bloodPool, Pool treeParticlesPool)
    {
        m_shotsPool = shotsPool;
        m_bloodPool = bloodPool;
        m_treeParticlePool = treeParticlesPool;
        m_settings = settings;
        m_callbackReady = callbackReady;
        m_callbackBoidDamaged = callbackBoidDamaged;
        m_computer = new RaysCollisionComputer(m_settings);
    }

    public void AddRay(RayData rayData)
    {
        raysData.Add(rayData);
    }

    public void ComputeRays(ComputeBuffer boidBasesBuffer,
        ComputeBuffer gridBoidsMapBuffer,
        ComputeBuffer gridStartBuffer,
        BoidsInitComputer.Cyllinder[] boidColliders,
        ComputeBuffer treesGridBuffer,
        ComputeBuffer treesCollidersBuffer,
        int gridBoidsMapLength,
        int boidsLength,
        RectInt boidsRect
    )
    {
        //Debug.Log("RaysManager.ComputeRays");
        this.boidsLength = boidsLength;
        if (raysData.Count > 0)
        {
            m_computer.InitBuffers(
                boidBasesBuffer,
                gridBoidsMapBuffer,
                gridStartBuffer,
                boidColliders,
                treesGridBuffer,
                treesCollidersBuffer,
                raysData
            );

            m_computer.Compute(boidsRect, gridBoidsMapLength, boidsLength, ComputeReady);
        }
        else
        {
            ComputeReady();
        }
    }

    void ComputeReady()
    {
        SpawnShots();
        raysData.Clear();
        m_callbackReady.Invoke();
    }

    void SpawnShots()
    {
        if (raysData.Count > 0)
        {
            RayCollision[] rays = m_computer.rays;
            for (int i = 0; i < rays.Length; i++)
            {
                SpawsShot(raysData[i], rays[i]);
            }
        }
    }

    public void SpawsShot(RayData rayData, RayCollision collision)
    {
        //GameObject shot = m_shotsPool.Get();
        //Transform ray = shot.transform.GetChild(0);
        Vector3 p1 = collision.boidI != m_settings.EMPTY && collision.boidI > 0 ? collision.p : rayData.p1;
        //ray.localScale = new Vector3(ray.localScale.x, ray.localScale.y, Vector3.Magnitude(p1 - rayData.p0));
        
        //shot.transform.position = rayData.p0;
        //shot.transform.LookAt(p1);
        //shot.SetActive(true);
        //m_testP1.position = p1;
        //t_p0 = rayData.p0;
        //t_p1 = p1;

        //Debug.Log("ddddddd "+collision.boidI+"   "+ boidsLength+"  "+ m_settings.EMPTY);
        if (collision.boidI != m_settings.EMPTY && collision.boidI != 0)
        {
            if (collision.boidI < boidsLength)
            {
                //Debug.Log(collision.boidI + "   " + boidsLength + "  " + m_settings.EMPTY);
                GameObject particle = m_bloodPool.Get();
                particle.transform.position = p1;
                particle.transform.LookAt(p1 + collision.n);
                particle.SetActive(true);
                m_callbackBoidDamaged.Invoke(collision.boidI);
            }
            else
            {
                GameObject particle = m_treeParticlePool.Get();
                particle.transform.position = p1;
                particle.transform.LookAt(p1 + collision.n);
                particle.SetActive(true);
            }
        }

        //EditorApplication.isPaused = true;
    }

    public void OnDestroy()
    {
        if (m_computer != null)
            m_computer.Release();
    }

    Vector3 t_p0, t_p1;

    public void OnDrawGizmos()
    {
        var gizmoColor = Gizmos.color;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(t_p0, t_p1);
        Gizmos.color = gizmoColor;

        /*
        if (m_computer != null)
        {
            m_computer.OnDrawGizmo();
        }
        */
    }
}