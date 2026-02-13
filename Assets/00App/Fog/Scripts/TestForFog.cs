using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestForFog : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float f = 0;
        float g = 0;
        string aa = "";
        int steps = 100;
        for(int i=0;i< steps; i++)
        {
            float step = i / ((float)(steps));
            f += 1/Mathf.Exp((1.5f * step + 1.8f) * 1.5f);
            g = 1 - Mathf.Exp(-step * 0.2f);
            aa += g.ToString() + ", ";
        }
        Debug.Log(f);
        Debug.Log(aa);

        float totalDistance = 10;
        float b = 3;
        float a = (Mathf.Log(totalDistance) + b) / steps;
        Debug.Log(a +"  "+ Mathf.Exp(a * steps - b) + "  "+ Mathf.Log(Mathf.Exp(1)));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
