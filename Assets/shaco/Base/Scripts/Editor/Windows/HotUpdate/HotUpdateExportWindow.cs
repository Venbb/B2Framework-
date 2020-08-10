using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AnimatedValues;
using System.Linq;

namespace shacoEditor
{
    public partial class HotUpdateExportWindow : EditorWindow
    {
        public class SelectFile
        {
            public class FileInfo
            {
                public string Asset = null;

                public FileInfo(string Asset)
                {
                    this.Asset = Asset;
                }
            }

            //是否作为原始资源文件导出
            public shaco.HotUpdateDefine.ExportFileFormat exportFormat = shaco.HotUpdateDefine.ExportFileFormat.AssetBundle;
            public string AssetBundleMD5 = string.Empty;
            public Dictionary<string, FileInfo> ListAsset = new Dictionary<string, FileInfo>();
        }

        //所有需要被导出的资源关系列表
        public Dictionary<string, SelectFile> mapAssetbundlePath = new Dictionary<string, SelectFile>();
        //该字典仅为了快速查看是否有相同id的资源导入了
        public Dictionary<string, string> mapAllExportAssetSameKeyCheck = new Dictionary<string, string>();
        public string currentRootPath = string.Empty;
        public shaco.HotUpdateDefine.SerializeVersionControl versionControlConfigOld = null;
        public shaco.HotUpdateDefine.SerializeVersionControl versionControlConfig = null;

        private HotUpdateExportWindow _currentWindow = null;
        private HotUpdateVersionViewerWindow _windowVersionViewer = new HotUpdateVersionViewerWindow();
        private List<int> _versionCodes = new List<int>();
        private string _printPath = string.Empty;
        private string _decrptAssetBundlePath = string.Empty;
        private SelectFile.FileInfo[] _selectAsset = null;
        private Vector2 _vec2ScrollPosition = Vector2.zero;
        private AnimBool _isShowSettings = new AnimBool(true);
        private AnimBool _isShowBaseButton = new AnimBool(true);
        private AnimBool _isShowTestButton = new AnimBool(false);
        private TextAsset _buildVersionAsset = null;
        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparator = new shaco.Instance.Editor.TreeView.WindowSplitter();
        private BuildAssetBundleOptions _buildAssetbundleOptions = BuildAssetBundleOptions.None;

        [MenuItem("shaco/HotUpdateResourceBuilder %#e", false, (int)ToolsGlobalDefine.MenuPriority.Default.HOT_UPDATE_EXPORT)]
        static public HotUpdateExportWindow ShowHotUpdateExportWindow()
        {
            return EditorHelper.GetWindow<HotUpdateExportWindow>(null, true, "HotUpdateResourceBuilder");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<HotUpdateExportWindow>(this, true, "HotUpdateResourceBuilder");
            _currentWindow.Init();
        }

        public void UpdateDatas()
        {
            if (null != _windowVersionViewer)
            {
                _windowVersionViewer.Init(this);
            }
        }

        public void Init()
        {
            this.OnClearButtonClick();

            //读取保存配置
            _printPath = EditorPrefs.GetString(GetDataSaveKey("_printPath"), string.Empty);
            _decrptAssetBundlePath = EditorPrefs.GetString(GetDataSaveKey("_decrptAssetBundlePath"), string.Empty);
            _isShowSettings.value = EditorPrefs.GetBool(GetDataSaveKey("_isShowSettings"), true);
            _isShowBaseButton.value = EditorPrefs.GetBool(GetDataSaveKey("_isShowBaseButton"), true);
            _isShowTestButton.value = EditorPrefs.GetBool(GetDataSaveKey("_isShowTestButton"), true);
            // var buildVersionSavePath = EditorPrefs.GetString(GetDataSaveKey("_buildVersionAsset"));
            var leftWindowPercent = shaco.GameHelper.datasave.ReadFloat(GetDataSaveKey("_leftWindowPercent"));
            var rightWindowPercent = shaco.GameHelper.datasave.ReadFloat(GetDataSaveKey("_rightWindowPercent"));

            _buildAssetbundleOptions = shaco.GameHelper.datasave.ReadEnum("HotUpdateExportWindow." + shaco.Base.Utility.ToVariableName(() => _buildAssetbundleOptions), _buildAssetbundleOptions);

            _windowVersionViewer.Init(this);

            //设置版本号信息自动显示
            _buildVersionAsset = Resources.Load<TextAsset>(shaco.HotUpdateDefine.RESOURCES_VERSION_PATH);
            SetVersionAutomatic();

            if (leftWindowPercent <= 0 || rightWindowPercent <= 0)
            {
                leftWindowPercent = 0.3f;
                rightWindowPercent = 0.7f;
            }
            _dragLineSeparator.SetSplitWindow(this, leftWindowPercent, rightWindowPercent);

            //刷新界面数据
            UpdateDatas();
        }

