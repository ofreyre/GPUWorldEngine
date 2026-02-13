using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolFactory:MonoBehaviour
{
    public virtual void Fill(Pool pool, Transform container)
    {
        //Stack<GameObject> objectsOff = new Stack<GameObject>();
        for (int i = pool.m_objectsOff.Count; i < pool.m_min; i++)
        {
            //objectsOff.Push(GetItem(pool, container));
            pool.Return(GetItem(pool, container));
        }
        //pool.m_objectsOff = objectsOff;
    }

    public virtual GameObject GetItem(Pool pool, Transform container)
    {
        GameObject gobj = Object.Instantiate(pool.m_prefab);
        gobj.GetComponent<PoolItem>().m_pool = pool;
        gobj.transform.parent = container;
        return gobj;
    }
}
