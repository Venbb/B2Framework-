using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace B2Framework
{
    public class SceneAssetRequest : AssetRequest
    {
        protected readonly LoadSceneMode loadSceneMode;
        protected readonly string sceneName;
        public string assetBundleName;
        protected BundleRequest bundleRequest;
        public SceneAssetRequest(string path, bool additive)
        {
            base.url = path;
            AssetsManger.GetAssetBundleName(path, out assetBundleName);
            sceneName = Path.GetFileNameWithoutExtension(base.url);
            loadSceneMode = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
        }
        public override float progress
        {
            get { return 1; }
        }
        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                bundleRequest = AssetsManger.LoadBundle(assetBundleName);
                if (bundleRequest != null) SceneManager.LoadScene(sceneName, loadSceneMode);
            }
            else
            {
                try
                {
                    SceneManager.LoadScene(sceneName, loadSceneMode);
                    loadState = LoadState.LoadAsset;
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    error = e.ToString();
                    loadState = LoadState.Loaded;
                }
            }
        }
        internal override void Unload()
        {
            if (bundleRequest != null) bundleRequest.Release();
            if (loadSceneMode == LoadSceneMode.Additive)
            {
                if (SceneManager.GetSceneByName(sceneName).isLoaded) SceneManager.UnloadSceneAsync(sceneName);
            }
            bundleRequest = null;
        }
    }
    public class SceneAssetAsyncRequest : SceneAssetRequest
    {
        private AsyncOperation _request;
        public SceneAssetAsyncRequest(string path, bool additive) : base(path, additive) { }
        public override float progress
        {
            get
            {
                if (bundleRequest == null) return _request == null ? 0 : _request.progress;
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
        public override bool isDone
        {
            get
            {
                switch (loadState)
                {
                    case LoadState.Loaded:
                        return true;
                    case LoadState.LoadAssetBundle:
                        {
                            if (bundleRequest == null || bundleRequest.error != null) return true;
                            for (int i = 0, max = bundleRequest.dependencies.Count; i < max; i++)
                            {
                                var item = bundleRequest.dependencies[i];
                                if (item.error != null) return true;
                            }
                            if (!bundleRequest.isDone) return false;
                            for (int i = 0, max = bundleRequest.dependencies.Count; i < max; i++)
                            {
                                var item = bundleRequest.dependencies[i];
                                if (!item.isDone) return false;
                            }
                            LoadSceneAsync();
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
                if (_request != null && !_request.isDone) return false;
                loadState = LoadState.Loaded;
                return true;
            }
        }
        private void LoadSceneAsync()
        {
            try
            {
                _request = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
                loadState = LoadState.LoadAsset;
            }
            catch (Exception e)
            {
                Log.Error(e);
                error = e.ToString();
                loadState = LoadState.Loaded;
            }
        }
        internal override void Load()
        {
            if (!string.IsNullOrEmpty(assetBundleName))
            {
                bundleRequest = AssetsManger.LoadBundleAsync(assetBundleName);
                loadState = LoadState.LoadAssetBundle;
            }
            else LoadSceneAsync();
        }
        internal override void Unload()
        {
            base.Unload();
            _request = null;
        }
    }
}
