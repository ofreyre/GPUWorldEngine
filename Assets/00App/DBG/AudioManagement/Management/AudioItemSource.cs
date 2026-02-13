using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DBG.Audio
{
    public class AudioItemSource : MonoBehaviour
    {
        AudioManager m_audioManager;
        AudioSource m_audioSource;
        int m_audioQeueID;
        uint m_audioItemInstanceID;
        bool m_sound3D;

        public static AudioItemSource New(AudioManager audioManager, int audioQeueID, bool _3Dsound = false)
        {
            AudioItemSource audioSource = null;

            if (!_3Dsound)
            {
                audioSource = audioManager.gameObject.AddComponent<AudioItemSource>();
                audioSource.m_audioSource = audioManager.gameObject.AddComponent<AudioSource>();
                audioSource.m_audioSource.spatialBlend = 0;
            }
            else
            {
                GameObject obj = new GameObject();
                audioSource = obj.AddComponent<AudioItemSource>();
                audioSource.m_audioSource = obj.AddComponent<AudioSource>();
                obj.transform.SetParent(audioManager.gameObject.transform);
            }


            audioSource.Init(audioManager, audioQeueID, _3Dsound);
            return audioSource;
        }

        public void Init(AudioManager audioManager, int audioQeueID, bool _3Dsound)
        {
            m_audioManager = audioManager;
            m_audioQeueID = audioQeueID;
            m_sound3D = _3Dsound;
        }

        public void Play(uint audioItemInstanceID, AudioItem audioItem, int loop, float volume)
        {
            m_audioItemInstanceID = audioItemInstanceID;
            m_audioSource.clip = audioItem.clip;
            m_audioSource.loop = loop != 1;
            m_audioSource.volume = audioItem.volume * volume;
            m_audioSource.pitch = audioItem.pitch;
            if (m_audioSource.loop)
            {
                m_audioSource.Play();
            }
            else
            {
                m_audioSource.PlayOneShot(audioItem.clip);
            }
            if (loop > 0)
            {
                Invoke("Return", audioItem.clip.length * loop * m_audioSource.pitch);
            }
        }

        public void Stop()
        {
            m_audioSource.Stop();
        }

        public void Pause()
        {
            m_audioSource.Pause();
        }

        public void UnPause()
        {
            m_audioSource.UnPause();
        }

        void Return()
        {
            m_audioManager.Return(this);
        }

        public int audioQeueID { get { return m_audioQeueID; } }
        public uint audioItemInstanceID { get { return m_audioItemInstanceID; } }

        public float volume
        {
            get { return m_audioSource.volume; }
            set { m_audioSource.volume = value; }
        }

        public bool sound3D
        {
            get { return m_sound3D; }
        }

        public Vector3 position
        {
            get { return transform.position; }
            set { transform.position = value; }
        }
    }
}
