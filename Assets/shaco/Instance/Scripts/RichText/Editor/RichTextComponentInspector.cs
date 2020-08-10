#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shaco.Instance.RichText
{

    [CustomEditor(typeof(shaco.Instance.RichText.RichTextComponent))]
    public class RichTextComponentInspector : UnityEditor.Editor
    {
        private Object _dynamicCharacterFolderAsset = null;
        private Object _assetNew = null;
        private string _inputKey = string.Empty;
        private string _inputFullKey = string.Empty;
        private shaco.Instance.RichText.RichTextComponent.TextType _inputType = shaco.Instance.RichText.RichTextComponent.TextType.Text;
        private bool _isInputFolder = false;
        private List<string> _listWillRemoveValidAsset = new List<string>();
        private bool _isAutoUseFullName = false;
        private shaco.Instance.RichText.RichTextComponent.CharacterFolderInfo _currentCharacterFolderInfo = null;
        private shaco.Instance.RichText.RichTextComponent _target;

        void OnEnable()
        {
            _target = target as shaco.Instance.RichText.RichTextComponent;

            if (!string.IsNullOrEmpty(_target.dynamicCharacterFolder))
                _dynamicCharacterFolderAsset = AssetDatabase.LoadAssetAtPath<Object>(_target.dynamicCharacterFolder);

            Undo.undoRedoPerformed += UndoRedoPerformedCallBack;
        }

        void OnDestroy()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformedCallBack;
        }

        private void UndoRedoPerformedCallBack()
        {
            //在undo后需要强制刷新查找引用，否则因为字典无法序列化，不能自动undo导致Characters出现bug
            _target.ForceUpdateListDataToDictionaryData();
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            // base.OnInspectorGUI();

            GUI.changed = false;
            DrawText();
            DrawMaxCachedCharacterCount();
            DrawTextModel();
            // DrawWarningHelpBox();
            DrawAnchorGroup();
            DrawNewGroup();
            DrawCharactersGroup();
            if (GUI.changed)
            {
                SetDirty(_target);
                _target.ForceUpdateLayout();
            }
        }

        private void DrawDynamicCharacterFolder()
        {
            var newFolderAsset = EditorGUILayout.ObjectField("Dynamic Character Folder", _dynamicCharacterFolderAsset, typeof(DefaultAsset), false);
            if (newFolderAsset != _dynamicCharacterFolderAsset)
            {
                var assetPath = AssetDatabase.GetAssetPath(newFolderAsset);
                if (AssetDatabase.IsValidFolder(assetPath))
                {
                    var oldFolderPath = _target.dynamicCharacterFolder;
                    _target.dynamicCharacterFolder = assetPath;

                    if (oldFolderPath != _target.dynamicCharacterFolder)
                        _dynamicCharacterFolderAsset = newFolderAsset;
                }
                else
                {
                    Debug.LogError("RichTextComponentInspector DrawDynamicCharacterFoldr error: not a folder, path=" + assetPath);
                }
            }
        }

        private void DrawTextModel()
        {
            //text model
            var newTextModel = (UnityEngine.UI.Text)EditorGUILayout.ObjectField("Text Model", _target.textModel, typeof(UnityEngine.UI.Text), true);
            if (newTextModel != _target.textModel)
            {
                _target.textModel = newTextModel;
                if (null != _target.textModel && _target.textModel.resizeTextForBestFit)
                {
                    _target.textModel.resizeTextMinSize = _target.textModel.fontSize;
                    _target.textModel.resizeTextMaxSize = _target.textModel.fontSize;
                }
            }
        }

        private void DrawAnchorGroup()
        {
            //horizontalOverflow
            _target.horizontalOverflow = (HorizontalWrapMode)EditorGUILayout.EnumPopup("Horizontal Overflow", _target.horizontalOverflow);

            //draw anchor
            var newTextAnchor = shaco.Instance.RichText.GUILayoutHelper.DrawAnchor("Text Anchor", _target.textAnchor);
            if (newTextAnchor != _target.textAnchor)
            {
                _target.textAnchor = newTextAnchor;
                _target.contentAnchor = _target.textAnchor;
                shaco.Instance.RichText.RichTextComponent.SetPivotByLocalPosition(_target.gameObject, shaco.Instance.RichText.RichTextComponent.ToPivot(_target.contentAnchor));
                GUI.changed = true;
            }

            //draw margin
            if (_target.GetDisplayCharacterCount() > 1)
                _target.margin = EditorGUILayout.Vector2Field("Margin", _target.margin);
        }

        public void DrawText()
        {
            EditorGUILayout.PrefixLabel("Text");
            _target.text = EditorGUILayout.TextArea(_target.text, GUILayout.MinHeight(48));
        }

        private void DrawMaxCachedCharacterCount()
        {
            _target.maxCachedCharacterCount = EditorGUILayout.IntField("Max Cached Character Count", _target.maxCachedCharacterCount);
        }

        public void DrawNewGroup()
        {
            GUILayout.BeginVertical("box");
            {
                DrawNewAPI(_target);
            }
            GUILayout.EndVertical();
        }

        public void DrawCharactersGroup()
        {
            GUILayout.BeginVertical("box");
            {
                bool isShowCharacters = true;
                isShowCharacters = RichTextComponentInspector.DrawHeader("Characters", "RichTextEditorCharacter", true, null);
                if (isShowCharacters)
                {
                    _target.ForeachCharacters((shaco.Instance.RichText.RichTextComponent.CharacterInfo character) =>
                    {
                        GUILayout.BeginHorizontal();
                        {
                            if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                            {
                                _listWillRemoveValidAsset.Add(character.key);
                            }
                            
                            EditorGUI.BeginDisabledGroup(true);
                            {
                                EditorGUILayout.EnumPopup(character.type, GUILayout.Width(50));
                            }
                            EditorGUI.EndDisabledGroup();

                            var newText = EditorGUILayout.TextField(character.key, GUILayout.Width(Screen.width / 2));
                            if (newText != character.key)
                            {
                                GUI.FocusControl(string.Empty);
                            }

                            EditorGUI.BeginDisabledGroup(true);
                            {
                                switch (character.type)
                                {
                                    case shaco.Instance.RichText.RichTextComponent.TextType.Image:
                                        {
                                            var spriteTmp = character.asset as Sprite;
                                            if (null == spriteTmp) _listWillRemoveValidAsset.Add(character.key);
                                            EditorGUILayout.ObjectField(spriteTmp, typeof(Sprite), true);
                                            break;
                                        }
                                    case shaco.Instance.RichText.RichTextComponent.TextType.Prefab:
                                        {
                                            var prefabTmp = character.asset as GameObject;
                                            if (null == prefabTmp) _listWillRemoveValidAsset.Add(character.key);
                                            EditorGUILayout.ObjectField(prefabTmp, typeof(GameObject), true);
                                            break;
                                        }
                                    default: Debug.LogError("RichTextComponentInspector unsupport type=" + character.type); break;
                                }
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        GUILayout.EndHorizontal();
                        return true;
                    });

                    if (_listWillRemoveValidAsset.Count > 0)
                    {
                        for (int i = _listWillRemoveValidAsset.Count - 1; i >= 0; --i)
                            _target.RemoveCharacter(_listWillRemoveValidAsset[i]);
                        _listWillRemoveValidAsset.Clear();
                    }
                }

                DrawDynamicCharacterFolder();
            }
            GUILayout.EndVertical();
        }

        // public void DrawWarningHelpBox()
        // {
        //     if (!_canUseAutoWrap)
        //         return;

        //     bool haveAutoWrapWarning = _target.horizontalOverflow && null != _target.textModel && _target.textModel.horizontalOverflow != HorizontalWrapMode.Wrap;
        //     if (haveAutoWrapWarning)
        //     {
        //         GUILayout.BeginHorizontal();
        //         {
        //             EditorGUILayout.HelpBox("Auto Wrap works only if the template text is Wrap mode", MessageType.Warning);

        //             if (GUILayout.Button("Fix it", GUILayout.ExpandWidth(false), GUILayout.Height(38)))
        //             {
        //                 _target.textModel.horizontalOverflow = HorizontalWrapMode.Wrap;
        //                 _target.OnTextModelUpdate();
        //                 GUI.changed = true;
        //             }
        //         }
        //         GUILayout.EndHorizontal();
        //     }
        // }

        static public void SetDirty(Object target)
        {
            if (target != null)
                EditorUtility.SetDirty(target);

#if UNITY_2018_1_OR_NEWER
            GameObject gameObjectTarget = null;
            if (target is Component)
                gameObjectTarget = ((Component)target).gameObject;
            else if (target is GameObject)
                gameObjectTarget = target as GameObject;
            if (null != gameObjectTarget)
            {
                var prefabStage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObjectTarget);
                if (null != prefabStage)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(prefabStage.scene);
                }
                else
                {
                    if (!Application.isPlaying)
                        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                }
            }
#elif UNITY_5_3_OR_NEWER
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
        }

        private void DrawNewAPI(shaco.Instance.RichText.RichTextComponent target)
        {
            if (RichTextComponentInspector.DrawHeader("New Character", "RichTextEditorNew", true, null))
            {
                if (_assetNew == null)
                {
                    var newSprite = EditorGUILayout.ObjectField("Sprite", _assetNew, typeof(Sprite), false, GUILayout.Height(16)) as Sprite;
                    if (null != newSprite && newSprite != _assetNew)
                    {
                        _assetNew = newSprite;
                        _inputType = shaco.Instance.RichText.RichTextComponent.TextType.Image;
                        _isInputFolder = false;
                        AutoSetInputKey(_assetNew);
                        CheckInspectionWindowReFocuseWhenObjectSelectionWindowActive();
                    }

                    var newPrefab = EditorGUILayout.ObjectField("Prefab", _assetNew, typeof(GameObject), false) as GameObject;
                    if (null != newPrefab && newPrefab != _assetNew)
                    {
                        _assetNew = newPrefab;
                        _inputType = shaco.Instance.RichText.RichTextComponent.TextType.Prefab;
                        _isInputFolder = false;
                        AutoSetInputKey(_assetNew);
                        CheckInspectionWindowReFocuseWhenObjectSelectionWindowActive();
                    }

                    var newFolder = EditorGUILayout.ObjectField("Folder", _assetNew, typeof(DefaultAsset), false) as DefaultAsset;
                    if (null != newFolder && newFolder != _assetNew)
                    {
                        _assetNew = newFolder;
                        _isInputFolder = true;
                        CheckInspectionWindowReFocuseWhenObjectSelectionWindowActive();
                    }
                }
                else
                {
                    if (_isInputFolder)
                        GUILayout.Label("Comfirn add all character from folder");
                    else
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.TextField("New Key", _inputKey);
                        }
                        EditorGUI.EndDisabledGroup();
                    }

                    var newIsAutoUseFullName = GUILayout.Toggle(_isAutoUseFullName, "UseFullName");
                    if (newIsAutoUseFullName != _isAutoUseFullName && !_isInputFolder)
                    {
                        //根据选择类型，同时刷新显示的key
                        _inputKey = _isAutoUseFullName ? _inputFullKey : System.IO.Path.GetFileName(_inputFullKey);
                    }
                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Yes"))
                        {
                            if (_isInputFolder)
                            {
                                AddCharacterWithFolder(target, _assetNew);
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(_inputKey))
                                {
                                    AddCharacter(target, _inputKey, _assetNew, _inputType);
                                }
                            }
                            ResetInput();
                        }
                        if (GUILayout.Button("No"))
                        {
                            ResetInput();
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("Clear"))
                {
                    _dynamicCharacterFolderAsset = null;
                    _target.ClearCharacters();
                }
            }
        }

        private void AutoSetInputKey(Object asset)
        {
            if (null == asset) return;

            var path = AssetDatabase.GetAssetPath(asset);
            _inputFullKey = path;
            _inputKey = _isAutoUseFullName ? _inputFullKey : System.IO.Path.GetFileName(_inputFullKey);
        }

        private void CheckInspectionWindowReFocuseWhenObjectSelectionWindowActive()
        {
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var typeObjectSelectorWindow = assembly.GetType("UnityEditor.ObjectSelector");
            if (EditorWindow.focusedWindow.GetType() == typeObjectSelectorWindow)
            {
                var typeInspectionWindow = assembly.GetType("UnityEditor.InspectorWindow");
                EditorWindow.FocusWindowIfItsOpen(typeInspectionWindow);
            }
        }

        private void ResetInput()
        {
            _inputKey = string.Empty;
            _inputFullKey = string.Empty;
            _assetNew = null;
            _isInputFolder = false;
        }

        private string RemoveString(string path, string find)
        {
            int indexFind = path.IndexOf(find);
            if (indexFind >= 0)
                path = path.Remove(indexFind, find.Length);
            return path;
        }

        private void AddCharacter(shaco.Instance.RichText.RichTextComponent target, string key, Object asset, shaco.Instance.RichText.RichTextComponent.TextType type)
        {
            if (string.IsNullOrEmpty(key) || asset == null)
                return;
 
            if (!_isAutoUseFullName)
                key = System.IO.Path.GetFileName(key);

            if (_target.HasCharacter(key))
                return;

            var newCharacter = new shaco.Instance.RichText.RichTextComponent.CharacterInfo();

            newCharacter.key = key;
            newCharacter.type = type;
            newCharacter.asset = asset;
            newCharacter.value = AssetDatabase.GetAssetPath(asset);
            _target.AddCharacter(newCharacter);
        }

        private void AddCharacterWithFolder(shaco.Instance.RichText.RichTextComponent target, Object asset)
        {
            if (null == asset)
            {
                Debug.Log("RichTextEditor AddCharacterWithFolder error: asset is null");
                return;
            }

            var assetPathFolder = AssetDatabase.GetAssetPath(asset);
            if (!System.IO.Directory.Exists(assetPathFolder))
                Debug.LogError("RichTextComponentInspector AddCharacterWithFolder error: not found path=" + assetPathFolder);
            else
            {
                AddCharacterFolderSafe(target, asset);

                var allFiles = AssetDatabase.FindAssets("t:texture2d t:prefab", new string[] { assetPathFolder });
                if (null == allFiles)
                    return;

                var projectPath = RemoveString(Application.dataPath, "Assets");
                for (int i = 0; i < allFiles.Length; ++i)
                {
                    var pathTmp = AssetDatabase.GUIDToAssetPath(allFiles[i]);
                    var assetPath = RemoveString(pathTmp, projectPath);

                    if (pathTmp.EndsWith(".prefab"))
                        AddCharacter(target, assetPath, AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject, shaco.Instance.RichText.RichTextComponent.TextType.Prefab);
                    else
                        AddCharacter(target, assetPath, AssetDatabase.LoadAssetAtPath(assetPath, typeof(Sprite)) as Sprite, shaco.Instance.RichText.RichTextComponent.TextType.Image);
                }
            }
        }

        private void AddCharacterFolderSafe(shaco.Instance.RichText.RichTextComponent target, Object asset)
        {
            bool findFolder = false;
            var pathAsset = AssetDatabase.GetAssetPath(asset);

            for (int i = _target.characterFolderPaths.Count - 1; i >= 0; --i)
            {
                if (_target.characterFolderPaths[i].path == pathAsset)
                {
                    _currentCharacterFolderInfo = _target.characterFolderPaths[i];
                    findFolder = true;
                    break;
                }
            }

            if (!findFolder)
            {
                _currentCharacterFolderInfo = new shaco.Instance.RichText.RichTextComponent.CharacterFolderInfo();
                _currentCharacterFolderInfo.path = pathAsset;
                _currentCharacterFolderInfo.isAutoUseFullName = _isAutoUseFullName;
                _target.characterFolderPaths.Add(_currentCharacterFolderInfo);
            }
        }

        static private bool DrawHeader(string text, string key, bool defaultShow, string backgroundStyle, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            if (string.IsNullOrEmpty(backgroundStyle))
                backgroundStyle = "TE NodeBackground";

            bool state = EditorPrefs.GetBool(key, defaultShow);
            // var oldColor = GUI.backgroundColor;

            // if (!state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            // text = "<b><size=11>" + text + "</size></b>";

            bool isEmptyText = string.IsNullOrEmpty(text);
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;

            GUILayout.BeginHorizontal(backgroundStyle, options);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(4);

                    if (isEmptyText)
                    {
                        options = new GUILayoutOption[] { GUILayout.Width(50) };
                    }

                    var oldTextColor = EditorStyles.label.normal.textColor;
                    EditorStyles.label.normal.textColor = state ? Color.white : Color.grey;
                    {
                        if (GUILayout.Button(text, EditorStyles.label, options))
                        {
                            state = !state;
                            EditorPrefs.SetBool(key, state);
                            GUI.FocusControl(string.Empty);
                        }
                    }
                    EditorStyles.label.normal.textColor = oldTextColor;
                }
                GUILayout.EndVertical();
                if (null != onHeaderDrawCallBack)
                {
                    onHeaderDrawCallBack();
                }
            }
            GUILayout.EndHorizontal();
            return state;
        }
    }
}

#endif