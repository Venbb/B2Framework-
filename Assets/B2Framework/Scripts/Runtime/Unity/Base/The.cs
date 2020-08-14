using UnityEngine;
using B2Framework.Unity.Net;

namespace B2Framework.Unity
{
    /// <summary>
    /// 全局统一的接口访问
    /// </summary>
    public static class The
    {
        /// <summary>
        /// 当前平台
        /// Unity3D 平台宏定义:https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
        /// </summary>
        /// <value></value>
        // public static string platform
        // {
        //     get
        //     {
        //         var platform = GameUtility.GetRuntimePlatform(Application.platform);
        //         return platform != Platform.Unknown ? platform.ToString() : null;
        //     }
        // }
        // public static SensitiveWordsFilter sensitiveWordsFilter { get { return SensitiveWordsFilter.Instance; } }
        // public static PreloadManager PreloadMgr { get { return PreloadManager.Instance; } }
        // public static LuaManager LuaMgr { get { return LuaManager.Instance; } }
        // public static NetManager NetMgr { get { return NetManager.Instance; } }
        // public static ScenesManager SceneMgr{ get { return ScenesManager.Instance; } }
    }
}