using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace shacoEditor
{
    public partial class HotUpdateExportWindow : EditorWindow
    {
        private void ExecuteExportProcess()
        {
            //如果发现导出目录已经存在版本控制文件，则选择是导出覆盖还是合并它们
            if (shaco.Base.FileHelper.ExistsDirectory(currentRootPath))
            {
                int result = EditorUtility.DisplayDialogComplex("Select Export Mode", "'VerionControl' already exist in this directory. Please select an export mode.", "Merge", "Export", "Cancel");

                //合并资源
                if (0 == result)
                {
                    MergeProcess(currentRootPath);
                }
                //导出全部内容并合并到目标目录
                else if (1 == result)
                {
                    ExportAllProcess();
                }
                //取消导出
                else if (2 == result)
                {
                    return;
                }
            }
            else
            {
                if (EditorUtility.DisplayDialog("Export All ?", string.Empty, "Confirm", "Cancel"))
                {
                    ExportAllProcess();
                }
            }
            SaveSettings();
        }

        public void ExportAllProcess()
        {
            //先保存一下场景，以防止某些场景或者prefab资源的修改没有保存到，而导出了旧的资源
#if UNITY_5_3_OR_NEWER
            AssetDatabase.SaveAssets();
#else
            EditorApplication.SaveAssets();
#endif
            Debug.Log("HotUpdateExportWindow+Export ExportAllProcess start");
            
            //自动获取旧版本配置
            if (null == versionControlConfigOld)
                versionControlConfigOld = shaco.HotUpdateHelper.FindVersionControlInPath(currentRootPath);

            //收集和整理需要导出的资源列表
            Debug.Log("HotUpdateExportWindow+Export ExportAllProcess CollectionExportInfo");
            CollectionExportInfo();

            Debug.Log("HotUpdateExportWindow+Export ExportAllProcess CollectionExportInfo, assetbundle count=" + versionControlConfig.ListAssetBundles.Count);

            var platform = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(currentRootPath);
            ExecuteBuildProcess(currentRootPath, versionControlConfig, platform);

            OnClearButtonClick();

            //需要刷新下Unity，否则Unity中查看不到更新的资源
            AssetDatabase.Refresh();
        }

        //获取需要导出的保留文件，和默认导出的文件
        private void GetExportFiles(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl, out Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle> keepFiles, out Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle> defaultFiles)
        {
            keepFiles = new Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle>();
            defaultFiles = new Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle>();

            for (int i = 0; i < versionControl.ListAssetBundles.Count; ++i)
            {
                var exportAssetBundleTmp = versionControl.ListAssetBundles[i];
                var sourceRelativePath = GetAssetBundleSourceRelativePath(exportAssetBundleTmp);

                //已经存在的文件不再重复添加
                // var pathAssetbundleWithMD5Tmp = GetAssetBundleFullPath(pathRoot, sourceRelativePath, exportAssetBundleTmp.AssetBundleMD5);
                // bool isOriginalFile = shaco.HotUpdateHelper.IsKeepOriginalFile(pathAssetbundleWithMD5Tmp);
                // bool isOriginalExportFormat = versionControl.ExportFilesFormat == shaco.HotUpdateDefine.ExportFileFormat.OriginalFile;
                // if (shaco.Base.FileHelper.ExistsFile(pathAssetbundleWithMD5Tmp) && isOriginalFile == isOriginalExportFormat)
                // {
                //     continue;
                // }

                if (exportAssetBundleTmp.exportFormat == shaco.HotUpdateDefine.ExportFileFormat.OriginalFile)
                {
                    keepFiles.Add(sourceRelativePath, exportAssetBundleTmp);
                }
                else
                {
                    defaultFiles.Add(sourceRelativePath, exportAssetBundleTmp);
                }
            }
        }

        private void CollectionExportInfo()
        {
            //清理旧assetbundle目录
            versionControlConfig.ListAssetBundles.Clear();

            //set version code
            versionControlConfig.Version = GetResourceVersion();

            //set all assetbundle's files path and md5
            foreach (var key in mapAssetbundlePath.Keys)
            {
                var value = mapAssetbundlePath[key];

                shaco.HotUpdateDefine.ExportAssetBundle newExportAB = new shaco.HotUpdateDefine.ExportAssetBundle();
                newExportAB.ListFiles = new List<shaco.HotUpdateDefine.ExportAssetBundle.ExportFile>();
                newExportAB.AssetBundleName = shaco.HotUpdateHelper.AssetBundlePathToKey(key);
                newExportAB.exportFormat = value.exportFormat;

                //computer path
                foreach (var key2 in value.ListAsset.Keys)
                {
                    var value2 = value.ListAsset[key2];

                    var newFileInfo = new shaco.HotUpdateDefine.ExportAssetBundle.ExportFile();
                    var pathTmp = value2.Asset.ToLower();

                    newFileInfo.Key = pathTmp;

                    newExportAB.ListFiles.Add(newFileInfo);

                    if (!mapAllExportAssetSameKeyCheck.ContainsKey(key2))
                    {
                        mapAllExportAssetSameKeyCheck.Add(key2, key);
                    }
                }

                if (newExportAB.ListFiles.Count > 0)
                {
                    versionControlConfig.ListAssetBundles.Add(newExportAB);
                }
                else
                    Debug.LogWarning("Hotupdate editor warning: no asset in assetbundle=" + key);
            }
        }

        private void ExecuteBuildProcess(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl, shaco.HotUpdateDefine.Platform platform)
        {
            //如果KeepFolder目录中存在Resources相对目录文件，则先拷贝Resources相对目录到Resources_HotUpdate目录中来
            // CopyResourcesToResourcesHotUpdate(versionControl.ListKeepFileFolders);

            // //偶尔的在文件更新后，unity却没有识别到assetbundle变化而重新打包时，需要检查强制重新打包assetbundle
            // Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess CheckForceBuildAssetbundle");
            // CheckForceBuildAssetbundle(versionControl, pathRoot, listModifyAssetBundle);

            var keepFiles = new Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle>();
            var defaultFiles = new Dictionary<string, shaco.HotUpdateDefine.ExportAssetBundle>();

            //获取导出文件分类
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess GetExportFiles");
            GetExportFiles(pathRoot, versionControl, out keepFiles, out defaultFiles);

            //删除丢失文件，强制重新打包
            CheckForceBuildAssetbundle(versionControlConfigOld, pathRoot, platform);

            //打包assetbundle
            Debug.Log("HotUpdateExportWindow+Export BuildAssetBundles: assetbundle file count=" + defaultFiles.Count);
            BuildAssetBundles(pathRoot, defaultFiles, _buildAssetbundleOptions);

            //重新编译manifest依赖关系文件为json，方便以后的合并操作
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess RepackageManifestToJson");
            var manifestInfo = CollectionManifestDepencies(pathRoot, versionControl, platform);

            //打包源文件
            Debug.Log("HotUpdateExportWindow+Export BuildOriginalFiles: original file count=" + keepFiles.Count);
            var buildOriginalFilesPath = BuildOriginalFiles(pathRoot, keepFiles, versionControl.AutoEncryt, versionControl.AutoCompressdFile);

            //计算所有文件md5
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess ComputerAssetbundleMD5");
            ComputerAssetbundleMD5(pathRoot, versionControl, platform);
            
            //对assetbundle用md5重命名
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess RenameAssetbundleByMD5");
            RenameAssetbundleByMD5(pathRoot, versionControl);

            //对assetbundle加密，需要放在RenameAssetbundleByMD5后执行，否则会找不到assetbundle
            if (versionControl.AutoEncryt)
            {
                Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess EncryptAssetBundles");
                EncryptAssetBundles(pathRoot, versionControl);
            }

            //删除旧的不用资源
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess CheckVersionControlAPI, api=" + versionControl.VersionControlAPI);
            CheckVersionControlAPI(platform, versionControl, buildOriginalFilesPath);

            //计算修改文件列表
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess SelectUpdateFiles");
            var listModifyAssetBundle = shaco.HotUpdateHelper.SelectUpdateFiles(currentRootPath, string.Empty, versionControlConfigOld.DicAssetBundles, versionControlConfig.ListAssetBundles, true);

            //计算所有文件大小
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess ComputerAllDataSize");
            ComputerAllDataSize(pathRoot.ContactPath("assets"), versionControl);

            //根据名字进行排序以保证主MD5计算结果一致性
            versionControl.ListAssetBundles.Sort((a, b) => { return a.AssetBundleMD5.CompareTo(b.AssetBundleMD5); });

            //计算主MD5
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess ComputerAndWriteMainMD5");
            var mainMD5 = ComputerAndWriteMainMD5(pathRoot, versionControl);

            //导出资源版本描述文件
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess ExportVersionControl");
            ExportVersionControl(pathRoot, mainMD5, manifestInfo, platform, versionControl);

            //输出打包日志
            Debug.Log("HotUpdateExportWindow+Export ExecuteBuildProcess OutputBuildLog");
            OutputBuildLog(mainMD5, platform, listModifyAssetBundle);

            //因为现在版本keepfolder文件夹已经作为原始文件导出了，这2个方法暂时弃用吧
            // SaveRetainOriginalFile(shaco.HotUpdateHelper.GetVersionControlFolder(currentRootPath, platform), platform);
            // CheckKeepFolderFilesValid(platform);
        }

        //计算并设置单个文件和所有文件大小
        private void ComputerAllDataSize(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl)
        {
            var fullPathTmp = string.Empty;
            long totalDataSize = 0;

            for (int i = versionControl.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var exportInfoTmp = versionControl.ListAssetBundles[i];
                fullPathTmp = GetAssetBundleFullPath(pathRoot, exportInfoTmp);
                exportInfoTmp.fileSize = shaco.Base.FileHelper.GetFileSize(fullPathTmp);
                totalDataSize += exportInfoTmp.fileSize;
            }

            //设置需要下载的总资源文件大小
            versionControl.TotalDataSize = totalDataSize;
        }

        /// <summary>
        /// 收集所有资源引用关系
        /// <summary>
        private shaco.HotUpdateManifestInfo.ManifestInfo CollectionManifestDepencies(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl, shaco.HotUpdateDefine.Platform platform)
        {
            var manifestInfo = new shaco.HotUpdateManifestInfo.ManifestInfo();
#if UNITY_5_3_OR_NEWER
            AssetBundle assetbundleLoad = null;
            try
            {
                //将unity原manifest文件转换为json文件
                var versionControlName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform);
                var unityManifestPath = shaco.Base.FileHelper.AddExtensions(pathRoot.ContactPath(versionControlName));

                if (!shaco.Base.FileHelper.ExistsFile(unityManifestPath))
                {
                    //如果是直接打的源文件模式，是不会生成unity manifest文件的
                    return manifestInfo;
                }

                assetbundleLoad = AssetBundle.LoadFromFile(unityManifestPath);
                var unityManifest = assetbundleLoad.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                var allAssetbundles = unityManifest.GetAllAssetBundles();

                if (null != allAssetbundles)
                {
                    var abCount = allAssetbundles.Length;
                    for (int i = 0; i < abCount; ++i)
                    {
                        var dependenies = unityManifest.GetAllDependencies(allAssetbundles[i]);
                        if (!dependenies.IsNullOrEmpty())
                            manifestInfo.dependenies.Add(allAssetbundles[i], dependenies);
                    }
                }

                //因为所有路径都是小写的，要避免后缀名有大的情况，所以要转换一次所有后缀名为小写
                // var allowAssetbundleExtensionsLower = ALLOW_ASSETBUNDLE_EXTENSIONS.Convert(v => v.ToLower());

                // for (int i = versionControl.ListAssetBundles.Count - 1; i >= 0; -- i)
                // {
                //     var assetBundleName = shaco.HotUpdateHelper.AssetBundleKeyToPath(versionControl.ListAssetBundles[i].AssetBundleName);
                //     var listAssetsTmp = versionControl.ListAssetBundles[i].ListFiles;
                //     currentDependencies.Clear();
                    
                //     for (int j = listAssetsTmp.Count - 1; j >= 0; --j)
                //     {
                //         var filePathTmp = listAssetsTmp[j].Key;
                //         var dependeniesTmp = AssetDatabase.GetDependencies(filePathTmp);
                //         if (null == dependeniesTmp)
                //             continue;
                            
                //         for (int k = dependeniesTmp.Length - 1; k >= 0; --k)
                //         {
                //             var dependendFilePathTmp = dependeniesTmp[k].ToLower();

                //             //过滤不能打包的文件
                //             if (!shaco.Base.FileHelper.ContainsFileNamePatterns(dependendFilePathTmp, allowAssetbundleExtensionsLower))
                //                 continue;

                //             //过滤自己
                //             if (dependendFilePathTmp == filePathTmp)
                //                 continue;

                //             //过滤不在打包路径中的文件
                //             var assetbundleNameTmp = string.Empty;
                //             if (!mapAllExportAssetSameKeyCheck.TryGetValue(dependendFilePathTmp, out assetbundleNameTmp))
                //                 continue;

                //             //过滤相同路径，不同后缀名文件，因为文件名相同则会自动打为同一个ab包中
                //             var pathWithoutExtension1 = shaco.Base.FileHelper.RemoveLastExtension(dependendFilePathTmp);
                //             var pathWithoutExtension2 = shaco.Base.FileHelper.RemoveLastExtension(filePathTmp);
                //             if (pathWithoutExtension1 == pathWithoutExtension2)
                //                 continue;
                                
                //             if (!currentDependencies.ContainsKey(dependendFilePathTmp))
                //                 currentDependencies.Add(dependendFilePathTmp, null);
                //         }
                //     }

                //     if (!currentDependencies.IsNullOrEmpty())
                //         manifestInfo.dependenies.Add(assetBundleName, currentDependencies.Keys.ToArray());
                // }
            }
            catch (System.Exception e)
            {
                Debug.LogError("HotUpdateExportWindow+Export RepackageManifestToJson exception: e=" + e);
            }
            finally
            {
                if (null != assetbundleLoad)
                {
                    assetbundleLoad.Unload(true);
                    assetbundleLoad = null;
                }
            }
#endif
            return manifestInfo;
        }

        /// <summary>
        /// 计算所有文件的md5
        /// <param name="versionControl">版本控制文件</param>
        /// </summary>
        private void ComputerAssetbundleMD5(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl, shaco.HotUpdateDefine.Platform platform)
        {
            var versionControlName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform);
            var unityManifestPath = shaco.Base.FileHelper.AddExtensions(pathRoot.ContactPath(versionControlName));
            
            string md5Tmp = null;
            for (int i = versionControl.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var assetbundleInfo = versionControl.ListAssetBundles[i];
                var abPath = pathRoot.ContactPath(shaco.HotUpdateHelper.AssetBundleKeyToPath(assetbundleInfo.AssetBundleName));

                //如果是作为源文件打包，需要重新计算md5
                if (assetbundleInfo.exportFormat == shaco.HotUpdateDefine.ExportFileFormat.OriginalFile)
                {
                    if (!shaco.Base.FileHelper.ExistsFile(abPath))
                        throw new System.ArgumentException("HotUpdateExportWindow+Export ComputerAssetbundleMD5 erorr: not found assetbundle path=" + abPath);

                    var strAppend = new System.Text.StringBuilder();
                    strAppend.Append(shaco.Base.FileHelper.MD5FromFile(abPath));
                    strAppend.Append("_AutoEncryt_" + versionControl.AutoEncryt);
                    strAppend.Append("_AutoCompressdFile_" + versionControl.AutoCompressdFile);
                    md5Tmp = shaco.Base.FileHelper.MD5FromString(strAppend.ToString());
                }
                else
                {
                    md5Tmp = GetAssetBundleManifestMD5(abPath);
                }

                assetbundleInfo.AssetBundleMD5 = md5Tmp;
            }
        }

        private string ComputerMainMD5(shaco.HotUpdateDefine.SerializeVersionControl versionControl)
        {
            //computer main md5
            string allFileMD5 = string.Empty;
            int filesCount = versionControl.ListAssetBundles.Count;
            for (int i = 0; i < filesCount; ++i)
            {
                allFileMD5 += versionControl.ListAssetBundles[i].AssetBundleMD5;
            }
            return shaco.Base.FileHelper.MD5FromString(allFileMD5);
        }

        private string ComputerAndWriteMainMD5(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl)
        {
            var pathRootPrevFolder = shaco.Base.FileHelper.GetFolderNameByPath(pathRoot);
            var mainMD5FilePath = pathRootPrevFolder.ContactPath(shaco.HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + shaco.HotUpdateDefine.EXTENSION_MAIN_MD5);

            string newMainMD5 = ComputerMainMD5(versionControl);
            if (shaco.Base.FileHelper.ExistsFile(mainMD5FilePath))
            {
                var oldMainMD5 = shaco.Base.FileHelper.ReadAllByUserPath(mainMD5FilePath);
                var oldMainMD5RootPath = pathRoot.ContactPath(oldMainMD5);
                var newMainMD5RootPath = pathRoot.ContactPath(newMainMD5);
                if (shaco.Base.FileHelper.ExistsDirectory(oldMainMD5RootPath))
                {
                    if (shaco.Base.FileHelper.ExistsDirectory(newMainMD5RootPath))
                        shaco.Base.FileHelper.DeleteByUserPath(newMainMD5RootPath);

                    if (oldMainMD5RootPath != newMainMD5RootPath)
                        shaco.Base.FileHelper.MoveFileByUserPath(oldMainMD5RootPath, newMainMD5RootPath);
                }
            }

            shaco.Base.FileHelper.WriteAllByUserPath(mainMD5FilePath, newMainMD5);
            return newMainMD5;
        }

        private void ExportVersionControl(string pathRoot, string mainMD5, shaco.HotUpdateManifestInfo.ManifestInfo manifestInfo, shaco.HotUpdateDefine.Platform platform, shaco.HotUpdateDefine.SerializeVersionControl versionControl)
        {
            //export json - version control
            versionControl.FileTag = shaco.HotUpdateDefine.VERSION_CONTROL;
            versionControl.UnityVersion = Application.unityVersion;

            pathRoot = pathRoot.ContactPath(mainMD5);
            var pathSaveVersion = pathRoot.ContactPath(shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform));
            EditorPrefs.SetString(EditorHelper.GetEditorPrefsKey(HotUpdateVersionImportWindow.PREVIOUS_PATH_KEY), shaco.Base.FileHelper.GetFolderNameByPath(pathSaveVersion));

            
            Debug.Log("HotUpdateExportWindow+Export ExportVersionControl pathSaveVersion=" + pathSaveVersion + " pathRoot=" + pathRoot);
            pathSaveVersion = shaco.Base.FileHelper.AddExtensions(pathSaveVersion, shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL);
            shaco.HotUpdateHelper.SaveVersionControlFiles(pathSaveVersion, versionControl);

            //覆盖写入新的manifest(实际是一个json文件)
            var manifestFileName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform);
            manifestFileName = shaco.Base.FileHelper.AddExtensions(manifestFileName, shaco.HotUpdateDefine.EXTENSION_DEPEND);
            var manifestPath = pathRoot.ContactPath(manifestFileName);
            shaco.HotUpdateManifestInfo.SaveToFile(manifestPath, versionControl, manifestInfo);

