using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;

public class BoidsComputer
{
    public struct BoidBase
    {
        public Vector3 pos;
        public float r;
        public Vector2 v;
        public float maxSpeed;
        public float maxForce;
        public float seekWeight; //]0,1[

        public override string ToString()
        {
            return "pos = " + pos + "   r = " + r + "   v = " + v + "   maxSpeed = " + maxSpeed + "   maxForce = " + maxForce + "   seekWeight = "+ seekWeight;
        }
    };

    public struct BoidResult
    {
        public Vector3 pos;
        public Vector2 v;
        public Vector2Int gridCoords;
        public Vector2Int chunkCoords;

        public override string ToString()
        {
            return "pos = " + pos + "   v = " + v + "   gridPos = " + gridCoords + "   chunkPos = " + chunkCoords;
        }
    };

    static int[] bitonicArgs;
    static int[] startArrayArgs;
    static int[] boidsArgs;

    public BoidResult[] boidsResult; //= new BoidResult[100];
    //public int[] boidChunkChangeResult = new int[100];
    Vector2Int[] gridBoidsMap = new Vector2Int[128];
    uint[] gridStart;
    //Vector4[] cellProcessed;
    //List<MeshComputer.MeshCollisionData> meshCollisionData = new List<MeshComputer.MeshCollisionData>();


    public ComputeBuffer boidBasesBuffer;
    public ComputeBuffer boidsResultBuffer;

    //ComputeBuffer cellsProcessedByStartArrayBuffer;
    //ComputeBuffer cellProcessedBuffer;

    /*
    * List of float2(cell Index, boid index).
    * Febore sorting, de boids are not ordered by (cell index)
    * Ej:
    *       (2,1), (0,0), (2,2), (2,4), (0,3) etc.
    * Ordered by bitonec sort by (cell index).
    * Ej of list above:
    *       (0,0), (0,3), (2,1), (2,2), (2,4) etc.
    *       
    * Where x = cell index, y = boid index in "boidBases" and "boidsResult"
    */
    public ComputeBuffer gridBoidsMapBuffer;

    /*
     * List of start index in "gridBoidsMapBuffer" of the boids of the cell index = list index
     * Ej based on above list:
     *      0, 2, -1 etc.
     *      Where gridStartBuffer[0] = 0 is the start index of cell 0 in "gridBoidsMapBuffer"
     *      Where gridStartBuffer[1] = 2 is the start index of cell 1 in "gridBoidsMapBuffer"
     *      Where gridStartBuffer[2] = -1 because cell 2 has no boids and though, no start index in "gridBoidsMapBuffer"
    */
    public ComputeBuffer gridStartBuffer;

    /*
     * The functions are:
     *      boidsResult : (boid index) -> (Boids)
     *      gridBoidsMapBuffer: (map index) -> (cell index, boid index)
     *      gridStartBuffer (cell index) -> (start cell index == map index)
     *  
     *  The composition:
     *      boidsResult (gridBoidsMapBuffer( gridStartBuffer(cell index) ).y )
     */

    ComputeBuffer meshCollisionDataBuffer;
    //public ComputeBuffer boidChunkChangeResultBuffer;

    public ComputeBuffer treesGridBuffer;
    public ComputeBuffer treesCollidersBuffer;

    public int gridBoidsMapStart;
    public int gridBoidsMapLength;
    int gridStartLength;
    int bitonicI0, bitonicI1;
    int bitonicOffset;
    int np;

    float gridCellWidth;
    int boidsGridWidth;
    int collisionGridWidthInCells;
    Vector2 meshCollisionOrigin;
    float cellCollisionWidth;
    int npComparers;


    public TerrainSettings settings;

    public int boidsCount;
    Action callbackReady;
    int gpuDataCount;
    public float chunkWorldSize;

    public BoidsComputer(TerrainSettings settings)
    {
        this.settings = settings;
        cellCollisionWidth = settings.m_meshSettings.meshScale;
        gridCellWidth = settings.m_meshSettings.meshScale;
        chunkWorldSize = settings.MeshSideLength;
    }

