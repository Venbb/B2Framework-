using System;
using System.Collections.Generic;
using B2Framework.Net;
using UnityEngine;

namespace B2Framework
{
    public partial class Game : MonoBehaviour
    {
        /// <summary>
        /// 游戏单例
        /// </summary>
        /// <value></value>
        public static Game Instance { get; private set; }
        public static SensitiveWordsFilter sensitiveWordsFilter { get; private set; }
        public static PreloadManager preloadMgr { get; private set; }
        public static LuaManager luaMgr { get; private set; }
        public static NetManager netMgr { get; private set; }
        public static ScenesManager sceneMgr { get; private set; }
        /// <summary>
        /// 当前平台
        /// Unity3D 平台宏定义:https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
        /// </summary>
        /// <value></value>
        public static string platform
        {
            get
            {
                var platform = GameUtility.GetRuntimePlatform(Application.platform);
                return platform != Platform.Unknown ? platform.ToString() : null;
            }
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        public static void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
