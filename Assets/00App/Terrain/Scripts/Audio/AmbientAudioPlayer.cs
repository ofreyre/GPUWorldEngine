using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DBG.Audio;
using System;

public class AmbientAudioPlayer : MonoBehaviour
{
    [Serializable]
    public class RandomSounds
    {
        public AudioQeue[] qeue;
        public float minDelay;
        public float maxDelay;
        public float minDistance;
        public float maxDistance;
    }

    [SerializeField] RandomSounds[] m_sounds;
    [SerializeField] Transform m_player;

    void Start()
    {
        for(int i = 0; i < m_sounds.Length; i++)
        {
            StartCoroutine(SetPlayParams(m_sounds[i].qeue, m_sounds[i].minDelay, m_sounds[i].maxDelay, m_sounds[i].minDistance, m_sounds[i].maxDistance));
        }
    }

    IEnumerator SetPlayParams(AudioQeue[] qeues, float minDelay, float maxDelay, float minDistance, float maxDistance)
    {
        AudioQeue qeue = qeues[UnityEngine.Random.Range(0, qeues.Length)];
        float distance = UnityEngine.Random.Range(minDistance, maxDistance);
        Vector3 position = m_player.transform.position + (new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-0.1f, 1), UnityEngine.Random.Range(-1f, 1f))).normalized * distance;
        AudioManager.instance.PlayQueue(qeue, position);
        float nextT = Time.time + UnityEngine.Random.Range(minDelay, maxDelay);
        while (Time.time < nextT)
        {
            yield return null;
        }
        StartCoroutine(SetPlayParams(qeues, minDelay, maxDelay, minDistance, maxDistance));
    }
}
