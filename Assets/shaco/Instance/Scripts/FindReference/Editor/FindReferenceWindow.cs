using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace shaco.Instance.FindReference
{
    public class FindReferenceWindow : EditorWindow
    {
        //被依赖信息
        public class DependentInfo
        {
            public string assetPath { get { return _assetPath; } private set { _assetPath = value; } }
            public List<string> dependencies = new List<string>();

            private string _assetPath = null;

            public DependentInfo(string assetPath)
            {
                this.assetPath = assetPath;
            }
        }

        //动态获取到的引用信息
        public class DepenciesInfo
        {
            public string guid = null;
            public string assetPath = null;
        }

        //缓存起来的引用信息
        public class CacheDepenciesInfo
        {
            public string assetPath { get { return _assetPath; } private set { _assetPath = value; } }
            public Hash128 hash128 { get { return _hash128; } private set { _hash128 = value; } }
            public string[] dependenciesGUID = null;

            private string _assetPath = null;
            private Hash128 _hash128;

            public CacheDepenciesInfo(string assetPath, Hash128 hash128)
            {
                this.hash128 = hash128;
                this.assetPath = assetPath;
            }
        }

        public enum FindStatus
        {
            None,
            NoReference,
            FindReference,
        }

        private shaco.Instance.Editor.TreeView.PathTreeView _treeViewUnuse = null;
        private shaco.Instance.Editor.TreeView.PathTreeView _treeViewReferenced = null;
        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparator = new shaco.Instance.Editor.TreeView.WindowSplitter(shaco.Instance.Editor.TreeView.WindowSplitter.Direction.Vertical);
        private Rect _prevRectDrawFolder = new Rect();
        private Rect _currentRectDrawFolder = new Rect();
        private FindStatus _statusFind = FindStatus.None;
        private int _iReferenceCountTmp = 0;
        private List<string> _unusedFilePath = new List<string>();
        static private IList _currentForeachCollections = null;
        static private System.Func<object, bool> _currentForeachCallbackData = null;
        static private System.Action<float> _currentForeachcallbackProgress = null;
        static private int _currentForeachCountInPerFrame = 1;
        static private int _currentForeachIndex = 0;

        [MenuItem("Assets/Find References In Project %#&f", false, 20)]
        static void OpenFindReferenceWindowInProjectMenu()
        {
            var selectAssetTmp = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);

            if (selectAssetTmp.Length > 0)
            {
                var assetsPathTmp = new string[selectAssetTmp.Length];
                for (int i = 0; i < selectAssetTmp.Length; ++i)
                {
                    assetsPathTmp[i] = AssetDatabase.GetAssetPath(selectAssetTmp[i]);
                }
                OpenFindReferenceWindow().FindReferencesInProject(assetsPathTmp);
            }
        }

        static FindReferenceWindow OpenFindReferenceWindow()
        {
            var retValue = EditorWindow.GetWindow<FindReferenceWindow>(true, "FindReference");
            retValue.Init();
            return retValue;
        }

        static public void FindReferencesInProject(System.Action<Dictionary<string, DependentInfo>> callbackEnd, bool useProgressBar, params string[] selectAssets)
        {
            FindReferencesInProjectBase(callbackEnd, useProgressBar, selectAssets);
        }

        void Init()
        {
            _treeViewUnuse = new shaco.Instance.Editor.TreeView.PathTreeView();
            _treeViewReferenced = new shaco.Instance.Editor.TreeView.PathTreeView();

            _treeViewUnuse.callbackTopBarGUI += () =>
            {
                GUILayout.Label("Unuse Asset Count:" + _treeViewUnuse.fileCount);
                return 18;
            };

            _treeViewReferenced.callbackTopBarGUI += () =>
            {
                GUILayout.Label("Referenced Asset Count:" + _treeViewReferenced.fileCount);
                return 18;
            };

            _treeViewReferenced.AddHeader("ExpandAll", () => _treeViewReferenced.ExpandAll());
            _treeViewReferenced.AddHeader("CollapseAll", () => _treeViewReferenced.CollapseAll());
            _treeViewUnuse.AddHeader("ExpandAll", () => _treeViewUnuse.ExpandAll());
            _treeViewUnuse.AddHeader("CollapseAll", () => _treeViewUnuse.CollapseAll());

            //批量修改对象属性内容，现版本发现并没有太大作用了，所以注释掉
            // _pathTreeView.AddIgnoreFolderTag(".");
            //     _pathTreeView.AddContextMenuWithItems("ChangeReferences", (contextInfos) =>
            //    {
            //        var allAssetsTmp = new List<Object>();
            //        for (int i = 0; i < allFiles.Count; ++i)
            //        {
            //            allAssetsTmAp.Add(AssetDatabase.LoadAssetAtPath(realPath, typeof(Object)));
            //        }

            //        ChangeComponentDataWindow.OpenChangeComponentDataWindowInProjectMenu(AssetDatabase.LoadAssetAtPath(selectFullPath, typeof(Object)), allAssetsTmp);
            //    });
        }

        void OnDestroy()
        {
        }

        void OnGUI()
        {
            if (null == _treeViewUnuse)
                return;

            if (_dragLineSeparator.isDragSplitter)
                this.Repaint();

            if (_statusFind == FindStatus.None)
            {
                GUILayout.Label("please select a asset in ''Project' window\n");
            }

            GUILayout.Space(0);

            var currentEvent = Event.current;
            if (_prevRectDrawFolder != this.position)
            {
                if ((null != currentEvent && currentEvent.type == EventType.Repaint))
                {
                    var lastRect = GUILayoutUtility.GetLastRect();
                    _prevRectDrawFolder = this.position;
                    _currentRectDrawFolder = new Rect(0, lastRect.yMax, this.position.width, this.position.height - lastRect.yMax);

                    //设置拆分绘制窗口
                    if (_treeViewUnuse.hasChildren && _treeViewReferenced.hasChildren)
                    {
                        _dragLineSeparator.SetSplitWindow(_currentRectDrawFolder);
                    }
                }
            }

            //带分割线的绘制
            if (_treeViewUnuse.hasChildren && _treeViewReferenced.hasChildren)
            {
                _dragLineSeparator.BeginLayout(true);
                {
                    _treeViewUnuse.DrawTreeView(_dragLineSeparator.GetSplitWindowRect(0));
                }
                _dragLineSeparator.EndLayout();
                _dragLineSeparator.BeginLayout();
                {
                    _treeViewReferenced.DrawTreeView(_dragLineSeparator.GetSplitWindowRect(1));
                }
                _dragLineSeparator.EndLayout();
            }
            //单独绘制
            else if (_treeViewUnuse.hasChildren)
            {
                _treeViewUnuse.DrawTreeView(_currentRectDrawFolder);
            }
            else if (_treeViewReferenced.hasChildren)
            {
                _treeViewReferenced.DrawTreeView(_currentRectDrawFolder);
            }
        }

        static private void Update()
        {
            if (null == _currentForeachCollections)
                return;

            //停止遍历
            if (_currentForeachIndex >= _currentForeachCollections.Count)
            {
                _currentForeachIndex = 0;
                _currentForeachCollections = null;
                _currentForeachcallbackProgress(1.0f);
                return;
            }

            var loopCount = System.Math.Min(_currentForeachCountInPerFrame, _currentForeachCollections.Count - _currentForeachIndex);
            for (int i = 0; i < loopCount; ++i)
            {
                var index = _currentForeachIndex + i;
                var data = _currentForeachCollections[index];
                if (!_currentForeachCallbackData(data))
                {
                    _currentForeachIndex = 0;
                    _currentForeachCollections = null;
                    _currentForeachcallbackProgress(1.0f);
                    return;
                }
            }

            _currentForeachIndex += loopCount;
            var currentPercent = (float)_currentForeachIndex / (float)_currentForeachCollections.Count;
            if (currentPercent >= 1.0f)
                currentPercent = 0.99f;
            _currentForeachcallbackProgress(currentPercent);
        }

        private void DrawDeleteUnsusedAssetsButton()
        {
            if (_unusedFilePath.Count > 0)
            {
                var appendString = new System.Text.StringBuilder();
                int maxShowAssetPath = System.Math.Min(_unusedFilePath.Count, 10);
                for (int i = 0; i < maxShowAssetPath; ++i)
                {
                    appendString.Append(_unusedFilePath[i]);
                    appendString.Append('\n');
                }
                if (_unusedFilePath.Count > maxShowAssetPath)
                {
                    appendString.Append("more assets...");
                }

                var titleDisplay = _unusedFilePath.Count > 1 ? string.Format("Delete selected {0} assets", _unusedFilePath.Count) : "Delete selected asset";
                if (EditorUtility.DisplayDialog(string.Format(titleDisplay, _unusedFilePath.Count), appendString.ToString(), "Delete", "Cancel"))
                {
                    for (int i = _unusedFilePath.Count - 1; i >= 0; --i)
                    {
                        var fullPath = System.IO.Path.GetFullPath(_unusedFilePath[i]);
                        AssetDatabase.MoveAssetToTrash(_unusedFilePath[i]);
                        FindReferenceWindow.DeleteEmptyFolder(fullPath, ".DS_Store", ".meta");
                    }
                    _treeViewUnuse.Clear();
                    AssetDatabase.Refresh();
                }
            }
        }

        private void FindReferencesInProject(params string[] selectAssets)
        {
            _treeViewUnuse.Clear();
            _unusedFilePath.Clear();
            _statusFind = FindStatus.None;
            _iReferenceCountTmp = 0;
            
            FindReferencesInProjectBase((mapFindDependencies) =>
            {
                foreach (var iter in mapFindDependencies)
                {
                    var listDependenciesTmp = iter.Value.dependencies;
                    var pathAsset = iter.Value.assetPath;

                    if (listDependenciesTmp.Count == 0)
                    {
                        _treeViewUnuse.AddPath(pathAsset);
                        _unusedFilePath.Add(pathAsset);
                    }
                    else
                    {
                        _treeViewReferenced.AddPath(iter.Value.assetPath, listDependenciesTmp);
                        ++_iReferenceCountTmp;
                    }
                }

                _statusFind = mapFindDependencies.Count > 0 ? FindStatus.FindReference : FindStatus.NoReference;
                _treeViewUnuse.ExpandAll();
                _treeViewReferenced.ExpandAll();
                AssetDatabase.Refresh();
                _prevRectDrawFolder.width = _prevRectDrawFolder.height = 0;

                //设置拆分绘制窗口
                if (_treeViewUnuse.hasChildren && _treeViewReferenced.hasChildren)
                {
                    _dragLineSeparator.SetSplitWindow(this, 0.5f, 0.5f);
                }

                if (_unusedFilePath.Count > 0)
                    _treeViewUnuse.AddHeader("Delete Unuse Assets", () => DrawDeleteUnsusedAssetsButton());

                this.Repaint();
            }, true, selectAssets);
        }

        static private void FindReferencesInProjectBase(System.Action<Dictionary<string, DependentInfo>> callbackEnd, bool useProgressBar, params string[] selectAssets)
        {
            EditorApplication.update += Update;
            var cachedDepenciesInfo = GetDepenciesFromCached();

            //some prefab only can changed when save scene
            if (!Application.isPlaying)
            {
                if (!string.IsNullOrEmpty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name))
                {
#if UNITY_5_3_OR_NEWER
                    UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
#else
                    EditorApplication.SaveScene();
#endif
                }
            }

            if (selectAssets.Length > 0)
            {
                GetDependenciesInProject(selectAssets, cachedDepenciesInfo, (Dictionary<string, DependentInfo> mapFindDependencies) =>
                {
                    EditorApplication.update -= Update;
                    if (null != callbackEnd)
                    {
                        callbackEnd(mapFindDependencies);
                        return;
                    }
                }, useProgressBar);
            }
        }

        static private Dictionary<string, CacheDepenciesInfo> GetDepenciesFromCached()
        {
            Dictionary<string, CacheDepenciesInfo> retValue = null;
            var savePath = System.IO.Path.Combine(Application.dataPath, "../Library/shacoDepenciesCache.txt");

            if (!System.IO.File.Exists(savePath))
                return retValue;

            retValue = new Dictionary<string, CacheDepenciesInfo>();
            var dependenciesGUIDTmp = new List<string>();
            try
            {
                using (var readStream = new System.IO.StreamReader(savePath))
                {
                    while (true)
                    {
                        var assetPath = readStream.ReadLine();
                        if (string.IsNullOrEmpty(assetPath))
                            break;

                        var hash128String = readStream.ReadLine();
                        if (string.IsNullOrEmpty(hash128String))
                        {
                            Debug.LogError("FindReferenceWindow GetDepenciesFromCached error: not found has128 string, name=" + assetPath);
                            break;
                        }

                        var newDependInfo = new CacheDepenciesInfo(assetPath, Hash128.Parse(hash128String));

                        var dependCountString = readStream.ReadLine();
                        if (string.IsNullOrEmpty(assetPath))
                        {
                            Debug.LogError("FindReferenceWindow GetDepenciesFromCached error: not found depend count, name=" + assetPath);
                            break;
                        }

                        var dependCount = int.Parse(dependCountString);
                        dependenciesGUIDTmp.Clear();
                        for (int i = 0; i < dependCount; ++i)
                        {
                            var dependAssetGUID = readStream.ReadLine();
                            if (string.IsNullOrEmpty(dependAssetGUID))
                            {
                                Debug.LogError("FindReferenceWindow GetDepenciesFromCached error: not found depend asset, name=" + assetPath);
                                break;
                            }
                            dependenciesGUIDTmp.Add(dependAssetGUID);
                        }
                        newDependInfo.dependenciesGUID = dependenciesGUIDTmp.ToArray();

                        if (retValue.ContainsKey(assetPath))
                            Debug.LogError("FindReferenceWindow GetDepenciesFromCached error: not found depend count, name=" + assetPath);
                        else
                            retValue.Add(assetPath, newDependInfo);
                    }
                    readStream.Close();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("FindReferenceWindow GetDepenciesFromCached exception: e=" + e);
                System.IO.File.Delete(savePath);
            }
            return retValue;
        }

        static private void SaveDependenciesToCahce(Dictionary<string, DepenciesInfo[]> dependenciesInfo)
        {
            if (null == dependenciesInfo || 0 == dependenciesInfo.Count)
                return;

            var savePath = System.IO.Path.Combine(Application.dataPath, "../Library/shacoDepenciesCache.txt");
            List<string> dependenciesTmp = new List<string>();
            using (var writeStream = new System.IO.StreamWriter(savePath))
            {
                foreach (var iter in dependenciesInfo)
                {
                    var hash128String = AssetDatabase.GetAssetDependencyHash(iter.Key);

                    writeStream.WriteLine(iter.Key);
                    writeStream.WriteLine(hash128String);

                    dependenciesTmp.Clear();
                    for (int i = iter.Value.Length - 1; i >= 0; --i)
                    {
                        //过滤自己
                        if (iter.Value[i].assetPath == iter.Key)
                            continue;
                        dependenciesTmp.Add(iter.Value[i].assetPath);
                    }

                    writeStream.WriteLine(dependenciesTmp.Count);

                    for (int i = dependenciesTmp.Count - 1; i >= 0; --i)
                    {
                        writeStream.WriteLine(AssetDatabase.AssetPathToGUID(dependenciesTmp[i]));
                    }
                }
                writeStream.Close();

                Debug.Log("FindReferenceWindow SaveDependenciesToCahce: write depencies, path=" + savePath);
            }
        }

        static private void GetDependenciesInProject(string[] selectAssetsPath, Dictionary<string, CacheDepenciesInfo> cachedDepenciesInfo, System.Action<Dictionary<string, DependentInfo>> callback, bool useProgressBar)
        {
            var ret = new Dictionary<string, DependentInfo>();
            bool shouldCancel = false;

            if (useProgressBar)
                shouldCancel = EditorUtility.DisplayCancelableProgressBar("get all files...", "please wait", 0);

            var allSearchAssets = AssetDatabase.FindAssets("t:spriteatlas t:prefab t:scene t:ScriptableObject t:Material t:AnimatorController t:Animation");
            for (int i = 0; i < allSearchAssets.Length; ++i)
            {
                allSearchAssets[i] = AssetDatabase.GUIDToAssetPath(allSearchAssets[i]);
            }

            //collection all files dependencies
            var allDependencies = new Dictionary<string, DepenciesInfo[]>();
            var currentCollectionDepenciesPath = string.Empty;
            int currentCollectionIndex = 0;
            bool isDepenciesCacheChanged = false;
            ForeachCoroutine(allSearchAssets, (object data) =>
            {
                try
                {
                    currentCollectionDepenciesPath = data as string;
                    isDepenciesCacheChanged |= CollectionDepencies(cachedDepenciesInfo, allDependencies, currentCollectionDepenciesPath);
                    ++currentCollectionIndex;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("FindReferenceWindow Collection depencies exception: path=" + currentCollectionDepenciesPath + "\ne=" + e);
                }
                return !shouldCancel;
            }, (float percent) =>
            {
                if (useProgressBar)
                    shouldCancel = EditorUtility.DisplayCancelableProgressBar(string.Format("collection reference [{0}/{1}]", currentCollectionIndex, allSearchAssets.Length), currentCollectionDepenciesPath, 0.5f * percent);
                if (shouldCancel)
                {
                    EditorUtility.ClearProgressBar();
                }

                if (percent >= 1 && !shouldCancel)
                {
                    //select dependencies
                    var currentSelectDepenciesAssetPath = string.Empty;
                    var currentSelectDepenciesIndex = 0;
                    ForeachCoroutine(selectAssetsPath, (object data) =>
                    {
                        bool retValue = true;
                        try
                        {
                            currentSelectDepenciesAssetPath = data as string;
                            retValue = !shouldCancel && SelectDepencies(ret, allSearchAssets, allDependencies, currentSelectDepenciesAssetPath);
                            ++currentSelectDepenciesIndex;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("FindReferenceWindow Select dependencies exception: path=" + currentSelectDepenciesAssetPath + "\ne=" + e);
                        }
                        return retValue;
                    }, (float percent2) =>
                    {
                        try
                        {
                            if (useProgressBar)
                                shouldCancel = EditorUtility.DisplayCancelableProgressBar(string.Format("select depencies [{0}/{1}]", currentSelectDepenciesIndex, selectAssetsPath.Length), currentSelectDepenciesAssetPath, 0.5f * percent2 + 0.5f);
                            if (percent2 >= 1)
                            {
                                EditorUtility.ClearProgressBar();

                                //保存引用关系表到本地缓存文件中
                                if (isDepenciesCacheChanged)
                                {
                                    SaveDependenciesToCahce(allDependencies);
                                }
                                callback(ret);
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("FindReferenceWindow GetDependenciesInProject exception: " + e);
                            EditorUtility.ClearProgressBar();
                        }
                    }, 0.1f);
                }
            }, 0.1f);
        }

        static private void ForeachCoroutine(IList collections, System.Func<object, bool> callbackData, System.Action<float> callbackProgress = null, float speed = 0.05f)
        {
            if (speed < 0)
                speed = 0.05f;
            if (speed > 1)
                speed = 1;

            _currentForeachIndex = 0;
            _currentForeachCollections = collections;
            _currentForeachCallbackData = callbackData;
            _currentForeachcallbackProgress = callbackProgress;
            _currentForeachCountInPerFrame = (int)(_currentForeachCollections.Count * speed);
            if (_currentForeachCountInPerFrame <= 0)
                _currentForeachCountInPerFrame = 1;
        }

        static private bool CollectionDepencies(Dictionary<string, CacheDepenciesInfo> cachedDepenciesInfo, Dictionary<string, DepenciesInfo[]> allDependencies, string path)
        {
            if (allDependencies.ContainsKey(path))
                return false;

            //优先从缓存引用获取信息
            if (null != cachedDepenciesInfo)
            {
                CacheDepenciesInfo findInfo = null;
                if (cachedDepenciesInfo.TryGetValue(path, out findInfo))
                {
                    //如果二者的hash引用值一样则表示引用没有发生变化，直接用缓存的引用信息不再重新动态获取了
                    var hash128String = AssetDatabase.GetAssetDependencyHash(path);
                    if (hash128String.Equals(findInfo.hash128))
                    {
                        allDependencies.Add(path, AssetsGUIDToDepenciesInfo(findInfo.dependenciesGUID));
                        return false;
                    }
                }
            }

#if UNITY_5_3_OR_NEWER
            var listDependence = AssetDatabase.GetDependencies(path);
#else
            var listDependence = AssetDatabase.GetDependencies(new string[]{ path });
#endif
            allDependencies.Add(path, AssetsPathToDepenciesInfo(listDependence));
            return true;
        }

        static private DepenciesInfo[] AssetsGUIDToDepenciesInfo(string[] assetsGUID)
        {
            var retValue = new DepenciesInfo[assetsGUID.Length];
            for (int i = assetsGUID.Length - 1; i >= 0; --i)
            {
                var newInfo = new DepenciesInfo();
                retValue[i] = newInfo;
                newInfo.assetPath = AssetDatabase.GUIDToAssetPath(assetsGUID[i]);
                newInfo.guid = assetsGUID[i];
            }
            return retValue;
        }

        static private DepenciesInfo[] AssetsPathToDepenciesInfo(string[] assetsPath)
        {
            var retValue = new DepenciesInfo[assetsPath.Length];
            for (int i = assetsPath.Length - 1; i >= 0; --i)
            {
                var newInfo = new DepenciesInfo();
                retValue[i] = newInfo;
                newInfo.assetPath = assetsPath[i];
                newInfo.guid = AssetDatabase.AssetPathToGUID(assetsPath[i]);
            }
            return retValue;
        }

        static private bool SelectDepencies(Dictionary<string, DependentInfo> selectDepencies, string[] allSearchAssets, Dictionary<string, DepenciesInfo[]> allDependencies, string assetPath)
        {
            var selectPath = assetPath;
            if (System.IO.Directory.Exists(selectPath))
                return true;

            var selectGUID = AssetDatabase.AssetPathToGUID(selectPath);

            if (!string.IsNullOrEmpty(selectPath) && !selectDepencies.ContainsKey(assetPath))
            {
                selectDepencies.Add(assetPath, new DependentInfo(assetPath));
            }

            DependentInfo selectDependenceInfo = selectDepencies[assetPath];
            for (int i = allSearchAssets.Length - 1; i >= 0; --i)
            {
                var pathTmp = allSearchAssets[i];
                DepenciesInfo[] listDependence = null;
                if (allDependencies.TryGetValue(pathTmp, out listDependence))
                {
                    for (int j = 0; j < listDependence.Length; ++j)
                    {
                        if (listDependence[j].guid == selectGUID)
                        {
                            selectDependenceInfo.dependencies.Add(pathTmp);
                            break;
                        }
                    }
                }
            }

            if (!selectDepencies.ContainsKey(assetPath))
            {
                Debug.LogError("not find asset=" + assetPath + " in dictionary");
                return false;
            }

            //remove self dependence
            var listDependenciesTmp = selectDependenceInfo.dependencies;
            for (int i = listDependenciesTmp.Count - 1; i >= 0; --i)
            {
                if (listDependenciesTmp[i] == assetPath)
                {
                    listDependenciesTmp.RemoveAt(i);
                }
            }
            return true;
        }

        static private GameObject GetPrefabInstanceParent(GameObject go)
        {
            if (go == null)
            {
                return null;
            }

            if (!IsPrefab(go))
                return null;

            if (go.transform.parent == null)
                return go;

            if (!IsPrefab(go.transform.parent.gameObject))
                return go;

            return GetPrefabInstanceParent(go.transform.parent.gameObject);
        }

        static private bool IsPrefab(GameObject target)
        {
#if UNITY_2018_1_OR_NEWER
            var pType = PrefabUtility.GetPrefabAssetType(target);
            return pType != PrefabAssetType.NotAPrefab;
#else
            var pType = PrefabUtility.GetPrefabType(target);
            return pType == PrefabType.PrefabInstance;
#endif
        }

        // private bool HavePrefabInCurrentSelection(ICollection<shaco.Instance.Editor.TreeView.CustomTreeView<List<string>>.ItemData<List<string>>> datas)
        // {
        //     bool retValue = false;

        //     foreach (var iter in datas)
        //     {
        //         if (iter.path.EndsWith(".prefab"))
        //         {
        //             retValue = true;
        //             break;
        //         }
        //     }
        //     return retValue;
        // }

        /// <summary>
        /// 递归删除空文件
        /// <param name="path">递归开始的路径，从该文件夹开始往上层依次删除空文件夹</param>
        /// <param name="ignorePatterns">判断文件数量的时候需要过滤的文件后缀名，如果文件数量为0则会删除该文件夹</param>
        /// </summary>
        static public bool DeleteEmptyFolder(string path, params string[] ignorePatterns)
        {
            var retValue = false;

            //如果文件夹内容空，则删除它
            var folderPath = string.Empty;

            //本身是文件夹
            if (System.IO.Directory.Exists(path))
            {
                folderPath = path;
            }
            //可能是文件路径
            else
            {
                folderPath = System.IO.Directory.GetParent(path).FullName;
            }

            //获取文件夹下所有文件
            bool isEmptyFolder = IsEmptyFolder(folderPath, ignorePatterns);

            //文件内容为空
            if (isEmptyFolder)
            {
                System.IO.Directory.Delete(folderPath, true);

                //获取上级目录递归确认空文件夹并删除
                folderPath = System.IO.Directory.GetParent(folderPath).FullName;
                DeleteEmptyFolder(folderPath, ignorePatterns);
                retValue = true;
            }
            return retValue;
        }

        /// <summary>
        /// 判断文件夹是否为空
        /// <param name="path">文件夹路径</param>
        /// <param name="ignorePatterns">判断文件数量的时候需要过滤的文件后缀名，如果文件数量为0则会删除该文件夹</param>
        /// </summary>
        static public bool IsEmptyFolder(string path, params string[] ignorePatterns)
        {
            var allFilesPath = System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.TopDirectoryOnly);
            bool retValue = true;
            if (null != allFilesPath && allFilesPath.Length > 0)
            {
                for (int i = allFilesPath.Length - 1; i >= 0; --i)
                {
                    var assetPath = allFilesPath[i];

                    if (null == ignorePatterns || ignorePatterns.Length == 0)
                    {
                        retValue = false;
                        break;
                    }
                    else
                    {
                        bool hasIngorePattern = false;
                        for (int j = ignorePatterns.Length - 1; j >= 0; --j)
                        {
                            if (assetPath.EndsWith(ignorePatterns[j]))
                            {
                                hasIngorePattern = true;
                                break;
                            }
                        }
                        retValue = hasIngorePattern;
                    }

                    if (!retValue)
                        break;
                }
            }
            
            if (retValue)
            {
                var allFoldersPath = System.IO.Directory.GetDirectories(path, "*", System.IO.SearchOption.TopDirectoryOnly);
                if (null != allFoldersPath && allFoldersPath.Length > 0)
                {
                    for (int i = 0; i < allFoldersPath.Length; ++i)
                    {
                        retValue = IsEmptyFolder(allFoldersPath[i]);
                        if (!retValue)
                            break;
                    }
                }
            }
            return retValue;
        }
    }
}