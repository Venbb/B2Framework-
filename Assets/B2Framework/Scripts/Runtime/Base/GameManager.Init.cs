using System;
using UnityEngine;

namespace B2Framework
{
    public partial class GameManager: MonoSingleton<GameManager>, IManager
    {
        [SerializeField]
        [HideInInspector]
        private string m_LogHelperTypeName = string.Empty;
        /// <summary>
        /// 初始化脚本
        /// </summary>
        public IManager Initialize()
        {
            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            GameUtility.Assets.runtimeMode = runtimeMode;
            GameUtility.Assets.platform = buildPlatform;
            GameUtility.Assets.downloadURL = downloadURL;

            InitLogHelper();

            return this;
        }
        /// <summary>
        /// 初始化日志打印
        /// </summary>
        void InitLogHelper()
        {
            if (string.IsNullOrEmpty(m_LogHelperTypeName)) return;
            if (typeof(DefaultLogHelper).FullName != m_LogHelperTypeName)
            {
                Log.SetHelper(Create<ILogHelper>(m_LogHelperTypeName));
            }
        }
        /// <summary>
        /// 通过反射实例化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private T Create<T>(string typeName) where T : class
        {
            var type = Utility.Assembly.GetType(typeName);
            if (type == null)
            {
                throw new Exception(string.Format("Can not find log helper type '{0}'.", typeName));
            }
            T instance = (T)Activator.CreateInstance(type);
            if (instance == null)
            {
                throw new Exception(string.Format("Can not create log helper instance '{0}'.", typeName));
            }
            return instance;
        }
    }
}