    public void InitBuffers(RectInt boidsRect, RectInt collisionRect,
        List<Vector2Int> preGridBoidsMap,
        BoidBase[] boidBases,
        MeshComputer.MeshCollisionData[] meshCollisionData, 
        int[] treesGrid, 
        CyllinderColliderData[] treesColliders)
    {
        InitTrees(boidsRect, treesGrid, treesColliders);
        InitBitonicParams(boidsRect, collisionRect, preGridBoidsMap.Count);
        InitBitonicBoids(preGridBoidsMap);
        InitBuffers(boidBases, meshCollisionData);
    }

    void InitTrees(RectInt boidsRect, int[] treesGrid, CyllinderColliderData[] treesColliders)
    {
        if(treesGridBuffer != null)
        {
            treesGridBuffer.Release();
        }

        if(treesCollidersBuffer == null)
        {
            treesCollidersBuffer = new ComputeBuffer(boidsRect.width * boidsRect.height, sizeof(float) * 3);
        }

        treesGridBuffer = new ComputeBuffer(treesGrid.Length, sizeof(int));
        treesGridBuffer.SetData(treesGrid);
        treesCollidersBuffer.SetData(treesColliders);
    }

    void InitBitonicParams(RectInt boidsRect, RectInt collisionRect, int boidsCount)
    {
        boidsGridWidth = boidsRect.width;
        gridStartLength = boidsRect.width * boidsRect.height;

        this.boidsCount = boidsCount;
        int n = boidsCount;
        np = UtilsMath.UpperPower2(n);
        npComparers = Mathf.Max(0, n - np / 2);
        int toOdd = npComparers % 2;
        gridBoidsMapLength = Mathf.Min(n + npComparers, np);
        npComparers = gridBoidsMapLength - n;
        bitonicOffset = np - gridBoidsMapLength;
        bitonicI0 = bitonicOffset / 2 + toOdd;
        bitonicI1 = np / 2 - bitonicI0;
        gridBoidsMapStart = npComparers;

        collisionGridWidthInCells = collisionRect.width;
        meshCollisionOrigin = collisionRect.min;
    }

    void InitBitonicBoids(List<Vector2Int> preGridBoidsMap)
    {
        if (gridBoidsMap.Length < gridBoidsMapLength)
        {
            Array.Resize(ref gridBoidsMap, gridBoidsMapLength);
        }

        for (int i = 0; i < gridBoidsMapLength; i++)
        {
            if (i < npComparers)
            {
                gridBoidsMap[i] = new Vector2Int(-1, -1);
            }
            else
            {
                gridBoidsMap[i] = preGridBoidsMap[i - npComparers];
            }
        }
    }

    void InitBuffers(BoidBase[] boidBases, MeshComputer.MeshCollisionData[] meshCollisionData)
    {
        this.meshCollisionData = meshCollisionData;
        //Debug.Log("rrrrrr " + boidBases[0]);

        if (gridStart == null || gridStart.Length != gridStartLength)
        {
            gridStart = new uint[gridStartLength];
            //cellProcessed = new Vector4[gridStartLength];
        }

        bitonicArgs = UtilsComputeShader.GetThreadGroups(
            settings.m_boidsSettings.bitomicShader,
            settings.m_boidsSettings.bitomicKernel,
            new Vector3Int(bitonicI1, 0, 0),
            bitonicArgs
        );

        startArrayArgs = UtilsComputeShader.GetThreadGroups(
            settings.m_boidsSettings.startArrayShader,
            settings.m_boidsSettings.startArrayKernel,
            new Vector3Int(gridStartLength, 0, 0),
            startArrayArgs
        );

        boidsArgs = UtilsComputeShader.GetThreadGroups(
            settings.m_boidsSettings.boidsShader,
            settings.m_boidsSettings.boidsKernel,
            new Vector3Int(gridStartLength, 0, 0),
            boidsArgs
        );


        if (boidBasesBuffer != null)
        {
            boidBasesBuffer.Release();
            boidsResultBuffer.Release();
            gridBoidsMapBuffer.Release();
            gridStartBuffer.Release();
            meshCollisionDataBuffer.Release();
        }

        boidBasesBuffer = new ComputeBuffer(boidBases.Length, sizeof(float) * 9);
        boidBasesBuffer.SetData(boidBases);
        //boidBasesBuffer = new ComputeBuffer(boidsCount, sizeof(float) * 9);
        //boidBasesBuffer.SetData(boidBases, 0, 0, boidsCount);

        boidsResultBuffer = new ComputeBuffer(boidBases.Length, sizeof(float) * 5 + sizeof(int) * 4);
        //boidsResultBuffer = new ComputeBuffer(boidsCount, sizeof(float) * 5 + sizeof(int) * 4);

        gridBoidsMapBuffer = new ComputeBuffer(gridBoidsMap.Length, sizeof(int) * 2);
        gridBoidsMapBuffer.SetData(gridBoidsMap);
        gridStartBuffer = new ComputeBuffer(gridStartLength, sizeof(uint));
        meshCollisionDataBuffer = new ComputeBuffer(meshCollisionData.Length, sizeof(float) * 7);
        meshCollisionDataBuffer.SetData(meshCollisionData);
    }

