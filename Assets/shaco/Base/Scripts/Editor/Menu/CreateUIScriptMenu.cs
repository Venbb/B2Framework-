using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class CreateUIScriptMenu : Editor
    {
        private enum AddScriptType
        {
            CSharp,
            Lua
        }

        private const string _modelCSharpScritPath = "Base/Scripts/Editor/Menu/CreateUIScriptModel.cs";
        private const string _modelCSharp2ScritPath = "Base/EditorResources/LuaBehaviourTemplate.cs";
        private const string _modelLuaScritPath = "Base/EditorResources/LuaBehaviourTemplate" + shaco.Base.GlobalParams.LUA_FILE_EXTENSIONS;
        private const string WILL_ADD_SCRIPT_TARGET_PATH = "CreateUIScriptMenu.WillAddScriptTarget";
        private const string WILL_ADD_SCRIPT_TYPE = "CreateUIScriptMenu.WillAddScriptType";

        [MenuItem("Assets/Create/shaco/UIScript(C#)", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_UI)]
        static public void CreateUICSharpScript()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;

            var selectGameObjects = new List<GameObject>();
            for (int i = 0; i < Selection.gameObjects.Length; ++i)
            {
                var targetTmp = Selection.gameObjects[i];
                var newScriptFile = GetScriptFile(targetTmp, _modelCSharpScritPath);

                if (!string.IsNullOrEmpty(newScriptFile))
                {
                    var newScriptPath = GetScriptPath(targetTmp, ".cs");
                    newScriptFile = ReplaceScriptClassName(newScriptFile, targetTmp.name);
                    if (shaco.Base.FileHelper.ExistsFile(newScriptPath))
                    {
                        var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(newScriptPath);
                        if (!EditorUtility.DisplayDialog("Warning", string.Format("'{0}' already exists in this location. Do you want to replace it?", fileNameTmp), "Overwrite", "Cancel"))
                            continue;
                    }
                    shaco.Base.FileHelper.WriteAllByUserPath(newScriptPath, newScriptFile);
                    selectGameObjects.Add(targetTmp);
                }
            }

            SetWillAddScriptTargets(selectGameObjects.ToArray(), AddScriptType.CSharp);
            AssetDatabase.Refresh();

            if (!EditorApplication.isCompiling)
            {
                AddScriptToPrefab();
            }
        }

        [MenuItem("Assets/Create/shaco/UIScript(C#)", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_UI)]
        static public bool CreateUICSharpScriptValidate()
        {
            var currentSelectGameObject = Selection.activeGameObject;
            if (null == currentSelectGameObject)
                return false;

            var components = currentSelectGameObject.GetComponents<Component>();
            return !components.Find(v => null != v && v.GetType().Name.Contains(currentSelectGameObject.name));
        }

        [MenuItem("Assets/Create/shaco/UIScript(Lua)", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_UI)]
        static public void CreateUILuaScript()
        {
            if (Selection.gameObjects == null || Selection.gameObjects.Length == 0) return;

            var selectGameObjects = new List<GameObject>();
            for (int i = 0; i < Selection.gameObjects.Length; ++i)
            {
                var targetTmp = Selection.gameObjects[i];
                var newCSharpScriptFile = GetScriptFile(targetTmp, _modelCSharp2ScritPath);
                var newLuaScriptFile = GetScriptFile(targetTmp, _modelLuaScritPath);

                if (!string.IsNullOrEmpty(newCSharpScriptFile))
                {
                    var newCSharpScriptPath = GetScriptPath(targetTmp, ".cs");
                    var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(newCSharpScriptPath);

                    if (shaco.Base.FileHelper.ExistsFile(newCSharpScriptPath))
                    {
                        var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(newCSharpScriptPath);
                        if (!EditorUtility.DisplayDialog("Warning", string.Format("'{0}' already exists in this location. Do you want to replace it?", fileNameTmp), "Overwrite", "Cancel"))
                            continue;
                    }
                    newCSharpScriptFile = ReplaceScriptClassName(newCSharpScriptFile, targetTmp.name);
                    shaco.Base.FileHelper.WriteAllByUserPath(newCSharpScriptPath, newCSharpScriptFile);
                }

                if (!string.IsNullOrEmpty(newLuaScriptFile))
                {
                    var newLuaScriptPath = GetScriptPath(targetTmp, shaco.Base.GlobalParams.LUA_FILE_EXTENSIONS);
                    if (shaco.Base.FileHelper.ExistsFile(newLuaScriptPath))
                    {
                        var fileNameTmp = shaco.Base.FileHelper.GetLastFileName(newLuaScriptPath);
                        if (!EditorUtility.DisplayDialog("Warning", string.Format("'{0}' already exists in this location. Do you want to replace it?", fileNameTmp), "Overwrite", "Cancel"))
                            continue;
                    }

                    newLuaScriptFile = newLuaScriptFile.Replace("__ClassName__", targetTmp.name);
                    shaco.Base.FileHelper.WriteAllByUserPath(newLuaScriptPath, newLuaScriptFile);
                }

                selectGameObjects.Add(targetTmp);
            }

            SetWillAddScriptTargets(selectGameObjects.ToArray(), AddScriptType.Lua);
            AssetDatabase.Refresh();

            if (!EditorApplication.isCompiling)
            {
                AddScriptToPrefab();
            }
        }

        [MenuItem("Assets/Create/shaco/UIScript(Lua)", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_UI)]
        static public bool CreateUILuaScriptValidate()
        {
            var currentSelectGameObject = Selection.activeGameObject;
            if (null == currentSelectGameObject)
                return false;

            var components = currentSelectGameObject.GetComponents<Component>();
            return !components.Find(v => null != v && v.GetType().IsInherited<shaco.LuaBehaviour>());
        }

        static private string GetScriptFile(GameObject obj, string modelFileName)
        {
            string retValue = string.Empty;
            if (null == obj)
                return retValue;

            var fullPath = shaco.Base.FileHelper.ContactPath(shaco.Base.GlobalParams.GetShacoFrameworkRootPath(), modelFileName);
            retValue = shaco.Base.FileHelper.ReadAllByUserPath(fullPath);
            return retValue;
        }

        static public void AddScriptToPrefab()
        {
            var willAddScriptTargets = GetWillAddScriptTargets();

            if (null == willAddScriptTargets || willAddScriptTargets.Count == 0)
                return;

            AddScriptType scriptType = (AddScriptType)EditorPrefs.GetInt(WILL_ADD_SCRIPT_TYPE);
            RemoveWillAddScriptTargets();

#if UNITY_2018_1_OR_NEWER
            //获取当前prefab场景
            var currentPrefabState = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
            for (int i = willAddScriptTargets.Count - 1; i >= 0; --i)
            {
                var targetTmp = willAddScriptTargets[i];
                var newScriptPath = GetScriptPath(targetTmp, ".cs");

#if UNITY_2018_1_OR_NEWER
                //当prefab编辑场景存在时候，需要使用编辑场景的prefab
                if (null != currentPrefabState && currentPrefabState.prefabAssetPath == AssetDatabase.GetAssetPath(targetTmp))
                {
                    targetTmp = currentPrefabState.prefabContentsRoot;
                }
#endif
                var applicationRootPath = Application.dataPath.Remove("Assets");
                newScriptPath = newScriptPath.Remove(applicationRootPath);

                var newScript = AssetDatabase.LoadAssetAtPath(newScriptPath, typeof(MonoScript)) as MonoScript;
                var classType = newScript.GetClass();

                if (null == newScript)
                {
                    Debug.LogError("CreateUIScriptMenu AddScriptToPrefab error: Not found script, path=" + newScriptPath);
                    continue;
                }

                var duplicateComponent = targetTmp.GetComponent(classType);
                if (null != duplicateComponent)
                {
                    MonoBehaviour.DestroyImmediate(duplicateComponent, true);
                }

                duplicateComponent = targetTmp.AddComponent(newScript.GetClass());

                //如果是lua脚本还需要给LuaBehaviour挂上lua脚本路径
                if (scriptType == AddScriptType.Lua)
                {
                    var luaBehaviour = duplicateComponent as shaco.LuaBehaviour;
                    if (null == luaBehaviour)
                    {
                        Debug.LogError("CreateUIScriptMenu AddScriptToPrefab erorr: Component can't conver to 'LuaBehaviour'");
                        continue;
                    }
                    var newLuaScriptPath = GetScriptPath(willAddScriptTargets[i], shaco.Base.GlobalParams.LUA_FILE_EXTENSIONS);
                    luaBehaviour.SetPath(EditorHelper.FullPathToUnityAssetPath(newLuaScriptPath));
                }
            }

#if UNITY_2018_1_OR_NEWER
            //获取当前prefab场景
            if (null != currentPrefabState)
            {
                //prefab场景已经打开的情况直接保存就可以了
                PrefabUtility.SaveAsPrefabAsset(currentPrefabState.prefabContentsRoot, currentPrefabState.prefabAssetPath);
                currentPrefabState.ClearDirtiness();
            }
#endif

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static private string GetScriptPath(GameObject target, string extension)
        {
            string retValue = string.Empty;

            if (null == target)
                return retValue;

            var prefabPath = AssetDatabase.GetAssetPath(target);
            if (string.IsNullOrEmpty(prefabPath))
            {
                Debug.LogError("CreateUIScriptMenu GetScriptPath error: not a asset, target=" + target);
                return retValue;
            }

            prefabPath = prefabPath.Replace('\\', '/');
            var folder = shaco.Base.FileHelper.GetFolderNameByPath(prefabPath).Remove("Assets/");
            retValue = shaco.Base.FileHelper.ContactPath(Application.dataPath, folder + target.name + extension);

            return retValue;
        }

        static private void SetWillAddScriptTargets(GameObject[] targets, AddScriptType scriptType)
        {
            if (targets != null && targets.Length > 0)
            {
                var prefabPaths = string.Empty;
                for (int i = 0; i < targets.Length; ++i)
                {
                    var prefabPath = AssetDatabase.GetAssetPath(targets[i]);
                    prefabPaths += prefabPath + "@";
                }

                prefabPaths = prefabPaths.Remove(prefabPaths.Length - 1, 1);
                EditorPrefs.SetString(WILL_ADD_SCRIPT_TARGET_PATH, prefabPaths);
            }
            EditorPrefs.SetInt(WILL_ADD_SCRIPT_TYPE, (int)scriptType);
        }

        static private List<GameObject> GetWillAddScriptTargets()
        {
            var retValue = new List<GameObject>();
            var prefabPaths = EditorPrefs.GetString(WILL_ADD_SCRIPT_TARGET_PATH);

            if (!string.IsNullOrEmpty(prefabPaths))
            {
                var prefabSplitPaths = prefabPaths.Split("@");
                for (int i = 0; i < prefabSplitPaths.Length; ++i)
                {
                    var prefabPath = prefabSplitPaths[i];
                    if (!string.IsNullOrEmpty(prefabPath))
                    {
                        var newPrefab = AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
                        if (newPrefab == null)
                        {
                            shaco.Log.Error("CreateUIScriptMenu GetWillAddScriptTargets error: can't load prefab path=" + prefabPath);
                            RemoveWillAddScriptTargets();
                        }
                        else
                            retValue.Add(newPrefab);
                    }
                }
            }
            return retValue;
        }

        static private void RemoveWillAddScriptTargets()
        {
            EditorPrefs.DeleteKey(WILL_ADD_SCRIPT_TARGET_PATH);
            EditorPrefs.DeleteKey(WILL_ADD_SCRIPT_TYPE);
        }

        static private string ReplaceScriptClassName(string script, string newClassName)
        {
            int indexClass = script.IndexOf("class");
            if (indexClass < 0)
            {
                shaco.Log.Error("CreateUIScriptMenu ReplaceScriptClassName error: not find 'class' flag ");
                return script;
            }

            indexClass += "class".Length;

            //get old class name
            string oldClassName = string.Empty;
            for (int i = indexClass; i < script.Length; ++i)
            {
                var cTmp = script[i];
                if (cTmp != ' ' && cTmp != '\t')
                {
                    oldClassName += cTmp;
                }
                else
                {
                    if (!string.IsNullOrEmpty(oldClassName))
                    {
                        break;
                    }
                }
            }

            return script.Replace(oldClassName, newClassName);
        }
    }
}