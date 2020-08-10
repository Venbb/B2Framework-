using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Linq;

namespace shacoEditor
{
    public class FileToolsWindow : EditorWindow
    {
        private class FileSizeInfo
        {
            public Object asset = null;
            public long fileSize = 0;
        }

        private Object _objPrevSelect = null;
        private string _strGetMD5 = string.Empty;
        private string _strSelectPath = string.Empty;
        private List<string> _listDeleteFileTag = new List<string>();
        private bool _isFolder = false;
        private List<FileSizeInfo> _listFileSize = new List<FileSizeInfo>();
        private long _lTotalSize = 0;
        private Vector2 _vec2ScrollPos = Vector2.zero;
        private string _inputNewFileNameExtensions = string.Empty;
        private Object[] _currentSelectObjects = null;

        private FileToolsWindow _currentWindow = null;

        [MenuItem("shaco/Tools/FileTools", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.FILE_TOOLS)]
        static FileToolsWindow OpenFileToolsWindow()
        {
            var retValue = EditorHelper.GetWindow<FileToolsWindow>(null, true, "FileTools");
            retValue.Init();
            retValue.Show();
            return retValue;
        }

        [MenuItem("shaco/OpenFolder/DataPath &1")]
        static void OpenDataPath()
        {
            EditorHelper.ShowInFolder(Application.dataPath);
        }

        [MenuItem("shaco/OpenFolder/PersistentDataPath &2")]
        static void OpenPersistentDataPath()
        {
            var allFiles = shaco.Base.FileHelper.GetFiles(Application.persistentDataPath);

            if (allFiles.IsNullOrEmpty())
                EditorHelper.ShowInFolder(Application.persistentDataPath);
            else
                EditorHelper.ShowInFolder(allFiles[0]);
        }

        [MenuItem("shaco/OpenFolder/StreamingAssetsPath &3")]
        static void OpenStreamingAssetsPath()
        {
            if (shaco.Base.FileHelper.ExistsDirectory(Application.streamingAssetsPath))
                EditorHelper.ShowInFolder(Application.streamingAssetsPath);
        }

        [MenuItem("shaco/OpenFolder/TemporaryCachePath &4")]
        static void OpenTemporaryCachePath()
        {
            if (shaco.Base.FileHelper.ExistsDirectory(Application.temporaryCachePath))
                EditorHelper.ShowInFolder(Application.temporaryCachePath);
        }

        [MenuItem("shaco/OpenFolder/ShacoFrameworkPath &5")]
        static void OpenShacoFrameworkPath()
        {
            EditorHelper.ShowInFolder(Application.dataPath + "/shaco");
        }


        [MenuItem("shaco/OpenFolder/DownloadResourcesPath &6")]
        static void OpenDownloadResourcesPath()
        {
            EditorHelper.ShowInFolder(GetDownloadResourcesPath());
        }

        [MenuItem("shaco/OpenFolder/DownloadResourcesPath &6", true)]
        static bool OpenDownloadResourcesPathValid()
        {
            return shaco.Base.FileHelper.ExistsDirectory(GetDownloadResourcesPath());
        }

        [MenuItem("shaco/Tools/ReverseHierarchyGameObjectActive _F2", false, (int)ToolsGlobalDefine.MenuPriority.Tools.REVERSE_HIERARCHY_GAMEOBJECT_ACTIVE)]
        static void ReverseHierarchyGameObjectActive()
        {
            var selectGameObjects = Selection.gameObjects;
            if (selectGameObjects.IsNullOrEmpty())
                return;

            for (int i = selectGameObjects.Length - 1; i >= 0; --i)
            {
                var objTmp = selectGameObjects[i];
                if (!AssetDatabase.IsForeignAsset(objTmp))
                {
                    objTmp.SetActive(!objTmp.activeSelf);
                    EditorHelper.SetDirty(objTmp);
                }
            }
        }

