using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace shaco
{
    public class HotUpdateImport : shaco.Base.IObjectPoolData
    {
        private enum UnLoadingStatus
        {
            None,
            UnLoading,
            InterruptUnload,
            UnLoaded,
        }

        //读取到的资源对象
        protected AssetBundle _assetBundleRead = null;
        //当assetbundle为原始文件时候的输出
        protected TextOrigin _textAssetOrigin = null;
        //是否正在下载或读取
        protected bool _isWorking = false;

        //资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源
        protected string _multiVersionControlRelativePath = string.Empty;
        //当前进度(范围0 ~ 1.0)
        protected float _fCurrentProgress = 0;
        //是否已经关闭并清理过资源
        protected bool _isClosed { get { return __isClosed; } }
        //已经加载过的所有资源对象
        private Dictionary<string, Object> _loadedAllAssets = new Dictionary<string, Object>();
        //是否已经关闭并清理过资源
        private bool __isClosed = false;
        //最新一次错误信息
        private string _strLastError = string.Empty;
        //资源卸载状态
        private UnLoadingStatus _unLoadingStatus = UnLoadingStatus.None;
        //是否还在加载中
        private bool _isLoading = false;

        public void Dispose()
        {
            _textAssetOrigin = null;
            _isWorking = false;
            __isClosed = false;
            _multiVersionControlRelativePath = string.Empty;
            _loadedAllAssets.Clear();
            _strLastError = string.Empty;
        }

        /// <summary>
        /// 读取unity对象，支持TextAssets, GameObject, Texture2D
        /// </summary>
        /// <param name="fileName">File name.</param> assetbundle中的资源名字
        public Object Read(string fileName, System.Type type)
        {
            Object retValue = null;

            //从原始资源加载
            if (this._assetBundleRead == null)
            {
                if (null != _textAssetOrigin)
                {
                    retValue = LoadFromOriginalFormatFile(type);
                }
            }
            //从AssetBundle加载资源
            else
            {
                if (!CheckAssetBundleValid(fileName))
                    return retValue;

                if (!string.IsNullOrEmpty(fileName))
                {
                    shaco.GameHelper.atlas.StartCheckLoadAtlas();
                    retValue = LoadAssetEx(fileName, type, true);
                    shaco.GameHelper.atlas.EndCheckLoadAtlas(type);
                    if (null != retValue)
                    {
                        UnityHelper.ResetShader(retValue);
                    }
                }
            }
            return retValue;
        }

        public T Read<T>(string fileName) where T : UnityEngine.Object
        {
            return (T)Read(fileName, typeof(T));
        }

        public Object[] ReadAll()
        {
            return ReadAll(typeof(UnityEngine.Object));
        }

        public Object[] ReadAll(System.Type type)
        {
            if (this._assetBundleRead == null)
            {
                if (null != _textAssetOrigin)
                {
                    return new Object[] { LoadFromOriginalFormatFile(type) };
                }
                else
                {
                    Log.Error("HotUpdate ReadAll error: no resource be loaded");
                    return new Object[0];
                }
            }

            //read all from one assetbundle
            var ret = _assetBundleRead.LoadAllAssets(type);
            if (ret.Length == 0)
            {
                Log.Info("ReadAll error: type=" + type);
            }

            return ret;
        }

        public T[] ReadAll<T>() where T : UnityEngine.Object
        {
            return ReadAll(typeof(T)).Convert(v => (T)v);
        }

        // /// <summary>
        // /// 读取一个字符串，必须为纯文本对象
        // /// </summary>
        // /// <param name="fileName">File name.</param> assetbundle中的资源名字
        // public string ReadString(string fileName)
        // {
        //     return HotUpdateHelper.AssetToString(Read(fileName));
        // }

        // /// <summary>
        // /// 读取文件字节信息
        // /// </summary>
        // /// <param name="fileName">File name.</param>
        // public byte[] ReadByte(string fileName)
        // {
        //     var readObjTmp = Read(fileName);
        //     return HotUpdateHelper.AssetToByte(readObjTmp);
        // }


        /// <summary>
        /// 读取主资源对象
        /// </summary>
        /// <returns>The main asset.</returns> assetbundle中的资源名字
        public Object ReadMainAsset()
        {
            var bundleTmp = this._assetBundleRead;
            if (bundleTmp == null)
            {
                if (null != _textAssetOrigin)
                    return _textAssetOrigin;
                else
                {
                    Log.Error("HotUpdate ReadMainAsset error: no resource be loaded");
                    return null;
                }
            }

#if UNITY_5_3_OR_NEWER
            var fileName = shaco.Base.FileHelper.GetLastFileName(bundleTmp.name);
            fileName = shaco.Base.FileHelper.RemoveLastExtension(fileName);
            return bundleTmp.LoadAsset(fileName);
#else
            return bundleTmp.mainAsset;
#endif
        }

        /// <summary>
        /// 判断资源是否有效
        /// </summary>
        public bool IsValidAsset()
        {
            if (HasError())
                return false;
            else
                return this._assetBundleRead != null || (this._assetBundleRead == null && null != _textAssetOrigin);
        }

        // /// <summary>
        // /// 异步读取unity对象
        // /// </summary>
        // /// <param name="fileName">File name.</param> assetbundle中的资源名字
        // public void ReadAsync(string fileName, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress)
        // {
        //     ReadAsync(fileName, typeof(UnityEngine.Object), callbackReadEnd, callbackProgress);
        // }

        public void ReadAsync(string fileName, System.Type type, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress)
        {
            if (null == _assetBundleRead)
            {
                if (__isClosed)
                {
                    OnUserCancelLoad("HotUpdateImport ReadAsync: user canel, file name=" + fileName, null);
                    return;
                }

                if (null != _textAssetOrigin)
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(LoadFromOriginalFormatFile(type));
                }
                else
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(null);
                }
            }
            else
            {
                if (!CheckAssetBundleValid(fileName))
                    return;

                try
                {
                    shaco.GameHelper.atlas.StartCheckLoadAtlasAysnc();
                    _isLoading = true;
                    LoadAssetAsyncEx(fileName, type, (loadObj) =>
                    {
                        _isLoading = false;

                        //当ab包已经被关闭了不再回调
                        if (__isClosed)
                        {
                            return;
                        }

                        shaco.GameHelper.atlas.EndCheckLoadAtlasAsync(type, () =>
                        {
                            //当ab包已经被关闭了不再回调
                            if (__isClosed)
                            {
                                shaco.Log.Info("HotUpdateImport ReadAsync: user canel, fileName=" + fileName + " type=" + type.ToTypeString());
                                return;
                            }

                            if (null != loadObj)
                            {
                                UnityHelper.ResetShader(loadObj);
                            }

                            if (null != callbackProgress)
                                callbackProgress(1.0f);

                            if (null != callbackReadEnd)
                                callbackReadEnd(loadObj);
                        });
                    }, (percent) =>
                    {
                        if (null != callbackProgress)
                        {
                            if (percent >= 1.0f)
                                percent = 0.9f;
                            callbackProgress(percent);
                        }
                    }, true);
                }
                catch (System.Exception e)
                {
                    shaco.Log.Error("HotUpdateImport ReadAsync exception: e=" + e);
                }
            }
        }

        // public void ReadAsync<T>(string fileName, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress) where T : UnityEngine.Object
        // {
        //     ReadAsync(fileName, typeof(T), callbackReadEnd, callbackProgress);
        // }

        // public void ReadAllAsync(System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress)
        // {
        //     ReadAllAsync(typeof(UnityEngine.Object), callbackReadEnd, callbackProgress);
        // }

        public void ReadAllAsync(System.Type type, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress)
        {
#if UNITY_5_3_OR_NEWER
            if (null == _assetBundleRead)
            {
                if (__isClosed)
                {
                    shaco.Log.Info("HotUpdateImport ReadAllAsync: user canel, type=" + type.ToTypeString());
                    return;
                }

                if (null != _textAssetOrigin)
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(new Object[] { LoadFromOriginalFormatFile(type) });
                }
                else
                {
                    if (null != callbackProgress)
                        callbackProgress(1);
                    callbackReadEnd(null);
                }
            }
            else
            {
                var requsetAsync = this._assetBundleRead.LoadAllAssetsAsync(type);

                _isLoading = true;
                shaco.GameHelper.atlas.StartCheckLoadAtlasAysnc();
                shaco.Base.WaitFor.Run((System.Func<bool>)(() =>
                {
                    if (null != callbackProgress)
                        callbackProgress(requsetAsync.progress);
                    return requsetAsync.isDone;
                }),
                (System.Action)(() =>
                {
                    shaco.GameHelper.atlas.EndCheckLoadAtlasAsync(type, () =>
                    {
                        _isLoading = false;
                        if (this.__isClosed)
                        {
                            shaco.Log.Info("HotUpdateImport LoadAssetAsyncEx: user canel, assetbundle name=" + _assetBundleRead.name + " type=" + type.ToTypeString());
                            return;
                        }

                        if (requsetAsync.allAssets.Length == 0)
                            Log.Error("HotUpdate ReadAllAsync error: type=" + type);
                        else
                            callbackReadEnd(requsetAsync.allAssets);
                    });
                }));
            }