    public void ReleaseTemp()
    {
        boidBasesBuffer.Release();
        boidsResultBuffer.Release();
        gridBoidsMapBuffer.Release();
        gridStartBuffer.Release();
        meshCollisionDataBuffer.Release();
    }

    public void Release()
    {
        if (boidBasesBuffer != null)
            boidBasesBuffer.Release();

        if (boidsResultBuffer != null)
            boidsResultBuffer.Release();

        if (gridBoidsMapBuffer != null)
            gridBoidsMapBuffer.Release();

        if (gridStartBuffer != null)
            gridStartBuffer.Release();

        if (meshCollisionDataBuffer != null)
            meshCollisionDataBuffer.Release();

        /*
        if (cellProcessedBuffer != null)
            cellProcessedBuffer.Release();
        */

        if (treesGridBuffer != null)
            treesGridBuffer.Release();

        if (treesCollidersBuffer != null)
            treesCollidersBuffer.Release();

    }

    public void Compute(float deltaTime, Vector2 targetPosition, float targetR, Action callbackReady)
    {
        this.callbackReady = callbackReady;
        ComputeBitonicSort();
        ComputeArrayStart();
        ComputeBoids(deltaTime, targetPosition, targetR);
    }

    public void ComputeBitonicSort()
    {
        BoidsSettings boidsSettings = settings.m_boidsSettings;
        ComputeShader shader = boidsSettings.bitomicShader;
        int kernelHandle = shader.FindKernel(boidsSettings.bitomicKernel);

        for (int changeOrder = 2; changeOrder <= np; changeOrder *= 2)
        {
            for (int separation = changeOrder / 2; separation > 0; separation /= 2)
            {
                int sequenceLength = separation * 2;

                shader.SetInt("changeOrder", changeOrder);
                shader.SetInt("separation", separation);
                shader.SetInt("sequenceLength", sequenceLength);
                shader.SetInt("offset", bitonicOffset); 
                shader.SetInt("i0", bitonicI0); 
                shader.SetInt("i1", bitonicI1); 

                shader.SetBuffer(kernelHandle, "workingArray", gridBoidsMapBuffer);
                
                shader.Dispatch(kernelHandle, bitonicArgs[0], bitonicArgs[1], bitonicArgs[2]);
            }
        }
    }

    public void ComputeArrayStart()
    {
        //Debug.Log("ComputeArrayStart boidsCount = " + boidsCount);
        BoidsSettings boidsSettings = settings.m_boidsSettings;
        ComputeShader shader = boidsSettings.startArrayShader;
        int kernelHandle = shader.FindKernel(boidsSettings.startArrayKernel);

        shader.SetInt("emptyCell", settings.EMPTY);
        shader.SetInt("idsMapStart", gridBoidsMapStart);
        shader.SetInt("idsMapLength", boidsCount);
        shader.SetInt("startArrayLength", gridStartLength);
        shader.SetBuffer(kernelHandle, "startArray", gridStartBuffer);
        shader.SetBuffer(kernelHandle, "idsMap", gridBoidsMapBuffer);
        //shader.SetBuffer(kernelHandle, "cellsProcessed", cellsProcessedByStartArrayBuffer);

        shader.Dispatch(kernelHandle, startArrayArgs[0], startArrayArgs[1], startArrayArgs[2]);
    }

