using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class DownloadSequeue
    {
        private class LoadRequestInfo
        {
            public string path = string.Empty;
            public string multiVersionControlRelativePath = string.Empty;
            public System.Action<bool, string> callbackLoadEnd = null;
            public float prevProgress = 0;
        }

        private List<LoadRequestInfo> _loadRequests = new List<LoadRequestInfo>();
        private Dictionary<string, LoadRequestInfo> _addedRequests = new Dictionary<string, LoadRequestInfo>();

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

        public void SetDownLoadCountInFrame(int loadCount)
        {
            _loadCountInFrame = loadCount;
        }

        public void AddRequest(string path, System.Action<bool, string> callbackLoadEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            if (_addedRequests.ContainsKey(path))
                return;

            var info = new LoadRequestInfo()
            {
                path = path,
                multiVersionControlRelativePath = multiVersionControlRelativePath,
                callbackLoadEnd = callbackLoadEnd
            };
            _addedRequests.Add(path, info);
            _loadRequests.Add(info);
        }

        public void Start(System.Action<float> callbackProgress)
        {
            if (HotUpdateHelper.GetDynamicResourceAddress().IsNullOrEmpty())
            {
                Log.Error("DownloadSequeue Start error: dynamic download address is empty, please call 'shaco.HotUpdateHelper.SetDynamicResourceAddress()' at first");
                if (null != callbackProgress)
                    callbackProgress(1.0f);
                return;
            }

            //自动添加资源依赖的文件下载请求
            AutoAddDepenciesRequest();

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

        private void AutoAddDepenciesRequest()
        {
            var resCacheTmp = shaco.GameHelper.resCache;
            for (int i = _loadRequests.Count - 1; i >= 0; --i)
            {
                var request = _loadRequests[i];
                request.prevProgress = 0;
                var key = resCacheTmp.GetFullAssetBundleKey(request.path, request.multiVersionControlRelativePath);
                var depencies = HotUpdateManifest.GetDependenciesRecursive(key, request.multiVersionControlRelativePath);
                if (!depencies.IsNullOrEmpty())
                {
                    for (int j = depencies.Length - 1; j >= 0; --j)
                    {
                        if (_addedRequests.ContainsKey(depencies[j]))
                            continue;

                        var newInfo = new LoadRequestInfo()
                        {
                            path = depencies[j],
                            prevProgress = request.prevProgress,
                            multiVersionControlRelativePath = request.multiVersionControlRelativePath,
                            callbackLoadEnd = request.callbackLoadEnd
                        };
                        _addedRequests.Add(depencies[j], newInfo);
                        _loadRequests.Add(newInfo);
                    }
                }
            }
        }

        private void _StartLoadResourcesOrLocalRequestInSequeue()
        {
            if (_loadedCount < 0 || _loadedCount > _loadRequests.Count - 1 || _isUserRequestStop)
            {
                _loadRequests.Clear();
                _addedRequests.Clear();
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

            //转换为完整的相对路径
            var fullRelativePath = shaco.GameHelper.resCache.GetFullAssetBundleKey(loadRequestInfo.path, loadRequestInfo.multiVersionControlRelativePath);
            shaco.HotUpdateHelper.ForeachDownloadAssetbundleByUrls(fullRelativePath, loadRequestInfo.multiVersionControlRelativePath, (error) =>
            {
                ++_loadedCount;
                bool downloadError = !string.IsNullOrEmpty(error);
                if (null != loadRequestInfo.callbackLoadEnd)
                {
                    try
                    {
                        loadRequestInfo.callbackLoadEnd(!downloadError, loadRequestInfo.path);
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("DownloadSequeue download exception: e=" + e);
                        return;
                    }
                }

                if (downloadError)
                {
                    shaco.Log.Error("DownloadSequeue download resource error: path=" + loadRequestInfo.path + _callstackInformation);
                }

                _StartLoadResourcesOrLocalRequestInSequeue();
            }, (percent) =>
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
            });
        }

        private string GetCallStackInfo()
        {
#if DEBUG_LOG
            var framsString = shaco.Base.FileHelper.GetCallStackFrames(1);
            return null != framsString ? "\n【【【DownloadSequeue stack information:\n" + framsString.ToContactString("\n") + "\n】】】\n" : null;
#else
            return null;
#endif
        }
    }
}