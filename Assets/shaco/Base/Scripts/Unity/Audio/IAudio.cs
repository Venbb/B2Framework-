using UnityEngine;

namespace shaco
{
    public abstract class IAudio
    {
        abstract public string fileName { get; }
    }

    public abstract class IAudioSound : IAudio
    {

    }

    public abstract class IAudioMusic : IAudio
    {

    }

    public abstract class IAudioVoice : IAudio
    {

    }
}