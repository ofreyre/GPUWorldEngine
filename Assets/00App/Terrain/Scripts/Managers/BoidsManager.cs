using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AnimatedModel;

public class BoidsManager
{
    TerrainSettings m_settings;
    Chunk[] m_chunks;
    public BoidsComputer m_boidsComputer;
    Action m_callbackBoidsReady;
    bool m_ready;
    RectInt m_chunksRect;

    public List<Vector2Int> m_gridBoidsMap = new List<Vector2Int>();
    public List<Boid> m_activeBoids = new List<Boid>();
    public BoidsComputer.BoidBase[] m_boidBases = new BoidsComputer.BoidBase[100];
    public BoidsInitComputer.Cyllinder[] m_boidColliders = new BoidsInitComputer.Cyllinder[100];
    public CyllinderColliderData[] m_treesColliders = new CyllinderColliderData[100];
    public int[] m_treesGrid;
    bool m_destroyed;

    public BoidsManager(TerrainSettings settings, Chunk[] chunks, Action callbackBoidsReady)
    {
        m_settings = settings;
        m_chunks = chunks;
        m_callbackBoidsReady = callbackBoidsReady;
    }

    public void ComputeBoids(float deltaTime, Vector2 center, RectInt boidsRect, RectInt collisionRect, Boid player, MeshComputer.MeshCollisionData[] meshCollisionData)
    {
        m_ready = false;
        UpdateActiveBoids(boidsRect, player);

        if (m_boidsComputer == null)
        {
            m_boidsComputer = new BoidsComputer(m_settings);
        }
        m_boidsComputer.InitBuffers(boidsRect, collisionRect, m_gridBoidsMap, m_boidBases, meshCollisionData, m_treesGrid, m_treesColliders);
        m_boidsComputer.Compute(deltaTime, center, 0.5f, BoidsReady);
    }

    void BoidsReady()
    {
        if (m_destroyed) return;
        UpdateBoids();
        m_ready = true;
        m_callbackBoidsReady.Invoke();
    }

    public bool Ready { get { return m_ready; } }

    public void Update(float deltaTime, Vector2 center, RectInt boidsRect, RectInt collisionRect, RectInt chunksRect, Boid player, MeshComputer.MeshCollisionData[] meshCollisionData)
    {
        if(m_ready)
        {
            //Debug.Log("BoidsManager.Update");
            m_chunksRect = chunksRect;
            ComputeBoids(deltaTime, center, boidsRect, collisionRect, player, meshCollisionData);
        }
    }

