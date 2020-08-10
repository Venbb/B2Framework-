using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    internal static partial class EditorUtility
    {
        /// <summary>
        /// 获取资源构建平台
        /// </summary>
        /// <param name="buildTarget"></param>
        /// <returns></returns>
        internal static Platform GetBuildPlatform(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    return Platform.Android;
                case BuildTarget.iOS:
                    return Platform.iOS;
                case BuildTarget.WebGL:
                    return Platform.WebGL;
                case BuildTarget.StandaloneWindows:
                    return Platform.Windows;
                case BuildTarget.StandaloneWindows64:
                    return Platform.Windows64;
                case BuildTarget.StandaloneOSX:
                    return Platform.MacOSX;
                default:
                    return Platform.Unknown;
            }
        }
        /// <summary>
        /// 获取（创建）配置脚本
        /// </summary>
        /// <param name="path"></param>
        /// <param name="true"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        internal static T GetScriptAsset<T>(string path, bool auto = true) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null && auto)
            {
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
            }

            return asset;
        }
        /// <summary>
        /// 拷贝资源
        /// </summary>
        /// <param name="source"></param>
        internal static string CopyAssets(string source, string dest, params string[] extents)
        {
            if (string.IsNullOrEmpty(source) || !Directory.Exists(source)) return null;
            // 获取目标文件夹
            var destPath = Utility.Path.Combine(dest, Path.GetFileName(source));
            // 如果目标文件夹存在，则删除
            if (Directory.Exists(destPath)) Directory.Delete(destPath, true);
            // 拷贝文件
            FileUtil.CopyFileOrDirectoryFollowSymlinks(source, destPath);
            // 删除其它
            if (extents.Length > 0) Utility.Files.DeleteFilesExcept(destPath, extents);
            return destPath;
        }
        internal static void DeleteDir(string path)
        {
            if (Utility.Files.DeleteDir(path))
            {
                // 删除meta文件
                path = path + ".meta";
                Utility.Files.DeleteFile(path);
            }
        }
        /// <summary>
        /// 清除所有AssetBundleName
        /// 工程中只要设置了AssetBundleName的，都会进行打包
        /// </summary>
        internal static void ClearAssetBundleNames()
        {
            // string[] allAssetBundleNames = AssetDatabase.GetAllAssetBundleNames();
            // for (int i = 0; i < allAssetBundleNames.Length; i++)
            // {
            //     string text = allAssetBundleNames[i];
            //     EditorUtility.DisplayProgressBar(Utility.Text.Format("Clear AssetBundles {0}/{1}", i, allAssetBundleNames.Length), text, i * 1f / allAssetBundleNames.Length);
            //     AssetDatabase.RemoveAssetBundleName(text, true);
            // }
            var assets = AssetDatabase.GetAllAssetPaths();
            // 获取所有有AssetBundleName的资源
            assets = System.Array.FindAll<string>(assets, (asset) =>
            {
                // 不带后缀的AssetBundle
                bool ok = !string.IsNullOrEmpty(AssetDatabase.GetImplicitAssetBundleName(asset));
                // 带后缀的AssetBundle
                if (!ok)
                    ok = !string.IsNullOrEmpty(AssetDatabase.GetImplicitAssetBundleVariantName(asset));
                return ok;
            });
            var max = assets.Length;
            for (int i = 0; i < max; i++)
            {
                var asset = assets[i];
                UnityEditor.EditorUtility.DisplayProgressBar(Utility.Text.Format("Clear AssetBundles {0}/{1}", i, max), asset, i * 1f / max);
                var ai = AssetImporter.GetAtPath(asset);
                if (!string.IsNullOrEmpty(ai.assetBundleName)) ai.assetBundleName = string.Empty;
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            UnityEditor.EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// 清除选中的资源AssetBundleName
        /// </summary>
        internal static void ClearSelectedAssetBundleNames()
        {
            Object[] selectedAsset = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
            var max = selectedAsset.Length;
            for (int i = 0; i < max; i++)
            {
                var asset = AssetDatabase.GetAssetPath(selectedAsset[i]);
                UnityEditor.EditorUtility.DisplayProgressBar(Utility.Text.Format("Clear AssetBundles {0}/{1}", i, max), asset, i * 1f / max);
                AssetImporter ai = AssetImporter.GetAtPath(asset);
                ai.assetBundleName = string.Empty; //清空文件夹中的资源AB名称
            }
            AssetDatabase.RemoveUnusedAssetBundleNames();
            AssetDatabase.Refresh();
            UnityEditor.EditorUtility.ClearProgressBar();
        }
        /// <summary>
        /// 验证是否为资源文件
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        internal static bool ValidateAsset(string asset)
        {
            if (!asset.StartsWith("Assets/")) return false;

            var ext = Path.GetExtension(asset).ToLower();
            return ext != ".dll" && ext != ".cs" && ext != ".meta" && ext != ".js" && ext != ".boo";
        }
        /// <summary>
        /// 设置资源的bundleName和variant
        /// </summary>
        /// <param name="assetPath"></param>
        /// <param name="bundleName"></param>
        /// <param name="variant"></param>
        internal static void SetAssetBundleNameAndVariant(string assetPath, string bundleName, string variant)
        {
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer == null) return;
            importer.assetBundleName = bundleName;
            importer.assetBundleVariant = variant;
        }
        /// <summary>
        /// 格式化AssetBundleName
        /// </summary>
        /// <param name="name"></param>
        /// <param name="hashName"></param>
        /// <returns></returns>
        internal static string GetAssetBundleName(string name, bool hashName = true)
        {
            name = name.ToPath();
            if (hashName)
            {
                return Utility.Verifier.GetMD5(name) + AppConst.ASSETBUNDLE_VARIANT;
            }
            return name.ToLower() + AppConst.ASSETBUNDLE_VARIANT;
        }
        internal static string[] GetAssetsPath(string path)
        {
            var paths = AssetDatabase.GetAllAssetPaths();
            return System.Array.FindAll(paths, (p) => p.Contains(path));
        }
    }
}