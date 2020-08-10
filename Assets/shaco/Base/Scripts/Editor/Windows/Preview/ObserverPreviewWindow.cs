using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ObserverPreviewWindow : EditorWindow
    {
        private class SelectInfo
        {
            public bool isShow = true;
            public Dictionary<shaco.Base.ISubjectBase, shaco.Base.SubjectLocation> subjectsLocation = null;
        }

        /// <summary>
        /// 窗口对象
        /// </summary>
        private ObserverPreviewWindow _currentWindow = null;

        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparator = new shaco.Instance.Editor.TreeView.WindowSplitter();
        private shaco.Instance.Editor.TreeView.CustomTreeView<shaco.Base.ISubjectBase> _leftTreeViewObservers = null;
        private Dictionary<object, SelectInfo> _currentSelectSubject = new Dictionary<object, SelectInfo>();
        private Dictionary<shaco.Base.ISubjectBase, string> _currentSelectSubjectParamName = new Dictionary<shaco.Base.ISubjectBase, string>();
        private Vector2 _scrollViewPosition = Vector2.zero;
        private string _searchParamName = string.Empty;
        private string _searchParamNameLower = string.Empty;

        [MenuItem("shaco/Viewer/ObserverPreview " + ToolsGlobalDefine.MenuPriority.ViewerShortcutKeys.OBSERVER, false, (int)ToolsGlobalDefine.MenuPriority.Viewer.OBSERVER)]
        static void Open()
        {
            EditorHelper.GetWindow<ObserverPreviewWindow>(null, true, "ObserverPreview");
        }

        void OnEnable()
        {
            _currentWindow = EditorHelper.GetWindow<ObserverPreviewWindow>(this, true, "ObserverPreview");
            _currentWindow.Init();
        }

        void Init()
        {
            _dragLineSeparator.SetSplitWindow(this, 0.3f, 0.7f);
            _leftTreeViewObservers = new shaco.Instance.Editor.TreeView.CustomTreeView<shaco.Base.ISubjectBase>();
            _leftTreeViewObservers.callbackSelectionChanged += (datas) =>
            {
                _currentSelectSubject.Clear();
                _currentSelectSubjectParamName.Clear();

                foreach (var iter in datas)
                {
                    var bindTarget = iter.Value.GetBindTarget();
                    _currentSelectSubject.Add(bindTarget, new SelectInfo() { subjectsLocation = shaco.GameHelper.observer.GetSubjectLocation(bindTarget) });
                }

                if (null != _currentSelectSubject && _currentSelectSubject.Count > 0)
                {
                    foreach (var iter in _currentSelectSubject)
                    {
                        foreach (var iter2 in iter.Value.subjectsLocation)
                            _currentSelectSubjectParamName.Add(iter2.Key, GetSubectParamName(iter2.Key));
                    }
                }
            };

            _leftTreeViewObservers.callbackDoubleClickedItem += (path, data) =>
            {
                PingIfComponent(data);
            };

            _leftTreeViewObservers.AddContextMenu("Ping", (path, data) =>
            {
                PingIfComponent(data);
            }, (data) =>
            {
                foreach (var iter in data)
                {
                    if (iter.customData.GetBindTarget() is UnityEngine.Component)
                        return true;
                }
                return false;
            });

            _leftTreeViewObservers.callbackClickedCancel += CancelSelect;

            RefreshSubjects();

            shaco.GameHelper.observer.callbackAddObserver.AddCallBack(this, OnObserverAddCallBack);
            shaco.GameHelper.observer.callbackRemoveObserver.AddCallBack(this, OnObserverRemoveCallBack);
        }

        private void RefreshSubjects()
        {
            _leftTreeViewObservers.Clear();
            var allSubject = shaco.GameHelper.observer.GetSubjects();
            foreach (var iter in allSubject)
            {
                OnObserverAddCallBack(iter.GetBindTarget(), iter);
            }
        }

        void OnGUI()
        {
            this.Repaint();

            _dragLineSeparator.BeginLayout(true);
            {
                var leftRect = _dragLineSeparator.GetSplitWindowRect(0);
                DrawLeftWindow(leftRect);
            }
            _dragLineSeparator.EndLayout();

            _dragLineSeparator.BeginLayout();
            {
                DrawRightWindow();
            }
            _dragLineSeparator.EndLayout();
        }

        private void DrawLeftWindow(Rect rect)
        {
            _leftTreeViewObservers.DrawTreeView(rect);
        }

        private void DrawRightWindow()
        {
            if (null == _currentSelectSubject || _currentSelectSubject.Count == 0)
                return;

            var newSearchParamName = GUILayoutHelper.SearchField(_searchParamName);
            if (_searchParamName != newSearchParamName)
            {
                //删除搜索内容时候需要刷新当前选中
                if (newSearchParamName.Length < _searchParamName.Length)
                {
                    foreach (var iter in _currentSelectSubject)
                        iter.Value.isShow = true;
                }

                _searchParamName = newSearchParamName;
                _searchParamNameLower = newSearchParamName.ToLower();
            }

            _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
            {
                int index = 0;
                int offsetCount = _currentSelectSubject.Count;
                foreach (var iter in _currentSelectSubject)
                {
                    if (!iter.Value.isShow)
                    {
                        --offsetCount;
                        continue;
                    }
                    else
                    {
                        if (index++ > 0 && index != offsetCount - 1)
                            GUILayoutHelper.DrawSeparatorLine();
                    }

                    var bindTarget = iter.Key;
                    var componentTmp = bindTarget as UnityEngine.Component;

                    if (null != componentTmp)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            EditorGUILayout.ObjectField("Target", componentTmp, componentTmp.GetType(), true);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Target", bindTarget.ToString());
                    }

                    bool hasShowSubject = false;
                    foreach (var iter2 in iter.Value.subjectsLocation)
                    {
                        hasShowSubject |= DrawObservers(iter2.Key, iter2.Value.observserLocations);
                    }
                    iter.Value.isShow = hasShowSubject;
                }
            }
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 绘观测者的堆栈信息
        /// <param name="subject">数据主体</param>
        /// <param name="observerLocations">数据主体绑定的所有观测者堆栈信息</param>
        /// </summary>
        private bool DrawObservers(shaco.Base.ISubjectBase subject, Dictionary<shaco.Base.IObserverBase, shaco.Base.ObserverLocation> observerLocations)
        {
            bool retValue = false;
            foreach (var iter in observerLocations)
            {
                retValue |= DrawObserver(subject, iter.Value);
            }
            return retValue;
        }

        /// <summary>
        /// 绘制观测者的堆栈信息
        /// </summary>
        private bool DrawObserver(shaco.Base.ISubjectBase subject, shaco.Base.ObserverLocation observerLocation)
        {
            string findParamName = null;
            bool findResult = _currentSelectSubjectParamName.TryGetValue(subject, out findParamName);
            if (!string.IsNullOrEmpty(_searchParamNameLower) && findResult && !findParamName.Contains(_searchParamNameLower.ToLower()))
            {
                return false;
            }

            GUILayout.BeginHorizontal("box");
            {
                if (findResult)
                {
                    GUILayout.Label(findParamName, GUILayout.ExpandWidth(false));
                }

                //draw location 'Init' button
                DrawStackLocationButton("Init", observerLocation.stackLocationObserverInit, observerLocation.callbackInitDelegate);

                //draw location 'ValueChange' button
                DrawStackLocationButton("Change", observerLocation.stackLocationValueChange, observerLocation.callbackUpdateDelegate);
            }
            GUILayout.EndHorizontal();
            return true;
        }

        /// <summary>
        /// 绘制堆栈定位信息
        /// <param name="prefix">标题前缀</param>
        /// <param name="stackLocation">堆栈信息</param>
		/// <param name="del">调用委托</param>
        /// </summary>
        private void DrawStackLocationButton(string prefix, shaco.Base.StackLocation stackLocation, System.Delegate del)
        {
            if (stackLocation.HasStack())
            {
                GUILayout.BeginHorizontal();
                {
                    var stackInformation = stackLocation.GetStackInformation();
                    var stackLine = stackLocation.GetStackLine();
                    var stackFileName = System.IO.Path.GetFileName(stackInformation);

                    GUILayout.Label(stackFileName + ":" + stackLine, GUILayout.ExpandWidth(false));

                    var buttonStr = prefix + stackLocation.GetPerformanceDescription();
                    if (GUILayout.Button(buttonStr, GUILayout.ExpandWidth(false)))
                    {
                        shaco.Log.Info(stackLocation.GetTotalStackInformation());
                        EditorHelper.OpenAsset(stackInformation, stackLine);
                    }
                }
                GUILayout.EndHorizontal();

            }
        }

        private void OnObserverAddCallBack(object sender, shaco.Base.ISubjectBase arg)
        {
            var key = GetBindTargetKey(sender, arg);
            _leftTreeViewObservers.AddPath(key, arg);
        }

        private void OnObserverRemoveCallBack(object sender, shaco.Base.ISubjectBase arg)
        {
            var key = GetBindTargetKey(sender, arg);
            _leftTreeViewObservers.RemovePath(key);

            if (null != _currentSelectSubject && _currentSelectSubject.ContainsKey(arg))
            {
                CancelSelect();
            }
        }

        private void CancelSelect()
        {
            _currentSelectSubject.Clear();
            _currentSelectSubjectParamName.Clear();
            _searchParamName = string.Empty;
            _searchParamNameLower = string.Empty;
        }

        private string GetBindTargetKey(object sender, shaco.Base.ISubjectBase arg)
        {
            return arg.GetBindTarget().ToString();
        }

        private string GetSubectParamName(shaco.Base.ISubjectBase subject)
        {
            var retValue = string.Empty;
            var bindTarget = subject.GetBindTarget();

            if (null == bindTarget)
                return retValue;

            try
            {
                var findFiled = bindTarget.GetType().GetFields().Find(v => v.GetValue(bindTarget) == subject);
                if (null != findFiled)
                    retValue = findFiled.Name;
                else
                {
                    var findProperty = bindTarget.GetType().GetProperties().Find(v => v.GetValue(bindTarget) == subject);
                    if (null != findProperty)
                        retValue = findProperty.Name;
                    else
                        retValue = "null";
                }
            }
            catch (System.Exception e)
            {
                //忽略反射对象带来的任何报错，实际上多半是unity的弃用方法警告
                Debug.LogWarning("ObserverPreviewWindow GetSubectParamName warning: subject=" + subject + " bindTarget=" + bindTarget + "\ne=" + e);
            }
            return retValue;
        }

        private void PingIfComponent(shaco.Base.ISubjectBase subject)
        {
            var bindTarget = subject.GetBindTarget();
            var componentTmp = bindTarget as UnityEngine.Component;
            if (null != componentTmp)
            {
                EditorGUIUtility.PingObject(componentTmp);
            }
        }
    }
}