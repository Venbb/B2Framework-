using System.Collections.Generic;
using System.Linq;

namespace shaco
{
    public class HotUpdateManifest
    {
        private Dictionary<string, HotUpdateManifestInfo> _allManifestInfo = new Dictionary<string, HotUpdateManifestInfo>();

        static public void CheckDependenciesAsync(string multiVersionControlRelativePath, bool autoCheckDepencies, string assetBundleName, System.Action<bool> callbackCheckEnd)
        {
            if (!autoCheckDepencies)
            {
                if (null != callbackCheckEnd)
                    callbackCheckEnd(true);
                return;
            }

            var manifestInfo = CheckLoaded(multiVersionControlRelativePath);
            if (null == manifestInfo)
            {
                Log.Error("HotUpdateManifest CheckDependenciesAsync error: manifest not setup...");
                callbackCheckEnd(true);
                return;
            }

            var listDepend = GetDependenciesRecursive(assetBundleName, multiVersionControlRelativePath);
            if (listDepend == null || listDepend.Length == 0)
            {
                callbackCheckEnd(true);
                return;
            }

            bool needLoadOtherAsset = false;
            bool hasError = false;

            //队列加载依赖文件
            var loadSequeue = shaco.GameHelper.objectpool.Instantiate<ResourcesOrLocalSequeue>(() => new ResourcesOrLocalSequeue());
            loadSequeue.SetLoadCountInFrame(20);
            for (int i = listDepend.Length - 1; i >= 0; --i)
            {
                //发生错误立即停止加载
                if (hasError)
                    break;

                var dependPath = listDepend[i];

                //已经加载过的资源不再重复加载
                if (shaco.GameHelper.resCache.IsLoadedAssetBundle(dependPath, multiVersionControlRelativePath))
                {
                    //因为没有走ab包的读取流程，则需要手动增加读取引用计数，为了保证引用计数的真实性
                    shaco.GameHelper.resCache.AddReferenceCount(dependPath, multiVersionControlRelativePath, false);
                    continue;
                }
                else
                {
                    needLoadOtherAsset = true;
                    loadSequeue.AddRequestAssetBundle(dependPath, (isSuccess =>
                    {
                        hasError |= !isSuccess;
                    }), multiVersionControlRelativePath, false);
                }
            }

            //由于发生加载错误退出的
            if (hasError)
            {
                loadSequeue.Stop();
                loadSequeue.RecyclingWithPool();
                callbackCheckEnd(false);
                return;
            }

            //等待加载完毕
            if (needLoadOtherAsset)
            {
                loadSequeue.Start((percent) =>
                {
                    if (percent >= 1.0f)
                    {
                        loadSequeue.RecyclingWithPool();
                        callbackCheckEnd(!hasError);
                    }
                });
            }
            //没有资源需要加载，直接回调
            else
            {
                loadSequeue.RecyclingWithPool();
                callbackCheckEnd(true);
            }
        }

        /// <summary>
        /// 强制重新加载manifest文件
        /// </summary>
        static public void Reload(string multiVersionControlRelativePath)
        {
            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            if (instance._allManifestInfo.ContainsKey(multiVersionControlRelativePath))
                instance._allManifestInfo.Remove(multiVersionControlRelativePath);
            CheckLoaded(multiVersionControlRelativePath);
        }

        /// <summary>
        /// manifest配置文件是否准备好了
        /// </summary>
        static public bool CheckManifestSetupReady(string multiVersionControlRelativePath)
        {
            var retValue = CheckLoaded(multiVersionControlRelativePath);
            return null != retValue;
        }

