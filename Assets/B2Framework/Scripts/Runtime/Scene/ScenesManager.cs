using System;
using System.Collections;
using System.Collections.Generic;

namespace B2Framework
{
    public sealed class ScenesManager : MonoSingleton<ScenesManager>, IDisposable
    {
        private readonly Dictionary<string, SceneAssetRequest> _loading = new Dictionary<string, SceneAssetRequest>();
        private readonly Dictionary<string, SceneAssetRequest> _loaded = new Dictionary<string, SceneAssetRequest>();
        public float progress { get; private set; }
        public bool isDone { get; private set; }
        public Action<float> OnProgress { get; set; }
        public Action<string> OnCompleted { get; set; }
        public ScenesManager LoadSceneAsync(string sceneName, bool additive = true)
        {
            if (_loading.ContainsKey(sceneName)) return this;
            if (_loaded.ContainsKey(sceneName))
            {
                Progress(1.0f);
                Completed(sceneName);
                return this;
            }
            if (!additive) ReleaseLoaded();

            StartCoroutine(OnLoadSceneAsync(sceneName, additive));
            return this;
        }
        private IEnumerator OnLoadSceneAsync(string sceneName, bool additive)
        {
            progress = 0;
            isDone = false;
            // var loadSceneMode = additive ? UnityEngine.SceneManagement.LoadSceneMode.Additive : UnityEngine.SceneManagement.LoadSceneMode.Single;
            // var async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, loadSceneMode);
            var async = AssetsManger.LoadSceneAsync(sceneName, additive);
            _loading.Add(sceneName, async);
            while (!async.isDone)
            {
                if (async.progress >= 0.9f)
                {
                    Progress(1.0f);
                }
                else Progress(async.progress);

                yield return null;
            }
            _loaded.Add(sceneName, async);
            Completed(sceneName);
        }
        public void UnloadSceneAsync(string sceneName)
        {
            // UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(sceneName);
            SceneAssetRequest request;
            if (_loaded.TryGetValue(sceneName, out request))
            {
                _loaded.Remove(sceneName);
                request?.Release();
            }
        }
        private void ReleaseLoaded()
        {
            foreach (var req in _loaded.Values)
                req?.Release();
            _loaded.Clear();
        }
        public void Dispose()
        {
            StopAllCoroutines();
            foreach (var req in _loading.Values)
                req?.Release();
            _loading.Clear();

            ReleaseLoaded();
        }
        private void Progress(float progress)
        {
            this.progress = progress;
            if (OnProgress != null) OnProgress(progress);
        }
        private void Completed(string sceneName)
        {
            isDone = true;
            if (OnCompleted != null) OnCompleted(sceneName);
        }
    }
}