    public void ComputeBoids(float deltaTime, Vector2 targetPosition, float targetR)
    {
        gpuDataCount = 0;
        BoidsSettings boidsSettings = settings.m_boidsSettings;
        ComputeShader shader = boidsSettings.boidsShader;
        int kernelHandle = shader.FindKernel(boidsSettings.boidsKernel);

        shader.SetInt("emptyCell", settings.EMPTY);
        shader.SetVector("targetPosition", targetPosition);
        shader.SetFloat("targetR", targetR);
        shader.SetInt("gridWidth", boidsGridWidth);
        shader.SetFloat("gridCellWidth", gridCellWidth);
        shader.SetInt("gridBoidsMapStart", gridBoidsMapStart);
        shader.SetInt("gridBoidsMapLength", gridBoidsMapLength);
        shader.SetInt("gridStartLength", gridStartLength);
        shader.SetFloat("desireSeparation", boidsSettings.desireSeparation);
        shader.SetFloat("deltaTime", deltaTime);

        shader.SetInt("collisionGridWidthInCells", collisionGridWidthInCells);
        shader.SetVector("meshCollisionOrigin", meshCollisionOrigin);
        shader.SetFloat("cellCollisionWidth", cellCollisionWidth);
        shader.SetFloat("chunkWorldSize", chunkWorldSize);
        shader.SetFloat("meshScale", settings.m_meshSettings.meshScale);


        shader.SetBuffer(kernelHandle, "gridBoidsMap", gridBoidsMapBuffer);
        shader.SetBuffer(kernelHandle, "gridStart", gridStartBuffer);
        shader.SetBuffer(kernelHandle, "boidsBases", boidBasesBuffer);
        shader.SetBuffer(kernelHandle, "treesGrid", treesGridBuffer);
        shader.SetBuffer(kernelHandle, "treesColliders", treesCollidersBuffer);

        shader.SetBuffer(kernelHandle, "boidResults", boidsResultBuffer);
        //shader.SetBuffer(kernelHandle, "boidChunkChangeResult", boidChunkChangeResultBuffer);
        shader.SetBuffer(kernelHandle, "meshCollisionData", meshCollisionDataBuffer);

        //shader.SetBuffer(kernelHandle, "cellProcessed", cellProcessedBuffer);

        shader.Dispatch(kernelHandle, boidsArgs[0], boidsArgs[1], boidsArgs[2]);
        AsyncGPUReadback.Request(boidsResultBuffer, BoidsResultBufferCallback);
        //AsyncGPUReadback.Request(boidChunkChangeResultBuffer, boidChunkChangeResultCallback);

    }

    void BoidsResultBufferCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            boidsResult = readbackRequest.GetData<BoidResult>().ToArray();
            boidsResultBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void boidChunkChangeResultCallback(AsyncGPUReadbackRequest readbackRequest)
    {
        if (!readbackRequest.hasError)
        {
            //boidChunkChangeResult = readbackRequest.GetData<int>().ToArray();
            //boidChunkChangeResultBuffer.Release();
            gpuDataCount++;
            DataReady();
        }
    }

    void DataReady()
    {
        if (gpuDataCount >= 1)
        {   
            callbackReady.Invoke();
        }
    }

    public static Vector3Int IsIntArraySorted(Vector2Int[] a, int max = -1)
    {
        max = max == -1 ? a.Length : max;

        for (int i = 0; i < max - 1; i++)
        {
            if (a[i].x > a[i + 1].x) return new Vector3Int(i, a[i].x, a[i + 1].x);
        }
        return new Vector3Int(-1, -1, -1);
    }

