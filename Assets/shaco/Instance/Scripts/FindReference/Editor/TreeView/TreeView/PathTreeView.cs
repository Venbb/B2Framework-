using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

namespace shaco.Instance.Editor.TreeView
{
    public class PathTreeView : CustomTreeView<List<string>>
    {
        public enum DragLocationType
        {
            LeftWindow,
            RightWindow
        }

        //文件数量
        public int fileCount { get { return _fileCount; } }

        //是否允许拖拽非Unity项目中的文件
        public bool allowNonUnityAssetPath = true;

        //是否允许删除文件并移动到垃圾桶
        public bool allowDeleteAndMoveToTrash = true;
        
        //是否允许自动删除空白文件夹
        public bool allowAutoDeleteEmptyDirectory = false;

        //拖拽文件定位回调
        public System.Action<DragLocationType, string[]> callbackDragLocation = null;

        //删除组件回调
        new public System.Func<string, List<string>, bool> callbackWillDelete = null;

        private CustomTreeView<Object> _rightView = new CustomTreeView<Object>();
        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparator = new shaco.Instance.Editor.TreeView.WindowSplitter();

        private Texture2D INTERNAL_ICON_FOLDER = null;
        private Dictionary<string, Texture2D> _lastFileOrFolderIcon = new Dictionary<string, Texture2D>();
        private string _customExtensions = string.Empty;
        private Rect _oldWindowRect;
        private int _fileCount = 0;
        private bool _isUpdateDragEvent = false;

        public PathTreeView()
        {
            INTERNAL_ICON_FOLDER = EditorGUIUtility.FindTexture("Folder Icon");
            _rightView.callbackRowGUIDraw += OnDrawRowItem;

            this.AddContextMenu("Show In Folder", (assetPath, customData) =>
            {
                if (System.IO.File.Exists(assetPath))
                    EditorUtility.RevealInFinder(assetPath);
                else if (System.IO.Directory.Exists(assetPath))
                {
                    EditorUtility.RevealInFinder(assetPath);
                }
                else
                    Debug.LogError("PathTreeView Show In Folder erorr: not found asset path=" + assetPath);
            }, (items) =>
            {
                return items.Any(v => System.IO.File.Exists(v.path) || System.IO.Directory.Exists(v.path));
            });

            base.callbackWillDelete += (assetPath, customData) =>
            {
                bool isDeleted = false;
                if (!allowDeleteAndMoveToTrash || (!System.IO.File.Exists(assetPath) && !System.IO.Directory.Exists(assetPath)))
                {
                    //非本地文件可以直接删除
                    if (null != this.callbackWillDelete)
                        this.callbackWillDelete(assetPath, customData);
                    isDeleted = true;
                }
                else if (EditorUtility.DisplayDialog("Delete selected asset?", assetPath, "Delete", "Cancel"))
                {
                    if (null != this.callbackWillDelete)
                        this.callbackWillDelete(assetPath, customData);
                    AssetDatabase.MoveAssetToTrash(assetPath);
                    isDeleted = true;
                    return true;
                }

                if (isDeleted)
                {
                    RemovePath(assetPath);
                    _rightView.Clear();
                }
                return false;
            };

            this.callbackSingleClickedItem += OnSingleClickedItemCallBack;
            this.callbackDoubleClickedItem += OnDoubleClickedItemCallBack;
        }

        /// <summary>
        /// 设置左边窗口最后一个文件或者文件夹为特殊图标
        /// 注意：该方法需要在AddPath or AddFile前调用，否则不生效
        /// </summary>
        public void SetCustomIcon(string extension, Texture2D texture)
        {
            if (_lastFileOrFolderIcon.ContainsKey(extension))
            {
                Debug.LogError("PathTreeView SetLastFileOrFolderIcon erorr: duplicate key=" + extension);
                return;
            }
            _lastFileOrFolderIcon.Add(extension, texture);
        }

        /// <summary>
        /// 添加路径
        /// </summary>
        public bool AddPath(string path, string internalPath)
        {
            return AddPath(path, new List<string>() { internalPath });
        }

