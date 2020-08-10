//------------------------------------------------------------
// Game Framework v3.x
// Copyright © 2013-2017 Jiang Yin. All rights reserved.
// Homepage: http://gameframework.cn/
// Feedback: mailto:jiangyin@gameframework.cn
//------------------------------------------------------------

#if DEBUG_WINDOW
using GameFramework;
using System;
using UnityEngine;

namespace UnityGameFramework.Runtime
{
    /// <summary>
    /// 基础组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Base")]
    public sealed class BaseComponent : MonoBehaviour
    {
        private const int DefaultDpi = 96;  // default windows dpi

        private string m_GameVersion = string.Empty;
        private float m_GameSpeedBeforePause = 1f;

        // [SerializeField]
        // private bool m_EditorResourceMode = true;

        // [SerializeField]
        // private string m_ProfilerHelperTypeName = "UnityGameFramework.Runtime.ProfilerHelper";

        [SerializeField]
        private int m_FrameRate = 30;

        [SerializeField]
        private float m_GameSpeed = 1f;

        [SerializeField]
        private bool m_RunInBackground = true;

        [SerializeField]
        private int m_SleepTimeout = UnityEngine.SleepTimeout.SystemSetting;

        /// <summary>
        /// 获取或设置游戏版本号。
        /// </summary>
        public string GameVersion
        {
            get
            {
                return m_GameVersion;
            }
            set
            {
                m_GameVersion = value;
            }
        }

        // /// <summary>
        // /// 获取或设置是否使用编辑器资源模式（仅编辑器内有效）。
        // /// </summary>
        // public bool EditorResourceMode
        // {
        //     get
        //     {
        //         return m_EditorResourceMode;
        //     }
        //     set
        //     {
        //         m_EditorResourceMode = value;
        //     }
        // }

        /// <summary>
        /// 获取或设置游戏帧率。
        /// </summary>
        public int FrameRate
        {
            get
            {
                return m_FrameRate;
            }
            set
            {
                if (m_FrameRate != value)
                    Application.targetFrameRate = m_FrameRate = value;
            }
        }

        /// <summary>
        /// 获取或设置游戏速度。
        /// </summary>
        public float GameSpeed
        {
            get
            {
                return m_GameSpeed;
            }
            set
            {
                if (m_GameSpeed != value)
                    Time.timeScale = m_GameSpeed = (value >= 0f ? value : 0f);
            }
        }

        /// <summary>
        /// 获取游戏是否暂停。
        /// </summary>
        public bool IsGamePaused
        {
            get
            {
                return m_GameSpeed <= 0f;
            }
        }

        /// <summary>
        /// 获取是否正常游戏速度。
        /// </summary>
        public bool IsNormalGameSpeed
        {
            get
            {
                return m_GameSpeed == 1f;
            }
        }

        /// <summary>
        /// 获取或设置是否允许后台运行。
        /// </summary>
        public bool RunInBackground
        {
            get
            {
                return m_RunInBackground;
            }
            set
            {
                if (m_RunInBackground != value)
                    Application.runInBackground = m_RunInBackground = value;
            }
        }

