using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DBG.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager instance;

        public AudioBank m_audioBank;

        Dictionary<int, AudioQeue> m_audioQueues;
        Dictionary<int, Stack<AudioItemSource>> m_freeAudioSources;//AudioQueueID,ItemSource
        Dictionary<int, int> m_mapClipAudioQueue;
        Dictionary<int, AudioItem> m_mapClipIdClip;
        Dictionary<uint, AudioItemSource> m_playingClips; //AudioItem instance id, AudioItemSource: to perform action by clip: stop, pause, fade, etc.
        Dictionary<int, List<AudioItemSource>> m_playingAudioSources; //AudioQueueID,ItemSource: to perform action by AudioQueue: stop, pause, fade, etc.
        uint m_audioItemInstanceCount;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            instance = this;

            m_freeAudioSources = new Dictionary<int, Stack<AudioItemSource>>();
            m_mapClipAudioQueue = new Dictionary<int, int>();
            m_mapClipIdClip = new Dictionary<int, AudioItem>();
            m_playingAudioSources = new Dictionary<int, List<AudioItemSource>>();
            m_audioQueues = new Dictionary<int, AudioQeue>();

            for (int i = 0; i < m_audioBank.audioItems.Length; i++)
            {
                AudioQeue audioIQueue = m_audioBank.audioItems[i];
                m_freeAudioSources.Add(audioIQueue.id, new Stack<AudioItemSource>());
                m_playingAudioSources.Add(audioIQueue.id, new List<AudioItemSource>());
                m_audioQueues.Add(audioIQueue.id, audioIQueue);
                for (int j = 0; j < audioIQueue.channels; j++)
                {
                    m_freeAudioSources[audioIQueue.id].Push(NewAudioSource(audioIQueue.id));
                }
                for (int j = 0; j < audioIQueue.clips.Length; j++)
                {
                    audioIQueue.clips[j].id = i * 1000 + j;
                    m_mapClipAudioQueue.Add(audioIQueue.clips[j].id, audioIQueue.id);
                    m_mapClipIdClip.Add(audioIQueue.clips[j].id, audioIQueue.clips[j]);
                }
            }
            m_playingClips = new Dictionary<uint, AudioItemSource>();
        }

        AudioItemSource NewAudioSource(int audioQueueID)
        {
            return AudioItemSource.New(this, audioQueueID);
        }

        AudioItemSource GetAudioSource(int audioQueueID)
        {
            if(m_freeAudioSources[audioQueueID].Count > 0)
            {
                return m_freeAudioSources[audioQueueID].Pop();
            }

            return null;
        }

        public uint Play(int audioClipID, int loop = 1)
        {
            AudioItem audioItem = m_mapClipIdClip[audioClipID];
            int audioQueueID = m_mapClipAudioQueue[audioItem.id];
            return Play(audioQueueID, audioItem, loop, audioItem.volume);
        }

        public uint Play(int audioClipID, int loop, float volume)
        {
            AudioItem audioItem = m_mapClipIdClip[audioClipID];
            int audioQueueID = m_mapClipAudioQueue[audioItem.id];
            return Play(audioQueueID, audioItem, loop, volume);
        }

        public uint Play(int audioClipID, float volume)
        {
            AudioItem audioItem = m_mapClipIdClip[audioClipID];
            int audioQueueID = m_mapClipAudioQueue[audioItem.id];
            return Play(audioQueueID, audioItem, 0, volume);
        }

        public uint Play(AudioItem audioItem, int loop)
        {
            int audioQueueID = m_mapClipAudioQueue[audioItem.id];
            return Play(audioQueueID, audioItem, loop, audioItem.volume);
        }

        public uint Play(int audioQueueID, AudioItem audioItem, int loop, float volume)
        {
            AudioItemSource audioItemSource = GetAudioSource(audioQueueID);
            if (audioItemSource != null)
            {
                m_audioItemInstanceCount++;
                audioItemSource.Play(m_audioItemInstanceCount, audioItem, loop, volume);
                m_playingClips.Add(m_audioItemInstanceCount, audioItemSource);
                m_playingAudioSources[audioItemSource.audioQeueID].Add(audioItemSource);
                return m_audioItemInstanceCount;
            }
            return 0;
        }

        public uint Play(int audioQueueID, AudioItem audioItem, int loop, float volume, Vector3 position)
        {
            AudioItemSource audioItemSource = GetAudioSource(audioQueueID);
            if (audioItemSource != null)
            {
                audioItemSource.position = position;
                m_audioItemInstanceCount++;
                audioItemSource.Play(m_audioItemInstanceCount, audioItem, loop, volume);
                m_playingClips.Add(m_audioItemInstanceCount, audioItemSource);
                m_playingAudioSources[audioItemSource.audioQeueID].Add(audioItemSource);
                return m_audioItemInstanceCount;
            }
            return 0;
        }

        public uint PlayQueue(AudioQeue audioQueue, Vector3 position, int loop = 1)
        {
            AudioItem audioItem = audioQueue.clip;
            if (audioQueue.sound3D)
            {
                return Play(audioQueue.id, audioItem, loop, audioItem.volume, position);
            }
            return Play(audioQueue.id, audioItem, loop, audioItem.volume);
        }

        public uint PlayQueue(AudioQeue audioQueue, int loop = 1)
        {
            AudioItem audioItem = audioQueue.clip;
            return Play(audioQueue.id, audioItem, loop, audioItem.volume);
        }

        public uint PlayQueue(int audioQueueID, int loop = 1)
        {
            AudioQeue audioQueue = m_audioQueues[audioQueueID];
            AudioItem audioItem = audioQueue.clip;
            return Play(audioQueue.id, audioItem, loop, audioItem.volume);
        }

        public void Stop(uint audioItemInstanceID)
        {
            AudioItemSource audioItemSource;
            if (m_playingClips.TryGetValue(audioItemInstanceID, out audioItemSource))
            {
                audioItemSource.Stop();
                m_playingClips.Remove(audioItemInstanceID);
                m_playingAudioSources[audioItemSource.audioQeueID].Remove(audioItemSource);
                m_freeAudioSources[audioItemSource.audioQeueID].Push(audioItemSource);
            }
        }

        public void StopQueue(AudioQeue audioQeue)
        {
            StopQueue(audioQeue.id);
        }

        public void StopQueue(int audioQeueID)
        {
            var audioSources = m_playingAudioSources[audioQeueID];
            var queues = m_freeAudioSources[audioQeueID];
            for (int i=0;i< audioSources.Count;)
            {
                var audioSource = audioSources[i];
                audioSource.Stop();
                audioSources.RemoveAt(i);
                if(m_playingClips.ContainsKey(audioSource.audioItemInstanceID))
                {
                    m_playingClips.Remove(audioSource.audioItemInstanceID);
                }
                queues.Push(audioSource);
            }
        }

        public void Return(AudioItemSource audioItemSource)
        {
            m_playingClips.Remove(audioItemSource.audioItemInstanceID);
            m_playingAudioSources[audioItemSource.audioQeueID].Remove(audioItemSource);
            m_freeAudioSources[audioItemSource.audioQeueID].Push(audioItemSource);
        }

        public void SetQueueVolume(AudioQeue audioQeue, float volume)
        {
            var audioSources = m_playingAudioSources[audioQeue.id];
            for(int i=0;i< audioSources.Count;i++)
            {
                audioSources[i].volume = volume;
            }
        }
    }
}
