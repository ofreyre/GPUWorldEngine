using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Unity.Collections;

public class BitonicTest : MonoBehaviour
{
    public int arrayLength;
    public Text text;
    int testsCount = 0;
    public int power;
    public ComputeShader shaderBitonicSort;
    public string shaderKernelBitonicSort;
    public ComputeShader shaderStartArray;
    public string shaderKernelStartArray;
    public int gridLength = 8;

    ComputeBuffer dispatchArgsBuffer;

    Vector2Int[] array;
    Vector2Int[] workingArray;
    int[] startArray;
    ComputeBuffer workingArrayBuffer;
    ComputeBuffer startArrayBuffer;
    int bitonicArrayN;
    int bitonicWorkingArrayStart;

    // Start is called before the first frame update
    void Start()
    {
        arrayLength = 17;
        testsCount = 0;


        //int[] array = BitonicSortResearch.GetRandomIntArray(arrayLength, 0, testsCount * 1000);
        //BitonicSortResearch.Sort(array);
        //BitonicSortResearch.MergeSort(array);

        //TestScriptCompute();
        TestComputeShaderBitonicSort();
    }

    // Update is called once per frame
    void Update()
    {
        //TestScriptCompute();
    }

    void TestScriptCompute()
    {
        int[] array = BitonicSortResearch.GetRandomIntArray(arrayLength, 0, testsCount * 1000);
        BitonicSortResearch.Sort(array);
        //BitonicSortResearch.MergeSort(array);
        Vector3Int sorted = BitonicSortResearch.IsIntArraySorted(array);
        if (sorted != new Vector3Int(-1, -1, -1))
        {
            Debug.Log("################################ Error");
            Debug.Log("arrayLength = " + arrayLength);
            Debug.Log(sorted);
#if UNITY_EDITOR
            if (UnityEditor.EditorApplication.isPlaying)
            {
                UnityEditor.EditorApplication.isPlaying = false;
            }
#endif
        }
        testsCount++;
        text.text = arrayLength + "  " + testsCount;
        if (testsCount % 1 == 0)
        {
            testsCount = 0;
            arrayLength++;
        }
    }

    void TestComputeShaderBitonicSort()
    {
        ComputeBitonicSort();
    }

    public void Decompose(int n)
    {
        List<int> list = new List<int>();
        int i = 0;
        for(;n>0 && i < 1000;)
        {
            int m = (int)Mathf.Pow(2,GetLowPower(n));
            list.Add(m);
            n -= m;
            i++;
        }
        Debug.Log(string.Join(",", list));
    }

    public int GetLowPower(int n)
    {
        return (int)Mathf.Floor(Mathf.Log10(n) / Mathf.Log10(2));
    }

