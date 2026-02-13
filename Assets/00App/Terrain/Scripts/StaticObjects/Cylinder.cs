using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct CyllinderColliderData
{
    public Vector2 pos;
    public float r;
}

public class Cylinder : MonoBehaviour
{
    public int type;
    public float r;
    public Vector2Int gridCoords;
    public Vector2Int chunkCoords;

    public BoidsInitComputer.Cyllinder m_collider;
    public Vector3 m_position;

    public virtual SpacialData spacialData
    {
        get
        {
            return new SpacialData
            {
                //pos = transform.position,
                position = transform.position,
                eulerAngles = transform.eulerAngles,
                scale = transform.localScale,
                r = r,
                gridCoords = gridCoords,
                chunkCoords = chunkCoords
            };
        }

        set
        {
            m_position = value.position;
            transform.position = m_position;
            transform.eulerAngles = value.eulerAngles;
            transform.localScale = value.scale;
            gridCoords = value.gridCoords;
            chunkCoords = value.chunkCoords;
            r = value.r;
        }
    }

    public CyllinderColliderData cyllinderCollisionData
    {
        get
        {
            return new CyllinderColliderData
            {
                pos = new Vector2(m_position.x, m_position.z),
                r = r
            };
        }
    }
}
