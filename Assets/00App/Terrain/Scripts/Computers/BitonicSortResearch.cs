using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BitonicSortResearch
{
    public static void Sort(int[] array)
    {
        Debug.Log("#######################################");
        Debug.Log(string.Join(",",array));
        int n = array.Length;
        int p = (int)Mathf.Ceil(Mathf.Log10(n) / Mathf.Log10(2));
        int np = (int)Mathf.Pow(2, p);
        int npComparers = n - np / 2;
        int toOdd = npComparers % 2;
        int workingArrayLenth = Mathf.Min(n + npComparers, np);
        npComparers = workingArrayLenth - n;
        int offset = np - workingArrayLenth;
        int i0 = offset / 2 + toOdd;
        int i1 = np / 2 - i0;

        int[] workingArray = new int[workingArrayLenth];
        for(int i=0; i< workingArrayLenth; i++)
        {
            if(i < npComparers)
            {
                workingArray[i] = -1;
            }
            else
            {
                workingArray[i] = array[i - npComparers];
            }
        }
        Debug.Log("i0 = " + i0 + "   i1 = " + i1 + "   offset = " + offset);

        //Debug.Log("***************************************");
        //Debug.Log("n = " + n + "   p = " + p + "   np = " + np + "   offset = " + offset + "   i1 = "+ i1);

        for (int changeOrder=2; changeOrder <= np; changeOrder *= 2)
        {
            for (int separation = changeOrder / 2; separation > 0; separation /= 2)
            {
                int sequenceLength = separation * 2;
                //for (int i = 0; i < np / 2; i++)
                for (int k = 0; k < i1; k++)
                {

                    int i = k + i0;
                    int sequence = i / separation;
                    int sequenceStart = sequence * sequenceLength;
                    int compare0 = sequenceStart + (i % separation) - offset;
                    if (compare0 >= 0)
                    {
                        int order = ((i * 2) / changeOrder) % 2;
                        int compare1 = compare0 + separation;
                        if ((order == 0 && workingArray[compare0] > workingArray[compare1]) ||
                            (order == 1 && workingArray[compare0] < workingArray[compare1]))
                        {
                            int val = workingArray[compare0];
                            workingArray[compare0] = workingArray[compare1];
                            workingArray[compare1] = val;
                        }
                    }                    
                }
            }
        }


        for (int i = 0; i < n; i++)
        {
            array[i] = workingArray[i + npComparers];
        }
        Debug.Log(string.Join(",", array));
    }

    public static void MergeSort(int[] array)
    {
        Debug.Log(string.Join(",", array));
        int n = array.Length;
        for(int d = 1;d<n;d*=2)
        {
            //Debug.Log("************ d = " + d);
            for (int i = 0; i < (int)Mathf.Ceil(n / 2.0f); i++)
            {
                int i0 = ( ((i / d) * d * 2) + (i % d));
                int i1 = (int)Mathf.Clamp(i0 + d, 0, n-1);
                //Debug.Log(i + "  " + i0+"  "+i1);
                if (array[i0] > array[i1])
                {
                    int val = array[i0];
                    array[i0] = array[i1];
                    array[i1] = val;
                }
            }
            
        }

        Debug.Log(string.Join(",", array));
    }

    public static int[] GetRandomIntArray(int length, int min, int max)
    {
        List<int> list = new List<int>(length);
        for(int i=0;i<length;i++)
        {
            list.Add(i);
        }

        int[] array = new int[length];
        for(int i=0;i<length;i++)
        {
            int index = Random.Range(0, list.Count);
            array[i] = list[index];
            list.RemoveAt(index);
        }
        return array;
    }

    public static Vector2Int[] GetRandomVector2IntArray(int length, Vector2Int min, Vector2Int max)
    {
        Vector2Int[] array = new Vector2Int[length];
        for (int i = 0; i < length; i++)
        {
            array[i] = new Vector2Int(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
        }
        return array;
    }

    public static Vector2Int[] GetRandomVector2IntArray(int length, int min, int max)
    {
        List<int> values = new List<int>(length);
        for(int i=0;i< length;i++)
        {
            values.Add(i);
        }

        Vector2Int[] array = new Vector2Int[length];
        for (int i = 0; i < length; i++)
        {
            int valIndex = Random.Range(0, values.Count);
            int value = values[valIndex];
            values.RemoveAt(valIndex);
            array[i] = new Vector2Int(Random.Range(min, max), valIndex);
        }
        return array;
    }

    public static Vector3Int IsIntArraySorted(int[] a)
    {
        for(int i=0;i<a.Length-1;i++)
        {
            if (a[i] > a[i + 1]) return new Vector3Int(i, a[i], a[i + 1]);
        }
        return new Vector3Int(-1,-1,-1);
    }

    public static Vector3Int IsVector2IntArraySorted(Vector2Int[] a)
    {
        for (int i = 0; i < a.Length - 1; i++)
        {
            if (a[i].x > a[i + 1].x) return new Vector3Int(i, a[i].x, a[i + 1].x);
        }
        return new Vector3Int(-1, -1, -1);
    }
}