        /// <summary>
        /// 添加路径
        /// </summary>
        public bool AddPath(string path, List<string> internalPaths = null)
        {
            //已经添加过的路径再次添加的话，只新增内部路径
            var id = GetID(path);
            if (id >= 0)
            {
                if (null == internalPaths || internalPaths.Count == 0)
                {
                    Debug.LogError("PathTreeView AddPath error: duplicate call 'AddPath' when internal path is empty, path=" + path);
                    return false;
                }
                
                var customData = GetCustomData(id);
                customData.AddRange(internalPaths);
                return true;
            }

            bool existsFile = System.IO.File.Exists(path);
            bool existsFolder = System.IO.Directory.Exists(path);
            if (!existsFile && !existsFolder)
            {
                //当非文件路径时候直接添加
                var folderPath = System.IO.Path.GetDirectoryName(path);
                base.AddPath(folderPath, null, System.IO.Directory.Exists(folderPath) ? INTERNAL_ICON_FOLDER : null, System.IO.Path.DirectorySeparatorChar);
                base.AddPath(path, internalPaths, GetLastFileOrFolderIcon(path, false), System.IO.Path.DirectorySeparatorChar);
                return false;
            }

            if (existsFile)
            {
                var folderPath = System.IO.Path.GetDirectoryName(path);

                if (null != callbackWillAdd)
                {
                    if (!callbackWillAdd(path, internalPaths))
                        return false;
                }

                this._isProhibitExecuteWillAddCallBack = null != callbackWillAdd;
                base.AddPath(folderPath, null, INTERNAL_ICON_FOLDER, System.IO.Path.DirectorySeparatorChar);
                base.AddPath(path, internalPaths, GetLastFileOrFolderIcon(path, true), System.IO.Path.DirectorySeparatorChar);
                this._isProhibitExecuteWillAddCallBack = false;

                ++_fileCount;
            }
            else if (existsFolder)
            {
                if (null == _lastFileOrFolderIcon)
                {
                    if (null != callbackWillAdd)
                    {
                        if (!callbackWillAdd(path, internalPaths))
                            return false;
                    }

                    this._isProhibitExecuteWillAddCallBack = null != callbackWillAdd;
                    base.AddPath(path, internalPaths, INTERNAL_ICON_FOLDER, System.IO.Path.DirectorySeparatorChar);
                    this._isProhibitExecuteWillAddCallBack = false;
                }
                else
                {
                    if (null != callbackWillAdd)
                    {
                        if (!callbackWillAdd(path, internalPaths))
                            return false;
                    }

                    var folderPath = System.IO.Path.GetDirectoryName(path);

                    this._isProhibitExecuteWillAddCallBack = null != callbackWillAdd;
                    base.AddPath(folderPath, null, INTERNAL_ICON_FOLDER, System.IO.Path.DirectorySeparatorChar);
                    base.AddPath(path, internalPaths, GetLastFileOrFolderIcon(path, false), System.IO.Path.DirectorySeparatorChar);
                    this._isProhibitExecuteWillAddCallBack = false;

                    var allAssets = AssetDatabase.FindAssets("*", new string[] { path });
                    if (null != allAssets)
                        _fileCount += allAssets.Length;
                }
            }
            return true;
        }

        /// <summary>
        /// 删除路径
        /// </summary>
        override public TreeViewItem RemovePath(string path)
        {
            var parentItem = base.RemovePath(path);

            //自动删除空白文件夹
            if (allowAutoDeleteEmptyDirectory && null != parentItem && null != parentItem.parent)
            {
                if (!parentItem.hasChildren)
                {
                    var assetPath = GetItemPath(parentItem.id);
                    parentItem = this.RemovePath(assetPath);
                }
            }
            return parentItem;
        }

        /// <summary>
        /// 清除内容
        /// </summary>
        override public void Clear()
        {
            base.Clear();
            _rightView.Clear();
        }

