using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolItem : MonoBehaviour {
    [HideInInspector] public Pool m_pool;

    void OnDisable() {
        //if (m_pool != null)
        {
            m_pool.Return(gameObject);
        }
    }
}
