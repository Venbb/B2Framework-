using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class HotUpdateVersionImportWindow
    {
        public const string PREVIOUS_PATH_KEY = "PreviousConfigImportPath";

        public static void GetConfigByVersionControl(HotUpdateExportWindow windowExport)
        {
            // var pathFolder = EditorUtility.OpenFolderPanel("Select a version control folder", string.Empty, string.Empty);
            HotUpdateImportHistoryWindow.OpenHoUpdateImportHistoryWindow(windowExport, (string selectPath)=>
            {
                if (!string.IsNullOrEmpty(selectPath))
                {
                    return GetConfigByVersionControl(selectPath, windowExport);
                }
                else 
                    return false;
            });
        }

        public static void GetPreviousConfigByVersionControl(HotUpdateExportWindow windowExport)
        {
            var previousConfigImportPath = EditorPrefs.GetString(EditorHelper.GetEditorPrefsKey(PREVIOUS_PATH_KEY), string.Empty);
            if (string.IsNullOrEmpty(previousConfigImportPath))
            {
                GetConfigByVersionControl(windowExport);
            }
            else
            {
                var splitString = shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING;
                previousConfigImportPath = previousConfigImportPath.RemoveRangeIfHave(previousConfigImportPath.Length - splitString.Length - 1, previousConfigImportPath.Length - 1, splitString);
                GetConfigByVersionControl(previousConfigImportPath, windowExport);
            }
        }

        /// <summary>
        /// 获取版本控制文件，并检测有效性
        /// </summary>
        static public string FindVersionControlWithRecursion(string pathFolder, HotUpdateExportWindow windowExport)
        {
            var retValue = string.Empty;

            windowExport.CheckRootPathValidWithPlatform(pathFolder);

            if (!shaco.Base.FileHelper.ExistsDirectory(pathFolder))
            {
                EditorPrefs.DeleteKey(EditorHelper.GetEditorPrefsKey(PREVIOUS_PATH_KEY));
                return retValue;
            }

            var searchFolderPath = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(pathFolder);
            var allFiles = shaco.Base.FileHelper.GetFiles(searchFolderPath, "*" + shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL);

            if (allFiles.IsNullOrEmpty())
            {
                throw new System.Exception("HotUpdateVersionImportWindow FindVersionControlWithRecursion error: not found file in path=" + searchFolderPath);
            }

            for (int i = 0; i < allFiles.Length; ++i)
            {
                if (allFiles[i].Contains(shaco.HotUpdateDefine.VERSION_CONTROL))
                {
                    retValue = allFiles[i];
                    break;
                }
            }

            if (string.IsNullOrEmpty(retValue))
            {
                return retValue;
            }

            windowExport.versionControlConfig = shaco.HotUpdateHelper.PathToVersionControl(retValue);
            windowExport.versionControlConfigOld = shaco.HotUpdateHelper.PathToVersionControl(retValue);
            return retValue;
        }

        static public bool CheckVersionControlValid(shaco.HotUpdateDefine.SerializeVersionControl versionControl, HotUpdateExportWindow windowExport)
        {
            bool retValue = false;
            if (versionControl == null)
            {
                Debug.LogError("HotUpdateVersionImportWindow CheckVersionControlValid error: Not found version control file, please check your unity version or path is valid !");
                return retValue;
            }

            if (versionControl.FileTag != shaco.HotUpdateDefine.VERSION_CONTROL)
            {
                Debug.LogError("HotUpdateVersionImportWindow CheckVersionControlValid error: Not found version control tag");
                return retValue;
            }

            if (versionControl.UnityVersion != Application.unityVersion)
            {
                Debug.LogWarning("HotUpdateVersionImportWindow CheckVersionControlValid error: opened with different unity version, package on in original file mode");
            }

            retValue = true;
            return retValue;
        }

        public static bool GetConfigByVersionControl(string pathFolder, HotUpdateExportWindow windowExport)
        {
            var verionControlConfigPath = FindVersionControlWithRecursion(pathFolder, windowExport);
            if (string.IsNullOrEmpty(verionControlConfigPath) || !CheckVersionControlValid(windowExport.versionControlConfig, windowExport))
            {
                windowExport.versionControlConfig = null;
                windowExport.versionControlConfigOld = null;
                return false;
            }

            var platformTarget = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(verionControlConfigPath);
            int splitLength = shaco.Base.FileDefine.PATH_FLAG_SPLIT_STRING.Length;
            windowExport.currentRootPath = pathFolder;

            //set all assetbundle
            for (int i = windowExport.versionControlConfig.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var assetBundleTmp = windowExport.versionControlConfig.ListAssetBundles[i];

                if (windowExport.mapAssetbundlePath.ContainsKey(assetBundleTmp.AssetBundleName))
                    continue;

                //没有ab包信息直接退出
                if (0 == assetBundleTmp.ListFiles.Count)
                {
                    Debug.LogError("HotUpdateVersionImportWindow GetConfigByVersionControl error: no file in assetbundle, name=" + assetBundleTmp.AssetBundleName);
                    continue;
                }

                var selectFiles = new HotUpdateExportWindow.SelectFile();
                selectFiles.AssetBundleMD5 = assetBundleTmp.AssetBundleMD5;
                selectFiles.exportFormat = assetBundleTmp.exportFormat;

                string keyCheck = shaco.HotUpdateHelper.AssetBundleKeyToPath(assetBundleTmp.AssetBundleName);
                keyCheck = shaco.HotUpdateHelper.ResetAsssetBundleFileName(keyCheck, string.Empty, platformTarget);
                
                for (int j = 0; j < assetBundleTmp.ListFiles.Count; ++j)
                {
                    var fileName = assetBundleTmp.ListFiles[j];
                    var convertFileName = fileName.Key;
                    var fullFilePathTmp = EditorHelper.GetFullPath(convertFileName);

                    if (!shaco.Base.FileHelper.ExistsFile(fullFilePathTmp))
                    {
                        shaco.Log.Error("not found file path=" + fullFilePathTmp + " in assetbundle name=" + keyCheck);
                        continue;
                    }

                    selectFiles.ListAsset.Add(convertFileName, new HotUpdateExportWindow.SelectFile.FileInfo(convertFileName));

                    if (!windowExport.mapAllExportAssetSameKeyCheck.ContainsKey(convertFileName))
                        windowExport.mapAllExportAssetSameKeyCheck.Add(convertFileName, shaco.HotUpdateHelper.AssetBundleKeyToPath(assetBundleTmp.AssetBundleName));
                }

                //没有ab包信息直接退出，可能因为源文件丢失了
                if (0 == selectFiles.ListAsset.Count)
                {
                    continue;
                }

                if (!windowExport.mapAssetbundlePath.ContainsKey(keyCheck))
                    windowExport.mapAssetbundlePath.Add(keyCheck, selectFiles);
            }

            EditorPrefs.SetString(EditorHelper.GetEditorPrefsKey(PREVIOUS_PATH_KEY), pathFolder);
            windowExport.UpdateDatas();

            return true;
        }
    }
}