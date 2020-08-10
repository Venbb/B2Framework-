using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class BehaviourTreePreviewWindow : EditorWindow
    {
        private readonly Vector2 MIN_ITEM_MARGIN = new Vector2(10, 10);
        private const float INIT_DEFAULT_GUI_WIDTH = 250;

        private shaco.Base.BehaviourRootTree _rootTree = new shaco.Base.BehaviourRootTree();
        private string _assetBehaviourTreeGUID = string.Empty;
        private string _assetBehaviourTreePath = string.Empty;
        private float _defaultGUIWidth = INIT_DEFAULT_GUI_WIDTH;
        private float _defaultGUIHeight = 20;
        private Rect _maxTreeItemRect = new Rect();
        private Vector2 _scrollPossition = Vector2.zero;
        private bool _updateFormatPositionsDirty = false;
        private bool _updateFormatPositionsInNextLoopDirty = false;
        private Vector2 _rootTreeMoveToCenterOffset = Vector2.zero;
        private string _searchTreeName = string.Empty;
        private bool _shouldHideRootTreeFilePath = false;
        private float _arrowActionPercent = 0;
        private bool _isShowMiniUI = false;
        private float _miniUIScale = 1.0f;
        private bool _isUpdatingAllClassNameInScript = false;

        [MenuItem("shaco/Viewer/BehaviourTreePreview " + ToolsGlobalDefine.MenuPriority.ViewerShortcutKeys.BEHAVIOUR_TREE, false, (int)ToolsGlobalDefine.MenuPriority.Viewer.BEHAVIOUR_TREE)]
        static public void OpenBehaviourTreePreviewWindow()
        {
            EditorHelper.GetWindow<BehaviourTreePreviewWindow>(null, true, "BehaviourTreePreview");
        }

        void OnEnable()
        {
            EditorHelper.GetWindow<BehaviourTreePreviewWindow>(this, true, "BehaviourTreePreview").Init();
        }

        static public BehaviourTreePreviewWindow OpenBehaviourTreePreviewWindow(shaco.Base.BehaviourRootTree rootTree)
        {
            if (null != rootTree)
            {
                var window = EditorHelper.GetWindow<BehaviourTreePreviewWindow>(null, true, "BehaviourTreePreview");
                window._rootTree = rootTree;
                window._shouldHideRootTreeFilePath = true;
                window.Init();
                return window;
            }
            else
            {
                Debug.LogError("BehaviourTreePreviewWindow OpenBehaviourTreePreviewWindow error: root tree is invalid");
                return null;
            }
        }

        void OnDestroy()
        {
            Exit();
        }

        public void Draw()
        {
            if (!_shouldHideRootTreeFilePath)
            {
                GUILayout.BeginHorizontal();
                {
                    GUI.changed = false;
                    this._isShowMiniUI = EditorGUILayout.Toggle("Show Mini UI", this._isShowMiniUI);
                    if (GUI.changed)
                    {
                        if (!_isShowMiniUI)
                            _defaultGUIWidth = INIT_DEFAULT_GUI_WIDTH;
                    }
                    if (_isShowMiniUI)
                    {
                        GUILayout.Space(50);
                        GUI.changed = false;
                        _miniUIScale = GUILayout.HorizontalSlider(_miniUIScale, 0, 1, GUILayout.Width(200));
                        if (GUI.changed)
                        {
                            _defaultGUIWidth = INIT_DEFAULT_GUI_WIDTH * _miniUIScale;
                        }
                    }
                    DrawSearchField();
                }
                GUILayout.EndHorizontal();

                GUI.changed = false;
                _assetBehaviourTreePath = GUILayoutHelper.PathField("Asset Path", _assetBehaviourTreePath, "json");
                if (GUI.changed)
                {
                    _assetBehaviourTreeGUID = AssetDatabase.AssetPathToGUID(EditorHelper.FullPathToUnityAssetPath(_assetBehaviourTreePath));
                    UpdateTree();
                    _updateFormatPositionsDirty = true;
                }
            }

            if (_updateFormatPositionsDirty)
            {
                FormatTreesPosition();
                _updateFormatPositionsDirty = false;
            }

            var windowRect = new Rect(0, _defaultGUIHeight * 2, this.position.width, this.position.height - _defaultGUIHeight * 2);
            _scrollPossition = GUI.BeginScrollView(windowRect, _scrollPossition, _maxTreeItemRect);
            {
                DrawTrees();
            }
            GUI.EndScrollView();

            DrawTreeLinkLines();

            if (_updateFormatPositionsInNextLoopDirty && Event.current.type == EventType.Layout)
            {
                _updateFormatPositionsInNextLoopDirty = false;
                _updateFormatPositionsDirty = true;
            }

            UpdateEvent();
        }

        void OnGUI()
        {
            this.Repaint();
            this.Draw();

            _arrowActionPercent += 0.01f;
            if (_arrowActionPercent > 1.0f)
                _arrowActionPercent = 0;
        }

        private void DrawSearchField()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                GUI.changed = false;
                _searchTreeName = GUILayoutHelper.SearchField(_searchTreeName, GUILayout.Width(this.position.width / 5));
                if (GUI.changed)
                {
                    if (!string.IsNullOrEmpty(_searchTreeName))
                    {
                        shaco.Base.BehaviourTree findTree = null;
                        var searchTreeNameLower = _searchTreeName.ToLower();
                        _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
                        {
                            if (tree.displayName.ToLower().Contains(searchTreeNameLower) || tree.processTypeName.ToLower().Contains(searchTreeNameLower))
                            {
                                findTree = tree;
                                return false;
                            }
                            else
                                return true;
                        });
                        if (null != findTree)
                        {
                            _scrollPossition = findTree.editorDrawPosition.position - _maxTreeItemRect.position - this.position.size / 2 + findTree.editorDrawPosition.size / 2;
                            ForceSelectTrees(new shaco.Base.BehaviourTree[] { findTree });
                        }
                    }
                    else
                    {
                        ForceUnSelectTrees();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void DrawTrees()
        {
            if (null == _rootTree)
                return;

            DrawTree(_rootTree, 0, 0);
            _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
            {
                DrawTree(tree, index, level);
                return true;
            });
        }

        private void DrawTree(shaco.Base.BehaviourTree tree, int index, int level)
        {
            bool isSelectedTree = IsSelectedTree(tree) && tree != _currentDragTargetParentTree && !tree.IsRoot();
            var oldColor = GUI.color;

            if (isSelectedTree)
                GUI.color = Color.green;

            GUI.Box(tree.editorDrawPosition, string.Empty);

            GUILayout.BeginArea(tree.editorDrawPosition);
            {
                if (tree.editorHasInValidParam)
                {
                    EditorGUILayout.HelpBox("Has invalid param", MessageType.Warning);
                }

                if (!tree.IsRoot())
                {
                    GUI.changed = false;
                    EditorGUI.BeginDisabledGroup(_currentDragTrees.Count > 0);
                    {
                        tree.editorAssetProcess = (TextAsset)EditorGUILayout.ObjectField(tree.editorAssetProcess, typeof(TextAsset), true);
                    }
                    EditorGUI.EndDisabledGroup();
                    if (GUI.changed)
                    {
                        if (null == tree.editorAssetProcess)
                        {
                            tree.processTypeName = string.Empty;
                        }
                        else
                            CheckScriptAssetChanged(tree);
                    }

                    if (!_isShowMiniUI)
                    {
                        GUILayout.BeginHorizontal();
                        {
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                EditorGUILayout.ObjectField(tree.editorAssetTree, typeof(TextAsset), true);
                            }
                            EditorGUI.EndDisabledGroup();
                            if (GUILayout.Button("Change", GUILayout.Width(55)))
                            {
                                ShowChangeTreeTypeMenu(tree);
                            }
                        }
                        GUILayout.EndHorizontal();

                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.TextField("Class: " + tree.processTypeName);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                }

                if (tree.root == tree)
                {
                    DrawRootTree(tree as shaco.Base.BehaviourRootTree);
                }
                else
                {
                    DrawTreeType(tree);
                }
                tree.OnGUIDraw();
                FixFormatTreeSizeWithGUILayout(tree, _defaultGUIWidth);
            }
            GUILayout.EndArea();

            if (isSelectedTree)
                GUI.color = oldColor;

            if (tree == _currentDragTargetParentTree)
                DragDragParentTree(tree);
        }

        private void DragDragParentTree(shaco.Base.BehaviourTree tree)
        {
            var oldColor = GUI.color;
            GUI.color = Color.green;

            var areaOffset = new Vector2(tree.editorDrawPosition.width * DRAG_OFFSET_AREA.x, tree.editorDrawPosition.height * DRAG_OFFSET_AREA.y);
            var rectPos = tree.editorDrawPosition;

            switch (_currentDragTargetDirection)
            {
                case shaco.Direction.Up:
                    {
                        var tmpVec = new Vector2(rectPos.width, areaOffset.y);
                        GUI.Button(new Rect(rectPos.position.x, rectPos.position.y, tmpVec.x, tmpVec.y), "Up");
                        break;
                    }
                case shaco.Direction.Down:
                    {
                        var tmpVec1 = rectPos.position + new Vector2(0, rectPos.height - areaOffset.y);
                        var tmpVec2 = new Vector2(rectPos.width, areaOffset.y);
                        GUI.Button(new Rect(tmpVec1.x, tmpVec1.y, tmpVec2.x, tmpVec2.y), "Down");
                        break;
                    }
                case shaco.Direction.Left:
                    {
                        var tmpVec = new Vector2(areaOffset.x, rectPos.height);
                        GUI.Button(new Rect(rectPos.position.x, rectPos.position.y, tmpVec.x, tmpVec.y), "Left");
                        break;
                    }
                case shaco.Direction.Right:
                    {
                        var tmpVec1 = rectPos.position + new Vector2(rectPos.width - areaOffset.x, 0);
                        var tmpVec2 = new Vector2(areaOffset.x, rectPos.height);
                        GUI.Button(new Rect(tmpVec1.x, tmpVec1.y, tmpVec2.x, tmpVec2.y), "Right");
                        break;
                    }
                default:
                    Debug.LogError("unsupport direction");
                    break;
            }

            GUI.color = oldColor;
        }

        private void DrawTreeLinkLines()
        {
            if (null == _rootTree)
                return;

            Color oldColor = GUI.color;

            _rootTree.ForeachAllChildren((shaco.Base.BehaviourTree tree, int index, int level) =>
            {
                var rect1 = ((shaco.Base.BehaviourTree)tree.parent).editorDrawPosition;
                var rect2 = tree.editorDrawPosition;
                var offsetPos = GetDrawOffset();
                var point1 = new Vector3(rect1.center.x + offsetPos.x, +rect1.yMax + offsetPos.y);
                var point2 = new Vector3(rect2.center.x + offsetPos.x, +rect2.yMin + offsetPos.y);

                bool isRunning = tree.IsRunningRecentInEditor();
                if (isRunning)
                {
                    GUI.color = Color.green;
                    GUIHelper.LineDraw.DrawLineWithArrow(point1, point2, 1, 10, _arrowActionPercent);
                    GUI.color = oldColor;
                }
                else
                {
                    GUIHelper.LineDraw.DrawLine(point1, point2, 1);
                }

                return true;
            });
        }

        private Vector2 GetDrawOffset()
        {
            return new Vector2(-_maxTreeItemRect.x - _scrollPossition.x, -_maxTreeItemRect.y - _scrollPossition.y + _defaultGUIHeight * 2);
        }

        protected void Init()
        {
            LoadSettings();
            _updateFormatPositionsDirty = true;
        }

        private void Exit()
        {
            SaveSetings();
        }

        private void SaveSetings()
        {
            shaco.GameHelper.datasave.WriteString("BehaviourTreePreviewWindow.BehaviourtTree", _assetBehaviourTreeGUID);
            shaco.GameHelper.datasave.WriteBool("BehaviourTreePreviewWindow._isShowMiniUI", this._isShowMiniUI);
        }

        private void LoadSettings()
        {
            if (_rootTree.Count == 0)
            {
                _assetBehaviourTreeGUID = shaco.GameHelper.datasave.ReadString("BehaviourTreePreviewWindow.BehaviourtTree");
                if (!string.IsNullOrEmpty(_assetBehaviourTreeGUID))
                {
                    _assetBehaviourTreePath = AssetDatabase.GUIDToAssetPath(_assetBehaviourTreeGUID);
                    UpdateTree();
                }
            }

            this._isShowMiniUI = shaco.GameHelper.datasave.ReadBool("BehaviourTreePreviewWindow._isShowMiniUI", this._isShowMiniUI);
        }

        private void UpdateTree()
        {
            if (string.IsNullOrEmpty(_assetBehaviourTreePath))
                return;

            var fullPath = EditorHelper.GetFullPath(_assetBehaviourTreePath);
            var loadJson = shaco.Base.FileHelper.ReadAllByUserPath(fullPath);
            shaco.Base.BehaviourTreeConfig.LoadFromJson(loadJson, _rootTree);
        }

        private string GetLoadJsonPath()
        {
            if (string.IsNullOrEmpty(_assetBehaviourTreePath))
            {
                return string.Empty;
            }
            else
            {
                return EditorHelper.GetFullPath(_assetBehaviourTreePath);
            }
        }

        private void DrawRootTree(shaco.Base.BehaviourRootTree tree)
        {
            if (GUILayout.Button("New"))
            {
                ForceSelectTrees(new shaco.Base.BehaviourTree[] { _rootTree });
                ShowContextMenu(true);
            }

            float widthTmp = _defaultGUIWidth / 2 - 3;
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Save", GUILayout.Width(widthTmp)))
                {
                    if (string.IsNullOrEmpty(_assetBehaviourTreePath))
                    {
                        SaveAs(GetLoadJsonPath());
                    }
                    else
                    {
                        _rootTree.SaveToJson(GetLoadJsonPath());
                        if (EditorHelper.IsUnityAssetPath(_assetBehaviourTreePath))
                        {
                            var assetPath = EditorHelper.FullPathToUnityAssetPath(_assetBehaviourTreePath);
                            AssetDatabase.Refresh();
                        }
                    }
                    SaveSetings();
                }
                if (GUILayout.Button("Save As", GUILayout.Width(widthTmp)))
                {
                    SaveAs(GetLoadJsonPath());
                    SaveSetings();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                if (!string.IsNullOrEmpty(_assetBehaviourTreePath) && GUILayout.Button("Revert", GUILayout.Width(widthTmp)))
                {
                    _rootTree.LoadFromJsonPath(GetLoadJsonPath());
                    UpdateClassNameInScripts();
                }

                if (null != _rootTree && GUILayout.Button("Clear", GUILayout.Width(widthTmp)))
                {
                    _rootTree.RemoveChildren();
                    _assetBehaviourTreePath = string.Empty;
                    _assetBehaviourTreeGUID = string.Empty;
                }
            }
            GUILayout.EndHorizontal();

            // if (GUILayout.Button("CheckValid"))
            // {
            //     CheckAllTreeParamValid();
            // }
            FixFormatTreeSizeWithGUILayout(tree, _defaultGUIWidth);
        }

        private void SaveAs(string defaultPath)
        {
            var folderTmp = string.IsNullOrEmpty(defaultPath) ? Application.dataPath : shaco.Base.FileHelper.GetFolderNameByPath(defaultPath);
            var filenameTmp = string.IsNullOrEmpty(defaultPath) ? "BehaviourConfig" : shaco.Base.FileHelper.GetLastFileName(defaultPath);
            var pathTmp = EditorUtility.SaveFilePanel("Save Config", folderTmp, filenameTmp, "json");
            if (!string.IsNullOrEmpty(pathTmp))
            {
                _rootTree.SaveToJson(pathTmp);
                AssetDatabase.Refresh();
            }
        }

        private void UpdateClassNameInScripts()
        {
            if (null == _rootTree)
                return;

            _isUpdatingAllClassNameInScript = true;
            _rootTree.ForeachAllChildren((child, index, level) =>
            {
                if (string.IsNullOrEmpty(child.processTypeName))
                {
                    child.editorHasInValidParam = true;
                }
                else
                {
                    CheckScriptAssetChanged(child);
                }
                return true;
            });
            _isUpdatingAllClassNameInScript = false;
        }

        private void DrawTreeType(shaco.Base.BehaviourTree tree, int fontSize = 15)
        {
            var oldSize = GUI.skin.label.fontSize;
            GUI.skin.label.fontSize = _isShowMiniUI ? (int)(20 * _miniUIScale) : fontSize;
            if (tree.ShowDefaultDisplayName())
            {
                tree.displayName = GUILayout.TextArea(tree.displayName, GUI.skin.label);
            }
            GUI.skin.label.fontSize = oldSize;
        }

        private void GetITreeProcessClassNames(shaco.Base.BehaviourTree tree, out List<string> enabledClasses, out List<string> disabledClasses)
        {
            enabledClasses = new List<string>();
            disabledClasses = new List<string>();

            var textScript = tree.editorAssetProcess.text;
            var parentFullName = tree.parent.fullName;
            // var rootTreeTmp = tree.root as shaco.Base.BehaviourRootTree;

            var classNames = shaco.Base.Utility.GetFullClassNames(textScript, typeof(shaco.Base.IBehaviourProcess));
            if (null != classNames)
            {
                for (int i = 0; i < classNames.Length; ++i)
                {
                    var classNameTmp = classNames[i];
                    if (!string.IsNullOrEmpty(classNameTmp))
                    {
                        // if (rootTreeTmp.HasTree(parentFullName + "." + classNameTmp))
                        // {
                        //     disabledClasses.Add(classNameTmp);
                        // }
                        // else
                        // {
                        enabledClasses.Add(classNameTmp);
                        // }
                    }
                }
            }
        }

        private void CheckScriptAssetChanged(shaco.Base.BehaviourTree tree)
        {
            if (null == tree.editorAssetProcess)
                return;

            List<string> enabledClasses = null;
            List<string> disabledClasses = null;
            GetITreeProcessClassNames(tree, out enabledClasses, out disabledClasses);
            if (enabledClasses.Count == 0 && disabledClasses.Count == 0)
            {
                tree.editorAssetProcess = null;
                tree.processTypeName = string.Empty;
                Debug.LogError("BehaviourTreePreviewWindow CheckScriptAssetChanged error: please check class Inherit from 'shaco.Base.IBehaviourProcess', path=" + AssetDatabase.GetAssetPath(tree.editorAssetProcess));
            }
            else if (enabledClasses.Count == 1)
                tree.processTypeName = enabledClasses[0];
            else
            {
                //当是刷新所有节点class名字时候，仅仅支持包含1个class的脚本
                if (_isUpdatingAllClassNameInScript)
                {
                    //检查原来脚本是否丢失
                    if (null == enabledClasses.Find(v => v == tree.processTypeName))
                        Debug.LogError(string.Format("BehaviourTreePreviewWindow CheckScriptAssetChanged error: missing class '{0}', please manually reassign it, path={1}", tree.processTypeName, AssetDatabase.GetAssetPath(tree.editorAssetProcess)));
                    return;
                }
                ShowSelectMenu((string selectName) =>
                {
                    tree.processTypeName = selectName;
                    if (null != _currentSelectScript)
                    {
                        tree.editorAssetProcess = _currentSelectScript;
                        tree.editorAssetPathProcess = AssetDatabase.GetAssetPath(tree.editorAssetProcess);

                        if (tree.editorAssetProcess != null && !string.IsNullOrEmpty(tree.processTypeName))
                        {
                            tree.editorHasInValidParam = false;
                        }
                    }
                }, enabledClasses.ToArray(), disabledClasses.ToArray());

                _currentSelectScript = tree.editorAssetProcess;
                tree.editorAssetProcess = null;
                tree.processTypeName = string.Empty;
                tree.editorHasInValidParam = true;
            }
        }
    }
}