        private void OnClearButtonClick()
        {
            _printPath = string.Empty;
            mapAssetbundlePath.Clear();
            mapAllExportAssetSameKeyCheck.Clear();
            currentRootPath = string.Empty;
            versionControlConfigOld = null;
            versionControlConfig = new shaco.HotUpdateDefine.SerializeVersionControl();

            if (null != _windowVersionViewer)
                _windowVersionViewer.ClearDrawFolder();
        }

        private string GetDataSaveKey(string key)
        {
            return "HotUpdateExportWindow" + key;
        }

        private void SetVersionAutomatic()
        {
            //重置version参数
            _versionCodes.Clear();

            if (null == _buildVersionAsset)
            {
                return;
            }

            var readString = _buildVersionAsset.ToString();

            if (string.IsNullOrEmpty(readString))
            {
                return;
            }

            int indexFindVersion = readString.IndexOf("appVersion");
            if (indexFindVersion < 0)
            {
                var splitString = readString.Split(".");
                for (int i = 0; i < splitString.Length; ++i)
                {
                    _versionCodes.Add(splitString[i].ToInt());
                }
                return;
            }

            indexFindVersion += "appVersion".Length;

            //find version code
            for (int i = indexFindVersion; i < readString.Length; ++i, ++indexFindVersion)
            {
                char cTmp = readString[i];
                if (shaco.Base.FileHelper.isNumber(cTmp))
                {
                    break;
                }
            }

            //get version code
            for (int i = indexFindVersion; i < readString.Length; ++i)
            {
                char cTmp = readString[i];
                if (!shaco.Base.FileHelper.isNumber(cTmp))
                {
                    break;
                }
                else
                {
                    if (cTmp != '.')
                    {
                        _versionCodes.Add(cTmp.ToString().ToInt());
                    }
                }
            }
        }

