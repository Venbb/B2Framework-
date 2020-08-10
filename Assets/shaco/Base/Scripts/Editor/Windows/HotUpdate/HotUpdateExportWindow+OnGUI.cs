using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class HotUpdateExportWindow : EditorWindow
    {
#if !UNITY_5_3_OR_NEWER
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_COLLECT_DEPENDENCIES = new GUIContent(BuildAssetBundleOptions.CollectDependencies.ToString(), "包括所有依赖关系的资源 （AssetBundle中所有类型的hash，该类型在Unity5.3以后版本一直生效");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_COMPLETE_ASSETS = new GUIContent(BuildAssetBundleOptions.CompleteAssets.ToString(), "强制包含所有关联资源并打包到AssetBundle中，该类型在Unity5.3以后版本一直生效");
#endif
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_UNCOMPRESSED_ASSETBUNDLE = new GUIContent(BuildAssetBundleOptions.UncompressedAssetBundle.ToString(), "不进行数据压缩。没有压缩/解压的过程，AssetBundle的发布和加载会更快，但是AssetBundle也会更大，导致下载速度变慢。");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_DETERMINSTIC_ASSETBUNDLE = new GUIContent(BuildAssetBundleOptions.DeterministicAssetBundle.ToString(), "使每个Object具有唯一不变的hash ID，使用ID可避免资源改名、移动位置等导致重新导出。 可用于增量式发布AssetBundle。");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_FORCE_REBUILD_ASSETBUNDLE = new GUIContent(BuildAssetBundleOptions.ForceRebuildAssetBundle.ToString(), "强制重新Build所有的AssetBundle。");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_CHUNK_BASED_COMPRESSION = new GUIContent(BuildAssetBundleOptions.ChunkBasedCompression.ToString(), "用LZ4压缩算法来Build AssetBundle。默认LZMA压缩。");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_STRICT_MODE = new GUIContent(BuildAssetBundleOptions.StrictMode.ToString(), "使用严格模式打包，如果发布过程中有任何错误报告就不允许发布成功。");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_DRY_RUN_BUILD = new GUIContent(BuildAssetBundleOptions.DryRunBuild.ToString(), "获得打包一个AssetBundle的AssetBundleManifest，AssetBundleManifest对象包含有效AssetBundle依赖性和散列。但是不实际创建AssetBundle。");
        private readonly GUIContent GUI_CONTENT_BUILD_OPTION_NONE = new GUIContent(BuildAssetBundleOptions.None.ToString(), "构建AssetBundle没有任何特殊的选项，使用Unity默认打包配置");

        private void DrawSettings()
        {
            _isShowSettings.target = EditorHelper.Foldout(_isShowSettings.target, "Settings");
            if (EditorGUILayout.BeginFadeGroup(_isShowSettings.faded))
            {
                if (!string.IsNullOrEmpty(currentRootPath))
                {
                    GUILayout.BeginVertical("box");
                    {
                        GUI.changed = false;
                        currentRootPath = GUILayoutHelper.PathField("Root path", currentRootPath, string.Empty, currentRootPath);
                        if (GUI.changed)
                        {
                            currentRootPath = shaco.HotUpdateHelper.CheckAutoAddVersionControlFolder(currentRootPath);
                        }
                        versionControlConfig.AutoEncryt = EditorGUILayout.Toggle("Auto encrypt", versionControlConfig.AutoEncryt);
                        versionControlConfig.AutoOutputBuildLog = EditorGUILayout.Toggle("Auto output build log", versionControlConfig.AutoOutputBuildLog);
                        versionControlConfig.AutoCompressdFile = EditorGUILayout.Toggle("Auto compressed", versionControlConfig.AutoCompressdFile);
                        versionControlConfig.VersionControlAPI = (shaco.HotUpdateDefine.ExportFileAPI)EditorGUILayout.EnumPopup("Export api", (System.Enum)versionControlConfig.VersionControlAPI);
                    }
                    GUILayout.EndVertical();
                }

                DrawVersionSettings();
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawExecuteButtons()
        {
            _isShowBaseButton.target = EditorHelper.Foldout(_isShowBaseButton.target, "Execute");
            if (EditorGUILayout.BeginFadeGroup(_isShowBaseButton.faded))
            {
                if (EditorApplication.isCompiling)
                {
                    EditorGUILayout.HelpBox("Please wait for compiling", MessageType.Info);
                }
                EditorGUI.BeginDisabledGroup(EditorApplication.isCompiling);
                {
                    if (GUILayout.Button("Import"))
                    {
                        HotUpdateVersionImportWindow.GetConfigByVersionControl(_currentWindow);
                    }

                    if (mapAssetbundlePath.Count == 0 && GUILayout.Button("Merge"))
                    {
                        var mergeFromPath = EditorUtility.OpenFolderPanel("Select merge from folder", Application.dataPath, string.Empty);
                        if (!string.IsNullOrEmpty(mergeFromPath))
                        {
                            var mergeToPath = EditorUtility.OpenFolderPanel("Select merge to folder", mergeFromPath.ContactPath("../"), string.Empty);
                            if (!string.IsNullOrEmpty(mergeToPath))
                                MergeProcess(mergeFromPath, mergeToPath, false, true);
                        }
                    }

                    if (mapAssetbundlePath.Count > 0)
                    {
                        if (GUILayout.Button("Clear"))
                        {
                            if (EditorUtility.DisplayDialog("Warning", "Clear all export config ?", "Confirm", "Canel"))
                            {
                                OnClearButtonClick();
                            }
                        }

                        if (GUILayout.Button("Export", GUILayout.Height(50)))
                        {
                            ExecuteExportProcess();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawTestButtons()
        {
            _isShowTestButton.target = EditorHelper.Foldout(_isShowTestButton.target, "Test");
            if (EditorGUILayout.BeginFadeGroup(_isShowTestButton.faded))
            {
                if (GUILayout.Button("Assetbundle Preview"))
                {
                    AssetBundlePreviewWindow.OpenAssetbundleCachePreviewWindow();
                }

                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("DecryptFolder"))
                    {
                        var pathTmp = EditorUtility.OpenFolderPanel("Open Export Folder", string.IsNullOrEmpty(_printPath) ? Application.dataPath : _printPath, string.Empty);
                        DecryptExportFolder(pathTmp);
                    }

                    if (GUILayout.Button("DecryptFile"))
                    {
                        var extensionConvert = shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE.RemoveRangeIfHave(0, 1, shaco.Base.FileDefine.DOT_SPLIT_STRING);
                        var pathTmp = EditorUtility.OpenFilePanel("Select a assetbundle", string.IsNullOrEmpty(_decrptAssetBundlePath) ? Application.dataPath : _decrptAssetBundlePath, extensionConvert);
                        DecryptAndOverrideAssetBundle(pathTmp);
                    }
                }
                GUILayout.EndHorizontal();

                if (mapAssetbundlePath.Count > 0 && GUILayout.Button("Check And Fix Missing Files"))
                {
                    CheckAndFixMissingFiles();
                }

                // if (GUILayout.Button("Print MainMD5"))
                // {
                //     var versionControlFolderPath = EditorUtility.OpenFolderPanel(Application.dataPath, string.Empty, string.Empty);
                //     if (!string.IsNullOrEmpty(versionControlFolderPath))
                //     {
                //         var versionControlTmp = shaco.HotUpdateHelper.FindVersionControlInPath(versionControlFolderPath);
                //         if (null == versionControlTmp)
                //         {
                //             Debug.LogError("HotUpdateExportWindow+OnGUI Print MainMD5 error: not found version control in path=" + versionControlFolderPath);
                //         }
                //         else
                //         {
                //             var mainMD5Tmp = ComputerMainMD5(versionControlTmp);
                //             Debug.Log("main md5=" + mainMD5Tmp);
                //         }
                //     }
                // }
            }
            EditorGUILayout.EndFadeGroup();
        }

        private SelectFile.FileInfo[] GetCurrentSelection()
        {
            SelectFile.FileInfo[] retValue = null;
            var currentSelectAsset = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            if (!currentSelectAsset.IsNullOrEmpty())
            {
                retValue = new SelectFile.FileInfo[currentSelectAsset.Length];
                for (int i = 0; i < currentSelectAsset.Length; ++i)
                {
                    retValue[i] = new SelectFile.FileInfo(AssetDatabase.GetAssetPath(currentSelectAsset[i]));
                }
            }
            return retValue;
        }

        private void DrawVersionSettings()
        {
            GUILayout.BeginVertical("box");
            {
                if (null == _buildVersionAsset)
                {
                    EditorGUILayout.HelpBox("If the version file is null, 'Application.version' is used by default", MessageType.Warning);
                }

                // GUI.changed = false;
                EditorGUI.BeginDisabledGroup(true);
                {
                    _buildVersionAsset = EditorGUILayout.ObjectField("Version Asset", _buildVersionAsset, typeof(TextAsset), true) as TextAsset;
                }
                EditorGUI.EndDisabledGroup();
                // if (GUI.changed)
                // {
                //     SetVersionAutomatic();
                // }

                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.LabelField("Version", GetResourceVersion());
                    EditorGUILayout.LabelField("Platform", shaco.HotUpdateHelper.GetAssetBundleAutoPlatform().ToString());
                }
                EditorGUI.EndDisabledGroup();
                versionControlConfig.baseVersion = (shaco.HotUpdateDefine.BaseVersionType)EditorGUILayout.EnumPopup("Build Resources Version", versionControlConfig.baseVersion);
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.PrefixLabel("Build Assetbundle Options");

#if !UNITY_5_3_OR_NEWER
                //在Unity5.3以后的打包系统中，下面两个参数是一直生效的
                GUILayout.BeginHorizontal();
                {
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_COLLECT_DEPENDENCIES, BuildAssetBundleOptions.CollectDependencies);
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_COMPLETE_ASSETS, BuildAssetBundleOptions.CompleteAssets);
                }
                GUILayout.EndHorizontal();
#endif
                GUILayout.BeginHorizontal();
                {
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_UNCOMPRESSED_ASSETBUNDLE, BuildAssetBundleOptions.UncompressedAssetBundle);
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_DETERMINSTIC_ASSETBUNDLE, BuildAssetBundleOptions.DeterministicAssetBundle);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_FORCE_REBUILD_ASSETBUNDLE, BuildAssetBundleOptions.ForceRebuildAssetBundle);
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_CHUNK_BASED_COMPRESSION, BuildAssetBundleOptions.ChunkBasedCompression);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_STRICT_MODE,  BuildAssetBundleOptions.StrictMode);
                    DrawBuildAssetbundleOption(GUI_CONTENT_BUILD_OPTION_DRY_RUN_BUILD, BuildAssetBundleOptions.DryRunBuild);
                }
                GUILayout.EndHorizontal();

                if (GUILayout.Button(GUI_CONTENT_BUILD_OPTION_NONE, EditorStyles.toolbarButton))
                {
                    _buildAssetbundleOptions = BuildAssetBundleOptions.None;
                }

                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                //打包参数中切记不可以使用 DisableWriteTypeTree 和 IgnoreTypeTreeChanges ，否则会有以下问题：
                //当预制体中的脚本所在Assembly发生变化后，unity无法更新AssetFileHash
                //而是更新了TypeTreeHash，因此忽略TypeTreeHash会导致assetbundle无法正常刷新md5
                //ps: 目前assetbundle计算规则是根据manifest文件的md5来决定的
                //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

                // DrawBuildAssetbundleOption(BuildAssetBundleOptions.DisableWriteTypeTree);
                // DrawBuildAssetbundleOption(BuildAssetBundleOptions.IgnoreTypeTreeChanges);
                // DrawBuildAssetbundleOption(BuildAssetBundleOptions.AppendHashToAssetBundleName);
                // DrawBuildAssetbundleOption(BuildAssetBundleOptions.DisableLoadAssetByFileName);
                // DrawBuildAssetbundleOption(BuildAssetBundleOptions.DisableLoadAssetByFileNameWithExtension);
            }
            GUILayout.EndVertical();
        }

        private void DrawBuildAssetbundleOption(GUIContent content, BuildAssetBundleOptions option)
        {
            var toogleTmp = 0 != (_buildAssetbundleOptions & option);
            var newToogle = EditorGUILayout.Toggle(content, toogleTmp);
            if (toogleTmp != newToogle)
            {
                if (newToogle)
                    _buildAssetbundleOptions |= option;
                else
                    _buildAssetbundleOptions &= ~option;
            }
        }

        private void DecryptAndOverrideAssetBundle(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!shaco.Base.EncryptDecrypt.IsEncryptionPath(filePath))
                {
                    Debug.LogError("not a encrypted assetbundle !\n" + filePath);
                }
                else
                {
                    shaco.HotUpdateHelper.CheckCompressedFileAndOverwrite(filePath);
                    var readBytes = shaco.Base.EncryptDecrypt.DecryptPath(filePath);
                    shaco.Base.FileHelper.WriteAllByteByUserPath(filePath, readBytes);

                    EditorHelper.ShowInFolder(filePath);

                    _decrptAssetBundlePath = shaco.Base.FileHelper.GetFolderNameByPath(filePath);
                }
            }
        }

        private void DecryptExportFolder(string folderPath)
        {
            if (!string.IsNullOrEmpty(folderPath))
            {
                int selectMode = EditorUtility.DisplayDialogComplex("Information", "Select a mode", "WriteToTemp", "Cancel", "Replace");

                //取消
                if (1 == selectMode)
                {
                    return;
                }

                var fileNameCheck = shaco.Base.FileHelper.GetLastFileName(folderPath);
                if (fileNameCheck.Contains(shaco.HotUpdateDefine.VERSION_CONTROL))
                {
                    var platform = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(folderPath);
                    var allFiles = new List<string>();

                    var sourceVersionControlFolderName = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);
                    var copyVersionControlFolderPath = string.Empty;

                    //写入下载文件夹到临时文件夹
                    if (0 == selectMode)
                    {
                        copyVersionControlFolderPath = folderPath.Replace(sourceVersionControlFolderName, "/CopyDownload/" + sourceVersionControlFolderName);
                    }
                    //写入并替换下载文件夹到本地下载文件夹
                    else if (2 == selectMode)
                    {
                        var fullPath = shaco.Base.FileDefine.persistentDataPath.ContactPath(sourceVersionControlFolderName);
                        copyVersionControlFolderPath = fullPath;
                    }

                    shaco.Base.FileHelper.GetSeekPath(folderPath.ContactPath("assets"), ref allFiles, "assetbundle");

                    //delete old version control folder
                    if (shaco.Base.FileHelper.ExistsDirectory(copyVersionControlFolderPath))
                    {
                        shaco.Base.FileHelper.DeleteByUserPath(copyVersionControlFolderPath);
                    }

                    //copy assetbundles
                    for (int i = allFiles.Count - 1; i >= 0; --i)
                    {
                        var relativeAssetbundlePath = allFiles[i].RemoveFront(folderPath);
                        relativeAssetbundlePath = shaco.HotUpdateHelper.RemoveAssetBundleFileNameMD5(relativeAssetbundlePath);
                        var saveTmpFilePath = copyVersionControlFolderPath.ContactPath(relativeAssetbundlePath);
                        shaco.Base.FileHelper.CopyFileByUserPath(allFiles[i], saveTmpFilePath);
                    }

                    //copy version control configs
                    var mainMD5PathFrom = folderPath.ContactPath(shaco.HotUpdateDefine.VERSION_CONTROL_MAIN_MD5 + shaco.HotUpdateDefine.EXTENSION_MAIN_MD5);
                    var mainMD5RootPath = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(folderPath);

                    var versionControlName = shaco.HotUpdateHelper.GetVersionControlFolder(string.Empty, string.Empty, platform);
                    var versionJsonFrom = mainMD5RootPath.ContactPath(versionControlName + shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL);
                    var versionJsonTo = folderPath.ContactPath(versionControlName + shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL);
                    var versionDependFrom = mainMD5RootPath.ContactPath(versionControlName + shaco.HotUpdateDefine.EXTENSION_DEPEND);
                    var versionDependTo = folderPath.ContactPath(versionControlName + shaco.HotUpdateDefine.EXTENSION_DEPEND);
                    var versionFileListFrom = mainMD5RootPath.ContactPath(versionControlName + shaco.HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME);
                    var versionFileListTo = folderPath.ContactPath(versionControlName + shaco.HotUpdateDefine.EXTENSION_FILE_LIST_RUNTIME);

                    shaco.Base.FileHelper.CopyFileByUserPath(mainMD5PathFrom, copyVersionControlFolderPath.ContactPath(shaco.Base.FileHelper.GetLastFileName(mainMD5PathFrom)));
                    shaco.Base.FileHelper.CopyFileByUserPath(versionJsonFrom, copyVersionControlFolderPath.ContactPath(shaco.Base.FileHelper.GetLastFileName(versionJsonFrom)));
                    shaco.Base.FileHelper.CopyFileByUserPath(versionDependFrom, copyVersionControlFolderPath.ContactPath(shaco.Base.FileHelper.GetLastFileName(versionDependFrom)));
                    shaco.Base.FileHelper.CopyFileByUserPath(versionFileListFrom, copyVersionControlFolderPath.ContactPath(shaco.Base.FileHelper.GetLastFileName(versionFileListFrom)));

                    System.Diagnostics.Process.Start(copyVersionControlFolderPath);
                    _printPath = shaco.Base.FileHelper.GetFolderNameByPath(folderPath);
                }
                else
                {
                    Debug.LogError("HotUpdateExportWindow ConvertToDownloadedResources error: not a version control folder=" + folderPath);
                }
            }
        }
    }
}