    void ComputeBitonicSort()
    {
        //array = BitonicSortResearch.GetRandomVector2IntArray(arrayLength, new Vector2Int(0, 0), new Vector2Int(gridLength * gridLength, 1024));
        //arrayLength = Random.Range(200, 400);
        //array = BitonicSortResearch.GetRandomVector2IntArray(arrayLength, 0, 1681);
        array = new Vector2Int[]{
            new Vector2Int(8, 0),new Vector2Int(18, 1),new Vector2Int(21, 2),new Vector2Int(22, 3),new Vector2Int(48, 4),new Vector2Int(58, 5),new Vector2Int(61, 6),new Vector2Int(62, 7),new Vector2Int(67, 8),new Vector2Int(88, 9),new Vector2Int(139, 10),new Vector2Int(141, 11),new Vector2Int(143, 12),new Vector2Int(148, 13),new Vector2Int(169, 14),new Vector2Int(179, 15),new Vector2Int(182, 16),new Vector2Int(183, 17),new Vector2Int(188, 18),new Vector2Int(294, 19),new Vector2Int(344, 20),new Vector2Int(386, 21),new Vector2Int(347, 22),new Vector2Int(392, 23),new Vector2Int(394, 24),new Vector2Int(334, 25),new Vector2Int(384, 26),new Vector2Int(257, 27),new Vector2Int(388, 28),new Vector2Int(433, 29),new Vector2Int(435, 30),new Vector2Int(249, 31),new Vector2Int(425, 32),new Vector2Int(299, 33),new Vector2Int(428, 34),new Vector2Int(473, 35),new Vector2Int(474, 36),new Vector2Int(288, 37),new Vector2Int(465, 38),new Vector2Int(173, 39),new Vector2Int(468, 40),new Vector2Int(513, 41),new Vector2Int(515, 42),new Vector2Int(328, 43),new Vector2Int(464, 44),new Vector2Int(338, 45),new Vector2Int(508, 46),new Vector2Int(553, 47),new Vector2Int(556, 48),new Vector2Int(504, 49),new Vector2Int(213, 50),new Vector2Int(549, 51),new Vector2Int(594, 52),new Vector2Int(596, 53),new Vector2Int(544, 54),new Vector2Int(253, 55),new Vector2Int(589, 56),new Vector2Int(594, 57),new Vector2Int(636, 58),new Vector2Int(584, 59),new Vector2Int(293, 60),new Vector2Int(588, 61),new Vector2Int(634, 62),new Vector2Int(677, 63),new Vector2Int(625, 64),new Vector2Int(333, 65),new Vector2Int(628, 66),new Vector2Int(675, 67),new Vector2Int(676, 68),new Vector2Int(351, 69),new Vector2Int(665, 70),new Vector2Int(373, 71),new Vector2Int(669, 72),new Vector2Int(674, 73),new Vector2Int(717, 74),new Vector2Int(637, 75),new Vector2Int(722, 76),new Vector2Int(664, 77),new Vector2Int(458, 78),new Vector2Int(709, 79),new Vector2Int(714, 80),new Vector2Int(757, 81),new Vector2Int(678, 82),new Vector2Int(763, 83),new Vector2Int(704, 84),new Vector2Int(413, 85),new Vector2Int(708, 86),new Vector2Int(754, 87),new Vector2Int(798, 88),new Vector2Int(718, 89),new Vector2Int(803, 90),new Vector2Int(659, 91),new Vector2Int(539, 92),new Vector2Int(748, 93),new Vector2Int(795, 94),new Vector2Int(797, 95),new Vector2Int(758, 96),new Vector2Int(843, 97),new Vector2Int(698, 98),new Vector2Int(497, 99),new Vector2Int(788, 100),new Vector2Int(794, 101),new Vector2Int(797, 102),new Vector2Int(714, 103),new Vector2Int(883, 104),new Vector2Int(738, 105),new Vector2Int(746, 106),new Vector2Int(828, 107),new Vector2Int(834, 108),new Vector2Int(837, 109),new Vector2Int(593, 110),new Vector2Int(882, 111),new Vector2Int(884, 112),new Vector2Int(782, 113),new Vector2Int(827, 114),new Vector2Int(874, 115),new Vector2Int(877, 116),new Vector2Int(631, 117),new Vector2Int(925, 118),new Vector2Int(925, 119),new Vector2Int(821, 120),new Vector2Int(823, 121),new Vector2Int(874, 122),new Vector2Int(876, 123),new Vector2Int(671, 124),new Vector2Int(963, 125),new Vector2Int(1006, 126),new Vector2Int(966, 127),new Vector2Int(861, 128),new Vector2Int(862, 129),new Vector2Int(873, 130),new Vector2Int(876, 131),new Vector2Int(711, 132),new Vector2Int(883, 133),new Vector2Int(882, 134),new Vector2Int(1049, 135),new Vector2Int(902, 136),new Vector2Int(913, 137),new Vector2Int(916, 138),new Vector2Int(833, 139),new Vector2Int(1006, 140),new Vector2Int(1086, 141),new Vector2Int(1046, 142),new Vector2Int(953, 143),new Vector2Int(956, 144),new Vector2Int(754, 145),new Vector2Int(922, 146),new Vector2Int(1091, 147),new Vector2Int(929, 148),new Vector2Int(952, 149),new Vector2Int(996, 150),new Vector2Int(878, 151),new Vector2Int(963, 152),new Vector2Int(886, 153),new Vector2Int(1133, 154),new Vector2Int(992, 155),new Vector2Int(995, 156),new Vector2Int(918, 157),new Vector2Int(1003, 158),new Vector2Int(922, 159),new Vector2Int(1169, 160),new Vector2Int(1028, 161),new Vector2Int(994, 162),new Vector2Int(958, 163),new Vector2Int(1043, 164),new Vector2Int(966, 165),new Vector2Int(926, 166),new Vector2Int(965, 167),new Vector2Int(1067, 168),new Vector2Int(1075, 169),new Vector2Int(957, 170),new Vector2Int(1083, 171),new Vector2Int(964, 172),new Vector2Int(1008, 173),new Vector2Int(965, 174),new Vector2Int(1138, 175),new Vector2Int(1148, 176),new Vector2Int(1074, 177),new Vector2Int(991, 178),new Vector2Int(1082, 179),new Vector2Int(1004, 180),new Vector2Int(1006, 181),new Vector2Int(965, 182),new Vector2Int(1338, 183),new Vector2Int(361, 184),new Vector2Int(401, 185),new Vector2Int(441, 186),new Vector2Int(481, 187),new Vector2Int(284, 188),new Vector2Int(521, 189),new Vector2Int(406, 190),new Vector2Int(561, 191),new Vector2Int(446, 192),new Vector2Int(572, 193),new Vector2Int(601, 194),new Vector2Int(486, 195),new Vector2Int(570, 196),new Vector2Int(641, 197),new Vector2Int(526, 198),new Vector2Int(610, 199),new Vector2Int(613, 200),new Vector2Int(566, 201),new Vector2Int(650, 202),new Vector2Int(653, 203),new Vector2Int(606, 204),new Vector2Int(649, 205),new Vector2Int(693, 206),new Vector2Int(646, 207),new Vector2Int(689, 208),new Vector2Int(733, 209),new Vector2Int(686, 210),new Vector2Int(729, 211),new Vector2Int(773, 212),new Vector2Int(726, 213),new Vector2Int(769, 214),new Vector2Int(812, 215),new Vector2Int(766, 216),new Vector2Int(809, 217),new Vector2Int(811, 218),new Vector2Int(776, 219),new Vector2Int(806, 220),new Vector2Int(849, 221),new Vector2Int(851, 222),new Vector2Int(815, 223),new Vector2Int(846, 224),new Vector2Int(889, 225),new Vector2Int(891, 226),new Vector2Int(855, 227),new Vector2Int(886, 228),new Vector2Int(929, 229),new Vector2Int(931, 230),new Vector2Int(895, 231),new Vector2Int(926, 232),new Vector2Int(928, 233),new Vector2Int(971, 234),new Vector2Int(894, 235),new Vector2Int(967, 236),new Vector2Int(1012, 237),new Vector2Int(934, 238),new Vector2Int(1010, 239),new Vector2Int(974, 240),new Vector2Int(1008, 241),new Vector2Int(973, 242),new Vector2Int(1008, 243),new Vector2Int(1013, 244),new Vector2Int(1053, 245),new Vector2Int(1062, 246),new Vector2Int(1053, 247),new Vector2Int(1102, 248),new Vector2Int(1052, 249),new Vector2Int(1101, 250),new Vector2Int(1121, 251),new Vector2Int(1046, 252),new Vector2Int(1047, 253),new Vector2Int(1007, 254),new Vector2Int(1161, 255),new Vector2Int(1086, 256),new Vector2Int(1128, 257),new Vector2Int(1048, 258),new Vector2Int(1201, 259),new Vector2Int(1126, 260),new Vector2Int(1169, 261),new Vector2Int(1130, 262),new Vector2Int(1241, 263),new Vector2Int(1166, 264),new Vector2Int(1209, 265),new Vector2Int(1212, 266),new Vector2Int(1172, 267),new Vector2Int(1281, 268),new Vector2Int(1206, 269),new Vector2Int(1249, 270),new Vector2Int(1211, 271),new Vector2Int(1213, 272),new Vector2Int(1321, 273),new Vector2Int(1246, 274),new Vector2Int(1289, 275),new Vector2Int(1251, 276),new Vector2Int(1213, 277),new Vector2Int(1361, 278),new Vector2Int(1286, 279),new Vector2Int(1329, 280),new Vector2Int(1291, 281),new Vector2Int(1253, 282),new Vector2Int(1401, 283),new Vector2Int(1326, 284),new Vector2Int(1369, 285),new Vector2Int(1331, 286),new Vector2Int(1294, 287),new Vector2Int(1366, 288),new Vector2Int(1409, 289),new Vector2Int(1371, 290),new Vector2Int(1334, 291),new Vector2Int(1406, 292),new Vector2Int(1449, 293),new Vector2Int(1412, 294),new Vector2Int(1374, 295),new Vector2Int(1446, 296),new Vector2Int(1490, 297),new Vector2Int(1493, 298),new Vector2Int(1414, 299),new Vector2Int(1486, 300),new Vector2Int(1530, 301),new Vector2Int(1533, 302),new Vector2Int(1455, 303),new Vector2Int(1526, 304),new Vector2Int(1570, 305),new Vector2Int(1573, 306),new Vector2Int(1495, 307),new Vector2Int(1501, 308),new Vector2Int(1566, 309),new Vector2Int(1652, 310),new Vector2Int(1613, 311),new Vector2Int(1535, 312),new Vector2Int(1542, 313),new Vector2Int(1604, 314),new Vector2Int(1653, 315),new Vector2Int(1616, 316),new Vector2Int(1582, 317),new Vector2Int(1093, 318),new Vector2Int(1133, 319),new Vector2Int(1173, 320),new Vector2Int(1309, 321),new Vector2Int(1349, 322),new Vector2Int(1389, 323),new Vector2Int(1351, 324),new Vector2Int(1388, 325),new Vector2Int(1390, 326),new Vector2Int(1429, 327),new Vector2Int(1431, 328),new Vector2Int(1469, 329),new Vector2Int(1471, 330),new Vector2Int(1509, 331),new Vector2Int(1511, 332),new Vector2Int(1508, 333),new Vector2Int(1551, 334),new Vector2Int(1556, 335),new Vector2Int(1549, 336),new Vector2Int(1591, 337),new Vector2Int(1596, 338),new Vector2Int(1589, 339),new Vector2Int(1590, 340),new Vector2Int(1636, 341),new Vector2Int(1638, 342),new Vector2Int(1629, 343),new Vector2Int(1671, 344),new Vector2Int(1676, 345),new Vector2Int(1678, 346),new Vector2Int(532, 347),new Vector2Int(200, 348),new Vector2Int(1520, 349),new Vector2Int(151, 350),new Vector2Int(119, 351),new Vector2Int(1190, 352),new Vector2Int(1559, 353),new Vector2Int(70, 354),new Vector2Int(79, 355),new Vector2Int(31, 356),new Vector2Int(40, 357),new Vector2Int(1271, 358),new Vector2Int(1640, 359)
        };

        bitonicArrayN = array.Length;
        //int p = UtilsMath.UpperPower2(bitonicArrayN);
        int np = UtilsMath.UpperPower2(bitonicArrayN); //(int)Mathf.Pow(2, p);
        int npComparers = bitonicArrayN - np / 2;
        int toOdd = npComparers % 2;
        int workingArrayLength = Mathf.Min(bitonicArrayN + npComparers, np);
        npComparers = workingArrayLength - bitonicArrayN;
        int offset = np - workingArrayLength;
        int i0 = offset / 2 + toOdd;
        int i1 = np / 2 - i0;
        bitonicWorkingArrayStart = npComparers;

        workingArray = new Vector2Int[workingArrayLength];

        for (int i = 0; i < workingArrayLength; i++)
        {
            if (i < npComparers)
            {
                workingArray[i] = new Vector2Int(-1, -1);
            }
            else
            {
                workingArray[i] = array[i - npComparers];
            }
        }

        int kernelHandle = shaderBitonicSort.FindKernel(shaderKernelBitonicSort);

        int[] dispatchArgs = new int[3];
        dispatchArgs = UtilsComputeShader.GetThreadGroups(
                shaderBitonicSort,
                shaderKernelBitonicSort,
                new Vector3Int(i1, 0, 0),
                dispatchArgs
            );
        dispatchArgsBuffer = UtilsComputeShader.GetArgsComputeBuffer(dispatchArgs);
        workingArrayBuffer = new ComputeBuffer(workingArray.Length, sizeof(int) * 2);
        workingArrayBuffer.SetData(workingArray);

        for (int changeOrder = 2; changeOrder <= np; changeOrder *= 2)
        {
            for (int separation = changeOrder / 2; separation > 0; separation /= 2)
            {
                int sequenceLength = separation * 2;
                shaderBitonicSort.SetInt("changeOrder", changeOrder);
                shaderBitonicSort.SetInt("separation", separation);
                shaderBitonicSort.SetInt("sequenceLength", sequenceLength);
                shaderBitonicSort.SetInt("offset", offset);
                shaderBitonicSort.SetInt("i0", i0);
                shaderBitonicSort.SetInt("i1", i1);
                shaderBitonicSort.SetBuffer(kernelHandle, "workingArray", workingArrayBuffer);

                //shaderBitonicSort.DispatchIndirect(kernelHandle, dispatchArgsBuffer, 0);
                shaderBitonicSort.Dispatch(kernelHandle, Mathf.CeilToInt(workingArray.Length / 16), 1, 1);
            }
        }
        AsyncGPUReadbackRequest callback = AsyncGPUReadback.Request(workingArrayBuffer, BitonicSortCallback);
        //AsyncGPUReadbackRequest callback = AsyncGPUReadback.Request(workingArrayBuffer);

        /*
        while (!callback.done)
        {
            yield return new WaitForEndOfFrame();
        }
        workingArrayBuffer.GetData(workingArray);

        Debug.Log(string.Join(",", array));
        Debug.Log(string.Join(",", workingArray));
        */
    }