#else 
            callbackReadEnd(null);
            shaco.Log.Warning("HotUpdateImport ReadAllAsync error: this function only support on Unity5.3 or upper !");
#endif
        }

        // public void ReadAllAsync<T>(System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress) where T : UnityEngine.Object
        // {
        //     ReadAllAsync(typeof(T), callbackReadEnd, callbackProgress);
        // }

        // /// <summary>
        // /// 异步读取一个字符串，必须为纯文本对象
        // /// </summary>
        // /// <param name="fileName">File name.</param> assetbundle中的资源名字
        // public void ReadStringAsync(string fileName, HotUpdateDefine.CALL_FUNC_READ_STRING callbackReadEnd, System.Action<float> callbackProgress)
        // {
        //     ReadAsync(fileName, (UnityEngine.Object obj) =>
        //     {
        //         callbackReadEnd(HotUpdateHelper.AssetToString(obj));
        //     }, callbackProgress);
        // }

        // /// <summary>
        // /// 异步读取文件字节信息
        // /// </summary>
        // /// <param name="fileName">File name.</param>
        // public void ReadByteAsync(string fileName, HotUpdateDefine.CALL_FUNC_READ_BYTE callbackReadEnd, System.Action<float> callbackProgress)
        // {
        //     ReadAsync(fileName, (UnityEngine.Object obj) =>
        //     {
        //         callbackReadEnd(HotUpdateHelper.AssetToByte(obj));
        //     }, callbackProgress);
        // }

        /// <summary>
        /// 打印所有资源名字
        /// </summary>
        /// <param name="assetBundle">Asset bundle.</param>
        public void PrintAllAsset()
        {
            if (null == _assetBundleRead)
            {
                if (null != _textAssetOrigin)
                {
                    if (!string.IsNullOrEmpty(_textAssetOrigin.text))
                    {
                        Debug.Log("original file text=" + _textAssetOrigin.text);
                    }
                    else
                    {
                        Debug.Log("original file data count=" + _textAssetOrigin.bytes.Length);
                    }
                }
                else
                {
                    Debug.Log("no data");
                }
                return;
            }

#if UNITY_5_3_OR_NEWER

            var allPaths = _assetBundleRead.GetAllAssetNames();
            var allAssets = _assetBundleRead.LoadAllAssets();

            Log.Info("PrintAllAsset count=" + allPaths.Length);

            for (int i = 0; i < allPaths.Length; ++i)
            {
                Log.Info("PrintAllAsset path=" + allPaths[i] + " type=" + allAssets[i]);
            }
#else
            Log.Info("main asset=" + _assetBundleRead.mainAsset);
#endif
        }

        /// <summary>
        /// 获取最近一次错误信息
        /// </summary>
        virtual public string GetLastError()
        {
            return _strLastError;
        }

        // /// <summary>
        // /// 获取只读文件夹路径 + 文件名字
        // /// </summary>
        // /// <param name="fileName">File name.</param> 文件名字
        // public string GetReadOnlyFullPath(string fileName)
        // {
        //     return shaco.Base.FileHelper.ContactPath(Application.streamingAssetsPath, fileName);
        // }

        //是否发生过错误
        virtual public bool HasError()
        {
            return !string.IsNullOrEmpty(GetLastError());
        }

        /// <summary>
        /// 读取场景名字
        /// <param name="sceneName">场景路径，例如Assets/Scenes/Demo.unity</param>
        /// <return>如果获取场景失败则返回空字符串</return>
        /// </summary>
        protected string ReadScene(string sceneName)
        {
#if UNITY_5_3_OR_NEWER
            if (null == _assetBundleRead)
            {
                Log.Error("HotUpdate ReadScene error: invablid assetbundle");
                return string.Empty;
            }
            var scenePath = _assetBundleRead.GetAllScenePaths();

            sceneName = sceneName.AddBehindNotContains(".unity");

            var findSceneName = string.Empty;
            for (int i = scenePath.Length - 1; i >= 0; --i)
            {
                if (scenePath[i].ToLower() == sceneName)
                {
                    findSceneName = scenePath[i];
                    break;
                }
            }

            if (string.IsNullOrEmpty(findSceneName))
            {
                Log.Error("HotUpdate ReadScene error: not found scene=" + findSceneName);
                return string.Empty;
            }
            return findSceneName;
#else
            Log.Error("HotUpdate ReadScene error: only support on Unity 5.3 or upper");
            return string.Empty;
#endif

        }

        protected bool CheckAssetBundleValid(string fileName)
        {
            if (this._assetBundleRead == null)
            {
                OnError("HotUpdate GetReadFileName error: no resource be loaded fileName=" + fileName);
                return false;
            }
            return true;
        }

        protected Object CheckLoadValidWhenAssetIsNull(Object asset, string fileName, System.Type type)
        {
            if (null != asset)
                return asset;

            //sprite to texture
            if ((type == typeof(UnityEngine.Texture2D) || type == typeof(UnityEngine.Texture)) && type != typeof(UnityEngine.Sprite))
            {
                var readSpr = LoadAssetEx(fileName, typeof(UnityEngine.Sprite), false) as Sprite;
                if (null != readSpr)
                {
                    asset = readSpr.texture;
                }

                Log.Error("HotUpdate read error: can't read asset with type=" + type + " we fix it from sprite to texture");
            }

            //texture to sprite
            if (type == typeof(UnityEngine.Sprite) && (type != typeof(UnityEngine.Texture2D) && type != typeof(UnityEngine.Texture)))
            {
                var texTmp = LoadAssetEx(fileName, typeof(UnityEngine.Texture2D), false) as Texture2D;

                if (null != texTmp)
                {
                    asset = Sprite.Create(texTmp, new Rect(0, 0, texTmp.width, texTmp.height), Vector2.zero);
                }

                Log.Error("HotUpdate read error: can't read asset with type=" + type + " we fix it from texture to sprite");
            }

            return asset;
        }

        protected void InvokeCallBack(System.Action callback)
        {
            if (callback != null)
            {
                callback();
            }
        }

        protected Object LoadAssetEx(string filename, System.Type type, bool isAutoCheckTypeConvert)
        {
#if UNITY_5_3_OR_NEWER
            //加载场景
            if (this._assetBundleRead.isStreamedSceneAssetBundle)
            {
                var sceneName = ReadScene(filename);
                if (!string.IsNullOrEmpty(sceneName))
                {
                    shaco.SceneManager.LoadScene(sceneName);
                }
                _textAssetOrigin.bytes = sceneName.ToByteArray();
                return _textAssetOrigin;
            }
            //加载资源
            else
#endif
            {
                Object retValue = GetAssetFromLoadedAssets(filename, type);
                if (null != retValue)
                {
                    return retValue;
                }

                //删除文件夹路径尝试加载
                var lastFileName = shaco.Base.FileHelper.GetLastFileName(filename);
                retValue = this._assetBundleRead.LoadAsset(lastFileName, type);

                //当文件名无法加载的时候，尝试使用全名字进行加载
                if (null == retValue)
                {
                    retValue = this._assetBundleRead.LoadAsset(filename, type);
                }

                if (null == retValue)
                {
                    if (shaco.Base.FileHelper.HasFileNameExtension(filename))
                    {
                        //删除后缀名和路径尝试加载
                        if (null == retValue)
                        {
                            retValue = this._assetBundleRead.LoadAsset(shaco.Base.FileHelper.RemoveLastExtension(lastFileName), type);
                        }
                    }
                }

                if (null != retValue)
                    SaveToLoadedAssets(filename, retValue);
                return retValue;
            }
        }

        protected void LoadAssetAsyncEx(string filename, System.Type type, System.Action<UnityEngine.Object> callbackEnd, System.Action<float> callbackProgress, bool isAutoCheckTypeConvert)
        {
            if (null == _assetBundleRead)
            {
                Log.Error("HotUpdateImport LoadAssetAsyncEx error: assetbundle is null");
                return;
            }

            // if (this.__isClosed)
            // {
            //     OnUserCancelLoad("HotUpdateImport LoadAssetAsyncEx 1: user canel, file name=" + filename, callbackEnd);
            //     return;
            // }

#if UNITY_5_3_OR_NEWER
            //加载场景
            if (this._assetBundleRead.isStreamedSceneAssetBundle)
            {
                var sceneName = ReadScene(filename);
                AsyncOperation asyncOperation = null;
                asyncOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
                shaco.Base.WaitFor.Run((System.Func<bool>)(() =>
                {
                    if (null != callbackProgress)
                        callbackProgress(asyncOperation.progress);
                    return asyncOperation.isDone;
                }), (System.Action)(() =>
                {
                    //场景的话只能返回名字了
                    if (this.__isClosed)
                    {
                        OnUserCancelLoad("HotUpdateImport LoadAssetAsyncEx 2: user canel, scene name=" + filename, callbackEnd);
                        return;
                    }

                    if (null != callbackEnd)
                    {
                        callbackEnd(new TextAsset(sceneName));
                    }
                }));
            }
            else
#endif
            {
                Object loadObj = GetAssetFromLoadedAssets(filename, type);
                if (null != loadObj && null != callbackEnd)
                {
                    callbackEnd(loadObj);
                    return;
                }

                var tmpAssetbundleRead = this._assetBundleRead;
                var lastFileName = shaco.Base.FileHelper.GetLastFileName(filename);
                var requsetAsync = this._assetBundleRead.LoadAssetAsync(lastFileName, type);
                shaco.Base.WaitFor.Run((System.Func<bool>)(() =>
                {
                    if (null != callbackProgress)
                        callbackProgress(requsetAsync.progress);
                    return requsetAsync.isDone;
                }), (System.Action)(() =>
                {
                    if (this.__isClosed)
                    {
                        OnUserCancelLoad("HotUpdateImport LoadAssetAsyncEx 3: user canel, file name=" + filename, callbackEnd);
                        return;
                    }

                    if (null == requsetAsync.asset)
                    {
                        //使用原路径尝试加载
                        requsetAsync = this._assetBundleRead.LoadAssetAsync(filename, type);
                        shaco.Base.WaitFor.Run(() =>
                        {
                            return requsetAsync.isDone;
                        }, () =>
                        {
                            if (this.__isClosed)
                            {
                                OnUserCancelLoad("HotUpdateImport LoadAssetAsyncEx 4: user canel, file name=" + filename, callbackEnd);
                                return;
                            }

                            //删除后缀名和路径尝试加载
                            if (null == requsetAsync.asset)
                            {
                                //如果还有后缀名，则尝试从后缀名再加载一次，否则直接返回了
                                if (!shaco.Base.FileHelper.HasFileNameExtension(filename))
                                {
                                    if (null != callbackEnd)
                                    {
                                        SaveToLoadedAssets(filename, requsetAsync.asset);
                                        callbackEnd(requsetAsync.asset);
                                    }
                                }
                                else
                                {
                                    requsetAsync = this._assetBundleRead.LoadAssetAsync(shaco.Base.FileHelper.RemoveLastExtension(lastFileName), type);
                                    shaco.Base.WaitFor.Run(() =>
                                    {
                                        return requsetAsync.isDone;
                                    }, () =>
                                    {
                                        if (this.__isClosed)
                                        {
                                            OnUserCancelLoad("HotUpdateImport LoadAssetAsyncEx 5: user canel, file name=" + filename, callbackEnd);
                                            return;
                                        }

                                        if (null != callbackEnd)
                                        {
                                            SaveToLoadedAssets(filename, requsetAsync.asset);
                                            callbackEnd(requsetAsync.asset);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                if (null != callbackEnd)
                                {
                                    SaveToLoadedAssets(filename, requsetAsync.asset);
                                    callbackEnd(requsetAsync.asset);
                                }
                            }
                        });
                    }
                    else if (null != callbackEnd)
                    {
                        SaveToLoadedAssets(filename, requsetAsync.asset);
                        callbackEnd(requsetAsync.asset);
                    }
                }));
            }
        }

        protected Object GetAssetFromLoadedAssets(string filename, System.Type type)
        {
            Object retValue = null;
            if (_loadedAllAssets.Count > 0)
            {
                Object findTmp = null;
                if (_loadedAllAssets.TryGetValue(filename, out findTmp))
                {
                    //必须是指定类型
                    retValue = findTmp.GetType().IsInherited(type) ? findTmp : null;
                }
            }
            return retValue;
        }

        protected void SaveToLoadedAssets(string filename, UnityEngine.Object asset)
        {
            if (null == asset)
                return;

            Object findValue = null;
            if (!_loadedAllAssets.TryGetValue(filename, out findValue))
            {
                _loadedAllAssets.Add(filename, asset);
            }
            else
            {
                //当读取同一文件名类型却不一致的时候刷新已储存的asset
                //因为_loadedAllAssets本身只是为了加快异步加载速度而缓存的数据，所以只保留最后一次加载的内容
                if (findValue.GetType() != asset.GetType())
                    _loadedAllAssets[filename] = asset;
            }
        }

        /// <summary>
        /// 关闭并释放资源，建议在assetbundle使用完毕后调用，否则再次加载相同assetbundle的时候会报错
        /// </summary>
        virtual public void Close(bool unloadAllLoadedObjects = false)
        {
            if (!__isClosed)
            {
                __isClosed = true;
                if (_isLoading)
                {
                    _unLoadingStatus = UnLoadingStatus.UnLoading;

                    //等待加载完毕后再卸载，因为assetbundle没有提供中断request的方法，所以会导致内存泄漏
                    shaco.Base.WaitFor.Run(() =>
                    {
                        return !_isLoading;
                    }, () =>
                    {
                        //用户中断了卸载(可能是重新加载了该assetbundle)
                        if (_unLoadingStatus != UnLoadingStatus.InterruptUnload)
                            CloseBase(unloadAllLoadedObjects);
                        else
                        {
                            Log.Info("HotUpdateImport Close: InterruptUnload, name=" + (null == _assetBundleRead ? "null" : _assetBundleRead.name));
                            _unLoadingStatus = UnLoadingStatus.None;
                        }
                    });
                }
                else
                {
                    CloseBase(unloadAllLoadedObjects);
                }
            }
            else
            {
                shaco.Log.Warning("HotUpdateImport Close warning: dont need close again");
            }
        }

        virtual public void StopWorking()
        {
            _isWorking = false;
        }

        virtual public bool IsSuccess()
        {
            return _fCurrentProgress >= 1.0f;
        }

        /// <summary>
        /// 判断还在释放内存中
        /// </summary>
        public bool IsUnLoading()
        {
            return _unLoadingStatus == UnLoadingStatus.UnLoading;
        }

        //获取ab包名字
        public string GetAssetBundleName()
        {
            return null == _assetBundleRead ? "null" : _assetBundleRead.name;
        }

        /// <summary>
        /// 检查在卸载中的ab包并中断卸载
        /// </summary>
        public bool CheckInterruptUnload()
        {
            if (_unLoadingStatus == UnLoadingStatus.UnLoading)
            {
                _unLoadingStatus = UnLoadingStatus.InterruptUnload;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 重制网络参数，一般在开始连接时候调用
        /// </summary>
        virtual protected void ResetParam()
        {
            _isWorking = false;
            _strLastError = string.Empty;
            _fCurrentProgress = 0;
            __isClosed = false;
        }

        virtual protected void OnError(string error)
        {
            CheckCompleted(error, false);
        }

        virtual protected void OnCompleted()
        {
            CheckCompleted(GetLastError(), true);
        }

        virtual protected void CheckCompleted(string error, bool isInvokeEndCallFunc)
        {
            SetLastError(error);

            if (isInvokeEndCallFunc)
            {
                if (HasError())
                {
                    Log.Warning("HotUpdate " + error);
                }
            }
        }

        protected void ClearLastError()
        {
            _strLastError = string.Empty;
        }

        private void SetLastError(string errorMsg)
        {
            if (string.IsNullOrEmpty(errorMsg) && HasError())
            {
                Log.Error("HotUpdate SetLastError error: can't set error message with empty string when has error, old error msg=" + GetLastError());
                return;
            }

            _strLastError = errorMsg;
        }

        /// <summary>
        /// 从原始资源中加unity资源
        /// <param name="type">加载文件类型</param>
        /// <return>unity资源</return>
        /// </summary>
        private Object LoadFromOriginalFormatFile(System.Type type)
        {
            Object retValue = null;

            //异步加载过程中用户取消加载的标记直接返回
            if (null == _textAssetOrigin)
                return _textAssetOrigin;

            if (typeof(TextAsset) == type)
                shaco.Log.Error("Please use 'TextOrigin' or 'Object.ToString()' instead of");
            else if (typeof(Texture2D) == type || typeof(Texture) == type)
            {
                var tex2D = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                tex2D.LoadImage(_textAssetOrigin.bytes);
                retValue = tex2D;
            }
            else if (typeof(Sprite) == type)
            {
                var tex2D = new Texture2D(0, 0, TextureFormat.RGBA32, false);
                tex2D.LoadImage(_textAssetOrigin.bytes);
                if (null != tex2D)
                {
                    var sprite = Sprite.Create(tex2D, new Rect(0, 0, tex2D.width, tex2D.height), new Vector2(0.5f, 0.5f));
                    retValue = sprite;
                }
            }
            else if (typeof(TextOrigin) == type || typeof(Object) == type)
            {
                retValue = _textAssetOrigin;
            }
            else
            {
                Log.Error("HotUpdateImport LoadFromOriginalFormatFile error: unsupport type=" + type);
            }

            return retValue;
        }

        //当在异步加载时候强制Close取消加载
        private void OnUserCancelLoad(string message, System.Action<Object> callbackEnd)
        {
            shaco.Log.Info(message);

            if (null != callbackEnd)
                callbackEnd(null);
        }

        private void CloseBase(bool unloadAllLoadedObjects)
        {
            _unLoadingStatus = UnLoadingStatus.None;
            _isLoading = false;

            if (null != _assetBundleRead)
            {
                _assetBundleRead.Unload(unloadAllLoadedObjects);
                _assetBundleRead = null;
            }

            this.Dispose();
            __isClosed = true;

            if (_isWorking)
            {
                OnError(GetLastError());
                StopWorking();
            }
        }

        // private UnityEngine.Object LoadAsAtlasPackage(string filename, System.Type type, bool isAutoCheckTypeConvert)
        // {
        //     //delete by shaco 2020/03/20
        //     //不再检测图集加载的方式了
        //     return null;
        //     //delete end

        //过滤内部资源标记
        //             if (!filename.Contains(shaco.HotUpdateDefine.INTERNAL_RESOURCE_PATH_TAG))
        //             {
        //                 return null;
        //             }

        //             UnityEngine.Object retValue = null;
        //             var filenameTmp = shaco.Base.FileHelper.GetLastFileName(filename);
        //             filenameTmp = shaco.Base.FileHelper.RemoveAllExtentsion(filenameTmp);
        //             var loadObjs = this._assetBundleRead.LoadAllAssets();

        //             SaveToLoadedAssets(loadObjs);

        //             List<UnityEngine.Object> findValue = null;
        //             if (_loadedAllAssets.TryGetValue(filenameTmp, out findValue))
        //             {
        //                 retValue = findValue.Find(v => v.GetType().IsInherited(type));
        //                 if (null != retValue)
        //                 {
        //                     //检查资源类型自动转换
        //                     if (isAutoCheckTypeConvert)
        //                     {
        //                         retValue = CheckLoadValidWhenAssetIsNull(retValue, filename, type);
        //                     }
        //                     return retValue;
        //                 }
        //             }

        // #if DEBUG_LOG
        //             //图集加载失败，如果存在同名的则提示可能是类型使用错误导致
        //             var findKeyTmp = _loadedAllAssets.ToKeyList().Find((v => v == filenameTmp));
        //             if (null != findKeyTmp)
        //             {
        //                 Log.Error("HotUpdateImport LoadAsAtlasPackage error: has same asset name=" + filenameTmp + ", but need type=" + findKeyTmp + " request type=" + type);
        //             }
        // #endif

        //             //按图集加载失败后，清理缓存资源
        //             _loadedAllAssets.Clear();
        //             return retValue;
        // }
    }
}