using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ChunksManager
{
    TerrainSettings m_settings;
    Queue<Chunk> m_chunksBank = new Queue<Chunk>();
    Dictionary<Vector2Int, Chunk> m_chunks = new Dictionary<Vector2Int, Chunk>();
    Chunk[] m_activeChunks;
    //Chunk[] m_prevActiveChunks;
    public int m_collisionDataLength;
    int m_readyChunks;
    int m_totalChunks;
    Action m_callbackChunksReady;
    public MeshComputer.MeshCollisionData[] m_meshCollisionData;
    RectInt m_chunksRect;
    RectInt m_chunksCacheRect;
    float m_chunkLength;
    int m_chunksBankCapacity;

    public ChunksManager(TerrainSettings settings, Action callbackChunksReady, Transform excludeCenter)
    {
        m_settings = settings;
        m_callbackChunksReady = callbackChunksReady;
        Chunk.m_exclusionTransformCenter = excludeCenter;
        m_chunkLength = m_settings.MeshSideLength;
        m_chunksBankCapacity = settings.m_cacheRadiusInChunks * 4;
    }

    public Chunk[] InitChunks(RectInt chunksRect)
    {
        m_chunksRect = chunksRect;
        m_chunksCacheRect = m_settings.GetCacheRectInChunks(chunksRect.center * m_settings.MeshSideLength);
        float chunkLength = m_settings.MeshSideLength;
        m_activeChunks = new Chunk[chunksRect.width * chunksRect.height];
        //m_prevActiveChunks = new Chunk[chunksRect.width * chunksRect.height];

        for (int i = chunksRect.y; i < chunksRect.yMax; i++)
        {
            for (int j = chunksRect.x; j < chunksRect.xMax; j++)
            {
                m_totalChunks++;
                Chunk chunk = new Chunk(new Vector2(j, i) * chunkLength, m_settings, ChunkInitialized, ChunkUnloaded);
                chunk.ActiveSelf = true;
                m_chunks.Add(chunk.m_chunkCoords, chunk);
                m_activeChunks[(i - chunksRect.y) * chunksRect.width + j - chunksRect.x] = chunk;
            }
        }

        return m_activeChunks;
    }

    void SetChunksRect(RectInt chunksRect)
    {
        m_chunksRect = chunksRect;
        RectInt chunksCacheRect = m_chunksCacheRect;
        m_chunksCacheRect = m_settings.GetCacheRectInChunks(chunksRect.center * m_settings.MeshSideLength);
        if(chunksCacheRect.size != m_chunksCacheRect.size || chunksCacheRect.min == m_chunksCacheRect.min)
        {
            for(int y = chunksCacheRect.yMin; y < chunksCacheRect.yMax;y++)
            {
                for (int x = chunksCacheRect.xMin; x < chunksCacheRect.xMax; x++)
                {
                    Vector2Int chunkKey = new Vector2Int(x, y);
                    if(m_chunks.ContainsKey(chunkKey))
                    {
                        Chunk chunk = m_chunks[chunkKey];
                        m_chunks.Remove(chunkKey);
                        if (m_chunksBank.Count < m_chunksBankCapacity)
                        {
                            m_chunksBank.Enqueue(chunk);
                            chunk.Unload();
                        }
                        else
                        {
                            chunk.Destroy();
                        }
                    }
                }
            }
        }
    }

    void ChunkInitialized(Vector2Int m_chunkCoord)
    {
        m_readyChunks++;
        if (Ready)
        {
            //m_activeChunks[0].m_heightMapComputer.Log();
            //m_activeChunks[0].m_meshComputer.LogNormals();
            m_callbackChunksReady.Invoke();
        }
    }

    void ChunkUnloaded(Vector2Int m_chunkCoord)
    {
    }

    public bool Ready
    {
        get { return m_totalChunks == m_readyChunks; }
    }

    /*
    void UpdateChunks(RectInt newChunksRect)
    {
        if (newChunksRect.min != m_chunksRect.min)
        {
            Chunk[] chunks = m_activeChunks;
            //m_activeChunks = m_prevActiveChunks;
            //m_prevActiveChunks = chunks;
            float chunkLength = m_settings.MeshSideLength;

            int dj = newChunksRect.min.x - m_chunksRect.min.x;
            int di = newChunksRect.min.y - m_chunksRect.min.y;

            for (int i = m_chunksRect.y; i < m_chunksRect.yMax; i++)
            {
                for (int j = m_chunksRect.x; j < m_chunksRect.xMax; j++)
                {
                    if (di != 0 || dj != 0)
                    {
                        int indexI0 = i - m_chunksRect.y;
                        int indexJ0 = j - m_chunksRect.x;
                        int indexI1 = indexI0 - di;
                        int indexJ1 = indexJ0 - dj;

                        if (indexI1 < 0 || indexI1 > newChunksRect.height || indexJ1 < 0 || indexJ1 > newChunksRect.width)
                        {
                            Chunk chunk = m_activeChunks[indexI0 * newChunksRect.width + indexJ0];
                            chunk.Unload();
                            //m_readyChunks--;
                            m_activeChunks[indexI0 * newChunksRect.width + indexJ0].InitMesh(new Vector2(j, i) * chunkLength);
                        }
                        else
                        {
                            indexI1 = indexI1 < 0 ? m_chunksRect.yMax : indexI1 % (m_chunksRect.yMax);
                            indexJ1 = indexJ1 < 0 ? m_chunksRect.xMax : indexJ1 % (m_chunksRect.xMax);

                            Debug.Log("*********************************************");
                            Debug.Log("( " + indexJ0 + ", " + indexI0 + " ) (" + +indexJ0 + ", " + indexI0 + " )");
                            Debug.Log(m_chunksRect.BoundsToString() + "   " + newChunksRect.BoundsToString());
                            m_activeChunks[indexI0 * newChunksRect.width + indexJ0] = chunks[indexI1 * newChunksRect.width + indexJ1];
                        }
                    }
                }
            }

            //m_boidsReady = m_readyChunks == m_totalChunks;
            SetChunksRect(newChunksRect);
        }
    }
    */

    void UpdateChunks(RectInt newChunksRect)
    {
        if (newChunksRect.min != m_chunksRect.min)
        {
            for (int i = newChunksRect.y; i < newChunksRect.yMax; i++)
            {
                for (int j = newChunksRect.x; j < newChunksRect.xMax; j++)
                {
                    Vector2Int key = new Vector2Int(j, i);
                    if (m_chunks.ContainsKey(key))
                    {
                        m_activeChunks[(i - newChunksRect.y) * newChunksRect.width + (j - newChunksRect.x)] = m_chunks[key];
                    }
                    else if(m_chunksBank.Count > 0)
                    {
                        Chunk chunk = m_chunksBank.Dequeue();
                        chunk.InitMesh(new Vector2(j, i) * m_chunkLength);
                        m_activeChunks[(i - newChunksRect.y) * newChunksRect.width + (j - newChunksRect.x)] = chunk;
                    }
                    else
                    {
                        Chunk chunk = new Chunk(new Vector2(j, i) * m_chunkLength, m_settings, ChunkInitialized, ChunkUnloaded);
                        chunk.ActiveSelf = true;
                        m_chunks.Add(chunk.m_chunkCoords, chunk);
                        m_activeChunks[(i - newChunksRect.y) * newChunksRect.width + (j - newChunksRect.x)] = chunk;
                    }
                }
            }

            //m_boidsReady = m_readyChunks == m_totalChunks;
            SetChunksRect(newChunksRect);
        }
    }

    public void UpdateMeshCollisionData(RectInt rect)
    {
        m_collisionDataLength = rect.width * rect.height * 2;

        if (m_meshCollisionData == null)
        {
            m_meshCollisionData = new MeshComputer.MeshCollisionData[m_collisionDataLength];
        }
        else if (m_collisionDataLength > m_meshCollisionData.Length)
        {
            Array.Resize(ref m_meshCollisionData, m_collisionDataLength);
        }

        for (int i = 0; i < m_activeChunks.Length; i++)
        {
            Chunk chunk = m_activeChunks[i];
            if (chunk.m_state == Chunk.STATE.ready && chunk.m_gridRect.Overlaps(rect))
            {
                chunk.CopyCollisionData(rect, m_meshCollisionData);
            }
        }
    }

    public void Update(RectInt collisionRect, RectInt chunksRect)
    {
        //if (Ready)
        {
            UpdateChunks(chunksRect);
            UpdateMeshCollisionData(collisionRect);
        }
    }

    public void OnDestroy()
    {
        for (int i = 0; i < m_activeChunks.Length; i++)
        {
            m_activeChunks[i].Destroy();
        }

        /*
        for (int i = 0; i < m_prevActiveChunks.Length; i++)
        {
            if (m_prevActiveChunks[i] != null)
                m_prevActiveChunks[i].Destroy();
        }
        */
    }

#if UNITY_EDITOR
    void OnDrawMeshGizmo()
    {


        for (int i = 0; i < m_activeChunks.Length; i++)
        {
            m_activeChunks[i].LogCollisionMesh();
        }
    }
#endif
}
