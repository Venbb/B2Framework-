using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class BuildHelperWindow : EditorWindow
    {
        public enum IOSExportOption
        {
            AdHoc = 0,
            AppStore = 1,
            Enterprise,
            Development
        }

        public enum IOSCodeIndentity
        {
            Development,
            Distribution
        }

        public enum ProjectUpdateType
        {
            GitHub = 0,
            Svn = 1
        }

        [System.Serializable]
        private class ShacoDefineInfo
        {
            public string define = string.Empty;
            public bool toggle = false;
            public ShacoDefineInfo(string define, bool toggle) { this.define = define; this.toggle = toggle; }
        }

        //配置路径
        // private string _configPath = string.Empty;
        private shaco.Base.IDataSave _dataSave = null;

        //安装包名字
        [SerializeField]
        private string _applicationIdentifier = "com.shaco.test";
        //版本号
        [SerializeField]
        private string _versionCode = "1.0.0";
        //Build号
        [SerializeField]
        private string _buildCode = "1";
        //游戏服务器版本
        [SerializeField]
        private CustomEnumsField _buildServerMode = new CustomEnumsField("DEV", "STG", "PRD");
        //打包宏
        [SerializeField]
        private List<string> _globalDefines = new List<string>();

        //iOS打包身份描述
        [SerializeField]
        private IOSCodeIndentity _iOSCodeIndentity = IOSCodeIndentity.Development;
        //iOS开发者账号team
        [SerializeField]
        private string _iOSDevelopmentTeam = string.Empty;
        //iOS证书描述名字
        [SerializeField]
        private string _iOSProvisioningProfileSpecifier = string.Empty;
        //iOS打包导出ipa的配置
        [SerializeField]
        private IOSExportOption _iOSExportOption = IOSExportOption.AppStore;
        //windows下需要一个可以运行shell的执行工具
        [SerializeField]
        static private string _windowsRunShellExePath = string.Empty;
        //项目更新类型
        [SerializeField]
        private ProjectUpdateType _projectUpdateType = ProjectUpdateType.GitHub;
        //渠道类型
        [SerializeField]
        private CustomEnumsField _channel = new CustomEnumsField();
        //是否为开发模式
        [SerializeField]
        private bool _isDebugSettings = true;

        //Android Keyalias
        [SerializeField]
        private string _androidKeyaliasName = string.Empty;
        [SerializeField]
        private bool _isShowAndroidKeyAliasPass = false;
        [SerializeField]
        private string _androidKeyaliasPass = string.Empty;
        //Android Keystore
        [SerializeField]
        private string _androidKeystoreName = string.Empty;
        [SerializeField]
        private bool _isShowAndroidKeystorePass = false;
        [SerializeField]
        private string _androidKeystorePass = string.Empty;
        [SerializeField]
        private int _toobalIndex = 0;
        private UnityEditor.AnimatedValues.AnimBool _isShowVerionSettings = new UnityEditor.AnimatedValues.AnimBool(true);
        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparatorLeftRight = new shaco.Instance.Editor.TreeView.WindowSplitter();
        private Vector2 _leftWindowScrollPosition = Vector2.zero;
        private Vector2 _rightWindowScrollPosition = Vector2.zero;
        [SerializeField]
        private readonly string[] _shacoDefines = new string[] { "DEBUG_LOG", "DEBUG_WINDOW", "XLUA_ENABLE", "HOTFIX_ENABLE" };
        [SerializeField]
        private List<ShacoDefineInfo> _shacoDefinesToToggle = new List<ShacoDefineInfo>();
        private bool _isVersionCodeChanged = false;

        static public string GetIOSDevelepmentTeam() { return shaco.GameHelper.datasave.ReadString("BuildHelperWindow.Current._iOSCodeIndentity"); }
        static public string GetIOSProvisioningProfileSpecifier() { return shaco.GameHelper.datasave.ReadString("BuildHelperWindow.Current._iOSProvisioningProfileSpecifier"); }
        static public IOSCodeIndentity GetIOSCodeIndentity() { return shaco.GameHelper.datasave.ReadEnum<IOSCodeIndentity>("BuildHelperWindow.Current._iOSCodeIndentity"); }

        private void DrawLeftWindow()
        {
            EditorHelper.RecordObjectWindow(this);

            DrawDebugReleaseTab();

            _isShowVerionSettings.target = EditorHelper.Foldout(_isShowVerionSettings.target, "Version Settings");

            if (_isShowVerionSettings.isAnimating)
                this.Repaint();

            if (EditorGUILayout.BeginFadeGroup(_isShowVerionSettings.faded))
            {
                GUILayout.BeginVertical("box");
                {
                    // DrawConfigPath();
#if UNITY_EDITOR_WIN
                    DrawWindowsRunShellExePath();   
#endif
                    DrawBundleIdentifier();
                    DrawVersion();
                    DrawBuild();
                    DrawServerMode();
                    DrawChannel();
#if UNITY_ANDROID
                    //只有android平台才使用的参数
                    DrawAndroidKeystore();
#endif

#if UNITY_IPHONE
                    //只有ios平台才使用的参数
                    _iOSDevelopmentTeam = EditorGUILayout.TextField("iOS Development Team", _iOSDevelopmentTeam);
                    if (!string.IsNullOrEmpty(_iOSDevelopmentTeam)) _iOSProvisioningProfileSpecifier = EditorGUILayout.TextField("iOS Provisioning Profile", _iOSProvisioningProfileSpecifier);
                    DrawIOSCodeIndentity();
                    DrawIOSExportOption();
#endif

                    _projectUpdateType = (ProjectUpdateType)EditorGUILayout.EnumPopup("Update Type", _projectUpdateType);
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawRightWindow()
        {
            DrawGlobalDefines();
        }

        private void DrawVersions()
        {
            _dragLineSeparatorLeftRight.BeginLayout(true);
            {
                _leftWindowScrollPosition = GUILayout.BeginScrollView(_leftWindowScrollPosition);
                {
                    DrawLeftWindow();
                }
                GUILayout.EndScrollView();
            }
            _dragLineSeparatorLeftRight.EndLayout();

            _dragLineSeparatorLeftRight.BeginLayout();
            {
                _rightWindowScrollPosition = GUILayout.BeginScrollView(_rightWindowScrollPosition);
                {
                    DrawRightWindow();
                }
                GUILayout.EndScrollView();
            }
            _dragLineSeparatorLeftRight.EndLayout();
        }

        private void DrawDebugReleaseTab()
        {
            GUI.changed = false;
            _toobalIndex = GUILayout.Toolbar(_toobalIndex, new string[] { "Debug", "Release" });
            if (GUI.changed)
            {
                //默认取消焦点
                GUI.FocusControl(string.Empty);

                SetDebugReleaseSettings(_toobalIndex == 0);
            }
        }

        //现在同一使用shaco.Base.GameHelper.gameConfig了无需再次设置
        // private void DrawConfigPath()
        // {
        //     GUI.changed = false;
        //     var configPathTmp = GUILayoutHelper.PathField("Config Path", _configPath, "sdata");
        //     if (GUI.changed)
        //     {
        //         SaveSettings();
        //         if (null != configPathTmp)
        //         {
        //             _dataSave.ReloadFromFile(configPathTmp, false);
        //         }
        //         else
        //         {
        //             shaco.GameHelper.datasave.Remove("BuildHelperWindow_configPath");
        //             _dataSave.Clear();
        //             _configPath = null;
        //         }
        //         shaco.GameHelper.datasave.Write("BuildHelperWindow_configPath", configPathTmp);
        //         LoadSettings();
        //     }
        // }

        private void DrawWindowsRunShellExePath()
        {
            _windowsRunShellExePath = GUILayoutHelper.PathField("Run Shell exe", _windowsRunShellExePath, "exe");
            if (string.IsNullOrEmpty(_windowsRunShellExePath))
                EditorGUILayout.HelpBox("Set up a program that can run the shell, for example 'git-bash.exe'", MessageType.Error);
        }

        private void DrawBundleIdentifier()
        {
            GUI.changed = false;
            _applicationIdentifier = EditorGUILayout.TextField("Bundle Identifier", _applicationIdentifier);
            if (GUI.changed)
            {
#if UNITY_5_3_OR_NEWER
                PlayerSettings.SetApplicationIdentifier(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), _applicationIdentifier);
#else
                PlayerSettings.bundleIdentifier = _applicationIdentifier;
#endif
                SaveSettings();
            }
        }

        private void DrawVersion()
        {
            GUI.changed = false;
            var versionCodeTmp = EditorGUILayout.TextField("Version", _versionCode);
            if (GUI.changed)
            {
                SetVersionCode(versionCodeTmp);
                UpdateVersionCode();
            }
        }

        private void DrawBuild()
        {
            GUI.changed = false;
            var oldBuildCode = _buildCode;
            _buildCode = EditorGUILayout.TextField("Build", _buildCode);
            if (GUI.changed)
            {
                int resultTmp = 0;
                if (string.IsNullOrEmpty(_buildCode))
                {
                    //ignore case
                }
                else if (!int.TryParse(_buildCode, out resultTmp))
                {
                    _buildCode = oldBuildCode;
                    Debug.LogWarning("BuildHelperWindow+Version DrawVersion warning: build code must be integer");
                }
                else
                {
                    UpdateBuildCode();
                }
            }
        }

        private void DrawServerMode()
        {
            _buildServerMode.DrawEnums("Server Mode");
        }

        private void DrawChannel()
        {
            _channel.DrawEnums("Channel");
        }

        private void DrawAndroidKeystore()
        {
            GUI.changed = false;
            _androidKeystoreName = EditorGUILayout.TextField("Android keystoreName", _androidKeystoreName);
            if (GUI.changed)
            {
                PlayerSettings.Android.keystoreName = _androidKeystoreName;
            }

            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                if (!_isShowAndroidKeystorePass)
                    _androidKeystorePass = EditorGUILayout.PasswordField("Android keystorePass", _androidKeystorePass);
                else
                    _androidKeystorePass = EditorGUILayout.TextField("Android keystorePass", _androidKeystorePass);
                if (GUI.changed)
                {
                    PlayerSettings.Android.keystorePass = _androidKeystorePass;
                }

                _isShowAndroidKeystorePass = EditorGUILayout.Toggle(_isShowAndroidKeystorePass, GUILayout.Width(15));
            }
            GUILayout.EndHorizontal();

            GUI.changed = false;
            _androidKeyaliasName = EditorGUILayout.TextField("Android keyaliasName", _androidKeyaliasName);
            if (GUI.changed)
            {
                PlayerSettings.Android.keyaliasName = _androidKeyaliasName;
            }

            GUILayout.BeginHorizontal();
            {
                GUI.changed = false;
                if (!_isShowAndroidKeyAliasPass)
                    _androidKeyaliasPass = EditorGUILayout.PasswordField("Android keyaliasPass", _androidKeyaliasPass);
                else
                    _androidKeyaliasPass = EditorGUILayout.TextField("Android keyaliasPass", _androidKeyaliasPass);
                if (GUI.changed)
                {
                    PlayerSettings.Android.keyaliasPass = _androidKeyaliasPass;
                }

                _isShowAndroidKeyAliasPass = EditorGUILayout.Toggle(_isShowAndroidKeyAliasPass, GUILayout.Width(15));
            }
            GUILayout.EndHorizontal();
        }

        private void DrawIOSCodeIndentity()
        {
            GUI.changed = false;
            var newCodeIndentity = (IOSCodeIndentity)EditorGUILayout.EnumPopup("iOS Code Indentity", _iOSCodeIndentity);
            if (GUI.changed)
            {
                _iOSCodeIndentity = newCodeIndentity;

                //当代码描述配置为开发模式的时候，导出配置也需要为开发模式才能导出ipa
                if (_iOSCodeIndentity == IOSCodeIndentity.Development)
                {
                    _iOSExportOption = IOSExportOption.Development;
                }
            }
        }

        private void DrawIOSExportOption()
        {
            if (!string.IsNullOrEmpty(_iOSDevelopmentTeam) && !string.IsNullOrEmpty(_iOSProvisioningProfileSpecifier) && _iOSCodeIndentity != IOSCodeIndentity.Development)
            {
                var newOption = (IOSExportOption)EditorGUILayout.EnumPopup("iOS Export Option", _iOSExportOption);
                if (GUI.changed)
                {
                    if (_iOSCodeIndentity == IOSCodeIndentity.Development && newOption != IOSExportOption.Development)
                    {
                        newOption = IOSExportOption.Development;
                    }
                    _iOSExportOption = newOption;
                }
            }
        }

        private bool DrawGlobalShacoDefineToggle(string prefix, bool value, string define)
        {
            GUI.changed = false;
            value = EditorGUILayout.Toggle(prefix, value);
            if (GUI.changed)
            {
                if (!value)
                    _globalDefines.Remove(define);
                else
                {
                    if (!_globalDefines.Contains(define))
                        _globalDefines.Add(define);
                }
            }
            return value;
        }

        private void DrawGlobalDefines()
        {
            foreach (var iter in _shacoDefinesToToggle)
            {
                iter.toggle = DrawGlobalShacoDefineToggle(iter.define, iter.toggle, iter.define);
            }

            System.Func<string, string, bool> onCheckValueChangeCallBack = (v1, v2) =>
            {
                //当v1和v2都为空的时候是clear数组了
                if (v1 == null && v2 == null)
                {
                    foreach (var iter in _shacoDefinesToToggle)
                    {
                        iter.toggle = false;
                    }
                }
                else
                {
                    var v1Find = _shacoDefinesToToggle.Find(v => v1 == v.define);
                    if (null != v1 && null != v1Find)
                        v1Find.toggle = false;

                    var v2Find = _shacoDefinesToToggle.Find(v => v2 == v.define);
                    if (null != v2 && null != v2Find)
                        v2Find.toggle = false;
                }
                return true;
            };

            GUI.changed = false;
            GUILayoutHelper.DrawStringList(_globalDefines, "Global Defines", onCheckValueChangeCallBack, () =>
            {
                if (GUILayout.Button("UpdateProjectDefines", GUILayout.Width(140)))
                {
                    UpdateProjectDefines();
                }
            });
            if (GUI.changed)
            {
                UpdateShacoDefinesToggle();
            }
        }

        private void UpdateShacoDefinesToggle()
        {
            //刷新shaco框架自带宏标记
            foreach (var iter in _shacoDefinesToToggle)
            {
                iter.toggle = _globalDefines.Contains(iter.define);
            }
        }

        private void UpdateProjectSettings()
        {
#if UNITY_5_3_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), _applicationIdentifier);
#else
            PlayerSettings.bundleIdentifier = _applicationIdentifier;
#endif
            UpdateVersionCode();
            UpdateBuildCode();

            PlayerSettings.Android.keystoreName = _androidKeystoreName;
            PlayerSettings.Android.keystorePass = _androidKeystorePass;
            PlayerSettings.Android.keyaliasName = _androidKeyaliasName;
            PlayerSettings.Android.keyaliasPass = _androidKeyaliasPass;
        }

        private void SetDebugReleaseSettings(bool isDebug)
        {
            SaveSettings();
            _isDebugSettings = isDebug;
            _dataSave.WriteBool(GetSaveKey("_isDebugSettings"), _isDebugSettings);
            LoadSettings();

            //已经在打包的option中设置过了，不用再次设定
            // EditorUserBuildSettings.development = isDebug;
            // EditorUserBuildSettings.allowDebugging = isDebug;
            // EditorUserBuildSettings.connectProfiler = isDebug;

            Debug.Log("BuildHelperWindow+Version SetDebugReleaseSettings: isDebug=" + isDebug);
#if UNITY_WEBGL
            // EditorUserBuildSettings.webGLUsePreBuiltUnityEngine = true;
            // PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
#endif
        }

        private bool SetChannel(string channel)
        {
            return _channel.SetEnum(channel);
        }

        private bool SetServerMode(string serverMode)
        {
            return _buildServerMode.SetEnum(serverMode);
        }

        private void SetVersionCode(string versionCode)
        {
            if (_versionCode == versionCode)
                return;

            _versionCode = versionCode;

            if (null != EditorHelper.GetBuildVerionFile())
            {
                EditorHelper.SetBuildVersionFile(versionCode);
                _isVersionCodeChanged = true;
            }
        }

        private void UpdateProjectDefines()
        {
            var globalDefinesTmp = _globalDefines.ToContactString(";");
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), globalDefinesTmp);
        }

        private void SaveSettings()
        {
            if (_isVersionCodeChanged)
            {
                _isVersionCodeChanged = false;
                AssetDatabase.Refresh();
            }

            // shaco.GameHelper.datasave.Write("BuildHelperWindow_configPath", _configPath);
            shaco.GameHelper.datasave.WriteString(GetSaveKey("_windowsRunShellExePath"), _windowsRunShellExePath);

            _dataSave.WriteBool(GetSaveKey("_isShowVerionSettings"), _isShowVerionSettings.value);
            _dataSave.WriteBool(GetSaveKey("_isDebugSettings"), _isDebugSettings);
            _dataSave.WriteInt(GetSaveKey("_buildServerMode"), (int)_buildServerMode.currentSelectIndex);
            _dataSave.WriteString(GetSaveKey("_buildServerModeDisplay"), _buildServerMode.GetEnumsDisplay().ToSplit(","));
            _dataSave.WriteEnum(GetSaveKey("_projectUpdateType"), _projectUpdateType);

            _dataSave.WriteInt(GetFullSaveKey("_currentSelectChannel"), _channel.currentSelectIndex);
            _dataSave.WriteString(GetFullSaveKey("_channelsDisplay"), _channel.GetEnumsDisplay().ToSplit(","));
            _dataSave.WriteString(GetFullSaveKey("_androidKeyaliasName"), _androidKeyaliasName);
            _dataSave.WriteString(GetFullSaveKey("_androidKeyaliasPass"), _androidKeyaliasPass);
            _dataSave.WriteString(GetFullSaveKey("_androidKeystoreName"), _androidKeystoreName);
            _dataSave.WriteString(GetFullSaveKey("_androidKeystorePass"), _androidKeystorePass);
            _dataSave.WriteEnum(GetFullSaveKey("_iOSCodeIndentity"), _iOSCodeIndentity);
            _dataSave.WriteString(GetFullSaveKey("_iOSDevelopmentTeam"), _iOSDevelopmentTeam);
            _dataSave.WriteString(GetFullSaveKey("_iOSProvisioningProfileSpecifier"), _iOSProvisioningProfileSpecifier);
            _dataSave.WriteString(GetFullSaveKeyWithChannel("_globalDefines"), _globalDefines.ToContactString(";"));
            _dataSave.WriteString(GetFullSaveKey("_applicationIdentifier"), _applicationIdentifier);
            _dataSave.WriteInt(GetFullSaveKey("_iOSExportOption"), (int)_iOSExportOption);

            _dataSave.WriteEnum("BuildHelperWindow.Current._iOSCodeIndentity", _iOSCodeIndentity);
            _dataSave.WriteString("BuildHelperWindow.Current._iOSDevelopmentTeam", _iOSDevelopmentTeam);
            _dataSave.WriteString("BuildHelperWindow.Current._iOSProvisioningProfileSpecifier", _iOSProvisioningProfileSpecifier);

            _dataSave.CheckSaveModifyData();
        }

        private void InitSettings()
        {
            _dataSave = shaco.Base.GameHelper.gameConfig;

            LoadSettings();

            _channel.onEnumWillChangeCallBack = () => { SaveSettings(); };
            _channel.onEnumChangedCallBack = () =>
            {
                _dataSave.WriteInt(GetFullSaveKey("_currentSelectChannel"), _channel.currentSelectIndex);
                _dataSave.WriteString(GetFullSaveKey("_channelsDisplay"), _channel.GetEnumsDisplay().ToSplit(","));
                LoadSettings();
            };

            _buildServerMode.onEnumWillChangeCallBack = () => { SaveSettings(); };
            _buildServerMode.onEnumChangedCallBack = () =>
            {
                _dataSave.WriteInt(GetSaveKey("_buildServerMode"), (int)_buildServerMode.currentSelectIndex);
                _dataSave.WriteString(GetSaveKey("_buildServerModeDisplay"), _buildServerMode.GetEnumsDisplay().ToSplit(","));
                LoadSettings();
            };

            _dragLineSeparatorTopDowwn.SetSplitWindow(this, 0.7f, 0.3f);
            _dragLineSeparatorLeftRight.SetSplitWindow(this, 0.5f, 0.5f);

            //当宏设置为空的时候，自动匹配BuildSettings中已经设置过的shaco框架宏
            if (_globalDefines.IsNullOrEmpty())
            {
                var globalDefinedInBuildSetting = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)).Split(";");
                for (int i = globalDefinedInBuildSetting.Length - 1; i >= 0; --i)
                {
                    if (!_globalDefines.Contains(globalDefinedInBuildSetting[i]))
                    {
                        _globalDefines.Add(globalDefinedInBuildSetting[i]);
                    }
                }
            }

            //根据shaco框架宏设定toggle显示默认状态
            for (int i = 0; i < _shacoDefines.Length; ++i)
            {
                var findTmp = _shacoDefinesToToggle.Find(v => v.define == _shacoDefines[i]);
                var toogleEnabled = _globalDefines.Contains(_shacoDefines[i]);
                if (null == findTmp)
                    _shacoDefinesToToggle.Add(new ShacoDefineInfo(_shacoDefines[i], toogleEnabled));
                else
                    findTmp.toggle = toogleEnabled;
            }

            //监听Unity undo事件
            Undo.undoRedoPerformed -= UpdateProjectSettings;
            Undo.undoRedoPerformed += UpdateProjectSettings;
        }

        private void LoadSettings()
        {
            // _dataSave.Clear();

            _versionCode = PlayerSettings.bundleVersion;
            _buildCode = GetBuildCode();

            // _configPath = shaco.GameHelper.datasave.ReadString("BuildHelperWindow_configPath");
            // if (null != _configPath)
            // {
            //     _dataSave.ReloadFromFile(shaco.GameHelper.datasave.ReadString("BuildHelperWindow_configPath"), false);
            // }

            _windowsRunShellExePath = shaco.GameHelper.datasave.ReadString(GetSaveKey("_windowsRunShellExePath"));

            _isShowVerionSettings.value = _dataSave.ReadBool(GetSaveKey("_isShowVerionSettings"), true);
            _isDebugSettings = _dataSave.ReadBool(GetSaveKey("_isDebugSettings"), true);
            _buildServerMode.currentSelectIndex = _dataSave.ReadInt(GetSaveKey("_buildServerMode"));
            _buildServerMode.SetEnumDisplay(_dataSave.ReadString(GetSaveKey("_buildServerModeDisplay"), "DEV,STG,PRD").Split(","));
            _projectUpdateType = _dataSave.ReadEnum<ProjectUpdateType>(GetSaveKey("_projectUpdateType"));
            _channel.currentSelectIndex = _dataSave.ReadInt(GetFullSaveKey("_currentSelectChannel"));
            _channel.SetEnumDisplay(_dataSave.ReadString(GetFullSaveKey("_channelsDisplay"), "Default").Split(","));
            _androidKeyaliasName = _dataSave.ReadString(GetFullSaveKey("_androidKeyaliasName"), PlayerSettings.Android.keyaliasName);
            _androidKeyaliasPass = _dataSave.ReadString(GetFullSaveKey("_androidKeyaliasPass"), PlayerSettings.Android.keyaliasPass);
            _androidKeystoreName = _dataSave.ReadString(GetFullSaveKey("_androidKeystoreName"), PlayerSettings.Android.keystoreName);
            _androidKeystorePass = _dataSave.ReadString(GetFullSaveKey("_androidKeystorePass"), PlayerSettings.Android.keystorePass);
            _iOSCodeIndentity = _dataSave.ReadEnum<IOSCodeIndentity>(GetFullSaveKey("_iOSCodeIndentity"));
            _iOSDevelopmentTeam = _dataSave.ReadString(GetFullSaveKey("_iOSDevelopmentTeam"));
            _iOSProvisioningProfileSpecifier = _dataSave.ReadString(GetFullSaveKey("_iOSProvisioningProfileSpecifier"));
            _globalDefines = _dataSave.ReadString(GetFullSaveKeyWithChannel("_globalDefines")).Split(";").ToList();
            _applicationIdentifier = _dataSave.ReadString(GetFullSaveKey("_applicationIdentifier"), string.Empty);
            _iOSExportOption = _dataSave.ReadEnum<IOSExportOption>(GetFullSaveKey("_iOSExportOption"));

            //设置安装包名字 
            if (string.IsNullOrEmpty(_applicationIdentifier))
            {
#if UNITY_5_3_OR_NEWER
                _applicationIdentifier = PlayerSettings.applicationIdentifier;
#else
                _applicationIdentifier = PlayerSettings.bundleIdentifier;
#endif
            }
#if UNITY_5_3_OR_NEWER
            PlayerSettings.SetApplicationIdentifier(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), _applicationIdentifier);
