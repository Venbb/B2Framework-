using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;

namespace shaco.Instance.Editor.TreeView
{
    public class CustomTreeView<T> : UnityEditor.IMGUI.Controls.TreeView
    {
        public class ItemData<TSub>
        {
            public string path { get { return _path; } }
            public TSub customData { get { return _customData; } }

            private string _path = null;
            private TSub _customData = default(TSub);

            public ItemData(string path, TSub customData)
            {
                this._path = path;
                this._customData = customData;
            }
        }

        private class TreeViewItemTemplate<TSub> : TreeViewItem
        {
            public ItemData<TSub> item = null;

            private Dictionary<string, TreeViewItemTemplate<TSub>> _childrenDic = new Dictionary<string, TreeViewItemTemplate<TSub>>();

            public TreeViewItemTemplate(string path, TSub customData)
            {
                item = new ItemData<TSub>(path, customData);
            }

            public bool TryGetValue(string displayName, out TreeViewItemTemplate<TSub> item)
            {
                return _childrenDic.TryGetValue(displayName, out item);
            }

            public void AddChild(string displayName, TreeViewItemTemplate<TSub> item)
            {
                if (!this._childrenDic.ContainsKey(displayName))
                    this._childrenDic.Add(displayName, item);
                this.AddChild(item);
            }

            public void RemoveChild(TreeViewItem item)
            {
                this.children.Remove(item);
                this._childrenDic.Remove(item.displayName);
            }

            public void Clear()
            {
                if (null != this.children)
                {
                    this.children.Clear();
                }
                this._childrenDic.Clear();
            }

            public void SetDisplayName(string newName)
            {
                this.displayName = newName;
                if (null != item)
                {
                    var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(item.path), newName);
                    item = new ItemData<TSub>(newPath, item.customData);
                }
            }
        }

        private class ContextMenuInfo
        {
            public System.Action<string, T> callbackExecute = null;
            public System.Func<ICollection<ItemData<T>>, bool> callbackValid = null;
        }

        private class TreeViewHeader : MultiColumnHeader
        {
            public System.Action<MultiColumnHeaderState.Column, int> callbackClicked = null;

            private readonly GUIStyle GUI_STYLE_TEXT = new GUIStyle(EditorStyles.label);

            public TreeViewHeader(MultiColumnHeaderState state) : base(state)
            {
                GUI_STYLE_TEXT.alignment = TextAnchor.MiddleCenter;
            }

            protected override void ColumnHeaderClicked(MultiColumnHeaderState.Column column, int columnIndex)
            {
                if (null != callbackClicked)
                {
                    callbackClicked(column, columnIndex);
                }
            }

            protected override void ColumnHeaderGUI(MultiColumnHeaderState.Column column, Rect headerRect, int columnIndex)
            {
                if (GUI.Button(headerRect, column.headerContent, GUI_STYLE_TEXT))
                {
                    if (null != callbackClicked)
                    {
                        callbackClicked(column, columnIndex);
                    }
                }
            }
        }

        //选择对象发生变化事件
        public System.Action<IEnumerable<KeyValuePair<string, T>>> callbackSelectionChanged = null;

        //单点事件
        public System.Action<string, T> callbackSingleClickedItem = null;

        //点击到了空白区域的取消事件
        public System.Action callbackClickedCancel = null;

        //双点事件
        public System.Action<string, T> callbackDoubleClickedItem = null;

        //绘制一行数据回调
        public System.Action<TreeViewItem, Rect, T, int> callbackRowGUIDraw = null;

        //自定义绘制标题回调
        public System.Func<float> callbackTopBarGUI = null;

        //添加组件回调
        public System.Func<string, T, bool> callbackWillAdd = null;

        //删除组件回调
        public System.Func<string, T, bool> callbackWillDelete = null;

        //判断是否可以重命名回调，如果没有设置该回调则不支持重命名
        public System.Func<string, T, bool> callbackCanRename = null;

        //重命名成功回调
        public System.Action<string, string, T> callbackRenameSuccess = null;

