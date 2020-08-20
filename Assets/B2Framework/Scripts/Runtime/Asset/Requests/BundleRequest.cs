using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace B2Framework
{
    /// <summary>
    /// AssetBundle 加载器（同步）
    /// </summary>
    public class BundleRequest : AssetRequest
    {
        public readonly List<BundleRequest> dependencies = new List<BundleRequest>();

        public AssetBundle assetBundle
        {
            get { return asset as AssetBundle; }
            internal set { asset = value; }
        }

        internal override void Load()
        {
            asset = AssetBundle.LoadFromFile(url);
            if (assetBundle == null) error = url + " LoadFromFile failed.";
        }

        internal override void Unload()
        {
            if (assetBundle == null) return;
            assetBundle.Unload(true);
            assetBundle = null;
        }
    }
    /// <summary>
    /// AssetBundle 加载器（异步）
    /// </summary>
    public class BundleAsyncRequest : BundleRequest
    {
        private AssetBundleCreateRequest _request;
        public override bool isDone
        {
            get
            {
                if (loadState == LoadState.Init) return false;
                if (loadState == LoadState.Loaded) return true;
                if (loadState == LoadState.LoadAssetBundle && _request.isDone)
                {
                    asset = _request.assetBundle;
                    if (_request.assetBundle == null)
                    {
                        error = string.Format("unable to load assetBundle:{0}", url);
                    }
                    loadState = LoadState.Loaded;
                }
                return _request == null || _request.isDone;
            }
        }

        public override float progress
        {
            get { return _request != null ? _request.progress : 0f; }
        }

        internal override void Load()
        {
            _request = AssetBundle.LoadFromFileAsync(url);
            if (_request == null)
            {
                error = url + " LoadFromFile failed.";
                return;
            }
            loadState = LoadState.LoadAssetBundle;
        }

        internal override void Unload()
        {
            _request = null;
            loadState = LoadState.Unload;
            base.Unload();
        }
    }
    /// <summary>
    /// 从远端下载AssetBundle（异步）
    /// </summary>
    public class WebBundleRequest : BundleRequest
    {
        private UnityWebRequest _request;
        public override string error
        {
            get { return _request != null ? _request.error : null; }
        }
        public override bool isDone
        {
            get
            {
                if (loadState == LoadState.Init) return false;
                if (_request == null || loadState == LoadState.Loaded) return true;
                if (_request.isDone)
                {
                    assetBundle = DownloadHandlerAssetBundle.GetContent(_request);
                    loadState = LoadState.Loaded;
                }
                return _request.isDone;
            }
        }
        public override float progress
        {
            get { return _request != null ? _request.downloadProgress : 0f; }
        }
        internal override void Load()
        {
            _request = UnityWebRequestAssetBundle.GetAssetBundle(url);
            _request.SendWebRequest();
            loadState = LoadState.LoadAssetBundle;
        }
        internal override void Unload()
        {
            if (_request != null)
            {
                _request.Dispose();
                _request = null;
            }
            loadState = LoadState.Unload;
            base.Unload();
        }
    }
}