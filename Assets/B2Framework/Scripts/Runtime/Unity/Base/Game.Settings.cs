using UnityEngine;

namespace B2Framework.Unity
{
    public partial class Game
    {
        [SerializeField]
        private bool _runtimeMode = false;

        [SerializeField]
        private int _frameRate = 30;

        [SerializeField]
        private float _gameSpeed = 1f;

        [SerializeField]
        private bool _runInBackground = true;

        [SerializeField]
        private bool _neverSleep = true;

        [SerializeField]
        private GameLanguage _language = GameLanguage.ChineseSimplified;
        /// <summary>
        /// 是否开启运行模式
        /// </summary>
        /// <value></value>
        public bool runtimeMode
        {
            get
            {
                return _runtimeMode = Application.isEditor ? _runtimeMode : true;
            }
        }
        /// <summary>
        /// 游戏帧率
        /// </summary>
        /// <value></value>
        public int frameRate
        {
            get
            {
                return _frameRate;
            }
            set
            {
                Application.targetFrameRate = _frameRate = value;
            }
        }
        /// <summary>
        /// 游戏语言
        /// </summary>
        /// <value></value>
        public GameLanguage language
        {
            get
            {
                return _language;
            }
        }
        /// <summary>
        /// 游戏速度
        /// </summary>
        /// <value></value>
        public float gameSpeed
        {
            get
            {
                return _gameSpeed;
            }
            set
            {
                Time.timeScale = _gameSpeed = value >= 0f ? value : 0f;
            }
        }
        /// <summary>
        /// 游戏是否暂停
        /// </summary>
        /// <value></value>
        public bool isGamePaused
        {
            get
            {
                return _gameSpeed <= 0f;
            }
        }
        /// <summary>
        /// 是否允许后台运行
        /// </summary>
        public bool runInBackground
        {
            get
            {
                return _runInBackground;
            }
            set
            {
                Application.runInBackground = _runInBackground = value;
            }
        }
        /// <summary>
        /// 是否禁止休眠
        /// </summary>
        public bool neverSleep
        {
            get
            {
                return _neverSleep;
            }
            set
            {
                _neverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }
    }
}