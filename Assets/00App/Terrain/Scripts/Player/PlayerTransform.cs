using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BoidResult = BoidsComputer.BoidResult;

public class PlayerTransform : Boid
{
    [SerializeField] PlayerRotateMouse m_rorate;
    [SerializeField] PlayerMoveKeys m_move;
    [SerializeField] RayShooter rayShooter;
    [SerializeField] float m_seekWeight;
    [SerializeField] RayManagerReadyEvent m_rayManagerReadyEvent;
    [SerializeField] AudioSource m_audioPlayer;
    [SerializeField] AudioClip m_shotFX;
    [SerializeField] AudioClip m_shotEndFX;
    [SerializeField] GameObject m_rayView;

    Vector3 m_boidPos;
    Vector2 m_separationVelocity;
    bool m_rayManagerReady;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        m_rayManagerReadyEvent.Register(RayManagerReadyListener);
    }

    void RayManagerReadyListener(bool ready)
    {
        m_rayManagerReady = ready;
    }

    public void UpdateCoords(float cellLength, float chunkLength)
    {
        gridCoords = new Vector2Int((int)(transform.position.x / cellLength), (int)(transform.position.z / cellLength));
        chunkCoords = new Vector2Int((int)(transform.position.x / chunkLength), (int)(transform.position.z / chunkLength));
    }

    /*
    public void Update()
    {
        UpdateInput();
    }
    */

    void UpdateV()
    {
        if (v != Vector2.zero)
        {
            Vector2 separation = v.normalized * maxForce;
            m_separationVelocity += separation * (1 - m_seekWeight);
        }
        else
        {
            if (m_separationVelocity.magnitude > 0.1f)
            {
                m_separationVelocity *= 0.5f;
            }
            else
            {
                m_separationVelocity = Vector3.zero;
            }
        }

        v = Vector2.zero;
    }

    void UpdatePosition(float deltaTime)
    {
        Vector2 seekVelocity = m_move.GetSeek(maxForce, maxSpeed);
        Vector2 velocity = (seekVelocity * m_seekWeight + m_separationVelocity * (1 - m_seekWeight)).normalized * maxSpeed * deltaTime;
        transform.position += new Vector3(velocity.x, 0, velocity.y);
        transform.position = new Vector3(transform.position.x, m_boidPos.y, transform.position.z);
    }

    // Update is called once per frame
    public void UpdateInput()
    {
        float deltaTime = Time.deltaTime;
        m_rorate.Update(deltaTime);
        UpdateV();
        UpdatePosition(deltaTime);
        if (m_rayManagerReady && Input.GetMouseButton(0))
        {
            if (!m_audioPlayer.isPlaying)
            {
                m_audioPlayer.clip = m_shotFX;
                m_audioPlayer.loop = true;
                m_audioPlayer.Play();
                m_rayView.SetActive(true);
            }
            rayShooter.Shoot();
        }
        else if(Input.GetMouseButtonUp(0))
        {
            m_rayView.SetActive(false);
            m_audioPlayer.clip = m_shotEndFX;
            m_audioPlayer.loop = false;
            m_audioPlayer.Play();
        }
    }

    public override BoidResult boidResult
    {
        get
        {
            return new BoidResult
            {
                pos = transform.position,
                v = v,
                gridCoords = gridCoords,
                chunkCoords = chunkCoords
            };
        }

        set
        {
            m_boidPos = value.pos;
            v = value.v;
            gridCoords = value.gridCoords;
            chunkCoords = value.chunkCoords;
            //transform.LookAt(value.pos + new Vector3(v.x, 0, v.y), Vector3.up);
        }
    }
}
