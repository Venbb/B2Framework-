using UnityEngine;

namespace B2Framework.Unity
{
    public static partial class GameUtility
    {
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
            if (size >= AppConst.SIZE_GB)
            {
                return Utility.Text.Format(speed ? "{0:f2}GB/s" : "{0:f2}GB", size / AppConst.SIZE_GB);
            }
            if (size >= AppConst.SIZE_MB)
            {
                return Utility.Text.Format(speed ? "{0:f2}MB/s" : "{0:f2}MB", size / AppConst.SIZE_MB);
            }
            if (size >= AppConst.SIZE_KB)
            {
                return Utility.Text.Format(speed ? "{0:f2}KB/s" : "{0:f2}KB", size / AppConst.SIZE_KB);
            }
            return Utility.Text.Format(speed ? "{0:f2}B/s" : "{0:f2}B", size);
        }
    }
}