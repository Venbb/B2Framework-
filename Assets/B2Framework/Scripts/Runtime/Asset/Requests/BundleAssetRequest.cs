using System;
using System.IO;
using UnityEngine;

namespace B2Framework
{
    /// <summary>
    /// 同步方式加载AssetBundle里的资源
    /// </summary>
    public class BundleAssetRequest : AssetRequest
    {
        protected readonly string assetBundleName;
        protected BundleRequest bundleRequest;

        public BundleAssetRequest(string bundleName)
        {
            assetBundleName = bundleName;
        }
        internal override void Load()
        {
            bundleRequest = AssetsManger.LoadBundle(assetBundleName);
            var assetName = Path.GetFileName(url);
            asset = bundleRequest.assetBundle.LoadAsset(assetName, assetType);
        }
        internal override void Unload()
        {
            if (bundleRequest != null)
            {
                bundleRequest.Release();
                bundleRequest = null;
            }
            asset = null;
        }
    }
    /// <summary>
    /// 异步方式加载AssetBundle里的资源
    /// </summary>
    public class BundleAssetAsyncRequest : BundleAssetRequest
    {
        private AssetBundleRequest _request;
        public BundleAssetAsyncRequest(string bundleName) : base(bundleName) { }
        public override bool isDone
        {
            get
            {
                if (error != null || bundleRequest.error != null) return true;
                for (int i = 0, max = bundleRequest.dependencies.Count; i < max; i++)
                {
                    var item = bundleRequest.dependencies[i];
                    if (item.error != null) return true;
                }
                switch (loadState)
                {
                    case LoadState.Init:
                        return false;
                    case LoadState.Loaded:
                        return true;
                    case LoadState.LoadAssetBundle:
                        {
                            if (!bundleRequest.isDone) return false;
                            for (int i = 0, max = bundleRequest.dependencies.Count; i < max; i++)
                            {
                                var item = bundleRequest.dependencies[i];
                                if (!item.isDone) return false;
                            }
                            if (bundleRequest.assetBundle == null)
                            {
                                error = "assetBundle == null";
                                return true;
                            }
                            var assetName = Path.GetFileName(url);
                            _request = bundleRequest.assetBundle.LoadAssetAsync(assetName, assetType);
                            loadState = LoadState.LoadAsset;
                            break;
                        }
                    case LoadState.Unload:
                        break;
                    case LoadState.LoadAsset:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                if (loadState != LoadState.LoadAsset) return false;
                if (!_request.isDone) return false;
                asset = _request.asset;
                loadState = LoadState.Loaded;
                return true;
            }
        }

        public override float progress
        {
            get
            {
                var bundleProgress = bundleRequest.progress;
                if (bundleRequest.dependencies.Count <= 0) return bundleProgress * 0.3f + (_request != null ? _request.progress * 0.7f : 0);
                for (int i = 0, max = bundleRequest.dependencies.Count; i < max; i++)
                {
                    var item = bundleRequest.dependencies[i];
                    bundleProgress += item.progress;
                }
                return bundleProgress / (bundleRequest.dependencies.Count + 1) * 0.3f + (_request != null ? _request.progress * 0.7f : 0);
            }
        }

        internal override void Load()
        {
            bundleRequest = AssetsManger.LoadBundleAsync(assetBundleName);
            loadState = LoadState.LoadAssetBundle;
        }

        internal override void Unload()
        {
            _request = null;
            loadState = LoadState.Unload;
            base.Unload();
        }
    }
}
