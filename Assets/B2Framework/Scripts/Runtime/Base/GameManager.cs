using System;
using System.Collections.Generic;
using B2Framework.Net;
using UnityEngine;

namespace B2Framework
{
    public partial class GameManager : MonoSingleton<GameManager>, IManager
    {
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
        static Dictionary<string, IManager> m_Managers = new Dictionary<string, IManager>();
        public static void Register(IManager mgr)
        {
            var name = mgr.GetType().Name;
            if (!m_Managers.ContainsKey(name))
            {
                m_Managers.Add(name, mgr);
            }
        }
        public static T GetManager<T>() where T : IManager
        {
            return (T)GetManager(typeof(T));
        }
        public static IManager GetManager(Type type)
        {
            var name = type.Name;
            if (m_Managers.TryGetValue(name, out var manager))
            {
                return manager;
            }
            return null;
        }
        public static void DisposeManagers()
        {
            var go = m_Managers.GetEnumerator();
            while (go.MoveNext())
            {
                go.Current.Value.Dispose();
            }
            go.Dispose();
            m_Managers.Clear();
        }
        public static SensitiveWordsFilter sensitiveWordsFilter { get { return SensitiveWordsFilter.Instance; } }
        public static PreloadManager PreloadMgr { get { return PreloadManager.Instance; } }
        public static LuaManager LuaMgr { get { return LuaManager.Instance; } }
        public static NetManager NetMgr { get { return NetManager.Instance; } }
        public static ScenesManager SceneMgr { get { return ScenesManager.Instance; } }
    }
}
