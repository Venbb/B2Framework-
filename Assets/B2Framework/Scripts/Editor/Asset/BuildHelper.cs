using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using B2Framework;
using System;

namespace B2Framework.Editor
{
    /// <summary>
    /// 构建本地资源和AssetBundle
    /// </summary>
    public static class BuildHelper
    {
        /// <summary>
        /// 构建全局配置文件
        /// </summary>
        public static Settings GetSettings()
        {
            var path = "Assets/Resources/GameSettings.asset";
            return EditorUtility.GetScriptAsset<Settings>(path);
        }
        /// <summary>
        /// 打包规则
        /// </summary>
        /// <returns></returns>
        public static BuildRules GetBuildRules()
        {
            var path = "Assets/Rules.asset";
            return EditorUtility.GetScriptAsset<BuildRules>(path);
        }
        /// <summary>
        /// 打包设置
        /// </summary>
        /// <returns></returns>
        public static BuildSettings GetBuildSettings()
        {
            var path = "Assets/BuildSettings.asset";
            return EditorUtility.GetScriptAsset<BuildSettings>(path);
        }
        /// <summary>
        /// 获取Manifest配置
        /// </summary>
        /// <returns></returns>
        public static BundleManifest GetManifest()
        {
            var path = Utility.Assets.manifestFilePath;
            return EditorUtility.GetScriptAsset<BundleManifest>(path);
        }
        /// <summary>
        /// 获取扩展工具数据资源
        /// </summary>
        /// <returns></returns>
        public static ExToolsObject GetExTools()
        {
            var path = "Assets/ExTools.asset";
            return EditorUtility.GetScriptAsset<ExToolsObject>(path);
        }
        /// <summary>
        /// 执行资源打包规则
        /// </summary>
        public static void ApplyBuildRules()
        {
            var rules = GetBuildRules();
            rules.Apply();
        }
        /// <summary>
        /// 给资源打包分组
        /// </summary>
        /// <param name="nameBy"></param>
        public static void GroupAssets(BundleNameBy nameBy)
        {
            var rules = GetBuildRules();
            var selection = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets);
            foreach (var o in selection)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path) || Directory.Exists(path)) continue;
                rules.GroupAsset(path, nameBy);
            }
            UnityEditor.EditorUtility.SetDirty(rules);
            AssetDatabase.SaveAssets();

            Selection.activeObject = rules;
            UnityEditor.EditorUtility.FocusProjectWindow();
        }
        /// <summary>
        /// 获取包输出目录
        /// </summary>
        /// <returns></returns>
        public static string GeReleaseOutPutPath(BuildTarget buildTarget)
        {
            return AppConst.RELEASE_DIR;
        }
        /// <summary>
        /// 获取AssetBundles输出目录
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBundlesOutPutPath(BuildTarget buildTarget)
        {
            return Utility.Path.Combine(AppConst.ASSETBUNDLES, BuildHelper.GetPlatformName(buildTarget));
        }
        /// <summary>
        /// 一键打包
        /// </summary>
        public static void BuildAll()
        {
            BuildPlayer();
            BuildAssetBundles();
        }
        /// <summary>
        /// 一键打包
        /// </summary>
        public static void BuildPlayer()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var releasePath = GeReleaseOutPutPath(buildTarget);
            BuildPlayer(releasePath, buildTarget);
        }
        /// <summary>
        /// 一键构建AssetBundle
        /// </summary>
        public static void BuildAssetBundles()
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var outputPath = GetAssetBundlesOutPutPath(buildTarget);
            var options = BuildAssetBundleOptions.ChunkBasedCompression;
            BuildAssetBundles(outputPath, options, buildTarget);
        }
        /// <summary>
        /// 打包
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="target"></param>
        /// <param name="options"></param>
        public static void BuildPlayer(string outputPath, BuildTarget target, BuildOptions options = BuildOptions.None)
        {
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
            // 递增版本号
            // IncreaseVersion(target);
            // 包输出路径
            var locationPathName = Utility.Path.Combine(outputPath, GetBuildTargetName(target));
            // 开始打包
            BuildPipeline.BuildPlayer(GetBuildScenes(), locationPathName, target, options);
        }
        /// <summary>
        /// 资源打包
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="options"></param>
        /// <param name="targetPlatform"></param>
        public static void BuildAssetBundles(string outputPath, BuildAssetBundleOptions options, BuildTarget targetPlatform)
        {
            // 输出目录不存在则创建
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
            // 获取打包规则
            var rules = GetBuildRules();
            // 获取要打包的Asset
            var builds = rules.GetBuilds();
            // 开始打包
            var assetBundleManifest = BuildPipeline.BuildAssetBundles(outputPath, builds, options, targetPlatform);
            if (assetBundleManifest == null)
            {
                Debug.LogError("no assetbundle !!"); return;
            }
            // 获取所有AssetBundleName
            var bundleNames = assetBundleManifest.GetAllAssetBundles();

            // 给所有的AssetBundleName分配索引
            var tempDic = new Dictionary<string, int>();
            var index = 0;
            foreach (var name in bundleNames) { tempDic[name] = index; index++; }

            // 解析每个AssetBundle，依赖、大小、hash值
            var bundles = new List<BundleInfo>();
            for (var i = 0; i < bundleNames.Length; i++)
            {
                var bundleName = bundleNames[i];
                var deps = assetBundleManifest.GetAllDependencies(bundleName);
                var path = Utility.Text.Format("{0}/{1}", outputPath, bundleName);
                if (!File.Exists(path))
                {
                    Debug.LogError(path + " file not exsit.");
                    continue;
                }
                using (var stream = File.OpenRead(path))
                {
                    bundles.Add(new BundleInfo
                    {
                        bundleName = bundleName,
                        deps = Array.ConvertAll(deps, name => tempDic[name]),
                        len = stream.Length,
                        hash = assetBundleManifest.GetAssetBundleHash(bundleName).ToString(),
                        crc = Utility.Verifier.GetCRC32(stream)
                    });
                }
            }

            // 资源存放的目录
            var dirs = new List<string>();
            // 所有的资源信息
            var assets = new List<AssetInfo>();

            // 解析所有打包的资源，对路径和AssetBundleName做映射
            for (int i = 0; i < rules.assets.Length; i++)
            {
                var item = rules.assets[i];
                var dir = Path.GetDirectoryName(item.path).ToPath();
                index = dirs.FindIndex(d => d.Equals(dir));
                if (index == -1)
                {
                    index = dirs.Count;
                    dirs.Add(dir);
                }

                var asset = new AssetInfo();
                asset.fileName = Path.GetFileName(item.path);
                if (!tempDic.TryGetValue(item.assetBundleName, out asset.bundle))
                {
                    var bundle = new BundleInfo();
                    var id = bundles.Count;
                    bundle.bundleName = asset.fileName;
                    using (var stream = File.OpenRead(item.path))
                    {
                        bundle.len = stream.Length;
                        bundle.crc = Utility.Verifier.GetCRC32(stream);
                    }

                    bundles.Add(bundle);
                    asset.bundle = id;
                }
                asset.dir = index;
                assets.Add(asset);
            }

            // 获取存储BundleManifest的asset
            var manifest = GetManifest();
            // 将数据写到asset文件
            manifest.dirs = dirs.ToArray();
            manifest.assets = assets.ToArray();
            manifest.bundles = bundles.ToArray();

            UnityEditor.EditorUtility.SetDirty(manifest);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 最后将manifest.asset打包
            var assetPath = AssetDatabase.GetAssetPath(manifest);
            var manifestBundleName = Path.GetFileNameWithoutExtension(assetPath).ToLower();
            builds = new[]
             {
                new AssetBundleBuild
                {
                    assetNames = new[] { assetPath },
                    assetBundleName = manifestBundleName
                }
            };
            BuildPipeline.BuildAssetBundles(outputPath, builds, options, targetPlatform);
            {
                var path = outputPath + "/" + manifestBundleName;
                var bundle = new BundleInfo();
                bundle.bundleName = Path.GetFileName(path);
                using (var stream = File.OpenRead(path))
                {
                    bundle.len = stream.Length;
                    bundle.crc = Utility.Verifier.GetCRC32(stream);
                }
                bundles.Add(bundle);
            }
            ArrayUtility.Add(ref bundleNames, manifestBundleName);

            // 生成版本文件
            BuildVersions(outputPath, bundles, rules.IncreaseVersion());
        }
        /// <summary>
        /// 生成版本文件
        /// </summary>
        /// <param name="outputPath"></param>
        /// <param name="bundles"></param>
        /// <param name="versionCode"></param>
        public static void BuildVersions(string outputPath, List<BundleInfo> bundles, int versionCode)
        {
            var path = outputPath + "/" + AppConst.RES_VER_FILE;
            if (File.Exists(path)) File.Delete(path);
            var files = new List<VFile>();
            foreach (var bundle in bundles)
            {
                files.Add(new VFile()
                {
                    name = bundle.bundleName,
                    hash = bundle.crc,
                    len = bundle.len,
                });
            }
            var ver = new B2Framework.Version();
            ver.ver = versionCode;
            ver.files = files;

            using (var stream = File.OpenWrite(path))
            {
                var writer = new BinaryWriter(stream);
                ver.Serialize(writer);
            }
        }
        /// <summary>
        /// 递增版本号
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static string IncreaseVersion(BuildTarget target)
        {
            var bundleVersion = PlayerSettings.bundleVersion;
            System.Version version;
            if (!System.Version.TryParse(bundleVersion, out version))
            {
                Debug.Log("bundleVersion is empty!!!!");
                return bundleVersion;
            }
            // 递增版本号，最后一位自动+1
            var vers = bundleVersion.Split('.');
            var versionCode = vers[vers.Length - 1] += 1;
            PlayerSettings.bundleVersion = bundleVersion = string.Join(".", vers);

            //这个版本号用来大版本更新
            switch (target)
            {
                case BuildTarget.Android:
                    PlayerSettings.Android.bundleVersionCode = versionCode.ToInt();
                    break;
                case BuildTarget.iOS:
                    PlayerSettings.iOS.buildNumber = versionCode;
                    break;
            }
            // 重置资源版本号
            GetBuildRules().ResetVersion();
            return bundleVersion;
        }
        /// <summary>
        /// 获取需要打包的场景
        /// </summary>
        /// <returns></returns>
        public static string[] GetBuildScenes()
        {
            List<string> names = new List<string>();
            foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
            {
                if (e != null && e.enabled) names.Add(e.path);
            }
            return names.ToArray();
        }
        /// <summary>
        /// 获取构建平台，用于生成资源根目录
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformName(BuildTarget buildTarget)
        {
            var platform = EditorUtility.GetBuildPlatform(buildTarget);
            return platform != Platform.Unknown ? platform.ToString() : null;
        }
        private static string GetBuildTargetName(BuildTarget target)
        {
            var time = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var name = PlayerSettings.productName + "-v" + GetVersion();
            switch (target)
            {
                case BuildTarget.Android:
                    return Utility.Text.Format("{0}-{1}.apk", name, time);
                case BuildTarget.iOS:
                    return Utility.Text.Format("{0}-{1}.ipa", name, time);
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    return Utility.Text.Format("{0}-{1}.exe", name, time);
                case BuildTarget.StandaloneOSX:
                    return name + ".app";
                case BuildTarget.WebGL:
                    return "";
                default:
                    Debug.Log("Target not implemented.");
                    return null;
            }
        }
        private static string GetVersion()
        {
            return Utility.Text.Format("{0}.{1}", PlayerSettings.bundleVersion, GetBuildRules().version);
        }
    }
}