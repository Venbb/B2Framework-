using UnityEngine;
using System.Collections.Generic;

namespace shaco
{
    public class ResourcesEditor : IResources
    {
        private ResourcesRuntime _resourcesRuntime = new ResourcesRuntime();

#if UNITY_EDITOR
        private Dictionary<string, string[]> _loadedEditorPaths = new Dictionary<string, string[]>();
#endif

        public UnityEngine.Object Load(string path, System.Type type, string multiVersionControlRelativePath)
        {
            UnityEngine.Object retValue = null;
#if UNITY_EDITOR
            retValue = LoadUnityEditorAsset(path, type, multiVersionControlRelativePath);
            if (null != retValue)
            {
                shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
            }
#endif
            if (null == retValue)
                retValue = _resourcesRuntime.Load(path, type, multiVersionControlRelativePath);
			return retValue;
        }

        public UnityEngine.Object[] LoadAll(string path, System.Type type, string multiVersionControlRelativePath)
        {
            var retValue = new System.Collections.Generic.List<UnityEngine.Object>();
#if UNITY_EDITOR
            var unityApplicationSubPath = Application.dataPath.Remove("Assets");
            var directoryPath = string.Empty;
            if (!shaco.HotUpdateHelper.IsCustomPrefixPath(path))
                directoryPath = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER.ContactPath(path);
            else
                directoryPath = path;

            directoryPath = shaco.Base.FileHelper.RemoveLastExtension(directoryPath);
            directoryPath = unityApplicationSubPath.ContactPath(directoryPath);
            if (!shaco.Base.FileHelper.ExistsDirectory(directoryPath))
                return retValue.ToArray();

            var allFiles = shaco.Base.FileHelper.GetFiles(directoryPath, "*", System.IO.SearchOption.AllDirectories);
            foreach (var iter in allFiles)
            {
                //过滤meta文件
                if (iter.EndsWith(".meta"))
                    continue;

                //只处理Unity支持的文件
                var loadAssetTmp = UnityEditor.AssetDatabase.LoadAssetAtPath("assets" + iter.RemoveFront("assets"), type);
                if (null != loadAssetTmp)
                {
                    retValue.Add(loadAssetTmp);
                }
            }
#endif
			if (retValue.IsNullOrEmpty())
            {
                return _resourcesRuntime.LoadAll(path, type, multiVersionControlRelativePath);
            }
            else 
            {
                shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
                return retValue.ToArray();
            }
        }

        public void LoadAsync(string path, System.Type type, System.Action<float> callbackProgress, System.Action<UnityEngine.Object> callbackEnd, string multiVersionControlRelativePath)
        {
			UnityEngine.Object loadAsset = null;
#if UNITY_EDITOR
            loadAsset = LoadUnityEditorAsset(path, type, multiVersionControlRelativePath);
            if (null != loadAsset)
            {
                //已经加载过的资源不再异步加载，而是直接同步加载了，保持模拟Editor逻辑和Runtime一致
                if (shaco.GameHelper.resCache.IsLoadedAssetBundle(path))
                {
                    shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
                    if (null != callbackProgress)
                        callbackProgress(1.0f);
                    if (null != callbackEnd)
                        callbackEnd(loadAsset);
                }
                else
                {
                    shaco.GameHelper.resCache.AddReferenceCount(path, multiVersionControlRelativePath, true);
                    shaco.Delay.Run(() =>
                    {
                        if (null != callbackProgress)
                            callbackProgress(1.0f);
                        if (null != callbackEnd)
                            callbackEnd(loadAsset);
                    }, Time.deltaTime * 2);
                }
            }
#endif
			if (null == loadAsset)
			{
                _resourcesRuntime.LoadAsync(path, type, callbackProgress, callbackEnd, multiVersionControlRelativePath);
            }
        }

        private UnityEngine.Object LoadUnityEditorAsset(string path, System.Type type, string multiVersionControlRelativePath)
        {
            UnityEngine.Object retValue = null;
            if (string.IsNullOrEmpty(path))
            {
                shaco.Log.Error("ResourcesEditor LoadUnityEditorAsset error: path is empty");
                return retValue;
            }
#if UNITY_EDITOR
            //以assetbundle作为后缀名是无效的
            if (path.EndsWith(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE))
            {
                path = path.RemoveBehind(shaco.HotUpdateDefine.EXTENSION_ASSETBUNDLE);
            }

            if (!shaco.Base.FileHelper.HasFileNameExtension(path))
            {
                // shaco.Log.Warning("If you want to load folder resources directly in editor mode, please add suffix, which will be much faster, path=" + path);
                var fullPath = shaco.HotUpdateHelper.IsCustomPrefixPath(path) ? path : shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER.ContactPath(path);
                var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(fullPath);
                if (!shaco.Base.FileHelper.ExistsDirectory(shaco.UnityHelper.UnityPathToFullPath(folderPath)))
                    return retValue;

                string[] findPaths = null;
                if (!_loadedEditorPaths.TryGetValue(folderPath, out findPaths))
                {
                    var allFiles = shaco.Base.FileHelper.GetFiles(folderPath, "*", System.IO.SearchOption.TopDirectoryOnly).ToList();
                    if (null != allFiles)
                    {
                        for (int i = allFiles.Count - 1; i >= 0; --i)
                        {
                            //过滤文件夹
                            if (shaco.Base.FileHelper.ExistsDirectory(allFiles[i]))
                            {
                                allFiles.RemoveAt(i);
                            }
                            else
                            {
                                //过滤各种隐藏文件
                                var lastFileName = shaco.Base.FileHelper.GetLastFileName(allFiles[i]);
                                if (lastFileName.StartsWith("."))
                                    allFiles.RemoveAt(i);
                            }
                            
                        }

                        findPaths = allFiles.ToArray();
                        _loadedEditorPaths.Add(folderPath, findPaths);
                    }
                }

                if (null != findPaths)
                {
                    var fileNameLower = shaco.Base.FileHelper.GetLastFileName(path).ToLower();
                    for (int i = findPaths.Length - 1; i >= 0; --i)
                    {
                        var fileNameWithoutExtentsion = shaco.Base.FileHelper.RemoveLastExtension(shaco.Base.FileHelper.GetLastFileName(findPaths[i]));
                        if (fileNameWithoutExtentsion.ToLower() == fileNameLower)
                        {
                            var loadAssetTmp = UnityEditor.AssetDatabase.LoadAssetAtPath(findPaths[i], type);
                            if (null != loadAssetTmp)
                            {
                                if (loadAssetTmp.GetType().IsInherited(type))
                                {
                                    retValue = loadAssetTmp;
                                    break;
                                }
                            }
                            else
                                Log.Error("ResourcesEditor LoadUnityEditorAsset error: mismatched type, asset=" + findPaths[i] + " load type=" + type);
                        }
                    }
                }
            }
            else 
            {
                path = path.ToLower();
                if (!shaco.HotUpdateHelper.IsCustomPrefixPath(path))
                    path = shaco.GameHelper.res.DEFAULT_PREFIX_PATH_LOWER.ContactPath(path);
                retValue = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
            }
#else
            Log.Error("ResourcesEditor LoadUnityEditorAsset error: only support on Unity Editor mode");
#endif
            return retValue;
        }
    }
}