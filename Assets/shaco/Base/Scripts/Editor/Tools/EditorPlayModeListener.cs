using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class EditorPlayModeListener
{
    public enum PlayModeState
    {
        Stopped,
        Playing,
        Paused
    }

    private static PlayModeState _currentState = PlayModeState.Stopped;

    public static event Action<PlayModeState, PlayModeState> playModeChanged;

    static EditorPlayModeListener()
    {
#if UNITY_2017_2_OR_NEWER
        EditorApplication.playModeStateChanged += (state) =>
#else
        EditorApplication.playmodeStateChanged += () =>
#endif
        {
            var changedState = PlayModeState.Stopped;
            switch (_currentState)
            {
                case PlayModeState.Stopped:
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        changedState = PlayModeState.Playing;
                    }
                    break;
                case PlayModeState.Playing:
                    if (EditorApplication.isPaused)
                    {
                        changedState = PlayModeState.Paused;
                    }
                    else
                    {
                        changedState = PlayModeState.Stopped;
                    }
                    break;
                case PlayModeState.Paused:
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        changedState = PlayModeState.Playing;
                    }
                    else
                    {
                        changedState = PlayModeState.Stopped;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Fire PlayModeChanged event.
            OnPlayModeChanged(_currentState, changedState);

            // Set current state.
            _currentState = changedState;
        };
    }

    public static void Play()
    {
        EditorApplication.isPlaying = true;
    }

    public static void Pause()
    {
        EditorApplication.isPaused = true;
    }

    public static void Stop()
    {
        EditorApplication.isPlaying = false;
    }


    private static void OnPlayModeChanged(PlayModeState currentState, PlayModeState changedState)
    {
        if (playModeChanged != null)
            playModeChanged(currentState, changedState);
    }
}