#else
            PlayerSettings.bundleIdentifier = _applicationIdentifier;
#endif

            //设置debug标签下标
            _toobalIndex = _isDebugSettings ? 0 : 1;

            _channel.UpdateEnumsDisplay();
            UpdateShacoDefinesToggle();
        }

        private void UpdateVersionCode()
        {
            PlayerSettings.bundleVersion = _versionCode;
        }

        private void UpdateBuildCode()
        {
#if UNITY_5_3_OR_NEWER
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android: PlayerSettings.Android.bundleVersionCode = _buildCode.ToInt(); break;
                case BUILD_TARGET_IOS: PlayerSettings.iOS.buildNumber = _buildCode; break;
                case BuildTarget.WebGL: break;
                default: Debug.LogError("BuildHelperWindow+Version UpdateBuildCode error: unsupport type=" + EditorUserBuildSettings.activeBuildTarget); break;
            }
#else
            PlayerSettings.shortBundleVersion = _buildCode;
            PlayerSettings.Android.bundleVersionCode = _buildCode.ToInt();
#endif
        }

        private string GetBuildCode()
        {
#if UNITY_5_3_OR_NEWER
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.Android: return PlayerSettings.Android.bundleVersionCode.ToString();
                case BUILD_TARGET_IOS: return PlayerSettings.iOS.buildNumber;
                case BuildTarget.WebGL: return string.Empty;

#if !UNITY_2018_1_OR_NEWER
                case BuildTarget.StandaloneOSXIntel: return PlayerSettings.macOS.buildNumber;
                case BuildTarget.StandaloneOSXIntel64: return PlayerSettings.macOS.buildNumber;
                case BuildTarget.StandaloneOSXUniversal: return PlayerSettings.macOS.buildNumber;
#else
                case BuildTarget.StandaloneOSX: return PlayerSettings.macOS.buildNumber;
#endif
                case BuildTarget.StandaloneWindows: return string.Empty;
                case BuildTarget.StandaloneWindows64: return string.Empty;
                default: Debug.LogError("BuildHelperWindow+Version GetBuildCode error: unsupport type=" + EditorUserBuildSettings.activeBuildTarget); return string.Empty;
            }
#else
            return PlayerSettings.shortBundleVersion;
#endif
        }

        private string GetSaveKey(string key)
        {
            return this.ToTypeString() + key;
        }

        private string GetFullSaveKey(string key)
        {
            return this.ToTypeString() + key + (_isDebugSettings ? "_Debug" : "_Release") + "_" + _buildServerMode.GetCurrentEnumString();
        }

        private string GetFullSaveKeyWithChannel(string key)
        {
            return this.ToTypeString() + key + (_isDebugSettings ? "_Debug" : "_Release") + "_" + _channel.GetCurrentEnumString() + "_" + _buildServerMode.GetCurrentEnumString();
        }
    }
}