        /// <summary>
        /// 绘制内容树
        /// </summary>
        override public void DrawTreeView(Rect rect)
        {
            if (_oldWindowRect != rect)
            {
                OnDrawWindowRectChanged(rect);
            }

            if (_dragLineSeparator.isDragSplitter)
            {
                this.Repaint();
            }

            UpdateEvent();

            _dragLineSeparator.BeginLayout(true);
            {
                var rectLeft = _dragLineSeparator.GetSplitWindowRect(0);
                rectLeft.x = 0;
                rectLeft.y = 0;
                base.DrawTreeView(rectLeft);
            }
            _dragLineSeparator.EndLayout();

            _dragLineSeparator.BeginLayout();
            {
                var rectRight = _dragLineSeparator.GetSplitWindowRect(1);
                rectRight.x = 0;
                rectRight.y = 0;
                _rightView.DrawTreeView(rectRight);
            }
            _dragLineSeparator.EndLayout();
        }

        protected override void UnSelection()
        {
            base.UnSelection();
            _rightView.Clear();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (null == selectedIds)
                return;

            OnSingleClickedItemCallBack(null, null);
        }

        /// <summary>
        /// 绘制窗口大小发生变化
        /// </summary>
        private void OnDrawWindowRectChanged(Rect rect)
        {
            if (_oldWindowRect.width == 0 && _oldWindowRect.height == 0)
                _dragLineSeparator.SetSplitWindow(rect, 0.5f, 0.5f);
            else
                _dragLineSeparator.SetSplitWindow(rect);
            _oldWindowRect = rect;
        }

        private void UpdateSelectionDetail(string assetPath, string[] internalPaths)
        {
            if (null != internalPaths && internalPaths.Length > 0)
            {
                foreach (var path in internalPaths)
                {
                    var iconTmp = AssetDatabase.GetCachedIcon(path);
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (!_rightView.AddFile(path, mainAsset, iconTmp as Texture2D))
                        break;
                }
                return;
            }

            if (System.IO.Directory.Exists(assetPath))
            {
                var childrenAssetPath = GetChildrenPath(assetPath, false);
                foreach (var path in childrenAssetPath)
                {
                    var iconTmp = AssetDatabase.GetCachedIcon(path);
                    var mainAsset = AssetDatabase.LoadMainAssetAtPath(path);
                    if (!_rightView.AddFile(path, mainAsset, iconTmp as Texture2D))
                        break;
                }
                _rightView.Reload();
            }
            else if (System.IO.File.Exists(assetPath))
            {
                var iconTmp = AssetDatabase.GetCachedIcon(assetPath);
                var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
                if (_rightView.AddFile(assetPath, mainAsset, iconTmp as Texture2D))
                    _rightView.Reload();
            }
            //忽略丢失路径文件
            // else
            // {
            //     Debug.LogError("PathTreeView SingleClickedItem error: not found path=" + assetPath);
            //     return;
            // }
        }

        private void OnSingleClickedItemCallBack(string assetPath, List<string> internalPaths)
        {
            _rightView.Clear();

            var selection = GetSelection();
            foreach (var iter in selection)
            {
                var pathTmp = GetItemPath(iter);
                var customData = GetCustomData(iter);
                UpdateSelectionDetail(pathTmp, null != customData ? customData.ToArray() : null);
            }
        }

        private void OnDoubleClickedItemCallBack(string path, object customData)
        {
            var asset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            if (null == asset)
                return;

            EditorGUIUtility.PingObject(asset);

            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var typeProjectBrower = assembly.GetType("UnityEditor.ProjectBrowser");
            EditorWindow.FocusWindowIfItsOpen(typeProjectBrower);
        }

        private void OnDrawRowItem(TreeViewItem item, Rect rowRect, Object customData, int row)
        {
            EditorGUI.BeginDisabledGroup(true);
            {
                if (null == customData)
                    EditorGUI.TextField(rowRect, item.displayName);
                else
                    EditorGUI.ObjectField(rowRect, customData, customData.GetType(), true);
            }
            EditorGUI.EndDisabledGroup();
        }

