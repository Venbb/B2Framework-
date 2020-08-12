using System;
using System.Collections.Generic;
using System.IO;
using B2Framework.Unity;

namespace B2Framework.Unity
{
    public partial class Assets
    {
        private static List<string> _searchPaths = new List<string>();
        public static AssetRequest LoadAsset(string path, Type type)
        {
            path = GameUtility.Assets.GetAssetPath(path);
            return LoadAsset(path, type, false);
        }
        public static AssetRequest LoadAssetAsync(string path, Type type)
        {
            path = GameUtility.Assets.GetAssetPath(path);
            return LoadAsset(path, type, true);
        }
        public static void UnloadAsset(AssetRequest asset)
        {
            asset.Release();
        }
        public static SceneAssetRequest LoadSceneAsync(string path, bool additive)
        {
            var asset = new SceneAssetAsyncRequest(path, additive);
            asset.Load();
            asset.Retain();
            _scenes.Add(asset);
            Log.Debug(Utility.Text.Format("LoadScene:{0}", path));
            return asset;
        }
        public static void UnloadScene(SceneAssetRequest scene)
        {
            scene.Release();
        }
        private static string GetExistPath(string path)
        {
            if (GameUtility.Assets.runtimeMode)
            {
                if (_assetToBundles.ContainsKey(path)) return path;
                for (var i = 0; i < _searchPaths.Count; i++)
                {
                    var existPath = string.Format("{0}/{1}", _searchPaths[i], path);
                    if (_assetToBundles.ContainsKey(existPath)) return existPath;
                }
            }
            else
            {
                if (File.Exists(path)) return path;
                for (var i = 0; i < _searchPaths.Count; i++)
                {
                    var existPath = string.Format("{0}/{1}", _searchPaths[i], path);
                    if (File.Exists(existPath)) return existPath;
                }
            }
            Log.Error("找不到资源路径" + path);
            return path;
        }
        public static void UnloadUnusedAssets()
        {
            foreach (var item in _assets)
            {
                if (item.Value.IsUnused())
                {
                    _unusedAssets.Add(item.Value);
                }
            }
            foreach (var request in _unusedAssets)
            {
                _assets.Remove(request.url);
            }
            foreach (var item in _bundles)
            {
                if (item.Value.IsUnused())
                {
                    _unusedBundles.Add(item.Value);
                }
            }
            foreach (var request in _unusedBundles)
            {
                _bundles.Remove(request.url);
            }
        }
        public static void Clear()
        {
            UnloadUnusedAssets();

            UpdateAssets();
            UpdateBundles();

            _searchPaths.Clear();
            _activeVariants.Clear();
            _assetToBundles.Clear();
            _bundleToDependencies.Clear();
        }
    }
}