        //是否拦截添加组件回调事件
        protected bool _isProhibitExecuteWillAddCallBack = false;

        //是否有子节点
        public bool hasChildren { get { return null != _rootItem && _rootItem.hasChildren; } }

        private SearchField _searchField = new SearchField();
        private TreeViewItemTemplate<T> _rootItem = new TreeViewItemTemplate<T>(null, default(T)) { depth = -1, id = -1 };
        private Dictionary<int, TreeViewItemTemplate<T>> _idToItem = new Dictionary<int, TreeViewItemTemplate<T>>();
        private Dictionary<string, int> _itemPathToID = new Dictionary<string, int>();
        private Dictionary<string, System.Action> _headerInfo = new Dictionary<string, System.Action>();
        private Dictionary<string, ContextMenuInfo> _contextInfo = new Dictionary<string, ContextMenuInfo>();

        private bool _isDirtyReBuild = true;
        private int _uuidOrder = 0;
        private int _itemCount = 0;

        public CustomTreeView() : base(new TreeViewState())
        {
            _searchField.downOrUpArrowKeyPressed += this.SetFocusAndEnsureSelectedItem;
        }

        /// <summary>
        /// 展开所有文件夹
        /// </summary>
        new public void ExpandAll()
        {
            if (null != _rootItem && _rootItem.hasChildren)
            {
                CheckInit();
                base.ExpandAll();
            }
        }

        /// <summary>
        /// 收拢所有文件夹
        /// </summary>
        new public void CollapseAll()
        {
            if (null != _rootItem && _rootItem.hasChildren)
            {
                CheckInit();
                base.CollapseAll();
            }
        }

        /// <summary>
        /// 设置头
        /// </summary>
        public void AddHeader(string text, System.Action callback)
        {
            if (_headerInfo.ContainsKey(text))
            {
                if (null == callback)
                    _headerInfo.Remove(text);
                else
                    _headerInfo[text] = callback;
            }
            else
            {
                if (null == callback)
                    Debug.LogError("CustomTreeView AddHeader erorr: not found header when need remove, text=" + text);
                else
                    _headerInfo.Add(text, callback);
            }
            _isDirtyReBuild = true;
        }

        /// <summary>
        /// 设置右键菜单
        /// </summary>
        public void AddContextMenu(string text, System.Action<string, T> callbackExecute, System.Func<ICollection<ItemData<T>>, bool> callbackValid = null)
        {
            var newInfo = new ContextMenuInfo();
            newInfo.callbackExecute = callbackExecute;
            newInfo.callbackValid = callbackValid;

            if (_contextInfo.ContainsKey(text))
                _contextInfo[text] = newInfo;
            else
                _contextInfo.Add(text, newInfo);
        }

