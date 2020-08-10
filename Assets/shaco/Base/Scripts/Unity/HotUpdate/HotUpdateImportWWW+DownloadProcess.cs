using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace shaco
{
    public partial class HotUpdateImportWWW : HotUpdateDownloader, IHotUpdateImportWWW
    {
        //step 1 - 检查main md5文件是否正确
        protected void CheckMainMD5File(string urlVersion, string mainMD5, string packageVersion)
        {
            if (!string.IsNullOrEmpty(mainMD5))
            {
                //为了保持和从网络下载一样逻辑，这里需要延迟0.1秒后再执行
                shaco.Delay.Run(() =>
                {
                    OnMainMD5SuccessCallBack(urlVersion, mainMD5, string.Empty);
                }, 0.1f);
                return;
            }
            var filenameMainMD5 = HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + HotUpdateDefine.EXTENSION_MAIN_MD5;
            var urlMainMD5File = shaco.Base.FileHelper.ContactPath(urlVersion, filenameMainMD5);
            // urlMainMD5File = shaco.Base.Utility.GetHttpRequestFullUrl(urlMainMD5File, new shaco.Base.HttpComponent("rand_num", new System.Random().Next().ToString()));

            var updateTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDownloader());
            var httpHelperTmp = updateTmp.GetHttpHelper();

            updateTmp.DownloadByHttp(urlMainMD5File);
            updateTmp.onDownloadEndCalback.AddCallBack(this, (object defaultSender) =>
            {
                if (httpHelperTmp.IsSuccess())
                {
                    OnMainMD5SuccessCallBack(urlVersion, httpHelperTmp.GetDownloadByte().ToStringArray(), httpHelperTmp.GetLastError());
                }
                else
                {
                    if (updateTmp.HasError())
                    {
                        //If can't download main md5 file, it means url is missing
                        if (httpHelperTmp.IsNotFound404() || httpHelperTmp.IsForbidden403())
                        {
                            _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.ErrorNotFoundResource);
                        }
                        else
                        {
                            //Check whether the local resource needs to be updated without network
                            if (_filterPrefixPaths.IsNullOrEmpty())
                            {
                                if (HotUpdateHelper.CheckUpdateLocalOnly(packageVersion, _multiVersionControlRelativePath))
                                {
                                    _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.ErrorNeedUpdateResourceWithNetWork);
                                }
                                else
                                {
                                    _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig);
                                }
                            }
                        }

                        if (!HasStatus(HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig))
                        {
                            OnError("check main md5 file error: msg=" + updateTmp.GetLastError());
                        }
                        OnCompleted();
                    }
                }
            });
        }

        protected void OnMainMD5SuccessCallBack(string urlVersion, string mainMD5, string lastError)
        {
            OnError(lastError);

            var mainMD5Path = HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(_multiVersionControlRelativePath);
            var readMainMD5 = string.Empty;
            if (shaco.Base.FileHelper.ExistsFile(mainMD5Path))
                readMainMD5 = shaco.Base.FileHelper.ReadAllByUserPath(HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(_multiVersionControlRelativePath));

            var mainMD5Server = mainMD5;
            Log.Info("downloaded main md5=" + mainMD5Server + " local main md5=" + readMainMD5);

            _versionControlClient = null; //重制本地client让它可以重新加载
            CheckGetVersionControl();

            if (string.IsNullOrEmpty(readMainMD5) || readMainMD5 != mainMD5Server
                || !HotUpdateHelper.HasAllVersionControlFiles(_multiVersionControlRelativePath)
                || !HotUpdateHelper.CheckAllAssetbundleValidLocal(_versionControlClient, this._filterPrefixPaths, _multiVersionControlRelativePath))
            {
                //delete main md5 file at first, if downloading have some error, we can check update again
                HotUpdateHelper.DeleteAssetbundleConfigMainMD5(_multiVersionControlRelativePath);

                //set need resave main md5
                _strNeedRewriteMainMD5 = mainMD5Server;

                //remove last DirectorySeparatorChar if have
                if (!string.IsNullOrEmpty(urlVersion) && urlVersion[urlVersion.Length - 1] == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
                {
                    urlVersion = urlVersion.Remove(urlVersion.Length - 1);
                }

                Log.Info("check main md5 file end, will check version control file");
                CheckManifestBase(urlVersion);
            }
            else
            {
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToDownloadResource);
                _updateStatus.SetStatus(HotUpdateDownloadStatus.Status.NoNeedToUpdateConfig);

                onCheckVersionEndCallBack.InvokeAllCallBack();

                Log.Info("check main md5 file end, nothing changed");

                //quickly check update end, if main md5 is not changed !
                OnError(lastError);
                OnCompleted();
            }
        }

        //step 3 - 检查版本控制文件是否正确
        protected void CheckVersionControlFile(string urlVersion)
        {
            var urlVersionControlFile = urlVersion.ContactPath(_strNeedRewriteMainMD5).ContactPath(HotUpdateHelper.GetVersionControlFileName());

            var updateTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDownloader());
            var httpHelperTmp = updateTmp.GetHttpHelper();
            float prevProgress = 0;

            updateTmp.DownloadByHttp(urlVersionControlFile);
            updateTmp.onDownloadingCallBack.AddCallBack(this, (object defaultSender) =>
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

            updateTmp.onDownloadEndCalback.AddCallBack(this, (object defaultSender) =>
            {
                if (httpHelperTmp.IsSuccess())
                {
                    var readVersionBytes = httpHelperTmp.GetDownloadByte();
                    shaco.GameHelper.objectpool.RecyclingAllObjects(typeof(HotUpdateDownloader).ToTypeString());

                    Log.Info("check version control file end, will check upadate list", Color.white);
                    CheckUpdateByWWW_1(readVersionBytes, urlVersion);
                }
                else
                {
                    if (updateTmp.HasError())
                    {
                        OnError("check version control file 1 error msg=" + updateTmp.GetLastError());
                        OnCompleted();
                    }
                }
            });
        }

        //step 4.1 - 查看版本控制文件，是否需要下载新版控制文件
        protected void CheckUpdateByWWW_1(byte[] headerBytes, string urlVersion)
        {
            HotUpdateDefine.SerializeVersionControl versionServer = null;
            var strVersionControlSavePath = string.Empty;            
            try
            {
                strVersionControlSavePath = CheckGetVersionControl();
                versionServer = HotUpdateHelper.JsonToVersionControlHeader(headerBytes);
            }
            catch (System.Exception e)
            {
                versionServer = null;
                OnError("HotUpdate CheckUpdateByWWW_1 error: unable to parse json exception msg=" + e);
                OnCompleted();
            }

            if (versionServer == null)
            {
                OnError("HotUpdate CheckUpdateByWWW_1 error: can't load version control server");
                OnCompleted();
                return;
            }

            if (versionServer.baseVersion >= shaco.HotUpdateDefine.BaseVersionType.VER_2_0)
            {
                //开始下载新的文件列表
                var urlVersionControlFile = urlVersion.ContactPath(_strNeedRewriteMainMD5).ContactPath(HotUpdateHelper.GetVersionControlFileName());
                urlVersionControlFile = shaco.Base.FileHelper.ReplaceLastExtension(urlVersionControlFile, shaco.HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME);

                var updateTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDownloader());
                var httpHelperTmp = updateTmp.GetHttpHelper();
                float prevProgress = 0;

                updateTmp.DownloadByHttp(urlVersionControlFile);
                updateTmp.onDownloadingCallBack.AddCallBack(this, (object defaultSender) =>
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

                updateTmp.onDownloadEndCalback.AddCallBack(this, (object defaultSender) =>
                {
                    if (httpHelperTmp.IsSuccess())
                    {
                        Log.Info("check version control file end, will check upadate list, base version=" + versionServer.baseVersion, Color.white);
                        var downloadBytes = httpHelperTmp.GetDownloadByte();

                        //写入临时服务器文件标记到本地
                        var versionControlFolderPath = shaco.Base.FileHelper.GetFolderNameByPath(strVersionControlSavePath);
                        var tmpFileListPath = versionControlFolderPath.ContactPath("server/tmp" + shaco.HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME);
                        if (shaco.Base.FileHelper.ExistsFile(tmpFileListPath))
                            shaco.Base.FileHelper.DeleteByUserPath(tmpFileListPath);

                        shaco.Base.FileHelper.WriteAllByteByUserPath(tmpFileListPath, downloadBytes);
                        HotUpdateHelper.JsonToVersionControlBody(tmpFileListPath, versionServer, false);
                        shaco.Base.FileHelper.DeleteByUserPath(versionControlFolderPath.ContactPath("server"));

                        shaco.GameHelper.objectpool.RecyclingAllObjects(typeof(HotUpdateDownloader).ToTypeString());

                        //为了避免同一帧GC过高导致的闪退，在这里必须要等待一下
                        System.GC.Collect();
                        shaco.Delay.Run(() =>
                        {
                            CheckUpdateByWWW_2(versionServer, urlVersion, headerBytes, downloadBytes);
                        }, 1.0f);
                    }
                    else
                    {
                        if (updateTmp.HasError())
                        {
                            OnError("check version control 2 file error msg=" + updateTmp.GetLastError());
                            OnCompleted();
                        }
                    }
                });
            }
            else
            {
                CheckUpdateByWWW_2(versionServer, urlVersion, headerBytes, null);
            }
        }

        //step 4.2 - 对比版本控制文件，检查需要更新的资源包
        protected void CheckUpdateByWWW_2(HotUpdateDefine.SerializeVersionControl versionServer, string urlVersion, byte[] headerBytes, byte[] bodyBytes)
        {
            _listUpdateAssetBundle.Clear();
            var pathRoot = shaco.Base.FileHelper.GetFullpath(string.Empty);

            //update version
            if (_versionControlClient.Version != versionServer.Version)
            {
                Log.Info("HotUpdate check update version: client version=" + _versionControlClient.Version + " server version=" + versionServer.Version);
                _versionControlClient.Version = versionServer.Version;
            }

            var versionControlFullPath = shaco.Base.FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, _multiVersionControlRelativePath));
            if (HotUpdateHelper.ExecuteVersionControlAPI(versionControlFullPath, versionServer, false, null))
            {
                //force completed
                OnCompleted();
                return;
            }

            _versionControlClient.VersionControlAPI = versionServer.VersionControlAPI;

            //筛选需要更新的资源
            bool isOnlyCheckMD5 = !_filterPrefixPaths.IsNullOrEmpty();
            var listUpdateInfoTmp = HotUpdateHelper.SelectUpdateFiles(pathRoot, _multiVersionControlRelativePath, _versionControlClient.DicAssetBundles, versionServer.ListAssetBundles, false, isOnlyCheckMD5);

            //提前删除需要更新的资源，以便后续的动态下载(加载)
            if (!DeleteAssetBundles(listUpdateInfoTmp, _multiVersionControlRelativePath))
            {
                Log.Error("HotUpdate CheckUpdateByWWW error: can't delete assetbundle 1, please see log with tag=shaco.Base.FileHelper");
            }

            //根据筛选标记，再次筛选需要更新的资源
            if (!_filterPrefixPaths.IsNullOrEmpty())
            {
                //第一次筛选出需要更新资源
                var needCheckUpdateServerAssetbundle = new Dictionary<string, HotUpdateDefine.ExportAssetBundle>();

                //获取当前资源可能引用到的其他资源，并检测是否需要更新
                var defaultPrefixKey = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER;
                for (int i = _filterPrefixPaths.Length - 1; i >= 0; --i)
                {
                    //过滤无法找到的文件，可能是配置或者文件路径错误，也可能是文件夹
                    //如果是文件夹则不再考虑引用关系了，因为再反复去遍历整个文件夹会消耗很长时间
                    //如果需要自动关联引用关系的，则需要直接指定对应的文件，而不是文件夹
                    if (_filterPrefixPaths[i].EndsWith(shaco.Base.FileDefine.PATH_FLAG_SPLIT))
                    {
                        continue;
                    }

                    var prefixPathTmp = _filterPrefixPaths[i];
                    if (!shaco.HotUpdateHelper.IsCustomPrefixPath(prefixPathTmp))
                        prefixPathTmp = defaultPrefixKey.ContactPath(prefixPathTmp);

                    var assetbundleNameTmp = shaco.Base.FileHelper.ReplaceLastExtension(prefixPathTmp, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                    assetbundleNameTmp = HotUpdateHelper.AssetBundlePathToKey(assetbundleNameTmp).ToLower();
                    shaco.HotUpdateDefine.ExportAssetBundle findAssetbundleTmp = null;
                    if (!versionServer.DicAssetBundles.TryGetValue(assetbundleNameTmp, out findAssetbundleTmp))
                    {
                        shaco.Log.Error("HotUpdateImportWWW+DownloadProcess CheckUpdateByWWW error: not found path=" + _filterPrefixPaths[i]);
                        continue;
                    }

                    //获取引用关系表
                    var assetbundlePathTmp = HotUpdateHelper.AssetBundleKeyToPath(assetbundleNameTmp);

                    var dependencies = HotUpdateManifest.GetDependenciesRecursive(assetbundlePathTmp, _multiVersionControlRelativePath);
                    if (dependencies.IsNullOrEmpty())
                        continue;

                    for (int j = dependencies.Length - 1; j >= 0; --j)
                    {
                        var dependenciesAssetbundleName = shaco.Base.FileHelper.ReplaceLastExtension(dependencies[j], shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                        dependenciesAssetbundleName = shaco.HotUpdateHelper.AssetBundlePathToKey(dependenciesAssetbundleName);
                        shaco.HotUpdateDefine.ExportAssetBundle findServerAssetbundleTmp = null;
                        if (versionServer.DicAssetBundles.TryGetValue(dependenciesAssetbundleName, out findServerAssetbundleTmp))
                        {
                            if (!needCheckUpdateServerAssetbundle.ContainsKey(findServerAssetbundleTmp.AssetBundleName))
                                needCheckUpdateServerAssetbundle.Add(findServerAssetbundleTmp.AssetBundleName, findServerAssetbundleTmp);
                        }
                        else
                            Log.Error("HotUpdateImportWWW+DownloadProcess CheckUpdateByWWW error: not found server assetbundle name=" + dependenciesAssetbundleName);
                    }
                }

                //筛选出需要更新的资源列表
                var serverAssetbundles = HotUpdateHelper.FilterDownloadFiles(versionServer.DicAssetBundles, this._filterPrefixPaths);
                serverAssetbundles.AddRange(needCheckUpdateServerAssetbundle.ToValueList());
                
                var filterUpdateInfoTmp = HotUpdateHelper.SelectUpdateFiles(pathRoot, _multiVersionControlRelativePath, _versionControlClient.DicAssetBundles, serverAssetbundles, false);
                _listUpdateAssetBundle.AddRange(filterUpdateInfoTmp.Convert(v => new HotUpdateDefine.DownloadAssetBundle(v)));

                //删除本次需要筛选更新的旧内容，以防止资源冗余
                if (!DeleteAssetBundles(filterUpdateInfoTmp, _multiVersionControlRelativePath))
                {
                    Log.Error("HotUpdate CheckUpdateByWWW error: can't delete assetbundle 2, please see log with tag=shaco.Base.FileHelper");
                }
            }
            else
            {
                _listUpdateAssetBundle.AddRange(listUpdateInfoTmp.Convert(v => new HotUpdateDefine.DownloadAssetBundle(v)));
            }

            Log.Info("check update assetbundle end, ready to download count=" + _listUpdateAssetBundle.Count);

            //计算本次需要下载的文件总大小和数量
            _currentNeedUpdateDataSize = ComputerCurrentNeedUpdateDataSize(_listUpdateAssetBundle);
            _iTotalDownloadCount = _listUpdateAssetBundle.Count;

            //check update manifest and download assetbundle
            Log.Info("check update list end, will check download manifest file");

            //版本控制文件检查结束
            onCheckVersionEndCallBack.InvokeAllCallBack();

            //写入版本控制文件
            var strVersionControlSavePath = CheckGetVersionControl();
            if (!headerBytes.IsNullOrEmpty())
                shaco.Base.FileHelper.WriteAllByteByUserPath(strVersionControlSavePath, headerBytes);
            if (!bodyBytes.IsNullOrEmpty())
            {
                var fileListPath = shaco.Base.FileHelper.ReplaceLastExtension(strVersionControlSavePath, shaco.HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME);
                shaco.Base.FileHelper.WriteAllByteByUserPath(fileListPath, bodyBytes);
            }

            //开始下载所有资源
            if (_listUpdateAssetBundle.Count > 0)
            {
                Log.Info("check manifest end, will download data");
                CheckDownloadAllAssetBundle(urlVersion);
            }
            //没有需要下载资源则直接结束
            else
            {
                //no assetbundle need download, quickly completed
                Log.Info("check manifest end, no date need update");
                OnCompleted();
            }
        }

        //step 2 - 检查manifest(关系依赖文件)是否正确
        protected void CheckManifestBase(string urlVersion)
        {
            var strManifestPath1 = HotUpdateHelper.GetAssetBundleManifestMemoryPathAutoPlatform(_multiVersionControlRelativePath);
            // var strManifestPath2 = shaco.Base.FileHelper.RemoveLastExtension(strManifestPath1);

            UpdateAssetBundleManifest(urlVersion, strManifestPath1);
        }

        //step 5 - 检查并开始更新资源
        protected void CheckDownloadAllAssetBundle(string urlVersion)
        {
            if (_listUpdateAssetBundle.Count <= 0)
            {
                return;
            }

            //删除路径中可能存在的MainMD5字段
            if (urlVersion.Contains(_strNeedRewriteMainMD5))
            {
                urlVersion = urlVersion.Remove(_strNeedRewriteMainMD5);
            }

            Log.Info("will start download assetbundle count=" + _listUpdateAssetBundle.Count);

            _fCurrentProgress = HotUpdateDefine.CHECK_VERSION_PROGRESS_RATIO;
            _iCurrentDownloadCount = 0;

            CheckUpdateAssetBundleBySequeue(downloadSqueueCount, urlVersion);
            ComputerDownloadSpeedAll();

            _actionCheckDownload = shaco.WaitFor.Run(() =>
                                               {
                                                   if (IsSuccess() || HasError() || _isLoadAllAssetbundleSuccess)
                                                   {
                                                       return true;
                                                   }
                                                   else
                                                   {
                                                       onUpdatingCallBack.InvokeAllCallBack();
                                                       return false;
                                                   }

                                               }, () =>
                                               {
                                                   //all assetbundle download completed
                                                   OnError(GetLastError());
                                                   OnCompleted();
                                               });
        }
    }
}