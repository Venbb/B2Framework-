using UnityEngine;
using System.Collections;
using System.IO;

namespace shaco
{
    public class HotUpdateImportMemory : HotUpdateImport
    {
        //异步读取超时时间
        public float ReadTimeoutSeconds = 1.0f;

        private float _fReadEplaseTime = 0;

        //callbacks
        public shaco.Base.EventCallBack onLoadingCallBack = new shaco.Base.EventCallBack();
        public shaco.Base.EventCallBack onLoadEndCallBack = new shaco.Base.EventCallBack();

        /// <summary>
        /// 从本地储存中异步创建资源，该路径可读可写
        /// </summary>
        public void CreateByMemoryAsyncAutoPlatform(string fileName, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                OnCompleted();
                return;
            }

            HotUpdateManifest.CheckDependenciesAsync(multiVersionControlRelativePath, autoCheckDepencies, fileName, (result) =>
            {
                if (!result)
                {
                    OnError("HotUpdateImportMemory CreateByMemoryAsyncAutoPlatform error: check dependencies, path=" + fileName);
                    OnCompleted();
                }
                else
                {
                    var fullPath = HotUpdateHelper.GetAssetBundleFullPath(fileName, multiVersionControlRelativePath);
                    Create(fullPath, HotUpdateDefine.ResourceCreateMode.MemoryAsync, multiVersionControlRelativePath);
                }
            });
        }

        /// <summary>
        /// 从本地储存中创建资源，该路径可读可写
        /// 该方法仅支持unity5.x以上版本(因为unity4.x以下无法直接解析未压缩的assetbundle)
        /// </summary>
        public void CreateByMemoryAutoPlatform(string fileName, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                OnCompleted();
                return;
            }
            
            if (!HotUpdateManifest.CheckDependencies(fileName, multiVersionControlRelativePath, autoCheckDepencies))
            {
                OnError("HotUpdateImportMemory CreateByMemoryAutoPlatform error: check dependencies, path=" + fileName);
                OnCompleted();
                return;
            }