        static public bool CheckDependencies(string assetBundleName, string multiVersionControlRelativePath, bool autoCheckDepencies)
        {
            if (!autoCheckDepencies)
            {
                return true;
            }

            bool retValue = true;
            var manifestInfo = CheckLoaded(multiVersionControlRelativePath);
            if (null == manifestInfo)
            {
                Log.Warning("HotUpdateManifest CheckDependencies error: manifest not setup...");
                return retValue;
            }

            assetBundleName = GetValidAssetbundleName(assetBundleName);

            var listDepend = GetDependenciesRecursive(assetBundleName, multiVersionControlRelativePath);
            if (listDepend == null || listDepend.Length == 0)
            {
                return retValue;
            }

            for (int i = listDepend.Length - 1; i >= 0; --i)
            {
                var dependPath = listDepend[i];

                //已经加载过的资源不再重复加载
                if (shaco.GameHelper.resCache.IsLoadedAssetBundle(dependPath, multiVersionControlRelativePath))
                {
                    shaco.GameHelper.resCache.AddReferenceCount(dependPath, multiVersionControlRelativePath, false);
                    continue;
                }
                else
                {
                    bool isSuccess = shaco.GameHelper.resCache.LoadAssetBundle(dependPath, multiVersionControlRelativePath, false);
                    if (!isSuccess)
                    {
                        retValue = false;
                        break;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 获取有效的ab包名字，主要因为后缀名的一些问题
        /// </summary>
        static private string GetValidAssetbundleName(string assetBundleName)
        {
            assetBundleName = shaco.Base.FileHelper.ReplaceLastExtension(assetBundleName, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            return assetBundleName;
        }

        static public string[] GetDependenciesRecursive(string assetBundleName, string multiVersionControlRelativePath)
        {
            CheckLoaded(multiVersionControlRelativePath);

            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            assetBundleName = GetValidAssetbundleName(assetBundleName);

            List<string> retValue = null;
            instance.GetDependenciesBase(ref retValue, assetBundleName, multiVersionControlRelativePath, true);

            if (null != retValue && retValue.Count == 0)
            {
                Log.Error("HotUpdate GetDependenciesRecursive error: not find depend ! assetBundleName=" + assetBundleName);
            }
            return null == retValue ? null : retValue.ToArray();
        }

        static public string[] GetDependencies(string assetBundleName, string multiVersionControlRelativePath)
        {
            CheckLoaded(multiVersionControlRelativePath);

            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            assetBundleName = GetValidAssetbundleName(assetBundleName);

            List<string> retValue = null;
            instance.GetDependenciesBase(ref retValue, assetBundleName, multiVersionControlRelativePath, false);

            if (null != retValue && retValue.Count == 0)
            {
                Log.Error("HotUpdate GetDependencies error: not find depend ! assetBundleName=" + assetBundleName);
            }
            return null == retValue ? null : retValue.ToArray();
        }

        private void GetDependenciesBase(ref List<string> dependencies, string assetBundleName, string multiVersionControlRelativePath, bool recursive)
        {
            var findValue = _allManifestInfo.ContainsKey(multiVersionControlRelativePath) ? _allManifestInfo[multiVersionControlRelativePath].GetAllDependencies(assetBundleName) : null;
            if (!findValue.IsNullOrEmpty())
            {
                if (null == dependencies)
                    dependencies = new List<string>();
                dependencies.AddRange(findValue);

                if (recursive)
                {
                    for (int i = findValue.Length - 1; i >= 0; --i)
                    {
                        var findKeyTmp = shaco.Base.FileHelper.ReplaceLastExtension(findValue[i], shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);

                        //判断引用名字是否相同，防止死循环
                        if (findKeyTmp != assetBundleName)
                            GetDependenciesBase(ref dependencies, findKeyTmp, multiVersionControlRelativePath, recursive);
                    }
                }
            }
        }

        public static void Unload()
        {
            if (GameEntry.HasInstance<HotUpdateManifest>())
                GameEntry.RemoveIntance<HotUpdateManifest>();
        }

        static private HotUpdateManifestInfo CheckLoaded(string multiVersionControlRelativePath)
        {
            HotUpdateManifestInfo retValue = null;
            var instance = GameEntry.GetInstance<HotUpdateManifest>();
            if (instance._allManifestInfo.TryGetValue(multiVersionControlRelativePath, out retValue))
            {
                return retValue;
            }

            if (!UnityEngine.Application.isPlaying)
            {
                shaco.Log.Warning("HotUpdateManifest CheckLoaded warning: The method can only be called when the game is run");
                return retValue;
            }

            retValue = new HotUpdateManifestInfo();
            var pathVersion = HotUpdateHelper.GetAssetBundleManifestMemoryPathAutoPlatform(multiVersionControlRelativePath);

            shaco.Log.Info("HotUpdateManifest CheckLoaded: path=" + pathVersion);

            if (!shaco.Base.FileHelper.ExistsFile(pathVersion))
            {
                //资源文件可能存在本地，则不加载manifest了
                Log.Warning("HotUpdate init manifest error: not found path=" + pathVersion);
                return retValue;
            }

            bool success = retValue.LoadFromPath(pathVersion);
            if (success)
            {
                instance._allManifestInfo.Add(multiVersionControlRelativePath, retValue);
            }
            return retValue;
        }
    }
}