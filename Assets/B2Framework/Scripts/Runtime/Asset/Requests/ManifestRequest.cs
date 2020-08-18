using System.IO;
using UnityEngine;

namespace B2Framework
{
    /// <summary>
    /// 加载Manifest
    /// 这里没有去加载Unity的AssetBundleManifest，而是使用了自己的BundleManifest
    /// </summary>
    public class ManifestRequest : AssetRequest
    {
        private BundleRequest _request;
        private string _assetName;
        public override float progress
        {
            get
            {
                switch (loadState)
                {
                    case LoadState.LoadAsset:
                        return _request.progress;

                    case LoadState.Loaded:
                        return 1f;
                    default:
                        break;
                }
                return string.IsNullOrEmpty(error) ? 1f : 0f;
            }
        }

        public override bool isDone
        {
            get
            {
                if (!string.IsNullOrEmpty(error))
                {
                    return true;
                }
                return loadState == LoadState.Loaded;
            }
        }
        internal override void Load()
        {
            _assetName = Path.GetFileNameWithoutExtension(url);
            if (GameUtility.Assets.runtimeMode)
            {
                _request = AssetsManger.LoadBundle(_assetName.ToLower(), true);
                _request.completed = Request_completed;
                loadState = LoadState.LoadAssetBundle;
            }
            else loadState = LoadState.Loaded;
        }
        private void Request_completed(AssetRequest req)
        {
            _request.completed = null;
            if (_request.assetBundle == null)
            {
                error = "assetBundle == null";
            }
            else
            {
                var manifest = _request.assetBundle.LoadAsset<BundleManifest>(_assetName);
                if (manifest == null)
                {
                    error = "manifest == null";
                }
                else
                {
                    AssetsManger.OnLoadManifest(manifest);
                    _request.assetBundle.Unload(true);
                    _request.assetBundle = null;
                }
            }
            loadState = LoadState.Loaded;
        }
        internal override void Unload()
        {
            if (_request != null)
            {
                _request.Release();
                _request = null;
            }
        }
    }
}