        [MenuItem("shaco/Tools/RunGame _F5", false, (int)ToolsGlobalDefine.MenuPriority.Tools.RUN_GAME)]
        static void RunGame()
        {
            //默认播放第一个勾选上的场景
            var firstEnabledScene = EditorBuildSettings.scenes.Find(v => v.enabled);
            if (!EditorApplication.isPlaying && null != firstEnabledScene)
            {
                EditorHelper.OpenScene(firstEnabledScene.path);
                EditorApplication.isPlaying = true;
            }
        }

        [MenuItem("shaco/Tools/PauseOrResumeGame _F6", false, (int)ToolsGlobalDefine.MenuPriority.Tools.PAUSE_OR_RESUME_GAME)]
        static void PauseOrResumeGame()
        {
            EditorApplication.isPaused = !EditorApplication.isPaused;
        }

        [MenuItem("shaco/Tools/StopGame _F7", false, (int)ToolsGlobalDefine.MenuPriority.Tools.PAUSE_OR_RESUME_GAME)]
        static void StopGame()
        {
            EditorApplication.isPlaying = false;
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<FileToolsWindow>(this, true, "FileTools");
        }

        void Init()
        {

        }

        void OnDestroy()
        {

        }

        void OnGUI()
        {
            if (_currentWindow == null)
                return;

            _vec2ScrollPos = GUILayout.BeginScrollView(_vec2ScrollPos);

            EditorGUILayout.ObjectField("select", _objPrevSelect, typeof(Object), true);

            //draw select asset md5
            _currentSelectObjects = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (_currentSelectObjects != null && _currentSelectObjects.Length == 1)
            {
                if (_currentSelectObjects[0] != _objPrevSelect)
                {
                    _objPrevSelect = _currentSelectObjects[0];
                    _strSelectPath = Application.dataPath.Remove("Assets");
                    _strSelectPath = shaco.Base.FileHelper.ContactPath(_strSelectPath, AssetDatabase.GetAssetPath(_objPrevSelect));
                    _isFolder = shaco.Base.FileHelper.ExistsDirectory(_strSelectPath);
                    _strGetMD5 = !_isFolder ? shaco.Base.FileHelper.MD5FromFile(_strSelectPath) : string.Empty;
                }
            }
            else
            {
                _objPrevSelect = null;
                _strSelectPath = string.Empty;
                _strGetMD5 = string.Empty;
            }

            if (!string.IsNullOrEmpty(_strGetMD5))
            {
                EditorGUILayout.TextField("MD5", _strGetMD5);
            }

            DrawReplaceFilesExtensions();
            DrawFileSizeComputer();

            GUILayout.EndScrollView();
        }

