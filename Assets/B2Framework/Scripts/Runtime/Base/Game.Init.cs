using System;
using UnityEngine;

namespace B2Framework
{
    public partial class Game
    {
        [SerializeField]
        [HideInInspector]
        private string m_LogHelperTypeName = string.Empty;
        /// <summary>
        /// 初始化脚本
        /// </summary>
        protected void Init()
        {
            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            GameUtility.Assets.runtimeMode = runtimeMode;
            GameUtility.Assets.platform = buildPlatform;
            GameUtility.Assets.downloadURL = downloadURL;

            InitLogHelper();

            Log.Debug(buildPlatform);
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
            else
                Log.SetHelper(new DefaultLogHelper());
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