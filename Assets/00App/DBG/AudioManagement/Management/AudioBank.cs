using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DBG.Audio
{
    public class AudioBank : ScriptableObject
    {
        [Range(0,1)]
        public float volume;
        public AudioQeue[] audioItems;

        public Dictionary<int, AudioQeue> IndexedAudioItems {
            get {
                Dictionary<int, AudioQeue> items = new Dictionary<int, AudioQeue>();
                for(int i=0;i< audioItems.Length;i++)
                {
                    items.Add(audioItems[i].id, audioItems[i]);
                }
                return items;
            }
        }
    }
}