    public void Log()
    {
        gridBoidsMapBuffer.GetData(gridBoidsMap);
        gridStartBuffer.GetData(gridStart);
        //cellProcessedBuffer.GetData(cellProcessed);

        //int[] cellsProcessesByStartArray = new int[gridStartLength];
        //cellsProcessedByStartArrayBuffer.GetData(cellsProcessesByStartArray);

        //Debug.Log("$$$$$$$$$$$$$ cellProcessed");
        //Debug.Log(string.Join(",", cellProcessed));
        //Debug.Log("$$$$$$$$$$$$$ cellsProcessesByStartArray");
        //Debug.Log(string.Join(",", cellsProcessesByStartArray));

        //Debug.Log(string.Join(",", gridBoidsMap));
        //Debug.Log(gridStart.Length + " " + gridStartLength+" "+ gridBoidsMapStart);
        //Debug.Log(string.Join(",", gridStart));

        int c = 0;
        float b = 0;
        int boidsMapped = 0;
        //for(int i=0;i< cellProcessed.Length;i++)
        string notMapped = "";
        List<int> filledCells = new List<int>();
        for (int i = 0; i < gridStartLength; i++)
        {
            if (gridStart[i] != settings.EMPTY)
            {
                filledCells.Add(i);
                c++;
                //Debug.Log(gridStart[i]);
                if (gridBoidsMap[gridStart[i]].x == i)
                {
                    boidsMapped++;
                }
                else
                {
                    notMapped += gridBoidsMap[gridStart[i]] + "  *  ";
                }
            }
        }

        int boidsInMap = 0;
        string notMappedInv = "";
        for (uint i=0;i< gridBoidsMapLength; i++)
        {
            Vector2Int data = gridBoidsMap[i];
            if(data.x != -1)
            {
                boidsInMap++;
                if (i == 0 || (i > 0 && data.x != gridBoidsMap[i - 1].x))
                {
                    int k = Array.IndexOf(gridStart, i);
                    if (k == -1)
                    {
                        notMappedInv += i + " - " + (i - gridBoidsMapStart) + " - " + data + "  ,  ";
                    }
                }
            }
        }

        //if (boidsCount != c)
        {
            //Debug.Log("$$$$$$$$$$$$$ gridBoidsMap not sorted");
            //Debug.Log(string.Join(",", preGridBoidsMap));
            Debug.Log("$$$$$$$$$$$$$ gridBoidsMap sorted by cell index ");
            Debug.Log(string.Join(",", gridBoidsMap));
            Debug.Log("$$$$$$$$$$$$$ gridStart");
            Debug.Log(string.Join(",", gridStart));
            Debug.Log("boidsCount = " + boidsCount
                + "    gridStart notEmpty = " + c
                + "    bois > 0 = " + b
                + "   gridStartLength = " + gridStartLength
                + "   boidsMapped = " + boidsMapped
                + "   boidsInMap = " + boidsInMap
                + "   gridBoidsMap.Length = " + gridBoidsMap.Length
                + "   gridBoidsMapLength = " + gridBoidsMapLength
                + "   bitonic sorted = " + IsIntArraySorted(gridBoidsMap, gridBoidsMapLength));
            Debug.Log("notMapped :  " + notMapped);
            Debug.Log("notMappedInv :  " + notMappedInv);
            Debug.Log("filledCells : " + string.Join(",", filledCells));
            //Debug.Log("cellsProcessedEmpty :  " + cellsProcessedEmpty);
        }
    }


    MeshComputer.MeshCollisionData[] meshCollisionData;

    public Vector2Int WorldPosToCollisionCellCoords(Vector2 worldPos)
    {
        Vector2 localPos = worldPos - meshCollisionOrigin;
        return new Vector2Int((int)(localPos.x / settings.m_meshSettings.meshScale), (int)(localPos.y / settings.m_meshSettings.meshScale) * 2 + (int)(Mathf.Sign((localPos.y % settings.m_meshSettings.meshScale) - (localPos.x % settings.m_meshSettings.meshScale)) * 0.5));
    }

    public int WorldPosToCollisionCellIndex(Vector2 worldPos)
    {
        Vector2Int cellCoord = WorldPosToCollisionCellCoords(worldPos);
        Debug.Log("cellCoord = " + cellCoord+ "  collisionGridWidthInCells = "+ collisionGridWidthInCells + "  "+ (meshCollisionData.Length / collisionGridWidthInCells));
        return cellCoord.y * collisionGridWidthInCells + cellCoord.x;
    }

    public float GetMeshY(Vector2 worldPos)
    {
        int cellIndex = WorldPosToCollisionCellIndex(worldPos);
        MeshComputer.MeshCollisionData collisionData = meshCollisionData[cellIndex];
        return (collisionData.d - collisionData.n.x * worldPos.x - collisionData.n.z * worldPos.y) / collisionData.n.y;
    }
}
