using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [System.Diagnostics.DebuggerStepThrough]
    public class GameHelperWindow : EditorWindow
    {
        private shaco.Base.IDataSave _configData = null;
        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparator = new shaco.Instance.Editor.TreeView.WindowSplitter();
        [SerializeField]
        private string _searchName = string.Empty;
        [SerializeField]
        private string _searchNameLower = string.Empty;
        private Vector2 _leftWindowScrollPos = Vector2.zero;
        private Vector2 _rightWindowScrollPos = Vector2.zero;
        [SerializeField]
        private shaco.ResourcesLoadMode _resourcesLoadMode = shaco.ResourcesLoadMode.EditorDevelop;
        private shaco.ResourcesLoadOrder _resourcesLoadOrder = shaco.ResourcesLoadOrder.DownloadFirst;

        private GUIContent _resourcesLoadModeContent = new GUIContent("Resources Load Mode", "资源加载模式");
        private GUIContent _resourcesLoadOrderContent = new GUIContent("Resources Load Order", "资源加载优先顺序");
        private GUIContent _excelLoadFileTypeContent = new GUIContent("Excel Load File Type", "excel配置表运行时刻文件加载类型");

        private shaco.ResourcesLoadMode _prevResourcesLoadMode = shaco.ResourcesLoadMode.EditorDevelop;

        [MenuItem("shaco/GameHelper %#g", false, (int)ToolsGlobalDefine.MenuPriority.Default.GAMEH_HELPER)]
        static void Open()
        {
            EditorHelper.GetWindow<GameHelperWindow>(null, true, "GameHelper");
        }

        void OnEnable()
        {
            EditorHelper.GetWindow<GameHelperWindow>(this, true, "GameHelper");
            this.Init();
        }

        void OnGUI()
        {
            EditorHelper.RecordObjectWindow(this);
            
            _dragLineSeparator.BeginLayout(true);
            {
                DrawLeftWindow();
            }
            _dragLineSeparator.EndLayout();

            _dragLineSeparator.BeginLayout();
            {
                DrawRightWindow();
            }
            _dragLineSeparator.EndLayout();

            DrawSearchFiled();
        }

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformedCallBack;
        }

        private void Init()
        {
            _configData = shaco.Base.GameHelper.gameConfig;
            LoadSettings();
            _dragLineSeparator.SetSplitWindow(this, 0.6f, 0.4f);

            //窗口初始化的时候重置一次弹出类型记录，以免增改删了类型后不能及时反应出来
            GUILayoutHelper.ResetPopTypeField();

            //监听undo事件
            _prevResourcesLoadMode = _resourcesLoadMode;
            Undo.undoRedoPerformed -= OnUndoRedoPerformedCallBack;
            Undo.undoRedoPerformed += OnUndoRedoPerformedCallBack;
        }

        private void OnUndoRedoPerformedCallBack()
        {
            if (_prevResourcesLoadMode != _resourcesLoadMode)
                OnLoadModeChanged(_resourcesLoadMode);
        }

        private void LoadSettings()
        {
            if (Application.isPlaying)
            {
                _resourcesLoadMode = shaco.GameHelper.res.resourcesLoadMode;
                _resourcesLoadOrder = shaco.GameHelper.res.resourcesLoadOrder;
            }
            else
            {
                _resourcesLoadMode = _configData.ReadEnum(_resourcesLoadMode.ToTypeString(), shaco.GameHelper.res.resourcesLoadMode);
                _resourcesLoadOrder = _configData.ReadEnum(_resourcesLoadOrder.ToTypeString(), shaco.GameHelper.res.resourcesLoadOrder);
            }
        }

        private void SaveSettings()
        {
            _configData.WriteEnum(_resourcesLoadMode.ToTypeString(), _resourcesLoadMode);
            _configData.WriteEnum(_resourcesLoadOrder.ToTypeString(), _resourcesLoadOrder);
            _configData.CheckSaveModifyData();
        }

        private void DrawSearchFiled()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(Screen.width / 3 * 2);
                var newSearchName = GUILayoutHelper.SearchField(_searchName);
                if (newSearchName != _searchName)
                {
                    _searchName = newSearchName;
                    _searchNameLower = _searchName.ToLower();
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawLeftWindow()
        {
            _leftWindowScrollPos = GUILayout.BeginScrollView(_leftWindowScrollPos);
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Framework Version", shaco.Base.GlobalParams.SHACO_GAME_FRAMEWORK_VERSION);
                }
                GUILayout.EndHorizontal();

                shaco.ResourcesLoadMode loadModeTmp;
                GUILayout.BeginHorizontal();
                {
                    loadModeTmp = (shaco.ResourcesLoadMode)EditorGUILayout.EnumPopup(_resourcesLoadModeContent, _resourcesLoadMode);
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        EditorGUILayout.EnumPopup(_resourcesLoadOrderContent, _resourcesLoadOrder);
                    }
                    EditorGUI.EndDisabledGroup();
                }
                GUILayout.EndHorizontal();

                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.EnumPopup(_excelLoadFileTypeContent, shaco.GameHelper.excelSetting.runtimeLoadFileType);
                }
                EditorGUI.EndDisabledGroup();

                if (loadModeTmp != _resourcesLoadMode)
                {
                    OnLoadModeChanged(loadModeTmp);
                }

                DrawHelperInterfaces();
            }
            GUILayout.EndScrollView();
        }

        private void DrawRightWindow()
        {
            _rightWindowScrollPos = GUILayout.BeginScrollView(_rightWindowScrollPos);
            {
                GUILayout.Space(16);
                GUILayout.Label("Runtime Instance Componnets");
                shaco.GameEntry.Foreach((type, value) =>
                {
                    var typeStringTmp = type.ToTypeString();
                    if (!string.IsNullOrEmpty(_searchNameLower) && !typeStringTmp.ToLower().Contains(_searchNameLower))
                    {
                        return;
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label(typeStringTmp, GUILayout.Width(Screen.width * 0.1f));
                        GUILayout.TextField(value.ToTypeString());
                    }
                    GUILayout.EndHorizontal();
                });
            }
            GUILayout.EndScrollView();
        }

        private void DrawHelperInterfaces()
        {
            //c#
            if (GUILayoutHelper.DrawHeader("C#", "GameHelperWindow_C#"))
            {
                DrawHelperInterface("Log", shaco.GameHelper.log, (value) => { shaco.GameHelper.log = value; return shaco.GameHelper.log; });
                DrawHelperInterface("Event Manager", shaco.GameHelper.Event, (value) => { shaco.GameHelper.Event = value; return shaco.GameHelper.Event; });
                DrawHelperInterface("Data Save", shaco.GameHelper.datasave, (value) => { shaco.GameHelper.datasave = value; return shaco.GameHelper.datasave; });
                DrawHelperInterface("Object Pool", shaco.GameHelper.objectpool, (value) => { shaco.GameHelper.objectpool = value; return shaco.GameHelper.objectpool; });
                DrawHelperInterface("Object Pool Spwan", shaco.GameHelper.objectpoolSpawn, (value) => { shaco.GameHelper.objectpoolSpawn = value; return shaco.GameHelper.objectpoolSpawn; });
                DrawHelperInterface("Http Helper", shaco.GameHelper.http, (value) => { shaco.GameHelper.http = value; return shaco.GameHelper.http; });
                DrawHelperInterface("Guide Manager", shaco.GameHelper.newguide, (value) => { shaco.GameHelper.newguide = value; return shaco.GameHelper.newguide; });
                DrawHelperInterface("Guide Setting Helper", shaco.GameHelper.guideSettingHelper, (value) => { shaco.GameHelper.guideSettingHelper = value; return shaco.GameHelper.guideSettingHelper; });
                DrawHelperInterface("Setting Helper", shaco.GameHelper.settinghelper, (value) => { shaco.GameHelper.settinghelper = value; return shaco.GameHelper.settinghelper; });
                DrawHelperInterface("Zip Helper", shaco.GameHelper.zip, (value) => { shaco.GameHelper.zip = value; return shaco.GameHelper.zip; });
                DrawHelperInterface("Simulate Platfrom Event", shaco.GameHelper.simulatePlatformEvent, (value) => { shaco.GameHelper.simulatePlatformEvent = value; return shaco.GameHelper.simulatePlatformEvent; });
                DrawHelperInterface("Data Type To String", shaco.GameHelper.dateTypesTemplate, (value) => { shaco.GameHelper.dateTypesTemplate = value; return shaco.GameHelper.dateTypesTemplate; });
                DrawHelperInterface("Localization", shaco.GameHelper.localization, (value) => { shaco.GameHelper.localization = value; return shaco.GameHelper.localization; });
                DrawHelperInterface("Excel Setting", shaco.GameHelper.excelSetting, (value) => { shaco.GameHelper.excelSetting = value; return shaco.GameHelper.excelSetting; });
                DrawHelperInterface("Profiler", shaco.GameHelper.profiler, (value) => { shaco.GameHelper.profiler = value; return shaco.GameHelper.profiler; });
            }

            //unity
            if (GUILayoutHelper.DrawHeader("Unity", "GameHelperWindow_Unity"))
            {
                DrawHelperInterface("UI Manager", shaco.GameHelper.ui, (value) => { shaco.GameHelper.ui = value; return shaco.GameHelper.ui; });
                DrawHelperInterface("UI Depth Change", shaco.GameHelper.uiDepth, (value) => { shaco.GameHelper.uiDepth = value; return shaco.GameHelper.uiDepth; });
                DrawHelperInterface("Resources Loader", shaco.GameHelper.res, (value) => { shaco.GameHelper.res = value; return shaco.GameHelper.res; });
                DrawHelperInterface("Resources Cache", shaco.GameHelper.resCache, (value) => { shaco.GameHelper.resCache = value; return shaco.GameHelper.resCache; });
                DrawHelperInterface("HotUpdate Downloader", shaco.GameHelper.hotupdate, (value) => { shaco.GameHelper.hotupdate = value; return shaco.GameHelper.hotupdate; });
                // DrawHelperInterface("XLua", shaco.XLuaManager, (value) => { shaco.XLuaManager = value; return shaco.XLuaManager; });
                DrawHelperInterface("Http Downloader", shaco.GameHelper.httpDownloader, (value) => { shaco.GameHelper.httpDownloader = value; return shaco.GameHelper.httpDownloader; });
                DrawHelperInterface("Http UpLoader", shaco.GameHelper.httpUpLoader, (value) => { shaco.GameHelper.httpUpLoader = value; return shaco.GameHelper.httpUpLoader; });
                DrawHelperInterface("Sound", shaco.GameHelper.sound, (value) => { shaco.GameHelper.sound = value; return shaco.GameHelper.sound; });
                DrawHelperInterface("Voice", shaco.GameHelper.voice, (value) => { shaco.GameHelper.voice = value; return shaco.GameHelper.voice; });
                DrawHelperInterface("Music", shaco.GameHelper.music, (value) => { shaco.GameHelper.music = value; return shaco.GameHelper.music; });
                DrawHelperInterface("Action", shaco.GameHelper.action, (value) => { shaco.GameHelper.action = value; return shaco.GameHelper.action; });
                DrawHelperInterface("Atlas", shaco.GameHelper.atlas, (value) => { shaco.GameHelper.atlas = value; return shaco.GameHelper.atlas; });
                DrawHelperInterface("AtlasSettings", shaco.GameHelper.atlasSettings, (value) => { shaco.GameHelper.atlasSettings = value; return shaco.GameHelper.atlasSettings; });
            }

            //unity editor
            if (GUILayoutHelper.DrawHeader("UnityEditor", "GameHelperWindow_UnityEditor"))
            {
                DrawHelperInterface("SpineBinaryReader", shacoEditor.GameHelper.spineBinaryReader, (value) => { shacoEditor.GameHelper.spineBinaryReader = value; return shacoEditor.GameHelper.spineBinaryReader; });
            }
        }

        private T DrawHelperInterface<T>(string title, T classTarget, System.Func<T, T> createFunction, params System.Type[] ignoreTypes)
        {
            T retValue = classTarget;
            if (!string.IsNullOrEmpty(_searchNameLower) && !classTarget.ToTypeString().ToLower().Contains(_searchNameLower))
            {
                return retValue;
            }

            GUILayout.BeginHorizontal();
            {
                retValue = GUILayoutHelper.PopupTypeField(title, classTarget, (type) =>
                {
                    var instantiatedValue = (T)type.Instantiate();

                    _configData.WriteString(typeof(T).ToTypeString(), instantiatedValue.ToTypeString());
                    return createFunction(instantiatedValue);
                }, ignoreTypes);
            }
            GUILayout.EndHorizontal();
            return retValue;
        }

        private void OnLoadModeChanged(shaco.ResourcesLoadMode loadMode)
        {
            _prevResourcesLoadMode = loadMode;
            _resourcesLoadMode = loadMode;

            //根据加载模式自动设定读取顺序
            switch (_resourcesLoadMode)
            {
                case shaco.ResourcesLoadMode.RunTime:
                    {
                        _resourcesLoadOrder = shaco.ResourcesLoadOrder.DownloadFirst;
                        shaco.GameHelper.excelSetting.runtimeLoadFileType = shaco.Base.ExcelData.RuntimeLoadFileType.UnityAsset;
                        break;
                    }
                case shaco.ResourcesLoadMode.EditorDevelop:
                    {
                        _resourcesLoadOrder = shaco.ResourcesLoadOrder.ResourcesFirst;
                        shaco.GameHelper.excelSetting.runtimeLoadFileType = shaco.Base.ExcelData.RuntimeLoadFileType.CSV;
                        break;
                    }
                default: shaco.Log.Error("GameHelperWindow error: unsupport load resource mode=" + _resourcesLoadMode); break;
            }

            SaveSettings();
        }
    }
}