        /// <summary>
        /// 设置内容路径
        /// </summary>
        public bool AddPath(string path, T customData = default(T), Texture2D icon = null, char splitFlag = '0')
        {
            if (!_isProhibitExecuteWillAddCallBack && null != callbackWillAdd && !callbackWillAdd(path, customData))
                return false;

            //如果是windows环境因为Unity内部的路径分隔符为linux的，所以需要统一下
            if ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) && (path.IndexOf('/') >= 0))
            {
                path = path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            }

            if (splitFlag == '0')
                splitFlag = System.IO.Path.DirectorySeparatorChar;

            var splitValues = path.Split(splitFlag);
            if (null == splitValues || 0 == splitValues.Length)
            {
                Debug.LogError("CustomTreeView AddPath error: invalid path=" + path);
                return false;
            }

            TreeViewItemTemplate<T> findParent = null;
            var prevParentItem = _rootItem;
            var parentPath = new System.Text.StringBuilder();

            for (int i = 0; i < splitValues.Length; ++i)
            {
                var currentDepth = i;
                var nameTmp = splitValues[i];

                if (parentPath.Length == 0)
                    parentPath.Append(nameTmp);
                else
                    parentPath.Append(splitFlag).Append(nameTmp);

                if (!prevParentItem.TryGetValue(nameTmp, out findParent))
                {
                    var customDataCheck = i == splitValues.Length - 1 ? customData : default(T);
                    var currentPath = parentPath.ToString();
                    findParent = new TreeViewItemTemplate<T>(currentPath, customDataCheck)
                    {
                        depth = currentDepth,
                        icon = icon,
                        displayName = nameTmp,
                        id = _uuidOrder
                    };

                    prevParentItem.AddChild(nameTmp, findParent);
                    _idToItem.Add(_uuidOrder, findParent);

                    if (!_itemPathToID.ContainsKey(currentPath))
                        _itemPathToID.Add(currentPath, _uuidOrder);
                    ++_uuidOrder;
                    ++_itemCount;
                }
                prevParentItem = findParent;
            }
            _isDirtyReBuild = true;
            return true;
        }

        /// <summary>
        /// 添加文件
        /// </summary>
        public bool AddFile(string path, T customData = default(T), Texture2D icon = null)
        {
            if (!_isProhibitExecuteWillAddCallBack && null != callbackWillAdd && !callbackWillAdd(path, customData))
                return false;

            var fileName = System.IO.Path.GetFileName(path);
            var newItem = new TreeViewItemTemplate<T>(path, customData)
            {
                depth = 0,
                icon = icon,
                displayName = fileName,
                id = _uuidOrder
            };

            //暂不支持重名文件的查找，实际上好像也没这个必要
            _rootItem.AddChild(fileName, newItem);
            _idToItem.Add(_uuidOrder, newItem);

            if (!_itemPathToID.ContainsKey(fileName))
                _itemPathToID.Add(fileName, _uuidOrder);
            ++_uuidOrder;
            ++_itemCount;
            _isDirtyReBuild = true;
            return true;
        }

        /// <summary>
        /// 获取子节点
        /// </summary>
        public ICollection<string> GetChildren(string assetPath)
        {
            List<string> retValue = null;
            var findID = 0;
            if (_itemPathToID.TryGetValue(assetPath, out findID))
            {
                Debug.LogError("CustomTreeView GetChildren erorr: not found asset path=" + assetPath);
                return retValue;
            }

            TreeViewItemTemplate<T> findItem = null;
            if (_idToItem.TryGetValue(findID, out findItem))
            {
                Debug.LogError("CustomTreeView GetChildren erorr: not found item, asset path=" + assetPath + " id=" + findID);
                return retValue;
            }

            for (int i = findItem.children.Count - 1; i >= 0; --i)
            {
                var childTmp = findItem.children[i] as TreeViewItemTemplate<T>;
                retValue.Add(childTmp.item.path);
            }
            return retValue;
        }

        /// <summary>
        /// 递归获取所有子节点
        /// </summary>
        public ICollection<string> GetChildrenPath(string assetPath, bool recursive)
        {
            List<string> retValue = null;
            var findID = 0;
            if (!_itemPathToID.TryGetValue(assetPath, out findID))
            {
                Debug.LogError("CustomTreeView GetChildrenRecursive erorr: not found asset path=" + assetPath);
                return retValue;
            }

            TreeViewItemTemplate<T> itemFind = null;
            if (!_idToItem.TryGetValue(findID, out itemFind))
            {
                Debug.LogError("CustomTreeView GetChildrenRecursive error: not found id=" + findID);
                return retValue;
            }

            if (!itemFind.hasChildren)
                return retValue;

            if (null == retValue)
                retValue = new List<string>();
            retValue.AddRange(itemFind.children.Select(v => ((TreeViewItemTemplate<T>)v).item.path));

            if (recursive)
            {
                foreach (var iter in itemFind.children)
                {
                    var childAssetPath = ((TreeViewItemTemplate<T>)iter).item.path;
                    var childrenAssetPath = GetChildrenPath(childAssetPath, recursive);
                    if (null != childrenAssetPath)
                        retValue.AddRange(childrenAssetPath);
                }
            }

            return retValue;
        }

        /// <summary>
        /// 刷新当前选中
        /// </summary>
        public void RefreshSelection()
        {
            SelectionChanged(GetSelection());
        }

        /// <summary>
        /// 修改节点名字
        /// </summary>
        public bool ReName(string originalPath, string newPath)
        {
            var findID = 0;
            if (!_itemPathToID.TryGetValue(originalPath, out findID))
            {
                Debug.LogError("CustomTreeView ReName erorr: not found asset path=" + originalPath);
                return false;
            }

            //二者文件夹路径必须一致
            var originalDirectoryPath = System.IO.Path.GetDirectoryName(originalPath);
            var newDirectoryPath = System.IO.Path.GetDirectoryName(newPath);

            if (originalDirectoryPath != newDirectoryPath)
            {
                Debug.LogError("CustomTreeView ReName error: The original folder path and the new folder path must be the same");
                return false;
            }

            TreeViewItemTemplate<T> itemFind = null;
            if (!_idToItem.TryGetValue(findID, out itemFind))
            {
                Debug.LogError("CustomTreeView ReName error: not found id=" + findID);
                return false;
            }

            var newName = System.IO.Path.GetFileName(newPath);

            _itemPathToID.Remove(originalPath);
            _itemPathToID.Add(newPath, itemFind.id);
            itemFind.SetDisplayName(newName);
            return true;
        }

        /// <summary>
        /// 重新加载
        /// </summary>
        new public void Reload()
        {
            if (null != _rootItem && _rootItem.hasChildren)
            {
                base.Reload();
            }
        }

        /// <summary>
        /// 删除路径
        /// </summary>
        virtual public TreeViewItem RemovePath(string path)
        {
            TreeViewItemTemplate<T> parentItem = null;

            int itemID = -1;
            if (!_itemPathToID.TryGetValue(path, out itemID))
            {
                Debug.LogError("CustomTreeView RemovePath erorr: not found item id by path=" + path);
                return parentItem;
            }

            TreeViewItemTemplate<T> itemFind = null;
            if (!_idToItem.TryGetValue(itemID, out itemFind))
            {
                Debug.LogError("CustomTreeView RemovePath error: not found id=" + itemID);
                return parentItem;
            }

            parentItem = itemFind.parent as TreeViewItemTemplate<T>;
            parentItem.RemoveChild(itemFind);
            _itemPathToID.Remove(path);
            _idToItem.Remove(itemID);
            --_itemCount;
            _isDirtyReBuild = true;

            return parentItem;
        }

        /// <summary>
        /// 清除内容
        /// </summary>
        virtual public void Clear()
        {
            // callbackSingleClickedItem = null;
            // callbackDoubleClickedItem = null;
            // callbackRowGUIDraw = null;
            // callbackHeaderDraw = null;
            // callbackWillAdd = null;
            // callbackWillDelete = null;

            _uuidOrder = 0;
            _itemCount = 0;
            _rootItem.Clear();
            _idToItem.Clear();
            _itemPathToID.Clear();
            _isDirtyReBuild = true;
        }

        /// <summary>
        /// 绘制内容树
        /// </summary>
        virtual public void DrawTreeView(Rect rect)
        {
            CheckInit();
            UpdateEvent();

            float headerDrawHeight = 0;
            if (null == callbackTopBarGUI)
            {
                headerDrawHeight = 18;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label(string.Format("Count:{0}", this._itemCount));
                    this.searchString = _searchField.OnToolbarGUI(this.searchString);
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        headerDrawHeight = callbackTopBarGUI();
                    }
                    GUILayout.EndVertical();
                    this.searchString = _searchField.OnToolbarGUI(this.searchString);
                }
                GUILayout.EndHorizontal();
            }

            rect.height -= headerDrawHeight;
            rect.y += headerDrawHeight;

            if (_rootItem.hasChildren)
            {
                this.OnGUI(rect);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        virtual protected void CheckInit()
        {
            if (!_isDirtyReBuild)
            {
                return;
            }

            if (_rootItem.hasChildren)
            {
                _isDirtyReBuild = false;
                this.Reload();
            }
        }

        /// <summary>
        /// 取消当前选中
        /// </summary>
        virtual protected void UnSelection()
        {
            this.SetSelection(new List<int>());
        }

        protected string GetItemPath(int id)
        {
            TreeViewItemTemplate<T> retValue = null;
            if (!_idToItem.TryGetValue(id, out retValue))
            {
                Debug.LogError("CustomTreeView GetItemPath error: not found id=" + id);
                return string.Empty;
            }
            return retValue.item.path;
        }

        protected T GetCustomData(int id)
        {
            TreeViewItemTemplate<T> retValue = null;
            if (!_idToItem.TryGetValue(id, out retValue))
            {
                Debug.LogError("CustomTreeView GetCustomData error: not found id=" + id);
                return default(T);
            }
            return retValue.item.customData;
        }

        protected int GetID(string path)
        {
            int retValue = -1;
            if (!_itemPathToID.TryGetValue(path, out retValue))
            {
                retValue = -1;
                return retValue;
            }
            return retValue;
        }

        protected bool IsInPath(string path)
        {
            return _itemPathToID.ContainsKey(path);
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);

            if (null == selectedIds || null == callbackSelectionChanged)
                return;

            var tmpResult = new List<KeyValuePair<string, T>>();
            for (int i = 0; i < selectedIds.Count; ++i)
            {
                var id = selectedIds[i];
                var assetPath = GetItemPath(id);
                var customData = GetCustomData(id);
                tmpResult.Add(new KeyValuePair<string, T>(assetPath, null != customData ? customData : default(T)));
            }

            callbackSelectionChanged(tmpResult);
        }

        protected override TreeViewItem BuildRoot()
        {
            if (null != _headerInfo && _headerInfo.Count > 0)
            {
                var colums = new List<MultiColumnHeaderState.Column>();
                foreach (var iter in _headerInfo)
                {
                    var newColum = new MultiColumnHeaderState.Column();
                    colums.Add(newColum);
                    newColum.headerContent = new GUIContent(iter.Key);
                }
                var treeViewHeader = new TreeViewHeader(new MultiColumnHeaderState(colums.ToArray()));
                this.multiColumnHeader = treeViewHeader;
                treeViewHeader.callbackClicked = OnHeaderClicked;
                treeViewHeader.ResizeToFit();
            }
            return _rootItem;
        }

