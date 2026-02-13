using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class Pool: MonoBehaviour
{
    public GameObject m_prefab;
    public int m_min, m_max;
    [HideInInspector]
    public Stack<GameObject> m_objectsOff;
    protected int m_count;
    public PoolFactory m_factory;
    public Transform m_container;

    protected virtual void Awake()
    {
        Init();
    }

    public virtual void Init()
    {   
        if (m_objectsOff == null || m_objectsOff.Count < m_min)
        {
            if(m_objectsOff == null)
                m_objectsOff = new Stack<GameObject>();
            m_prefab.SetActive(false);
            m_factory.Fill(this, m_container);
            m_count = m_min;
        }
    }

    public GameObject Get() {
        GameObject gobj = null;
        int n = m_objectsOff.Count;
        if (n > 0)
        {
            gobj = m_objectsOff.Pop();
        }
        else if(m_count < m_max)
        {
            m_count++;
            gobj = m_factory.GetItem(this, m_container);
        }
        //gobj.SetActive(true);
        return gobj;
    }

    public void Return(GameObject gobj)
    {
        m_objectsOff.Push(gobj);
    }
}
