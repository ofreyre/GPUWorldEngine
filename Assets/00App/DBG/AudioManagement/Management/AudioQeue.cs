using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace DBG.Audio
{
    public class AudioQeue : ScriptableObject
    {
        public int id;
        public int channels;
        public bool sound3D;
        public AudioItem[] clips;

        public AudioItem clip { get { return clips[Random.Range(0, clips.Length)]; } }


    }
}