#if UNITY_2018_1_OR_NEWER
        protected override void SingleClickedItem(int id)
        {
            if (null != callbackSingleClickedItem)
            {
                var item = _idToItem[id];
                callbackSingleClickedItem(item.item.path, item.item.customData);
            }
        }
#else
        protected void CheckSingleSelectionChanged()
        {
            var selection = GetSelection();
            if (null == selection || 1 != selection.Count)
                return;

            if (null != callbackSingleClickedItem)
            {
                var item = _idToItem[selection[0]];
                callbackSingleClickedItem(selection[0], item.item.customData);
            }
        }
#endif

        protected override void DoubleClickedItem(int id)
        {
            if (null != callbackDoubleClickedItem)
            {
                var item = _idToItem[id];
                callbackDoubleClickedItem(item.item.path, item.item.customData);
            }
        }

        protected override void ContextClicked()
        {
            if (null == _contextInfo || 0 == _contextInfo.Count)
                return;

            var items = new List<TreeViewItemTemplate<T>>();
            foreach (var id in GetSelection())
            {
                var item = _idToItem[id];
                items.Add(item);
            }
            ShowContextMenu(items);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            if (null == callbackRowGUIDraw)
                base.RowGUI(args);
            else
            {
                var itemTemplate = args.item as TreeViewItemTemplate<T>;
                callbackRowGUIDraw(itemTemplate, args.rowRect, itemTemplate.item.customData, args.row);
            }
        }

        protected override bool CanRename(TreeViewItem item)
        {
            if (null == callbackCanRename)
                return false;

            var assetPath = GetItemPath(item.id);
            var customData = GetCustomData(item.id);
            return callbackCanRename(assetPath, customData);
        }

        protected override void RenameEnded(RenameEndedArgs args)
        {
            if (args.acceptedRename && !string.IsNullOrEmpty(args.newName) && args.originalName != args.newName)
            {
                if (null != callbackRenameSuccess)
                {
                    var customData = GetCustomData(args.itemID);
                    var originalItem = _idToItem[args.itemID] as TreeViewItemTemplate<T>;
                    var originalPath = originalItem.item.path;
                    var newPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(originalPath), args.newName);

                    ReName(originalPath, newPath);

                    callbackRenameSuccess(originalPath, newPath, customData);
                }
            }
        }

        protected void OnHeaderClicked(MultiColumnHeaderState.Column column, int columnIndex)
        {
            var callback = _headerInfo[column.headerContent.text];
            if (null != callback)
                callback();
        }

        private void ShowContextMenu(List<TreeViewItemTemplate<T>> items)
        {
            if (null == _contextInfo || 0 == _contextInfo.Count)
                return;

            GenericMenu contextMenu = new GenericMenu();
            contextMenu.AddItem(new GUIContent("Delete"), false, (object context) =>
            {
                if (null != callbackWillDelete)
                {
                    for (int i = 0; i < items.Count; ++i)
                    {
                        var itemTmp = items[i];
                        if (callbackWillDelete(itemTmp.item.path, itemTmp.item.customData))
                        {
                            var parentTmp = items[i].parent as TreeViewItemTemplate<T>;
                            parentTmp.RemoveChild(itemTmp);
                            --_itemCount;
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < items.Count; ++i)
                    {
                        var itemTmp = items[i];
                        var parentTmp = items[i].parent as TreeViewItemTemplate<T>;
                        parentTmp.RemoveChild(itemTmp);
                        --_itemCount;
                    }
                }
                this.Reload();
            }, null);

            var itemDatas = new List<ItemData<T>>();
            foreach (var iter in items)
                itemDatas.Add(iter.item);

            foreach (var iter in _contextInfo)
            {
                bool isInValid = null != iter.Value.callbackValid && !iter.Value.callbackValid(itemDatas);

                if (isInValid)
                {
                    contextMenu.AddDisabledItem(new GUIContent(iter.Key));
                }
                else
                {
                    contextMenu.AddItem(new GUIContent(iter.Key), false, (object context) =>
                    {
                        for (int i = 0; i < items.Count; ++i)
                        {
                            var itemTmp = items[i];
                            iter.Value.callbackExecute(itemTmp.item.path, itemTmp.item.customData);
                        }
                    }, null);
                }
            }
            contextMenu.ShowAsContext();
        }

        private void UpdateEvent()
        {
            var currentEvent = Event.current;
            if (null == currentEvent)
                return;

            //鼠标点击到了空白区域，取消当前选择
            switch (currentEvent.type)
            {
                case EventType.MouseUp:
                    {
                        if (_rootItem.hasChildren)
                        {
                            var rect = this.treeViewRect;
                            rect.height = this.totalHeight;
                            if (!rect.Contains(currentEvent.mousePosition))
                            {
                                UnSelection();

                                if (null != callbackClickedCancel)
                                    callbackClickedCancel();
                            }
#if !UNITY_2018_1_OR_NEWER
                            else
                            {
                                CheckSingleSelectionChanged();
                            }
#endif
                        }
                        break;
                    }
                case EventType.KeyUp:
                    {
                        //当在搜索过程中已经选中了对象，按下Esc键的时候定位到文件位置
                        if (currentEvent.keyCode == KeyCode.Escape && !string.IsNullOrEmpty(this.searchString))
                        {
                            var currentSelection = this.GetSelection();
                            if (null != currentSelection && currentSelection.Count > 0)
                            {
                                this.searchString = string.Empty;
                                this.SetSelection(currentSelection, TreeViewSelectionOptions.RevealAndFrame);
                                this.SetFocus();
                                currentEvent.Use();
                            }
                        }
                        if (null != EditorWindow.focusedWindow)
                            EditorWindow.focusedWindow.Repaint();
                        break;
                    }
                case EventType.KeyDown:
                    {
                        if (null != EditorWindow.focusedWindow)
                            EditorWindow.focusedWindow.Repaint();
                        break;
                    }
            }
        }
    }
}