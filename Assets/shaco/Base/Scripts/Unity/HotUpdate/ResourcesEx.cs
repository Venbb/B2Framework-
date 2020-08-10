using System.Collections;

namespace shaco
{
    public partial class ResourcesEx : IResourcesEx
    {
        private IResources _resourcesHelper
        {
            get
            {
                if (null == ___resourcesHelper)
                {
                    switch (resourcesLoadMode)
                    {
                        case ResourcesLoadMode.EditorDevelop: ___resourcesHelper = new ResourcesEditor(); break;
                        case ResourcesLoadMode.RunTime: ___resourcesHelper = new ResourcesRuntime(); break;
                        default: shaco.Log.Error("ResourcesEx _resourcesHelper error: unsupport mode type=" + resourcesLoadMode); return new ResourcesRuntime();
                    }
                }
                return ___resourcesHelper;
            }
        }
        private IResources ___resourcesHelper = null;

        private bool _isInitedResourcesMode = false;
        private string _versionControlTag = shaco.Base.GlobalParams.EmptyString;

        private void CheckInitResourceMode()
        {
            if (_isInitedResourcesMode)
                return;
            _isInitedResourcesMode = true;

            //初始化游戏资源加载模式和顺序
            var configTmp = shaco.Base.GameHelper.gameConfig;
            this.resourcesLoadMode = configTmp.ReadEnum(this.resourcesLoadMode.ToTypeString(), ResourcesLoadMode.RunTime);
            this.resourcesLoadOrder = configTmp.ReadEnum(this.resourcesLoadOrder.ToTypeString(), ResourcesLoadOrder.DownloadFirst);
            this._versionControlTag = HotUpdateHelper.GetAssetBundlePathTagPlatform(HotUpdateHelper.GetAssetBundleAutoPlatform());

#if UNITY_2017_1_OR_NEWER
            //初始化一次图集
            var atlasManger = shaco.GameHelper.atlas;
#endif
        }