            var fullPath = HotUpdateHelper.GetAssetBundleFullPath(fileName, multiVersionControlRelativePath);
            Create(fullPath, HotUpdateDefine.ResourceCreateMode.Memory, multiVersionControlRelativePath);
        }

        /// <summary>
        /// 从用户自定义路径中异步加载assetbundle
        /// <param name="path">路径</param>
        /// </summary>
        public void CreateByMemoryAsyncByUserPath(string path)
        {
            var pathCheck = GetAssetbundlePath(path);
            if (string.IsNullOrEmpty(pathCheck))
            {
                OnCompleted();
                return;
            }
            Create(pathCheck, HotUpdateDefine.ResourceCreateMode.MemoryAsync, string.Empty);
        }

        /// <summary>
        /// 从用户自定义路径中加载assetbundle
        /// <param name=""> </param>
        /// <return></return>
        /// </summary>
        public void CreateByMemoryByUserPath(string path)
        {
            var pathCheck = GetAssetbundlePath(path);
            if (string.IsNullOrEmpty(pathCheck))
            {
                return;
            }
            Create(pathCheck, HotUpdateDefine.ResourceCreateMode.Memory, string.Empty);
        }

        /// <summary>
        /// 保存二进制数据到本地储存中
        /// </summary>
        /// <param name="fileName">File name.</param>
        /// <param name="byteData">Byte data.</param>
        public void SaveDataToStorage(string fileName, byte[] byteData)
        {
            var fullPath = HotUpdateHelper.GetAssetBundleFullPath(fileName, _multiVersionControlRelativePath);
            shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(fullPath);
            shaco.Base.FileHelper.WriteAllByteByUserPath(fullPath, byteData);

            Log.Info("SaveDataToStorage success, path=" + fullPath);
        }

        /// <summary>
        /// 获取加载进度，范围(0 ~ 1)
        /// </summary>
        public float GetLoadProgress()
        {
            return _fCurrentProgress;
        }

        /// <summary>
        /// 获取可读文件夹路径
        /// </summary>
        protected string GetReadOnlyFolder()
        {
            //不同平台下StreamingAssets的路径是不同的，这里需要注意一下。  
            string ret =
#if UNITY_ANDROID && !UNITY_EDITOR
                "jar:file://" + Application.dataPath + "!/assets/";  
#elif UNITY_IPHONE && !UNITY_EDITOR
            Application.dataPath + "/Raw/";  
#elif UNITY_STANDALONE_WIN || UNITY_EDITOR
            "file://" + Application.dataPath + "/StreamingAssets/";
#else
            Application.streamingAssetsPath;
#endif

            return ret;
        }

        /// <summary>
        /// 从本地文件中读取assetbundle
        /// </summary>
        /// <returns>返回加载assetbundle请求</returns>
        /// <param name="path">assetbundle路径</param> 
        /// <param name="callbackEnd">获取完毕assetbundle请求文件回调</param>
        protected void LoadResourceByMemoryAsync(string path, System.Action<AssetBundleCreateRequest> callbackEnd)
        {
            if (!Application.isPlaying)
            {
                OnErrorAndPrint("HotUpdate LoadResourceByMemory error: this function only use on playing mode");
                return;
            }

            //从内存中读取则可以使用中文路径了
            byte[] readBytesTmp = null;
            bool isCompleted = false;
            shaco.Base.EncryptDecrypt.DecryptAsyncPath(path, (bytes) =>
            {
                readBytesTmp = bytes;
                isCompleted = true;
            });

            shaco.Base.WaitFor.Run(() =>
            {
                return !this._isClosed && isCompleted;
            }, () =>
            {
                if (this._isClosed)
                {
                    shaco.Log.Info("HotUpdateImportMemory LoadResourceByMemoryAsync: user cancel, path=" + path);
                    return;
                }

                //从内存中创建资源
                AssetBundleCreateRequest assetBundleRequst = null;

                if (!readBytesTmp.IsNullOrEmpty())
                {
                    var loadResourceData = readBytesTmp;
                    if (loadResourceData != null)
                    {
                        try
                        {
#if UNITY_5_3_OR_NEWER
                            assetBundleRequst = AssetBundle.LoadFromMemoryAsync(loadResourceData);
#else
                            assetBundleRequst = AssetBundle.CreateFromMemory(loadResourceData);
#endif
                        }
                        catch (System.Exception e)
                        {
                            shaco.Log.Error("HotUpdateImportMemory LoadResourceByMemoryAsync erorr: path=" + path + " exception=" + e);
                        }
                    }
                }

                if (null != callbackEnd)
                {
                    callbackEnd(assetBundleRequst);
                }
            });
        }

        protected void LoadResourceByMemory(string path)
        {
            if (this._isClosed)
            {
                shaco.Log.Info("HotUpdateImportMemory LoadResourceByMemory: user cancel, path=" + path);
                return;
            }

            //判断文件是否存在
            if (!shaco.Base.FileHelper.ExistsFile(path))
            {
                OnError("HotUpdate LoadResourceByMemory error: not found, path=" + path);
                OnCompleted();
                return;
            }

            bool isOriginalTextFile = HotUpdateHelper.IsKeepOriginalFile(path);
            _fCurrentProgress = 0;

            if (!isOriginalTextFile)
            {
                var loadResourceData = shaco.Base.EncryptDecrypt.DecryptPath(path);

                if (!loadResourceData.IsNullOrEmpty())
                {
                    try
                    {
#if UNITY_5_3_OR_NEWER
                        _assetBundleRead = AssetBundle.LoadFromMemory(loadResourceData);
#else
                        _assetBundleRead = AssetBundle.CreateFromMemoryImmediate(loadResourceData);
#endif
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("HotUpdateImportMemory LoadResourceByMemory erorr: path=" + path + " exception=" + e);
                        return;
                    }
                }
                else
                {
                    shaco.Log.Error("HotUpdateImportMemory LoadResourceByMemory error: empty data path=" + path);
                }
            }

            _fCurrentProgress = 1.0f;
            CheckAssetbundleValid(path, isOriginalTextFile);
        }

        protected IEnumerator LoadResourceByUserPathAsync(string path, System.Action callbackLoadEnd, float dynamicDownloadFixedPercent)
        {
            //判断文件是否存在
            if (!shaco.Base.FileHelper.ExistsFile(path))
            {
                OnError("HotUpdate LoadResourceByUserPathAsync error: not found, path=" + path);
                if (null != callbackLoadEnd)
                {
                    callbackLoadEnd();
                }
                yield break;
            }
            
            bool isOriginalTextFile = HotUpdateHelper.IsKeepOriginalFile(path);
            AssetBundleCreateRequest assetBundleRequst = null;

            if (!isOriginalTextFile)
            {
                bool loadCompleted = false;
                LoadResourceByMemoryAsync(path, (AssetBundleCreateRequest request) =>
                {
                    assetBundleRequst = request;
                    loadCompleted = true;
                });

                //等待assetbundle请求文件加载完毕
                while (!loadCompleted)
                {
                    yield return null;
                }

                if (null != assetBundleRequst)
                {
                    //开始加载assetbundle
                    while (!this._isClosed && !assetBundleRequst.isDone)
                    {
                        _fCurrentProgress = assetBundleRequst.progress * dynamicDownloadFixedPercent + dynamicDownloadFixedPercent;

                        if (_fCurrentProgress == 0)
                        {
                            _fReadEplaseTime += Time.fixedDeltaTime;
                            if (_fReadEplaseTime >= ReadTimeoutSeconds)
                            {
                                break;
                            }
                        }

                        if (assetBundleRequst.progress < 1.0f)
                        {
                            if (onLoadingCallBack != null)
                            {
                                onLoadingCallBack.InvokeAllCallBack();
                            }
                        }
                        yield return null;
                    }

                    if (assetBundleRequst.assetBundle != null)
                    {
                        this._assetBundleRead = assetBundleRequst.assetBundle;
                    }
                }
            }

            if (this._isClosed)
            {
                _textAssetOrigin = null;

                if (null != assetBundleRequst && assetBundleRequst.assetBundle != null)
                {
                    assetBundleRequst.assetBundle.Unload(true);
                    this._assetBundleRead = null;
                }
                Log.Info("HotUpdate LoadResourceByUserPathAsync: user cancel, path=" + path);
                yield break;
            }

            CheckAssetbundleValid(path, isOriginalTextFile);
            _fCurrentProgress = 1.0f;

            //没读取到资源
            if (null == _textAssetOrigin && null == _assetBundleRead)
                OnError("HotUpdate LoadResourceByUserPathAsync error: can't load assetbundle, path=" + path);

            if (null != callbackLoadEnd)
            {
                callbackLoadEnd();
            }
        }

        /// <summary>
        /// 创建assetbundle加载请求
        /// </summary>
        /// <param name="url">网址、地址或者路径</param>
        /// <param name="mode">创建assetbundle的方式</param>
        protected void Create(string url, HotUpdateDefine.ResourceCreateMode createMode, string multiVersionControlRelativePath)
        {
            if (_isWorking)
            {
                Log.Warning("downloading, please wait");
                return;
            }

            Close();
            ResetParam();
            _isWorking = true;
            _multiVersionControlRelativePath = multiVersionControlRelativePath;

            switch (createMode)
            {
                case HotUpdateDefine.ResourceCreateMode.Memory:
                    {
                        LoadResourceByMemory(url);
                        break;
                    }
                case HotUpdateDefine.ResourceCreateMode.MemoryAsync:
                    {
                        CreateWithAsync(url);
                        break;
                    }
                default: Log.Error("HotUpdateImportMemory Create error: unsupport mode=" + createMode); break;
            }
        }

        protected void CreateWithAsync(string url)
        {
            //先从本地加载资源
            shaco.GameHelper.StartCoroutine(LoadResourceByUserPathAsync(url, () =>
            {
                //如果manifest文件还没有准备好的情况下，是不能进行动态下载的
                if (!shaco.HotUpdateManifest.CheckManifestSetupReady(_multiVersionControlRelativePath))
                {
                    Log.Error("HotUpdateImportMemory CreateWithAsync error: manifest is not ready, version path=" + _multiVersionControlRelativePath);

                    //load end
                    OnCompleted();
                    return;
                }

                //本地资源加载出现错误，在线更新资源
                if (HasError())
                {
                    if (!HotUpdateHelper.GetDynamicResourceAddress().IsNullOrEmpty())
                        CreateWithAsyncDynamic(url);
                    else
                    {
                        //没有决定在线更新则设置进度为100%
                        _fCurrentProgress = 1.0f;
                        OnCompleted();
                    }
                }
                else
                {
                    //load end
                    OnCompleted();
                }
            }, 0));
        }

        /// <summary>
        /// 在线更新资源
        /// <param name="url">下载地址或本地路径</param>
        /// </summary>
        protected void CreateWithAsyncDynamic(string url)
        {
            HotUpdateHelper.ForeachDownloadAssetbundleByUrls(url, _multiVersionControlRelativePath, (string errorMessage) =>
            {
                if (this._isClosed)
                {
                    Log.Info("HotUpdate CreateWithAsyncDynamic: user cancel, url=" + url);
                    return;
                }

                if (string.IsNullOrEmpty(errorMessage))
                {
                    //restart load local resource with async
                    shaco.GameHelper.StartCoroutine(LoadResourceByUserPathAsync(url, () =>
                    {
                        //load end - success
                        ClearLastError();
                        OnCompleted();
                    }, HotUpdateDefine.DYNAMIC_DOWNLOAD_PROGRESS_PERCENT));
                }
                else
                {
                    //has some error, quickly end
                    OnError(errorMessage);
                    OnCompleted();
                }
            },
            (float progress) =>
            {
                _fCurrentProgress = progress * HotUpdateDefine.DYNAMIC_DOWNLOAD_PROGRESS_PERCENT;
                if (null != this.onLoadingCallBack)
                {
                    this.onLoadingCallBack.InvokeAllCallBack();
                }
            });
        }

        protected string GetAssetbundlePath(string path)
        {
            string retPath = string.Empty;
            if (!shaco.Base.FileHelper.ExistsFile(path))
            {
                OnError("HotUpdate load assetbundle error: missing file name=" + shaco.Base.FileHelper.GetLastFileName(path) + "\npath=" + path);
            }
            else
            {
                retPath = path;
            }

            return retPath;
        }

        /// <summary>
        /// 检查assetbundle是否正常可以使用，如果不能使用则会删除assetbundle文件，如果可以使用并且是原始文件则转换为原始文件使用
        /// </summary>
        /// <param name="path">assetbundle路径</param>
        /// <param name="isAssetBundle">该路径的文件是否为assetbundle，反之为原始文件</param>
        private void CheckAssetbundleValid(string path, bool isOriginalTextFile)
        {
            if (_assetBundleRead != null || _isClosed)
                return;
            else
            {
                if (!shaco.Base.FileHelper.ExistsFile(path))
                {
                    OnError("HotUpdate CheckAssetbundleValid: not found assetbundle path=" + path);
                }
                else
                {
                    if (isOriginalTextFile)
                    {
                        if (!HotUpdateHelper.CheckCompressedFileAndOverwrite(path))
                        {
                            return;
                        }

                        _textAssetOrigin = new TextOrigin();
                        _textAssetOrigin.bytes = shaco.Base.EncryptDecrypt.DecryptPath(path);
                    }
                    else
                    {
                        //运行时刻如果发现了读取ab包错误的情况，删除它以便重新下载
                        if (Application.isPlaying)
                        {
                            bool isEncryption = shaco.Base.EncryptDecrypt.IsEncryption(shaco.Base.FileHelper.ReadAllByteByUserPath(path));
                            OnErrorAndPrint("HotUpdate CheckAssetbundleValid: assetbundle is invalid will delete it isEncryption=" + isEncryption + "\npath=" + path);
                            shaco.Base.FileHelper.DeleteByUserPath(path);
                        }
                    }
                }
            }
        }

        private void OnErrorAndPrint(string msg)
        {
            OnError(msg);

            if (!string.IsNullOrEmpty(GetLastError()))
            {
                Log.Error(GetLastError());
            }
        }

        protected override void ResetParam()
        {
            base.ResetParam();

            _fReadEplaseTime = 0;
            _fCurrentProgress = 0;
        }

        public override bool IsSuccess()
        {
            return base.IsSuccess() && !HasError() && (null != _assetBundleRead || null != _textAssetOrigin);
        }

        protected override void OnCompleted()
        {
            base.OnCompleted();

            _fCurrentProgress = 1.0f;
            onLoadingCallBack.InvokeAllCallBack();
            onLoadEndCallBack.InvokeAllCallBack();
        }
    }
}