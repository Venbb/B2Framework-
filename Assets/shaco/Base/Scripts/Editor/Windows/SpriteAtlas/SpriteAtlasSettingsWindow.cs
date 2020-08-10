#if UNITY_2017_1_OR_NEWER

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace shacoEditor
{
    /// <summary>
    /// 图集设置窗口
    /// </summary>
    public class SpriteAtlasSettingsWindow : EditorWindow
    {
        //默认全局图集配置
        [SerializeField]
        private ISpriteAtlasSettings _defaultSettings = new SpriteAtlasSettingsDefault();

        //运行时图集配置
        [SerializeField]
        private shaco.ISpriteAtlasSettings _runtimeAtlasSettings = null;

        //是否自动删除空文件夹
        [SerializeField]
        private bool _isAutoDeleteEmptyFolder = true;

        //特殊文件夹标记名字
        //切记：文件夹名字不可以包含以下特殊名字 
        private readonly string NO_ATLAS_FOLDER = "__NoAtlas"; //非图集文件夹
        private readonly string IN_ATLAS_FOLDER = "__InAtlas"; //普通图集文件夹
        private readonly string IN_SUB_SHARED_ATLAS_FOLDER = "__SharedAtlas"; //共享图集文件夹
        private readonly string GLOBAL_ATLAS_FOLDER = "__GlobalAtlas"; //全局共享图集文件夹

        private readonly GUIContent GUI_CONTENT_AUTO_GLOABAL_ATLAS = new GUIContent("Auto Global Atlas", "Automatically calculate and generate global or subfolder shared atlas, which will collect references of all objects in the target directory, it will take a long time");
        private readonly GUIContent GUI_CONTENT_AUTO_PACK_PREVIEW = new GUIContent("Auto Pack Preview", "Automatically package after the optimization of atlas");
        private readonly GUIContent GUI_CONTENT_RUNTIME_SETTINGS = new GUIContent("Runtime Settings", "The runtime configuration will automatically synchronize when the optimize button is clicked");

        //图片资源根目录
        [SerializeField]
        private string _textureRootPath = string.Empty;

        //运行时图集配置路径
        [SerializeField]
        private string _runtimeAtlasSettingsPath = string.Empty;

        //是否自动计算全局和子文件夹共享图集
        //这会计算图片引用关系比较消耗时间
        [SerializeField]
        private bool _isAutoGlobalAndSubSharedAtlas = true;

        [SerializeField]
        private bool _isAutoPackPreviewAfterOptimize = true;

        //是否支持自动化配置运行时图集配置
        [SerializeField]
        private bool _isSupportRuntimeAtlasSettings = false;

        [MenuItem("shaco/Tools/SpriteAtlasSettings _F11", false, (int)(int)ToolsGlobalDefine.MenuPriority.Tools.SPRITE_ATLAS_SETTINGS)]
        static public SpriteAtlasSettingsWindow OpenSpriteAtlasSettingsWindow()
        {
            var retValue = EditorHelper.GetWindow<SpriteAtlasSettingsWindow>(null, true, "SpriteAtlasSettingsWindow");
            retValue.Init();
            return retValue;
        }

        private void OnEnable()
        {
            var retValue = EditorHelper.GetWindow<SpriteAtlasSettingsWindow>(this, true, "SpriteAtlasSettingsWindow");
            retValue.Init();
        }

        private void Init()
        {
            LoadSettings();
            UpdateRuntimeAtlasSettings();
        }

        private bool IsSupportRuntimeAtlasSettings()
        {
            var altasSettingsType = shaco.GameHelper.atlasSettings.GetType();
            return altasSettingsType.IsInherited(typeof(ScriptableObject));
        }

        private void UpdateRuntimeAtlasSettings()
        {
            if (!IsSupportRuntimeAtlasSettings())
            {
                _isSupportRuntimeAtlasSettings = false;
                return;
            }

            if (!string.IsNullOrEmpty(_runtimeAtlasSettingsPath))
            {
                _runtimeAtlasSettings = (shaco.ISpriteAtlasSettings)AssetDatabase.LoadAssetAtPath(_runtimeAtlasSettingsPath, shaco.GameHelper.atlasSettings.GetType());
            }
            else
            {
                _runtimeAtlasSettings = null;
            }
            _isSupportRuntimeAtlasSettings = true;
        }

        private void SetRuntimeAtlasSettingsDirty()
        {
            if (null != _runtimeAtlasSettings)
            {
                if (!IsSupportRuntimeAtlasSettings())
                {
                    return;
                }
                EditorHelper.SetDirty((UnityEngine.Object)_runtimeAtlasSettings);
            }
        }

        private void AddRuntimeAtlasSettingsInfo(string atlasName, string atlasFolder)
        {
            if (null != _runtimeAtlasSettings)
            {
                if (!_runtimeAtlasSettings.ContainsAtlasPath(atlasFolder))
                {
                    var atlasNameWithoutExtension = shaco.Base.FileHelper.RemoveLastExtension(atlasName);
                    _runtimeAtlasSettings.AddAtlasInfo(new shaco.SpriteAtlasSettingsInfo()
                    {
                        atlasName = atlasNameWithoutExtension,
                        atlasFolder = atlasFolder
                    });
                }
            }
        }

        private void OnDestroy()
        {
            SaveSettings();
        }

        private void SaveSettings()
        {
            shaco.GameHelper.datasave.WriteString("SpriteAtlasSettingsWindow._textureRootPath", _textureRootPath);
            shaco.GameHelper.datasave.WriteBool("SpriteAtlasSettingsWindow._isAutoDeleteEmptyFolder", _isAutoDeleteEmptyFolder);
            shaco.GameHelper.gameConfig.WriteString(shaco.SpriteAtlasSettings.SETTINGS_PATH_KEY, _runtimeAtlasSettingsPath);
            shaco.GameHelper.datasave.WriteBool("SpriteAtlasSettingsWindow._isAutoGlobalAndSubSharedAtlas", _isAutoGlobalAndSubSharedAtlas);
            shaco.GameHelper.datasave.WriteBool("SpriteAtlasSettingsWindow._isAutoPackPreviewAfaterOptimize", _isAutoPackPreviewAfterOptimize);
            _defaultSettings.SaveSettings(shaco.GameHelper.datasave);
        }

        private void LoadSettings()
        {
            _textureRootPath = shaco.GameHelper.datasave.ReadString("SpriteAtlasSettingsWindow._textureRootPath", EditorHelper.FullPathToUnityAssetPath(Application.dataPath));
            _isAutoDeleteEmptyFolder = shaco.GameHelper.datasave.ReadBool("SpriteAtlasSettingsWindow._isAutoDeleteEmptyFolder", _isAutoDeleteEmptyFolder);
            _runtimeAtlasSettingsPath = shaco.GameHelper.gameConfig.ReadString(shaco.SpriteAtlasSettings.SETTINGS_PATH_KEY, _runtimeAtlasSettingsPath);
            _isAutoGlobalAndSubSharedAtlas = shaco.GameHelper.datasave.ReadBool("SpriteAtlasSettingsWindow._isAutoGlobalAndSubSharedAtlas", _isAutoGlobalAndSubSharedAtlas);
            _isAutoPackPreviewAfterOptimize = shaco.GameHelper.datasave.ReadBool("SpriteAtlasSettingsWindow._isAutoPackPreviewAfaterOptimize", _isAutoPackPreviewAfterOptimize);
            _defaultSettings.LoadSettings(shaco.GameHelper.datasave);
        }

        private void OnGUI()
        {
            EditorHelper.RecordObjectWindow(this);

            _textureRootPath = GUILayoutHelper.PathField("Texture Root Path", _textureRootPath, string.Empty);
            DrawAutoGlobalAndSubSharedAtlas();
            DrawAutoPackPreview();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Auto Delete Empty Folder", GUILayout.ExpandWidth(false));
                GUILayout.Space(7);
                _isAutoDeleteEmptyFolder = GUILayout.Toggle(_isAutoDeleteEmptyFolder, string.Empty);
            }
            GUILayout.EndHorizontal();

            _defaultSettings = GUILayoutHelper.PopupTypeField("Editor Settings Delegate", _defaultSettings);
            _defaultSettings.OnGUI();

            DrawRuntimeAltasSettings();
            if (!string.IsNullOrEmpty(_textureRootPath) && GUILayout.Button("Optimize", GUILayout.Height(40)))
            {
                OptimizeStart();
            }
        }

        private void DrawRuntimeAltasSettings()
        {
            if (_isSupportRuntimeAtlasSettings)
            {
                if (null == _runtimeAtlasSettings || string.IsNullOrEmpty(_runtimeAtlasSettingsPath))
                {
                    if (GUILayout.Button("Create Runtime Settings"))
                    {
                        var saveFilePath = EditorUtility.SaveFilePanel("Create Sprite Atals Settings", _textureRootPath, typeof(shaco.SpriteAtlasSettings).Name, "asset");
                        if (!string.IsNullOrEmpty(saveFilePath))
                        {
                            if (!saveFilePath.Contains(Application.dataPath))
                            {
                                Debug.LogError("SpriteAtlasSettingsWindow error: runtime altas setting path must in Unity Project\n" + Application.dataPath);
                            }
                            else if (!saveFilePath.Contains("/Resources/"))
                            {
                                Debug.LogError("SpriteAtlasSettingsWindow error: runtime altas setting path must in 'Resources' folder");
                            }
                            else
                            {
                                _runtimeAtlasSettings = (shaco.ISpriteAtlasSettings)ScriptableObject.CreateInstance(shaco.GameHelper.atlasSettings.GetType());
                                _runtimeAtlasSettingsPath = EditorHelper.FullPathToUnityAssetPath(saveFilePath);
                                AssetDatabase.CreateAsset((UnityEngine.Object)_runtimeAtlasSettings, EditorHelper.FullPathToUnityAssetPath(_runtimeAtlasSettingsPath));

                                SaveSettings();
                            }
                        }
                    }
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUI.changed = false;
                        var newSettingPath = GUILayoutHelper.PathField(GUI_CONTENT_RUNTIME_SETTINGS, _runtimeAtlasSettingsPath, "asset");
                        if (GUI.changed)
                        {
                            if (string.IsNullOrEmpty(newSettingPath))
                            {
                                _runtimeAtlasSettingsPath = newSettingPath;
                                UpdateRuntimeAtlasSettings();
                            }
                            else if (!newSettingPath.Contains(Application.dataPath))
                            {
                                Debug.LogError("SpriteAtlasSettingsWindow error: runtime altas setting path must in Unity Project\n" + Application.dataPath);
                            }
                            else if (!newSettingPath.Contains("/Resources/"))
                            {
                                Debug.LogError("SpriteAtlasSettingsWindow error: runtime altas setting path must in 'Resources' folder");
                            }
                            else
                            {
                                _runtimeAtlasSettingsPath = newSettingPath;
                                UpdateRuntimeAtlasSettings();
                            }
                        }

                        if (GUILayout.Button("Delete", GUILayout.Width(55f)))
                        {
                            if (shaco.Base.FileHelper.ExistsFile(EditorHelper.GetFullPath(_runtimeAtlasSettingsPath)))
                            {
                                AssetDatabase.DeleteAsset(_runtimeAtlasSettingsPath);
                                _runtimeAtlasSettingsPath = string.Empty;
                                _runtimeAtlasSettings = null;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                var fullTypeName = shaco.GameHelper.atlasSettings.GetType().FullName;
                EditorGUILayout.HelpBox(string.Format("'{0}' Runtime atlas configuration can only be automated if it inherits from type 'UnityEngine.ScriptableObject'", fullTypeName), MessageType.Warning);
            }
        }

        private void DrawAutoGlobalAndSubSharedAtlas()
        {
            _isAutoGlobalAndSubSharedAtlas = EditorGUILayout.Toggle(GUI_CONTENT_AUTO_GLOABAL_ATLAS, _isAutoGlobalAndSubSharedAtlas);
        }

        private void DrawAutoPackPreview()
        {
            var shouldEnabledSpriteAtlasPack = EditorSettings.spritePackerMode != SpritePackerMode.AlwaysOnAtlas;

            GUILayout.BeginHorizontal();
            {
                if (shouldEnabledSpriteAtlasPack)
                    _isAutoPackPreviewAfterOptimize = false;

                EditorGUI.BeginDisabledGroup(shouldEnabledSpriteAtlasPack);
                {
                    _isAutoPackPreviewAfterOptimize = EditorGUILayout.Toggle(GUI_CONTENT_AUTO_PACK_PREVIEW, _isAutoPackPreviewAfterOptimize, GUILayout.ExpandWidth(false));
                }
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.PrefixLabel("Sprite Pack Mode");
                if (shouldEnabledSpriteAtlasPack)
                {
                    if (GUILayout.Button(SpritePackerMode.AlwaysOnAtlas.ToString(), GUILayout.ExpandWidth(false)))
                    {
                        EditorSettings.spritePackerMode = SpritePackerMode.AlwaysOnAtlas;
                        _isAutoPackPreviewAfterOptimize = true;
                    }
                }
                else
                {
                    if (GUILayout.Button(SpritePackerMode.Disabled.ToString(), GUILayout.ExpandWidth(false)))
                    {
                        EditorSettings.spritePackerMode = SpritePackerMode.Disabled;
                    }

                    if (GUILayout.Button("Pack All Atlases", GUILayout.ExpandWidth(false)))
                    {
                        UnityEditor.U2D.SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);
                    }
                }
            }
            GUILayout.EndHorizontal();

            if (shouldEnabledSpriteAtlasPack)
            {
                EditorGUILayout.HelpBox("Sprite Atlas packing is disabled", MessageType.Info);
            }
        }

        //开始优化图集文件夹
        private void OptimizeStart()
        {
            //避免选择根目录了
            if (_textureRootPath == "Assets" || _textureRootPath == Application.dataPath)
            {
                Debug.LogError("SpriteAtlasSettingsWindow OptimizeStart error: can't use Application.dataPath=" + Application.dataPath);
                return;
            }

            if (!System.IO.Directory.Exists(_textureRootPath))
            {
                Debug.LogError("SpriteAtlasSettingsWindow OptimizeStart error: not found texture root=" + _textureRootPath);
                return;
            }

            //获取文件夹下所有图片
            var allSpritesGUID = AssetDatabase.FindAssets("t:sprite", new string[] { _textureRootPath });
            if (allSpritesGUID.IsNullOrEmpty())
            {
                Debug.LogError("SpriteAtlasSettingsWindow Optimize error: not found texture in path=" + _textureRootPath);
                return;
            }

            var allSpritesPath = allSpritesGUID.Convert(v => AssetDatabase.GUIDToAssetPath(v));
            if (_isAutoGlobalAndSubSharedAtlas)
            {
                //如果公共图集目录不存在则创建一个
                var globalAtlasPath = this.GetGlobalAtlasPath();
                if (string.IsNullOrEmpty(globalAtlasPath))
                {
                    Debug.LogError("SpriteAtlasSettingsWindow Optimize error: global atlas path is valid");
                    return;
                }

                //获取引用关系表
                shaco.Instance.FindReference.FindReferenceWindow.FindReferencesInProject((depencies) =>
                {
                    OptimizeAltas(allSpritesPath, depencies);
                }, true, allSpritesPath);
            }
            else
            {
                OptimizeAltas(allSpritesPath, null);
            }
        }

        private void OptimizeAltas(string[] allTexturesPath, Dictionary<string, shaco.Instance.FindReference.FindReferenceWindow.DependentInfo> textureDepencies)
        {
            if (null != _runtimeAtlasSettings)
                _runtimeAtlasSettings.Clear();

            var currentTexturePath = string.Empty;
            shaco.Base.Coroutine.Foreach(allTexturesPath, (obj) =>
            {
                var texturePath = obj as string;
                currentTexturePath = texturePath;
                try
                {
                    OptimizeAltasOnce(texturePath, textureDepencies);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("SpriteAtlasSettingsWindow OptimizeAltas error: e=" + e);
                }
                return true;
            }, (percent) =>
            {
                if (percent < 1)
                    EditorUtility.DisplayCancelableProgressBar("Optimizing", currentTexturePath, percent);
                else
                {
                    EditorUtility.ClearProgressBar();
                    DeleteEmptyFolder();
                    SetRuntimeAtlasSettingsDirty();

                    if (_isAutoPackPreviewAfterOptimize)
                    {
                        // var spriteAtlasGUID = AssetDatabase.FindAssets("t:spriteatlas", new string[] { _textureRootPath });
                        // var atlases = new UnityEngine.U2D.SpriteAtlas[spriteAtlasGUID.Length];
                        // for (int i = spriteAtlasGUID.Length - 1; i >= 0; --i)
                        // {
                        //     var assetPath = AssetDatabase.GUIDToAssetPath(spriteAtlasGUID[i]);
                        //     atlases[i] = AssetDatabase.LoadAssetAtPath<UnityEngine.U2D.SpriteAtlas>(assetPath);
                        // }

                        try
                        {
                            //该方法在unity2018.4.5f1打包图集(不确定是单个还是多个图集导致)时候会出现死循环
                            //所以需要使用PackAllAtlas方法代替
                            // UnityEditor.U2D.SpriteAtlasUtility.PackAtlases(atlases, EditorUserBuildSettings.activeBuildTarget);

                            UnityEditor.U2D.SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget);

                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("SpriteAtlasSettingsWindow PackAtlases error: e=" + e);
                        }
                    }
                }
            }, 0.1f);
        }

        private void OptimizeAltasOnce(string texturePath, Dictionary<string, shaco.Instance.FindReference.FindReferenceWindow.DependentInfo> textureDepencies)
        {
            var textureAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);

            //不可打入图集
            if (!_defaultSettings.CanBuildInAtlas(textureAsset))
            {
                MoveToNoAtlasFolder(texturePath);
                return;
            }

            if (null != textureDepencies)
            {
                //可打入文件夹共享图集
                var depencies = textureDepencies[texturePath];
                if (_defaultSettings.CanBuildInSubSharedAtlas(textureAsset, depencies.dependencies))
                {
                    MoveToSubSharedAtlasFolder(texturePath);
                    return;
                }

                //可打入共享图集
                if (_defaultSettings.CanBuildInGlobalAtlas(textureAsset, depencies.dependencies))
                {
                    MoveToGlobalAtlasFolder(texturePath);
                    return;
                }
            }

            //可打入普通图集
            MoveToInAtlasFolder(texturePath);
        }

        private void MoveToNoAtlasFolder(string texturePath)
        {
            if (texturePath.Contains(NO_ATLAS_FOLDER))
                return;

            var newTexturePath = GetNewTexutrePath(texturePath, NO_ATLAS_FOLDER);
            EditorHelper.CheckFolderPathWithAutoCreate(shaco.Base.FileHelper.GetFolderNameByPath(newTexturePath));
            AssetDatabase.MoveAsset(texturePath, newTexturePath);
        }

        private void MoveToSubSharedAtlasFolder(string texturePath)
        {
            if (texturePath.Contains(IN_SUB_SHARED_ATLAS_FOLDER))
            {
                AutoCreateSpriteAtlas(texturePath);
                return;
            }

            var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(texturePath);
            var fileName = shaco.Base.FileHelper.GetLastFileName(texturePath);
            var subFolderPath = shaco.Base.FileHelper.RemoveLastPathByLevel(folderPath, 1).RemoveLastFlag();

            subFolderPath = ResetToOriginalPath(subFolderPath);

            var newTexturePath = (subFolderPath + IN_SUB_SHARED_ATLAS_FOLDER).ContactPath(fileName);
            AutoCreateSpriteAtlas(newTexturePath);
            AssetDatabase.MoveAsset(texturePath, newTexturePath);
        }

        private void MoveToGlobalAtlasFolder(string texturePath)
        {
            var globalAtlasPath = this.GetGlobalAtlasPath();
            if (texturePath.Contains(GLOBAL_ATLAS_FOLDER))
            {
                AutoCreateSpriteAtlas(texturePath);
                return;
            }

            var fileName = shaco.Base.FileHelper.GetLastFileName(texturePath);
            var newTexturePath = globalAtlasPath.ContactPath(fileName);
            AutoCreateSpriteAtlas(newTexturePath);
            AssetDatabase.MoveAsset(texturePath, newTexturePath);
        }

        private void MoveToInAtlasFolder(string texturePath)
        {
            if (texturePath.Contains(IN_ATLAS_FOLDER))
            {
                AutoCreateSpriteAtlas(texturePath);
                return;
            }

            var newTexturePath = GetNewTexutrePath(texturePath, IN_ATLAS_FOLDER);
            AutoCreateSpriteAtlas(newTexturePath);
            AssetDatabase.MoveAsset(texturePath, newTexturePath);
        }

        private string GetNewTexutrePath(string texturePath, string folderTag)
        {
            var originalTexturePath = ResetToOriginalPath(texturePath);
            var findIndex = originalTexturePath.LastIndexOf(shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            if (findIndex < 0)
            {
                Debug.LogError("SpriteAtlasSettingsWindow MoveToNoAtlasFolder error: not a unity asset path=" + originalTexturePath);
                return string.Empty;
            }

            var fileName = shaco.Base.FileHelper.GetLastFileName(originalTexturePath);
            var folderPath = originalTexturePath.Remove(findIndex) + folderTag;
            var newTexturePath = folderPath.ContactPath(fileName);
            return newTexturePath;
        }

        private string ResetToOriginalPath(string path)
        {
            if (path.Contains(NO_ATLAS_FOLDER))
                path = path.Remove(NO_ATLAS_FOLDER);

            if (path.Contains(IN_ATLAS_FOLDER))
                path = path.Remove(IN_ATLAS_FOLDER);

            if (path.Contains(IN_SUB_SHARED_ATLAS_FOLDER))
                path = path.Remove(IN_SUB_SHARED_ATLAS_FOLDER);

            if (path.Contains(GLOBAL_ATLAS_FOLDER))
                path = path.Remove(GLOBAL_ATLAS_FOLDER);

            return path;
        }

        private void AutoCreateSpriteAtlas(string newTexturePath)
        {
            var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(newTexturePath).RemoveLastFlag();
            var spriteAtlasName = shaco.Base.FileHelper.GetLastFileName(folderPath);
            spriteAtlasName = shaco.Base.FileHelper.AddExtensions(spriteAtlasName, shaco.SpriteAtlasDefine.EXTENSION_SPRITE_ATTLAS);
            var spriteAtlasPath = folderPath.ContactPath(spriteAtlasName);

            if (!shaco.Base.FileHelper.ExistsFile(EditorHelper.GetFullPath(spriteAtlasPath)))
            {
                EditorHelper.CheckFolderPathWithAutoCreate(folderPath);

                var newSpritAtlas = new UnityEngine.U2D.SpriteAtlas();
                var folderAsset = AssetDatabase.LoadAssetAtPath<Object>(folderPath);
                UnityEditor.U2D.SpriteAtlasExtensions.Add(newSpritAtlas, new Object[] { folderAsset });
                UnityEditor.U2D.SpriteAtlasExtensions.SetIncludeInBuild(newSpritAtlas, false);
                AssetDatabase.CreateAsset(newSpritAtlas, spriteAtlasPath);
            }

            AddRuntimeAtlasSettingsInfo(spriteAtlasName, folderPath);
        }

        private string GetGlobalAtlasPath()
        {
            return _textureRootPath.ContactPath(GLOBAL_ATLAS_FOLDER);
        }

        private void DeleteEmptyFolder()
        {
            if (!_isAutoDeleteEmptyFolder)
                return;

            var fullPath = EditorHelper.GetFullPath(_textureRootPath);
            var folders = shaco.Base.FileHelper.GetDirectories(fullPath, "*", SearchOption.AllDirectories);
            if (folders.IsNullOrEmpty())
                return;

            bool hasEmptyFolder = false;
            for (int i = 0; i < folders.Length; ++i)
            {
                hasEmptyFolder |= shaco.Base.FileHelper.DeleteEmptyFolder(folders[i], ".meta", ".DS_Store", shaco.SpriteAtlasDefine.EXTENSION_SPRITE_ATTLAS);
            }

            if (hasEmptyFolder)
            {
                AssetDatabase.Refresh();
            }
        }
    }
}

#endif