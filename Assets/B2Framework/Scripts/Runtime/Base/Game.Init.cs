using System;
using B2Framework.Net;
using UnityEngine;

namespace B2Framework
{
    public partial class Game : MonoBehaviour
    {
        [SerializeField]
        [HideInInspector]
        private string m_LogHelperTypeName = string.Empty;
        /// <summary>
        /// 初始化脚本
        /// </summary>
        public void Initialize()
        {
            Application.targetFrameRate = frameRate;
            Time.timeScale = gameSpeed;
            Application.runInBackground = runInBackground;
            Screen.sleepTimeout = neverSleep ? SleepTimeout.NeverSleep : SleepTimeout.SystemSetting;

            GameUtility.Assets.runtimeMode = runtimeMode;
            GameUtility.Assets.platform = buildPlatform;
            GameUtility.Assets.downloadURL = downloadURL;

            InitLogHelper();

            if (debugEnable)//Debug.isDebugBuild
            {
                Log.Info("====================================================================================");
                Log.Info("Application.platform = {0}", Application.platform);
                Log.Info("Application.dataPath = {0} , WritePermission: {1}", Application.dataPath, Utility.Files.HasWriteAccess(Application.dataPath));
                Log.Info("Application.streamingAssetsPath = {0} , WritePermission: {1}",
                    Application.streamingAssetsPath, Utility.Files.HasWriteAccess(Application.streamingAssetsPath));
                Log.Info("Application.persistentDataPath = {0} , WritePermission: {1}", Application.persistentDataPath,
                    Utility.Files.HasWriteAccess(Application.persistentDataPath));
                Log.Info("Application.temporaryCachePath = {0} , WritePermission: {1}", Application.temporaryCachePath,
                    Utility.Files.HasWriteAccess(Application.temporaryCachePath));
                Log.Info("Application.unityVersion = {0}", Application.unityVersion);
                Log.Info("SystemInfo.deviceModel = {0}", SystemInfo.deviceModel);
                Log.Info("SystemInfo.deviceUniqueIdentifier = {0}", SystemInfo.deviceUniqueIdentifier);
                Log.Info("SystemInfo.graphicsDeviceVersion = {0}", SystemInfo.graphicsDeviceVersion);
                Log.Info("====================================================================================");
            }
            var startInitTime = 0f;
            var startMem = 0L;
            if (debugEnable)
            {
                startInitTime = Time.time;
                startMem = GC.GetTotalMemory(false);// byte
            }

            sensitiveWordsFilter = SensitiveWordsFilter.Instance;
            preloadMgr = PreloadManager.Instance;
            luaMgr = LuaManager.Instance;
            netMgr = NetManager.Instance;
            sceneMgr = ScenesManager.Instance;

            if (debugEnable)
            {
                var nowMem = GC.GetTotalMemory(false);
                Log.Info("Init Modules Time:{0}, DiffMem:{1}, NowMem:{2}", Time.time - startInitTime, nowMem - startMem, nowMem);
            }
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