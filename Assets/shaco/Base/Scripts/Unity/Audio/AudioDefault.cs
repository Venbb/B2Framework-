using UnityEngine;

namespace shaco
{
    public class AudioDefault : IAudio, IAudioWillPlay, IAudioWillResume
    {
        override public string fileName { get { return _fileName; } }
        private string _fileName = string.Empty;
        private bool _isLoop = false;

        public AudioDefault(string fileName, bool isLoop = false)
        {
            _fileName = fileName;
            _isLoop = isLoop;
        }

        public void OnWillPlay(AudioSource audio)
        {
            audio.loop = _isLoop;
        }

        public void OnWillResume(AudioSource audio)
        {
            audio.loop = _isLoop;
        }
    }
}