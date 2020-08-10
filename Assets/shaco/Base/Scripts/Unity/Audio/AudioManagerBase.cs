using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace shaco
{
    public class AudioManagerBase : shaco.IAudioManager
    {
        private enum AudioState
        {
            Playing,
            Paused
        }

        private class AudioInfo
        {
            public string fileName = string.Empty;
            public List<AudioInfoComponent> audioCompnents = new List<AudioInfoComponent>();
        }

        private Dictionary<string, AudioInfo> _audioInfos = new Dictionary<string, AudioInfo>();
        private List<string> _audioInfosSequeue = new List<string>();

        /// <summary>
        /// 音量
        /// </summary>
        public float volume { get { return _volume; } }
        private float _volume = 1.0f;

        //音频挂载对象，方便统一管理
        private AudioManagerComponent _audioManagerComponent { get { return shaco.GameEntry.GetComponentInstance<AudioManagerComponent>(); }}

        //当前准备播放的音频对象
        private AudioInfoComponent _currentWillPlayAudio = null;

        /// <summary>
        /// 播放
        /// <param name="fileName">音频文件相对路径</param>
        /// <param name="playSettings">音频播放的相关设置(如果为空默认为播放一次)</param>
        /// </summary>
        public void Play(IAudio audio)
        {
            try
            {
                PlayBase(audio);
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} Play exception: fileName=" + audio.fileName + " e=" + e, this.ToTypeString());
            }
        }

        private void PlayBase(IAudio audio)
        {
            AudioInfoComponent audioComponent = null;
            AudioInfo findAudioInfo = null;

            IAudioCanPlay canPlayState = audio as IAudioCanPlay;
            if (null != canPlayState && !canPlayState.CanPlay(this, audio))
                return;

            _audioInfos.TryGetValue(audio.fileName, out findAudioInfo);
            var willCreateState = audio as IAudioWillCreate;

            if (null == findAudioInfo)
            {
                findAudioInfo = new AudioInfo();
                findAudioInfo.fileName = audio.fileName;
            }

            //如果之前有暂停过，则恢复播放
            var findPausedSoundsInfo = GetAudioInfos(audio.fileName, AudioState.Paused, true);

            //没有暂停过的音频
            if (findPausedSoundsInfo.IsNullOrEmpty())
            {
                var stackLocationPlay = new shaco.Base.StackLocation();
                stackLocationPlay.GetStack();

                //即将场景音频
                if (null != willCreateState)
                {
                    willCreateState.OnWillCreate(audio.fileName, (newClip) =>
                    {
                        if (null == newClip)
                        {
                            shaco.Log.ErrorFormat("{0} Play error: can't create new clip, state=" + willCreateState + " fileName=" + audio.fileName, this.ToTypeString());
                            return;
                        }
                        OnCreateAudioClipEnd(findAudioInfo, audio, newClip, stackLocationPlay);
                    });
                }
                else
                {
                    var newClip = shaco.GameHelper.res.LoadResourcesOrLocal<AudioClip>(audio.fileName);
                    if (null == newClip)
                    {
                        shaco.Log.ErrorFormat("{0} Play error: not found clip, filleName=" + audio.fileName, this.ToTypeString());
                        return;
                    }
                    OnCreateAudioClipEnd(findAudioInfo, audio, newClip, stackLocationPlay);
                }
            }
            //有暂停过的声音，则重新开始播放
            else
            {
                if (!findPausedSoundsInfo.IsNullOrEmpty())
                {
                    audioComponent = findPausedSoundsInfo[0];
                    if (null != audioComponent)
                    {
                        ResumeBase(audioComponent);

                        //设置音量
                        audioComponent.audioSource.volume = _volume;
                    }
                }
            }
        }

        private void OnCreateAudioClipEnd(AudioInfo audioInfo, IAudio audio, AudioClip newClip, shaco.Base.StackLocation stackLocationPlay)
        {
            //创建audio信息
            var audioComponent = shaco.GameHelper.objectpool.Instantiate<AudioInfoComponent>("AudioModel", () =>
            {
                var newObj = new GameObject();
                var newAudioInfoTmp = newObj.AddComponent<AudioInfoComponent>();
                return newAudioInfoTmp;
            });
            audioComponent.Init(audio);
            audioComponent.stackLocationPlay = stackLocationPlay;

            //设置音频挂载对象
            _audioManagerComponent.AddAudioSourceAsChild(audioComponent);

            //设置音频信息
            audioComponent.audioSource.clip = newClip;

            //等待播放完毕后自动关闭，或者等待设定为循环播放后停止等待
            shaco.Base.WaitFor.Run(() =>
            {
                return AudioManagerComponent.isDestroyed || null == audioComponent.audioSource || !audioComponent.audioSource.isPlaying || audioComponent.isLoop;
            }, () =>
            {
                //如果是非循环的音频则关闭它
                if (!AudioManagerComponent.isDestroyed && !audioComponent.isLoop)
                {
                    var willStopState = audio as IAudioWillAutoStop;
                    if (null != willStopState && null != audioComponent.audioSource)
                    {
                        try
                        {
                            willStopState.OnWillAutoStop(audioComponent.audioSource);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("AudioManagerBase OnCreateAudioClipEnd OnWillAutoStop error: fileName=" + audioInfo.fileName + " e=" + e, audioComponent);
                        }
                    }

                    if (shaco.GameHelper.objectpool.IsInstantiated(audioComponent))
                        audioComponent.RecyclingWithPool();
                    audioInfo.audioCompnents.Remove(audioComponent);
                }
            });

            //根据音频名字设定对象名字
            audioComponent.name = audio.fileName + "[" + audioInfo.audioCompnents.Count + "]";

            //设置音量
            audioComponent.audioSource.volume = _volume;

            //即将播放音频
            _currentWillPlayAudio = audioComponent;
            var willState = audio as IAudioWillPlay;
            if (null != willState)
            {
                try
                {
                    willState.OnWillPlay(audioComponent.audioSource);
                }
                catch (System.Exception e)
                {
                    Log.Error("AudioManagerBase OnCreateAudioClipEnd OnWillPlay error: fileName=" + audioInfo.fileName + " e=" + e, audioComponent);
                }
            }
            audioInfo.audioCompnents.Add(audioComponent);

            //音频混合
            var mixerState = audio as IAudioMixer;
            if (null != mixerState)
                audioComponent.audioSource.outputAudioMixerGroup = mixerState.GetAudioMixerGroup();

            //开始播放
            CheckStartAudioVolumeAction(audioComponent, true, null);
            audioComponent.Play();
            _currentWillPlayAudio = null;

            if (!_audioInfos.ContainsKey(audioInfo.fileName))
            {
                _audioInfos.Add(audioInfo.fileName, audioInfo);
                _audioInfosSequeue.Add(audioInfo.fileName);
            }
        }

        /// <summary>
        /// 关闭最后一次正在播放的音频
        /// <param name="isStop">true：关闭最后一次正在播放音乐 false：暂停最后一次正在播放音乐</param>
        /// </summary>
        public void Pop(bool isStop = true)
        {
            try
            {
                PopBase(isStop);
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} Pop exception: isStop=" + isStop + " e=" + e, this.ToTypeString());
            }
        }

        private void PopBase(bool isStop)
        {
            AudioInfo lastPlayingAudioInfo = null;
            AudioInfo lastPausedAudioInfo = null;

            if (!_audioInfos.IsNullOrEmpty())
            {
                //关闭最上层正在播放的声音
                for (int i = _audioInfosSequeue.Count - 1; i >= 0; --i)
                {
                    var audioInfo = _audioInfos[_audioInfosSequeue[i]];
                    if (audioInfo.audioCompnents.Any(v => v.isPlaying))
                    {
                        lastPlayingAudioInfo = audioInfo;
                        break;
                    }
                }
            }

            if (!_audioInfos.IsNullOrEmpty())
            {
                //恢复最上层暂停过的声音
                for (int i = _audioInfosSequeue.Count - 1; i >= 0; --i)
                {
                    var audioInfo = _audioInfos[_audioInfosSequeue[i]];
                    if (audioInfo.audioCompnents.Any(v => !v.isPlaying))
                    {
                        lastPausedAudioInfo = audioInfo;
                        break;
                    }
                }
            }
            else
            {
                shaco.Log.WarningFormat("{0} Pop warning: no audio at current", this.ToTypeString());
            }

            if (isStop)
            {
                if (null != lastPlayingAudioInfo)
                    StopBase(lastPlayingAudioInfo.fileName);
            }
            else
            {
                if (null != lastPlayingAudioInfo && !lastPlayingAudioInfo.audioCompnents.IsNullOrEmpty())
                    lastPlayingAudioInfo.audioCompnents.ForEach(v => PauseBase(v));
            }
        }

        /// <summary>
        /// 暂停
        /// <param name="fileName">音频文件相对路径</param>
        /// </summary>
        public void Pause(IAudio audio)
        {
            try
            {
                PauseBase(audio);
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} Pop exception: fileName=" + audio.fileName + " e=" + e, this.ToTypeString());
            }
        }

        private void PauseBase(IAudio audio)
        {
            var findAudioInfos = GetAudioInfos(audio.fileName, AudioState.Playing, false);
            if (!findAudioInfos.IsNullOrEmpty())
            {
                foreach (var iter in findAudioInfos)
                {
                    PauseBase(iter);
                }
            }
            else
            {
                shaco.Log.ErrorFormat("{0} Pause error: not found plyaing, fileName=" + audio.fileName, this.ToTypeString());
            }
        }

        /// <summary>
        /// 停止
        /// <param name="fileName">音频文件相对路径</param>
        /// <return>是否停止成功</return>
        /// </summary>
        public bool Stop(IAudio audio)
        {
            try
            {
                return StopBase(audio.fileName);
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} Stop exception: fileName=" + audio.fileName + " e=" + e, this.ToTypeString());
                return false;
            }
        }

        /// <summary>
        /// 恢复所有
        /// </summary>
        public void ResumeAll()
        {
            try
            {
                ResumeAllBase();
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} ResumeAll exception: e=" + e, this.ToTypeString());
            }
        }

        private void ResumeAllBase()
        {
            var callbacksTmp = new List<KeyValuePair<IAudioWillResume, AudioSource>>();
            foreach (var iter in _audioInfos)
            {
                foreach (var info in iter.Value.audioCompnents)
                {
                    var stateTmp = info.audioTarget as IAudioWillResume;
                    if (null != stateTmp)
                        callbacksTmp.Add(new KeyValuePair<IAudioWillResume, AudioSource>(stateTmp, info.audioSource));
                }
            }
            for (int i = 0; i < callbacksTmp.Count; ++i)
            {
                var callbackInfo = callbacksTmp[i];
                try
                {
                    callbackInfo.Key.OnWillResume(callbackInfo.Value);
                }
                catch (System.Exception e)
                {
                    Log.Error("AudioManagerBase ResumeAllBase OnWillResume error: audioSource=" + callbackInfo.Value + " e=" + e);
                }
            }

            foreach (var iter in _audioInfos)
            {
                foreach (var info in iter.Value.audioCompnents)
                {
                    ResumeBase(info);
                }
            }
        }

        /// <summary>
        /// 暂停所有
        /// </summary>
        public void PauseAll()
        {
            try
            {
                PauseAllBase();
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} Pop exception: PauseAll e=" + e, this.ToTypeString());
            }
        }

        private void PauseAllBase()
        {
            var callbacksTmp = new List<KeyValuePair<IAudioWillPause, AudioSource>>();
            foreach (var iter in _audioInfos)
            {
                foreach (var info in iter.Value.audioCompnents)
                {
                    var stateTmp = info.audioTarget as IAudioWillPause;
                    if (null != stateTmp)
                        callbacksTmp.Add(new KeyValuePair<IAudioWillPause, AudioSource>(stateTmp, info.audioSource));
                }
            }
            for (int i = 0; i < callbacksTmp.Count; ++i)
            {
                var callbackInfo = callbacksTmp[i];
                try
                {
                    callbackInfo.Key.OnWillPause(callbackInfo.Value);
                }
                catch (System.Exception e)
                {
                    Log.Error("AudioManagerBase PauseAllBase OnWillPause error: audioSource=" + callbackInfo.Value + " e=" + e);
                }
            }

            foreach (var iter in _audioInfos)
            {
                foreach (var info in iter.Value.audioCompnents)
                {
                    PauseBase(info);
                }
            }
        }

        /// <summary>
        /// 停止所有
        /// </summary>
        public void StopAll()
        {
            try
            {
                StopAllBase();
            }
            catch (System.Exception e)
            {
                shaco.Log.ErrorFormat("{0} Pop exception: StopAll e=" + e, this.ToTypeString());
            }
        }

        private void StopAllBase()
        {
            var callbacksTmp = new List<KeyValuePair<IAudioWillStop, AudioSource>>();
            foreach (var iter in _audioInfos)
            {
                foreach (var info in iter.Value.audioCompnents)
                {
                    var stateTmp = info.audioTarget as IAudioWillStop;
                    if (null != stateTmp)
                        callbacksTmp.Add(new KeyValuePair<IAudioWillStop, AudioSource>(stateTmp, info.audioSource));
                }
            }
            for (int i = 0; i < callbacksTmp.Count; ++i)
            {
                var callbackInfo = callbacksTmp[i];
                try
                {
                    callbackInfo.Key.OnWillStop(callbackInfo.Value);
                }
                catch (System.Exception e)
                {
                    Log.Error("AudioManagerBase StopAllBase OnWillStop error: audioSource=" + callbackInfo.Value + " e=" + e);
                }
            }

            for (int i = _audioInfosSequeue.Count - 1; i >= 0; --i)
            {
                var iter = _audioInfos[_audioInfosSequeue[i]];
                for (int j = iter.audioCompnents.Count - 1; j >= 0; --j)
                {
                    var info = iter.audioCompnents[j];
                    if (StopBase(info))
                        iter.audioCompnents.RemoveAt(j);
                }

                if (0 == iter.audioCompnents.Count)
                {
                    _audioInfosSequeue.RemoveAt(i);
                    _audioInfos.Remove(iter.fileName);
                }
            }
        }

        /// <summary>
        /// 判断是否有该音频正在播放
        /// <param name="audio">音频</param>
        /// </summary>
        public bool IsAudioPlaying(IAudio audio)
        {
            var notState = GetAudioInfos(audio.fileName, AudioState.Playing, true).IsNullOrEmpty();
            return !notState;
        }

        /// <summary>
        /// 判断是否有该音频
        /// <param name="audio">音频</param>
        /// </summary>
        public bool HasAudio(IAudio audio)
        {
            return _audioInfos.ContainsKey(audio.fileName); 
        }

        /// <summary>
        /// 设置音量大小
        /// <param name="percent">音量大小百分比(0 ~ 1)</param>
        /// </summary>
        public void SetVolume(float percent)
        {
            _volume = percent;
            foreach (var iter in _audioInfos)
            {
                foreach (var info in iter.Value.audioCompnents)
                {
                    info.audioSource.volume = _volume;
                }
            }
        }

        /// <summary>
        /// 设置单个音频的音量大小
        /// <param name="audio">音频对象</param>
        /// <param name="percent">音量大小百分比(0 ~ 1)</param>
        /// </summary>
        public void SetVolume(IAudio audio, float percent)
        {
            AudioInfo findAudioInfo = null;
            if (!_audioInfos.TryGetValue(audio.fileName, out findAudioInfo))
            {
                shaco.Log.Error("AudioManagerBase SetVolume error: not found audio, name=" + audio.fileName);
                return;
            }

            for (int i = findAudioInfo.audioCompnents.Count - 1; i >= 0; --i)
            {
                findAudioInfo.audioCompnents[i].audioSource.volume = _volume * percent;
            }
        }

        /// <summary>
        /// 获取当前正在播放的音频
        /// </summary>
        public IAudio[] GetCurrentPlayingAudio()
        {
            var retValue = new List<IAudio>();
            for (int i = 0; i < _audioInfosSequeue.Count; ++i)
            {
                var audioComponentsTmp = _audioInfos[_audioInfosSequeue[i]].audioCompnents;
                for (int j = 0; j < audioComponentsTmp.Count; ++j)
                {
                    retValue.Add(audioComponentsTmp[i].audioTarget);
                }
            }
            return retValue.ToArray();
        }

        private bool StopBase(AudioInfoComponent audio)
        {
            if (audio == _currentWillPlayAudio)
                return false;
                
            var willState = audio.audioTarget as IAudioWillStop;
            if (null != willState)
                willState.OnWillStop(audio.audioSource);

            var convertTarget = audio.audioTarget as IAudioMultiVersionControlRelativePath;
            var multiVersionControlRelativePath = null == convertTarget ? string.Empty : convertTarget.multiVersionControlRelativePath;
            shaco.GameHelper.resCache.RetainStart(audio.audioTarget.fileName);

            CheckStartAudioVolumeAction(audio, false, () =>
            {
                audio.Stop();
                audio.DestroyWithPool();

                //卸载不再使用的音频
                shaco.GameHelper.resCache.RetainEnd(audio.audioTarget.fileName);

                //不能卸载当前正准备播放的声音
                AudioInfo findAudioInfo = null;
                bool hasPlayingSound = false;
                if (_audioInfos.TryGetValue(audio.audioTarget.fileName, out findAudioInfo))
                {
                    hasPlayingSound = null != findAudioInfo.audioCompnents.Find(v => v.isPlaying);
                }
                if (!hasPlayingSound)
                    shaco.GameHelper.resCache.UnloadAssetBundle(audio.audioTarget.fileName, true, multiVersionControlRelativePath);
            });
            return true;
        }

        private bool StopBase(string fileName)
        {
            AudioInfo findAudioInfo = null;
            if (!_audioInfos.TryGetValue(fileName, out findAudioInfo))
                return false;
                
            bool canStop = true;
            foreach (var iter in findAudioInfo.audioCompnents)
            {
                canStop &= StopBase(iter);
            }
            _audioInfosSequeue.RemoveOne(v => v == fileName);
            _audioInfos.Remove(fileName);
            return canStop;
        }

        private void ResumeBase(AudioInfoComponent audioComponent)
        {
            _currentWillPlayAudio = audioComponent;
            var willState = audioComponent.audioTarget as IAudioWillResume;
            if (null != willState)
            {
                try
                {
                    willState.OnWillResume(audioComponent.audioSource);
                }
                catch (System.Exception e)
                {
                    Log.Error("AudioManagerBase ResumeBase OnWillResume error: fileName=" + (null == audioComponent.audioTarget ? "null" : audioComponent.audioTarget.fileName) + " e=" + e, audioComponent);
                }
            }

            CheckStartAudioVolumeAction(audioComponent, true, null);
            audioComponent.UnPause();
            _currentWillPlayAudio = null;
        }

        public void PauseBase(AudioInfoComponent audioInfoComponent)
        {
            var willState = audioInfoComponent as IAudioWillPause;
            if (null != willState)
            {
                if (null != willState)
                {
                    try
                    {
                        willState.OnWillPause(audioInfoComponent.audioSource);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("AudioManagerBase PauseBase OnWillPause error: fileName=" + (null == audioInfoComponent.audioTarget ? "null" : audioInfoComponent.audioTarget.fileName) + " e=" + e, audioInfoComponent);
                    }
                }
            }

            CheckStartAudioVolumeAction(audioInfoComponent, false, () =>
            {
                audioInfoComponent.Pause();
            });
        }

        private AudioInfoComponent[] GetAudioInfos(string fileName, AudioState state, bool isFindOne)
        {
            List<AudioInfoComponent> retValue = null;
            AudioInfo findAudioInfo = null;
            _audioInfos.TryGetValue(fileName, out findAudioInfo);

            if (null != findAudioInfo)
            {
                for (int i = findAudioInfo.audioCompnents.Count - 1; i >= 0; --i)
                {
                    var audioComponnet = findAudioInfo.audioCompnents[i];

                    if (IsAudioState(audioComponnet, state))
                    {
                        if (null == retValue)
                            retValue = new List<AudioInfoComponent>();
                        retValue.Add(findAudioInfo.audioCompnents[i]);

                        if (isFindOne)
                            break;
                    }
                }
            }
            return null != retValue ? retValue.ToArray() : null;
        }

        private void CheckStartAudioVolumeAction(AudioInfoComponent audio, bool isOn, System.Action callbackEnd)
        {
            var switchTimeState = audio.audioTarget as IAudioSwitchTime;
            if (null == switchTimeState)
            {
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.ErrorFormat("{0} CheckStartAudioVolumeAction exception: fileName=" + (null == audio.audioTarget ? "null" : audio.audioTarget.fileName) + " e=" + e, this.ToTypeString());
                    }
                }
                return;
            }

            if (isOn)
            {
                audio.StartVolumeUpAction(switchTimeState.GetSwitchTime(), _volume, callbackEnd);
            }
            else
            {
                audio.StartVolumeDownAction(switchTimeState.GetSwitchTime(), _volume, callbackEnd);
            }
        }

        private bool IsAudioState(AudioInfoComponent audio, AudioState state)
        {
            var retValue = false;
            switch (state)
            {
                case AudioState.Paused:
                    {
                        if (!audio.isPlaying)
                            retValue = true;
                        break;
                    }
                case AudioState.Playing:
                    {
                        if (audio.isPlaying)
                            retValue = true;
                        break;
                    }
                default: shaco.Log.ErrorFormat("{0} IsAudioState error: unsupport state=" + state, this.ToTypeString()); break;
            }
            return retValue;
        }
    }
}