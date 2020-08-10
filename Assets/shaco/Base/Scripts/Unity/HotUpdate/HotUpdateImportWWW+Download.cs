using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateDownloader, IHotUpdateImportWWW
    {
        //检查是否有未使用的需要被清理的下载对象
        protected void CheckRemoveUnuseUpdateInfo(HotUpdateDefine.DownloadAssetBundle downloadAssetBundle)
        {
            downloadAssetBundle.HotUpdateDel.Close();
            shaco.GameHelper.objectpool.RecyclingObject(downloadAssetBundle.HotUpdateDel);

#if DEBUG_LOG
            if (!_listCurrentUpdate.Contains(downloadAssetBundle))
            {
                shaco.Log.Error("HotUpdate CheckRemoveUnuseUpdateInfo error: not found download info, url=" + downloadAssetBundle.url);
            }
#endif
            _listCurrentUpdate.Remove(downloadAssetBundle);
        }

        /// <summary>
        /// 更新manifest(资源关系依赖)文件
        /// <param name="urlVersion">资源路径根目录</param>
        /// <param name="paths">manifest下载路径</param>
        /// </summary>
        protected void UpdateAssetBundleManifest(string urlVersion, params string[] paths)
        {
            var urls = new List<string>();
            var urlMainMD5Root = urlVersion.ContactPath(_strNeedRewriteMainMD5);
            for (int i = 0; i < paths.Length; ++i)
            {
                var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(paths[i]);
                var urlTmp = shaco.Base.FileHelper.ContactPath(urlMainMD5Root, fileNameTmp);
                urls.Add(urlTmp);
            }

            //download manifest from server
            var listUpdateTmp = new List<HotUpdateDownloader>();
            var downloadCount = 0;

            List<HotUpdateDefine.DownloadAssetBundle> listDownloadTmp = new List<HotUpdateDefine.DownloadAssetBundle>();

            for (int i = 0; i < urls.Count; ++i)
            {
                var urlTmp = urls[i];
                var pathTmp = paths[i];

                var downloaderTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDownloader());
                listUpdateTmp.Add(downloaderTmp);

                var assetBundleTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDefine.DownloadAssetBundle());
                var httpHelperTmp = downloaderTmp.GetHttpHelper();
                assetBundleTmp.HotUpdateDel = downloaderTmp;
                assetBundleTmp.HttpDel = httpHelperTmp;
                listDownloadTmp.Add(assetBundleTmp);

                urlTmp = shaco.Base.Utility.GetHttpRequestFullUrl(urlTmp, new shaco.Base.HttpComponent("main_md5", _strNeedRewriteMainMD5));

                downloaderTmp.DownloadByHttp(urlTmp);
                float prevProgress = 0;
                downloaderTmp.onDownloadingCallBack.AddCallBack(this, (object defaultSender) =>
                {
                    //计算下载百分比
                    var currentProgress = httpHelperTmp.GetDownloadProgress();
                    var offsetProgress = currentProgress - prevProgress;
                    if (offsetProgress > 0)
                    {
                        _fCurrentProgress += offsetProgress / (float)HotUpdateDefine.ALL_VERSION_CONTROL_FILE_COUNT * HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO;
                    }
                    prevProgress = currentProgress;

                    onUpdatingCallBack.InvokeAllCallBack();
                });

                downloaderTmp.onDownloadEndCalback.AddCallBack(this, (object defaultSender) =>
                {
                    if (httpHelperTmp.IsSuccess())
                    {
                        var fullpathTmp = shaco.Base.FileHelper.GetFullpath(pathTmp);
                        shaco.Base.FileHelper.WriteAllByteByUserPath(fullpathTmp, httpHelperTmp.GetDownloadByte());
                        ++downloadCount;

                        if (downloadCount >= urls.Count)
                        {
                            shaco.GameHelper.objectpool.RecyclingAllObjects(typeof(HotUpdateDownloader).ToTypeString());
                            shaco.GameHelper.objectpool.RecyclingAllObjects(typeof(HotUpdateDefine.DownloadAssetBundle).ToTypeString());
                            CheckManifestBaseEnd(urlVersion);
                        }
                    }
                    else
                    {
                        if (httpHelperTmp.HasError())
                        {
                            for (int j = 0; j < listUpdateTmp.Count; ++j)
                                listUpdateTmp[j].Close();
                            OnError("check manifest error: msg=" + downloaderTmp.GetLastError());
                            OnCompleted();
                        }
                        else
                        {
                            OnError("unkown error, but download manifest failed");
                            OnCompleted();
                        }
                    }
                });
            }

            ComputerDownloadSpeedAll();
        }

        /// <summary>
        /// 按队列依次下载资源
        /// <param name="squeueCount">队列中同时下载的文件数量</param>
        /// <param name="urlVersion">资源路径根目录</param>
        /// </summary>
        protected void CheckUpdateAssetBundleBySequeue(int squeueCount, string urlVersion)
        {
            //已经关闭的情况下不能再进行下载了
            if (_isClosed)
            {
                return;
            }
                
            if (squeueCount < 0 || _iCurrentDownloadCount >= _iTotalDownloadCount || HasError())
            {
                Log.Info("HotUpdate download all assetbundle completed ! " + (HasError() ? "error message=" + GetLastError() : string.Empty)
                 + "\ndownload count=" + _iCurrentDownloadCount + " all count=" + _iTotalDownloadCount, Color.white);

                if (HasError())
                    OnError(GetLastError());
                else
                {
                    _isLoadAllAssetbundleSuccess = true;
                }
                return;
            }
            
            squeueCount = squeueCount < _listUpdateAssetBundle.Count ? squeueCount : _listUpdateAssetBundle.Count;
            var versionControlFullPath = shaco.Base.FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath));

            for (int i = squeueCount - 1; i >= 0; --i)
            {
                lock (_listCurrentUpdate)
                {
                    var downloadAssetBundle = _listUpdateAssetBundle[i];

                    _listUpdateAssetBundle.RemoveAt(i);

                    if (downloadAssetBundle.HttpDel != null)
                    {
                        Log.Error("HotUpdate create http error: this http connect has be created !");
                        continue;
                    }

                    CheckUpdateAssetBundleBySequeueOnce(downloadAssetBundle, urlVersion, versionControlFullPath);
                }
            }
        }

        /// <summary>
        /// 队列模式下载一次ab包
        /// <param name="downloadAssetBundle">下载信息</param>
        /// <param name="urlVersion">url前缀</param>
        /// <param name="versionControlFullPath">下载写入文件根目录</param>
        /// <return></return>
        /// </summary>
        private void CheckUpdateAssetBundleBySequeueOnce(HotUpdateDefine.DownloadAssetBundle downloadAssetBundle, string urlVersion, string versionControlFullPath)
        {
            HotUpdateDefine.ExportAssetBundle exportBundleParam = downloadAssetBundle.ExportInfo;

            //for safe url
            var assetbundleNameConvert = HotUpdateHelper.AddAssetBundleNameTag(exportBundleParam.AssetBundleName, exportBundleParam.AssetBundleMD5);
            var fullUrl = shaco.Base.FileHelper.ContactPath(urlVersion, assetbundleNameConvert);

            //start downloading assetbundle
            var updateTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDownloader());

            //start downloading
            fullUrl = HotUpdateHelper.AssetBundleKeyToPath(fullUrl);

            //set auto save path
            var fileNameTmp = HotUpdateHelper.AssetBundleKeyToPath(downloadAssetBundle.ExportInfo.AssetBundleName);
            var pathTmp = shaco.Base.FileHelper.ContactPath(versionControlFullPath, fileNameTmp);
            updateTmp.GetHttpHelper().SetAutoSaveWhenCompleted(pathTmp);
            
            updateTmp.DownloadByHttp(fullUrl);

            long prevDownloadFileSize = 0;
            updateTmp.onDownloadingCallBack.AddCallBack(this, (object defaultSender) =>
            {
                var updateTarget = updateTmp;

                if (_isClosed)
                {
                    shaco.Log.Info("HotUpdate download: user cancel...");
                    return;
                }

                if (null == updateTarget)
                {
                    shaco.Log.Error("HotUpdate check download progress error: updateTarget is null");
                    return;
                }

                if (null == downloadAssetBundle)
                {
                    shaco.Log.Error("HotUpdate check download progress error: downloadAssetBundle is null");
                    return;
                }
                else if (null == downloadAssetBundle.HttpDel)
                {
                    shaco.Log.Error("HotUpdate check download progress error: downloadAssetBundle.HttpDel is null");
                    return;
                }

                //计算下载文件大小
                var currentDownloadSize = downloadAssetBundle.HttpDel.GetCurrentDownloadSize();
                _currentDownloadedDataSize += (currentDownloadSize - prevDownloadFileSize);
                prevDownloadFileSize = currentDownloadSize;

                //计算百分比
                var currentPercent = (double)_currentDownloadedDataSize / (double)_currentNeedUpdateDataSize;
                _fCurrentProgress = (float)currentPercent * (1 - HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO) + HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO;

                if (updateTarget.HasError())
                {
                    OnError(updateTarget.GetLastError());
                }
            });

            downloadAssetBundle.HotUpdateDel = updateTmp;
            downloadAssetBundle.HttpDel = updateTmp.GetHttpHelper();
            downloadAssetBundle.url = fullUrl;

            //downloaded compeleted callback
            _listCurrentUpdate.Add(downloadAssetBundle);
            updateTmp.onDownloadEndCalback.AddCallBack(this, sender =>
            {
                OnSequeueDownloadCompletedAsync(downloadAssetBundle, urlVersion, pathTmp);
            });
        }

        /// <summary>
        /// 开启计时器在后台持续计算下载速度
        /// </summary>
        protected void ComputerDownloadSpeedAll()
        {
            if (_actionUpdateDownloadSpeed != null)
            {
                _actionUpdateDownloadSpeed.StopMe();
                _actionUpdateDownloadSpeed = null;
            }

            var actionTmp = shaco.DelayTime.Create(updateDownloadSpeedTime);
            actionTmp.onCompleteFunc = (shaco.ActionBase ac) =>
            {
                _iDownloadSpeed = UpdateDownloadSpeed();
            };
            _actionUpdateDownloadSpeed = shaco.Repeat.CreateRepeatForever(actionTmp);
            _actionUpdateDownloadSpeed.RunAction(shaco.GameHelper.action.GetGlobalInvokeTarget().gameObject);
        }

        /// <summary>
        /// 刷新当前下载速度
        /// </summary>
        protected long UpdateDownloadSpeed()
        {
            long retValue = 0;
            long allDownloadCount = 0;
            long allDownloadSpeed = 0;

            lock (_listCurrentUpdate)
            {
                for (int i = 0; i < _listCurrentUpdate.Count; ++i)
                {
                    var httpTarget = _listCurrentUpdate[i].HttpDel;

                    if (httpTarget != null)
                    {
                        long downloadSpeedTmp = httpTarget.GetDownloadSpeed();
                        allDownloadSpeed += downloadSpeedTmp;
                        ++allDownloadCount;

                        httpTarget.ResetDownloadSpeed();
                    }
                }
            }

            if (allDownloadSpeed <= 0)
            {
                retValue = _lPrevDownloadSpeed > 0 ? _lPrevDownloadSpeed : 0;
            }
            else
            {
                retValue = allDownloadSpeed;
                _lPrevDownloadSpeed = retValue;
            }
            return retValue;
        }

        /// <summary>
        /// 检查版本更新开始
        /// </summary>
        protected void CheckVersionBase(string urlVersion, string mainMD5, string packageVersion)
        {
            CheckMainMD5File(urlVersion, mainMD5, packageVersion);
        }

        /// <summary>
        /// 检查manifest文件结束
        /// </summary>
        protected void CheckManifestBaseEnd(string urlVersion)
        {
            Log.Info("check manifest end, need download manifest files");
            onUpdatingCallBack.InvokeAllCallBack();

            if (null == _filterPrefixPaths)
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.OnlyDownloadConfig);
            }
            else if (_listUpdateAssetBundle.Count <= 0)
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToDownloadResource);
            }

            //manifest文件下载完毕后需要重新加载一次
            HotUpdateManifest.Reload(_multiVersionControlRelativePath);

            CheckVersionControlFile(urlVersion);
        }

        protected long ComputerCurrentNeedUpdateDataSize(List<HotUpdateDefine.DownloadAssetBundle> needUpdateAssetBundles)
        {
            long retValue = 0;
            for (int i = needUpdateAssetBundles.Count - 1; i >= 0; --i)
            {
                retValue += needUpdateAssetBundles[i].ExportInfo.fileSize;
            }
            return retValue;
        }

        // /// <summary>
        // /// 从网络下载或者本地只读路径中创建资源
        // /// </summary>
        // /// <param name="url">URL.</param> 网址或者只读文件夹相对路径
        // /// <param name="callbackLoadProgress">Callback load progress.</param> 进度回调函数
        // /// <param name="param">Parameter.</param> 自定义参数，会在进度回调函数中作为参数返回
        // private void CreateByWWW(string url)
        // {
        //     if (_isWorking)
        //     {
        //         Log.Warning("HotUpdateImporWWW warning: is busy now, please wait");
        //         return;
        //     }

        //     OnStartWorking();

        //     _url = url;
        //     DownloadByHttp(_url);
        // }

        // private void CreateByWWWAutoPlatform(string url)
        // {
        //     url = HotUpdateHelper.GetAssetBundlePathAutoPlatform(url, _multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
        //     CreateByWWW(url);
        // }
    }
}