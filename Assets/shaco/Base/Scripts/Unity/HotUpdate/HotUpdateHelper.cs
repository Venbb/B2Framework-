using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace shaco
{
    public partial class HotUpdateHelper
    {
        // static readonly string UnityVersion = Application.unityVersion;

        static private HotUpdateDefine.Platform _currentPlatform = HotUpdateDefine.Platform.None;

        static private System.Collections.Generic.Dictionary<string, string> _versionControlFolderPathCached = new System.Collections.Generic.Dictionary<string, string>();

        //重置路径为原始路径，因为可能加入了平台识别的字符串
        static public string ResetAsssetBundleFileNameTag(string filename, string extension, HotUpdateDefine.Platform platform)
        {
            var pathTagOld = GetAssetBundlePathTagPlatform(platform);
            var fileTagOld = GetAssetBundleFileNameTagPlatform(platform);

            filename = filename.Remove(pathTagOld);
            filename = filename.Remove(fileTagOld);

            if (!string.IsNullOrEmpty(extension))
                filename = filename.Remove(extension);

            return filename;
        }

        //移除文件名中md5部分
        static public string RemoveAssetBundleFileNameMD5(string path)
        {
            var startIndex = path.LastIndexOf(shaco.Base.GlobalParams.SIGN_FLAG);
            var retValue = path.RemoveSubstring(shaco.Base.GlobalParams.SIGN_FLAG, HotUpdateDefine.EXTENSION_ASSETBUNDLE, startIndex);
            if (retValue.Length != path.Length)
                retValue = shaco.Base.FileHelper.AddExtensions(retValue, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            return retValue;
        }

        //获取文件名中的md5
        // static public string GetAssetBundleFileNameMD5(string path)
        // {
        //     int findIndexStart = path.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
        //     return path.Substring(shaco.Base.GlobalParams.SIGN_FLAG, shaco.Base.FileDefine.DOT_SPLIT, findIndexStart);
        // }

        static public string ResetAsssetBundleFileName(string filename, string extension, HotUpdateDefine.Platform platform)
        {
            filename = ResetAsssetBundleFileNameTag(filename, extension, platform);

            //remove file name md5 tag
            var lastFileNameTmp = shaco.Base.FileHelper.GetLastFileName(filename, true);
            if (lastFileNameTmp.Contains(shaco.Base.GlobalParams.SIGN_FLAG))
            {
                var folderTmp = shaco.Base.FileHelper.GetFolderNameByPath(filename);
                int indexBegin = lastFileNameTmp.IndexOf(shaco.Base.GlobalParams.SIGN_FLAG);
                int indexEnd = lastFileNameTmp.LastIndexOf(shaco.Base.FileDefine.DOT_SPLIT);
                if (indexEnd < 0)
                {
                    Log.Error("HotUpdate ResetAsssetBundleFileName error: can't remove file name md5 tag, not find '" + shaco.Base.FileDefine.DOT_SPLIT + "' in last file name=" + lastFileNameTmp);
                }
                else
                {
                    lastFileNameTmp = lastFileNameTmp.Remove(indexBegin, indexEnd - indexBegin);
                    filename = shaco.Base.FileHelper.ContactPath(folderTmp, lastFileNameTmp);
                }
            }

            return filename;
        }

        static public string GetAssetBundlePathPlatform(string filename, string multiVersionControlRelativePath, HotUpdateDefine.Platform platform, string extension)
        {
            string ret = HotUpdateHelper.ResetAsssetBundleFileNameTag(filename, string.Empty, platform);

            //check extension
            ret = shaco.Base.FileHelper.ReplaceLastExtension(ret, extension);

            //check path tag
            var pathTag = GetAssetBundlePathTagPlatform(platform);
            if (filename.Contains(shaco.Base.FileDefine.PATH_FLAG_SPLIT))
            {
                //is folder
                ret = pathTag.ContactPath(ret);
            }
            else
            {
                //is file
                ret = shaco.Base.FileHelper.AddFolderNameByPath(ret, pathTag, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            }

            //convert to path
            ret = HotUpdateHelper.AssetBundleKeyToPath(ret);

            //add multy version control relative path
            if (!ret.Contains(multiVersionControlRelativePath.ContactPath(pathTag)))
            {
                ret = multiVersionControlRelativePath.ContactPath(ret);
            }

            ret = AssetBundleKeyToPath(ret);

            return ret;
        }

        static public string GetAssetBundlePathAutoPlatform(string filename, string multiVersionControlRelativePath, string extension)
        {
            var platform = GetAssetBundleAutoPlatform();
            return GetAssetBundlePathPlatform(filename, multiVersionControlRelativePath, platform, extension);
        }

        static public string GetAssetBundleFullPath(string filename, string multiVersionControlRelativePath)
        {
            string versionControlFolderPath = GetAssetbundleVersionControlFolderPath(multiVersionControlRelativePath);
            return versionControlFolderPath + filename;
        }

        /// <summary>
        /// 获取版本配置文件目录
        /// </summary>
        /// <param name="filename">文件名字</param>
        /// <param name="platform">平台类型</param>
        static public string GetVersionControlFolder(string filename, string multiVersionControlRelativePath, HotUpdateDefine.Platform platform)
        {
            var pathTag = GetAssetBundlePathTagPlatform(platform);

            //is folder 
            if (filename.Contains(shaco.Base.FileDefine.PATH_FLAG_SPLIT))
            {
                filename = filename.ContactPath(multiVersionControlRelativePath.ContactPath(pathTag));
            }
            //is file
            else
            {
                filename = multiVersionControlRelativePath.ContactPath(pathTag.ContactPath(filename));
            }

            filename = AssetBundleKeyToPath(filename);
            return filename;
        }

        static public string GetVersionControlFolderAuto(string filename, string multiVersionControlRelativePath)
        {
            return GetVersionControlFolder(filename, multiVersionControlRelativePath, GetAssetBundleAutoPlatform());
        }

        static public string GetVersionControlFilePath(string filename, string multiVersionControlRelativePath)
        {
            filename = HotUpdateHelper.GetVersionControlFolderAuto(filename, multiVersionControlRelativePath);
            filename = filename.ContactPath(GetVersionControlFileName());
            return filename;
        }

        static public string GetVersionControlFileName()
        {
            var retValue = shaco.Base.FileHelper.AddExtensions(HotUpdateDefine.VERSION_CONTROL, HotUpdateDefine.EXTENSION_VERSION_CONTROL);
            retValue = HotUpdateHelper.AddFileTagAutoPlatform(retValue);
            return retValue;
        }

        static public string GetAssetBundleManifestMemoryPath(string multiVersionControlRelativePath, HotUpdateDefine.Platform platform)
        {
            var strManifestPath = shaco.Base.FileHelper.GetFullpath(HotUpdateHelper.GetVersionControlFolder(string.Empty, multiVersionControlRelativePath, platform));
            var pathTag = GetAssetBundlePathTagPlatform(platform);
            strManifestPath = shaco.Base.FileHelper.ContactPath(strManifestPath, pathTag);
            strManifestPath += HotUpdateDefine.EXTENSION_DEPEND;
            return strManifestPath;
        }

        static public string GetAssetBundleManifestMemoryPathAutoPlatform(string multiVersionControlRelativePath)
        {
            return GetAssetBundleManifestMemoryPath(multiVersionControlRelativePath, GetAssetBundleAutoPlatform());
        }

        static public string GetAssetBundleMainMD5MemoryPath(HotUpdateDefine.Platform platform, string multiVersionControlRelativePath)
        {
            var ret = HotUpdateHelper.GetVersionControlFolder(HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + HotUpdateDefine.EXTENSION_MAIN_MD5, multiVersionControlRelativePath, platform);
            ret = shaco.Base.FileHelper.GetFullpath(ret);
            return ret;
        }

        static public string GetAssetBundleMainMD5MemoryPathAuto(string multiVersionControlRelativePath)
        {
            return GetAssetBundleMainMD5MemoryPath(GetAssetBundleAutoPlatform(), multiVersionControlRelativePath);
        }

        static public void DeleteAssetbundleConfigMainMD5(string multiVersionControlRelativePath)
        {
            var pathMainMD5Tmp = HotUpdateHelper.GetAssetBundleMainMD5MemoryPathAuto(multiVersionControlRelativePath);
            if (shaco.Base.FileHelper.ExistsFile(pathMainMD5Tmp))
            {
                shaco.Base.FileHelper.DeleteByUserPath(pathMainMD5Tmp);
            }
        }

        static public string GetAssetBundleFileNameTagPlatform(HotUpdateDefine.Platform platform)
        {
            return shaco.Base.GlobalParams.SIGN_FLAG + platform.ToString();
        }

        static public string GetAssetBundlePathTagPlatform(HotUpdateDefine.Platform platform)
        {
            string ret = shaco.Base.FileDefine.PATH_FLAG_SPLIT + HotUpdateDefine.PATH_TAG + platform.ToString();
            return ret;
        }

        static public HotUpdateDefine.Platform GetAssetBundleAutoPlatform()
        {
            if (HotUpdateDefine.Platform.None != _currentPlatform)
            {
                return _currentPlatform;
            }

#if UNITY_ANDROID
            _currentPlatform = HotUpdateDefine.Platform.Android;
#elif UNITY_IPHONE
            _currentPlatform = HotUpdateDefine.Platform.iOS;
#elif UNITY_WEBGL
            _currentPlatform = HotUpdateDefine.Platform.WebGL;
#else
            _currentPlatform = HotUpdateDefine.Platform.Android;
			Log.Warning("GetAssetBundleAutoPlatform error: unsupport platform! default set it as android platform");
#endif
            return _currentPlatform;
        }

        static public HotUpdateDefine.Platform GetAssetBundlePlatformByPath(string path)
        {
            HotUpdateDefine.Platform ret = HotUpdateDefine.Platform.None;
            if (path.Contains(HotUpdateDefine.Platform.Android.ToString()))
                ret = HotUpdateDefine.Platform.Android;
            else if (path.Contains(HotUpdateDefine.Platform.iOS.ToString()))
                ret = HotUpdateDefine.Platform.iOS;
            else if (path.Contains(HotUpdateDefine.Platform.WebGL.ToString()))
                ret = HotUpdateDefine.Platform.WebGL;
            else
            {
                Debug.LogError("HotUpdateHelper GetAssetBundlePlatformByPath error: not find support platform by path=" + path);
            }

            return ret;
        }

        static public string AssetBundlePathToKey(string assetBundleName)
        {
            //已经转换过的key
            if (assetBundleName.IndexOf(HotUpdateDefine.PATH_RELATIVE_FLAG) >= 0)
                return assetBundleName;
            
            if (!string.IsNullOrEmpty(assetBundleName) && assetBundleName[0] == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
            {
                assetBundleName = assetBundleName.Substring(1);
            }
            return assetBundleName.Replace(shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING, HotUpdateDefine.PATH_RELATIVE_FLAG);
        }

        //自动检查并添加VersionControl目录
        static public string CheckAutoAddVersionControlFolder(string rootPath)
        {
            var lastFileName = shaco.Base.FileHelper.GetLastFileName(rootPath);
            if (!lastFileName.Contains(shaco.HotUpdateDefine.VERSION_CONTROL + shaco.Base.GlobalParams.SIGN_FLAG))
            {
                rootPath = rootPath.ContactPath(shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty));
            }
            return rootPath;
        }

        static public string AssetBundleKeyToPath(string assetbundleKey)
        {
            //已经转换过的路径
            if (assetbundleKey.IndexOf(HotUpdateDefine.PATH_RELATIVE_FLAG) < 0)
                return assetbundleKey;

            return assetbundleKey.Replace(HotUpdateDefine.PATH_RELATIVE_FLAG, shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING);
        }

        //执行版本描述文件(.version)中的APi
        static public bool ExecuteVersionControlAPI(string pathParam, HotUpdateDefine.SerializeVersionControl versionControl, bool isEditor, Dictionary<string, string> ignoreFilesPath)
        {
            shaco.Log.Info("HotUpdateHelper ExecuteVersionControlAPI type=" + versionControl.VersionControlAPI);
            switch (versionControl.VersionControlAPI)
            {
                case HotUpdateDefine.ExportFileAPI.DeleteFile:
                    {
                        shaco.Base.FileHelper.DeleteByUserPath(pathParam);
                        return true;
                    }
                case HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles:
                    {
                        //delete current version unuse files
                        DeleteUnUseFiles(pathParam, versionControl, isEditor, ignoreFilesPath);
                        return false;
                    }
                default: Log.Error("HotUpdateHelper ExecuteVersionControlAPI error: unsupport type=" + versionControl.VersionControlAPI); return false;
            }
        }

        //删除当前文件夹中不用的文件
        static public void DeleteUnUseFiles(string pathVersionFolder, HotUpdateDefine.SerializeVersionControl versionConfig, bool isEditor, Dictionary<string, string> ignoreFilesPath)
        {
            if (null == versionConfig)
                return;

            shaco.Log.Info("DeletUnUseFiles isEditor=" + isEditor + " pathVersionFolder=" + pathVersionFolder);

            if (string.IsNullOrEmpty(pathVersionFolder))
            {
                shaco.Log.Error("HotUpdateHelper DeleteUnUseFiles error: path is empty");
                return;
            }

            if (!string.IsNullOrEmpty(pathVersionFolder) && pathVersionFolder[pathVersionFolder.Length - 1] != shaco.Base.FileDefine.PATH_FLAG_SPLIT)
            {
                pathVersionFolder += shaco.Base.FileDefine.PATH_FLAG_SPLIT;
            }

            var assetsRootPath = pathVersionFolder.ContactPath("assets");
            var listPath = shaco.Base.FileHelper.GetFiles(assetsRootPath, "*", System.IO.SearchOption.AllDirectories);

            if (listPath.IsNullOrEmpty())
                return;

            var platformTarget = HotUpdateHelper.GetAssetBundlePlatformByPath(pathVersionFolder);

            // set search map
            // Dictionary<string, object> mapAssetbundlePath = new Dictionary<string, object>();
            // Dictionary<string, object> mapHasCheckedAssetBundleName = new Dictionary<string, object>();

            bool needReCollectConfig = false;
            if (isEditor)
            {
                //编辑器环境下要同时查看带md5的文件路径，所以它的字典查找数量刚好是正式资源数量的两倍
                needReCollectConfig = versionConfig.DicAssetBundles.Count != versionConfig.ListAssetBundles.Count * 2;
            }
            else
            {
                needReCollectConfig = versionConfig.DicAssetBundles.Count != versionConfig.ListAssetBundles.Count;
            }

            if (needReCollectConfig)
            {
                versionConfig.DicAssetBundles.Clear();
                for (int i = 0; i < versionConfig.ListAssetBundles.Count; ++i)
                {
                    var assetbundleName = versionConfig.ListAssetBundles[i].AssetBundleName;
                    if (isEditor)
                    {
                        var pathWithMD5 = HotUpdateHelper.AddAssetBundleNameTag(assetbundleName, versionConfig.ListAssetBundles[i].AssetBundleMD5);
                        if (!versionConfig.DicAssetBundles.ContainsKey(pathWithMD5))
                        {
                            versionConfig.DicAssetBundles.Add(pathWithMD5, versionConfig.ListAssetBundles[i]);
                        }
                    }
                    if (!versionConfig.DicAssetBundles.ContainsKey(assetbundleName))
                    {
                        versionConfig.DicAssetBundles.Add(assetbundleName, versionConfig.ListAssetBundles[i]);
                    }
                }
            }

            for (int i = 0; i < listPath.Length; ++i)
            {
                var pathTmp = listPath[i].Remove(pathVersionFolder);
                var keyTmp = HotUpdateHelper.AssetBundlePathToKey(pathTmp);

                //忽略导出的源文件
                if (null != ignoreFilesPath && ignoreFilesPath.ContainsKey(pathTmp))
                    continue;

                //ignore current version control file
                if (pathTmp.EndsWith(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE_MANIFEST))
                    continue;

                bool isFindKey = versionConfig.DicAssetBundles.ContainsKey(keyTmp);
                if (!isFindKey)
                {
                    var pathDeleteTmp = listPath[i];
                    if (shaco.Base.FileHelper.DeleteByUserPath(pathDeleteTmp))
                    {
                        if (isEditor)
                        {
                            DeleteManifest(pathDeleteTmp);
                        }
                        shaco.Base.FileHelper.DeleteEmptyFolder(pathDeleteTmp);
                    }
                }
            }
        }

        /// <summary>
        /// 删除在版本控制文件中找不到的引用关系
        /// <param name="manifestPath">引用关系文件路径</param>
        /// <param name="versionControl">版本控制文件</param>
        /// </summary>
        static public void DeleteUnusedDependenciesInManifest(string manifestPath, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            if (null == versionControl || !shaco.Base.FileHelper.ExistsFile(manifestPath))
                return;

            var manifestInfo = new shaco.HotUpdateManifestInfo();
            manifestInfo.LoadFromPath(manifestPath);
            manifestInfo.DeleteUnusedDependenciesInManifest(manifestPath, versionControl);
        }

        static public bool ExistsManifest(string pathAssetbundle)
        {
            var pathManifest = GetManifestPath(pathAssetbundle);
            return shaco.Base.FileHelper.ExistsFile(pathManifest);
        }

        static public bool DeleteManifest(string pathAssetbundle)
        {
            var pathManifest = GetManifestPath(pathAssetbundle);
            if (shaco.Base.FileHelper.ExistsFile(pathManifest))
            {
                shaco.Base.FileHelper.DeleteByUserPath(pathManifest);
                return true;
            }
            else
                return false;
        }

        //添加文件路径的后缀，用于平台区分
        static public string AddFileTag(string path, string stringTag)
        {
            return shaco.Base.FileHelper.AddFileNameTag(path, stringTag);
        }

        static public string AddFileTag(string path, HotUpdateDefine.Platform platform)
        {
            return AddFileTag(path, HotUpdateHelper.GetAssetBundleFileNameTagPlatform(platform));
        }

        static public string AddFileTagAutoPlatform(string path)
        {
            return AddFileTag(path, HotUpdateHelper.GetAssetBundleAutoPlatform());
        }

        // static public string AddFolderTag(string path, string tag)
        // {
        //     var ret = path;
        //     int findIndex = -1;
        //     if (shaco.Base.FileHelper.ExistsFile(path))
        //         findIndex = ret.LastIndexOf(shaco.Base.FileDefine.DOT_SPLIT);
        //     else if (shaco.Base.FileHelper.ExistsDirectory(path))
        //         findIndex = ret.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
        //     else 
        //     {
        //         Log.Error("HotUpdate AddFolderTag error: invaid path=" + path);
        //         return path;
        //     }

        //     if (!ret.Contains(tag))
        //         ret = ret.Insert(findIndex, tag);

        //     return ret;
        // }

        static public string AddAssetBundleNameTag(string assetbundleName, string assetbundleMD5)
        {
            string folder = shaco.Base.FileHelper.GetFolderNameByPath(assetbundleName);
            string filename = shaco.Base.FileHelper.GetLastFileName(assetbundleName, true);
            if (!filename.Contains(shaco.Base.GlobalParams.SIGN_FLAG))
            {
                var lastFileName = shaco.Base.FileHelper.GetLastFileName(filename);
                int dotCount = lastFileName.Split(shaco.Base.FileDefine.DOT_SPLIT).Length - 1;
                bool haveMuiltyDot = dotCount > 1;

                if (haveMuiltyDot)
                {
                    filename = filename.ReplaceFromBegin(shaco.Base.FileDefine.DOT_SPLIT_STRING, shaco.HotUpdateDefine.EXTENSION_DOT_FLAG, dotCount - 1);
                }
                filename = AddFileTag(filename, shaco.Base.GlobalParams.SIGN_FLAG + assetbundleMD5.ToLower());
                if (haveMuiltyDot)
                {
                    filename = filename.Replace(shaco.HotUpdateDefine.EXTENSION_DOT_FLAG, shaco.Base.FileDefine.DOT_SPLIT_STRING);
                }
            }
            return shaco.Base.FileHelper.ContactPath(folder, filename);
        }

        static public string RemoveAssetBundleNameTag(string assetbundleName)
        {
            string ret = assetbundleName;
            int indexFindStart = ret.LastIndexOf(shaco.Base.GlobalParams.SIGN_FLAG);
            if (indexFindStart >= 0)
            {
                int indexFindEnd = ret.LastIndexOf(shaco.Base.FileDefine.DOT_SPLIT);
                if (indexFindEnd < 0)
                {
                    Log.Error("HotUpdate RemoveAssetBundleNameTag error: not find '" + shaco.Base.FileDefine.DOT_SPLIT + "' in assetbundleName=" + assetbundleName);
                }
                else
                {
                    ret = ret.Remove(indexFindStart, indexFindEnd - indexFindStart);
                }
            }
            return ret;
        }

        /// <summary>
        /// 将纯文本对象转换为字符串
        /// </summary>
        /// <param name="asset">Asset.</param> 纯文本对象
        static public string AssetToString(Object asset)
        {
            var readObject = asset as TextAsset;
            if (readObject == null)
            {
                var readObject2 = asset as TextOrigin;
                if (null == readObject2)
                    return string.Empty;
                else
                    return readObject2.text;
            }
            return readObject.text;
        }

        static public byte[] AssetToByte(Object asset)
        {
            byte[] ret = null;
            var readText = asset as TextAsset;
            if (readText == null)
            {
                var readText2 = asset as TextOrigin;
                if (null == readText2)
                {
                    var readTexture = asset as Texture2D;
                    if (readTexture != null)
                    {
                        ret = readTexture.EncodeToPNG();
                    }
                }
                else
                    return readText2.bytes;
            }
            else
                ret = readText.bytes;

            return ret;
        }

        static public List<HotUpdateDefine.ExportAssetBundle> SelectUpdateFiles(
            string pathRoot,
            string multiVersionControlRelativePath,
            Dictionary<string, HotUpdateDefine.ExportAssetBundle> assetbundleOldDic,
            List<HotUpdateDefine.ExportAssetBundle> assetbundlesNewList,
            bool isEditor,
            bool isOnlyCheckMD5 = false)
        {
            List<HotUpdateDefine.ExportAssetBundle> listUpdateAssetBundle = new List<HotUpdateDefine.ExportAssetBundle>();

            if (assetbundlesNewList.IsNullOrEmpty() || assetbundleOldDic.IsNullOrEmpty())
                return assetbundlesNewList;

            //补充可能丢失的路径分隔符
            if (!pathRoot.EndsWith(shaco.Base.FileDefine.PATH_FLAG_SPLIT))
            {
                pathRoot = pathRoot + shaco.Base.FileDefine.PATH_FLAG_SPLIT;
            }

            //补充资源相对根目录名字
            if (!pathRoot.Contains(shaco.HotUpdateDefine.VERSION_CONTROL))
            {
                var versionControlFolderName = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath) + "/";
                pathRoot += versionControlFolderName;
            }

            for (int i = assetbundlesNewList.Count - 1; i >= 0; --i)
            {
                //check client assetbunle is valid
                var bundleServer = assetbundlesNewList[i];
                bool isShouldUpdateAssetbundle = false;

                //查找和服务器一样的ab包信息
                if (!assetbundleOldDic.ContainsKey(bundleServer.AssetBundleName))
                {
                    isShouldUpdateAssetbundle = true;
                }
                else
                {
                    HotUpdateDefine.ExportAssetBundle bundleClient = assetbundleOldDic[bundleServer.AssetBundleName];

                    //Comparison client file md5 and server file md5
                    if (bundleClient.AssetBundleMD5 != bundleServer.AssetBundleMD5)
                    {
                        isShouldUpdateAssetbundle = true;
                    }
                    //如果仅检查md5是否差异，则不再继续后续检查了
                    else if (!isOnlyCheckMD5)
                    {
                        var assetBundleNameCovert = bundleClient.AssetBundleName;
                        if (isEditor)
                        {
                            assetBundleNameCovert = HotUpdateHelper.AddAssetBundleNameTag(assetBundleNameCovert, bundleClient.AssetBundleMD5);
                        }

                        var fullPathTmp = pathRoot + HotUpdateHelper.AssetBundleKeyToPath(assetBundleNameCovert);
                        if (!shaco.Base.FileHelper.ExistsFile(fullPathTmp))
                        {
                            bundleServer.fullPathRuntime = fullPathTmp;
                            isShouldUpdateAssetbundle = true;
                        }
                    }
                }

                if (isShouldUpdateAssetbundle)
                {
                    //如果之前没有记录全称路径，则在这里记录一次
                    if (string.IsNullOrEmpty(bundleServer.fullPathRuntime))
                    {
                        var assetBundleNameCovert = bundleServer.AssetBundleName;
                        if (isEditor)
                        {
                            assetBundleNameCovert = HotUpdateHelper.AddAssetBundleNameTag(assetBundleNameCovert, bundleServer.AssetBundleMD5);
                        }
                        var fullPathTmp = pathRoot + HotUpdateHelper.AssetBundleKeyToPath(assetBundleNameCovert);
                        bundleServer.fullPathRuntime = fullPathTmp;
                    }
                    listUpdateAssetBundle.Add(bundleServer);
                }
            }
            return listUpdateAssetBundle;
        }

        static public List<HotUpdateDefine.ExportAssetBundle> SelectDeleteFiles(HotUpdateDefine.SerializeVersionControl versionOld,
                                                                                  HotUpdateDefine.SerializeVersionControl versionNew)
        {
            List<HotUpdateDefine.ExportAssetBundle> listUpdateAssetBundle = new List<HotUpdateDefine.ExportAssetBundle>();
            for (int i = 0; i < versionOld.ListAssetBundles.Count; ++i)
            {
                bool find = false;
                for (int j = 0; j < versionNew.ListAssetBundles.Count; ++j)
                {
                    if (versionNew.ListAssetBundles[j].AssetBundleName == versionOld.ListAssetBundles[i].AssetBundleName)
                    {
                        find = true;
                        break;
                    }
                }
                if (!find)
                {
                    var bundleNew = new HotUpdateDefine.ExportAssetBundle();
                    bundleNew.AssetBundleName = versionOld.ListAssetBundles[i].AssetBundleName;
                    listUpdateAssetBundle.Add(bundleNew);
                }
            }
            return listUpdateAssetBundle;
        }

        static public HotUpdateDefine.SerializeVersionControl GetVersionControlConfigLocalPathAutoPlatform(string multiVersionControlRelativePath)
        {
            var strVersionControlSavePath = GetVersionControlFilePath(string.Empty, multiVersionControlRelativePath);
            HotUpdateDefine.SerializeVersionControl retValue = new HotUpdateDefine.SerializeVersionControl();
            strVersionControlSavePath = shaco.Base.FileHelper.GetFullpath(strVersionControlSavePath);

            if (shaco.Base.FileHelper.ExistsFile(strVersionControlSavePath))
            {
                try
                {
                    retValue = HotUpdateHelper.PathToVersionControl(strVersionControlSavePath, false);
                }
                catch (System.Exception e)
                {
                    Log.Error("HotUpdate GetVersionControlConfigLocalPathAutoPlatform error: read json exception=" + e);
                }
            }
            return retValue;
        }

        static public bool CheckAllAssetbundleValid(List<HotUpdateDefine.ExportAssetBundle> assetbundles, string multiVersionControlRelativePath)
        {
            bool isMissingFile = false;
            if (assetbundles.IsNullOrEmpty())
            {
                Log.Warning("HotUpdate CheckAllAssetbundleValid warning: no version control config");
                isMissingFile = true;
            }
            else
            {
                for (int i = 0; i < assetbundles.Count; ++i)
                {
                    var bundleClient = assetbundles[i];
                    var fullPathTmp = shaco.Base.FileHelper.GetFullpath(HotUpdateHelper.GetAssetBundlePathAutoPlatform(bundleClient.AssetBundleName, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE));

                    if (!shaco.Base.FileHelper.ExistsFile(fullPathTmp))
                    {
                        Log.Info("HotUpdate check update local only fail: missing path=" + fullPathTmp);
                        isMissingFile = true;
                        break;
                    }
                }
            }

            return !isMissingFile;
        }

        static public bool CheckAllAssetbundleValidLocal(HotUpdateDefine.SerializeVersionControl versionControl, string[] filterPrefixPaths, string multiVersionControlRelativePath)
        {
            if (null == versionControl)
                return false;

            var shouldCheckAssetbundles = FilterDownloadFiles(versionControl.DicAssetBundles, filterPrefixPaths);
            if (shouldCheckAssetbundles.IsNullOrEmpty())
                return CheckAllAssetbundleValid(versionControl.ListAssetBundles, multiVersionControlRelativePath);
            else
                return CheckAllAssetbundleValid(shouldCheckAssetbundles, multiVersionControlRelativePath);
        }

        static public List<HotUpdateDefine.ExportAssetBundle> FilterDownloadFiles(Dictionary<string, HotUpdateDefine.ExportAssetBundle> assetbundlesInfo, string[] filterPrefixPaths)
        {
            var retValue = new List<HotUpdateDefine.ExportAssetBundle>();
            if (!filterPrefixPaths.IsNullOrEmpty())
            {
                var filterPrefixFolderPaths = new List<string>();
                var filterPrefixFilePaths = new List<string>();
                string[] convertFolderPaths = null;
                for (int i = 0; i < filterPrefixPaths.Length; ++i)
                {
                    if (filterPrefixPaths[i].EndsWith(shaco.Base.FileDefine.PATH_FLAG_SPLIT))
                        filterPrefixFolderPaths.Add(filterPrefixPaths[i]);
                    else
                        filterPrefixFilePaths.Add(filterPrefixPaths[i]);
                }
                ConvertToAssetbundleKey(filterPrefixFolderPaths);
                ConvertToAssetbundleKey(filterPrefixFilePaths);

                //筛选过滤目录
                convertFolderPaths = filterPrefixFolderPaths.ToArray();
                foreach (var iter in assetbundlesInfo)
                {
                    bool isFilterPath = HotUpdateHelper.IsFilterAssetbundle(iter.Value.AssetBundleName, convertFolderPaths);
                    if (isFilterPath)
                    {
                        retValue.Add(iter.Value);
                    }
                }

                //添加筛选的指定文件
                for (int i = 0; i < filterPrefixFilePaths.Count; ++i)
                {
                    var filePath = shaco.Base.FileHelper.ReplaceLastExtension(filterPrefixFilePaths[i], shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                    HotUpdateDefine.ExportAssetBundle findInfo = null;
                    if (!assetbundlesInfo.TryGetValue(filePath, out findInfo))
                    {
                        // shaco.Log.Error("HotUpdateHelper FilterDownloadFiles error: not found file path=" + filePath);
                        continue;
                    }
                    retValue.Add(findInfo);
                }
            }
            return retValue;
        }

        static private void ConvertToAssetbundleKey(List<string> filterPrefixPaths)
        {
            var defaultPrefixKey = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER;
            for (int i = filterPrefixPaths.Count - 1; i >= 0; --i)
            {
                var filterFullprefixPathTmp = filterPrefixPaths[i].ToLower();
                filterFullprefixPathTmp = shaco.Base.FileHelper.RemoveLastExtension(filterFullprefixPathTmp);
                if (!shaco.HotUpdateHelper.IsCustomPrefixPath(filterFullprefixPathTmp))
                {
                    filterFullprefixPathTmp = defaultPrefixKey.ContactPath(filterFullprefixPathTmp);
                }
                filterPrefixPaths[i] = HotUpdateHelper.AssetBundlePathToKey(filterFullprefixPathTmp);
            }
        }

        static private string[] ConvertToAssetbundlePath(string[] filterPrefixPaths)
        {
            var defaultPrefixKey = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER;
            for (int i = filterPrefixPaths.Length - 1; i >= 0; --i)
            {
                var filterFullprefixPathTmp = filterPrefixPaths[i].ToLower();
                filterFullprefixPathTmp = shaco.Base.FileHelper.RemoveLastExtension(filterFullprefixPathTmp);
                if (!shaco.HotUpdateHelper.IsCustomPrefixPath(filterFullprefixPathTmp))
                {
                    filterFullprefixPathTmp = defaultPrefixKey.ContactPath(filterFullprefixPathTmp);
                }
                filterPrefixPaths[i] = filterFullprefixPathTmp;
            }
            return filterPrefixPaths;
        }

        static public bool IsFilterAssetbundle(string asestbundleName, string[] filterPrefixPaths)
        {
            if (filterPrefixPaths.IsNullOrEmpty())
                return false;

            bool retValue = false;
            for (int j = filterPrefixPaths.Length - 1; j >= 0; --j)
            {
                if (asestbundleName.StartsWith(filterPrefixPaths[j]))
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 通过路径获取资源配置文件(自动合并拆分的配置文件)
        /// <param name="path">文件路径</param>
        /// <return></return>
        /// </summary>
        static public HotUpdateDefine.SerializeVersionControl PathToVersionControl(string path, bool isKeepListFiles = true)
        {
            if (!shaco.Base.FileHelper.ExistsFile(path))
            {
                shaco.Log.Warning("HotUpdateHelper PathToVersionControl warning: not found path=" + path);
                return new HotUpdateDefine.SerializeVersionControl();
            }
            else
            {
                var retValue = JsonToVersionControlHeader(shaco.Base.FileHelper.ReadAllByteByUserPath(path));
                JsonToVersionControlBody(path, retValue, isKeepListFiles);
                return retValue;
            }
        }

        static public string FindMainMD5RootPathInPath(string rootPath)
        {
            var mainMD5Path = rootPath.ContactPath(shaco.HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + shaco.HotUpdateDefine.EXTENSION_MAIN_MD5);
            if (!shaco.Base.FileHelper.ExistsFile(mainMD5Path))
            {
                Log.Warning("HotUpdateHelper FindMainMD5RootPathInPath warning: not found main md5 file, path=" + rootPath);
                return rootPath;
            }
            else
            {
                var mainMD5 = shaco.Base.FileHelper.ReadAllByUserPath(mainMD5Path);
                if (rootPath.Contains(mainMD5))
                    return rootPath;
                else
                    return rootPath.ContactPath(mainMD5);
            }
        }

        static public HotUpdateDefine.SerializeVersionControl FindVersionControlInPath(string versionControlRootPath)
        {
            var platformFrom = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(versionControlRootPath);
            var versionControlFileName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platformFrom);
            var versionControlJsonFileName = versionControlFileName + shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL;
            var versionControlJsonFilePath = FindMainMD5RootPathInPath(versionControlRootPath).ContactPath(versionControlJsonFileName);
            return shaco.HotUpdateHelper.PathToVersionControl(versionControlJsonFilePath);
        }

        static public HotUpdateDefine.SerializeVersionControl JsonToVersionControlHeader(byte[] headerBytes)
        {
            HotUpdateDefine.SerializeVersionControl retValue = null;
            shaco.LitJson.JsonData jsonData = null;

            if (!headerBytes.IsNullOrEmpty())
            {
                retValue = new HotUpdateDefine.SerializeVersionControl();
                jsonData = shaco.LitJson.JsonMapper.ToObject(shaco.Base.EncryptDecrypt.Decrypt(headerBytes).ToStringArray());

                retValue.AutoEncryt = (bool)jsonData["AutoEncryt"];
                retValue.AutoOutputBuildLog = (bool)jsonData["AutoOutputBuildLog"];
                retValue.AutoCompressdFile = (bool)jsonData["AutoCompressdFile"];
                retValue.FileTag = jsonData["FileTag"].ToString();
                retValue.UnityVersion = jsonData["UnityVersion"].ToString();
                retValue.Version = jsonData["Version"].ToString();
                retValue.VersionControlAPI = jsonData["VersionControlAPI"].ToString().ToEnum<shaco.HotUpdateDefine.ExportFileAPI>();
                retValue.TotalDataSize = (long)jsonData["TotalDataSize"];
                retValue.TotalSize = jsonData.ContainsKey("TotalSize") ? (int)jsonData["TotalSize"] : 0;
                retValue.baseVersion = jsonData.ContainsKey("BaseVersion") ? jsonData["BaseVersion"].ToString().ToEnum<HotUpdateDefine.BaseVersionType>() : HotUpdateDefine.BaseVersionType.VER_1_0;

                if (retValue.baseVersion == HotUpdateDefine.BaseVersionType.VER_1_0)
                {
                    var jsonAssetbundleDatas = jsonData["ListAssetBundles"];
                    int asestbundleCount = jsonAssetbundleDatas.Count;
                    for (int i = 0; i < asestbundleCount; ++i)
                    {
                        var assetbundleNew = new HotUpdateDefine.ExportAssetBundle();
                        assetbundleNew.ListFiles = new List<HotUpdateDefine.ExportAssetBundle.ExportFile>();
                        var jsonAssetbundleData = jsonAssetbundleDatas[i];
                        var jsonListFiles = jsonAssetbundleData["f"];

                        assetbundleNew.AssetBundleName = jsonAssetbundleData["n"].ToString();
                        assetbundleNew.AssetBundleMD5 = jsonAssetbundleData["m"].ToString();
                        assetbundleNew.fileSize = (long)jsonAssetbundleData["s"];
                        for (int j = 0; j < jsonListFiles.Count; ++j)
                        {
                            var assetbundleFileNew = new HotUpdateDefine.ExportAssetBundle.ExportFile();
                            assetbundleFileNew.Key = jsonListFiles[j].ToString();
                            assetbundleNew.ListFiles.Add(assetbundleFileNew);
                        }

                        retValue.ListAssetBundles.Add(assetbundleNew);
                        retValue.DicAssetBundles.Add(assetbundleNew.AssetBundleName, assetbundleNew);
                    }
                }
            }
            return retValue;
        }

        static public void JsonToVersionControlBody(string jsonPath, HotUpdateDefine.SerializeVersionControl versionControl, bool isEditor)
        {
            if (null == versionControl || versionControl.baseVersion < HotUpdateDefine.BaseVersionType.VER_2_0)
                return;

            if (string.IsNullOrEmpty(jsonPath))
                return;

            versionControl.ListAssetBundles.Clear();
            versionControl.DicAssetBundles.Clear();

            //新版本资源加载不再从json中读取，因为json本身会产生非常大量的GC问题
            var fileListPath = shaco.Base.FileHelper.ReplaceLastExtension(jsonPath, isEditor ? HotUpdateDefine.EXTENSION_FILE_LIST : HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME);

            if (!shaco.Base.FileHelper.ExistsFile(fileListPath))
            {
                // shaco.Log.Warning("HotUpdateHelper JsonToVersionControl erorr: not found file list path=" + fileListPath);
                return;
            }

            var asestbundleCount = versionControl.TotalSize;
            try
            {
                using (var streamReader = new System.IO.StreamReader(fileListPath, System.Text.Encoding.UTF8))
                {
                    for (int i = asestbundleCount - 1; i >= 0; --i)
                    {
                        var assetbundleNew = new HotUpdateDefine.ExportAssetBundle();
                        assetbundleNew.AssetBundleName = streamReader.ReadLine();
                        if (string.IsNullOrEmpty(assetbundleNew.AssetBundleName))
                        {
                            shaco.Log.Error("HotUpdateHelper JsonToVersionControl error: not found md5, index=" + i);
                            break;
                        }

                        assetbundleNew.AssetBundleMD5 = streamReader.ReadLine();
                        if (null == assetbundleNew.AssetBundleMD5)
                        {
                            shaco.Log.Error("HotUpdateHelper JsonToVersionControl error: not found md5, name=" + assetbundleNew.AssetBundleName);
                            break;
                        }

                        var fileSizeStr = streamReader.ReadLine();
                        if (null == fileSizeStr)
                        {
                            shaco.Log.Error("HotUpdateHelper JsonToVersionControl error: not found file size, name=" + assetbundleNew.AssetBundleName);
                            break;
                        }
                        assetbundleNew.fileSize = fileSizeStr.ToLong();

                        if (isEditor)
                        {
                            var formatString = streamReader.ReadLine();
                            if (null == formatString)
                            {
                                shaco.Log.Error("HotUpdateHelper JsonToVersionControl error: not found export format, name=" + assetbundleNew.AssetBundleName);
                                break;
                            }
                            assetbundleNew.exportFormat = formatString.ToEnum<shaco.HotUpdateDefine.ExportFileFormat>();

                            var childSizeStr = streamReader.ReadLine();
                            if (null == childSizeStr)
                            {
                                shaco.Log.Error("HotUpdateHelper JsonToVersionControl error: not found child size, name=" + assetbundleNew.AssetBundleName);
                                break;
                            }
                            var childSize = childSizeStr.ToInt();
                            assetbundleNew.ListFiles = new List<HotUpdateDefine.ExportAssetBundle.ExportFile>();
                            for (int j = 0; j < childSize; ++j)
                            {
                                var assetbundleFileNew = new HotUpdateDefine.ExportAssetBundle.ExportFile();
                                assetbundleFileNew.Key = streamReader.ReadLine();
                                if (null == assetbundleFileNew.Key)
                                {
                                    shaco.Log.Error("HotUpdateHelper JsonToVersionControl error: not found child path, name=" + assetbundleNew.AssetBundleName + " index=" + i + " subIndex=" + j);
                                    break;
                                }
                                assetbundleNew.ListFiles.Add(assetbundleFileNew);
                            }
                        }

                        versionControl.ListAssetBundles.Add(assetbundleNew);
                        versionControl.DicAssetBundles.Add(assetbundleNew.AssetBundleName, assetbundleNew);
                    }
                    streamReader.Close();
                }
            }
            catch (System.Exception e)
            {
                shaco.Log.Error("HotUpdateHelper JsonToVersionControlBody exception=" + e);
            }
        }

        static public void SaveVersionControlFiles(string filePath, HotUpdateDefine.SerializeVersionControl versionControl)
        {
            var retValue = new LitJson.JsonData();
            var assetbundleCount = versionControl.ListAssetBundles.Count;

            retValue["AutoEncryt"] = versionControl.AutoEncryt;
            retValue["AutoOutputBuildLog"] = versionControl.AutoOutputBuildLog;
            retValue["AutoCompressdFile"] = versionControl.AutoCompressdFile;
            retValue["FileTag"] = versionControl.FileTag;
            retValue["UnityVersion"] = versionControl.UnityVersion;
            retValue["Version"] = versionControl.Version;
            retValue["VersionControlAPI"] = versionControl.VersionControlAPI.ToString();
            retValue["TotalSize"] = assetbundleCount;
            retValue["TotalDataSize"] = versionControl.TotalDataSize;
            retValue["BaseVersion"] = versionControl.baseVersion.ToString();

            switch (versionControl.baseVersion)
            {
                case HotUpdateDefine.BaseVersionType.VER_1_0:
                    {
                        //这是1.0版本的做法，都存放在json中读取时候GC太高了
                        var nodeAssetbundels = new LitJson.JsonData();
                        nodeAssetbundels.SetJsonType(LitJson.JsonType.Array);
                        retValue["ListAssetBundles"] = nodeAssetbundels;

                        for (int i = 0; i < assetbundleCount; ++i)
                        {
                            var abInfo = versionControl.ListAssetBundles[i];

                            var nodeAssetbundel = new LitJson.JsonData();
                            nodeAssetbundel.SetJsonType(LitJson.JsonType.Object);

                            nodeAssetbundel["n"] = abInfo.AssetBundleName;
                            nodeAssetbundel["m"] = abInfo.AssetBundleMD5;
                            nodeAssetbundel["s"] = abInfo.fileSize;

                            var nodeAssetbundelFiles = new LitJson.JsonData();
                            nodeAssetbundelFiles.SetJsonType(LitJson.JsonType.Array);

                            nodeAssetbundel["f"] = nodeAssetbundelFiles;
                            for (int j = 0; j < abInfo.ListFiles.Count; ++j)
                            {
                                nodeAssetbundelFiles.Add(abInfo.ListFiles[j].Key);
                            }
                            nodeAssetbundels.Add(nodeAssetbundel);
                        }
                        shaco.Base.FileHelper.WriteAllByUserPath(filePath, retValue.ToJson());
                        break;
                    }
                default:
                    {
                        //这是2.0以上版本的做法，直接用文本，以后可以考虑用二进制或者异步

                        //保存编辑器用的配置文件
                        var fileListStr = new System.Text.StringBuilder();
                        for (int i = 0; i < assetbundleCount; ++i)
                        {
                            var abInfo = versionControl.ListAssetBundles[i];

                            fileListStr.Append(abInfo.AssetBundleName); fileListStr.AppendLine();
                            fileListStr.Append(abInfo.AssetBundleMD5); fileListStr.AppendLine();
                            fileListStr.Append(abInfo.fileSize); fileListStr.AppendLine();
                            fileListStr.Append(abInfo.exportFormat); fileListStr.AppendLine();
                            fileListStr.Append(abInfo.ListFiles.Count); fileListStr.AppendLine();

                            for (int j = 0; j < abInfo.ListFiles.Count; ++j)
                            {
                                fileListStr.Append(abInfo.ListFiles[j].Key); fileListStr.AppendLine();
                            }
                        }
                        shaco.Base.FileHelper.WriteAllByUserPath(filePath, retValue.ToJson());
                        shaco.Base.FileHelper.WriteAllByUserPath(shaco.Base.FileHelper.ReplaceLastExtension(filePath, shaco.HotUpdateDefine.EXTENSION_FILE_LIST), fileListStr.ToString());

                        //保存运行时用的配置文件
                        fileListStr.Length = 0;
                        for (int i = 0; i < assetbundleCount; ++i)
                        {
                            var abInfo = versionControl.ListAssetBundles[i];

                            fileListStr.Append(abInfo.AssetBundleName); fileListStr.AppendLine();
                            fileListStr.Append(abInfo.AssetBundleMD5); fileListStr.AppendLine();
                            fileListStr.Append(abInfo.fileSize); fileListStr.AppendLine();
                        }
                        shaco.Base.FileHelper.WriteAllByUserPath(filePath, retValue.ToJson());
                        shaco.Base.FileHelper.WriteAllByUserPath(shaco.Base.FileHelper.ReplaceLastExtension(filePath, shaco.HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME), fileListStr.ToString());
                        break;
                    }
            }
        }

        /// <summary>
        /// 判断是否为原始保留资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="shouldAutoLoadConfig">是否自动加载配置</param>
        /// <returns>
        static public bool IsKeepOriginalFile(string path)
        {
            var customParams = shaco.Base.EncryptDecrypt.GetEncryptCustomParametersPath(path);
            return !customParams.IsNullOrEmpty() ? customParams.Contains(shaco.HotUpdateDefine.ORIGINAL_FILE_TAG) : false;
        }

        /// <summary>
        /// 判断是否为压缩过的文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="shouldAutoLoadConfig">是否自动加载配置</param>
        /// <returns>
        static public bool IsCompressedFile(string path)
        {
            var customParams = shaco.Base.EncryptDecrypt.GetEncryptCustomParametersPath(path);
            return !customParams.IsNullOrEmpty() ? customParams.Contains(shaco.HotUpdateDefine.COMPRESSED_FILE_TAG) : false;
        }

        /// <summary>
        /// 检查是否为压缩文件，如果是压缩文件，则解压缩后重写加密信息头
        /// <param name="path">文件路径</param>
        /// <returns>true:文件未压缩或者解压成功 false:文件解压失败</returns>
        /// </summary>
        static public bool CheckCompressedFileAndOverwrite(string path)
        {
            bool retValue = true;

            //确认文件是否需要解压缩
            if (HotUpdateHelper.IsCompressedFile(path))
            {
                //读取文件二进制信息
                var readBytes = shaco.Base.EncryptDecrypt.DecryptPath(path);

                //获取原始加密内容
                var jumpSpeed = shaco.Base.EncryptDecrypt.GetJumpSpeedPath(path);
                var secretCode = shaco.Base.EncryptDecrypt.GetSecretCodePath(path);
                var oldEncryptCustomParameters = shaco.Base.EncryptDecrypt.GetEncryptCustomParametersPath(path);

                //解密文件内容
                var tmpPath = path + "tmp";
                shaco.Base.FileHelper.WriteAllByteByUserPath(tmpPath, readBytes);

                //解压文件
                try
                {
                    shaco.Base.FileHelper.DeleteByUserPath(path);
                    shaco.GameHelper.zip.UnZip(tmpPath, shaco.Base.FileHelper.GetFolderNameByPath(tmpPath));
                    shaco.Base.FileHelper.DeleteByUserPath(tmpPath);

                    //删除加密标记
                    oldEncryptCustomParameters.RemoveOne(value => value == HotUpdateDefine.COMPRESSED_FILE_TAG);

                    //如果源文件名字存在标记内容，则解压出来的文件名字也需要包含标记内容
                    if (path.Contains(shaco.Base.GlobalParams.SIGN_FLAG))
                    {
                        var pathWitoutTag = RemoveAssetBundleFileNameMD5(path);
                        if (shaco.Base.FileHelper.ExistsFile(pathWitoutTag))
                        {
                            shaco.Base.FileHelper.MoveFileByUserPath(pathWitoutTag, path);
                        }
                    }

                    //重写加密标记
                    shaco.Base.EncryptDecrypt.EncryptPath(path, jumpSpeed, secretCode, oldEncryptCustomParameters);
                }
                catch (System.Exception e)
                {
                    retValue = false;
                    Log.Error("HotUpdateHelper CheckCompressedFileAndOverwrite error: can't unzip path=" + path + "\nException=" + e);
                }
            }

            return retValue;
        }

        /// <summary>
        /// 检查本地资源管理配置文件是否都存在
        /// </summary>
        /// <param name="versionControlRootPath">资源版本根目录路径</param>
        /// <param name="platform">平台类型</param>
        /// <returns>true:配置ok false:配置有丢失</returns>
        static public bool HasAllVersionControlFiles(string versionControlRootPath, string mainmD5, HotUpdateDefine.Platform platform)
        {
            versionControlRootPath = versionControlRootPath.ContactPath(mainmD5);
            var pathManifest1 = versionControlRootPath.ContactPath(GetAssetBundlePathTagPlatform(platform) + HotUpdateDefine.EXTENSION_DEPEND);
            // var pathManifest2 = shaco.Base.FileHelper.RemoveLastExtension(pathManifest1);
            var pathVersionControl = versionControlRootPath.ContactPath(HotUpdateDefine.VERSION_CONTROL + GetAssetBundleFileNameTagPlatform(platform) + HotUpdateDefine.EXTENSION_VERSION_CONTROL);

            bool b2 = shaco.Base.FileHelper.ExistsFile(pathManifest1);
            // bool b3 = shaco.Base.FileHelper.ExistsFile(pathManifest2);
            bool b4 = shaco.Base.FileHelper.ExistsFile(pathVersionControl);

            return b2 && b4;
        }

        /// <summary>
        /// 检查本地资源管理配置文件是否都存在
        /// </summary>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <returns>true:配置ok false:配置有丢失</returns>
        static public bool HasAllVersionControlFiles(string multiVersionControlRelativePath)
        {
            var platform = GetAssetBundleAutoPlatform();
            var versionControlRootPath = shaco.Base.FileHelper.GetFullpath(multiVersionControlRelativePath.ContactPath(GetAssetBundlePathTagPlatform(platform)));
            return HasAllVersionControlFiles(versionControlRootPath, string.Empty, platform);
        }

        /// <summary>
        /// 从本地校验是否有文件需要更新(不需要联网)
        /// </summary>
        /// <param name="packageVersion">包名的版本号，如果设置为空默认使用Application.Version</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <returns>true:有文件需要更新 false:不需要更新</returns>
        static public bool CheckUpdateLocalOnly(string packageVersion, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            return IsCheckUpdateRequest(packageVersion, multiVersionControlRelativePath);
        }

        // /// <summary>
        // /// 获取热更新目录路径
        // /// <param name="path">文件路径</param>
        // /// </summary>
        // static public string GetHotUpdatePath(string path, string multiVersionControlRelativePath)
        // {
        //     var retValue = shaco.HotUpdateHelper.GetAssetBundlePathAutoPlatform(path, multiVersionControlRelativePath, HotUpdateDefine.EXTENSION_ASSETBUNDLE);
        //     retValue = shaco.Base.FileHelper.GetFullpath(retValue);
        //     retValue = shaco.Base.FileHelper.AddFolderNameByPath(retValue, shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER);
        //     return retValue;
        // }

        /// <summary>
        /// 判断路径是否为自定义前缀路径
        /// </summary>
        static public bool IsCustomPrefixPath(string path)
        {
            return path.StartsWith("assets/") || path.StartsWith("Assets/");
        }

        /// <summary>
        /// 删除本地下载资源
        /// <param name="platform">删除平台，当为None的时候自动设定为当前平台</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static public void DeleteVersionControlFolder(HotUpdateDefine.Platform platform = HotUpdateDefine.Platform.None, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            if (platform == HotUpdateDefine.Platform.None)
                platform = GetAssetBundleAutoPlatform();

            var versionControlFolder = GetVersionControlFolder(string.Empty, multiVersionControlRelativePath, platform);
            var fullPath = shaco.Base.FileDefine.persistentDataPath.ContactPath(versionControlFolder);
            if (shaco.Base.FileHelper.ExistsDirectory(fullPath))
            {
                shaco.Base.FileHelper.DeleteByUserPath(fullPath);
            }
            else
            {
                shaco.Log.Warning("HotUpdateHelper DeleteAllDownloadedResource warning: not found version control folder=" + fullPath);
            }
        }

        /// <summary>
        /// 判断版本号大小
        /// <param name="version1">版本号1</param>
        /// <param name="version2">版本号2</param>
        /// <return>true：version1 < version2 false: version1 > version2</return>
        /// </summary>
        static public bool CompareVersion(string version1, string version2)
        {
            bool retValue = false;
            var versionCodes1 = version1.Split(shaco.Base.FileDefine.DOT_SPLIT);
            var versionCodes2 = version2.Split(shaco.Base.FileDefine.DOT_SPLIT);
            for (int i = 0; i < versionCodes2.Length; ++i)
            {
                int codeResourceTmp = i < versionCodes1.Length ? versionCodes1[i].ToInt() : -1;
                int codePackageTmp = versionCodes2[i].ToInt();
                if (codeResourceTmp > codePackageTmp)
                {
                    retValue = false;
                    break;
                }
                else if (codeResourceTmp < codePackageTmp)
                {
                    retValue = true;
                    break;
                }
            }
            return retValue;
        }

        /// <summary>
        /// 资源下载目录是否存在
        /// <param name="platform">删除平台，当为None的时候自动设定为当前平台</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// </summary>
        static public bool IsExistsVersionControlFolder(HotUpdateDefine.Platform platform = HotUpdateDefine.Platform.None, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            if (platform == HotUpdateDefine.Platform.None)
                platform = GetAssetBundleAutoPlatform();

            var versionControlFolder = GetVersionControlFolder(string.Empty, multiVersionControlRelativePath, platform);
            var fullPath = shaco.Base.FileDefine.persistentDataPath.ContactPath(versionControlFolder);
            return shaco.Base.FileHelper.ExistsDirectory(fullPath);
        }

        static private string GetManifestPath(string pathAssetbundle)
        {
            var filenameTmp = shaco.Base.FileHelper.GetLastFileName(pathAssetbundle, true);
            var folderTmp = shaco.Base.FileHelper.GetFolderNameByPath(pathAssetbundle);

            filenameTmp = HotUpdateHelper.RemoveAssetBundleNameTag(filenameTmp);

            filenameTmp = shaco.Base.FileHelper.RemoveSubStringByFind(filenameTmp, shaco.Base.GlobalParams.SIGN_FLAG);
            filenameTmp = shaco.Base.FileHelper.AddExtensions(filenameTmp, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE_MANIFEST);
            var pathManifest = shaco.Base.FileHelper.ContactPath(folderTmp, filenameTmp);
            return pathManifest;
        }

        static private bool IsCheckUpdateRequest(string packageVersion, string multiVersionControlRelativePath)
        {
            //get client version
            if (!HasAllVersionControlFiles(multiVersionControlRelativePath))
            {
                return true;
            }
            try
            {
                var versionClient = HotUpdateHelper.GetVersionControlConfigLocalPathAutoPlatform(multiVersionControlRelativePath);
                if (versionClient == null)
                {
                    return true;
                }

                if (!string.IsNullOrEmpty(packageVersion))
                {
                    //if resource Version code < package Version code
                    //we must requst user download new resource from server !
                    bool isRequstUpdate = CompareVersion(versionClient.Version, packageVersion);
                    if (isRequstUpdate)
                    {
                        Log.Info("HotUpdate CheckUpdateRequest: client version code is less than package version code, we must requst user download new resource from server"
                        + "\n client version code=" + versionClient.Version + " package version code=" + packageVersion);

                        return true;
                    }
                }

                if (!HotUpdateHelper.CheckAllAssetbundleValid(versionClient.ListAssetBundles, multiVersionControlRelativePath))
                {
                    Log.Info("HotUpdate CheckUpdateRequest: The resources are incomplete and need to be updated online");
                    return true;
                }
            }
            catch (System.Exception e)
            {
                Log.Error("HotUpdate check update request error: msg=" + e);
                return true;
            }

            return false;
        }

        /// <summary>
        /// 完整的资源路径转换为VersionControl根目录开始的相对路径
        /// <param name="path">资源路径</param>
        /// <return>资源相对路径</return>
        /// </summary>
        static private string AssetBundleFullPathToRelativePath(string path, string multiVersionControlRelativePath)
        {
            var versionControlFolderPath = GetAssetbundleVersionControlFolderPath(multiVersionControlRelativePath);
            return path.RemoveFront(versionControlFolderPath);
        }

        static private string GetAssetbundleVersionControlFolderPath(string multiVersionControlRelativePath)
        {
            string retValue = null;
            if (!_versionControlFolderPathCached.TryGetValue(multiVersionControlRelativePath, out retValue))
            {
                if (string.IsNullOrEmpty(shaco.Base.FileDefine.persistentDataPath))
                {
                    Log.Exception("HotUpdateHelper GetAssetbundleVersionControlFolderPath exception: persistentDataPath is invalid");
                    return string.Empty;
                }

                var versionControlName = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, multiVersionControlRelativePath);
                retValue = shaco.Base.FileDefine.persistentDataPath.ContactPath(versionControlName) + shaco.Base.FileDefine.PATH_FLAG_SPLIT;
                _versionControlFolderPathCached.Add(multiVersionControlRelativePath, retValue);
            }
            return retValue;
        }
    }
}