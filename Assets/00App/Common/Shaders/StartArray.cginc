#ifndef dbg_startArray
  #define dbg_startArray

uint idsMapStart;
uint idsMapLength;
RWStructuredBuffer<int2> startArray;
StructuredBuffer<uint2> idsMap;

[numthreads(1024,1,1)]
void StartArrayCompute (uint3 id : SV_DispatchThreadID)
{
    int start = -1;
    uint minIndex = 0;
    uint maxIndex = (idsMapLength - 1);

    [unroll(100)]
    for(uint i = idsMap / 2; start>-1 && minIndex != maxIndex;)
    {
        uint cellIndex = idsMap[idsMap + i].x;
        if(cellIndex < id.x)
        {
            maxIndex = i;
            i = (i + minIndex) / 2;
            cellIndex = idsMap[idsMap + i].x;
        }
        else
        {
            minIndex = i;
            i = (i + maxIndex) / 2;
            cellIndex = idsMap[idsMap + i].x;
        }

        if(cellIndex == id.x)
        {
            start = i;
            for(uint j = i - 1; j>-1; j--)
            {
                if(idsMap[idsMap + j].x == id.x)
                {
                   start = j;
                }
                else
                {
                    break;
                }
            }
            startArray[id.x] = start + idsMap;
            return;
        }

    }
}



#endif




