using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

static public class shaco_ExtensionsAudio
{
	// static public void PlaySound(this shaco.IAudio audio)
	// {
    //     shaco.GameHelper.sound.Play(audio);
    // }

	// static public void PauseSound(this shaco.IAudio audio)
	// {
    //     shaco.GameHelper.music.Pause(audio);
    // }
	
	// static public void StopSound(this shaco.IAudio audio)
	// {
    //     shaco.GameHelper.music.Stop(audio);
    // }

    // static public void PlayMusic(this shaco.IAudio audio)
    // {
    //     shaco.GameHelper.music.Play(audio);
    // }

    // static public void PauseMusic(this shaco.IAudio audio)
    // {
    //     shaco.GameHelper.music.Pause(audio);
    // }

    // static public void StopMusic(this shaco.IAudio audio)
    // {
    //     shaco.GameHelper.music.Stop(audio);
    // }

    static public void OnSoundPlayEnd(this shaco.IAudioSound audio, System.Action callbackEnd, GameObject autoRealseTarget = null)
    {
        OnAudioPlayEnd(audio, shaco.GameHelper.sound, callbackEnd, autoRealseTarget);
    }

    static public void OnMusicPlayEnd(this shaco.IAudioMusic audio, System.Action callbackEnd, GameObject autoRealseTarget = null)
    {
        OnAudioPlayEnd(audio, shaco.GameHelper.music, callbackEnd, autoRealseTarget);
    }

    static public void OnVoicePlayEnd(this shaco.IAudioVoice audio, System.Action callbackEnd, GameObject autoRealseTarget = null)
    {
        OnAudioPlayEnd(audio, shaco.GameHelper.voice, callbackEnd, autoRealseTarget);
    }

    static private void OnAudioPlayEnd(this shaco.IAudio audio, shaco.IAudioManager audioManager, System.Action callbackEnd, GameObject autoRealseTarget = null)
    {
        if (null == callbackEnd)
        {
            shaco.Log.Error("ExtensionsAudio OnAudioPlayEnd error: callbackEnd is null");
            return;
        }

        if (null == audioManager)
        {
            shaco.Log.Error("ExtensionsAudio OnAudioPlayEnd error: audioManager is null");
            return;
        }

        shaco.WaitFor.Run(() =>
        {
            return !audioManager.IsAudioPlaying(audio);
        }, () =>
        {
            try
            {
                callbackEnd();
            }
            catch (System.Exception e)
            {
                shaco.Log.Error("ExtensionsAudio OnAudioPlayEnd exception: e=" + e);
            }
        }, autoRealseTarget);
    }
}