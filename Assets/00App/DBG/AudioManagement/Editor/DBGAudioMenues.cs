using UnityEditor;
using DBG.Utils.ScriptableObjects;

namespace DBG.Audio
{ 
    public class DBGAudioMenues
    {
        [MenuItem("DBG/Audio/New audio qeue")]
        [MenuItem("Assets/DBG/Audio/New audio qeue")]
        public static void AudioQeue_new()
        {
            UtilsScriptableObject.CreateAsset<AudioQeue>();
        }

        [MenuItem("DBG/Audio/New audio bank")]
        [MenuItem("Assets/DBG/Audio/New audio bank")]
        public static void AudioBank_new()
        {
            UtilsScriptableObject.CreateAsset<AudioBank>();
        }
    }
}
