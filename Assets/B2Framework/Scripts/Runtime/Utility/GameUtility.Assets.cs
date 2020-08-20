using System;
using System.IO;
using UnityEngine;

namespace B2Framework
{
    public static partial class GameUtility
    {
        public static partial class Assets
        {
            /// <summary>
            /// 替换Asset加载方法，非AssetBundle模式下使用
            /// 如Editor模式下AssetDatabase.LoadAssetAtPath
            /// </summary>
            public static Func<string, Type, UnityEngine.Object> loadHander;
            /// <summary>
            /// 是否使用运行时模式
            /// </summary>
            public static bool runtimeMode;
            /// <summary>
            /// 资源构建的平台，这个也是主Manifest的名字
            /// </summary>
            /// <value></value>
            public static string platform;
            /// <summary>
            /// 资源服务器地址
            /// </summary>
            public static string downloadURL;
            private static string _basePath;
            /// <summary>
            /// 默认的资源加载路径
            /// </summary>
            /// <value></value>
            public static string basePath
            {
                get
                {
                    if (string.IsNullOrEmpty(_basePath)) return streamingAssetsPath;
                    return _basePath;
                }
                set { _basePath = value; }
            }
            private static string _dataPath;
            /// <summary>
            /// 正式资源加载路径
            /// </summary>
            /// <value></value>
            public static string dataPath
            {
                get
                {
                    if (string.IsNullOrEmpty(_dataPath)) return persistentDataPath;
                    return _dataPath;
                }
                set { _dataPath = value; }
            }
            /// <summary>
            /// 资源存放的相对路径
            /// </summary>
            /// <value></value>
            public static string relativeDataPath
            {
                get
                {
                    return Utility.Path.Combine(GameConst.ASSETBUNDLES, runtimeMode ? platform + "/" : "");
                }
            }
            /// <summary>
            /// 本地持久化资源路径（可读写）
            /// </summary>
            /// <value></value>
            public static string persistentDataPath
            {
                get
                {
                    return Utility.Path.Combine(Application.persistentDataPath, relativeDataPath);
                }
            }
            /// <summary>
            /// streamingAssetsPath (只读)
            /// </summary>
            /// <value></value>
            public static string streamingAssetsPath
            {
                get
                {
                    return Utility.Path.Combine(Application.streamingAssetsPath, relativeDataPath);
                }
            }
            /// <summary>
            /// 资源下载路径
            /// </summary>
            /// <value></value>
            public static string downloadUrlPath
            {
                get
                {
                    return Utility.Path.Combine(downloadURL, relativeDataPath);
                }
            }
            /// <summary>
            /// persistentDataPath文件路径（www）
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string persistentDataRemotePath
            {
                get
                {
                    return "file://" + persistentDataPath;
                }
            }
            /// <summary>
            /// streamingAssetsPath文件路径（www）
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string streamingAssetsRemotePath
            {
                get
                {
                    var protocal = string.Empty;
                    if (Application.platform == RuntimePlatform.IPhonePlayer ||
                        Application.platform == RuntimePlatform.OSXEditor ||
                        Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        protocal = "file://";
                    }
                    else if (Application.platform == RuntimePlatform.OSXPlayer ||
                             Application.platform == RuntimePlatform.WindowsPlayer)
                    {
                        protocal = "file:///";
                    }

                    return protocal + streamingAssetsPath;
                }
            }
            /// <summary>
            /// 创建持久化目录
            /// </summary>
            /// <returns></returns>
            public static string CreatePersistentDataDir()
            {
                var path = persistentDataPath;
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                return path;
            }
            /// <summary>
            /// 获取持久化文件路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetPersistentDataPath(string path)
            {
                return Utility.Path.Combine(persistentDataPath, path);
            }
            /// <summary>
            /// 获取StreamingAssets文件路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetStreamingAssetsDataPath(string path)
            {
                return Utility.Path.Combine(streamingAssetsPath, path);
            }
            /// <summary>
            /// 获取本地持久化目录加载路径（www）
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetPersistentDataRemotePath(string path)
            {
                return Utility.Path.Combine(persistentDataRemotePath, path);
            }
            /// <summary>
            /// 获取StreamingAssets文件路径加载路径（www）
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetStreamingAssetsRemotePath(string path)
            {
                return Utility.Path.Combine(streamingAssetsRemotePath, path);
            }
            /// <summary>
            /// 获取文件下载地址
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetDownloadURL(string path)
            {
                return Utility.Path.Combine(downloadUrlPath, path);
            }
            /// <summary>
            /// 删除本地持久化数据
            /// </summary>
            public static void ClearPersistentData()
            {
                var path = persistentDataPath;
                if (Directory.Exists(path)) Directory.Delete(path, true);
            }
            /// <summary>
            /// 获取资源路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetDataPath(string path)
            {
                // 资源目录持久化目录，文件存在则返回资源路径
                var filePath = Utility.Path.Combine(dataPath, path);
                if (File.Exists(filePath)) return filePath;
                // 从默认资源目录返回资源路径
                filePath = Utility.Path.Combine(basePath, path);
                return filePath;
            }
            /// <summary>
            /// manifest文件路径
            /// </summary>
            /// <value></value>
            public static string manifestFilePath
            {
                get
                {
                    var path = string.Format("{0}.asset", GameConst.BUNDLE_MANIFEST);
                    return path = GetAssetPath(AssetBundles.Assets, path);
                }
            }
            /// <summary>
            /// AssetBundle资源路径
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetAssetPath(string path)
            {
                if (IsRemote(path)) return path;
                if (path.StartsWith(GameConst.ASSETBUNDLE_ASSETS_PATH)) return path;
                return Utility.Path.Combine(GameConst.ASSETBUNDLE_ASSETS_PATH, path);

            }
            /// <summary>
            /// AssetBundle资源路径
            /// </summary>
            /// <param name="bundle"></param>
            /// <param name="path"></param>
            /// <returns></returns>
            public static string GetAssetPath(AssetBundles bundle, string path)
            {
                path = string.Format("{0}/{1}", bundle, path);
                return GetAssetPath(path);
            }
            /// <summary>
            /// 是否远程请求
            /// </summary>
            /// <param name="path"></param>
            /// <returns></returns>
            public static bool IsRemote(string path)
            {
                if (path.StartsWith("http://", StringComparison.Ordinal) ||
                    path.StartsWith("https://", StringComparison.Ordinal) ||
                    path.StartsWith("file://", StringComparison.Ordinal) ||
                    path.StartsWith("ftp://", StringComparison.Ordinal) ||
                    path.StartsWith("jar:file://", StringComparison.Ordinal)) return true;
                return false;
            }
        }
    }
}