        private void UpdateEvent()
        {
            var currentEvent = Event.current;
            if (null == currentEvent)
                return;

            //鼠标点击到了空白区域，取消当前选择
            switch (currentEvent.type)
            {
                case EventType.KeyDown:
                    {
                        if (currentEvent.keyCode == KeyCode.Delete)
                        {
                            var currentSelection = GetSelection();
                            if (null != currentSelection && currentSelection.Count > 0)
                            {
                                var appendString = new System.Text.StringBuilder();
                                bool isDeleted = false;
                                foreach (var iter in currentSelection)
                                {
                                    var assetPath = GetItemPath(iter);
                                    if (!allowDeleteAndMoveToTrash || (!System.IO.File.Exists(assetPath) && !System.IO.Directory.Exists(assetPath)))
                                    {
                                        //非本地文件可以直接删除
                                        if (null != this.callbackWillDelete)
                                            this.callbackWillDelete(assetPath, GetCustomData(iter));
                                        RemovePath(assetPath);
                                        isDeleted = true;
                                        continue;
                                    }

                                    appendString.Append(assetPath);
                                    appendString.Append('\n');
                                }

                                if (appendString.Length > 0)
                                {
                                    var titleDisplay = currentSelection.Count > 1 ? string.Format("Delete selected {0} assets", currentSelection.Count) : "Delete selected asset";
                                    if (allowDeleteAndMoveToTrash && EditorUtility.DisplayDialog(titleDisplay, appendString.ToString(), "Delete", "Cancel"))
                                    {
                                        foreach (var iter in currentSelection)
                                        {
                                            var assetPath = GetItemPath(iter);

                                            if (null != this.callbackWillDelete)
                                                this.callbackWillDelete(assetPath, GetCustomData(iter));

                                            RemovePath(assetPath);
                                            AssetDatabase.MoveAssetToTrash(assetPath);
                                            isDeleted = true;
                                        }
                                    }
                                }

                                if (isDeleted)
                                    _rightView.Clear();
                            }
                        }
                        break;
                    }
                case EventType.DragUpdated:
                    {
                        _isUpdateDragEvent = true;
                        break;
                    }
                case EventType.DragExited:
                    {
                        //因为DragExited会执行两次，所以通过DragUpdated限制它
                        if (!_isUpdateDragEvent || null == callbackDragLocation)
                            break;
                        _isUpdateDragEvent = false;

                        var mousePosition = currentEvent.mousePosition;
                        var leftRect = _dragLineSeparator.GetSplitWindowRect(0);
                        var rightRect = _dragLineSeparator.GetSplitWindowRect(1);
                        rightRect.x = leftRect.xMax;

                        //拖拽到左窗口
                        if (leftRect.Contains(mousePosition))
                        {
                            if (!allowNonUnityAssetPath)
                            {
                                var applicationPath = Application.dataPath;
                                foreach (var iter in DragAndDrop.paths)
                                {
                                    if (iter.StartsWith(applicationPath))
                                        callbackDragLocation(DragLocationType.LeftWindow, DragAndDrop.paths);
                                }
                            }
                            else
                                callbackDragLocation(DragLocationType.LeftWindow, DragAndDrop.paths);
                        }

                        //拖拽到右窗口
                        if (rightRect.Contains(mousePosition))
                        {
                            if (!allowNonUnityAssetPath)
                            {
                                var applicationPath = Application.dataPath;
                                foreach (var iter in DragAndDrop.paths)
                                {
                                    if (iter.StartsWith(applicationPath))
                                        callbackDragLocation(DragLocationType.RightWindow, DragAndDrop.paths);
                                }
                            }
                            else
                                callbackDragLocation(DragLocationType.RightWindow, DragAndDrop.paths);
                        }
                        break;
                    }
            }
        }

        private Texture2D GetLastFileOrFolderIcon(string path, bool useCachedIcon)
        {
            var extension = System.IO.Path.GetExtension(path);
            Texture2D findValue = null;
            if (_lastFileOrFolderIcon.TryGetValue(extension, out findValue))
            {
                return findValue;
            }
            else
            {
                return useCachedIcon ? AssetDatabase.GetCachedIcon(path) as Texture2D : null;
            }
        }
    }
}