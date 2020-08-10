using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    public enum BundleNameBy
    {
        Custom,//Explicit,
        Filename,
        Directory,
    }
    [Serializable]
    public class AssetBuild
    {
        public string path;
        public BundleNameBy nameBy;
        public string assetBundleName;
    }

    [Serializable]
    public class BundleBuild
    {
        public string assetBundleName;
        public string[] assetNames;
        public AssetBundleBuild ToBuild()
        {
            return new AssetBundleBuild()
            {
                assetBundleName = assetBundleName,
                assetNames = assetNames
            };
        }
    }
    public class BuildRules : ScriptableObject
    {
        private readonly Dictionary<string, string> _asset2Bundles = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _conflicted = new Dictionary<string, string[]>();
        private readonly List<string> _duplicated = new List<string>();
        private readonly Dictionary<string, HashSet<string>> _tracker = new Dictionary<string, HashSet<string>>();

        [Tooltip("每次打资源包版本号自动 +1")]
        public int version;
        [Tooltip("是否把AssetBundle名字哈希处理")]
        public bool hashBundleName = true;

        [Tooltip("BuildPlayer 的时候被打包的场景")]
        public UnityEngine.Object[] scenes = new UnityEngine.Object[0];

        [Tooltip("所有要打包的资源")]
        public AssetBuild[] assets = new AssetBuild[0];

        [Tooltip("所有Assetbundle")]
        public BundleBuild[] bundles = new BundleBuild[0];
        public void GroupAsset(string path, BundleNameBy nameBy = BundleNameBy.Filename)
        {
            var asset = ArrayUtility.Find(assets, assetBuild => assetBuild.path.Equals(path));
            if (asset != null)
            {
                asset.nameBy = nameBy;
                return;
            }
            ArrayUtility.Add(ref assets, new AssetBuild()
            {
                path = path,
                nameBy = nameBy,
            });
        }
        /// <summary>
        /// 升级资源版本号
        /// </summary>
        /// <returns></returns>
        public int IncreaseVersion()
        {
            version += 1;
            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return version;
        }
        /// <summary>
        /// 重置版本号
        /// </summary>
        /// <returns></returns>
        public int ResetVersion()
        {
            version = 0;
            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return version;
        }
        /// <summary>
        /// 获取需要构建的AssetBundle
        /// </summary>
        /// <returns></returns>
        public AssetBundleBuild[] GetBuilds()
        {
            return Array.ConvertAll(bundles, input => input.ToBuild());
        }
        public void Reset()
        {
            version = 0;
            hashBundleName = true;
            scenes = new UnityEngine.Object[0];
            assets = new AssetBuild[0];
            bundles = new BundleBuild[0];
        }
        /// <summary>
        /// 执行规则
        /// </summary>
        public void Apply()
        {
            Clear();
            CollectAssets();
            AnalysisAssets();
            OptimizeAssets();
            Save();
        }
        /// <summary>
        /// 收集资源
        /// </summary>
        private void CollectAssets()
        {
            var list = new List<AssetBuild>();
            for (var index = 0; index < this.assets.Length; index++)
            {
                var asset = this.assets[index];
                if (File.Exists(asset.path) && EditorUtility.ValidateAsset(asset.path))
                {
                    list.Add(asset);
                }
            }

            foreach (var asset in list)
            {
                asset.assetBundleName = GetAssetBundleName(asset);
                _asset2Bundles[asset.path] = asset.assetBundleName;
            }

            assets = list.ToArray();
        }
        private string GetAssetBundleName(AssetBuild assetBuild)
        {
            var assetBundleName = string.Empty;
            if (assetBuild.path.EndsWith(".shader"))
            {
                return EditorUtility.GetAssetBundleName("shaders", hashBundleName);
            }
            switch (assetBuild.nameBy)
            {
                case BundleNameBy.Custom:
                    return EditorUtility.GetAssetBundleName(assetBuild.assetBundleName, hashBundleName);
                case BundleNameBy.Filename:
                    return EditorUtility.GetAssetBundleName(Utility.Path.GetFilePathWithoutExtension(assetBuild.path), hashBundleName);
                case BundleNameBy.Directory:
                    return EditorUtility.GetAssetBundleName(Utility.Path.GetDirectoryName(assetBuild.path), hashBundleName);
                default:
                    return string.Empty;
            }
        }
        /// <summary>
        /// 分析依赖
        /// </summary>
        private void AnalysisAssets()
        {
            var bundles = GetBundles();
            int i = 0, max = bundles.Count;
            foreach (var item in bundles)
            {
                var bundle = item.Key;
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(Utility.Text.Format("分析依赖{0}/{1}", i, max), bundle, i / (float)max)) break;
                var assetPaths = bundles[bundle];
                if (assetPaths.Exists(IsScene) && !assetPaths.TrueForAll(IsScene))
                    _conflicted.Add(bundle, assetPaths.ToArray());
                var dependencies = AssetDatabase.GetDependencies(assetPaths.ToArray(), true);
                foreach (var asset in dependencies)
                {
                    if (EditorUtility.ValidateAsset(asset)) Track(asset, bundle);
                }
                i++;
            }
        }
        /// <summary>
        /// 获取所有AssetBundles
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<string>> GetBundles()
        {
            var bundles = new Dictionary<string, List<string>>();
            foreach (var item in _asset2Bundles)
            {
                var bundle = item.Value;
                List<string> list;
                if (!bundles.TryGetValue(bundle, out list))
                {
                    list = new List<string>();
                    bundles[bundle] = list;
                }

                if (!list.Contains(item.Key)) list.Add(item.Key);
            }

            return bundles;
        }
        private bool IsScene(string asset)
        {
            return asset.EndsWith(".unity");
        }
        private void Track(string asset, string bundle)
        {
            HashSet<string> assets;
            if (!_tracker.TryGetValue(asset, out assets))
            {
                assets = new HashSet<string>();
                _tracker.Add(asset, assets);
            }

            assets.Add(bundle);
            if (assets.Count > 1)
            {
                string bundleName;
                _asset2Bundles.TryGetValue(asset, out bundleName);
                if (string.IsNullOrEmpty(bundleName))
                {
                    _duplicated.Add(asset);
                }
            }
        }
        private void OptimizeAssets()
        {
            int i = 0, max = _conflicted.Count;
            foreach (var item in _conflicted)
            {
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(Utility.Text.Format("优化冲突{0}/{1}", i, max), item.Key, i / (float)max)) break;
                var list = item.Value;
                foreach (var asset in list)
                {
                    if (!IsScene(asset)) _duplicated.Add(asset);
                }
                i++;
            }

            for (i = 0, max = _duplicated.Count; i < max; i++)
            {
                var item = _duplicated[i];
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar(Utility.Text.Format("优化冗余{0}/{1}", i, max), item, i / (float)max)) break;
                OptimizeAsset(item);
            }
        }
        private void OptimizeAsset(string asset)
        {
            if (asset.EndsWith(".shader"))
                _asset2Bundles[asset] = EditorUtility.GetAssetBundleName("shaders", hashBundleName);
            else
                _asset2Bundles[asset] = EditorUtility.GetAssetBundleName(asset, hashBundleName);
        }
        private void Save()
        {
            var getBundles = GetBundles();
            bundles = new BundleBuild[getBundles.Count];
            var i = 0;
            foreach (var item in getBundles)
            {
                bundles[i] = new BundleBuild
                {
                    assetBundleName = item.Key,
                    assetNames = item.Value.ToArray()
                };
                i++;
            }

            UnityEditor.EditorUtility.ClearProgressBar();
            UnityEditor.EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
        private void Clear()
        {
            _tracker.Clear();
            _duplicated.Clear();
            _conflicted.Clear();
            _asset2Bundles.Clear();
        }
    }
    [CustomEditor(typeof(BuildRules))]
    public class BuildRulesEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BuildRules rules = target as BuildRules;
            EditorGUILayout.HelpBox("【操作提示】" +
                "\n    - 编辑器菜单中 Assets/Group AssetBundle进行打包分组：" +
                "\n【注意事项】：" +
                "\n    - 所有shader放到一个包" +
                "\n    - 场景文件不可以和非场景文件放到一个包" +
                "\n    - 预知体通常单个文件一个包" +
                "\n    - 资源冗余可以自行集成 AssetBundle-Browser 查看", MessageType.None);

            using (var h = new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(16);
                if (GUILayout.Button("清理"))
                {
                    if (UnityEditor.EditorUtility.DisplayDialog("提示", "是否确定清理？", "确定"))
                    {
                        rules.Reset();
                        EditorUtility.ClearAssetBundleNames();
                    }
                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("执行"))
                {
                    rules.Apply();
                    GUIUtility.ExitGUI();
                }
                GUILayout.Space(16);
            }
            base.OnInspectorGUI();
        }
    }
}