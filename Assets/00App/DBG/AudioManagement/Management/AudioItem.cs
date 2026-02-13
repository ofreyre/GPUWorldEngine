using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

namespace DBG.Audio
{
    [Serializable]
    public struct AudioItem
    {
        public AudioClip clip;
        public float minPitch;
        public float maxPitch;
        [Range(0,1)]
        public float minVolume;
        [Range(0, 1)]
        public float maxVolume;
        public int id;

        public float volume { get { return minVolume == maxVolume ? minVolume : UnityEngine.Random.Range(minVolume, maxVolume); } }
        public float pitch { get { return minPitch == maxPitch ? minPitch : UnityEngine.Random.Range(minPitch, maxPitch); } }
    }
}