#if !UNITY_5_3_OR_NEWER
            //低版本Unity不会自动导出MainManifest相关文件，需要手动添加一个站位文件用，否则更新文件流程会出问题
            var pathVersionFolder = shaco.HotUpdateHelper.GetVersionControlFolder(pathRoot, string.Empty, platform);
            var mainManifestAssetbundlePath = pathVersionFolder.ContactPath(shaco.Base.FileHelper.GetLastFileName(pathVersionFolder));
            var mainManifestPath = shaco.Base.FileHelper.AddExtensions(mainManifestAssetbundlePath, shaco.HotUpdateDefine.EXTENSION_DEPEND);
            if (!shaco.Base.FileHelper.ExistsDirectory(mainManifestPath))
            {
                shaco.Base.FileHelper.WriteAllByUserPath(mainManifestPath, string.Empty);
            }
#endif

            Debug.Log("Export AssetBundle Resource success !\n" + "Platform=" + platform + " Version=" + versionControl.Version);
        }

        private void CheckVersionControlAPI(shaco.HotUpdateDefine.Platform platform, shaco.HotUpdateDefine.SerializeVersionControl versionControl, Dictionary<string, string> buildOriginalFilesPath)
        {
            var pathVersionFolder = shaco.HotUpdateHelper.GetVersionControlFolder(currentRootPath, string.Empty, platform);
            var strVersionFolder = shaco.Base.FileHelper.GetFolderNameByPath(pathVersionFolder, shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            shaco.HotUpdateHelper.ExecuteVersionControlAPI(strVersionFolder, versionControl, true, buildOriginalFilesPath);
        }

        /// <summary>
        /// 获取assetbundle name原文件或者文件夹相对路径
        /// </summary>
        private string GetAssetBundleSourceRelativePath(shaco.HotUpdateDefine.ExportAssetBundle exportAssetbundle)
        {
            return shaco.HotUpdateHelper.AssetBundleKeyToPath(exportAssetbundle.AssetBundleName);
        }

        //输出打包日志
        private void OutputBuildLog(string mainMD5, shaco.HotUpdateDefine.Platform platform, List<shaco.HotUpdateDefine.ExportAssetBundle> modifyAssetBundles)
        {
            if (versionControlConfig == null || string.IsNullOrEmpty(currentRootPath))
            {
                shaco.Log.Error("HotUpdateExportWindow+Export OutputBuildLog error: invalid param, config=" + versionControlConfig + " path=" + currentRootPath);
                return;
            }

            if (!versionControlConfig.AutoOutputBuildLog)
                return;

            var logPath = currentRootPath.ContactPath(shaco.HotUpdateDefine.BUILD_RESOURCES_LOG_FILE_NAME);
            var logString = new System.Text.StringBuilder();
            var listModifyAssetBundle = new List<shaco.HotUpdateDefine.ExportAssetBundle>();
            var listAddAssetBundle = new List<shaco.HotUpdateDefine.ExportAssetBundle>();
            var listDeleteAssetBundle = new List<shaco.HotUpdateDefine.ExportAssetBundle>();

            if (null == versionControlConfigOld)
                versionControlConfigOld = new shaco.HotUpdateDefine.SerializeVersionControl();

            listModifyAssetBundle = modifyAssetBundles;
            listDeleteAssetBundle = shaco.HotUpdateHelper.SelectDeleteFiles(versionControlConfigOld, versionControlConfig);

            //从更新列表中额外筛选出添加列表
            for (int i = listModifyAssetBundle.Count - 1; i >= 0; --i)
            {
                var infoTmp = listModifyAssetBundle[i];
                if (versionControlConfigOld.ListAssetBundles.Find(v => v.AssetBundleName == infoTmp.AssetBundleName) == null)
                {
                    listModifyAssetBundle.RemoveAt(i);
                    listAddAssetBundle.Add(infoTmp);
                }
            }

            //基础信息
            logString.Append("【Export root path】  " + currentRootPath);
            logString.Append("\n【Platform】          " + platform);
            logString.Append("\n【Main MD5】          " + mainMD5);
            logString.Append("\n【AutoEncryt】        " + versionControlConfig.AutoEncryt);
            logString.Append("\n【AutoCompressdFile】 " + versionControlConfig.AutoCompressdFile);
            logString.Append("\n【FileTag】           " + versionControlConfig.FileTag);
            logString.Append("\n【UnityVersion】      " + versionControlConfig.UnityVersion);
            logString.Append("\n【Version】           " + versionControlConfig.Version);
            logString.Append("\n【VersionControlAPI】 " + versionControlConfig.VersionControlAPI);
            logString.Append("\n【TotalDataSize】     " + versionControlConfig.TotalDataSize);
            logString.Append("\n【Total count】       " + versionControlConfig.ListAssetBundles.Count);
            logString.Append("\n");

            var defaultPrefix = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER;
            System.Action<string, string, List<shaco.HotUpdateDefine.ExportAssetBundle>, System.Action<shaco.HotUpdateDefine.ExportAssetBundle>> appendABLogFunc 
                = (string title, string flag, List<shaco.HotUpdateDefine.ExportAssetBundle> abInfos, System.Action<shaco.HotUpdateDefine.ExportAssetBundle> callbackDraw) =>
            {
                if (abInfos.Count == 0)
                    return;
                    
                logString.Append("\n-------------------------------------------------------------------------\n");
                logString.Append(title + ": " + abInfos.Count);
                logString.Append("\n-------------------------------------------------------------------------\n");
                for (int i = abInfos.Count - 1; i >= 0; --i)
                {
                    var abInfo = abInfos[i];
                    logString.Append(flag);
                    logString.Append(shaco.HotUpdateHelper.AssetBundleKeyToPath(abInfo.AssetBundleName).RemoveFront(defaultPrefix).RemoveBehind(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE));

                    if (null != callbackDraw)
                        callbackDraw(abInfo);
                    logString.Append("\n");
                }
                logString.Append("\n");
            };

            if (listAddAssetBundle.Count == 0 && listModifyAssetBundle.Count == 0 && listDeleteAssetBundle.Count == 0)
            {
                logString.Append("\n-------------------------------------------------------------------------\n");
                logString.Append("no resource build");
                logString.Append("\n-------------------------------------------------------------------------");
            }
            else
            {
                appendABLogFunc("Add list", "A ", listAddAssetBundle, (abInfo) =>
                {
                    // logString.Append("\n【MD5】  ");
                    // logString.Append(abInfo.AssetBundleMD5);
                    // logString.Append("\n【Size】 ");
                    // logString.Append(abInfo.fileSize);
                });
                appendABLogFunc("Modify list", "M ", listModifyAssetBundle, (abInfo) =>
                {
                    // logString.Append("\n【MD5】  ");
                    // logString.Append(abInfo.AssetBundleMD5);
                    // logString.Append("\n");
                    // logString.Append("\n【Size】 ");
                    // logString.Append(abInfo.fileSize);
                });

                //merge时候不允许有删除功能，所以也没有删除日志了
                if (!_isMerging)
                    appendABLogFunc("Delete list", "D ", listDeleteAssetBundle, null);
            }
            shaco.Base.FileHelper.WriteAllByUserPath(logPath, logString.ToString());
        }

        private void LogErrorNoAssetExport(string assetBundleName, List<SelectFile.FileInfo> listIgnoreeAsset)
        {
            bool isFindIgnore = false;
            for (int i = 0; i < listIgnoreeAsset.Count; ++i)
            {
                if (listIgnoreeAsset[i] != null)
                {
                    isFindIgnore = true;
                    break;
                }
            }
            if (!isFindIgnore)
                return;

            for (int i = 0; i < listIgnoreeAsset.Count; ++i)
            {
                if (listIgnoreeAsset[i] != null)
                    Debug.LogWarning("unsupport build unity asset path=" + listIgnoreeAsset[i].Asset);
            }
        }

        //用md5码对所有资源文件重命名
        private void RenameAssetbundleByMD5(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl)
        {
            foreach (var iter in versionControl.ListAssetBundles)
            {
                var pathSource = shaco.Base.FileHelper.ContactPath(pathRoot, GetAssetBundleSourceRelativePath(iter));

                if (!shaco.Base.FileHelper.ExistsFile(pathSource))
                {
                    //没有修改的文件不需要重命名
                    // Debug.LogError("HotUpdateExportWindow+Export RenameAssetbundleByMD5 error: mssing path=" + pathSource);
                    continue;
                }

                if (string.IsNullOrEmpty(iter.AssetBundleMD5))
                {
                    DisplayDialogError("rename error: md5 is empty !");
                    continue;
                }

                var pathDest = shaco.HotUpdateHelper.AddAssetBundleNameTag(pathSource, iter.AssetBundleMD5);
                bool isKeepOriginalFileSetting = iter.exportFormat == shaco.HotUpdateDefine.ExportFileFormat.OriginalFile;
                bool isExistsDestinationFile = shaco.Base.FileHelper.ExistsFile(pathDest);

                //如果目标文件已经存在则先删除目标文件，以防止可能的文件md5与文件本身不对应导致的资源错乱
                if (isExistsDestinationFile)
                {
                    //是否为原始文件的标记不一致的时候，也要删除原始文件
                    bool isKeepOriginalFileFile = shaco.HotUpdateHelper.IsKeepOriginalFile(pathDest);
                    if (isKeepOriginalFileFile != isKeepOriginalFileSetting)
                    {
                        shaco.Base.FileHelper.DeleteByUserPath(pathDest);
                        isExistsDestinationFile = false;
                    }
                }

                //强制原文件打包或者当目标文件不存在的时候，拷贝文件
                if (!isExistsDestinationFile)
                {
                    shaco.Base.FileHelper.MoveFileByUserPath(pathSource, pathDest);
                }
                //已有打包文件，则删除临时文件
                else
                {
                    shaco.Base.FileHelper.DeleteByUserPath(pathSource);
                }
            }
        }

        private void EncryptAssetBundles(string pathRoot, shaco.HotUpdateDefine.SerializeVersionControl versionControl)
        {
            foreach (var iter in versionControl.ListAssetBundles)
            {
                var pathSource = shaco.Base.FileHelper.ContactPath(pathRoot, GetAssetBundleSourceRelativePath(iter));
                var pathSourceWithMD5 = shaco.HotUpdateHelper.AddAssetBundleNameTag(pathSource, iter.AssetBundleMD5);

                var secretRandomCode = shaco.Base.Utility.Random(int.MinValue, int.MaxValue);

                if (shaco.Base.FileHelper.ExistsFile(pathSourceWithMD5) && !shaco.Base.EncryptDecrypt.IsEncryptionPath(pathSourceWithMD5))
                {
                    shaco.Base.EncryptDecrypt.EncryptPath(pathSourceWithMD5, 0.314f, secretRandomCode);
                }
            }
        }

        private string GetResourceVersion()
        {
            if (_versionCodes.IsNullOrEmpty())
            {
#if UNITY_5_3_OR_NEWER
                return Application.version;
#else
                return PlayerSettings.bundleVersion;
#endif
            }
            string retValue = string.Empty;
            for (int i = 0; i < _versionCodes.Count; ++i)
            {
                retValue += _versionCodes[i] + shaco.Base.FileDefine.DOT_SPLIT;
            }
            return retValue.Remove(retValue.Length - shaco.Base.FileDefine.DOT_SPLIT_STRING.Length);
        }

        /// <summary>
        /// assetbundle are sometimes modified, but unity does not detect changes,
        /// there is no automatic build, and we need to delete the manifest file and force unity to generate new assetbundle
        /// </summary>
        /// <returns> if return true, we need call 'BuildAssetBundles' again </returns>
        /// <param name="versionControl"> version control file </param>
        private void CheckForceBuildAssetbundle(shaco.HotUpdateDefine.SerializeVersionControl versionControl, string pathRoot, shaco.HotUpdateDefine.Platform platform)
        {
            var versionControlName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform);

            for (int i = 0; i < versionControl.ListAssetBundles.Count; ++i)
            {
                var assetbundleExportInfo = versionControl.ListAssetBundles[i];
                var pathAssetbundleTmp = shaco.Base.FileHelper.ContactPath(pathRoot, shaco.HotUpdateHelper.AssetBundleKeyToPath(assetbundleExportInfo.AssetBundleName));
                var pathAssetbundleWithMD5Tmp = shaco.HotUpdateHelper.AddAssetBundleNameTag(pathAssetbundleTmp, assetbundleExportInfo.AssetBundleMD5);
                bool isMissingFile = false;

                if (assetbundleExportInfo.exportFormat != shaco.HotUpdateDefine.ExportFileFormat.OriginalFile)
                {
                    //ab文件丢失
                    if (!shaco.Base.FileHelper.ExistsFile(pathAssetbundleWithMD5Tmp))
                    {
                        isMissingFile = true;
                    }
                }

                //ab丢失的时候
                if (isMissingFile)
                {
                    //删除manifest强制重新打包
                    shaco.HotUpdateHelper.DeleteManifest(pathAssetbundleTmp);
                }
                else
                {
                    //manifest文件丢失但是ab存在
                    if (!shaco.HotUpdateHelper.ExistsManifest(pathAssetbundleTmp))
                    {
                        //删除原assetbundle文件再强制重新重打包
                        shaco.Base.FileHelper.DeleteByUserPath(pathAssetbundleWithMD5Tmp);
                        continue;
                    }
                    else 
                    {
                        //manifest文件的md5值和原assetbundle的md5不一致
                        var oldMD5 = GetAssetBundleManifestMD5(pathAssetbundleTmp);
                        if (string.IsNullOrEmpty(oldMD5) || oldMD5 != assetbundleExportInfo.AssetBundleMD5)
                        {
                            //删除ab包和manifest文件
                            shaco.Base.FileHelper.DeleteByUserPath(pathAssetbundleWithMD5Tmp);
                            shaco.HotUpdateHelper.DeleteManifest(pathAssetbundleTmp);
                            continue;
                        }
                    }

                    //加密规则改变的时候强制重新打包
                    bool isKeepFile = shaco.HotUpdateHelper.IsKeepOriginalFile(pathAssetbundleWithMD5Tmp);
                    bool isEncryptFile = isKeepFile ? shaco.Base.EncryptDecrypt.GetJumpSpeedPath(pathAssetbundleWithMD5Tmp) != 1.0f : shaco.Base.EncryptDecrypt.IsEncryptionPath(pathAssetbundleWithMD5Tmp);
                    if (isEncryptFile != versionControl.AutoEncryt)
                    {
                        shaco.HotUpdateHelper.DeleteManifest(pathAssetbundleTmp);
                        if (shaco.Base.FileHelper.ExistsFile(pathAssetbundleWithMD5Tmp))
                            shaco.Base.FileHelper.DeleteByUserPath(pathAssetbundleWithMD5Tmp);
                    }
                }
            }
        }

        private string GetAssetBundleManifestMD5(string pathAssetBundle)
        {
            if (!pathAssetBundle.EndsWith(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE))
            {
                throw new System.ArgumentException("HotUpdateExportWindow+Export GetAssetBundleManifestMD5 erorr: not assetbundle file path=" + pathAssetBundle);
            }

            var manifestPathTmp = shaco.Base.FileHelper.AddExtensions(pathAssetBundle, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE_MANIFEST);
            if (!shaco.Base.FileHelper.ExistsFile(manifestPathTmp))
            {
                throw new System.ArgumentException("HotUpdateExportWindow+Export GetAssetBundleManifestMD5 erorr: not found manifest path=" + manifestPathTmp);
            }

            var retValue = shaco.Base.FileHelper.MD5FromFile(manifestPathTmp);
            return retValue;
        }
    }
}