        void OnGUI()
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                DrawGUI();
            }
            EditorGUI.EndDisabledGroup();
        }

        void DrawGUI()
        {
            if (_currentWindow == null)
                return;

            UpdateEvent();

            try
            {
                _dragLineSeparator.BeginLayout(true);
                _vec2ScrollPosition = GUILayout.BeginScrollView(_vec2ScrollPosition);
            }
            catch (System.Exception)
            {
                //ignore exception
            }

            DrawSettings();
            DrawExecuteButtons();
            DrawTestButtons();
            // DrawCurrentSelectAssets();

            try
            {
                GUILayout.EndScrollView();
                _dragLineSeparator.EndLayout();
            }
            catch (System.Exception)
            {
                //ignore exception
            }

            _dragLineSeparator.BeginLayout();
            {
                _windowVersionViewer.DrawInspector(_dragLineSeparator.GetSplitWindowRect(1));
            }
            _dragLineSeparator.EndLayout();
        }

        void UpdateEvent()
        {
            var curEvent = Event.current;
            if (null == curEvent)
                return;

            if (curEvent.button == 1 && curEvent.type == EventType.MouseUp)
            {
                GenericMenu contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("ShowAsFloatWindow"), false, (object context) =>
                {
                    if (EditorUtility.DisplayDialog("ShowAsFloatWindow", "This will reset all temporary configuration information that has been set", "OK", "Cancel"))
                    {
                        _currentWindow.Close();
                        _currentWindow = EditorWindow.GetWindow<HotUpdateExportWindow>(false, "HotUpdateResourceBuilder");
                    }
                }, null);
                contextMenu.AddItem(new GUIContent("ShowAsOverlayWindow"), false, (object context) =>
                {
                    if (EditorUtility.DisplayDialog("ShowAsOverlayWindow", "This will reset all temporary configuration information that has been set", "OK", "Cancel"))
                    {
                        _currentWindow.Close();
                        _currentWindow = EditorWindow.GetWindow<HotUpdateExportWindow>(true, "HotUpdateResourceBuilder");
                    }
                }, null);
                contextMenu.ShowAsContext();
            }
        }

        void OnDestroy()
        {
            SaveSettings();
            this.OnClearButtonClick();
            if (_windowVersionViewer != null)
            {
                _windowVersionViewer = null;
            }
            HotUpdateImportHistoryWindow.CloseWindow();
            _currentWindow = null;
        }

        private void SaveSettings()
        {
            EditorPrefs.SetString(GetDataSaveKey("_printPath"), _printPath);
            EditorPrefs.SetString(GetDataSaveKey("_decrptAssetBundlePath"), _decrptAssetBundlePath);
            EditorPrefs.SetBool(GetDataSaveKey("_isShowSettings"), _isShowSettings.value);
            EditorPrefs.SetBool(GetDataSaveKey("_isShowBaseButton"), _isShowBaseButton.value);
            EditorPrefs.SetBool(GetDataSaveKey("_isShowTestButton"), _isShowTestButton.value);
            shaco.GameHelper.datasave.WriteFloat(GetDataSaveKey("_leftWindowPercent"), _dragLineSeparator.GetSplitWindowPercent(0));
            shaco.GameHelper.datasave.WriteFloat(GetDataSaveKey("_rightWindowPercent"), _dragLineSeparator.GetSplitWindowPercent(1));
            shaco.GameHelper.datasave.WriteEnum("HotUpdateExportWindow." + shaco.Base.Utility.ToVariableName(() => _buildAssetbundleOptions), _buildAssetbundleOptions);

            // if (null != _buildVersionAsset)
            // {
            //     var buildVersionPath = AssetDatabase.GetAssetPath(_buildVersionAsset);
            //     EditorPrefs.SetString(GetDataSaveKey("_buildVersionAsset"), buildVersionPath);
            // }
        }

        public void NewAssetBundle(params SelectFile.FileInfo[] selects)
        {
            OpenFileDialog(true, false, selects);
        }

        public void NewAssetBundleDeepAssets(params SelectFile.FileInfo[] selects)
        {
            OpenFileDialog(true, true, selects);
        }


        public void CheckRootPathValidWithPlatform(string pathRoot)
        {
            //查看导入文件夹平台是否和当前平台一致
            var lastFileNameCheck = shaco.Base.FileHelper.GetLastFileName(pathRoot);
            if (lastFileNameCheck.IndexOf(shaco.Base.GlobalParams.SIGN_FLAG) >= 0)
            {
                var currentPlatform = shaco.HotUpdateHelper.GetAssetBundleAutoPlatform();
                var targetPlatform = shaco.HotUpdateHelper.GetAssetBundlePlatformByPath(lastFileNameCheck);
                if (currentPlatform != targetPlatform)
                {
                    Debug.LogError("HotUpdateExportWindow CheckRootPathValidWithPlatform erorr: Different Platform Current Platform: " + currentPlatform + "\n" + "Target Platform: " + targetPlatform);
                }
            }
        }

        // void ReloadExportDatas()
        // {
        //     HotUpdateVersionImportWindow.GetPreviousConfigByVersionControl(_currentWindow);
        // }

        //打开路径选择对话框
        void OpenFileDialog(bool isOneFileToOneAssetBundle, bool isDeepAssets, SelectFile.FileInfo[] selects)
        {
            if (selects.Length > 0)
                _selectAsset = selects;

            if (string.IsNullOrEmpty(currentRootPath))
            {
                currentRootPath = EditorUtility.SaveFolderPanel("Select export folder", Application.streamingAssetsPath, string.Empty);
                if (!string.IsNullOrEmpty(currentRootPath))
                {
                    //自动检查并添加VersionControl目录
                    currentRootPath = shaco.HotUpdateHelper.CheckAutoAddVersionControlFolder(currentRootPath);
                    CheckRootPathValidWithPlatform(currentRootPath);

                    var strInputFilename = shaco.Base.FileHelper.GetLastFileName(currentRootPath, shaco.Base.FileDefine.PATH_FLAG_SPLIT, true);
                    strInputFilename = shaco.HotUpdateHelper.AssetBundleKeyToPath(strInputFilename);
                    OpenFileDialogEnd(strInputFilename, isOneFileToOneAssetBundle, isDeepAssets, _selectAsset);
                }
            }
            else
            {
                //自动使用第一个选中的文件夹名字作为资源包的名字
                if (isOneFileToOneAssetBundle && _selectAsset.Length > 0)
                {
                    var path = _selectAsset[0].Asset.ToLower();
                    OpenFileDialogEnd(path, isOneFileToOneAssetBundle, isDeepAssets, _selectAsset);
                }
                else
                {
                    //默认使用第一次使用过的路径作为前置路径
                    HotUpdatePathIntputWindow.OpenHotUpdatePathInputWindow((string path) =>
                    {
                        if (!string.IsNullOrEmpty(path))
                        {
                            if (path[0] == shaco.Base.FileDefine.PATH_FLAG_SPLIT)
                                path = path.Remove(0, 1);

                            OpenFileDialogEnd(path, isOneFileToOneAssetBundle, isDeepAssets, _selectAsset);
                        }
                    }, this.position);
                }
            }
        }

        private bool CheckAddBuildInfo(string assetbundleKey, SelectFile info)
        {
            var retValue = false;
            if (!mapAssetbundlePath.ContainsKey(assetbundleKey))
            {
                retValue = true;
                List<string> removeKeys = null;
                foreach (var iter in info.ListAsset)
                {
                    var assetType = AssetDatabase.GetMainAssetTypeAtPath(iter.Value.Asset);
                    if (assetType == typeof(Sprite) || assetType == typeof(Texture2D))
                    {
                        if (mapAllExportAssetSameKeyCheck.ContainsKey(iter.Value.Asset))
                        {
                            if (null == removeKeys)
                                removeKeys = new List<string>();
                            removeKeys.Add(iter.Key);
                        }
                    }
                }

                if (null != removeKeys)
                {
                    //没有可以添加的资源，直接返回
                    if (removeKeys.Count == info.ListAsset.Count)
                    {
                        for (int i = removeKeys.Count - 1; i >= 0; --i)
                        {
                            var oldAssetKey = shaco.Base.FileHelper.ReplaceLastExtension(removeKeys[i], shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                            _windowVersionViewer.RemoveFile(oldAssetKey);
                        }
                        return retValue;
                    }

                    //删除已经添加过到图集中的资源
                    for (int i = removeKeys.Count - 1; i >= 0; --i)
                        info.ListAsset.Remove(removeKeys[i]);
                }

                mapAssetbundlePath.Add(assetbundleKey, info);

                //当一个assetbundle对应一个图集信息或者有打包配置时候，自动获取打包信息内容并把散列信息添加到assetbundle导出信息中
                //以便unity在打包时候自动生成对应的依赖关系
                if (info.ListAsset.Count != 1)
                    return retValue;

#if UNITY_2017_1_OR_NEWER
                var assetsInSpriteAtlas = GetObjectsInSpriteAtlas(info);
                if (null != assetsInSpriteAtlas)
                {
                    var searchAsssetPathTmp = GetSearchAssetPath(assetsInSpriteAtlas, "t:sprite");
                    ReplaceSearchAssetInPackSetting(searchAsssetPathTmp, info, assetbundleKey);
                }
#endif

                shaco.ResourcePackSetting resourcePackSetting = null;
                var assetsInResourcePack = GetObjectsInResourcePack(info, out resourcePackSetting);
                if (null != assetsInResourcePack)
                {
                    //设置打包格式
                    if (null != resourcePackSetting)
                        info.exportFormat = resourcePackSetting.exportFormat;

                    var searchAsssetPathTmp = GetSearchAssetPath(assetsInResourcePack, "t:object");
                    ReplaceSearchAssetInPackSetting(searchAsssetPathTmp, info, assetbundleKey);
                }
                return retValue;
            }
            else
            {
                return retValue;
            }
        }

        ICollection<Object> GetObjectsInSpriteAtlas(SelectFile info)
        {
#if UNITY_2017_1_OR_NEWER
            var spriteAtlasAssetPath = info.ListAsset.First().Value.Asset;
            var assetTypeCheck = AssetDatabase.GetMainAssetTypeAtPath(spriteAtlasAssetPath);
            if (assetTypeCheck != typeof(UnityEngine.U2D.SpriteAtlas))
                return null;

            var spriteAtlas = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(spriteAtlasAssetPath);
            var assetsTmp = UnityEditor.U2D.SpriteAtlasExtensions.GetPackables(spriteAtlas);
            if (assetsTmp.IsNullOrEmpty())
                return null;

            return assetsTmp;
#else
            return null;
#endif
        }

        void ReplaceSearchAssetInPackSetting(IEnumerable<string> allSearchPath, SelectFile info, string assetbundleKey)
        {
            if (null == allSearchPath)
                return;

            foreach (var iter in allSearchPath)
            {
                var pathLower = iter.ToLower();
                var oldAssetKey = shaco.Base.FileHelper.ReplaceLastExtension(pathLower, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);

                //移除项目中原散列打包信息
                SelectFile findValue = null;
                if (mapAssetbundlePath.TryGetValue(oldAssetKey, out findValue))
                {
                    foreach (var iter2 in findValue.ListAsset)
                    {
                        mapAllExportAssetSameKeyCheck.Remove(iter2.Value.Asset);
                    }
                    mapAssetbundlePath.Remove(oldAssetKey);
                    _windowVersionViewer.RemoveFile(oldAssetKey);
                }

                //移动散列打包信息到资源包中
                if (info.ListAsset.ContainsKey(pathLower))
                    Debug.LogError("HotUpdateExportWindow ReplaceSearchAssetInPackSetting error: duplicate asset key=" + pathLower);
                else
                {
                    //避免重复添加冗余资源
                    if (!mapAllExportAssetSameKeyCheck.ContainsKey(pathLower))
                    {
                        var newSelectInfo = new SelectFile.FileInfo(pathLower);
                        info.ListAsset.Add(pathLower, newSelectInfo);
                        _windowVersionViewer.AddFile(assetbundleKey, newSelectInfo);
                    }
                }
            }
        }

        ICollection<Object> GetObjectsInResourcePack(SelectFile info, out shaco.ResourcePackSetting setting)
        {
            setting = null;
            var firstAssetPath = info.ListAsset.First().Value.Asset;
            var assetTypeCheck = AssetDatabase.GetMainAssetTypeAtPath(firstAssetPath);
            if (assetTypeCheck != typeof(shaco.ResourcePackSetting))
                return null;

            setting = AssetDatabase.LoadAssetAtPath<shaco.ResourcePackSetting>(firstAssetPath);
            var assetsTmp = setting.assetsGUID.Convert(v => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(v)));

            if (assetsTmp.IsNullOrEmpty())
                return null;
            return assetsTmp;
        }

        IEnumerable<string> GetSearchAssetPath(ICollection<Object> assets, string searchFilter)
        {
            var retValue = new List<string>();
            foreach (var iter in assets)
            {
                var spriteAssetPath = AssetDatabase.GetAssetPath(iter);

                //文件夹
                if (AssetDatabase.IsValidFolder(spriteAssetPath))
                {
                    var filters = AssetDatabase.FindAssets(searchFilter, new string[] { spriteAssetPath });
                    filters = filters.Convert(v => AssetDatabase.GUIDToAssetPath(v));
                    if (!filters.IsNullOrEmpty())
                        retValue.AddRange(filters.Where(v => !AssetDatabase.IsValidFolder(v)));
                }
                //文件
                else
                {
                    var spriteAsset = iter;
                    var assetType = spriteAsset.GetType();
                    retValue.Add(spriteAssetPath);
                }
            }
            return retValue.Distinct();
        }

        void OpenFileDialogEnd(string filename, bool isOneFileToOneAssetBundle, bool isDeepAssets, params SelectFile.FileInfo[] selects)
        {
            if (!isOneFileToOneAssetBundle)
            {
                if (shaco.Base.FileHelper.HasFileNameExtension(filename))
                {
                    filename = shaco.Base.FileHelper.RemoveLastExtension(filename);
                }

                filename = shaco.Base.FileHelper.AddExtensions(filename, shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
                var fileNameKeyLower = filename.ToLower();

                var newItem = SelectCurrentAssetBundleFiles(filename, selects);
                if (null != newItem)
                {
                    var checkAddSuccess = CheckAddBuildInfo(fileNameKeyLower, newItem);

                    //添加引用
                    foreach (var iter in newItem.ListAsset)
                    {
                        if (!mapAllExportAssetSameKeyCheck.ContainsKey(iter.Key))
                            mapAllExportAssetSameKeyCheck.Add(iter.Key, filename);
                    }

                    if (!checkAddSuccess)
                    {
                        var oldItem = mapAssetbundlePath[fileNameKeyLower];
                        oldItem.AssetBundleMD5 = newItem.AssetBundleMD5;

                        foreach (var key in newItem.ListAsset.Keys)
                        {
                            if (oldItem.ListAsset.ContainsKey(key))
                            {
                                Debug.LogError("add asset to assetbundle erorr: has same asset=" + key);
                            }
                            else
                            {
                                oldItem.ListAsset.Add(key, newItem.ListAsset[key]);
                            }
                        }
                    }
                }
            }
            else
            {
                if (isDeepAssets)
                {
                    if (selects.Length == 0)
                    {
                        Debug.LogError("HotUpdateExportWindow OpenFileDialogEnd nothing need build, Probably filtered out all over, please check 'IGNORE_RESOURCE_EXTENSIONS'");
                        return;
                    }

                    int exportAssetsCount = 0;
                    for (int i = 0; i < selects.Length; ++i)
                    {
                        if (null == selects[i] || string.IsNullOrEmpty(selects[i].Asset))
                        {
                            continue;
                        }

                        //如果是文件夹，则遍历该文件夹，对每个文件打包
                        var fullPathTmp = EditorHelper.GetFullPath(selects[i].Asset);
                        if (shaco.Base.FileHelper.ExistsDirectory(fullPathTmp))
                        {
                            var allFilesTmp = GetCanBuildAssetBundlesAssetPath(fullPathTmp);
                            foreach (var iter in allFilesTmp)
                            {
                                OpenFileDialogEnd(iter, false, isDeepAssets, new SelectFile.FileInfo(iter));
                                ++exportAssetsCount;
                            }
                        }
                        //如果是文件，直接打包
                        else if (shaco.Base.FileHelper.ExistsFile(fullPathTmp))
                        {
                            if (!string.IsNullOrEmpty(selects[i].Asset))
                            {
                                var pathTmp = selects[i].Asset;
                                OpenFileDialogEnd(pathTmp.ToLower(), false, isDeepAssets, selects[i]);
                                ++exportAssetsCount;
                            }
                        }
                        //丢失文件
                        else
                        {
                            Debug.LogError("HotUpdateExportWindow OpenFileDialogEnd error: missing file=" + fullPathTmp);
                        }
                    }

                    if (exportAssetsCount <= 0)
                    {
                        LogErrorNoAssetExport("Asset", selects.ToList());
                    }
                }
                else
                {
                    var listAllSelect = selects.IsNullOrEmpty() ? GetCurrentSelection() : selects;
                    for (int i = 0; i < listAllSelect.Length; ++i)
                    {
                        var pathTmp = listAllSelect[i].Asset;
                        string[] listSelect = null;
                        if (shaco.Base.FileHelper.ExistsDirectory(pathTmp))
                        {
                            listSelect = GetCanBuildAssetBundlesAssetPath(pathTmp).ToArray();
                        }
                        else if (shaco.Base.FileHelper.ExistsFile(pathTmp))
                            listSelect = new string[] { pathTmp };
                        else
                        {
                            DisplayDialogError("missing file path=" + pathTmp);
                            continue;
                        }

                        var selectsTmp = new SelectFile.FileInfo[listSelect.Length];
                        for (int j = 0; j < listSelect.Length; ++j)
                        {
                            selectsTmp[j] = new SelectFile.FileInfo(listSelect[j]);
                        }
                        OpenFileDialogEnd(pathTmp, false, isDeepAssets, selectsTmp);
                    }
                }
            }
        }

        //设置当前assetbundle对应的文件名字
        //return: 如果设置失败返回false
        public SelectFile SelectCurrentAssetBundleFiles(string filename, SelectFile.FileInfo[] selects)
        {
            List<SelectFile.FileInfo> listSameAsset = new List<SelectFile.FileInfo>();
            return SelectCurrentAssetBundleFiles(filename, selects, ref listSameAsset);
        }
        public SelectFile SelectCurrentAssetBundleFiles(string filename, SelectFile.FileInfo[] selects, ref List<SelectFile.FileInfo> listSameAsset)
        {
            SelectFile retValue = null;
            if (!string.IsNullOrEmpty(currentRootPath))
            {
                for (int i = 0; i < selects.Length; ++i)
                {
                    if (selects[i] == null)
                        continue;

                    var assetPath = selects[i].Asset;
                    var pathLowerTmp = assetPath.ToLower();
                    bool isSameKey = mapAllExportAssetSameKeyCheck.ContainsKey(pathLowerTmp);

                    //有重复的文件被添加
                    if (isSameKey)
                    {
                        // Debug.LogWarning("has same item in asssetbundle, asset path=" + pathTmp);
                        listSameAsset.Add(selects[i]);
                    }
                    else
                    {
                        if (System.IO.File.Exists(assetPath))
                        {
                            if (null == retValue)
                                retValue = new SelectFile();

                            retValue.ListAsset.Add(pathLowerTmp, new SelectFile.FileInfo(pathLowerTmp));

                            selects[i].Asset = selects[i].Asset.ToLower();
                            filename = filename.ToLower();
                            _windowVersionViewer.AddFile(filename, selects[i]);
                        }
                        else
                        {
                            //到这里如果还有文件夹路径传入是错误的，应该在此之前就遍历过了
                            if (System.IO.Directory.Exists(assetPath))
                            {
                                Debug.LogError("HotUpdateExport SelectCurrentAssetBundleFiles error: cannot build a folder, path=" + assetPath);
                            }
                            //文件路径丢失
                            else
                            {
                                Debug.LogError("HotUpdateExport SelectCurrentAssetBundleFiles error: missing file path=" + assetPath);
                            }
                        }
                    }
                }

                return retValue;
            }
            else
            {
                DisplayDialogError("SelectCurrentAssetBundleFiles error: forget set prefix path");
            }
            return retValue;
        }

        public void CheckRootPathValid()
        {
            if (mapAssetbundlePath.Count == 0)
                currentRootPath = string.Empty;
        }

        /// <summary>
        /// 检查并自动修复所有配置中有但是Unity中丢失的文件
        /// </summary>
        private void CheckAndFixMissingFiles()
        {
            if (versionControlConfig.ListAssetBundles.Count == 0)
            {
                Debug.LogError("HotUpdateExportWindow CheckAndFixMissingFiles error: versionControlConfig.ListAssetBundles is empty");
                return;
            }

            if (mapAssetbundlePath.Count == 0)
            {
                Debug.LogError("HotUpdateExportWindow CheckAndFixMissingFiles error: mapAssetbundlePath is empty");
                return;
            }

            //检查版本控制文件中可能已经丢失的文件
            bool haveMissingFile = false;
            for (int i = versionControlConfig.ListAssetBundles.Count - 1; i >= 0; --i)
            {
                var assetbundleInfo = versionControlConfig.ListAssetBundles[i];
                bool shouldRemoveAssetBundleInfo = false;

                //找不到assetbundle
                var assetbundlePathFind = shaco.HotUpdateHelper.AssetBundleKeyToPath(assetbundleInfo.AssetBundleName);
                SelectFile findSelectFileInfo = null;
                if (!mapAssetbundlePath.TryGetValue(assetbundlePathFind, out findSelectFileInfo))
                {
                    shouldRemoveAssetBundleInfo = true;
                    haveMissingFile = true;
                }
                else
                {
                    for (int j = assetbundleInfo.ListFiles.Count - 1; j >= 0; --j)
                    {
                        //找不到文件
                        var pathTmp = assetbundleInfo.ListFiles[j];
                        if (!findSelectFileInfo.ListAsset.ContainsKey(pathTmp.Key))
                        {
                            assetbundleInfo.ListFiles.RemoveAt(j);
                            haveMissingFile = true;
                        }
                    }
                }

                if (shouldRemoveAssetBundleInfo || assetbundleInfo.ListFiles.Count == 0)
                {
                    versionControlConfig.ListAssetBundles.RemoveAt(i);
                }
            }

            if (haveMissingFile && EditorUtility.DisplayDialog("Information", "have some file missing, continue fix ?", "Fix", "Cancel"))
            {
                shaco.HotUpdateHelper.DeleteUnUseFiles(currentRootPath, versionControlConfig, true, null);

                var versionControlJsonName = shaco.Base.FileHelper.GetLastFileName(currentRootPath);
                versionControlJsonName = shaco.Base.FileHelper.AddExtensions(versionControlJsonName, shaco.HotUpdateDefine.EXTENSION_VERSION_CONTROL);
                var md5RootPath = shaco.HotUpdateHelper.FindMainMD5RootPathInPath(currentRootPath);

                shaco.HotUpdateHelper.SaveVersionControlFiles(md5RootPath.ContactPath(versionControlJsonName), versionControlConfig);
            }
            else
            {
                EditorUtility.DisplayDialog("Information", "nothing missing in project", "OK");
            }
        }

        static private void DisplayDialogError(string message)
        {
            if (Application.isBatchMode)
                Debug.LogError("HotUpdateExportWindow error: " + message);
            else
                EditorUtility.DisplayDialog("Error", message, "OK");
        }
    }
}