    void BitonicSortCallback(AsyncGPUReadbackRequest callback)
    {
        testsCount++;
        if(text)
            text.text = arrayLength + "  " + (arrayLength == array.Length) +"  "+testsCount;
        if(workingArrayBuffer == null)
        {
            return;
        }

        //Debug.Log(callback.done);
        dispatchArgsBuffer.Dispose();
        if (!callback.hasError)
        {
            /*NativeArray<Vector2Int> wa = callback.GetData<Vector2Int>(0);
            workingArray = wa.ToArray();
            wa.Dispose();*/
            workingArrayBuffer.GetData(workingArray);

            Vector3Int sorted = BitonicSortResearch.IsVector2IntArraySorted(workingArray);
            if (sorted != new Vector3Int(-1, -1, -1))
            {
                Debug.Log("################################ Bitonic sort Error");
                Debug.Log("arrayLength = " + arrayLength);
                Debug.Log(sorted);
                Debug.Log(string.Join(",", array));
                Debug.Log(string.Join(",", workingArray));
                workingArrayBuffer.Dispose();
#if UNITY_EDITOR
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    UnityEditor.EditorApplication.isPaused = true;
                }
#endif
            }
            else
            {
                //Debug.Log("################################ Bitonic sort Success");
                //Debug.Log(string.Join(",", array));
                //Debug.Log(string.Join(",", workingArray));
                ComputeStartArray();
            }
        }
    }

    void ComputeStartArray()
    {
        startArray = new int[gridLength * gridLength];
        int kernelHandle = shaderStartArray.FindKernel(shaderKernelStartArray);

        int[] dispatchArgs = new int[3];
        dispatchArgs = UtilsComputeShader.GetThreadGroups(
                shaderStartArray,
                shaderKernelStartArray,
                new Vector3Int(startArray.Length, 0, 0),
                dispatchArgs
            );
        dispatchArgsBuffer = UtilsComputeShader.GetArgsComputeBuffer(dispatchArgs);
        startArrayBuffer = new ComputeBuffer(startArray.Length, sizeof(int));
        startArrayBuffer.SetData(startArray);

        //Debug.Log("################################ Start Array");
        //Debug.Log("idsMapStart = " + bitonicWorkingArrayStart + "   idsMapLength = " + bitonicArrayN + "   startArrayLength = " + startArray.Length);

        shaderStartArray.SetInt("emptyCell", 99999999);
        shaderStartArray.SetInt("idsMapStart", bitonicWorkingArrayStart);
        shaderStartArray.SetInt("idsMapLength", bitonicArrayN);
        shaderStartArray.SetInt("startArrayLength", startArray.Length);

        shaderStartArray.SetBuffer(kernelHandle, "startArray", startArrayBuffer);
        shaderStartArray.SetBuffer(kernelHandle, "idsMap", workingArrayBuffer);

        shaderStartArray.DispatchIndirect(kernelHandle, dispatchArgsBuffer, 0);
        AsyncGPUReadback.Request(startArrayBuffer, StartArrayCallback);
    }

    void StartArrayCallback(AsyncGPUReadbackRequest callback)
    {
        dispatchArgsBuffer.Dispose();
        if (!callback.hasError)
        {
            /*
            Debug.Log("################################ Start Array Success");
            workingArrayBuffer.GetData(workingArray);
            Debug.Log(string.Join(",", workingArray));
            startArrayBuffer.GetData(startArray);
            Debug.Log(string.Join(",", startArray));
            */
            workingArrayBuffer.Dispose();
            startArrayBuffer.Dispose();
            ComputeBitonicSort();
        }
        else
        {
            workingArrayBuffer.Dispose();
            startArrayBuffer.Dispose();
            Debug.Log("################################ Start Array callback Error");
        }
    }

    private void OnDestroy()
    {
        if(dispatchArgsBuffer != null)
        {
            dispatchArgsBuffer.Dispose();
        }

        if (workingArrayBuffer != null)
        {
            workingArrayBuffer.Dispose();
        }
        if (startArrayBuffer != null)
        {
            startArrayBuffer.Dispose();
        }
    }

}