        /// <summary>
        /// 获取或设置是否禁止休眠。
        /// </summary>
        public int SleepTimeout
        {
            get
            {
                return m_SleepTimeout;
            }
            set
            {
                if (m_SleepTimeout != value)
                {
                    //这个设定在PC上似乎不生效，无法设定，只能默认为系统时间模式
                    Screen.sleepTimeout = value;
                    m_SleepTimeout = Screen.sleepTimeout;
                }
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        void Awake()
        {
            // Log.Info("Game Framework version is {0}. Unity Game Framework version is {1}.", GameFrameworkEntry.Version, GameEntry.Version);
            // InitProfilerHelper();

            GameFramework.Utility.Converter.ScreenDpi = Screen.dpi;
            if (GameFramework.Utility.Converter.ScreenDpi <= 0)
            {
                GameFramework.Utility.Converter.ScreenDpi = DefaultDpi;
            }

            // m_EditorResourceMode &= Application.isEditor;
            // if (m_EditorResourceMode)
            // {
            //     // Log.Info("During this run, Game Framework will use editor resource files, which you should validate first.");
            // }

            FrameRate = Application.targetFrameRate;
            GameSpeed = Time.timeScale;
            SleepTimeout = Screen.sleepTimeout;
            RunInBackground = Application.runInBackground;

            //从本地文件读取版本号
            var versionFileNameWithoutExtension = shaco.Base.FileHelper.RemoveLastExtension(shaco.HotUpdateDefine.VERSION_PATH);

            if (shaco.GameHelper.res.ExistsResourcesOrLocal(versionFileNameWithoutExtension))
            {
                var buildVerionTmp = shaco.GameHelper.res.LoadResourcesOrLocal<TextAsset>(versionFileNameWithoutExtension);
                this.GameVersion = buildVerionTmp.ToString();
            }
            //获取unity PlayerSettings中版本号
            else
            {
                this.GameVersion = Application.version;
            }

#if UNITY_5_6_OR_NEWER
            Application.lowMemory += OnLowMemory;
#endif
        }

        private void Start()
        {

        }

        private void Update()
        {
            GameFrameworkEntry.Update(Time.deltaTime, Time.unscaledDeltaTime);
        }

        private void OnDestroy()
        {
#if UNITY_5_6_OR_NEWER
            Application.lowMemory -= OnLowMemory;
#endif
            GameFrameworkEntry.Shutdown();
        }

        /// <summary>
        /// 暂停游戏。
        /// </summary>
        public void PauseGame()
        {
            if (IsGamePaused)
            {
                return;
            }

            m_GameSpeedBeforePause = GameSpeed;
            GameSpeed = 0f;
        }

        /// <summary>
        /// 恢复游戏。
        /// </summary>
        public void ResumeGame()
        {
            if (!IsGamePaused)
            {
                return;
            }

            GameSpeed = m_GameSpeedBeforePause;
        }

        /// <summary>
        /// 重置为正常游戏速度。
        /// </summary>
        public void ResetNormalGameSpeed()
        {
            if (IsNormalGameSpeed)
            {
                return;
            }

            GameSpeed = 1f;
        }

        internal void Shutdown()
        {
            Destroy(gameObject);
        }

        // private void InitProfilerHelper()
        // {
        //     if (string.IsNullOrEmpty(m_ProfilerHelperTypeName))
        //     {
        //         return;
        //     }

        //     Type profilerHelperType = GameFramework.Utility.Assembly.GetTypeWithinLoadedAssemblies(m_ProfilerHelperTypeName);
        //     if (profilerHelperType == null)
        //     {
        //         shaco.Log.ErrorFormat("Can not find profiler helper type '{0}'.", m_ProfilerHelperTypeName);
        //         return;
        //     }

        //     GameFramework.Utility.Profiler.IProfilerHelper profilerHelper = (GameFramework.Utility.Profiler.IProfilerHelper)Activator.CreateInstance(profilerHelperType);
        //     if (profilerHelper == null)
        //     {
        //         shaco.Log.ErrorFormat("Can not create profiler helper instance '{0}'.", m_ProfilerHelperTypeName);
        //         return;
        //     }

        //     GameFramework.Utility.Profiler.SetProfilerHelper(profilerHelper);
        // }

        private void OnLowMemory()
        {
            shaco.Log.Info("Low memory reported...");

            shaco.GameHelper.objectpool.UnloadUnusedPoolData();
            // ObjectPoolComponent objectPoolComponent = GameEntry.GetComponent<ObjectPoolComponent>();
            // if (objectPoolComponent != null)
            // {
            //     objectPoolComponent.ReleaseAllUnused();
            // }
        }
    }
}
#endif