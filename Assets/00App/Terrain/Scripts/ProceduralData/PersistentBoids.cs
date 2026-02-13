using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DBG.Utils.IO;

[Serializable]
public class PersistentBoids
{
    public PersistentBoid[] boids;

    public PersistentBoids(PersistentBoid[] boids)
    {
        this.boids = boids;
    }

    public static void Save(List<Boid> boids, Vector2Int chunkCoords)
    {
        PersistentBoid[] persistentBoids = new PersistentBoid[boids.Count];
        for(int i=0;i< boids.Count;i++)
        {
            persistentBoids[i] = PersistentBoid.GetPersistentBoid(boids[i]);
        }

        UtilsIO.SaveParallel(new PersistentBoids(persistentBoids), "Boids/" + chunkCoords.x + "_" + chunkCoords.y);
    }

    public static bool BoidsExist(Vector2Int chunkCoords)
    {
        string relativePath = "Boids/" + chunkCoords.x + "_" + chunkCoords.y;
        return UtilsIO.FileExistParallel(relativePath);
    }

    public static PersistentBoid[] Load(Vector2Int chunkCoords)
    {
        string relativePath = "Boids/" + chunkCoords.x + "_" + chunkCoords.y;
        return UtilsIO.LoadParallel<PersistentBoids>(relativePath).boids;
    }
}
