using System;
using System.Collections;
using System.Collections.Generic;
using B2Framework.Unity;
using UnityEngine;

namespace B2Framework.Unity
{
    public partial class Assets
    {
        private static readonly int MAX_BUNDLES_PERFRAME = 0;
        internal static Dictionary<string, string> _assetToBundles = new Dictionary<string, string>();
        private static Dictionary<string, string[]> _bundleToDependencies = new Dictionary<string, string[]>();
        private static readonly Dictionary<string, BundleRequest> _bundles = new Dictionary<string, BundleRequest>();
        private static readonly List<BundleRequest> _unusedBundles = new List<BundleRequest>();
        private static readonly List<BundleRequest> _toloadBundles = new List<BundleRequest>();
        private static readonly List<BundleRequest> _loadingBundles = new List<BundleRequest>();
        private static List<string> _activeVariants = new List<string>();
        internal static void OnLoadManifest(BundleManifest manifest)
        {
            _activeVariants.AddRange(manifest.activeVariants);

            var assets = manifest.assets;
            var bundles = manifest.bundles;
            var dirs = manifest.dirs;

            for (var i = 0; i < bundles.Length; i++)
                _bundleToDependencies[bundles[i].bundleName] = Array.ConvertAll(bundles[i].deps, id => bundles[id].bundleName);

            _searchPaths.AddRange(dirs);

            AssetInfo item;
            for (int i = 0, max = assets.Length; i < max; i++)
            {
                item = assets[i];

                var path = Utility.Path.Combine(dirs[item.dir],item.fileName);
                if (item.bundle >= 0 && item.bundle < bundles.Length)
                {
                    _assetToBundles[path] = bundles[item.bundle].bundleName;
                }
                else
                {
                    Log.Error(string.Format("{0} bundle {1} not exist.", path, item.fileName));
                }
            }
        }
        internal static BundleRequest LoadBundle(string assetBundleName)
        {
            return LoadBundle(assetBundleName, false);
        }
        internal static BundleRequest LoadBundleAsync(string assetBundleName)
        {
            return LoadBundle(assetBundleName, true);
        }
        internal static bool GetAssetBundleName(string path, out string assetBundleName)
        {
            return _assetToBundles.TryGetValue(path, out assetBundleName);
        }
        private static string[] GetAllDependencies(string bundle)
        {
            string[] deps;
            if (_bundleToDependencies.TryGetValue(bundle, out deps))
                return deps;

            return new string[0];
        }
        private static void UpdateBundles()
        {
            var max = MAX_BUNDLES_PERFRAME;
            if (max > 0 && _toloadBundles.Count > 0 && _loadingBundles.Count < max)
            {
                for (int i = 0; i < Math.Min(max - _loadingBundles.Count, _toloadBundles.Count); ++i)
                {
                    var request = _toloadBundles[i];
                    if (request.loadState == LoadState.Init)
                    {
                        request.Load();
                        _loadingBundles.Add(request);
                        _toloadBundles.RemoveAt(i);
                        --i;
                    }
                }
            }

            for (int i = 0; i < _loadingBundles.Count; ++i)
            {
                var request = _loadingBundles[i];
                if (request.Update()) continue;
                _loadingBundles.RemoveAt(i);
                --i;
            }

            for (var i = 0; i < _unusedBundles.Count; i++)
            {
                var request = _unusedBundles[i];
                if (request.isDone)
                {
                    UnloadDependencies(request);
                    request.Unload();
                    Log.Debug("UnloadBundle: " + request.url);
                    _unusedBundles.RemoveAt(i);
                    i--;
                }
            }

            _unusedBundles.Clear();
        }
        internal static BundleRequest LoadBundle(string assetBundleName, bool async)
        {
            if (string.IsNullOrEmpty(assetBundleName))
            {
                Log.Error("assetBundleName == null");
                return null;
            }

            assetBundleName = RemapVariantName(assetBundleName);
            var url = GameUtility.Assets.GetDataPath(assetBundleName);

            BundleRequest request;
            if (_bundles.TryGetValue(url, out request))
            {
                request.Update();
                request.Retain();
                _loadingBundles.Add(request);
                return request;
            }
            if (url.StartsWith("http://", StringComparison.Ordinal) ||
                url.StartsWith("https://", StringComparison.Ordinal) ||
                url.StartsWith("file://", StringComparison.Ordinal) ||
                url.StartsWith("ftp://", StringComparison.Ordinal))
                request = new WebBundleRequest();
            else
                request = async ? new BundleAsyncRequest() : new BundleRequest();

            request.url = url;
            _bundles.Add(url, request);

            if (MAX_BUNDLES_PERFRAME > 0 && (request is BundleAsyncRequest || request is WebBundleRequest))
            {
                _toloadBundles.Add(request);
            }
            else
            {
                request.Load();
                _loadingBundles.Add(request);
                Log.Debug("LoadBundle: " + url);
            }

            LoadDependencies(request, assetBundleName, async);

            request.Retain();
            return request;
        }
        internal static void UnloadBundle(BundleRequest bundle)
        {
            bundle.Release();
        }
        internal static string RemapVariantName(string assetBundleName)
        {
            var bundlesWithVariant = _activeVariants;

            // Get base bundle name
            var baseName = assetBundleName.Split('.')[0];

            var bestFit = int.MaxValue;
            var bestFitIndex = -1;
            // Loop all the assetBundles with variant to find the best fit variant assetBundle.
            for (var i = 0; i < bundlesWithVariant.Count; i++)
            {
                var curSplit = bundlesWithVariant[i].Split('.');
                var curBaseName = curSplit[0];
                var curVariant = curSplit[1];

                if (curBaseName != baseName) continue;

                var found = bundlesWithVariant.IndexOf(curVariant);

                // If there is no active variant found. We still want to use the first
                if (found == -1) found = int.MaxValue - 1;

                if (found >= bestFit) continue;
                bestFit = found;
                bestFitIndex = i;
            }

            if (bestFit == int.MaxValue - 1)
                Log.Error("Ambiguous asset bundle variant chosen because there was no matching active variant: " + bundlesWithVariant[bestFitIndex]);

            return bestFitIndex != -1 ? bundlesWithVariant[bestFitIndex] : assetBundleName;
        }
        internal static void LoadDependencies(BundleRequest request, string assetBundleName, bool async)
        {
            var dependencies = GetAllDependencies(assetBundleName);
            if (dependencies.Length <= 0) return;
            for (var i = 0; i < dependencies.Length; i++)
            {
                var item = dependencies[i];
                request.dependencies.Add(LoadBundle(item, async));
            }
        }
        internal static void UnloadDependencies(BundleRequest request)
        {
            for (var i = 0; i < request.dependencies.Count; i++)
            {
                var item = request.dependencies[i];
                item.Release();
            }
            request.dependencies.Clear();
        }
    }
}