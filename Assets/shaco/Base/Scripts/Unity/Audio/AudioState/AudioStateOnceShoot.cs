using UnityEngine;

namespace shaco
{
    /// <summary>
    /// 只播放一次
    /// </summary>
    public class AudioStateOnceShoot : IAudioWillPlay, IAudioWillResume
    {
        public void OnWillPlay(AudioSource audio)
        {
            audio.loop = false;
        }

        public void OnWillResume(AudioSource audio)
        {
            audio.loop = false;
        }
    }
}