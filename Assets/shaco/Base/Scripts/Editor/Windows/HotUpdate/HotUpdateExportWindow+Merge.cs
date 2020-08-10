using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class HotUpdateExportWindow : EditorWindow
    {
        private readonly string TEMPORARY_EXPORT_FOLDER_NAME = "_Merge(Temporary)";

        private enum FileNameStatus
        {
            WithMD5,
            NoMD5,
            Missing
        }

        private class ChagnedMergeInfo
        {
            public shaco.HotUpdateDefine.ExportAssetBundle exportInfoFrom = null;
            public shaco.HotUpdateDefine.ExportAssetBundle exportInfoTo = null;
        }

        //当前是否在进行merge
        private bool _isMerging = false;

        /// <summary>
        /// 获取资源平台名字
        /// <param name="resourceFolderPath">资源文件夹路径</param>
        /// </summary>
        private string GetResourcePlatformName(string resourceFolderPath)
        {
            var platformTarget = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(resourceFolderPath);
            var retValue = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platformTarget);
            if (retValue.IndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT) >= 0)
                retValue = retValue.Remove(0, shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING.Length);
            return retValue;
        }

        /// <summary>
        /// 合并当前导出的资源文件夹到目标文件夹中
        /// <param name="toPath">合并到的文件夹路径</param>
        /// </summary>
        private void MergeProcess(string toPath)
        {
            _isMerging = true;

            //获取对应平台文件夹
            var currentPlatformVersionControlFolderName = GetResourcePlatformName(toPath);

            //设置为当前临时导出目录
            var rootFolder = shaco.Base.FileHelper.RemoveLastPathByLevel(toPath, 1);
            currentRootPath = rootFolder.ContactPath(shaco.HotUpdateDefine.VERSION_CONTROL + TEMPORARY_EXPORT_FOLDER_NAME);
            var temMergeRootPath = currentRootPath;
            currentRootPath = currentRootPath.ContactPath(currentPlatformVersionControlFolderName);

            //因为ExportAllProcess会将currentRootPath置空，所以要临时保存下路径
            var fromPath = currentRootPath;

            //先新建一个临时目录导出本次的资源
            versionControlConfigOld = shaco.HotUpdateHelper.FindVersionControlInPath(toPath);
            var configOldTmp = versionControlConfigOld;
            ExportAllProcess();
            versionControlConfigOld = configOldTmp;

            //合并资源
            MergeProcess(fromPath, toPath, true, true);

            //删除临时merge导出目录
            shaco.Base.FileHelper.DeleteByUserPath(temMergeRootPath);

            _isMerging = false;
        }

        /// <summary>
        /// 获取根目录下所有资源目录
        /// <param name="rootPath">需要合并的资源根目录</param>
        /// </summary>
        private string[] FindVersionControlFolders(string rootPath)
        {
            var retValue = new List<string>();

            var directores = shaco.Base.FileHelper.GetDirectories(rootPath);
            if (directores.IsNullOrEmpty())
                return retValue.ToArray();

            var currentPlatformVersionControlFolderName = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);

            for (int i = directores.Length - 1; i >= 0; --i)
            {
                //找到对应平台资源目录
                if (directores[i].Contains(currentPlatformVersionControlFolderName))
                {
                    retValue.Add(directores[i]);
                }
                //递归遍历查找子目录，直到发现资源目录为止
                else
                {
                    retValue.AddRange(FindVersionControlFolders(directores[i]));
                }
            }

            return retValue.ToArray();
        }

        /// <summary>
        /// 合并资源根目录下所有资源组
        /// <param name="rootPath">需要合并的资源根目录</param>
        /// </summary>
        private void MergeMuiltyProcess(string rootPath)
        {
            var directores = FindVersionControlFolders(rootPath);

            //如果没有个2个或者以上的资源目录则不合并
            if (directores.Length < 2)
            {
                shaco.Log.Error("HotUpdateExportWindow+Merge MergeMuiltyProcess error: not found resources folder need merge");
                return;
            }

            //合并资源目录
            var toPath = directores[0];
            var moveToPaths = new List<string>();
            for (int i = 1; i < directores.Length; ++i)
            {
                var fromPath = toPath;
                toPath = directores[i];
                MergeProcess(fromPath, toPath, true, true);
                moveToPaths.Add(toPath);
            }

            var movedPath = shaco.Base.FileHelper.RemoveLastPathByLevel(toPath, 2).ContactPath(shaco.Base.FileHelper.GetLastFileName(toPath));

            //如果目录已经存在，则合并过去
            if (shaco.Base.FileHelper.ExistsDirectory(movedPath))
            {
                MergeProcess(toPath, movedPath, true, true);
            }
            //移动合并后的目录到根目录下
            else
            {
                shaco.Base.FileHelper.MoveFileByUserPath(toPath, movedPath);
            }

            //删除移动过的临时空文件夹
            for (int i = moveToPaths.Count - 1; i >= 0; --i)
            {
                var temMergePath = shaco.Base.FileHelper.RemoveLastPathByLevel(moveToPaths[i], 1);
                shaco.Base.FileHelper.DeleteByUserPath(temMergePath);
            }
        }

        /// <summary>
        /// 合并当前导出的资源文件夹到目标文件夹中
        /// <param name="fromPath">要合并的文件夹路径</param>
        /// <param name="toPath">合并到的文件夹路径</param>
        /// <param name="autoDeleteFromPath">是否自动删除要合并的文件夹</param>
        /// <param name="isCopyManifest">是否也拷贝manifest文件</param>
        /// <param name="ignoreFolders">合并过滤目录</param>
        /// </summary>
        public void MergeProcess(string fromPath, string toPath, bool autoDeleteFromPath, bool isCopyManifest, params string[] ignoreFolders)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                shaco.Log.Error("HotUpdateExportWindow+Merge MergeProcess error: invalid from path=" + fromPath);
                return;
            }

            if (string.IsNullOrEmpty(toPath))
            {
                shaco.Log.Error("HotUpdateExportWindow+Merge MergeProcess error: invalid to path=" + toPath);
                return;
            }

            if (fromPath == toPath)
            {
                shaco.Log.Info("HotUpdateExportWindow+Merge MergeProcess error: same path=" + fromPath);
                return;
            }

            //如果目标目录没有设定版本控制文件夹，则使用当前平台的
            fromPath = shaco.HotUpdateHelper.CheckAutoAddVersionControlFolder(fromPath);
            toPath = shaco.HotUpdateHelper.CheckAutoAddVersionControlFolder(toPath);

            shaco.Log.Info("HotUpdateExportWindow+Merge MergeProcess, from=" + fromPath + " to=" + toPath);

            //获取对应平台文件夹
            var currentPlatformVersionControlFolderName = GetResourcePlatformName(toPath);

            //自动匹配当前版本文件夹
            var lastFileNameCheckFrom = shaco.Base.FileHelper.GetLastFileName(fromPath);
            var lastFileNameCheckTo = shaco.Base.FileHelper.GetLastFileName(toPath);
            if (lastFileNameCheckTo.IndexOf(shaco.Base.GlobalParams.SIGN_FLAG) >= 0 && lastFileNameCheckFrom.IndexOf(shaco.Base.GlobalParams.SIGN_FLAG) >= 0)
            {
                //如果目标文件夹平台与被合并的平台名字不一致的时候，不允许合并
                if (lastFileNameCheckTo != currentPlatformVersionControlFolderName)
                {
                    Debug.LogError("HotUpdateExportWindow+Merge MergeProcess error: Folder merge on different platforms cannot be selected");
                    return;
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Not shaco GameFramework Resources Folder", "OK");
                return;
            }

            if (null == versionControlConfigOld)
            {
                versionControlConfigOld = shaco.HotUpdateHelper.FindVersionControlInPath(fromPath);
            }

            //如果目标版本信息为空，则直接拷贝所有内容并不再进行合并操作
            if (versionControlConfigOld.ListAssetBundles.Count == 0 && shaco.Base.FileHelper.ExistsDirectory(fromPath))
            {
                //当目标文件夹的内容为空的时候，直接拷贝临时merge文件夹覆盖目标文件夹
                shaco.Base.FileHelper.DeleteByUserPath(toPath);

                if (isCopyManifest)
                    shaco.Base.FileHelper.CopyFileByUserPath(fromPath, toPath);
                else
                    shaco.Base.FileHelper.CopyFileByUserPath(fromPath, toPath, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE_MANIFEST);
                return;
            }

            var versionControlTo = shaco.HotUpdateHelper.FindVersionControlInPath(toPath);

            //当使用Unity默认的assetbundle模式进行导出的时候，检查2个版本控制文件版本是否一致
            if (Application.unityVersion != versionControlTo.UnityVersion)
            {
                if (Application.isBatchMode)
                {
                    Debug.LogError("HotUpdateExportWindow+Merge MergeProcess error: is difference unity version, from=" + Application.unityVersion + " to=" + versionControlTo.UnityVersion);
                    return;
                }
                else
                {
                    if (!EditorUtility.DisplayDialog("Warning", string.Format("merge with different unity version \nFrom: {0}\nTo    : {1}", Application.unityVersion, versionControlTo.UnityVersion), "Force Continue", "Stop Merge"))
                    {
                        //停止merge，回滚路径
                        toPath = shaco.Base.FileHelper.RemoveSubStringByFind(toPath, TEMPORARY_EXPORT_FOLDER_NAME);
                        return;
                    }
                }
            }

            //可能有清理过重复文件，所以这里需要重新获取下版本控制文件
            var versionControlFrom = shaco.HotUpdateHelper.FindVersionControlInPath(fromPath);
            if (null == versionControlFrom)
            {
                shaco.Log.Error("HotUpdateExportWindow+Merge MergeProcess error: not found version control by fromPath=" + fromPath);
                return;
            }

            //合并2个资源目录
            if (!Merge(fromPath, toPath, versionControlFrom, versionControlTo, isCopyManifest, ignoreFolders))
                return;

            //删除临时导出目录
            if (autoDeleteFromPath)
            {
                shaco.Base.FileHelper.DeleteByUserPath(fromPath);
            }

            //删除合并目标目录可能存在的冗余资源
            shaco.HotUpdateHelper.ExecuteVersionControlAPI(toPath, versionControlTo, true, null);
            if (versionControlTo.VersionControlAPI == shaco.HotUpdateDefine.ExportFileAPI.DeleteUnUseFiles)
            {
                var manifestFileName = shaco.Base.FileHelper.GetLastFileName(fromPath) + shaco.HotUpdateDefine.EXTENSION_DEPEND;
                var mainMD5RootPath = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(toPath);
                var manifestPathTo = mainMD5RootPath.ContactPath(manifestFileName);
                shaco.HotUpdateHelper.DeleteUnusedDependenciesInManifest(manifestPathTo, versionControlTo);
            }

            //重新计算上一次导入路径为本次的导出路径
            EditorPrefs.SetString(EditorHelper.GetEditorPrefsKey(HotUpdateVersionImportWindow.PREVIOUS_PATH_KEY), toPath);
        }

        /// <summary>
        /// 合并2个VersionControl文件夹内容
        /// <param name="from">要合并的路径</param>
        /// <param name="to">合并到的路径</param>
        /// <param name="versionControlFrom">合并到的版本控制文件</param>
        /// <param name="versionControlTo">合并到的版本控制文件</param>
        /// <param name="isCopyManifest">是否也拷贝manifest文件</param>
        /// <param name="ignoreFolders">合并过滤目录</param>
        /// </summary>
        private bool Merge(string from, string to, shaco.HotUpdateDefine.SerializeVersionControl versionControlFrom, shaco.HotUpdateDefine.SerializeVersionControl versionControlTo, bool isCopyManifest, string[] ignoreFolders)
        {
            //确保2个合并的资源文件夹内容配置内容完整，以保证合并完整性
            if (!IsVersionControlFolder(from))
            {
                Debug.LogError("HotUpdateExportWindow+Merge Merge erorr: missing some config, please check your from folder path=" + from);
                return false;
            }

            // if (!IsVersionControlFolder(to))
            // {
            //     Debug.LogError("HotUpdateExportWindow+Merge Merge erorr: missing some config, please check your to folder path=" + to);
            //     return;
            // }

            if (null == versionControlFrom || null == versionControlTo)
            {
                Debug.LogError("HotUpdateExportWindow+Merge Merge erorr: version control files is null");
                return false;
            }

            //获取有变化的文件
            List<ChagnedMergeInfo> versionControlChanged = SelectChangedFiles(from, to, versionControlFrom, versionControlTo, ignoreFolders);
            if (versionControlChanged.IsNullOrEmpty())
                return false;

            //获取文件名字格式，目标资源目录是可能不存在的，默认使用md5文件名格式
            var fileNameStatus = FileNameStatus.Missing;
            if (!versionControlTo.ListAssetBundles.IsNullOrEmpty())
            {
                var checkPath = to.ContactPath(shaco.HotUpdateHelper.AssetBundleKeyToPath(versionControlTo.ListAssetBundles[0].AssetBundleName));
                if (shaco.Base.FileHelper.ExistsFile(checkPath))
                    fileNameStatus = FileNameStatus.NoMD5;
            }

            //拷贝文件
            CopyChangedFiles(from, to, fileNameStatus, versionControlChanged, isCopyManifest);

            //重写API方法
            versionControlTo.VersionControlAPI = versionControlFrom.VersionControlAPI;

            //合并manifest，一定要放在MergeVersionControlConfig计算了mainmd5之后
            MergeManifest(from, to, versionControlFrom, versionControlTo);

            //合并version control.json
            var versionControlFileNameTo = shaco.Base.FileHelper.GetLastFileName(to) + shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL;
            MergeVersionControlConfig(to.ContactPath(versionControlFileNameTo), versionControlChanged, versionControlTo);

            //拷贝当前打包日志到目标目录
            var logPathFrom = from.ContactPath(shaco.HotUpdateDefine.BUILD_RESOURCES_LOG_FILE_NAME);
            var logPathTo = to.ContactPath(shaco.HotUpdateDefine.BUILD_RESOURCES_LOG_FILE_NAME);
            shaco.Base.FileHelper.CopyFileByUserPath(logPathFrom, logPathTo);
            return true;
        }

        /// <summary>
        /// 检查该目录是否为资源管理目录
        /// <param name="versionControlRootPath">版本资源根目录</param>
        /// <return>如果有缺少文件或者损坏则返回false，反之true</return>
        /// </summary>
        private bool IsVersionControlFolder(string versionControlRootPath)
        {
            if (!shaco.Base.FileHelper.ExistsDirectory(versionControlRootPath))
            {
                Debug.LogError("HotUpdateExportWindow+Merge IsVersionControlFolder error: not found path=" + versionControlRootPath);
                return false;
            }

            var mainMD5Path = versionControlRootPath.ContactPath(shaco.HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + shaco.HotUpdateDefine.EXTENSION_MAIN_MD5);
            var mainMD5 = shaco.Base.FileHelper.ReadAllByUserPath(mainMD5Path);
            return shaco.HotUpdateHelper.HasAllVersionControlFiles(versionControlRootPath, mainMD5, shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(versionControlRootPath));
        }

        /// <summary>
        /// 筛选出有变化的资源文件
		/// <param name="from">要合并的路径</param>
		/// <param name="to">要合并的路径</param>
        /// <param name="versionControlFrom">要合并的资源配置</param>
        /// <param name="versionControlTo">合并到的资源配置</param>
        /// <param name="forceFileNameStatusFrom">强制使用的要合并的资源配置文件名格式，如果不设置则跟进要合并的目录自动设置格式</param>
        /// <return>有变化的资源文件</return>
        /// </summary>
        private List<ChagnedMergeInfo> SelectChangedFiles(string from, string to,
                                                          shaco.HotUpdateDefine.SerializeVersionControl versionControlFrom,
                                                          shaco.HotUpdateDefine.SerializeVersionControl versionControlTo,
                                                          string[] ignoreFolders)
        {
            var retValue = new List<ChagnedMergeInfo>();
            if (versionControlFrom.ListAssetBundles.Count == 0)
            {
                Debug.LogError("HotUpdateExportWindow+Merge SelectChangedFiles error: no data");
                return retValue;
            }

            //以第一个有效文件作为文件名字格式标准
            FileNameStatus fileNameStatusFrom = FileNameStatus.Missing;
            if (fileNameStatusFrom == FileNameStatus.Missing)
            {
                for (int i = 0; i < versionControlFrom.ListAssetBundles.Count; ++i)
                {
                    fileNameStatusFrom = GetFileNameStatus(from, versionControlFrom.ListAssetBundles[i]);
                    if (FileNameStatus.Missing != fileNameStatusFrom)
                    {
                        break;
                    }
                }
            }
            // var fileNameStatusTo = GetFileNameStatus(to, versionControlTo.ListAssetBundles[0]);

            for (int i = versionControlFrom.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var assetBundleInfoFrom = versionControlFrom.ListAssetBundles[i];
                var assetBundleInfoTo = versionControlTo.ListAssetBundles.Find(ab => ab.AssetBundleName == assetBundleInfoFrom.AssetBundleName);
                if (null == assetBundleInfoTo)
                {
                    assetBundleInfoTo = assetBundleInfoFrom;
                }

                // var checkPathFrom = GetAssetBundleFullPathWithFileNameStatus(from, assetBundleInfoFrom, fileNameStatusFrom);
                var checkPathTo = GetAssetBundleFullPathWithFileNameStatus(to, assetBundleInfoTo, fileNameStatusFrom);

                //目标目录为过滤目录
                if (!ignoreFolders.IsNullOrEmpty())
                {
                    bool isIgnore = false;
                    for (int j = ignoreFolders.Length - 1; j >= 0; --j)
                    {
                        if (checkPathTo.Contains(ignoreFolders[j]))
                        {
                            isIgnore = true;
                            break;
                        }
                    }

                    //跳过本次检测
                    if (isIgnore)
                        continue;
                }

                bool isNeedUpdateAssetBundle = false;

                //目标文件不存在
                if (!shaco.Base.FileHelper.ExistsFile(checkPathTo))
                {
                    isNeedUpdateAssetBundle = true;
                }
                //二者md5不一致，优先使用from的数据覆盖to
                else
                {
                    var md5From = assetBundleInfoFrom.AssetBundleMD5;
                    var md5To = assetBundleInfoTo.AssetBundleMD5;
                    if (md5From != md5To)
                    {
                        isNeedUpdateAssetBundle = true;
                    }
                    //当md5也一致的时候，如果二者文件导出格式不一致也判定为需要更新内容
                    else
                    {
                        var isOriginalFileFrom = assetBundleInfoFrom.exportFormat == shaco.HotUpdateDefine.ExportFileFormat.OriginalFile;

                        //部分特殊资源Unity需要强制导出为assetbundle文件才能读取
                        var isOriginalFileTo = shaco.HotUpdateHelper.IsKeepOriginalFile(checkPathTo);
                        if (isOriginalFileFrom != isOriginalFileTo)
                        {
                            isNeedUpdateAssetBundle = true;
                        }
                    }
                }

                if (isNeedUpdateAssetBundle)
                {
                    retValue.Add(new ChagnedMergeInfo()
                    {
                        exportInfoFrom = assetBundleInfoFrom,
                        exportInfoTo = assetBundleInfoTo
                    });
                }
            }

            if (retValue.Count == 0)
            {
                Debug.LogError("HotUpdateExportWindow+Merge Merge erorr: no data need merge");
                return retValue;
            }
            return retValue;
        }

        /// <summary>
        /// 拷贝有变化的资源配置文件到指定目录
		/// <param name="from">要合并的路径</param>
		/// <param name="to">要合并的路径</param>
		/// <param name="fileNameStatusTo">合并到的目录文件名字格式</param>
        /// <param name="versionControlChanged">有更新的资源文件表</param>
        /// <param name="isCopyManifest">是否也拷贝manifest文件</param>
        /// </summary>
        private void CopyChangedFiles(string from, string to, FileNameStatus fileNameStatusTo, List<ChagnedMergeInfo> versionControlChanged, bool isCopyManifest)
        {
            if (versionControlChanged.Count == 0)
            {
                Debug.LogError("HotUpdateExportWindow+Merge CopyChangedFiles error: no data need copy");
                return;
            }

            //以第一个文件作为文件名字格式标准
            var fileNameStatusFrom = GetFileNameStatus(from, versionControlChanged[0].exportInfoFrom);

            if (fileNameStatusTo == FileNameStatus.Missing)
            {
                fileNameStatusTo = fileNameStatusFrom;
            }

            for (int i = versionControlChanged.Count - 1; i >= 0; --i)
            {
                var copyPathFrom = GetAssetBundleFullPathWithFileNameStatus(from, versionControlChanged[i].exportInfoFrom, fileNameStatusFrom);
                var copyPathTo = GetAssetBundleFullPathWithFileNameStatus(to, versionControlChanged[i].exportInfoFrom, fileNameStatusTo);
                var copyManifestPathFrom = GetManifestFullPath(from, versionControlChanged[i].exportInfoFrom);
                var copyManifestPathTo = GetManifestFullPath(to, versionControlChanged[i].exportInfoFrom);
                var deletePathTo = GetAssetBundleFullPathWithFileNameStatus(to, versionControlChanged[i].exportInfoTo, fileNameStatusTo);
                var deleteManifestPathFrom = GetManifestFullPath(to, versionControlChanged[i].exportInfoTo);

                //delete old assetbundle
                if (shaco.Base.FileHelper.ExistsFile(deletePathTo))
                    shaco.Base.FileHelper.DeleteByUserPath(deletePathTo);

                //delete old manifest
                if (shaco.Base.FileHelper.ExistsFile(deleteManifestPathFrom))
                    shaco.Base.FileHelper.DeleteByUserPath(deleteManifestPathFrom);

                //copy assetbundle
                shaco.Base.FileHelper.CopyFileByUserPath(copyPathFrom, copyPathTo);

                //copy manifest
                if (isCopyManifest)
                {
                    if (shaco.Base.FileHelper.ExistsFile(copyManifestPathFrom))
                        shaco.Base.FileHelper.CopyFileByUserPath(copyManifestPathFrom, copyManifestPathTo);
                }
                else
                {
                    if (shaco.Base.FileHelper.ExistsFile(copyManifestPathTo))
                        shaco.Base.FileHelper.DeleteByUserPath(copyManifestPathTo);
                }
            }
        }

        /// <summary>
        /// 是否为名字格式化状态
        /// <param name="pathRoot">资源根目录</param>
        /// <param name="exportAssetBundleInfo">导出资源信息</param>
        /// <return>文件名格式化状态</return>
        /// </summary>
        private FileNameStatus GetFileNameStatus(string pathRoot, shaco.HotUpdateDefine.ExportAssetBundle exportAssetBundleInfo)
        {
            FileNameStatus retValue = FileNameStatus.Missing;
            var checkPath = pathRoot.ContactPath(shaco.HotUpdateHelper.AssetBundleKeyToPath(exportAssetBundleInfo.AssetBundleName));
            var checkMD5Path = string.Empty;
            if (shaco.Base.FileHelper.ExistsFile(checkPath))
            {
                retValue = FileNameStatus.NoMD5;
            }
            else
            {
                checkMD5Path = shaco.HotUpdateHelper.AddAssetBundleNameTag(checkPath, exportAssetBundleInfo.AssetBundleMD5);
                if (shaco.Base.FileHelper.ExistsFile(checkMD5Path))
                {
                    retValue = FileNameStatus.WithMD5;
                }
            }

            if (retValue == FileNameStatus.Missing)
            {
                Debug.LogError("HotUpdateExportWindow+Merge GetFileNameStatus error: not found path=" + checkPath + "\n or md5 path=" + checkMD5Path);
            }

            return retValue;
        }

        /// <summary>
        /// 根据文件名字格式化状态获取资源名字
        /// <param name="pathRoot">资源根目录</param>
		/// <param name="exportInfo">导出资源信息</param>
        /// <param name="status">文件名字格式化状态</param>
        /// <return></return>
        /// </summary>
        private string GetAssetBundleFullPathWithFileNameStatus(string pathRoot, shaco.HotUpdateDefine.ExportAssetBundle exportInfo, FileNameStatus status)
        {
            var retValue = string.Empty;
            switch (status)
            {
                case FileNameStatus.NoMD5:
                    {
                        retValue = pathRoot.ContactPath(shaco.HotUpdateHelper.AssetBundleKeyToPath(exportInfo.AssetBundleName));
                        break;
                    }
                case FileNameStatus.WithMD5:
                    {
                        retValue = pathRoot.ContactPath(shaco.HotUpdateHelper.AssetBundleKeyToPath(exportInfo.AssetBundleName));
                        retValue = shaco.HotUpdateHelper.AddAssetBundleNameTag(retValue, exportInfo.AssetBundleMD5);
                        break;
                    }
                default: Debug.LogError("HotUpdateExportWindow+Merge GetAssetBundleFullPathWithFileNameStatus error: unsupport type=" + status); break;
            }

            return retValue;
        }

        /// <summary>
        /// 获取assetbundle依赖文件的全路径
        /// <param name="pathRoot">资源根目录</param>
		/// <param name="exportInfo">导出资源信息</param>
        /// <return>依赖文件路径</return>
        /// </summary>
        private string GetManifestFullPath(string pathRoot, shaco.HotUpdateDefine.ExportAssetBundle exportInfo)
        {
            var retValue = pathRoot.ContactPath(shaco.HotUpdateHelper.AssetBundleKeyToPath(exportInfo.AssetBundleName));
            retValue = shaco.Base.FileHelper.AddExtensions(retValue, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE_MANIFEST);
            return retValue;
        }

        /// <summary>
        /// 合并版本控制配置文件
        /// <param name="versionControlPathTo">合并到的资源配置路径</param>
        /// <param name="versionControlChanged">有变化的文件</param>
        /// <param name="versionControlTo">合并到的资源配置</param>
        /// </summary>
        private void MergeVersionControlConfig(string versionControlPathTo, List<ChagnedMergeInfo> versionControlChanged, shaco.HotUpdateDefine.SerializeVersionControl versionControlTo)
        {
            for (int i = versionControlChanged.Count - 1; i >= 0; --i)
            {
                var chagnedMergeInfo = versionControlChanged[i];

                //如果导入和导出信息相同，则视为导出信息中没有该文件配置
                if (chagnedMergeInfo.exportInfoFrom == chagnedMergeInfo.exportInfoTo)
                {
                    versionControlTo.ListAssetBundles.Add(chagnedMergeInfo.exportInfoFrom);
                }
                //导出信息有有该文件配置，则更新导出信息
                else
                {
                    chagnedMergeInfo.exportInfoTo.CopyFrom(chagnedMergeInfo.exportInfoFrom);
                }
            }

            //重新计算main md5并写入文件
            var pathRoot = shaco.Base.FileHelper.GetFolderNameByPath(versionControlPathTo);
            var mainMD5 = ComputerAndWriteMainMD5(pathRoot, versionControlTo);

            //重新计算总文件大小
            ComputerAllDataSize(pathRoot, versionControlTo);

            //重新写入版本控制配置文件
            shaco.HotUpdateHelper.SaveVersionControlFiles(shaco.Base.FileHelper.AddFolderNameByPath(versionControlPathTo, mainMD5), versionControlTo);
        }

        /// <summary>
        /// 合并依赖关系文件
        /// <param name="from">要合并的路径</param>
        /// <param name="to">合并到的路径</param>
        /// <param name="versionControlFrom">要合并的版本控制文件</param>
        /// <param name="versionControlTo">合并到的版本控制文件</param>
        /// </summary>
        private void MergeManifest(string from, string to, shaco.HotUpdateDefine.SerializeVersionControl versionControlFrom, shaco.HotUpdateDefine.SerializeVersionControl versionControlTo)
        {
            var manifestFileName = shaco.Base.FileHelper.GetLastFileName(from) + shaco.HotUpdateDefine.EXTENSION_DEPEND;

            from = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(from);
            to = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(to);

            var manifestPathFrom = from.ContactPath(manifestFileName);
            var manifestPathTo = to.ContactPath(manifestFileName);

            var manifestInfoFrom = new shaco.HotUpdateManifestInfo();
            manifestInfoFrom.LoadFromPath(manifestPathFrom);
            var manifestInfoTo = new shaco.HotUpdateManifestInfo();

            if (shaco.Base.FileHelper.ExistsFile(manifestPathTo))
                manifestInfoTo.LoadFromPath(manifestPathTo);
            else
                Debug.LogError("HotUpdateExportWindow+Merge MergeManifest error: not found manifestPathTo=" + manifestPathTo);

            manifestInfoFrom.MergeTo(manifestInfoTo);

            //重新写入依赖关系文件
            shaco.HotUpdateManifestInfo.SaveToFile(manifestPathTo, versionControlTo, manifestInfoTo);
        }
    }
}