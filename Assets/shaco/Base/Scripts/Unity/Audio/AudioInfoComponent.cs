using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class AudioInfoComponent : MonoBehaviour
    {
        public bool isPlaying
        {
            get
            {
                if (null == _audioSource)
                    return false;
                return _isPlaying;
            }
        }
        private bool _isPlaying = false;

        public bool isStop
        {
            get
            {
                if (null == _audioSource)
                    return true;

                return _isStop;
            }
        }
        private bool _isStop = true;

        public bool isLoop
        {
            get
            {
                if (null == _audioSource)
                    return false;
                return _audioSource.loop;
            }
        }

        public AudioSource audioSource { get { return _audioSource; } }
        private AudioSource _audioSource;

        public IAudio audioTarget { get { return _audioTarget; } }
        private IAudio _audioTarget;

        public shaco.Base.StackLocation stackLocationPlay = null;
        public shaco.Base.StackLocation stackLocationResume = new shaco.Base.StackLocation();
        public shaco.Base.StackLocation stackLocationPause = new shaco.Base.StackLocation();

        public void Init(IAudio audio)
        {
            if (null == _audioSource)
                _audioSource = this.gameObject.AddComponent<AudioSource>();
            _audioTarget = audio;
        }

        public void StartVolumeUpAction(float switchTime, float maxVolume, System.Action callbackEnd)
        {
            this.gameObject.StopAllAction(true);

            var timeAction = shaco.DelayTime.Create(switchTime);
            timeAction.onFrameFunc += (percent) =>
            {
                this.audioSource.volume = percent * maxVolume;
            };
            timeAction.onCompleteFunc += (action) =>
            {
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("AudioInfoComponent StartVolumeUpAction exception: e=" + e, this);
                    }
                }
            };
            timeAction.RunAction(this.gameObject);
            _isPlaying = true;
        }

        public void StartVolumeDownAction(float switchTime, float maxVolume, System.Action callbackEnd)
        {
            this.gameObject.StopAllAction(true);
           
            var timeAction = shaco.DelayTime.Create(switchTime);
            timeAction.onFrameFunc += (percent) =>
            {
                this.audioSource.volume = (1 - percent) * maxVolume;
            };
            timeAction.onCompleteFunc += (action) =>
            {
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("AudioInfoComponent StartVolumeDownAction exception: e=" + e, this);
                    }
                }
            };
            timeAction.RunAction(this.gameObject);
            _isPlaying = false;
        }

        public void Play()
        {
            if (null != _audioSource)
            {
                _audioSource.Play();
                _isPlaying = true;
                _isStop = false;
            }
        }

        public void Stop()
        {
            if (null != _audioSource)
            {
                _audioSource.Stop();
                _isPlaying = false;
                _isStop = true;
            }
        }

        public void UnPause()
        {
            if (null != _audioSource)
            {
                stackLocationResume.GetStack();
                _audioSource.UnPause();
                _isPlaying = true;
            }
        }

        public void Pause()
        {
            if (null != _audioSource)
            {
                stackLocationPause.GetStack();
                _audioSource.Pause();
            }
            _isPlaying = false;
        }

        /// <summary>
        /// 数据清理函数，在内存对象被回收时候调用
        /// </summary>
        void OnDestroy()
        {
            _audioSource = null;
            _audioTarget = null;
        }
    }
}