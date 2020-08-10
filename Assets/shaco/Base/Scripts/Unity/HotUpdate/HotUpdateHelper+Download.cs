using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public partial class HotUpdateHelper
    {
        //最大支持的下载队列数量
        //测试过真机上3个值最合适，超过该值后下载效率并不会提高，并且可能导致网络阻塞
        static private readonly short MAX_DOWNLOAD_COUNT = 2;

        //所有下载过的资源列表<资源版本列表, <AssetBundle本地不加密原始路径，AssetBundle对应的加密MD5🐎>>
        static private Dictionary<string, Dictionary<string, string>> _downloadedAllAssetbundleConfigs = new Dictionary<string, Dictionary<string, string>>();

        //动态网络资源下载地址
        //当参数不为空的时候，如果调用Async相关方法的时候，如果本地资源不存在，则会从服务器请求对应的资源，当服务器资源也没有的时候，才会返回null
        static private string[] _dynamicNetResourceAddress = null;

        //当前下载队列数量
        static private short _currentDownloadCount = 0;

        //获取资源内部名字使用的字符串Builder，这样GC比直接使用字符串拼接减少到1/4
        static private System.Text.StringBuilder _strBuilderInteralAssetPath = new System.Text.StringBuilder();

        /// <summary>
        /// 删除本地的MainMD5文件并在重新调用HotUpdateWWW.CheckUpdate时候可以重新下载
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static public void RequireReDownload(string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var localMainMD5Path = GetAssetBundleMainMD5MemoryPathAuto(multiVersionControlRelativePath);
            if (shaco.Base.FileHelper.ExistsFile(localMainMD5Path))
            {
                shaco.Base.FileHelper.DeleteByUserPath(localMainMD5Path);
            }
        }

        /// <summary>
        /// 检查并自动加载已经下载的配置文件
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static public void CheckLoadDownloadedAssetBundleConfig(string multiVersionControlRelativePath)
        {
            if (!_downloadedAllAssetbundleConfigs.ContainsKey(multiVersionControlRelativePath))
            {
                var versionFilePath = GetVersionControlFilePath(string.Empty, multiVersionControlRelativePath);
                versionFilePath = shaco.Base.FileHelper.GetFullpath(versionFilePath);

                if (shaco.Base.FileHelper.ExistsFile(versionFilePath))
                {
                    try
                    {
                        var newConfig = new Dictionary<string, string>();
                        var versionControlTmp = HotUpdateHelper.PathToVersionControl(versionFilePath, false);

                        for (int i = versionControlTmp.ListAssetBundles.Count - 1; i >= 0; --i)
                        {
                            var assetbundleInfoTmp = versionControlTmp.ListAssetBundles[i];
                            var key = AssetBundleKeyToPath(assetbundleInfoTmp.AssetBundleName);
                            var value = assetbundleInfoTmp.AssetBundleMD5;
                            if (!newConfig.ContainsKey(key))
                            {
                                newConfig.Add(key, value);
                            }
                            else
                            {
                                Log.Error("HotUpdateHelper CheckLoadDownloadedAssetBundleConfig error: has duplicate key=" + key + " path=" + versionFilePath);
                            }
                        }
                        _downloadedAllAssetbundleConfigs.Add(multiVersionControlRelativePath, newConfig);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("HotUpdateHelper CheckLoadDownloadedAssetBundleConfig error: can't read json path=" + versionFilePath + "\n" + e);
                    }
                }
                else
                {
                    Log.Error("HotUpdateHelper CheckLoadDownloadedAssetBundleConfig error: not found version control file path=" + versionFilePath);
                }
            }
        }

        /// <summary>
        /// 文件原始路径根据本地配置转换为加密的文件路径(资源的真实路径)
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="errorMessage">报错日志信息</param>
        /// </summary>
        /// <returns> 返回加密文件路径，返回空表示转化失败
        static public string ConvertToDownloadedEncodePath(string path, string multiVersionControlRelativePath, out string errorMessage)
        {
            var retValue = string.Empty;
            Dictionary<string, string> configFind = null;
            errorMessage = string.Empty;

            if (!_downloadedAllAssetbundleConfigs.TryGetValue(multiVersionControlRelativePath, out configFind))
            {
                errorMessage = "HotupdateHelper ConvertToDownloadedLocalPath error: not found config, path=" + path;
            }
            else
            {
                string md5Find = string.Empty;
                if (!configFind.TryGetValue(path, out md5Find))
                {
                    errorMessage = "HotupdateHelper ConvertToDownloadedLocalPath error: not found assetbundle config, path=" + path;
                }
                else
                {
                    if (path.Contains(md5Find))
                    {
                        Log.Warning("HotUpdateHelper ConvertToDownloadedLocalPath warning: has added version folder flag path=" + path);
                    }
                    else
                    {
                        path = HotUpdateHelper.GetAssetBundlePathAutoPlatform(path, multiVersionControlRelativePath, shaco.Base.FileHelper.GetFilNameExtension(path));
                        path = HotUpdateHelper.AddAssetBundleNameTag(path, md5Find);

                        retValue = path;
                    }
                }
            }
            return retValue;
        }

        //设置网络动态下载资源路径
        static public void SetDynamicResourceAddress(params string[] address)
        {
            _dynamicNetResourceAddress = address;
        }

        //获取网络动态下载服务器地址
        static public string[] GetDynamicResourceAddress()
        {
            return _dynamicNetResourceAddress;
        }

        /// <summary>
        /// 根据名字前缀筛选文件夹或者文件
        /// <param name="filterPrefixPaths">筛选文件路径前缀</param>
        /// <param name="prefixPath">筛选路径前缀，默认为resources_hotupdate</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>下载文件列表</return>
        /// </summary>
        static public List<string> FilterAssetBundlesFolderOrFile(string[] filterPrefixPaths, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var retValue = new System.Collections.Generic.List<string>();
            filterPrefixPaths = ConvertToAssetbundlePath(filterPrefixPaths);

            //确定本地资源配置是否加载过了
            CheckLoadDownloadedAssetBundleConfig(multiVersionControlRelativePath);

            Dictionary<string, string> configFind = null;
            if (!_downloadedAllAssetbundleConfigs.TryGetValue(multiVersionControlRelativePath, out configFind))
            {
                Log.Error("HotUpdateHelper FilterDownloadAssetBundlesInConfig error: not found assetbundles config, multiVersionControlRelativePath=" + multiVersionControlRelativePath);
            }
            else
            {
                foreach (var iter in configFind)
                {
                    if (IsFilterAssetbundle(iter.Key, filterPrefixPaths))
                    {
                        retValue.Add(iter.Key);
                    }
                }
            }

            return retValue;
        }

        static public List<string> FilterAssetBundlesFolderOrFile(string filterPrefixPath, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            return FilterAssetBundlesFolderOrFile(new string[] { filterPrefixPath }, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 单独下载assetbundle目录或者类似文件
        /// <param name="filterPrefixPath">筛选文件路径前缀</param>
        /// <param name="prefixPath">筛选路径前缀，默认为resources_hotupdate</param>
        /// <param name="downloadRootUrl">资源下载地址</param>
        /// <param name="callbackProgress">下载进度回调</param>
        /// <param name="callbackLoadEnd">下载完毕回调</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>需要下载的资源列表</return>
        /// </summary>
        static public string[] DownloadAssetBundlesFolderOrFile(string[] filterPrefixPaths, string[] downloadRootUrl, System.Action<float> callbackProgress, System.Action callbackLoadEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            string[] retValue = new string[0];

            //没有设定下载地址，直接返回
            if (downloadRootUrl.IsNullOrEmpty())
            {
                if (null != callbackProgress) callbackProgress(1);
                if (null != callbackLoadEnd) callbackLoadEnd();
                return retValue;
            }

            var needDownloadPaths = FilterAssetBundlesFolderOrFile(filterPrefixPaths, multiVersionControlRelativePath);
            if (needDownloadPaths.IsNullOrEmpty())
            {
                Log.Error("HotUpdateHelper DownloadAssetBundlesFolderOrFile error: not found need download assetbundles, filterPrefixPaths=" + filterPrefixPaths.ToSerializeString() + " multiVersionControlRelativePath=" + multiVersionControlRelativePath);
                if (null != callbackProgress) callbackProgress(1);
                if (null != callbackLoadEnd) callbackLoadEnd();
                return retValue;
            }

            //过滤已经下载过的文件
            for (int i = needDownloadPaths.Count - 1; i >= 0; --i)
            {
                //获取下载路径
                var versionControlFullPath = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                var localFullPath = shaco.Base.FileHelper.ContactPath(versionControlFullPath, needDownloadPaths[i].RemoveFront(versionControlFullPath));
                localFullPath = shaco.Base.FileHelper.GetFullpath(localFullPath);

                //如果文件存在，则不再重复下载
                if (shaco.Base.FileHelper.ExistsFile(localFullPath))
                {
                    needDownloadPaths.RemoveAt(i);
                }
            }

            if (needDownloadPaths.IsNullOrEmpty())
            {
                Log.Info("HotUpdateHelper DownloadAssetBundlesFolderOrFile: nothing need download, filterPrefixPaths=" + filterPrefixPaths.ToSerializeString() + " multiVersionControlRelativePath=" + multiVersionControlRelativePath);
                if (null != callbackProgress) callbackProgress(1);
                if (null != callbackLoadEnd) callbackLoadEnd();
                return retValue;
            }

            DownloadAssetBundlesSequeue(needDownloadPaths, downloadRootUrl, multiVersionControlRelativePath, callbackProgress, callbackLoadEnd, 0);
            retValue = needDownloadPaths.ToArray();
            return retValue;
        }
        static public string[] DownloadAssetBundlesFolderOrFile(string[] filterPrefixPaths, System.Action<float> callbackProgress, System.Action callbackLoadEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            return DownloadAssetBundlesFolderOrFile(filterPrefixPaths, GetDynamicResourceAddress(), callbackProgress, callbackLoadEnd, multiVersionControlRelativePath);
        }
        static public string[] DownloadAssetBundlesFolderOrFile(string filterPrefixPath, System.Action<float> callbackProgress, System.Action callbackLoadEnd, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            return DownloadAssetBundlesFolderOrFile(new string[] { filterPrefixPath }, GetDynamicResourceAddress(), callbackProgress, callbackLoadEnd, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 队列下载资源
        /// <param name="needDownloadPaths">需要下载的资源列表</param>
        /// <param name="downloadRootUrl">资源下载地址</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="callbackProgress">下载进度回调</param>
        /// <param name="callbackLoadEnd">下载完毕回调</param>
        /// <param name="currentIndex">当前文件下载所在资源列表中的下标</param>
        /// </summary>
        static private void DownloadAssetBundlesSequeue(List<string> needDownloadPaths, string[] downloadRootUrl, string multiVersionControlRelativePath, System.Action<float> callbackProgress, System.Action callbackLoadEnd, int currentIndex)
        {
            ForeachDownloadAssetbundleByUrls(needDownloadPaths[currentIndex], multiVersionControlRelativePath, (string error) =>
            {
                //下载所有文件完毕
                if (currentIndex >= needDownloadPaths.Count - 1)
                {
                    if (null != callbackLoadEnd)
                        callbackLoadEnd();
                }
                //继续下个文件下载
                else
                {
                    DownloadAssetBundlesSequeue(needDownloadPaths, downloadRootUrl, multiVersionControlRelativePath, callbackProgress, callbackLoadEnd, currentIndex + 1);
                }
            }, (float percent) =>
            {
                if (null != callbackProgress)
                {
                    float currentProgress = percent / (float)needDownloadPaths.Count + (1.0f / needDownloadPaths.Count * currentIndex);
                    callbackProgress(currentProgress);
                }
            });
        }

        /// <summary>
        /// 获取内部ab包资源的父ab包名字
        /// <param name="fileName">ab包名字</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>1、返回当前ab包名字 2、当为内部ab包资源则返回父ab包名字 3、未找到ab包返回空字符串</return>
        /// </summary>
        static public string GetInternalAssetbundleParentPathInFileList(string fileName, string multiVersionControlRelativePath)
        {
            //使用当前ab包名字
            if (ExistsInFileList(fileName, multiVersionControlRelativePath))
                return fileName;

            lock (_strBuilderInteralAssetPath)
            {
                _strBuilderInteralAssetPath.Length = 0;

                //查找最后一个路径分隔符位置
                var indexEnd = fileName.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT) - shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING.Length;
                if (indexEnd < 0)
                    return shaco.Base.GlobalParams.EmptyString;

                //获取文件夹名字，如 1/2/3.assetbundle -> 1/2
                _strBuilderInteralAssetPath.Append(fileName, 0, indexEnd + shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING.Length);

                var indexStart = fileName.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT, indexEnd);
                if (indexStart < 0)
                    return shaco.Base.GlobalParams.EmptyString;

                //获取文件夹名字拼接，如 1/2 -> 1/2/2
                for (int i = indexStart; i <= indexEnd; ++i)
                {
                    _strBuilderInteralAssetPath.Append(fileName[i]);
                }

                //添加assetbundle后缀名
                _strBuilderInteralAssetPath.Append(HotUpdateDefine.EXTENSION_ASSETBUNDLE);

                //转换为最终的路径使用
                var parentFileName = _strBuilderInteralAssetPath.ToString();
                if (ExistsInFileList(parentFileName, multiVersionControlRelativePath))
                {
                    //使用父ab包名字
                    return parentFileName;
                }
                else
                {
                    //没有找到ab包，返回空名字
                    return shaco.Base.GlobalParams.EmptyString;
                }
            }
        }

        /// <summary>
        /// 依次从服务器地址中获取资源，如果获取到了正确资源，则停止遍历
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <param name="downloadRootUrl">资源下载地址</param>
        /// <param name="callbackLoadEnd">下载完毕回调，如果回调参数不为空，则表示有发生错误</param>
        /// <param name="currentIndex">当前动态服务器地址使用下标</param>
        /// <param name="lastError">最后一次发生的错误信息</param>
        /// <returns>
        static public void ForeachDownloadAssetbundleByUrls(string path, string multiVersionControlRelativePath, System.Action<string> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            ForeachDownloadAssetbundleByUrls(path, multiVersionControlRelativePath, GetDynamicResourceAddress(), callbackLoadEnd, callbackProgress, 0, string.Empty);
        }
        static private void ForeachDownloadAssetbundleByUrls(string path, string multiVersionControlRelativePath, string[] downloadRootUrl, System.Action<string> callbackLoadEnd, System.Action<float> callbackProgress, int currentIndex, string lastError)
        {
            //超出最大队列下载数量，挂起等待
            if (_currentDownloadCount >= MAX_DOWNLOAD_COUNT)
            {
                shaco.WaitFor.Run(() =>
                {
                    return _currentDownloadCount < MAX_DOWNLOAD_COUNT;
                }, () =>
                {
                    StartDynammicDownload(path, multiVersionControlRelativePath, downloadRootUrl, callbackLoadEnd, callbackProgress, currentIndex, lastError);
                });
            }
            else
            {
                StartDynammicDownload(path, multiVersionControlRelativePath, downloadRootUrl, callbackLoadEnd, callbackProgress, currentIndex, lastError);
            }
        }

        static private void StartDynammicDownload(string path, string multiVersionControlRelativePath, string[] downloadRootUrl, System.Action<string> callbackLoadEnd, System.Action<float> callbackProgress, int currentIndex, string lastError)
        {
            //网络动态加载Assetbundle
            if (!downloadRootUrl.IsNullOrEmpty())
            {
                //所有下载地址遍历完毕也下载资源失败，退出循环
                if (currentIndex < 0 || currentIndex > downloadRootUrl.Length - 1)
                {
                    if (null != callbackProgress) callbackProgress(1);
                    if (null != callbackLoadEnd) callbackLoadEnd(lastError);
                    return;
                }

                //获取相对路径
                var relativePath = AssetBundleFullPathToRelativePath(path, multiVersionControlRelativePath);

                //确定本地资源配置是否加载过了
                CheckLoadDownloadedAssetBundleConfig(multiVersionControlRelativePath);

                //获取加密后的地址
                string errorMessage = null;
                var relativePathWithMD5 = HotUpdateHelper.ConvertToDownloadedEncodePath(relativePath, multiVersionControlRelativePath, out errorMessage);

                //没有该资源记录，停止下载
                if (string.IsNullOrEmpty(relativePathWithMD5))
                {
                    if (null != callbackProgress) callbackProgress(1);
                    if (null != callbackLoadEnd) callbackLoadEnd(errorMessage);
                    return;
                }

                var address = downloadRootUrl[currentIndex];

                //错误的空下载地址
                if (string.IsNullOrEmpty(address))
                {
                    if (null != callbackProgress) callbackProgress(1);
                    if (null != callbackLoadEnd) callbackLoadEnd("HotUpdateHelper+Download StartDynammicDownload: invalid dynamic download url");
                    return;
                }

                //删除动态网址中尾部包含版本目录的地方
                if (address.LastIndexOf(multiVersionControlRelativePath) >= 0)
                {
                    address = address.RemoveBehind(multiVersionControlRelativePath);
                }

                //获取请求资源地址
                address = shaco.Base.FileHelper.ContactPath(address, relativePathWithMD5);

                //给网址添加随机数，以免获取到服务器旧的缓存资源，导致资源更新失败
                // address = shaco.Base.Utility.GetHttpRequestFullUrl(address, new shaco.Base.HttpComponent("rand_num", shaco.Base.Utility.Random().ToString()));

                //下载成功后自动写入文件路径
                var versionControlFullPath = HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                var localFullPath = shaco.Base.FileHelper.ContactPath(versionControlFullPath, path.RemoveFront(versionControlFullPath));
                localFullPath = shaco.Base.FileHelper.GetFullpath(localFullPath);

                //如果目标文件已存在，则停止下载
                if (shaco.Base.FileHelper.ExistsFile(localFullPath))
                {
                    if (null != callbackProgress) callbackProgress(1);
                    if (null != callbackLoadEnd) callbackLoadEnd(string.Empty);
                    return;
                }

                //开始从服务器下载资源
                var wwwTmp = shaco.GameHelper.objectpool.Instantiate(() => new HotUpdateDownloader());
                wwwTmp.DownloadByHttp(address);

                ++_currentDownloadCount;

                // Log.Info("HotUpdateHelper+Download url=" + address);

                wwwTmp.GetHttpHelper().SetAutoSaveWhenCompleted(localFullPath);

                //下载进度
                if (null != callbackProgress)
                {
                    wwwTmp.onDownloadingCallBack.AddCallBack(wwwTmp, (object sender) =>
                    {
                        callbackProgress(wwwTmp.GetDownloadResourceProgress());
                    });
                }

                //等待下载完毕回调
                wwwTmp.onDownloadEndCalback.AddCallBack(wwwTmp, (object sender2) =>
                {
                    if (wwwTmp.HasError())
                    {
                        //下载发生错误，进入下一个动态地址进行下载
                        var lastErrorTmp = wwwTmp.GetLastError();
                        wwwTmp.RecyclingWithPool();
                        --_currentDownloadCount;
                        ForeachDownloadAssetbundleByUrls(path, multiVersionControlRelativePath, downloadRootUrl, callbackLoadEnd, callbackProgress, currentIndex + 1, lastErrorTmp);
                    }
                    else if (wwwTmp.IsSuccess())
                    {
                        //下载资源成功
                        if (null != callbackLoadEnd)
                        {
                            callbackLoadEnd(string.Empty);
                        }

                        if (shaco.GameHelper.objectpool.IsInstantiated(wwwTmp))
                            wwwTmp.RecyclingWithPool();
                        --_currentDownloadCount;
                    }
                    else
                    {
                        shaco.Log.Error("HotUpdateHelper+Download StartDynammicDownload uncatched error: address=" + address);
                        wwwTmp.RecyclingWithPool();
                        --_currentDownloadCount;
                    }
                });
            }
        }

        /// <summary>
        /// 判断路径是否在资源表内
        /// <param name="assetbundleName">ab包名字</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static private bool ExistsInFileList(string fileName, string multiVersionControlRelativePath)
        {
            Dictionary<string, string> configFind = null;
            CheckLoadDownloadedAssetBundleConfig(multiVersionControlRelativePath);
            if (!_downloadedAllAssetbundleConfigs.TryGetValue(multiVersionControlRelativePath, out configFind))
                return false;
            return configFind.ContainsKey(fileName);
        }
    }
}