    void UpdateBoids()
    {
        //Debug.Log("BoidsManager.UpdateBoids");
        //Debug.Log("*************** UpdateBoidsChunk m_boidsComputer.boidsCount = " + m_boidsComputer.boidsCount);
        //int c = 0;

        for (int i = 0; i < m_boidsComputer.boidsCount; i++)
        {
            //bool processed = true;
            Boid boid = m_activeBoids[i];

            if (i < 0)
                boid.m_animator.nextState = ModelAnimator.STATE.idle;

            if (boid == null)
            {
                Debug.Log("Null boid");
                continue;
            }

            //boid.transform.GetChild(0).GetComponent<Renderer>().material = m_activeBoidsMaterial;

            BoidsComputer.BoidResult result = m_boidsComputer.boidsResult[i];
            /*
            if (m_boidsComputer.boidChunkChangeResult[i] == 1)
            {
                Debug.Log(boid.transform.position + "   " + result.pos + "   ");
                Debug.Log(result.ToString());
            }
            */
            if (boid.chunkCoords != result.chunkCoords && i > 0)
            {
                Vector2Int chunkCoords = new Vector2Int(
                    (int)(result.pos.x / m_settings.MeshSideLength),
                    (int)(result.pos.z / m_settings.MeshSideLength)
                    );
                
                if (chunkCoords != result.chunkCoords)
                {
                    Debug.Log("uuuuuuuuuuuuuu "
                        + "  boid.chunkCoords = " + boid.chunkCoords
                        + "  boid.transform.position = " + boid.transform.position
                        + "  result.chunkCoords = " + result.chunkCoords
                        + "  result.pos = " + result.pos
                        + "  chunkCoords = " + chunkCoords
                        + "  m_settings.MeshSideLength = " + m_settings.MeshSideLength
                        + "  result.gridCoords = " + result.gridCoords
                        + "  chunkWorldSize = " + m_boidsComputer.chunkWorldSize
                        + "  starmina = " + boid.m_stamina
                        );
                }
                Vector2Int chunkCoord = boid.chunkCoords - m_chunksRect.min;
                int boidChunkIndex = chunkCoord.y * m_chunksRect.width + chunkCoord.x;
                chunkCoord = result.chunkCoords - m_chunksRect.min;
                int boidresultChunkIndex = chunkCoord.y * m_chunksRect.width + chunkCoord.x;

                //Debug.Log(string.Join(",", m_boidsComputer.boidsResult));
                //Debug.Log(m_chunksRect.min + " " + boid.chunkCoords + "  " + result.chunkCoords + " " + chunkCoord+ "  boidresultChunkIndex = " + boidresultChunkIndex+ "   pos = "+ result.pos+"   i = "+i+"   "+ m_boidsComputer.boidsCount);

                if (boidresultChunkIndex > 0)
                {
                    m_chunks[boidChunkIndex].RemoveBoid(boid);
                    m_chunks[boidresultChunkIndex].AddBoid(boid);
                    if(m_chunks[boidresultChunkIndex].m_chunkCoords != result.chunkCoords)
                    {
                        string coords = "";
                        Debug.Log("*****************************");
                        for(int k = 0; k < m_chunks.Length; k++)
                        {
                            coords += m_chunks[k].m_chunkCoords + " ";
                        }
                        Debug.Log("coorrd = " + coords);
                        Debug.Log("local chunkCoord = "+ chunkCoord);
                        Debug.Log(m_chunksRect.min + "   " + result.chunkCoords + "  " + m_chunks[boidresultChunkIndex].m_chunkCoords);
                    }
                    //boid.m_chunk = m_chunks[boidresultChunkIndex];
                }
                
                /*else
                {
                    Debug.Log("boid i = " + i);
                    Debug.Log(boid.boidResult);
                    Debug.Log(result);
                    Debug.Log(m_boidsComputer.boidBases[i]);

                    LogBoids();
                    m_boidsComputer.Log();
                }
                */

                /*
#if UNITY_EDITOR
                else
                {
                    processed = false;
                    boid.transform.GetChild(0).GetComponent<Renderer>().material = m_notProcessedBoidsMaterial;
                    c++;
                    //Debug.Log(boidresultChunkIndex + "   " + m_chunksRect + "   " + result);
                }
#endif
                */
            }
            //if(processed)
            if(float.IsNaN(result.pos.y))
            {
                Debug.Log(m_boidsComputer.GetMeshY(new Vector2(result.pos.x, result.pos.z)));
            }
            boid.boidResult = result;
        }

        /*
#if UNITY_EDITOR
        if (c > 0)
        {
            Debug.Log("··· c = " + c);
            m_boidsComputer.Log();
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPaused = true;
            }
        }
#endif
        */
    }

    void ClearTreesGrid(RectInt rect)
    {
        if(m_treesGrid == null)
        {
            m_treesGrid = new int[rect.width * rect.height];
        }
        else
        {
            int w = rect.width;
            int h = rect.height;
            int empty = m_settings.EMPTY;
            for (int i=0;i< h;i++)
            {

                for (int j = 0; j < w; j++)
                {
                    m_treesGrid[i * w + j] = empty;
                }
            }
        }
    }

