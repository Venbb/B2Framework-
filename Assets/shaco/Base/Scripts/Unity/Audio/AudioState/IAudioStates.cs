using UnityEngine;

namespace shaco
{
    public interface IAudioWillCreate
    {
        void OnWillCreate(string path, System.Action<AudioClip> callbackCreate);
    }

    public interface IAudioWillPlay
    {
        void OnWillPlay(AudioSource audio);
    }

    public interface IAudioWillResume
    {
        void OnWillResume(AudioSource audio);
    }

    public interface IAudioWillPause
    {
        void OnWillPause(AudioSource audio);
    }

    public interface IAudioWillStop
    {
        void OnWillStop(AudioSource audio);
    }

    public interface IAudioWillAutoStop
    {
        void OnWillAutoStop(AudioSource audio);
    }

    //音频切换时候所需时间
    public interface IAudioSwitchTime
    {
        float GetSwitchTime();
    }

    //判断音频是否可以播放
    public interface IAudioCanPlay
    {
        bool CanPlay(IAudioManager audioManager, IAudio audio);
    }

    //音频混合接口
    public interface IAudioMixer
    {
        //获取音频设置，并自动赋值到对应音频上
        UnityEngine.Audio.AudioMixerGroup GetAudioMixerGroup();
    }

    //资源版本路径，如没有该接口则默认为string.Empty
    public interface IAudioMultiVersionControlRelativePath
    {
        string multiVersionControlRelativePath { get; }
    }
}