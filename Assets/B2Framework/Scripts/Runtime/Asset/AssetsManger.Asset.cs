using System;
using System.Collections.Generic;

namespace B2Framework
{
    public partial class AssetsManger
    {
        // 当前正在加载的资源请求数据字典，相同资源的请求只会增加引用计数
        private static readonly Dictionary<string, AssetRequest> _assets = new Dictionary<string, AssetRequest>();
        // 当前正在加载的资源请求队列
        private static readonly List<AssetRequest> _loadingAssets = new List<AssetRequest>();
        // 当前正在加载的场景请求队列
        private static List<SceneAssetRequest> _scenes = new List<SceneAssetRequest>();
        // 临时存放的已完成的加载队列
        private static readonly List<AssetRequest> _unusedAssets = new List<AssetRequest>();
        /// <summary>
        /// 更新资源加载队列
        /// </summary>
        private static void UpdateAssets()
        {
            for (int i = 0; i < _loadingAssets.Count; ++i)
            {
                var request = _loadingAssets[i];
                if (request.Update()) continue;
                _loadingAssets.RemoveAt(i);
                --i;
            }

            for (var i = 0; i < _unusedAssets.Count; ++i)
            {
                var request = _unusedAssets[i];
                if (!request.isDone) continue;
                Log.Debug("UnloadAsset:{0}", request.url);
                request.Unload();
                _unusedAssets.RemoveAt(i);
                i--;
            }

            for (int i = 0; i < _scenes.Count; ++i)
            {
                var request = _scenes[i];
                if (request.Update() || !request.IsUnused()) continue;
                _scenes.RemoveAt(i);
                Log.Debug("UnloadScene:{0}", request.url);
                request.Unload();
                UnloadUnusedAssets();
                --i;
            }
        }
        /// <summary>
        /// 加载一个资源
        /// </summary>
        /// <param name="path">资源路径（名称）</param>
        /// <param name="type">资源类型</param>
        /// <param name="async">是否异步加载</param>
        /// <returns></returns>
        internal static AssetRequest LoadAsset(string path, Type type, bool async)
        {
            if (string.IsNullOrEmpty(path))
            {
                Log.Error("invalid path");
                return null;
            }
            
            path = GetExistPath(path);

            // 同个资源只增加引用计数，并返回当前正在加载的请求
            AssetRequest request;
            if (_assets.TryGetValue(path, out request))
            {
                // request.Update();
                _loadingAssets.Add(request);
                request.Retain();
                return request;
            }

            string assetBundleName;
            // 根据路径判断是否为AssetBundle，如果是则启动Bundle加载流程
            if (GetAssetBundleName(path, out assetBundleName))
            {
                request = async ? new BundleAssetAsyncRequest(assetBundleName) : new BundleAssetRequest(assetBundleName);
            }
            else
            {
                if (GameUtility.Assets.IsRemote(path))
                    request = new WebAssetRequest();
                else
                    request = new AssetRequest();
            }

            request.url = path;
            request.assetType = type;
            AddAssetRequest(request);
            request.Retain();
            Log.Debug("LoadAsset:{0}", path);
            return request;
        }

        private static void AddAssetRequest(AssetRequest request)
        {
            _assets.Add(request.url, request);
            _loadingAssets.Add(request);
            request.Load();
        }
    }
}