    public void UpdateActiveBoids(RectInt rect, Boid player)
    {
        //Debug.Log("BoidsManager.UpdateActiveBoids");
        m_gridBoidsMap.Clear();
        m_activeBoids.Clear();
        ClearTreesGrid(rect);

        int boidsGridWidth = rect.width;
        Vector2Int gridCoordsOrigin = rect.min;

        AddBoidAt(0, player, gridCoordsOrigin, boidsGridWidth);

        int n = 1;
        int m = 0;

        for (int i = 0; i < m_chunks.Length; i++)
        {
            if (m_chunks[i].m_state == Chunk.STATE.ready && m_chunks[i].m_gridRect.Overlaps(rect))
            {
                List<Boid> boids = m_chunks[i].m_boids;
                for (int j = 0; j < boids.Count; j++)
                {
                    Boid boid = boids[j];
                    if (rect.xMin <= boid.gridCoords.x && boid.gridCoords.x < rect.xMax &&
                       rect.yMin <= boid.gridCoords.y && boid.gridCoords.y < rect.yMax && boid.m_stamina>0)
                    {
                        //if (boid != null)
                        {
                            //Debug.Log("Null boid in chunk.m_boids position at "+ j + " of " + boids.Count + "  chunk " + m_chunks[i].m_position);

                            if (n >= m_boidBases.Length)
                            {
                                Array.Resize(ref m_boidBases, m_boidBases.Length + 100);
                                Array.Resize(ref m_boidColliders, m_boidBases.Length + 100);

                                //Array.Resize(ref boidsResult, boidBases.Length + 100);
                                //Array.Resize(ref boidChunkChangeResult, boidBases.Length + 100);
                            }

                            /*
                            m_boidBases[n] = boid.boidBase;
                            //boidsResult[n] = boid.boidResult;
                            //boidsResult[n].pos = boid.boidBase.pos;

                            Vector2Int gridPos = boid.gridCoords - gridCoordsOrigin;
                            m_gridBoidsMap.Add(new Vector2Int(gridPos.x + gridPos.y * boidsGridWidth, n));
                            m_activeBoids.Add(boid);
                            */

                            AddBoidAt(n, boid, gridCoordsOrigin, boidsGridWidth);
                            boid.m_animator.nextState = ModelAnimator.STATE.walk;

                            n++;
                        }
                    }
                }

                List<Cylinder> trees = m_chunks[i].m_trees;
                for (int j = 0; j < trees.Count; j++)
                {
                    Cylinder tree = trees[j];
                    if (rect.xMin <= tree.gridCoords.x && tree.gridCoords.x < rect.xMax &&
                       rect.yMin <= tree.gridCoords.y && tree.gridCoords.y < rect.yMax)
                    {
                        //if (boid != null)
                        {
                            //Debug.Log("Null boid in chunk.m_boids position at "+ j + " of " + boids.Count + "  chunk " + m_chunks[i].m_position);

                            if (m >= m_treesColliders.Length)
                            {
                                Array.Resize(ref m_treesColliders, m_treesColliders.Length + 100);

                                //Array.Resize(ref boidsResult, boidBases.Length + 100);
                                //Array.Resize(ref boidChunkChangeResult, boidBases.Length + 100);
                            }

                            /*
                            m_boidBases[n] = boid.boidBase;
                            //boidsResult[n] = boid.boidResult;
                            //boidsResult[n].pos = boid.boidBase.pos;

                            Vector2Int gridPos = boid.gridCoords - gridCoordsOrigin;
                            m_gridBoidsMap.Add(new Vector2Int(gridPos.x + gridPos.y * boidsGridWidth, n));
                            m_activeBoids.Add(boid);
                            */

                            AddTreeAt(m, tree, gridCoordsOrigin, boidsGridWidth);

                            m++;
                        }
                    }
                }
            }
        }

        //Debug.Log("UpdateActiveBoids   " + string.Join(",", m_boidBases));
    }

    void AddBoidAt(int n, Boid boid, Vector2Int gridCoordsOrigin, int boidsGridWidth)
    {
        m_boidBases[n] = boid.boidBase;
        m_boidColliders[n] = boid.m_collider;
        Vector2Int gridCoords = boid.gridCoords - gridCoordsOrigin;
        m_gridBoidsMap.Add(new Vector2Int(gridCoords.x + gridCoords.y * boidsGridWidth, n));
        m_activeBoids.Add(boid);
    }

    void AddTreeAt(int n, Cylinder tree, Vector2Int gridCoordsOrigin, int boidsGridWidth)
    {
        m_treesColliders[n] = tree.cyllinderCollisionData;
        Vector2Int gridCoords = tree.gridCoords - gridCoordsOrigin;
        m_treesGrid[gridCoords.x + gridCoords.y * boidsGridWidth] = n;
    }

    public void DamageBoid(int boidID)
    {
        m_activeBoids[boidID].ApplyDamage(0.5f);
    }

    public void OnDestroy()
    {
        m_destroyed = true;
        if (m_boidsComputer != null)
            m_boidsComputer.Release();
    }

    public void LogBoids()
    {
        Debug.Log("m_gridBoidsMap = " + string.Join(",", m_gridBoidsMap));
    }
}
