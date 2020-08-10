using UnityEngine;

namespace shaco
{
    public interface IAudioManager : shaco.Base.IGameInstance
    {
        /// <summary>
        /// 音量
        /// </summary>
        float volume { get; }

        /// <summary>
        /// 播放或者恢复播放
        /// <param name="audio">音频</param>
        /// </summary>
        void Play(IAudio audio);

        /// <summary>
        /// 关闭最后一次正在播放的音频
        /// <param name="isStop">true：关闭最后一次正在播放音乐 false：暂停最后一次正在播放音乐</param>
        /// </summary>
        void Pop(bool isStop = true);

        /// <summary>
        /// 暂停
        /// <param name="audio">音频</param>
        /// </summary>
        void Pause(IAudio audio);

        /// <summary>
        /// 停止
        /// <param name="audio">音频</param>
        /// <return>是否停止成功</return>
        /// </summary>
        bool Stop(IAudio audio);

        /// <summary>
        /// 恢复所有
        /// </summary>
        void ResumeAll();

        /// <summary>
        /// 暂停所有
        /// </summary>
        void PauseAll();

        /// <summary>
        /// 停止所有
        /// </summary>
        void StopAll();

        /// <summary>
        /// 判断是否有该音频正在播放
        /// <param name="audio">音频</param>
        /// </summary>
        bool IsAudioPlaying(IAudio audio);

        /// <summary>
        /// 判断是否有该音频
        /// <param name="audio">音频</param>
        /// </summary>
        bool HasAudio(IAudio audio);

        /// <summary>
        /// 设置音量大小
        /// <param name="percent">音量大小百分比(0 ~ 1)</param>
        /// </summary>
        void SetVolume(float percent);

        /// <summary>
        /// 设置单个音频的音量大小
        /// <param name="audio">音频对象</param>
        /// <param name="percent">音量大小百分比(0 ~ 1)</param>
        /// </summary>
        void SetVolume(IAudio audio, float percent);

        /// <summary>
        /// 获取当前正在播放的音频
        /// </summary>
        IAudio[] GetCurrentPlayingAudio();
    }
}