        //查看路径中是否包含了版本管理路径，如果有则拆分开来
        private void CheckMultiVersionControlPathSplit(ref string path, ref string multiVersionControlRelativePath)
        {
            CheckInitResourceMode();
            path = path.ToLower();

            if (path.Contains(_versionControlTag))
            {
                var splitPaths = path.Split(_versionControlTag);

                if (splitPaths.Length == 2)
                {
                    path = splitPaths[1];
                    multiVersionControlRelativePath = splitPaths[0];

                    if (!string.IsNullOrEmpty(path) && path[0] == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
                    {
                        path = path.RemoveFront(shaco.Base.FileDefine.PATH_FLAG_SPLIT.ToString());
                    }
                }
                else
                {
                    Log.Error("ResourcesEx CheckMultiVersionControlPathSplit error: unsupport split type path=" + path);
                }
            }
        }

        private UnityEngine.Object _LoadResourcesOrLocal(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type)
        {
            //default load from resource
            UnityEngine.Object ret = null;

            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

            switch (this.resourcesLoadOrder)
            {
                case ResourcesLoadOrder.ResourcesFirst:
                    {
                        if (null == ret)
                            ret = _LoadFromResources(path, multiVersionControlRelativePath, type);

                        if (null == ret)
                            ret = _LoadFromLocal(path, multiVersionControlRelativePath, autoCheckDepencies, type);
                        break;
                    }
                case ResourcesLoadOrder.DownloadFirst:
                    {
                        if (null == ret)
                            ret = _LoadFromLocal(path, multiVersionControlRelativePath, autoCheckDepencies, type);

                        if (null == ret)
                            ret = _LoadFromResources(path, multiVersionControlRelativePath, type);
                        break;
                    }
                default: shaco.Log.Error("ResourcesEx _LoadResourcesOrLocal error: unsupport order type=" + this.resourcesLoadOrder); break;
            }

            if (null == ret)
            {
                shaco.Log.Error("_LoadResourcesOrLocal error: missing path=" + path + " type=" + type + " muilty=" + multiVersionControlRelativePath);
            }

            return ret;
        }

        private UnityEngine.Object _LoadFromResources(string path, string multiVersionControlRelativePath, System.Type type)
        {
            UnityEngine.Object ret = null;
            ret = _resourcesHelper.Load(path, type, multiVersionControlRelativePath);
            return ret;
        }

        private UnityEngine.Object _LoadFromLocal(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type)
        {
            UnityEngine.Object ret = null;
            ret = shaco.GameHelper.resCache.Read(path, multiVersionControlRelativePath, autoCheckDepencies, path, type);
            return ret;
        }

        private UnityEngine.Object[] _LoadAllResourcesOrLocal(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type)
        {
            UnityEngine.Object[] ret = null;

            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

            switch (this.resourcesLoadOrder)
            {
                case ResourcesLoadOrder.ResourcesFirst:
                    {
                        if (ret.IsNullOrEmpty())
                            ret = _LoadAllFromResources(path, multiVersionControlRelativePath, type);

                        if (ret.IsNullOrEmpty())
                            ret = _LoadAllFromLocal(path, multiVersionControlRelativePath, autoCheckDepencies, type);
                        break;
                    }
                case ResourcesLoadOrder.DownloadFirst:
                    {
                        if (ret.IsNullOrEmpty())
                            ret = _LoadAllFromLocal(path, multiVersionControlRelativePath, autoCheckDepencies, type);

                        if (ret.IsNullOrEmpty())
                            ret = _LoadAllFromResources(path, multiVersionControlRelativePath, type);
                        break;
                    }
                default: shaco.Log.Error("ResourcesEx _LoadAllResourcesOrLocal error: unsupport order type=" + this.resourcesLoadOrder); break;
            }

            if (ret.IsNullOrEmpty())
            {
                shaco.Log.Error("_LoadAllResourcesOrLocal error: missing path=" + path + " type=" + type + " muilty=" + multiVersionControlRelativePath);
            }
            return ret;
        }

        private UnityEngine.Object[] _LoadAllFromResources(string path, string multiVersionControlRelativePath, System.Type type)
        {
            return _resourcesHelper.LoadAll(path, type, multiVersionControlRelativePath);
        }

        private UnityEngine.Object[] _LoadAllFromLocal(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type)
        {
            var convertPath = GetAssetBundlePath(path, multiVersionControlRelativePath);
            var retValue = shaco.GameHelper.resCache.ReadAll(convertPath, multiVersionControlRelativePath, autoCheckDepencies, type);
            return retValue;
        }

        private void _LoadResourcesOrLocalAsync(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type, System.Action<UnityEngine.Object> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);

            string callStackInfo = null; GetCallStackInfo(ref callStackInfo);
            switch (this.resourcesLoadOrder)
            {
                case ResourcesLoadOrder.ResourcesFirst:
                    {
                        _LoadFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object readObj) =>
                        {
                            if (null == readObj)
                            {
                                _LoadFromLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, type, (UnityEngine.Object obj) =>
                                {
                                    if (null == obj)
                                    {
                                        shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync erorr: missing path=" + path + " type=" + type + " muilty=" + multiVersionControlRelativePath + callStackInfo);
                                    }
                                    try { callbackLoadEnd(obj); }
                                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                                }, callbackProgress);
                            }
                            else
                            {
                                try { callbackLoadEnd(readObj); }
                                catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                            }
                        }, callbackProgress);
                        break;
                    }
                case ResourcesLoadOrder.DownloadFirst:
                    {
                        _LoadFromLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, type, (UnityEngine.Object readObj) =>
                        {
                            if (null == readObj)
                            {
                                _LoadFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object obj) =>
                                {
                                    if (null == obj)
                                    {
                                        shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync erorr: missing path=" + path + " type=" + type + " muilty=" + multiVersionControlRelativePath + callStackInfo);
                                    }
                                    try { callbackLoadEnd(obj); }
                                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                                }, callbackProgress);
                            }
                            else
                            {
                                try { callbackLoadEnd(readObj); }
                                catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                            }
                        }, callbackProgress);
                        break;
                    }
                default: shaco.Log.Error("ResourcesEx _LoadResourcesOrLocalAsync error: unsupport order type=" + this.resourcesLoadOrder); break;
            }
        }

        private void _LoadFromResourcesAsync(string path, string multiVersionControlRelativePath, System.Type type, System.Action<UnityEngine.Object> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            _resourcesHelper.LoadAsync(path, type, (float percent) =>
            {
                if (null != callbackProgress)
                {
                    try { callbackProgress(percent); }
                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadFromResourcesAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e); }
                }
            },
            (UnityEngine.Object loadObj) =>
            {
                callbackLoadEnd(loadObj);
            }, multiVersionControlRelativePath);
        }

        private void _LoadFromLocalAsync(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type, System.Action<UnityEngine.Object> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            //load from assetbundle
            shaco.GameHelper.resCache.ReadAsync(path, multiVersionControlRelativePath, autoCheckDepencies, path, type, (UnityEngine.Object loadObjTmp) =>
            {
                callbackLoadEnd(loadObjTmp);
            }, (percent) =>
            {
                if (null != callbackProgress)
                {
                    try { callbackProgress(percent); }
                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadFromLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e); }
                }
            });
        }

        private void _LoadAllResourcesOrLocalAsync(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            CheckMultiVersionControlPathSplit(ref path, ref multiVersionControlRelativePath);
            string callStackInfo = null; GetCallStackInfo(ref callStackInfo);

            switch (this.resourcesLoadOrder)
            {
                case ResourcesLoadOrder.ResourcesFirst:
                    {
                        shaco.GameHelper.StartCoroutine(_LoadAllFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object[] loadObjs) =>
                        {
                            if (loadObjs.IsNullOrEmpty())
                            {
                                _LoadAllFromLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, type, (UnityEngine.Object[] objs) =>
                                {
                                    if (objs.IsNullOrEmpty())
                                        shaco.Log.Error("_LoadAllResourcesOrLocalAsync error1: missing path=" + path + " type=" + type + " muilty=" + multiVersionControlRelativePath + callStackInfo);

                                    try { callbackLoadEnd(objs); }
                                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadAllResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                                }, callbackProgress);
                            }
                            else
                            {
                                try { callbackLoadEnd(loadObjs); }
                                catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadAllResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                            }
                        }, null));
                        break;
                    }
                case ResourcesLoadOrder.DownloadFirst:
                    {
                        _LoadAllFromLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, type, (UnityEngine.Object[] loadObjs) =>
                        {
                            if (loadObjs.IsNullOrEmpty())
                            {
                                shaco.GameHelper.StartCoroutine(_LoadAllFromResourcesAsync(path, multiVersionControlRelativePath, type, (UnityEngine.Object[] objs) =>
                                {
                                    if (objs.IsNullOrEmpty())
                                    {
                                        shaco.Log.Error("_LoadAllResourcesOrLocalAsync error2: missing path=" + path + " type=" + type + " muilty=" + multiVersionControlRelativePath + callStackInfo);
                                    }
                                    try { callbackLoadEnd(objs); }
                                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadAllResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                                }, null));
                            }
                            else
                            {
                                try { callbackLoadEnd(loadObjs); }
                                catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadAllResourcesOrLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e + callStackInfo); }
                            }
                        }, callbackProgress);
                        break;
                    }
                default: shaco.Log.Error("ResourcesEx _LoadAllResourcesOrLocalAsync error: unsupport order type=" + this.resourcesLoadOrder); break;
            }
        }

        private IEnumerator _LoadAllFromResourcesAsync(string path, string multiVersionControlRelativePath, System.Type type, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            var loadObjs = _resourcesHelper.LoadAll(path, type, multiVersionControlRelativePath);
            yield return new UnityEngine.WaitForFixedUpdate();

            if (null != callbackProgress)
            {
                try { callbackProgress(1); }
                catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadAllFromResourcesAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e); }
            }
            callbackLoadEnd(loadObjs);
        }

        private void _LoadAllFromLocalAsync(string path, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress)
        {
            var pathAssetBundle = GetAssetBundlePath(path, multiVersionControlRelativePath);
            shaco.GameHelper.resCache.ReadAllAsync(pathAssetBundle, multiVersionControlRelativePath, autoCheckDepencies, type, (UnityEngine.Object[] objs) =>
            {
                callbackLoadEnd(objs);
            }, (percent) =>
            {
                if (null != callbackProgress)
                {
                    try { callbackProgress(percent); }
                    catch (System.Exception e) { shaco.Log.Error("ResourcesEx _LoadAllFromLocalAsync exception: path=" + path + " type=" + " muilty=" + multiVersionControlRelativePath + " e=" + e); }
                }
            });
        }

        private string GetAssetBundlePath(string inPath, string multiVersionControlRelativePath)
        {
            var outPath = string.Empty;
            if (!shaco.HotUpdateHelper.IsCustomPrefixPath(inPath))
            {
                outPath = shaco.Base.FileHelper.ContactPath(DEFAULT_PREFIX_PATH_LOWER, inPath);
            }
            else
                outPath = inPath;
            return outPath;
        }

        private bool ExistsLocal(string path, string multiVersionControlRelativePath)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            //是否为ab包
            var fullPath = GetAssetBundleFullPath(path, multiVersionControlRelativePath);
            if (shaco.Base.FileHelper.ExistsFile(fullPath))
            {
                return true;
            }
            //是否为文件夹
            else if (shaco.Base.FileHelper.ExistsDirectory(shaco.Base.FileHelper.RemoveLastExtension(fullPath)))
            {
                return true;
            }
            else
                return false;
        }

        private string GetAssetBundleFullPath(string path, string multiVersionControlRelativePath)
        {
            path = shaco.Base.FileHelper.ReplaceLastExtension(path, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            var convertPath = GetAssetBundlePath(path, multiVersionControlRelativePath);

            var fullPath = HotUpdateHelper.GetAssetBundleFullPath(convertPath, multiVersionControlRelativePath);
            return fullPath;
        }

        private bool ExistsResourcesOrLocal(string path, string multiVersionControlRelativePath, System.Type type)
        {
            //default load from resource
            bool retValue = false;

            switch (this.resourcesLoadOrder)
            {
                case ResourcesLoadOrder.ResourcesFirst:
                    {
                        if (!retValue)
                            retValue = _LoadFromResources(path, multiVersionControlRelativePath, type) != null;

                        if (!retValue)
                        {
                            retValue = ExistsLocal(path, multiVersionControlRelativePath);
                        }
                        break;
                    }
                case ResourcesLoadOrder.DownloadFirst:
                    {
                        if (!retValue)
                        {
                            retValue = ExistsLocal(path, multiVersionControlRelativePath);
                        }

                        if (!retValue)
                            retValue = _LoadFromResources(path, multiVersionControlRelativePath, type) != null;
                        break;
                    }
                default: shaco.Log.Error("ResourcesEx ExistsResourcesOrLocal error: unsupport order type=" + this.resourcesLoadOrder); break;
            }
            return retValue;
        }

        private bool UnloadAssetBundleLocal(string path, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            var pathAssetBundle = GetAssetBundlePath(path, multiVersionControlRelativePath);
            return shaco.GameHelper.resCache.UnloadAssetBundle(pathAssetBundle, unloadAllLoadedObjects, multiVersionControlRelativePath);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        private void GetCallStackInfo(ref string stackString)
        {
#if DEBUG_LOG
            var framsString = shaco.Base.FileHelper.GetCallStackFrames(4);
            stackString = null != framsString ? "\n【【【ResourcesEx stack information:\n" + framsString.ToContactString("\n") + "\n】】】\n" : null;
#else
            stackString = null;
#endif
        }
    }
}