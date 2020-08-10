using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace shacoEditor
{
    public partial class BuildHelperWindow : EditorWindow
    {
        static private BuildHelperWindow _currentWindow = null;
        private Vector2 _topWindowScrollPosition = Vector2.zero;
        private Vector2 _downWindowScrollPosition = Vector2.zero;
        private bool _isBuilding = false;

#if UNITY_5_3_OR_NEWER
        private const BuildTarget BUILD_TARGET_IOS = BuildTarget.iOS;
#else
        private const BuildTarget BUILD_TARGET_IOS = BuildTarget.iPhone;
#endif

        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparatorTopDowwn = new shaco.Instance.Editor.TreeView.WindowSplitter(shaco.Instance.Editor.TreeView.WindowSplitter.Direction.Vertical);
        private Dictionary<BuildTarget, GUIContent> _buildButtonsTile = new Dictionary<BuildTarget, GUIContent>();

        [MenuItem("shaco/Tools/BuildHelper %&b", false, (int)ToolsGlobalDefine.MenuPriority.Tools.BUILD_WINDOW)]
        static void OpenBuildHelperWindow()
        {
            _currentWindow = shacoEditor.EditorHelper.GetWindow<BuildHelperWindow>(null, true, "BuildHelperWindow");
        }

        void OnEnable()
        {
            _currentWindow = shacoEditor.EditorHelper.GetWindow<BuildHelperWindow>(this, true, "BuildHelperWindow");
            _currentWindow.Init();
        }

        void OnDestroy()
        {
            _currentWindow.SaveSettings();
            Undo.undoRedoPerformed -= UpdateProjectSettings;
        }

        void OnGUI()
        {
            if (_currentWindow == null)
                return;

            EditorGUI.BeginDisabledGroup(_isBuilding);
            {
                _dragLineSeparatorTopDowwn.BeginLayout(true);
                {
                    _topWindowScrollPosition = GUILayout.BeginScrollView(_topWindowScrollPosition);
                    {
                        DrawTopWindow();
                    }
                    GUILayout.EndScrollView();
                }
                _dragLineSeparatorTopDowwn.EndLayout();

                _dragLineSeparatorTopDowwn.BeginLayout();
                {
                    _downWindowScrollPosition = GUILayout.BeginScrollView(_downWindowScrollPosition);
                    {
                        DrawDownWindow();
                    }
                    GUILayout.EndScrollView();
                }
                _dragLineSeparatorTopDowwn.EndLayout();
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawTopWindow()
        {
            DrawVersions();
        }

        private bool CanBuildPackage()
        {
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            return true;
#else
            return !string.IsNullOrEmpty(_windowsRunShellExePath);
#endif
        }

        private void DrawDownWindow()
        {
            EditorGUI.BeginDisabledGroup(!CanBuildPackage());
            {
                GUILayout.BeginHorizontal("box");
                {
                    var currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;
#if UNITY_ANDROID
                    if (GUILayout.Button(_buildButtonsTile[BuildTarget.Android], GUILayout.Height(50)))
                    {
                        StartBuildProcessReady(BuildTarget.Android, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), true, _isDebugSettings);
                    }
                    EditorHelper.DrawForegourndFrame();
#endif

#if UNITY_IOS || UNITY_IPHONE
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                    if (GUILayout.Button(_buildButtonsTile[BUILD_TARGET_IOS], GUILayout.Height(50)))
                    {
                        StartBuildProcessReady(BUILD_TARGET_IOS, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), true, _isDebugSettings);
                    }
                    EditorHelper.DrawForegourndFrame();
                    GUILayout.Label("iPA");
#endif
                    if (GUILayout.Button(_buildButtonsTile[BUILD_TARGET_IOS], GUILayout.Height(50)))
                    {
                        StartBuildProcessReady(BUILD_TARGET_IOS, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), false, _isDebugSettings);
                    }
                    EditorHelper.DrawForegourndFrame();
                    GUILayout.Label("Xcode");
#endif

#if UNITY_WEBGL
                    if (GUILayout.Button(_buildButtonsTile[BuildTarget.WebGL], GUILayout.Height(50)))
                    {
                        StartBuildProcessReady(BuildTarget.WebGL, _buildServerMode.GetCurrentEnumString(), _channel.GetCurrentEnumString(), true, _isDebugSettings);
                    }
#endif
                }
                GUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            //现在同一使用shaco.Base.GameHelper.gameConfig了无需再次设置
            // if (GUILayout.Button("Save Settings"))
            // {
            //     var selectPath = EditorUtility.SaveFilePanel("Select a path", Application.dataPath, "BuildConfiguration", "sdata");
            //     if (!string.IsNullOrEmpty(selectPath))
            //     {
            //         _dataSave.SaveAsFile(selectPath);
            //         _configPath = selectPath;
            //         if (selectPath.Contains(Application.dataPath))
            //         {
            //             AssetDatabase.Refresh();
            //         }
            //     }
            // }

            // if (GUILayout.Button("Load Settings"))
            // {
            //     var pathSelect = EditorUtility.OpenFilePanel("Select a Data Config", Application.dataPath, shaco.Base.GlobalParams.DATA_SAVE_EXTENSIONS);
            //     if (!string.IsNullOrEmpty(pathSelect))
            //     {
            //         _dataSave.ReloadFromFile(pathSelect, false);
            //         _configPath = pathSelect;
            //         SaveSettings();
            //         LoadSettings();
            //     }
            // }

            if (GUILayout.Button("Reset Settings"))
            {
                bool checkContinue = EditorUtility.DisplayDialog("Warning", "Should Reset All Settings ?", "Continue", "Cancel");
                if (checkContinue)
                {
                    _dataSave.RemoveStartWith(this.ToTypeString());
                    InitSettings();
                }
            }
        }

        private void Init()
        {
            _buildButtonsTile.Clear();

            var allTargets = shaco.Base.Utility.ToEnums<BuildTarget>();
            // System.Reflection.MethodInfo loadIconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon", 
            //     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, 
            //     null, 
            //     new System.Type[] { typeof(string)}, null);

            for (int i = allTargets.Length - 1; i >= 0; --i)
            {
                var buildTargetTmp = allTargets[i];
                if (!_buildButtonsTile.ContainsKey(allTargets[i]))
                {
                    var buildTargetName = buildTargetTmp == BUILD_TARGET_IOS ? "iPhone" : buildTargetTmp.ToString();
                    var iconName = "BuildSettings." + buildTargetName;
                    var loadTextureTmp = EditorGUIUtility.FindTexture(iconName);
                    var newContentTitle = new GUIContent("Build " + buildTargetTmp.ToString(), loadTextureTmp);
                    _buildButtonsTile.Add(buildTargetTmp, newContentTitle);
                }
            }
            InitSettings();
        }
    }
}