using UnityEngine;

namespace B2Framework
{
    public static partial class GameUtility
    {
        /// <summary>
        /// wifi或者有线网
        /// </summary>
        /// <value></value>
        public static bool IsWifi
        {
            get
            {
                return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
            }
        }
        /// <summary>
        /// 删除对象
        /// </summary>
        /// <param name="go"></param>
        /// <param name="delay"></param>
        public static void Destroy(GameObject go, float delay = 0)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                GameObject.DestroyImmediate(go);
            }
            else
            {
                GameObject.Destroy(go, delay);
            }
        }
        /// <summary>
        /// 删除所有子对象
        /// </summary>
        /// <param name="go"></param>
        public static void DestroyChildren(GameObject go)
        {
            var tran = go.transform;

            while (tran.childCount > 0)
            {
                var child = tran.GetChild(0);

                if (Application.isEditor && !Application.isPlaying)
                {
                    child.parent = null; // 清空父, 因为.Destroy非同步的
                    GameObject.DestroyImmediate(child.gameObject);
                }
                else
                {
                    GameObject.Destroy(child.gameObject);
                    // 预防触发对象的OnEnable，先Destroy
                    child.parent = null; // 清空父, 因为.Destroy非同步的
                }
            }
        }
        /// <summary>
        /// 获取资源构建平台
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static Platform GetBuildPlatform(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return Platform.Android;
                case RuntimePlatform.IPhonePlayer:
                    return Platform.iOS;
                case RuntimePlatform.WebGLPlayer:
                    return Platform.WebGL;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.WindowsEditor:
                    return Platform.Windows;
                case RuntimePlatform.OSXPlayer:
                case RuntimePlatform.OSXEditor:
                    return Platform.MacOSX;
                default:
                    return Platform.Unknown;
            }
        }
        /// <summary>
        /// 获取当前运行平台
        /// </summary>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static Platform GetRuntimePlatform(RuntimePlatform platform)
        {
            switch (platform)
            {
                case RuntimePlatform.Android:
                    return Platform.Android;
                case RuntimePlatform.IPhonePlayer:
                    return Platform.iOS;
                case RuntimePlatform.WebGLPlayer:
                    return Platform.WebGL;
                case RuntimePlatform.WindowsPlayer:
                    return Platform.Windows;
                case RuntimePlatform.OSXPlayer:
                    return Platform.MacOSX;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    return Platform.Editor;
                default:
                    return Platform.Unknown;
            }
        }
        /// <summary>
        /// 格式化字节大小
        /// </summary>
        /// <param name="size"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        public static string FormatSize(float size, bool speed = false)
        {
            if (size >= GameConst.SIZE_GB)
            {
                return Utility.Text.Format(speed ? "{0:f2}GB/s" : "{0:f2}GB", size / GameConst.SIZE_GB);
            }
            if (size >= GameConst.SIZE_MB)
            {
                return Utility.Text.Format(speed ? "{0:f2}MB/s" : "{0:f2}MB", size / GameConst.SIZE_MB);
            }
            if (size >= GameConst.SIZE_KB)
            {
                return Utility.Text.Format(speed ? "{0:f2}KB/s" : "{0:f2}KB", size / GameConst.SIZE_KB);
            }
            return Utility.Text.Format(speed ? "{0:f2}B/s" : "{0:f2}B", size);
        }
    }
}