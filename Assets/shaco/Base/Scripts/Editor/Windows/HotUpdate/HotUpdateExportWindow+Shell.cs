using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace shacoEditor
{
    /// <summary>
    /// 自动化打包shell外部脚本自动调用接口
    /// </summary>
    public partial class HotUpdateExportWindow : EditorWindow
    {
        /// <summary>
        /// 打包资源
        /// 支持参数表如下：
        /// <name="IMPORT_PATH"           导入要被打包的资源路径(文件夹或文件)> <必须参数> 路径用英文逗号隔开，后续路径只能是相对路径(为了保证是打包同一个项目路径)
        /// <name="EXPORT_PATH"           导出打包资源路径>                  <必须参数>
        /// <name="EXPORT_OVERWRITE"      在导出目录存在的情况下也强制覆盖>     <默认不覆盖>
        /// <name="EXPORT_AUTO_ENCRYT"    是否自动加密文件>                  <默认不加密>
        /// <name="EXPORT_AUTO_COMPRESS"  是否自动加密文件>                  <默认不压缩>
        /// <name="EXPORT_AUTO_BUILD_LOG" 是否自动生成打包日志>               <默认生成>
        /// <name="EXPORT_ALL_IN_ONE_AB"  多文件打一个包还是打多个包>          <默认一个文件打一个ab包>
        /// </summary>
        static private void BuildResourcesShell()
        {
            var importPath = shaco.Base.Utility.GetEnviromentCommandValue("IMPORT_PATH");
            var exportPath = shaco.Base.Utility.GetEnviromentCommandValue("EXPORT_PATH");
            var isExportOverwrite = IsCommaondValue("EXPORT_OVERWRITE");
            var isAllFileInOneAssetBundle = IsCommaondValue("EXPORT_ALL_IN_ONE_AB");
            var isAutoEncrypt = IsCommaondValue("EXPORT_AUTO_ENCRYT");
            var isAutoCompress = IsCommaondValue("EXPORT_AUTO_COMPRESS");
            var isAutoBuildLog = IsCommaondValue("EXPORT_AUTO_BUILD_LOG");

            shaco.Log.Info("BuildResourceShell, all command value=" + System.Environment.GetCommandLineArgs().ToContactString("\n"));

            // importPath = "/Users/liuchang/Desktop/PitayaGithub/dk5plus_webgl/DK5Plus/Assets/Resources_HotUpdate/Config,Resources_HotUpdate/ADV";
            // exportPath = "/Users/liuchang/Desktop/Export";
            // exportMergePath = "/Users/liuchang/Desktop/Test";;
            // isExportOverwrite = true;
            // isAllFileInOneAssetBundle = false;
            // isAutoCompress = true;
            // isAutoEncrypt = true;
            // exportFormat = shaco.HotUpdateDefine.ExportFileFormat.OriginalFile;

            var importPaths = GetImportPaths(importPath);
            if (importPaths.IsNullOrEmpty())
            {
                shaco.Log.Exception("HotUpdateExportWindow+Shell BuildResources error: not found import path=" + importPath);
                return;
            }
            var firstImportPath = importPaths[0];
            
            if (string.IsNullOrEmpty(firstImportPath) || !shaco.Base.FileHelper.ExistsFileOrDirectory(firstImportPath))
            {
                shaco.Log.Exception("HotUpdateExportWindow+Shell BuildResources error: not found first import path=" + firstImportPath);
                return;
            }

            // if (!importPath.ToLower().Contains(shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER.RemoveLastFlag()))
            // {
            //     shaco.Log.Exception("HotUpdateExportWindow+Shell BuildResources error: not hotupdate support directory=" + importPath);
            //     return;
            // }

            if (string.IsNullOrEmpty(exportPath))
            {
                shaco.Log.Exception("HotUpdateExportWindow+Shell BuildResources error: export path is invalid=" + exportPath);
                return;
            }

            if (!isExportOverwrite && shaco.Base.FileHelper.ExistsDirectory(exportPath))
            {
                shaco.Log.Exception("HotUpdateExportWindow+Shell BuildResources error: export directory already exists, if want overwrite please pass variable 'EXPORT_OVERWRITE', path=" + exportPath);
                return;
            }

            //打开打包资源编辑器准备开始
            var exportWindow = HotUpdateExportWindow.ShowHotUpdateExportWindow();
            exportWindow.Init();

            exportPath = CheckBuildResourcesVersionPath(exportWindow, exportPath);

            BuildResources(exportWindow, importPaths, exportPath, isAllFileInOneAssetBundle, isAutoCompress, isAutoEncrypt, isAutoBuildLog);

            //打包成功，写入标记
            shaco.Base.FileHelper.WriteAllByUserPath(exportPath.ContactPath("is_build_resources_success_tmp.txt"), "1");
            exportWindow.Close();
            shaco.Log.Info("BuildResourceShell end");
        }

        /// <summary>
        /// 打包一次资源
        /// <param name="window">打包窗口</param>
        /// <param name="importPath">资源导入目录</param>
        /// <param name="exportPath">资源导出目录</param>
        /// <param name="isAllFileInOneAssetBundle">是否所有文件打一个ab包</param>
        /// <param name="isAutoCompress">是否自动压缩</param>
        /// <param name="isAutoEncrypt">是否自动加密</param>
        /// <param name="isAutoBuildLog">是否自动生成日志</param>
        /// </summary>
        static private void BuildResources(HotUpdateExportWindow window, string[] importPath, string exportPath, 
                                            bool isAllFileInOneAssetBundle, 
                                            bool isAutoCompress, 
                                            bool isAutoEncrypt,
                                            bool isAutoBuildLog)
        {
            window.OnClearButtonClick();
            
            //设定导出配置
            var versionControlFolderName = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);
            if (!exportPath.Contains(versionControlFolderName))
                exportPath = exportPath.ContactPath(versionControlFolderName);
                
            window.currentRootPath = exportPath;
            window.versionControlConfig.AutoCompressdFile = isAutoCompress;
            window.versionControlConfig.AutoEncryt = isAutoEncrypt;
            window.versionControlConfig.AutoOutputBuildLog = isAutoBuildLog;

            //加载资源列表
            for (int i = 0; i < importPath.Length; ++i)
                window.ReloadExportFilesFromDirector(importPath[i], isAllFileInOneAssetBundle);

            window.ExportAllProcess();
        }

        /// <summary>
        /// 从导出路径自动刷新当前需要导出的资源列表
        /// <param name="importPath">导入路径</param>
        /// <param name="isAllFileInOneAssetBundle">文件夹下所有文件是否打为1个ab包</param>
        /// </summary>
        public void ReloadExportFilesFromDirector(string importPath, bool isAllFileInOneAssetBundle)
        {
            Debug.Log("HotUpdateExportWindow+Shell ReloadExportFilesFromDirector importPath=" + importPath);
            if (isAllFileInOneAssetBundle)
            {
                UpdateAssetBundlePathConfig(importPath);
            }
            else
            {
                var allFiles = GetCanBuildAssetBundlesAssetPath(importPath);
                foreach (var iter in allFiles)
                {
                    var relativeFilePath = iter;
                    UpdateAssetBundlePathConfig(relativeFilePath);
                }
            }
        }

        /// <summary>
        /// 对比两个version control文件获取更新列表
        /// </summary>
        static public void SelectUpdateFileList(string fromPath, string toPath, out string updateListPath, out System.Text.StringBuilder updateListStr, string[] ignoreFolders)
        {
            updateListPath = null;
            updateListStr = null;

            if (!shaco.Base.FileHelper.ExistsDirectory(fromPath))
            {
                Debug.LogError("HotUpdateExportWindow+Shell SelectUpdateFileList error: not found fromPath=" + fromPath);
                return;
            }

            if (!shaco.Base.FileHelper.ExistsDirectory(toPath))
            {
                Debug.LogError("HotUpdateExportWindow+Shell SelectUpdateFileList error: not found toPath=" + toPath);
                return;
            }

            var versionControlConfigOld = shaco.HotUpdateHelper.FindVersionControlInPath(toPath);
            var versionControlConfig = shaco.HotUpdateHelper.FindVersionControlInPath(fromPath);
            var listModifyAssetBundle = shaco.HotUpdateHelper.SelectUpdateFiles(toPath, string.Empty, versionControlConfigOld.DicAssetBundles, versionControlConfig.ListAssetBundles, true);
            // var listDeleteAssetBundle = shaco.HotUpdateHelper.SelectDeleteFiles(versionControlConfigOld, versionControlConfig);

            shaco.Log.Info("HotUpdateExportWindow+Shell SelectUpdateFileList: old assetbundle count=" + versionControlConfigOld.DicAssetBundles.Count);
            shaco.Log.Info("HotUpdateExportWindow+Shell SelectUpdateFileList: new assetbundle count=" + versionControlConfig.ListAssetBundles.Count);
            shaco.Log.Info("HotUpdateExportWindow+Shell SelectUpdateFileList: modify count=" + listModifyAssetBundle.Count);

            //输出更新列表到文件
            var updateFilesTmp = new Dictionary<string, string>();
            var strAppend = new System.Text.StringBuilder();
            for (int i = 0; i < listModifyAssetBundle.Count; ++i)
            {
                var abName = listModifyAssetBundle[i].AssetBundleName;
                var md5 = listModifyAssetBundle[i].AssetBundleMD5;
                if (string.IsNullOrEmpty(md5))
                {
                    Debug.LogError("HotUpdateExportWindow+Shell SelectUpdateFileList error: md5 is missing, name=" + abName);
                    return;
                }
                var abPath = shaco.HotUpdateHelper.AssetBundleKeyToPath(abName);

                //目标目录为过滤目录
                if (!ignoreFolders.IsNullOrEmpty())
                {
                    bool isIgnore = false;
                    for (int j = ignoreFolders.Length - 1; j >= 0; --j)
                    {
                        if (abPath.Contains(ignoreFolders[j]))
                        {
                            isIgnore = true;
                            break;
                        }
                    }

                    //跳过本次检测
                    if (isIgnore)
                        continue;
                }

                abPath = shaco.HotUpdateHelper.AddAssetBundleNameTag(abPath, md5);

                //理论上在这里不应该存在重复路径，除非shaco.HotUpdateHelper.SelectUpdateFiles计算有问题
                if (updateFilesTmp.ContainsKey(abPath))
                {
                    shaco.Log.Error("HotUpdateExportWindow+Shell SelectUpdateFileList error: duplicate path=" + abPath);
                    continue;
                }

                updateFilesTmp.Add(abPath, null);
                strAppend.Append(abPath);
                strAppend.Append('\n');
            }

            // for (int i = 0; i < listDeleteAssetBundle.Count; ++i)
            // {
            //     var abName = listDeleteAssetBundle[i].AssetBundleName;
            //     var abPath = shaco.HotUpdateHelper.AssetBundleKeyToPath(abName);
            //     strAppend.Append("D ");
            //     strAppend.Append(abPath);
            //     strAppend.Append("\n");
            // }

            if (listModifyAssetBundle.Count > 0 && strAppend.Length > 0 && strAppend[strAppend.Length - 1] == '\n')
            {
                strAppend.Remove(strAppend.Length - 1, 1);
            }

            //如果原更新文件已经存在，则读取它并自动去重
            var updateFileListPath = toPath.ContactPath("update_file_list.txt");
            if (shaco.Base.FileHelper.ExistsFile(updateFileListPath))
            {
                var filePaths = shaco.Base.FileHelper.ReadAllByUserPath(updateFileListPath).Split("\n").ToList();
                filePaths.AddRange(updateFilesTmp.Keys.ToArray());
                updateFilesTmp.Clear();

                if (null != filePaths)
                {
                    for (int i = filePaths.Count - 1; i >= 0; --i)
                    {
                        var pathTmp = filePaths[i];

                        //空路径
                        if (string.IsNullOrEmpty(pathTmp))
                        {
                            continue;
                        }
                        
                        //去重复路径
                        if (updateFilesTmp.ContainsKey(pathTmp))
                        {
                            continue;
                        }
                        
                        //丢失路径
                        var fullPathTmp = fromPath.ContactPath(pathTmp);
                        if (!shaco.Base.FileHelper.ExistsFileOrDirectory(fullPathTmp))
                        {
                            Debug.LogWarning("HotUpdateExportWindow+Shell SelectUpdateFileList warning: not found path=" + fullPathTmp);
                            continue;
                        }

                        updateFilesTmp.Add(filePaths[i], null);
                    }
                }

                strAppend.Clear();
                foreach (var iter in updateFilesTmp)
                {
                    strAppend.Append(iter.Key);
                    strAppend.Append("\n");
                }
            }

            updateListPath = updateFileListPath;
            updateListStr = strAppend;
        }      

        private void UpdateAssetBundlePathConfig(string path)
        {
            UpdateSelectFile(path);
        }

        private void UpdateSelectFile(string path)
        {
            if (shaco.Base.FileHelper.ExistsFile(path))
            {
                var relativePath = path.ToLower();
                NewAssetBundle(new SelectFile.FileInfo(relativePath));     
            }
            else if (shaco.Base.FileHelper.ExistsDirectory(path))
            {
                var allFiles = GetCanBuildAssetBundlesAssetPath(path).ToArray();
                var listConvertAssetTmp = new HotUpdateExportWindow.SelectFile.FileInfo[allFiles.Length];
                for (int i = 0; i < allFiles.Length; ++i)
                {
                    var relativePath = allFiles[i].ToLower();
                    listConvertAssetTmp[i] = new SelectFile.FileInfo(relativePath);
                }
                NewAssetBundleDeepAssets(listConvertAssetTmp);
            }
        }

        /// <summary>
        /// 拆分导入资源路径
        /// <param name="importPath">导入路径</param>
        /// <return>拆分后的路径</return>
        /// </summary>
        static private string[] GetImportPaths(string importPath)
        {
            var retValue = importPath.Split(",");
            return retValue;
        }

        static private bool IsCommaondValue(string argName)
        {
            var commandValue = shaco.Base.Utility.GetEnviromentCommandValue(argName);
            return !string.IsNullOrEmpty(commandValue) && (commandValue == "1" || commandValue == "true");
        }

        /// <summary>
        /// 同步两个资源文件夹
        /// <param name="FROM_PATH">源目录</param>
        /// <param name="TO_PATH">目标目录</param>
        /// <param name="IS_EDITOR">是否为编辑器模式</param>编辑器模式：匹配MD5文件名，非编辑器模式：匹配原文件名
        /// <param name="IS_COPY_MANIFEST">是否同步manifest文件</param>
        /// <param name="IGNORE_FOLDER">过滤目录(同步)</param>多个过滤目录用英文逗号隔开
        /// </summary>
        static private void SyncResourcesFolder()
        {
            var fromPath = shaco.Base.Utility.GetEnviromentCommandValue("IMPORT_PATH");
            var toPath = shaco.Base.Utility.GetEnviromentCommandValue("EXPORT_PATH");
            var isEditor = IsCommaondValue("IS_EDITOR");
            var isCopyManifest = IsCommaondValue("IS_COPY_MANIFEST");
            var ignoreFolderStr = shaco.Base.Utility.GetEnviromentCommandValue("IGNORE_FOLDER");
            var ignoreFolders = string.IsNullOrEmpty(ignoreFolderStr) ? null : ignoreFolderStr.ToLower().Split(",");

            //合并目录
            var exportWindow = HotUpdateExportWindow.ShowHotUpdateExportWindow();
            exportWindow.Init();

            var platformFrom = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(fromPath);

            //当原目录不是指定目录时候才根据资源版本号匹配目录
            if (platformFrom == shaco.HotUpdateDefine.Platform.None)
                fromPath = CheckBuildResourcesVersionPath(exportWindow, fromPath);
            toPath = CheckBuildResourcesVersionPath(exportWindow, toPath);

            if (!shaco.Base.FileHelper.ExistsDirectory(fromPath))
            {
                shaco.Log.Error("HotUpdateExportWindow+Shell SyncResourcesFolder error: not found from path=" + fromPath);
                return;
            }

            if (platformFrom == shaco.HotUpdateDefine.Platform.None)
            {
                //查看所有子目录文件夹是否有资源目录
                var directoriesFrom = shaco.Base.FileHelper.GetDirectories(fromPath);
                if (null != directoriesFrom)
                {
                    for (int i = 0; i < directoriesFrom.Length; ++i)
                    {
                        var pathFromTmp = directoriesFrom[i];
                        var platformTmp = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(pathFromTmp);
                        if (platformTmp != shaco.HotUpdateDefine.Platform.None)
                        {
                            var versionControlFolderName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platformTmp);
                            var pathToTmp = toPath.ContactPath(versionControlFolderName);
                            SyncResourcesFolder(exportWindow, pathFromTmp, pathToTmp, isEditor, isCopyManifest, ignoreFolders);
                        }
                    }
                }
                else
                {
                    shaco.Log.Error("HotUpdateExportWindow SyncResourcesFolder error: not version control folder! from=" + fromPath + " to=" + toPath);
                }
                return;
            }
            else
            {
                //自动给目标目录添加VersionContol标签
                toPath = shaco.HotUpdateHelper.CheckAutoAddVersionControlFolder(toPath);
            }
            SyncResourcesFolder(exportWindow, fromPath, toPath, isEditor, isCopyManifest, ignoreFolders);
        }

        /// <summary>
        /// 同步两个资源文件夹
        /// <param name="fromPath">源目录</param>
        /// <param name="toPath">目标目录</param>
        /// <param name="isEditor">是否为编辑器模式</param>编辑器模式：匹配MD5文件名，非编辑器模式：匹配原文件名
        /// <param name="isCopyManifest">是否也拷贝manifest文件</param>
        /// <param name="IGNORE_FOLDER">过滤目录(同步)</param>多个过滤目录用英文逗号隔开
        /// </summary>
        static private void SyncResourcesFolder(HotUpdateExportWindow exportWindow, string fromPath, string toPath, bool isEditor, bool isCopyManifest, string[] ignoreFolders)
        {
            if (!shaco.Base.FileHelper.ExistsDirectory(fromPath))
            {
                shaco.Log.Error("HotUpdateExportWindow+Shell SyncResourcesFolder error: not found from path=" + fromPath);
                return;
            }

            var platformFrom = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(fromPath);

            if (shaco.Base.FileHelper.ExistsDirectory(toPath))
            {
                var platformTo = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(toPath);
                if (platformFrom == shaco.HotUpdateDefine.Platform.None || platformTo == shaco.HotUpdateDefine.Platform.None)
                {
                    shaco.Log.Error("HotUpdateExportWindow SyncResourcesFolder error: not version control folder! from=" + fromPath + " to=" + toPath);
                    return;
                }

                if (platformFrom != platformTo)
                {
                    shaco.Log.Error("HotUpdateExportWindow SyncResourcesFolder error: platform not equal ! from=" + fromPath + " to=" + toPath);
                    return;
                }
            }

            var versionControlFileName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platformFrom);
            var versionControlJsonFileName = versionControlFileName + shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL;
            // var versionControlManifestFileName = versionControlFileName + shaco.HotUpdateDefine.EXTENSION_DEPEND;

            // if (!CopyVersionControlFile(shaco.HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + shaco.HotUpdateDefine.EXTENSION_MAIN_MD5, fromPath, toPath))
            //     return;

            // var mainMD5FromPath = fromPath.ContactPath(shaco.HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + shaco.HotUpdateDefine.EXTENSION_MAIN_MD5);
            // if (!shaco.Base.FileHelper.ExistsFile(mainMD5FromPath))
            // {
            //     Debug.LogError("HotUpdateExportWindow+Shell SyncResourcesFolder erorr: not found main md5 path=" + mainMD5FromPath);
            //     return;
            // }
            // var mainMD5 = shaco.Base.FileHelper.ReadAllByUserPath(mainMD5FromPath);

            // if (!CopyVersionControlFile(versionControlFileName, fromPath, toPath))
            //     return;

            // if (!CopyVersionControlFile(versionControlJsonFileName, fromPath.ContactPath(mainMD5), toPath.ContactPath(mainMD5)))
            //     return;

            // if (!CopyVersionControlFile(versionControlManifestFileName, fromPath.ContactPath(mainMD5), toPath.ContactPath(mainMD5)))
            //     return;

            // //删除目标目录旧资源
            // var versionControlJsonPathFrom = fromPath.ContactPath(versionControlJsonFileName);
            // var versionControlTmp = shaco.HotUpdateHelper.PathToVersionControl(versionControlJsonPathFrom);
            // shaco.HotUpdateHelper.DeleteUnUseFiles(toPath, versionControlTmp, isEditor);

            //同步资源列表文件
            // var mainRootFromPath = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(fromPath);
            // var mainRootToPath = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(toPath);
            // if (!CopyVersionControlFile(versionControlJsonFileName, mainRootFromPath, mainRootToPath))
            //     return;

            //写入更新日志
            string updateListPath;
            System.Text.StringBuilder updateListStr;
            SelectUpdateFileList(fromPath, toPath, out updateListPath, out updateListStr, ignoreFolders);

            //合并两个资源目录
            exportWindow.MergeProcess(fromPath, toPath, false, isCopyManifest, ignoreFolders);
            exportWindow.Close();

            //写入更新列表
            if (null != updateListPath && null != updateListStr)
                shaco.Base.FileHelper.WriteAllByUserPath(updateListPath, updateListStr.ToString());
        }

        // static private bool CopyVersionControlFile(string fileName, string fromPath, string toPath)
        // {
        //     var filePathFrom = fromPath.ContactPath(fileName);
        //     var filePathTo = toPath.ContactPath(fileName);
        //     if (!shaco.Base.FileHelper.ExistsFile(filePathFrom))
        //     {
        //         shaco.Log.Error("HotUpdateExportWindow CopyVersionControlFile error: not found file path=" + filePathFrom);
        //         return false;
        //     }
        //     shaco.Base.FileHelper.CopyFileByUserPath(filePathFrom, filePathTo);
        //     return true;
        // }

        static private string CheckBuildResourcesVersionPath(HotUpdateExportWindow exportWindow, string path)
        {
            string retValue = path;

            //如果设定了资源版本，则在脚本打包时候自动加入资源版本目录
            if (null != exportWindow._buildVersionAsset)
            {
                var versioName = exportWindow._buildVersionAsset.ToString().Replace(".", "_");
                var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(retValue);

                //不需要替换
                Debug.Log("CheckBuildResourcesVersionPath path=" + path);
                if (path.Contains("{ignore}"))
                {
                    retValue = path.Remove("{ignore}");
                }
                //目录格式要求在目标位置直接替换版本号
                else if (path.Contains("{0}"))
                {
                    retValue = string.Format(path, versioName);
                }
                //目标为资源目录，在资源目录前方增加版本号
                else if (fileNameTmp.Contains(shaco.HotUpdateDefine.VERSION_CONTROL) && fileNameTmp.Contains(shaco.Base.GlobalParams.SIGN_FLAG))
                {
                    var folderNameTmp = shaco.Base.FileHelper.GetFolderNameByPath(retValue);

                    //如果目标路径本身就是文件夹的话，要删除末尾文件夹名字后再拼接
                    if (shaco.Base.FileHelper.ExistsDirectory(retValue))
                        folderNameTmp = folderNameTmp.RemoveBehind(fileNameTmp);
                    retValue = folderNameTmp.ContactPath(versioName).ContactPath(fileNameTmp);
                }
                //目标非资源目录，直接在后面增加版本号
                else
                {
                    retValue = retValue.ContactPath(versioName);
                }
            }

            //写入当前导出资源目录到本地文件中，以提供给shell脚本使用
            var projectPathTmp = shaco.Base.FileHelper.RemoveLastPathByLevel(Application.dataPath, 1);
            shaco.Base.FileHelper.WriteAllByUserPath(projectPathTmp.ContactPath("export_path_check_tmp.txt"), retValue);
            return retValue;
        }
    }
}