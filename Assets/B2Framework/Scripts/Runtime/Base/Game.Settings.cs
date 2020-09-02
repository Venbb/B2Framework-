using UnityEngine;

namespace B2Framework
{
    public partial class Game : MonoBehaviour
    {
        [Header("Base")]

        [Tooltip("是否开启日志")]
        [SerializeField]
        private bool m_DebugEnable = true;

        [Tooltip("游戏帧率")]
        [SerializeField]
        private int m_FrameRate = 30;

        [Tooltip("游戏速度")]
        [SerializeField]
        private float m_GameSpeed = 1f;

        [Tooltip("是否后台运行")]
        [SerializeField]
        private bool m_RunInBackground = true;

        [Tooltip("是否永不睡眠")]
        [SerializeField]
        private bool m_NeverSleep = true;

        [Tooltip("语言")]
        [SerializeField]
        private GameLanguage m_Language = GameLanguage.ChineseSimplified;

        [Header("Assets")]

        [SerializeField]
        private bool m_RuntimeMode = false;

        [Tooltip("资源构建平台")]
        [SerializeField]
        private string m_BuildPlatform;

        [Tooltip("资源服务器地址")]
        [SerializeField]
        private string m_DownloadURL = "http://127.0.0.1/b2";
        public bool debugEnable
        {
            get { return m_DebugEnable; }
            set { m_DebugEnable = value; }
        }
        /// <summary>
        /// 是否开启运行模式
        /// </summary>
        /// <value></value>
        public bool runtimeMode
        {
            get
            {
                return m_RuntimeMode = Application.isEditor ? m_RuntimeMode : true;
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
                return m_FrameRate;
            }
            set
            {
                Application.targetFrameRate = m_FrameRate = value;
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
                return m_Language;
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
                return m_GameSpeed;
            }
            set
            {
                Time.timeScale = m_GameSpeed = value >= 0f ? value : 0f;
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
                return m_GameSpeed <= 0f;
            }
        }
        /// <summary>
        /// 是否允许后台运行
        /// </summary>
        public bool runInBackground
        {
            get
            {
                return m_RunInBackground;
            }
            set
            {
                Application.runInBackground = m_RunInBackground = value;
            }
        }
        /// <summary>
        /// 是否禁止休眠
        /// </summary>
        public bool neverSleep
        {
            get
            {
                return m_NeverSleep;
            }
            set
            {
                m_NeverSleep = value;
                Screen.sleepTimeout = value ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;
            }
        }
        /// <summary>
        /// 资源构建平台
        /// </summary>
        /// <value></value>
        public string buildPlatform
        {
            get { return m_BuildPlatform; }
            set { m_BuildPlatform = value; }
        }
        /// <summary>
        /// 资源服务器地址
        /// </summary>
        /// <value></value>
        public string downloadURL
        {
            get { return m_DownloadURL; }
        }
    }
}