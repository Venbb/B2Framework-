using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 循环播放
    /// </summary>
    public class AudioStateLoop : IAudioWillPlay
    {
        public void OnWillPlay(AudioSource audio)
        {
            audio.loop = true;
        }
    }
}