        private void DrawFileSizeComputer()
        {
            if (!string.IsNullOrEmpty(_strSelectPath))
            {
                if (_isFolder)
                {
                    //delete folder files by tag
                    GUILayoutHelper.DrawStringList(_listDeleteFileTag, "Delete Files Tag");

                    if (_listDeleteFileTag.Count > 0)
                    {
                        if (GUILayout.Button("Delete"))
                        {
                            //修剪掉空字符串
                            _listDeleteFileTag.Trim(string.Empty);

                            if (_listDeleteFileTag.Count > 0)
                            {
                                shaco.Base.FileHelper.DeleteFileByTag(_strSelectPath, _listDeleteFileTag.ToArray());
                                AssetDatabase.Refresh();
                            }
                        }
                    }
                }

                DrawComputerFileSizeButton();
                DrawSearchEmptyFolderButton();
                DrawSearchScriptInResourcesFolderButton();
                DrawDeleteAllSearchButton();
                DrawComputerPicturesMemoryButton();
                DrawSearchNotAsciiFiles();
                DrawSearchSameFileName();
            }
            else
            {
                if (_listFileSize.Count == 0)
                    GUILayout.Label("Please select a file or folder in 'Project' window");
            }

            //draw computer files size
            if (_listFileSize.Count > 0)
            {
                EditorGUILayout.HelpBox("Select Total Size: " + shaco.Base.FileHelper.GetFileSizeFormatString(_lTotalSize) + "\nFile Count: " + _listFileSize.Count, MessageType.Info);

                for (int i = 0; i < _listFileSize.Count; ++i)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.ObjectField(_listFileSize[i].asset, typeof(Object), true, GUILayout.Width(_currentWindow.position.width / 2));
                    GUILayout.Label("size: " + shaco.Base.FileHelper.GetFileSizeFormatString(_listFileSize[i].fileSize), GUILayout.Width(_currentWindow.position.width / 3));
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void DrawComputerFileSizeButton()
        {
            if (GUILayout.Button("Computer File Size"))
            {
                _lTotalSize = 0;
                _listFileSize.Clear();
                var listFiles = new List<string>();
                if (_isFolder)
                {
                    shaco.Base.FileHelper.GetSeekPath(_strSelectPath, ref listFiles);
                }
                else
                {
                    listFiles.Add(_strSelectPath);
                }

                for (int i = 0; i < listFiles.Count; ++i)
                {
                    var newSizeInfo = new FileSizeInfo();
                    var pathRelative = listFiles[i].Remove(Application.dataPath);
                    newSizeInfo.asset = AssetDatabase.LoadAssetAtPath(shaco.Base.FileHelper.ContactPath("Assets/", pathRelative), typeof(Object));
                    if (newSizeInfo.asset == null)
                        continue;

                    newSizeInfo.fileSize = shaco.Base.FileHelper.GetFileSize(listFiles[i]);
                    _lTotalSize += newSizeInfo.fileSize;

                    _listFileSize.Add(newSizeInfo);
                }

                _listFileSize.Sort((FileSizeInfo x, FileSizeInfo y) =>
                {
                    if (x.fileSize < y.fileSize)
                        return 1;
                    else if (x.fileSize > y.fileSize)
                        return -1;
                    else
                        return 0;
                });
            }
        }

        private void DrawSearchEmptyFolderButton()
        {
            if (GUILayout.Button("Search Empty Folder"))
            {
                _lTotalSize = 0;
                _listFileSize.Clear();
                var directories = shaco.Base.FileHelper.GetDirectories(_strSelectPath, "*", System.IO.SearchOption.AllDirectories);

                for (int i = directories.Length - 1; i >= 0; --i)
                {
                    var filesFind = new List<string>();
                    shaco.Base.FileHelper.GetSeekPath(directories[i], ref filesFind, true, ".meta", ".DS_Store");
                    if (filesFind.IsNullOrEmpty())
                    {
                        _listFileSize.Add(new FileSizeInfo()
                        {
                            asset = AssetDatabase.LoadAssetAtPath("Assets/" + directories[i].RemoveFront("/Assets/"), typeof(Object)),
                            fileSize = 0,
                        });
                    }
                }
            }
        }

        private void DrawSearchScriptInResourcesFolderButton()
        {
            if (GUILayout.Button("Search script in 'Resources' Folder"))
            {
                _lTotalSize = 0;
                _listFileSize.Clear();
                var allScripts = shaco.Base.FileHelper.GetFiles(_strSelectPath, "*.cs", System.IO.SearchOption.AllDirectories);
                for (int i = allScripts.Length - 1; i >= 0; --i)
                {
                    var scriptPath = allScripts[i];
                    if (scriptPath.Contains("/Resources/"))
                    {
                        _listFileSize.Add(new FileSizeInfo()
                        {
                            asset = AssetDatabase.LoadAssetAtPath("Assets/" + scriptPath.RemoveFront("/Assets/"), typeof(Object)),
                            fileSize = 0,
                        });
                    }
                }
            }
        }

        private void DrawDeleteAllSearchButton()
        {
            if (_listFileSize.Count > 0 && GUILayout.Button("Delete all search"))
            {
                if (EditorUtility.DisplayDialog("Warning", "confirm delete all search ?", "Delete", "Cancel"))
                {
                    for (int i = 0; i < _listFileSize.Count; ++i)
                    {
                        var assetPath = AssetDatabase.GetAssetPath(_listFileSize[i].asset);
                        if (!string.IsNullOrEmpty(assetPath))
                        {
                            var fullPath = shaco.UnityHelper.UnityPathToFullPath(assetPath);
                            if (shaco.Base.FileHelper.ExistsFileOrDirectory(fullPath))
                            {
                                shaco.Base.FileHelper.DeleteByUserPath(fullPath);
                            }
                        }
                    }
                    _listFileSize.Clear();
                    AssetDatabase.Refresh();
                }
            }
        }

        private class UseMemroyInfo
        {
            public string assetPath = null;
            public long totalMemorySize = 0;
            public List<UseMemroyInfo> depenciesInfo = new List<UseMemroyInfo>();
        }

        private void DrawComputerPicturesMemoryButton()
        {
            //加载目录下所有文件并计算所占内存
            if (_currentSelectObjects.IsNullOrEmpty())
                return;

            if (GUILayout.Button("ComputerUsedMemory"))
            {
                EditorUtility.DisplayCancelableProgressBar("Get files path", "please wait", 0);

                //获取目录路径
                var findPaths = new List<string>();
                foreach (var iter in _currentSelectObjects)
                {
                    var pathTmp = AssetDatabase.GetAssetPath(iter);
                    if (!string.IsNullOrEmpty(pathTmp))
                    {
                        var fullPathTmp = EditorHelper.GetFullPath(pathTmp);
                        if (shaco.Base.FileHelper.ExistsDirectory(fullPathTmp))
                        {
                            var allFilesTmp = System.IO.Directory.GetFiles(fullPathTmp, "*", SearchOption.AllDirectories).Where(v => !v.EndsWith(".meta") && !v.EndsWith(".DS_Store"));
                            if (null != allFilesTmp)
                                findPaths.AddRange(allFilesTmp);
                        }
                        else if (!fullPathTmp.EndsWith(".meta") && !fullPathTmp.EndsWith(".DS_Store"))
                        {
                            findPaths.Add(fullPathTmp);
                        }
                    }
                }

                if (findPaths.IsNullOrEmpty())
                {
                    EditorUtility.ClearProgressBar();
                    shaco.Log.Error("FileToolsWindow DrawComputerPicturesMemoryButton error: not found file");
                    return;
                }

                var useMemroyInfos = new List<UseMemroyInfo>();
                var currentPath = string.Empty;
                shaco.Base.Coroutine.Foreach(findPaths, (v) =>
                {
                    try
                    {
                        var pathTmp = v.ToString();
                        currentPath = pathTmp;

                        var assetPath = EditorHelper.FullPathToUnityAssetPath(pathTmp);
                        var depencies = AssetDatabase.GetDependencies(assetPath, true);

                        var newInfo = new UseMemroyInfo();
                        useMemroyInfos.Add(newInfo);
                        var loadedObjTmp = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                        newInfo.assetPath = assetPath;
                        newInfo.totalMemorySize += EditorHelper.GetRuntimeMemorySizeLong(loadedObjTmp);

                        if (null == depencies)
                            return true;

                        for (int j = depencies.Length - 1; j >= 0; --j)
                        {
                            //过滤自己
                            if (depencies[j] == assetPath)
                                continue;

                            //过滤脚本文件
                            if (depencies[j].EndsWith(".cs"))
                                continue;

                            var newDependInfo = new UseMemroyInfo();
                            newDependInfo.assetPath = depencies[j];

                            newInfo.depenciesInfo.Add(newDependInfo);
                            loadedObjTmp = AssetDatabase.LoadAssetAtPath<Object>(depencies[j]);
                            newDependInfo.totalMemorySize = EditorHelper.GetRuntimeMemorySizeLong(loadedObjTmp);
                            newInfo.totalMemorySize += newDependInfo.totalMemorySize;
                        }
                        Resources.UnloadUnusedAssets();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Log.Error("FileToolsWindow computer memory error: path=" + currentPath + "\n" + e);                        
                    }
                    return true;
                }, (percent) =>
                {
                    EditorUtility.DisplayCancelableProgressBar("Computer", shaco.Base.FileHelper.GetLastFileName(currentPath), percent);

                    if (percent >= 1.0f)
                    {
                        EditorUtility.ClearProgressBar();

                        //根据内存大小排序
                        useMemroyInfos.Sort((v1, v2) => v1.totalMemorySize.CompareTo(v2.totalMemorySize));

                        var strAppend = new System.Text.StringBuilder();
                        var csvSplitFlag = ",";
                        strAppend.Append("Path,MemroySize,Depencies");
                        for (int i = useMemroyInfos.Count - 1; i >= 0; --i)
                        {
                            var info = useMemroyInfos[i];
                            strAppend.Append("\n");
                            strAppend.Append(info.assetPath + csvSplitFlag);
                            strAppend.Append(shaco.Base.FileHelper.GetFileSizeFormatString(info.totalMemorySize) + csvSplitFlag);

                            if (!info.depenciesInfo.IsNullOrEmpty())
                            {
                                info.depenciesInfo.Sort((v1, v2) => v1.totalMemorySize.CompareTo(v2.totalMemorySize));
                                strAppend.Append("\"");
                                for (int j = info.depenciesInfo.Count - 1; j >= 0; --j)
                                {
                                    strAppend.Append(info.depenciesInfo[j].assetPath);
                                    strAppend.Append("[" + shaco.Base.FileHelper.GetFileSizeFormatString(info.depenciesInfo[j].totalMemorySize) + "]\n");
                                }
                                strAppend.Append("\"");
                            }
                        }

                        //默认输出信息到项目根目录
                        var fileName = AssetDatabase.GetAssetPath(_currentSelectObjects[0]).Replace("\\", "/").Replace("/", "_");
                        var outputPath = Application.dataPath.ContactPath("../MemroyUseReport_" + fileName + ".csv");
                        shaco.Base.FileHelper.WriteAllByUserPath(outputPath, strAppend.ToString());
                        System.Diagnostics.Process.Start(outputPath);
                    }
                });
            }
        }

        /// <summary>
        /// 绘制查找非Ascii编码的文件名字
        /// </summary>
        private void DrawSearchNotAsciiFiles()
        {
            if (string.IsNullOrEmpty(_strSelectPath) || !shaco.Base.FileHelper.ExistsDirectory(_strSelectPath))
                return;

            if (GUILayout.Button("Search Not Ascii Files"))
            {
                _listFileSize.Clear();
                var listFiles = shaco.Base.FileHelper.GetFiles(_strSelectPath, "*", SearchOption.AllDirectories);
                if (listFiles.IsNullOrEmpty())
                    return;

                System.Func<string, bool> isAsciiString =  (string str) =>
                {
                    bool retValue = true;
                    for (int i = str.Length - 1; i >= 0; --i)
                    {
                        var charTmp = (short)str[i];
                        if (charTmp < 0 || charTmp > 127)
                        {
                            retValue = false;
                            break;
                        }
                    }
                    return retValue;
                };

                for (int i = listFiles.Length - 1; i >= 0; --i)
                {
                    if (listFiles[i].EndsWith(".meta"))
                        continue;

                    if (isAsciiString(listFiles[i]))
                        continue;
                        
                    var newSizeInfo = new FileSizeInfo();
                    var pathRelative = listFiles[i].Remove(Application.dataPath);
                    newSizeInfo.asset = AssetDatabase.LoadAssetAtPath(shaco.Base.FileHelper.ContactPath("Assets/", pathRelative), typeof(Object));
                    if (newSizeInfo.asset == null)
                        continue;

                    newSizeInfo.fileSize = shaco.Base.FileHelper.GetFileSize(listFiles[i]);
                    _lTotalSize += newSizeInfo.fileSize;
                    _listFileSize.Add(newSizeInfo);
                }
            }
        }

        /// <summary>
        /// 绘制查找文件夹中同名文件功能
        /// </summary>
        private class SameFileNameInfo
        {
            public List<string> fullPaths = new List<string>();
        }
        private void DrawSearchSameFileName()
        {
            if (string.IsNullOrEmpty(_strSelectPath) || !shaco.Base.FileHelper.ExistsDirectory(_strSelectPath))
                return;

            if (GUILayout.Button("Search same name files"))
            {
                _listFileSize.Clear();
                var dicFiles = new Dictionary<string, SameFileNameInfo>();
                var listFiles = shaco.Base.FileHelper.GetFiles(_strSelectPath, "*", SearchOption.AllDirectories);

                for (int i = listFiles.Length - 1; i >= 0; --i)
                {
                    var pathTmp = listFiles[i];
                    if (pathTmp.EndsWith(".meta"))
                        continue;

                    var fileName = shaco.Base.FileHelper.GetLastFileName(pathTmp);
                    if (fileName.StartsWith(shaco.Base.FileDefine.DOT_SPLIT))
                        continue;

                    SameFileNameInfo findInfo = null;
                    if (!dicFiles.TryGetValue(fileName, out findInfo))
                    {
                        findInfo = new SameFileNameInfo();
                        findInfo.fullPaths.Add(pathTmp);
                        dicFiles.Add(fileName, findInfo);
                    }
                    else
                    {
                        findInfo.fullPaths.Add(pathTmp);
                    }
                }

                var removeKeys = new List<string>();
                foreach (var iter in dicFiles)
                {
                    if (iter.Value.fullPaths.Count <= 1)
                        removeKeys.Add(iter.Key);
                }

                for (int i = removeKeys.Count - 1; i >= 0; --i)
                {
                    dicFiles.Remove(removeKeys[i]);
                }

                foreach (var iter in dicFiles)
                {
                    for (int i = iter.Value.fullPaths.Count - 1; i >= 0; --i)
                    {
                        var fullPathTmp = iter.Value.fullPaths[i];
                        var newSizeInfo = new FileSizeInfo();
                        var relativePath = EditorHelper.FullPathToUnityAssetPath(fullPathTmp);
                        newSizeInfo.asset = AssetDatabase.LoadAssetAtPath(relativePath, typeof(Object));
                        if (newSizeInfo.asset == null)
                            continue;

                        newSizeInfo.fileSize = shaco.Base.FileHelper.GetFileSize(relativePath);
                        _lTotalSize += newSizeInfo.fileSize;
                        _listFileSize.Add(newSizeInfo);
                    }
                }
            }
        }

        /// <summary>
        /// 绘制替换所有文件夹中文件后缀名功能
        /// </summary>
        private void DrawReplaceFilesExtensions()
        {
            if (!_isFolder)
                return;

            _inputNewFileNameExtensions = EditorGUILayout.TextField("New FileName Extension", _inputNewFileNameExtensions);
            EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(_inputNewFileNameExtensions));
            {
                if (GUILayout.Button("Relace FileName Extension"))
                {
                    if (string.IsNullOrEmpty(_strSelectPath) || !shaco.Base.FileHelper.ExistsDirectory(_strSelectPath))
                    {
                        _strSelectPath = EditorUtility.OpenFolderPanel("Select a Folder", Application.dataPath, string.Empty);
                    }
                    if (!string.IsNullOrEmpty(_strSelectPath))
                    {
                        var allFiles = new List<string>();
                        shaco.Base.FileHelper.GetSeekPath(_strSelectPath, ref allFiles, true, ".meta");
                        for (int i = allFiles.Count - 1; i >= 0; --i)
                        {
                            var pathTmp = allFiles[i];
                            if (shaco.Base.FileHelper.HasFileNameExtension(pathTmp))
                            {
                                pathTmp = shaco.Base.FileHelper.ReplaceLastExtension(pathTmp, _inputNewFileNameExtensions);
                                shaco.Base.FileHelper.MoveFileByUserPath(allFiles[i], pathTmp);
                            }
                        }

                        if (!allFiles.IsNullOrEmpty())
                            AssetDatabase.Refresh();
                    }
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private bool IsPicturePath(string path)
        {
            return path.EndsWith("*.png") || path.EndsWith("*.jpg") || path.EndsWith("*.tga")
                || path.EndsWith("*.bmp") || path.EndsWith("*.tif") || path.EndsWith("*.gif");
        }

        /// <summary>
        /// 获取热更新资源下载目录
        /// </summary>
        static private string GetDownloadResourcesPath()
        {
            var versionControlFolder = shaco.HotUpdateHelper.GetVersionControlFolderAuto(string.Empty, string.Empty);
            var retValue = shaco.Base.FileHelper.GetFullpath(versionControlFolder).ContactPath("assets");
            return retValue;
        }
    }
}