using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateDownloader, IHotUpdateImportWWW
    {
        //超时时间
        public double timeoutSeconds
        {
            get { return _timeoutSeconds; }
            set { _timeoutSeconds = value; }
        }

        //超时后重试次数
        public short timeoutRetryTimes
        {
            get { return _timeoutRetryTimes; }
            set { _timeoutRetryTimes = value; }
        }

        //刷新下载速度的间隔时间
        public float updateDownloadSpeedTime
        {
            get { return _updateDownloadSpeedTime; }
            set { _updateDownloadSpeedTime = value; }
        }

        //同一时间下载队列中http请求数量
        public int downloadSqueueCount
        {
            get { return _downloadSqueueCount; }
            set { _downloadSqueueCount = value; }
        }

        //检查需要的下载的版本完毕回掉
        public shaco.Base.EventCallBack onCheckVersionEndCallBack { get { return _onCheckVersionEndCallBack; } }
        //下载完毕单个文件回调<key：下载到本地的绝对路径>
        public shaco.Base.EventCallBack<string> onDownloadedOnceCallBack { get { return _onDownloadedOnceCallBack; } }
        //更新中回调
        public shaco.Base.EventCallBack onUpdatingCallBack { get { return _onUpdateingCallBack; } }
        //更新完毕回调
        public shaco.Base.EventCallBack onUpdateEndCallBack { get { return _onUpdateEndCallBack; } }

        private double _timeoutSeconds = 10.0f;
        private short _timeoutRetryTimes = 5;
        private float _updateDownloadSpeedTime = 1.0f;
        private int _downloadSqueueCount = 3;
        private shaco.Base.EventCallBack _onCheckVersionEndCallBack = new shaco.Base.EventCallBack();
        private shaco.Base.EventCallBack<string> _onDownloadedOnceCallBack = new shaco.Base.EventCallBack<string>();
        private shaco.Base.EventCallBack _onUpdateingCallBack = new shaco.Base.EventCallBack();
        private shaco.Base.EventCallBack _onUpdateEndCallBack = new shaco.Base.EventCallBack();

        //当前文件下载数量
        private int _iCurrentDownloadCount = 0;
        //当前文件需要总共下载的数量
        private int _iTotalDownloadCount = 0;
        //刷新下载速度计时器
        private shaco.ActionBase _actionUpdateDownloadSpeed = null;
        //检查下载进度是否完成计时器
        private shaco.ActionBase _actionCheckDownload = null;
        //当前下载速度
        private long _iDownloadSpeed = 0;
        //上一次的下载速度
        private long _lPrevDownloadSpeed = 0;
        //当前版本配置信息
        private HotUpdateDefine.SerializeVersionControl _versionControlClient = new HotUpdateDefine.SerializeVersionControl();
        //当前需要下载更新的资源包
        private List<HotUpdateDefine.DownloadAssetBundle> _listUpdateAssetBundle = new List<HotUpdateDefine.DownloadAssetBundle>();
        //当前正在下载更新的资源包
        private List<HotUpdateDefine.DownloadAssetBundle> _listCurrentUpdate = new List<HotUpdateDefine.DownloadAssetBundle>();
        //当前需要重新写入的Main MD5
        private string _strNeedRewriteMainMD5 = string.Empty;
        //下载所有assetbundle完毕
        private bool _isLoadAllAssetbundleSuccess = false;
        //当前下载状态
        private HotUpdateDownloadStatus _updateStatus = new HotUpdateDownloadStatus();
        //当前已经下载数据大小
        private long _currentDownloadedDataSize = 0;
        //当前需要更新的总数据大小
        private long _currentNeedUpdateDataSize = 0;
        //下载路径或本地地址
        private string _url = string.Empty;
        //是否已经关闭并清理过资源
        new private bool _isClosed = false;
        //是否只下载资源版本管理配置，不下载资源内容
        private string[] _filterPrefixPaths = null;

        /// <summary>
        /// 检查资源更新
        /// <param name="urlVersion">服务器资源根目录下载地址(例如VersionControl@@Android文件夹所在地址)</param>
        /// <param name="packageVersion">安装包版本号，例如1.0.0，如果填写了该字段，则在无网情况下对本地资源做版本管理检测，如果本地资源版本低于服务器资源则要求联网更新</param>
        /// <param name="mainMD5">主md5，如果没有设定该值，则会从服务器更新md5</param>
        /// <param name="filterPrefixPaths">下载文件筛选文件列表，如果为空则会下载所有资源</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        public void CheckUpdate(string urlVersion, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            CheckUpdate(urlVersion, string.Empty, string.Empty, null, multiVersionControlRelativePath);
        }

        public void CheckUpdate(string urlVersion, string mainMD5, string[] filterPrefixPaths, string multiVersionControlRelativePath)
        {
            CheckUpdate(urlVersion, mainMD5, string.Empty, filterPrefixPaths, multiVersionControlRelativePath);
        }

        public void CheckUpdate(string urlVersion, string mainMD5, string packageVersion, string[] filterPrefixPaths, string multiVersionControlRelativePath)
        {
            try
            {
                _multiVersionControlRelativePath = multiVersionControlRelativePath;
                CheckUpdateBase(urlVersion, mainMD5, packageVersion, filterPrefixPaths);
            }
            catch (System.Exception e)
            {
                OnError("check update exception=" + e);
            }
        }

        //确认是否存在该状态信息
        public bool HasStatus(HotUpdateDownloadStatus.Status status)
        {
            return _updateStatus.HasStatus(status);
        }

        /// <summary>
        /// 是否需要更新资源
        /// </summary>
        /// <returns><c>true</c>, if need update was ised, <c>false</c> otherwise.</returns>
        public bool IsNeedUpdate()
        {
            return _listUpdateAssetBundle.Count > 0;
        }

        /// <summary>
        /// 重置所有回调
        /// </summary>
        public void ResetAllCallBack()
        {
            onCheckVersionEndCallBack.ClearCallBack();
            onUpdatingCallBack.ClearCallBack();
            onUpdateEndCallBack.ClearCallBack();
        }

        //是否完成下载，没有其他错误
        public override bool IsSuccess()
        {
            return HasStatus(HotUpdateDownloadStatus.Status.UpdateCompleted) && !HasError();
        }

        protected void OnSequeueDownloadCompletedAsync(HotUpdateDefine.DownloadAssetBundle downloadAssetBundle,
                                                        string urlResourcePrefix,
                                                        string saveFullPath)
        {
            lock (_listCurrentUpdate)
            {
                //如果下载器已经关闭了，则不用再进行下载
                if (_isClosed)
                {
                    Log.Warning("HotUpdateImportWWW OnSequeueDownloadCompletedAsync warning: download already closed");
                    return;
                }

                OnError(downloadAssetBundle.HttpDel.GetLastError());
                CheckRemoveUnuseUpdateInfo(downloadAssetBundle);

                ++_iCurrentDownloadCount;

                CheckUpdateAssetBundleBySequeue(1, urlResourcePrefix);
                onDownloadedOnceCallBack.InvokeAllCallBack(saveFullPath);
            }
        }

        protected void CheckUpdateBase(string urlVersion, string mainMD5, string packageVersion, string[] filterPrefixPaths)
        {
            //Unity自带的这个网络有效性检查速度太慢，切换网络后很久状态不会更新
            // if (Application.internetReachability == NetworkReachability.NotReachable)
            // {
            //     OnError("CheckUpdateByWWW warning: no network !");
            //     OnCompleted();
            //     return;
            // }

            if (string.IsNullOrEmpty(urlVersion))
            {
                OnError("CheckUpdateByWWW error: params is valid !");
                OnCompleted();
                return;
            }

            if (_isWorking)
            {
                Log.Warning("HotUpdateImporWWW warning: is busy now, please wait");
                return;
            }

            Log.Info("check update start1");

            try
            {
                urlVersion = shaco.Base.FileHelper.RemoveSubStringByFind(urlVersion, shaco.Base.GlobalParams.SIGN_FLAG);
                urlVersion = urlVersion.ContactPath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath));

                OnStartWorking();

                _url = urlVersion;
                this._filterPrefixPaths = filterPrefixPaths;
                CheckVersionBase(_url, mainMD5, packageVersion);
            }
            catch (System.Exception e)
            {
                Log.Error(e.ToString());
            }

            Log.Info("check update start2");
        }

        protected void StopAllAction()
        {
            if (_actionCheckTimeout != null)
            {
                _actionCheckTimeout.StopMe();
                _actionCheckTimeout = null;
            }

            if (_actionUpdateDownloadSpeed != null)
            {
                _actionUpdateDownloadSpeed.StopMe();
                _actionUpdateDownloadSpeed = null;
            }
        }

        protected bool DeleteAssetBundles(List<HotUpdateDefine.ExportAssetBundle> exportAssetBundles, string multiVersionControlRelativePath)
        {
            if (exportAssetBundles.IsNullOrEmpty())
                return true;
                
            bool deleteSuccess = true;
            var pathRootTmp = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath);
            pathRootTmp = shaco.Base.FileHelper.GetFullpath(pathRootTmp);
            for (int i = 0; i < exportAssetBundles.Count; ++i)
            {
                var fullPath = exportAssetBundles[i].fullPathRuntime;
                if (shaco.Base.FileHelper.ExistsFile(fullPath))
                {
                    if (!shaco.Base.FileHelper.DeleteByUserPath(fullPath))
                    {
                        deleteSuccess = false;
                    }
                }
            }
            return deleteSuccess;
        }

        public override void Close(bool unloadAllLoadedObjects = false)
        {
            lock (_listCurrentUpdate)
            {
                base.Close(unloadAllLoadedObjects);
                if (!_isClosed)
                {
                    CleanCache();
                    StopAllAction();
                    _isClosed = true;

                    //如果发生错误，最后检查一次是否因为无网络导致
                    if (HasError() && Application.internetReachability == NetworkReachability.NotReachable)
                    {
                        _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.ErrorNeedUpdateResourceWithNetWork);
                    }

                    //回收内存，重置回调方法
                    shaco.GameHelper.http.UnloadUnusedMemory();

                    if (!_filterPrefixPaths.IsNullOrEmpty())
                    {
                        if (shaco.GameHelper.objectpool.IsInPool(typeof(HotUpdateDefine.DownloadAssetBundle).ToTypeString()))
                            shaco.GameHelper.objectpool.DestroyAllObjects(typeof(HotUpdateDefine.DownloadAssetBundle).ToTypeString());
                        if (shaco.GameHelper.objectpool.IsInPool(typeof(HotUpdateDownloader).ToTypeString()))
                            shaco.GameHelper.objectpool.DestroyAllObjects(typeof(HotUpdateDownloader).ToTypeString());
                    }

                    System.GC.Collect();

                    //下载完毕最后一次回调
                    var updatingCallBackTmp = onUpdatingCallBack.Clone();
                    var updateEndCallBackTmp = onUpdateEndCallBack.Clone();
                    ResetAllCallBack();
                    updatingCallBackTmp.InvokeAllCallBack();
                    updateEndCallBackTmp.InvokeAllCallBack();
                }
                else
                {
                    shaco.Log.Warning("HotUpdateImportWWW Close warning: dont need close again");
                }
            }
        }

        protected override void ResetParam()
        {
            base.ResetParam();

            _iCurrentDownloadCount = 0;
            _iTotalDownloadCount = 0;
            _currentDownloadedDataSize = 0;
            _currentNeedUpdateDataSize = 0;
            _httpHelper.timeoutSeconds = timeoutSeconds;
            _httpHelper.timeoutRetryTimes = timeoutRetryTimes;
            _httpHelper.CloseClient();
            _iDownloadSpeed = 0;
            _lPrevDownloadSpeed = 0;
            _strNeedRewriteMainMD5 = string.Empty;
            _isLoadAllAssetbundleSuccess = false;
            _updateStatus.ResetDownloadStatus();

            lock (_listCurrentUpdate)
            {
                _listCurrentUpdate.Clear();
                _listUpdateAssetBundle.Clear();
            }

            StopAllAction();
        }

        protected void CleanCache()
        {
            if (_httpHelper != null)
            {
                _httpHelper.CloseClient();
            }

            for (int i = 0; i < _listCurrentUpdate.Count; ++i)
            {
                if (null != _listCurrentUpdate[i].HttpDel)
                {
                    _listCurrentUpdate[i].HttpDel.CloseClient();
                }
                _listCurrentUpdate[i].HotUpdateDel.Close();
            }
            _listCurrentUpdate.Clear();
        }

        protected void OnStartWorking()
        {
            CleanCache();
            ResetParam();
            _isWorking = true;
            _isClosed = false;
        }

        protected override void OnError(string error)
        {
            base.OnError(error);

            if (HasError())
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.HasError);
            }
        }

        protected override void CheckCompleted(string error, bool isInvokeEndCallFunc)
        {
            if (_isClosed)
                return;

            if (isInvokeEndCallFunc)
            {
                StopAllAction();
                if (_actionCheckDownload != null)
                {
                    _actionCheckDownload.StopMe();
                    _actionCheckDownload = null;
                }

                if (!HasError() && string.IsNullOrEmpty(error) && !string.IsNullOrEmpty(_strNeedRewriteMainMD5))
                {
                    Log.Info("write main md5 files status=" + GetStatusDescription());
                    var fullPathTmp = shaco.Base.FileHelper.GetFullpath(HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(_multiVersionControlRelativePath));
                    shaco.Base.FileHelper.WriteAllByUserPath(fullPathTmp, _strNeedRewriteMainMD5);
                    _strNeedRewriteMainMD5 = string.Empty;
                }

                if (isInvokeEndCallFunc)
                {
                    if (HasStatus(HotUpdateDownloadStatus.Status.UpdateCompleted))
                    {
                        Log.Error("HotUpdate " + "onCompleted warning: It's a callback method that has been executed at one time");
                    }

                    _updateStatus.SetStatus(HasError() ? HotUpdateDownloadStatus.Status.HasError : HotUpdateDownloadStatus.Status.UpdateCompleted);
                    _isWorking = false;

                    if (IsSuccess())
                        _fCurrentProgress = 1.0f;

                    if (HasError())
                        Log.Error("HotUpdate " + error);
                }
            }

            if (isInvokeEndCallFunc)
                this.Close();
            base.CheckCompleted(error, isInvokeEndCallFunc);
        }

        /// <summary>
        /// 检查获取本地配置信息，并记录下来
        /// </summary>
        /// <returns> 返回本地主配置文件地址
        protected string CheckGetVersionControl()
        {
            var retValue = string.Empty;
            var strVersionControlSavePath = shaco.Base.FileHelper.GetFullpath(string.Empty);
            strVersionControlSavePath = HotUpdateHelper.GetVersionControlFilePath(strVersionControlSavePath, _multiVersionControlRelativePath);
            retValue = strVersionControlSavePath;
            
            if (null != _versionControlClient && _versionControlClient.ListAssetBundles.Count > 0)
            {
                return retValue;
            }

            if (!shaco.Base.FileHelper.ExistsFile(strVersionControlSavePath))
                _versionControlClient = new HotUpdateDefine.SerializeVersionControl();
            else
                _versionControlClient = HotUpdateHelper.PathToVersionControl(strVersionControlSavePath, false);

            return retValue;
        }
    }
}
