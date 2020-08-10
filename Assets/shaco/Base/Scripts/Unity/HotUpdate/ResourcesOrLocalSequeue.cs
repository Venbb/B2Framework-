using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class ResourcesOrLocalSequeue
    {
        private class LoadRequestInfo
        {
            public string path = string.Empty;
            public string multiVersionControlRelativePath = string.Empty;
            public bool autoCheckDepencies = true;
            public System.Type type;
            public System.Action<UnityEngine.Object[]> callbackLoadAllEnd = null;
            public System.Action<UnityEngine.Object> callbackLoadEnd = null;
            public System.Action<bool> callbackLoadAssetbundleEnd = null;
            public float prevProgress = 0;
        }

        private System.Collections.Generic.List<LoadRequestInfo> _loadRequests = new System.Collections.Generic.List<LoadRequestInfo>();

        private string _callstackInformation = null;

        //在同一帧加载的资源数量
        private int _loadCountInFrame = 1;

        //已经加载完毕的数量
        private int _loadedCount = 0;

        //当前加载下标 
        private int _currentLoadIndex = 0;

        //已经加载完毕的总百分比
        private double _loadedProgress = 0;

        //当前资源加载进度
        private float _currentProgress = 0;

        //主动停止标记
        private bool _isUserRequestStop = false;

        public void SetLoadCountInFrame(int loadCount)
        {
            _loadCountInFrame = loadCount;
        }

        public void AddRequest<T>(string path, System.Action<UnityEngine.Object> callbackLoadEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            AddRequest(path, callbackLoadEnd, typeof(T), multiVersionControlRelativePath, autoCheckDepencies);
        }

        public void AddRequest(string path, System.Action<UnityEngine.Object> callbackLoadEnd, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            _loadRequests.Add(new LoadRequestInfo()
            {
                path = path,
                type = type,
                multiVersionControlRelativePath = multiVersionControlRelativePath,
                autoCheckDepencies = autoCheckDepencies,
                callbackLoadEnd = callbackLoadEnd,
                callbackLoadAllEnd = null,
            });
        }

        public void AddRequestAll<T>(string path, System.Action<UnityEngine.Object[]> callbackLoadAllEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            AddRequestAll(path, callbackLoadAllEnd, typeof(T), multiVersionControlRelativePath, autoCheckDepencies);
        }

        public void AddRequestAll(string path, System.Action<UnityEngine.Object[]> callbackLoadAllEnd, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            _loadRequests.Add(new LoadRequestInfo()
            {
                path = path,
                type = type,
                multiVersionControlRelativePath = multiVersionControlRelativePath,
                autoCheckDepencies = autoCheckDepencies,
                callbackLoadEnd = null,
                callbackLoadAllEnd = callbackLoadAllEnd,
            });
        }

        public void AddRequestAssetBundle(string path, System.Action<bool> callbackLoadEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            _loadRequests.Add(new LoadRequestInfo()
            {
                path = path,
                type = typeof(UnityEngine.Object),
                multiVersionControlRelativePath = multiVersionControlRelativePath,
                autoCheckDepencies = autoCheckDepencies,
                callbackLoadEnd = null,
                callbackLoadAllEnd = null,
                callbackLoadAssetbundleEnd = callbackLoadEnd,
            });
        }

        public void Start(System.Action<float> callbackProgress)
        {
            _isUserRequestStop = false;
            _currentLoadIndex = 0;
            _loadedCount = 0;
            _currentProgress = 0;
            _loadedProgress = 0;
            _callstackInformation = GetCallStackInfo();
            if (_loadRequests.IsNullOrEmpty())
            {
                Log.Warning("ResourcesEx+Public StartLoadResourcesOrLocalRequestInSequeue error: load request sequeue is empty ! please call 'LoadRequestInfo.AddRequest' at first");

                if (!_isUserRequestStop && null != callbackProgress)
                    callbackProgress(1.0f);
                return;
            }

            shaco.Base.WaitFor.Run(()=>
            {
                if (null != callbackProgress && _currentProgress < 1.0f)
                    callbackProgress(_currentProgress);
                return _isUserRequestStop || _currentProgress >= 1.0f;
            }, () =>
            {
                if (!_isUserRequestStop && null != callbackProgress)
                    callbackProgress(1.0f);
            });

            _loadCountInFrame = System.Math.Min(_loadCountInFrame, _loadRequests.Count);
            for (int i = _loadCountInFrame - 1; i >= 0; --i)
                _StartLoadResourcesOrLocalRequestInSequeue();
        }

        public void Stop()
        {
            _isUserRequestStop = true;
        }

        private void _StartLoadResourcesOrLocalRequestInSequeue()
        {
            if (_loadedCount < 0 || _loadedCount > _loadRequests.Count - 1 || _isUserRequestStop)
            {
                _loadRequests.Clear();
                _currentProgress = 1.0f;
                return;
            }

            //所有请求发送完毕，可能现在其他资源还没加载完，继续等待
            if (_currentLoadIndex < 0 || _currentLoadIndex > _loadRequests.Count - 1)
            {
                return;
            }

            var loadRequestInfo = _loadRequests[_currentLoadIndex];
            ++_currentLoadIndex;

        
            if (null != loadRequestInfo.callbackLoadAssetbundleEnd)
            {
                LoadAssetbundle(loadRequestInfo);
            }
            else if (null != loadRequestInfo.callbackLoadEnd)
            {
                LoadOnce(loadRequestInfo);
            }
            else if (null != loadRequestInfo.callbackLoadAllEnd)
            {
                LoadAll(loadRequestInfo);
            }
            else
            {
                shaco.Log.Error("ResourcesOrLocalSequeue _StartLoadResourcesOrLocalRequestInSequeue error: path=" + loadRequestInfo.path + " muilty=" + loadRequestInfo.multiVersionControlRelativePath);
            }
        }

        private void LoadAssetbundle(LoadRequestInfo loadRequestInfo)
        {
            shaco.GameHelper.resCache.LoadAssetBundleAsync(loadRequestInfo.path, (bool success) =>
            {
                ++_loadedCount;
                if (null != loadRequestInfo.callbackLoadEnd)
                {
                    try
                    {
                        loadRequestInfo.callbackLoadAssetbundleEnd(success);
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("ResourcesOrLocalSequeue LoadAssetbundle exception: e=" + e);
                        return;
                    }
                }

                if (!success)
                {
                    shaco.Log.Error("ResourcesOrLocalSequeue LoadAssetbundle resource error: path=" + loadRequestInfo.path + _callstackInformation);
                }

                _StartLoadResourcesOrLocalRequestInSequeue();

            }, (float percent) =>
            {
                if (_isUserRequestStop)
                {
                    _StartLoadResourcesOrLocalRequestInSequeue();
                    return;
                }

                var offsetPercent = percent - loadRequestInfo.prevProgress;
                loadRequestInfo.prevProgress = percent;

                _loadedProgress += offsetPercent;
                _currentProgress = (float)(_loadedProgress / (_loadRequests.Count));
                if (_currentProgress >= 1.0f)
                {
                    _currentProgress = 0.99f;
                }
            }, loadRequestInfo.multiVersionControlRelativePath, loadRequestInfo.autoCheckDepencies);
        }

        private void LoadOnce(LoadRequestInfo loadRequestInfo)
        {
            shaco.GameHelper.res.LoadResourcesOrLocalAsync(loadRequestInfo.path, loadRequestInfo.type, (UnityEngine.Object obj) =>
            {
                ++_loadedCount;
                if (null != loadRequestInfo.callbackLoadEnd)
                {
                    try
                    {
                        loadRequestInfo.callbackLoadEnd(obj);
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("ResourcesOrLocalSequeue LoadOnce exception: e=" + e);
                        return;
                    }
                }

                if (obj == null)
                {
                    shaco.Log.Error("ResourcesOrLocalSequeue LoadOnce resource error: path=" + loadRequestInfo.path + _callstackInformation);
                }

                _StartLoadResourcesOrLocalRequestInSequeue();

            }, (float percent) =>
            {
                if (_isUserRequestStop)
                {
                    _StartLoadResourcesOrLocalRequestInSequeue();
                    return;
                }

                var offsetPercent = percent - loadRequestInfo.prevProgress;
                loadRequestInfo.prevProgress = percent;

                _loadedProgress += offsetPercent;
                _currentProgress = (float)(_loadedProgress / (_loadRequests.Count));
                if (_currentProgress >= 1.0f)
                {
                    _currentProgress = 0.99f;
                }
            }, loadRequestInfo.multiVersionControlRelativePath, loadRequestInfo.autoCheckDepencies);
        }

        private void LoadAll(LoadRequestInfo loadRequestInfo)
        {
            shaco.GameHelper.res.LoadResourcesOrLocalAsyncAll(loadRequestInfo.path, loadRequestInfo.type, (UnityEngine.Object[] objs) =>
            {
                ++_loadedCount;
                if (null != loadRequestInfo.callbackLoadAllEnd)
                {
                    try
                    {
                        loadRequestInfo.callbackLoadAllEnd(objs);
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("ResourcesOrLocalSequeue LoadAll exception: e=" + e);
                        return;
                    }
                }

                if (objs.IsNullOrEmpty())
                {
                    shaco.Log.Error("ResourcesOrLocalSequeue LoadAll resource error: path=" + loadRequestInfo.path + _callstackInformation);
                }

                _StartLoadResourcesOrLocalRequestInSequeue();

            }, (float percent) =>
            {
                if (_isUserRequestStop)
                {
                    _StartLoadResourcesOrLocalRequestInSequeue();
                    return;
                }

                var offsetPercent = percent - loadRequestInfo.prevProgress;
                loadRequestInfo.prevProgress = percent;

                _loadedProgress += offsetPercent;
                _currentProgress = (float)(_loadedProgress / (_loadRequests.Count));
                if (_currentProgress >= 1.0f)
                {
                    _currentProgress = 0.99f;
                }
            }, loadRequestInfo.multiVersionControlRelativePath, loadRequestInfo.autoCheckDepencies);
        }

        private string GetCallStackInfo()
        {
#if DEBUG_LOG
            var framsString = shaco.Base.FileHelper.GetCallStackFrames(1);
            return null != framsString ? "\n【【【ResourcesOrLocalSequeue stack information:\n" + framsString.ToContactString("\n") + "\n】】】\n" : null;
#else
            return null;
#endif
        }
    }
}