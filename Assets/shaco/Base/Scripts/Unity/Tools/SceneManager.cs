using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco
{
    public class SceneManager : MonoBehaviour
    {
        public delegate void SceneLoadEndCallFunc(string sceneName);

        public delegate void SceneLoadingCallFunc(float progress);

        public SceneLoadEndCallFunc callFuncLoadEnd = null;
        public SceneLoadingCallFunc callFunLoading = null;
        private AsyncOperation _asyncInfo = null;
        private string _currentLoadingSceneName = null;

        private string _currentSceneName = string.Empty;

        void Start()
        {
            shaco.GameEntry.GetComponentInstance<SceneManager>();

#if UNITY_5_3_OR_NEWER
            _currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
#else 
            _currentSceneName = Application.loadedLevelName;
#endif
        }

        void OnDestroy()
        {
            _asyncInfo = null;
            // shaco.GameHelper.resCache.UnloadUnusedDatas();
        }

        static public bool IsLoading()
        {
            return shaco.GameEntry.GetComponentInstance<SceneManager>()._asyncInfo != null;
        }

        static public void LoadScene(string sceneName)
        {
            shaco.Log.Info("SceneManager LoadScene: scene name=" + sceneName);
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            if (manager._asyncInfo != null)
            {
                shaco.Log.Error("SceneManager LoadScene erorr: other scene is loading, name=" + manager._currentLoadingSceneName);
                return;
            }

#if UNITY_5_3_OR_NEWER
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
#else
            Application.LoadLevel(sceneName);
#endif
        }

//         static public void LoadScene(int index)
//         {
//             shaco.Log.Info("SceneManager LoadScene: index=" + index);
//             var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
//             if (manager._asyncInfo != null)
//                 return;

// #if UNITY_5_3_OR_NEWER
//             UnityEngine.SceneManagement.SceneManager.LoadScene(index);
// #else
//             Application.LoadLevel(index);
// #endif
//         }

        static public void LoadSceneAsync(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            shaco.Log.Info("SceneManager LoadSceneAsync: scene name=" + sceneName + " mode=" + mode);
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            if (manager._asyncInfo != null)
                return;

            manager.StartCoroutine(manager.LoadSceneAsyncBase(sceneName, mode));
        }

        static public string GetCurrentSceneName()
        {
            return shaco.GameEntry.GetComponentInstance<SceneManager>()._currentSceneName;
        }

        private IEnumerator LoadSceneAsyncBase(string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            var manager = shaco.GameEntry.GetComponentInstance<SceneManager>();
            manager._currentSceneName = sceneName;
            manager._currentLoadingSceneName = sceneName;
#if UNITY_5_3_OR_NEWER
            _asyncInfo = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
#else
            _asyncInfo = Application.LoadLevelAsync(sceneName);
#endif

            while (!_asyncInfo.isDone)
            {
                if (_asyncInfo.progress < 1.0f)
                {
                    if (callFunLoading != null)
                    {
                        callFunLoading(_asyncInfo.progress);
                    }
                }
                yield return 1;
            }

            OnLoadSceneEnd(sceneName);
            shaco.Log.Info("LoadSceneAsync completed !! scene=" + sceneName);
        }

        private void OnLoadSceneEnd(string sceneName)
        {
            if (_asyncInfo != null)
            {
                if (callFunLoading != null)
                    callFunLoading(1.0f);
                _asyncInfo = null;
                _currentLoadingSceneName = null;
            }

            if (callFuncLoadEnd != null)
            {
                callFuncLoadEnd(sceneName);
                callFuncLoadEnd = null;
            }
        }
    }
}
