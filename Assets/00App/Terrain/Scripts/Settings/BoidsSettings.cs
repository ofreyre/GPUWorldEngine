using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AnimatedModel;

[Serializable]
public class BoidSettings
{
    public GameObject prefab;
    public float maxSpeed;
    public float maxForce;
    [Range(0.0f, 1.0f)]public float seekWeight;
    public float r;
    public float minScale;
    public float maxScale;
    public float spawnProb;
    public float stamina;
    public BoidsInitComputer.Cyllinder collider;
}

[Serializable]
public class BoidsSettings
{
    public ComputeShader bitomicShader;
    public string bitomicKernel;
    public ComputeShader startArrayShader;
    public string startArrayKernel;
    public ComputeShader boidsInitShader;
    public string boidsInitKernel;
    public ComputeShader boidsShader;
    public string boidsKernel;
    public float desireSeparation;
    public int boidsCellWidthInChunkCells;
    public int boidsPerceptionInBoidsCells;

    public float bakeFPS;
    public string bakeSavePath;
    public float minTerrainHeight;
    public float maxTerrainHeight;
    public int minBoidSpacing = 3;
    public int maxBoidSpacing = 4;
    public float minCos = 0.8f;
    public BoidSettings[] boids;

    public BoidSettings GetBoidSettings(System.Random prng)
    {
        float prob = (float)prng.NextDouble();
        float spawnProb = 0;
        for (int i = 0; i < boids.Length; i++)
        {
            BoidSettings boidSettings = boids[i];
            spawnProb += boidSettings.spawnProb;
            if (prob < spawnProb)
            {
                return boidSettings;
            }
        }
        return null;
    }

    public GameObject GetBoidGameObject(Vector3 position, System.Random prng)
    {
        BoidSettings boidSettings = GetBoidSettings(prng);
        if (boidSettings == null)
            return null;

        float rnd = (float)prng.NextDouble();
        float scale = boidSettings.minScale * rnd + boidSettings.maxScale * (1 - rnd);
        rnd = (float)prng.NextDouble();
        float angle = Mathf.PI * 2f * rnd;
        Vector3 up = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
        rnd = (float)prng.NextDouble();
        angle = rnd * Mathf.PI * 2;
        Vector3 forward = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
        GameObject gobj = GameObject.Instantiate(boidSettings.prefab, position, Quaternion.LookRotation(forward, up));
        gobj.transform.localScale = new Vector3(scale, scale, scale);
        Boid boid = gobj.AddComponent<Boid>();
        boid.SetProperties(boidSettings.r * scale, boidSettings.maxSpeed, boidSettings.maxForce, boidSettings.seekWeight);
        boid.m_animator = gobj.GetComponent<ModelAnimator>();
        boid.m_animator.enabled = false;

        return gobj;
    }

    public void GetBoidsInitData(out BoidsInitComputer.BoidInitSettings[] boidInitSettings, out BoidsInitComputer.Cyllinder[] collisionInitSettings)
    {
        boidInitSettings = new BoidsInitComputer.BoidInitSettings[boids.Length];
        collisionInitSettings = new BoidsInitComputer.Cyllinder[boids.Length];
        for (int i=0;i< boids.Length; i++)
        {
            BoidSettings boid = boids[i];
            boidInitSettings[i] = new BoidsInitComputer.BoidInitSettings
            {
                maxSpeed = boid.maxSpeed,
                maxForce = boid.maxForce,
                seekWeight = boid.seekWeight,
                r = boid.r,
                minScale = boid.minScale,
                maxScale = boid.maxScale,
                spawnProb = boid.spawnProb,
                stamina = boid.stamina
            };

            collisionInitSettings[i] = boid.collider;
        }
    }
}
