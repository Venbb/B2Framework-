using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class HotUpdateDataCache : shaco.IHotUpdateDataCache
    {
        public int loadedCount { get { return _mapDataCache.Count; } }

        private Dictionary<string, DataCache> _mapDataCache = new Dictionary<string, DataCache>();

        //加载Assetbundle所占进度百分比
        private readonly float LOAD_ASSETBUNDLE_PERCENT = 0.7f;

        public Object Read(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string fileName)
        {
            return Read(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, fileName, typeof(Object));
        }

        public T Read<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string fileName) where T : UnityEngine.Object
        {
            return (T)Read(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, fileName, typeof(T));
        }

        public Object Read(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string fileName, System.Type type)
        {
            var dataCacheTmp = GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies);
            Object retValue = null;

            dataCacheTmp.stackLocationRead.StartTimeSpanCalculate();
            {
                //检查引用的时候尽量不要获取堆栈信息否GC会很高
                if (autoCheckDepencies)
                {
                    dataCacheTmp.stackLocationRead.GetStack();
                }
                retValue = dataCacheTmp.hotUpdateDelMemory.Read(fileName, type);

                AddReadAssets(dataCacheTmp, retValue);

                //如果出现加载失败都情况，则删除原ab包信息，下次重新加载
                if (null == retValue)
                {
                    RemoveCacheWhenReadError(pathAssetBundle, multiVersionControlRelativePath);
                }
            }
            dataCacheTmp.stackLocationRead.StopTimeSpanCalculate();
            return retValue;
        }

        public Object[] ReadAll(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type)
        {
            Object[] ret = null;
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(pathAssetBundle);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            var fullPath = shaco.Base.FileHelper.GetFullpath(pathConvert);

            //read from one assetbundle
            if (shaco.Base.FileHelper.ExistsFile(fullPath))
            {
                var dataCacheTmp = GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies);
                dataCacheTmp.stackLocationRead.StartTimeSpanCalculate();
                {
                    //检查引用的时候尽量不要获取堆栈信息否GC会很高
                    if (autoCheckDepencies)
                    {
                        dataCacheTmp.stackLocationRead.GetStack();
                    }
                    ret = dataCacheTmp.hotUpdateDelMemory.ReadAll(type);

                    if (!ret.IsNullOrEmpty())
                    {
                        for (int i = dataCacheTmp.readedAssets.Count - 1; i >= 0; --i)
                        {
                            AddReadAssets(dataCacheTmp, ret[i]);
                        }
                        AddReferenceCount(dataCacheTmp, dataCacheTmp.readedAssets.Count);
                    }
                }
                dataCacheTmp.stackLocationRead.StopTimeSpanCalculate();
            }
            //read from directory
            else
            {
                var directoryFullPath = shaco.Base.FileHelper.RemoveLastExtension(fullPath);
                if (shaco.Base.FileHelper.ExistsDirectory(directoryFullPath))
                {
                    var listPath = GetAssetBunldesNameInFolder(directoryFullPath, multiVersionControlRelativePath);
                    ret = new Object[listPath.Count];

                    for (int i = 0; i < listPath.Count; ++i)
                    {
                        var assetbundleNameTmp = listPath[i];
                        ret[i] = Read(assetbundleNameTmp, multiVersionControlRelativePath, autoCheckDepencies, shaco.Base.FileHelper.RemoveLastExtension(assetbundleNameTmp), type);
                    }
                }
            }

            if (ret == null || ret.Length == 0)
            {
                Debug.LogError("HotUpdate ReadAll error: not a assetbundle path=" + fullPath);
            }

            return ret;
        }

        public Object[] ReadAll(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies)
        {
            return ReadAll(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, typeof(UnityEngine.Object));
        }

        public Object[] ReadAll<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies) where T : UnityEngine.Object
        {
            return ReadAll(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, typeof(T));
        }

        public void ReadAsync<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string filename, System.Action<T> callbackReadEnd, System.Action<float> callbackProgress) where T : UnityEngine.Object
        {
            ReadAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, filename, typeof(T), obj => callbackReadEnd(obj as T), callbackProgress);
        }

        public void ReadAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string filename, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress)
        {
            ReadAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, filename, typeof(Object), callbackReadEnd, callbackProgress);
        }

        public void ReadAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string filename, System.Type type, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress)
        {
            GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, (DataCache dataCache) =>
            {
                dataCache.hotUpdateDelMemory.ReadAsync(filename, type, (Object obj) =>
                {
                    AddReadAssets(dataCache, obj);
                    if (null == obj)
                    {
                        RemoveCacheWhenReadError(pathAssetBundle, multiVersionControlRelativePath);
                    }

                    if (null != callbackReadEnd)
                    {
                        dataCache.stackLocationRead.StartTimeSpanCalculate("HotUpdateDataCache ReadAsync: path=" + pathAssetBundle + " target=" + callbackReadEnd.Target + " method=" + callbackReadEnd.Method);
                        callbackReadEnd(obj);
                        dataCache.stackLocationRead.StopTimeSpanCalculate();
                    }
                }, (float percent) =>
                {
                    if (null != callbackProgress)
                    {
                        callbackProgress(percent * (1 - LOAD_ASSETBUNDLE_PERCENT) + LOAD_ASSETBUNDLE_PERCENT);
                    }
                });
            }, callbackProgress);
        }

        public void ReadAllAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress)
        {
            var pathConvert = HotUpdateHelper.AssetBundlePathToKey(pathAssetBundle);
            pathConvert = HotUpdateHelper.GetAssetBundlePathAutoPlatform(pathConvert, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            var fullPath = shaco.Base.FileHelper.GetFullpath(pathConvert);

            //是1个assetbundle文件，ab包下面还装有多个文件的类型
            if (shaco.Base.FileHelper.ExistsFile(fullPath))
            {
                GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, (DataCache dataCache) =>
                {
                    dataCache.hotUpdateDelMemory.ReadAllAsync(type, (Object[] objs) =>
                    {

                        if (null != callbackReadEnd)
                        {
                            shaco.GameHelper.profiler.BeginSample("HotUpdateDataCache ReadAllAsync: path=" + pathAssetBundle + " target=" + callbackReadEnd.Target + " method=" + callbackReadEnd.Method);
                            callbackReadEnd(objs);
                            shaco.GameHelper.profiler.EndSample();
                        }

                        if (!objs.IsNullOrEmpty())
                        {
                            for (int i = dataCache.readedAssets.Count - 1; i >= 0; --i)
                            {
                                AddReadAssets(dataCache, objs[i]);
                            }
                            AddReferenceCount(dataCache, dataCache.readedAssets.Count);
                        }
                    }, (float percent) =>
                    {
                        if (null != callbackProgress)
                        {
                            callbackProgress(percent * (1 - LOAD_ASSETBUNDLE_PERCENT) + LOAD_ASSETBUNDLE_PERCENT);
                        }
                    });
                }, callbackProgress);
            }
            //按照文件夹类型进行加载
            else
            {
                var directoryFullPath = shaco.Base.FileHelper.RemoveLastExtension(fullPath);
                if (shaco.Base.FileHelper.ExistsDirectory(directoryFullPath))
                {
                    var needLoadPaths = GetAssetBunldesNameInFolder(directoryFullPath, multiVersionControlRelativePath);
                    var loadedObjs = new List<Object>();

                    if (!needLoadPaths.IsNullOrEmpty())
                    {
                        ReadAllAsyncLoop(needLoadPaths, loadedObjs, multiVersionControlRelativePath, autoCheckDepencies, 0, callbackReadEnd, callbackProgress);
                    }
                    else if (null != callbackReadEnd)
                    {
                        callbackReadEnd(null);
                    }
                }
            }
        }

        public void ReadAllAsync<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress) where T : UnityEngine.Object
        {
            ReadAllAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, typeof(T), callbackReadEnd, callbackProgress);
        }
        

        public void ReadAllAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress)
        {
            ReadAllAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, typeof(UnityEngine.Object), callbackReadEnd, callbackProgress);
        }

        public void UnloadUnusedDatas(bool unloadAllLoadedObjects = false)
        {
            Log.Info("HotUpdateDataCache UnloadUnusedDatas: unloadAllLoadedObjects=" + unloadAllLoadedObjects, Color.white);

            if (unloadAllLoadedObjects)
                shaco.GameHelper.objectpool.Clear();
            else
                shaco.GameHelper.objectpool.UnloadUnusedPoolData();

            var needRemoveKeyValuePairs = new List<KeyValuePair<string, DataCache>>();
            foreach (var iter in _mapDataCache)
            {
                var dataCache = iter.Value;

                //当ab包没有加载或者卸载时候，直接将引用置空以便直接释放它
                if (!IsLoadingOrUnLoading(dataCache))
                {
                    dataCache.referenceCount = 0;
                }
                needRemoveKeyValuePairs.Add(iter);
            }

            for (int i = needRemoveKeyValuePairs.Count - 1; i >= 0; --i)
            {
                var iter = needRemoveKeyValuePairs[i];
                CheckSafeRemoveAssetbundle(iter.Key, iter.Value, unloadAllLoadedObjects, false, null);
            }

            //销毁可能使用过的下载器
            if (shaco.GameHelper.objectpool.isUnused(typeof(shaco.HotUpdateDownloader).ToTypeString()))
                shaco.GameHelper.objectpool.DestroyAllObjects(typeof(shaco.HotUpdateDownloader).ToTypeString());
            shaco.GameHelper.http.UnloadUnusedMemory();

            // #if UNITY_5_3_OR_NEWER
            //引用关系表重新加载意义不大
            //             HotUpdateManifest.Unload();
            // #endif
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public bool UnloadAssetBundle(string pathAssetBundle, bool unloadAllLoadedObjects, string multiVersionControlRelativePath)
        {
            if (string.IsNullOrEmpty(pathAssetBundle))
                return false;

            DataCache findDataCache = null;
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);

            if (!_mapDataCache.TryGetValue(key, out findDataCache))
            {
                shaco.Log.Error("HotUpdateDataCache UnloadAssetBundle error: not find assetbundle by path=" + key);
                return false;
            }

            //如果ab包还在加载中是要等待下
            bool isRemovedInCurrentFrame = CheckSafeRemoveAssetbundle(key, findDataCache, unloadAllLoadedObjects, true, () =>
            {
                //延迟等待销毁引用
                CheckUnloadDepencies(findDataCache, key, unloadAllLoadedObjects);
            });

            if (!isRemovedInCurrentFrame)
                return false;

            //当前帧直接销毁引用
            CheckUnloadDepencies(findDataCache, key, unloadAllLoadedObjects);
            return true;
        }

        public int UnloadAssetBundlesWithNameFlag(string nameFlag, bool unloadAllLoadedObjects, string multiVersionControlRelativePath)
        {
            List<string> removeKeys = new List<string>();
            foreach (var iter in _mapDataCache)
            {
                if (iter.Key.Contains(nameFlag))
                {
                    removeKeys.Add(iter.Key);
                }
            }
            for (int i = removeKeys.Count - 1; i >= 0; --i)
            {
                var key = removeKeys[i];
                UnloadAssetBundle(key, unloadAllLoadedObjects, multiVersionControlRelativePath);
            }

            if (removeKeys.Count > 10)
            {
                Resources.UnloadUnusedAssets();
                shaco.GameHelper.objectpool.UnloadUnusedPoolData();
                System.GC.Collect();
            }
            return removeKeys.Count;
        }

        public bool IsLoadedAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            return _mapDataCache.ContainsKey(key);
        }

        public bool IsLoadingAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            DataCache findValue = null;
            if (!_mapDataCache.TryGetValue(key, out findValue))
                return false;

            return findValue.isLoading;
        }

        public void AddReferenceCount(string pathAssetBundle, string multiVersionControlRelativePath, bool isResourcesAsset)
        {
            var key = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            DataCache findDataCache = null;
            if (!_mapDataCache.TryGetValue(key, out findDataCache))
            {
                //如果是Resources目录资源，则默认添加一个空的缓存信息
                if (isResourcesAsset)
                {
                    var newDataCache = new DataCache();
                    newDataCache.isResourcesAsset = true;
                    newDataCache.referenceCount = 1;
                    _mapDataCache.Add(key, newDataCache);
                }
                else
                {
                    Log.Error("HotUpdateDataCache AddReferenceCount error: not found key=" + key);
                }
                return;
            }

            AddReferenceCount(findDataCache);
        }

        public string GetFullAssetBundleKey(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var retValue = pathAssetBundle.ToLower();

            //补充前缀名
            if (!shaco.HotUpdateHelper.IsCustomPrefixPath(retValue))
            {
                retValue = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER.ContactPath(retValue);
            }

            //补充versioncontrol目录
            // if (!retValue.StartsWith(multiVersionControlRelativePath))
            // {
            //     retValue = HotUpdateHelper.GetVersionControlFolderAuto(HotUpdateHelper.AssetBundlePathToKey(retValue), multiVersionControlRelativePath);
            // }

            //补充后缀名
            if (!retValue.EndsWith(HotUpdateDefine.EXTENSION_ASSETBUNDLE))
            {
                retValue = shaco.Base.FileHelper.ReplaceLastExtension(retValue, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            }

            //获取可能存在的内部ab包名字
            retValue = HotUpdateHelper.GetInternalAssetbundleParentPathInFileList(retValue, multiVersionControlRelativePath);
            return retValue;
        }

        public bool RetainStart(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var fullKey = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            DataCache findValue = null;
            if (!_mapDataCache.TryGetValue(fullKey, out findValue))
            {
                Log.Error("HotUpdateDataCache RetainStart error: not found key=" + fullKey);
                return false;
            }
            findValue.isLoading = true;
            return true;
        }

        public bool RetainEnd(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var fullKey = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            DataCache findValue = null;
            if (!_mapDataCache.TryGetValue(fullKey, out findValue))
            {
                Log.Error("HotUpdateDataCache RetainEnd error: not found key=" + fullKey);
                return false;
            }
            findValue.isLoading = false;
            return true;
        }

        /// <summary>
        /// 获取所有缓存中的assetbundle信息
        /// </summary>
        public Dictionary<string, DataCache> GetAllCachedAssetbundle()
        {
            return _mapDataCache;
        }

        /// <summary>
        /// 同步读取一个assetbundle
        /// </summary>
        public bool LoadAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies)
        {
            var retValue = GetDataCacheWithAutoCreate(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies);
            return null != retValue && retValue.hotUpdateDelMemory.IsSuccess();
        }

        /// <summary>
        /// 异步读取一个assetbundle
        /// </summary>
        public void LoadAssetBundleAsync(string pathAssetBundle, System.Action<bool> callbackLoadEnd, System.Action<float> callbackProgress, string multiVersionControlRelativePath, bool autoCheckDepencies)
        {
            GetDataCacheWithAutoCreateAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, (dataCache) =>
            {
                if (null != callbackLoadEnd)
                {
                    shaco.GameHelper.profiler.BeginSample("HotUpdateDataCache LoadAssetBundleAsync path=" + pathAssetBundle + " target=" + callbackLoadEnd.Target + " method=" + callbackLoadEnd.Method);
                    callbackLoadEnd(null != dataCache && dataCache.hotUpdateDelMemory.IsSuccess());
                    shaco.GameHelper.profiler.EndSample();
                }
            }, (percent) =>
            {
                if (null != callbackProgress)
                {
                    callbackProgress(percent / LOAD_ASSETBUNDLE_PERCENT);
                }
            });
        }

        /// <summary>
        /// 循环加载文件夹中所有文件资源
        /// </summary>
        private void ReadAllAsyncLoop(IList<string> needLoadPaths, List<Object> loadedObjs, string multiVersionControlRelativePath, bool autoCheckDepencies, int index, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress)
        {
            //没有文件需要加载
            if (needLoadPaths.IsNullOrEmpty())
            {
                if (null != callbackReadEnd)
                {
                    callbackReadEnd(null);
                }
                return;
            }

            var filenameAssetBundle = needLoadPaths[index];
            ReadAsync(filenameAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, shaco.Base.FileHelper.RemoveLastExtension(filenameAssetBundle), (Object readObj) =>
            {
                //当前文件加载完毕
                if (null != readObj)
                    loadedObjs.Add(readObj);

                //准备加载下一个
                ++index;

                //没有下一个文件了，加载完毕
                if ((index < 0 || index > needLoadPaths.Count - 1) && null != callbackReadEnd)
                {
                    callbackReadEnd(loadedObjs.ToArray());
                }
                //继续加载下一个文件
                else
                {
                    ReadAllAsyncLoop(needLoadPaths, loadedObjs, multiVersionControlRelativePath, autoCheckDepencies, index, callbackReadEnd, callbackProgress);
                }

            }, (float percent) =>
            {
                if (null != callbackProgress)
                {
                    callbackProgress((1.0f / needLoadPaths.Count * index) + ((float)percent / (float)needLoadPaths.Count));
                }
            });
        }

        private DataCache GetDataCacheWithAutoCreate(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies)
        {
            var fullkey = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);

            DataCache ret = null;
            if (_mapDataCache.TryGetValue(fullkey, out ret))
            {
                //如果原来是Resources目录资源，则转换为Assetbundle资源进行加载
                if (ret.isResourcesAsset)
                {
                    _mapDataCache.Remove(fullkey);
                    ret.isResourcesAsset = false;
                    ret = GetDataCacheWithAutoCreate(ret, fullkey, multiVersionControlRelativePath, autoCheckDepencies);
                    return ret;
                }

                //如果对象正在loading，是无法获取数据的
                if (IsLoadingOrUnLoading(ret))
                {
                    Log.Error("HotUpdate GetDataCacheWithAutoCreate error: data is loading, please wait... path=" + pathAssetBundle + " multiVersionControlRelativePath=" + multiVersionControlRelativePath);
                    ret = new DataCache();
                }
            }
            else
            {
                var newDataCache = new DataCache();
                ret = GetDataCacheWithAutoCreate(newDataCache, fullkey, multiVersionControlRelativePath, autoCheckDepencies);
            }

            //增加引用计数
            AddReferenceCount(ret);
            return ret;
        }

        private DataCache GetDataCacheWithAutoCreate(DataCache dataCache, string fullkey, string multiVersionControlRelativePath, bool autoCheckDepencies)
        {
            dataCache.isLoading = true;
            dataCache.stackLocationCreate.StartTimeSpanCalculate();
            {
                dataCache.hotUpdateDelMemory.CreateByMemoryAutoPlatform(fullkey, multiVersionControlRelativePath, autoCheckDepencies);
                AddDataCache(fullkey, dataCache);
                dataCache.isLoading = false;

                //检查引用的时候尽量不要获取堆栈信息否GC会很高
                if (autoCheckDepencies)
                {
                    dataCache.stackLocationCreate.GetStack();
                }
            }
            dataCache.stackLocationCreate.StopTimeSpanCalculate();
            return dataCache;
        }

        private void AddDataCache(string key, DataCache data)
        {
            //只添加有效资源到缓存
            if (data.hotUpdateDelMemory.IsValidAsset())
            {
                if (_mapDataCache.ContainsKey(key))
                {
                    Log.Error("HotUpdate AddDataCache error: duplicate key=" + key);
                }
                else
                {
                    _mapDataCache.Add(key, data);
                }
            }
        }

        private void RemoveCacheWhenReadError(string pathAssetBundle, string multiVersionControlRelativePath)
        {
            var fullKey = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);
            if (_mapDataCache.ContainsKey(fullKey))
            {
                UnloadAssetBundle(pathAssetBundle, true, multiVersionControlRelativePath);
            }
        }

        private void GetDataCacheWithAutoCreateAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Action<DataCache> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            var fullKey = GetFullAssetBundleKey(pathAssetBundle, multiVersionControlRelativePath);

            DataCache ret = null;
            if (_mapDataCache.TryGetValue(fullKey, out ret))
            {
                //如果原来是Resources目录资源，则转换为Assetbundle资源进行加载
                if (ret.isResourcesAsset)
                {
                    _mapDataCache.Remove(fullKey);
                    ret.isResourcesAsset = false;
                    GetDataCacheWithAutoCreateAsync(ret, fullKey, multiVersionControlRelativePath, autoCheckDepencies, callbackLoadEnd, callbackProgress);
                    return;
                }

                //已有assetbundle
                //增加引用计数
                AddReferenceCount(ret);

                //检查引用的时候尽量不要获取堆栈信息否GC会很高
                if (autoCheckDepencies)
                {
                    ret.stackLocationRead.GetStack();
                }

                //如果目标正在加载，则需要等待一下，以免资源使用发生冲突
                if (IsLoadingOrUnLoading(ret))
                {
                    shaco.Base.WaitFor.Run(() =>
                    {
                        return !IsLoadingOrUnLoading(ret);
                    }, () =>
                    {
                        GetDataCacheAsync(ret, callbackProgress, callbackLoadEnd);
                    });
                }
                else
                {
                    GetDataCacheAsync(ret, callbackProgress, callbackLoadEnd);
                }
                return;
            }
            else
            {
                //新增assetbundle
                var newDataCache = new DataCache();
                GetDataCacheWithAutoCreateAsync(newDataCache, fullKey, multiVersionControlRelativePath, autoCheckDepencies, callbackLoadEnd, callbackProgress);
            }
        }

        private void GetDataCacheWithAutoCreateAsync(DataCache dataCache, string fullKey, string multiVersionControlRelativePath, bool autoCheckDepencies,
                                                     System.Action<DataCache> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            _mapDataCache.Add(fullKey, dataCache);

            //添加资源缓存
            dataCache.isLoading = true;
            dataCache.stackLocationCreate.StartTimeSpanCalculate();
            {
                //检查引用的时候尽量不要获取堆栈信息否GC会很高
                if (autoCheckDepencies)
                {
                    dataCache.stackLocationCreate.GetStack();
                    dataCache.stackLocationRead.GetStack();
                }

                //增加引用计数
                AddReferenceCount(dataCache);

                //加载完毕回调
                dataCache.hotUpdateDelMemory.onLoadEndCallBack.AddCallBack(dataCache.hotUpdateDelMemory, (object sender) =>
                {
                    //本地资源异步加载失败，清理缓存
                    if (!dataCache.hotUpdateDelMemory.IsSuccess())
                    {
                        // Log.Error("HotUpdateDataCache GetDataCacheWithAutoCreateAsync error: fullKey=" + fullKey);
                        _mapDataCache.Remove(fullKey);
                    }

                    GetDataCacheAsync(dataCache, callbackProgress, callbackLoadEnd);
                    dataCache.stackLocationCreate.StopTimeSpanCalculate();
                    dataCache.isLoading = false;
                });

                //加载进度回调
                if (callbackProgress != null)
                {
                    dataCache.hotUpdateDelMemory.onLoadingCallBack.AddCallBack(dataCache, (object defaultSender) =>
                    {
                        var progressTmp = dataCache.hotUpdateDelMemory.GetLoadProgress();

                        if (progressTmp < 1.0f)
                            callbackProgress(progressTmp * LOAD_ASSETBUNDLE_PERCENT);
                    });
                }
                dataCache.hotUpdateDelMemory.CreateByMemoryAsyncAutoPlatform(fullKey, multiVersionControlRelativePath, autoCheckDepencies);
            }
        }

        /// <summary>
        /// 异步获取缓存对象
        /// </summary>
        private void GetDataCacheAsync(DataCache dataCache, System.Action<float> callbackProgress, System.Action<DataCache> callbackLoadEnd)
        {
            if (null != callbackProgress)
                callbackProgress(LOAD_ASSETBUNDLE_PERCENT);

            if (null != callbackLoadEnd)
                callbackLoadEnd(dataCache);
        }

        //增加引用计数
        private void AddReferenceCount(DataCache dataCache, int referenceCount = 1)
        {
            dataCache.referenceCount += referenceCount;
            dataCache.hotUpdateDelMemory.CheckInterruptUnload();
        }

        //减少引用计数
        private void RemoveReferenceCount(DataCache dataCache)
        {
            --dataCache.referenceCount;
        }

        //检查并安全删除ab包
        //false: ab还在加载或者卸载中，需要等待完毕后才销毁
        //true: ab直接销毁了
        private bool CheckSafeRemoveAssetbundle(string key, DataCache dataCache, bool unloadAllLoadedObjects, bool autoRemoveInCaches, System.Action callbackDelayRemoveEnd)
        {
            //暂时不明白为什么要等待ab加载完毕后才卸载它，明明可以中断加载才是
            //日后如果出现异步递归释放资源时候出现问题的话才考虑再开启这里看看
            RemoveReferenceCount(dataCache);

            if (dataCache.referenceCount <= 0)
            {
                dataCache.referenceCount = 0;
                dataCache.hotUpdateDelMemory.Close(unloadAllLoadedObjects);
                if (_mapDataCache.ContainsKey(key))
                {
                    _mapDataCache.Remove(key);
                }
                else
                    shaco.Log.Warning("HotUpdateDataCache CheckSafeRemoveAssetbundle 1: not found key=" + key + " count=" + _mapDataCache.Count);
            }
            return true;
            // if (IsLoadingOrUnLoading(dataCache))
            // {
            //     shaco.Base.WaitFor.Run(() =>
            //     {
            //         return !IsLoadingOrUnLoading(dataCache);
            //     }, () =>
            //     {
            //         RemoveReferenceCount(dataCache);
            //         CheckRefCountAndRemoveAssetBundle(key, dataCache, unloadAllLoadedObjects, autoRemoveInCaches, callbackDelayRemoveEnd);
            //     });
            //     return false;
            // }
            // else
            // {
            //     RemoveReferenceCount(dataCache);

            //     //因为是同步卸载的就没有延时卸载回调了
            //     CheckRefCountAndRemoveAssetBundle(key, dataCache, unloadAllLoadedObjects, autoRemoveInCaches, null);
            //     return true;
            // }
        }

        private void CheckRefCountAndRemoveAssetBundle(string key, DataCache dataCache, bool unloadAllLoadedObjects, bool autoRemoveInCaches, System.Action callbackDelayRemoveEnd)
        {
            if (dataCache.referenceCount <= 0)
            {
                dataCache.referenceCount = 0;
                dataCache.hotUpdateDelMemory.Close(unloadAllLoadedObjects);

                if (autoRemoveInCaches)
                {
                    //如果资源需要异步卸载，则等待它完成
                    if (IsLoadingOrUnLoading(dataCache))
                    {
                        shaco.Base.WaitFor.Run(() =>
                        {
                            return !IsLoadingOrUnLoading(dataCache);
                        }, () =>
                        {
                            if (dataCache.referenceCount <= 0)
                            {
                                if (_mapDataCache.ContainsKey(key))
                                {
                                    _mapDataCache.Remove(key);
                                }
                                else
                                    shaco.Log.Warning("HotUpdateDataCache CheckRefCountAndRemoveAssetBundle 1: not found key=" + key + " count=" + _mapDataCache.Count);
                            }
                            if (null != callbackDelayRemoveEnd)
                                callbackDelayRemoveEnd();
                        });
                    }
                    else
                    {
                        if (dataCache.referenceCount <= 0)
                        {
                            if (_mapDataCache.ContainsKey(key))
                            {
                                _mapDataCache.Remove(key);
                            }
                            else
                                shaco.Log.Warning("HotUpdateDataCache CheckRefCountAndRemoveAssetBundle 2: not found key=" + key + " count=" + _mapDataCache.Count);
                        }
                        if (null != callbackDelayRemoveEnd)
                            callbackDelayRemoveEnd();
                    }
                }
            }
        }

        private void CheckUnloadDepencies(DataCache dataCache, string fullKey, bool unloadAllLoadedObjects)
        {
            //资源还在引用中
            if (dataCache.referenceCount > 0)
            {
                return;
            }

            //检查并自动卸载有引用到的ab包
            string[] depencies = null;
            var multiVersionControlRelativePath = dataCache.multiVersionControlRelativePath;

            //如果只从Resources目录加载的资源，是不Unity自动维护的引用关系，不需要再卸载
            //而从Assetbundle中加载的资源需要自行根据manifest文件维护引用关系和卸载
            if (!dataCache.isResourcesAsset)
                depencies = HotUpdateManifest.GetDependenciesRecursive(fullKey, multiVersionControlRelativePath);

            if (null != depencies)
            {
                for (int i = depencies.Length - 1; i >= 0; --i)
                {
                    var keyTmp = GetFullAssetBundleKey(depencies[i], multiVersionControlRelativePath);
                    DataCache findDependCache = null;

                    if (_mapDataCache.TryGetValue(keyTmp, out findDependCache))
                    {
                        //每unload一次的引用关系计数-1，当为0时候表示没有引用了，则可以销毁它
                        CheckSafeRemoveAssetbundle(keyTmp, findDependCache, unloadAllLoadedObjects, true, null);
                    }
                    else
                    {
                        Debug.LogError("HotUpdateDataCache UnloadAssetBundle error: not found paired reference count, depend path=" + depencies[i]);
                    }
                }
            }
        }

        private bool IsLoadingOrUnLoading(DataCache dataCache)
        {
            return dataCache.isLoading || (null == dataCache.hotUpdateDelMemory ? false : dataCache.hotUpdateDelMemory.IsUnLoading());
        }

        private IList<string> GetAssetBunldesNameInFolder(string path, string multiVersionControlRelativePath)
        {
            List<string> retValue = new List<string>();
            var versionControlFolderPath = shaco.HotUpdateHelper.GetAssetBundleFullPath(string.Empty, multiVersionControlRelativePath);

            shaco.Base.FileHelper.GetSeekPath(path, ref retValue, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            for (int i = 0; i < retValue.Count; ++i)
            {
                retValue[i] = retValue[i].Remove(versionControlFolderPath);
            }
            return retValue;
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        private void AddReadAssets(DataCache dataCache, Object asset)
        {
            if (null != asset)
            {
                if (dataCache.readedAssets.Count >= shaco.HotUpdateDefine.MAX_SAVE_READED_ASSET_COUNT)
                {
                    if (dataCache.readedAssets.Count > 0)
                        dataCache.readedAssets.RemoveAt(dataCache.readedAssets.Count - 1);
                }

                if (dataCache.readedAssets.Count < shaco.HotUpdateDefine.MAX_SAVE_READED_ASSET_COUNT)
                    dataCache.readedAssets.Add(asset);
            }
        }
    }
}