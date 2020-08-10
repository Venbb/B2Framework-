using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class AudioManagerComponent : MonoBehaviour
    {
        static public bool isDestroyed { get; private set; }

        private void Start()
        {
            this.name = "AudioManager";
        }

        private void OnDestroy()
        {
            shaco.GameHelper.sound.StopAll();            
            shaco.GameHelper.music.StopAll();

            isDestroyed = true;
        }

        /// <summary>
        /// 添加新的音频对象到该节点下
        /// </summary>
        /// <param name="audio">音频信息</param>
        public void AddAudioSourceAsChild(AudioInfoComponent audio)
        {
            audio.transform.parent = this.transform;
        }
    }
}