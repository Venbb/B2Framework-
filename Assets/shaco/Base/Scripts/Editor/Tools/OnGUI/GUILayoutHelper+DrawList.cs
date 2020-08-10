#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        private class ListActionInfo
        {
            public bool isSupportedForeignDragItem = false;
            public bool isForeignDragUpdating = false;
            public bool isForeignDragValidEvent = false;
            public bool isMousePressed = false;
            public object listTarget = null;
            public int mouseDragStartIndex = -1;
            public int mouseDragLastIndex = -1;
            public int mouseSelectedIndex = -1;
        }
        static private Dictionary<string, List<ListActionInfo>> _listActionsInfo = new Dictionary<string, List<ListActionInfo>>();
        static private object _currentSelectList = null;

        static public UnityEditorInternal.ReorderableList DrawReorderableList(UnityEditorInternal.ReorderableList list, SerializedObject serializedObject, string valueName)
        {
            if (null == list)
            {
                list = new UnityEditorInternal.ReorderableList(serializedObject,
                    serializedObject.FindProperty(valueName),
                    true, true, true, true);

                list.index = 0;

                list.drawElementCallback = (Rect rect, int index, bool selected, bool focused) =>
                {
                    SerializedProperty itemData = list.serializedProperty.GetArrayElementAtIndex(index);

                    rect.y += 2;
                    rect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.PropertyField(rect, itemData, GUIContent.none);
                };

                list.drawHeaderCallback = (Rect rect) =>
                {
                    var titleTmp = valueName;

                    if (titleTmp.Length > 0)
                        titleTmp = titleTmp.ReplaceWithIndex(char.ToUpper(titleTmp[0]).ToString(), 0, 1);

                    GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), titleTmp + " [Count:" + list.count + "]");
                    if (list.count > 0 && GUI.Button(new Rect(rect.x + rect.width - 52, rect.y, 52, rect.height), "Clear"))
                    {
                        list.serializedProperty.ClearArray();
                    }
                };
            }

            serializedObject.Update();
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();

            return list;
        }

        static public bool DrawList<T>(IList<T> list, string title, System.Func<T, T, bool> onCheckValueChangeCallBack = null, System.Action onAfterDrawHeaderCallBack = null, System.Func<int, T, System.Action<T>, bool> onDrawValueCallBack = null) where T : new()
        {
            return DrawListBase<T>(list, title, onCheckValueChangeCallBack, (callback) => callback(new T()), null, onAfterDrawHeaderCallBack, onDrawValueCallBack, null);
        }

        static public bool DrawStringList(List<string> list, string title, System.Func<string, string, bool> onCheckValueChangeCallBack = null, System.Action onAfterDrawHeaderCallBack = null)
        {
            return DrawListBase<string>(list, title, onCheckValueChangeCallBack, (callback) => callback(string.Empty), null, () =>
            {
                if (null != onAfterDrawHeaderCallBack)
                    onAfterDrawHeaderCallBack();

                DrawStringListCopyPaste(list, title);
            }, null, null);
        }

        static public void DrawStringListCopyPaste(List<string> list, string title)
        {
            System.Action executeCopy = () =>
            {
                shaco.UnityHelper.CopyToClipBoard(list.ToContactString(","));
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            };

            System.Action executePaste = () =>
            {
                list.Clear();
                list.AddRange(shaco.UnityHelper.PasteFromClipBoard().Split(","));
                GUI.changed = true;
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            };

            bool haveCopyData = shaco.UnityHelper.HaveCopyDataInClipBoard();

            //复制粘贴的快捷键和Unity自带的c、v操作冲突，所以注释这里功能
            // if (_currentSelectList == list)
            // {
            //     var currentEvent = Event.current;
            //     if (null != currentEvent && currentEvent.type == EventType.KeyDown)
            //     {
            //         if (currentEvent.control || currentEvent.command)
            //         {
            //             //copy
            //             if (currentEvent.keyCode == KeyCode.C)
            //             {
            //                 executeCopy();
            //                 currentEvent.Use();
            //             }
            //             //paste
            //             if (haveCopyData && currentEvent.keyCode == KeyCode.V)
            //             {
            //                 currentEvent.Use();
            //                 GUI.FocusControl(string.Empty);
            //                 executePaste();
            //             }
            //         }
            //     }
            // }

            if (GUILayout.Button("Copy", GUILayout.ExpandWidth(false)))
            {
                executeCopy();
            }

            if (haveCopyData && GUILayout.Button("Paste", GUILayout.ExpandWidth(false)))
            {
                executePaste();
            }
        }

        static private ListActionInfo CreateOrGetListActionInfo<T>(IList<T> list, string title)
        {
            ListActionInfo retValue = null;
            List<ListActionInfo> findInfo = null;
            if (!_listActionsInfo.TryGetValue(title, out findInfo))
            {
                findInfo = new List<ListActionInfo>();
                _listActionsInfo.Add(title, findInfo);
            }

            retValue = findInfo.Find(v => v.listTarget.GetType() == list.GetType());
            if (null == retValue)
            {
                retValue = new ListActionInfo();
                retValue.listTarget = list;
                retValue.isSupportedForeignDragItem = typeof(T) == typeof(string) || typeof(T).IsInherited(typeof(UnityEngine.Object));
                findInfo.Add(retValue);
            }
            return retValue;
        }

        static private bool OnListCreateCallBack<T>(IList<T> list, System.Func<T, T, bool> onCheckValueChangeCallBack, System.Action<System.Action<T>> onCreateCallBack, int insertIndex = -1)
        {
            bool valueChanged = false;
            if (list.IsOutOfRange(insertIndex))
                insertIndex = list.Count;
            System.Action<T> onInternalCreateCallBack = (T newItem) =>
            {
                if (null == newItem)
                    return;

                if (null != onCheckValueChangeCallBack)
                {
                    if (onCheckValueChangeCallBack(default(T), newItem))
                    {
                        list.Insert(insertIndex, newItem);
                        valueChanged |= GUI.changed;
                    }
                }
                else
                {
                    list.Insert(insertIndex, newItem);
                    valueChanged |= GUI.changed;
                }
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            };

            if (null == onCreateCallBack)
            {
                var newItem = (T)typeof(T).Instantiate();
                if (null == newItem)
                {
                    Debug.LogError("GUILayoutHelper+DrawList OnListCreateCallBack error: can't instantiate item, type=" + typeof(T));
                    return false;
                }

                if (list.Count > 0)
                {
                    if (typeof(T) == typeof(string))
                        newItem = list[0];
                    else
                        shaco.Base.Utility.CopyPropertiesAndFields(list[0], newItem);
                }
                onInternalCreateCallBack(newItem);
            }
            else
            {
                onCreateCallBack((newItem =>
                {
                    onInternalCreateCallBack(newItem);
                }));
            }
            return valueChanged;
        }

        static public bool DrawListBase<T>(IList<T> list, string title,
                                            System.Func<T, T, bool> onCheckValueChangeCallBack,
                                            System.Action<System.Action<T>> onCreateCallBack,
                                            System.Func<bool> onBeforeDrawHeaderCallBack,
                                            System.Action onAfterDrawHeaderCallBack,
                                            System.Func<int, T, System.Action<T>, bool> onDrawValueCallBack,
                                            System.Func<int, int, bool> onWillSwapValueCallBack)
        {
            var actionInfo = CreateOrGetListActionInfo(list, title);
            bool isOpened = false;
            bool isDisabled = false;
            bool valueChanged = false;
            Rect rectDragValid;

            if (null == list)
            {
                Debug.LogError("GUILayoutHelper+DrawList DrawListBase erorr: list is null");
                return false;
            }

            //绘制标题
            GUILayout.BeginHorizontal();
            {
                if (null != onBeforeDrawHeaderCallBack)
                    isDisabled = !onBeforeDrawHeaderCallBack();

                EditorGUI.BeginDisabledGroup(isDisabled);
                {
                    bool oldGUIChanged = GUI.changed;
                    GUI.changed = false;
                    isOpened = DrawHeader(title, title, true, () =>
                    {
                        if (GUILayout.Button("+", GUILayout.ExpandWidth(false)))
                        {
                            valueChanged |= OnListCreateCallBack(list, onCheckValueChangeCallBack, onCreateCallBack);
                        }
                        if (GUILayout.Button("-", GUILayout.ExpandWidth(false)))
                        {
                            if (null != onCheckValueChangeCallBack)
                            {
                                if (onCheckValueChangeCallBack(default(T), default(T)))
                                    list.Clear();
                            }
                            else
                                list.Clear();

                            GUI.changed = true;
                            GUI.FocusControl(string.Empty);
                            if (null != EditorWindow.focusedWindow)
                                EditorWindow.focusedWindow.Repaint();
                        }
                        if (null != onAfterDrawHeaderCallBack) onAfterDrawHeaderCallBack();
                    });
                    rectDragValid = GUILayoutUtility.GetLastRect();
                    if (GUI.changed)
                    {
                        _currentSelectList = list;
                    }
                    GUI.changed = oldGUIChanged;
                }
                EditorGUI.EndDisabledGroup();
            }
            GUILayout.EndHorizontal();

            if (!isOpened)
                return isOpened;

            //绘制列表内容
            T oldValue = default(T);
            var currentEvent = Event.current;
            bool mouseInAnyItemArea = false;
            bool shouldCallRepaint = false;

            for (int i = 0; i < list.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                {
                    oldValue = list[i];
                    if (null == onDrawValueCallBack)
                    {
                        if (null != list[i])
                            list[i] = (T)DrawValue("Item " + i, (object)list[i], list[i].GetType());
                        else
                            list[i] = (T)DrawValue("Item " + i, (object)list[i], typeof(T));

                        if (null != onCheckValueChangeCallBack && GUI.changed)
                        {
                            //可能对象重写过Equals方法，所以判断逻辑要特殊处理下
                            bool isEqualValue = true;
                            if (null != oldValue)
                            {
                                isEqualValue = oldValue.Equals(list[i]);
                            }
                            else if (null != list[i])
                            {
                                isEqualValue = list[i].Equals(oldValue);
                            }
                            else
                                isEqualValue = System.Object.Equals(oldValue, list[i]);

                            if (!isEqualValue && !onCheckValueChangeCallBack(oldValue, list[i]))
                            {
                                list[i] = oldValue;
                            }
                        }
                    }
                    else
                    {
                        int indexTmp = i;
                        bool needBreak = !onDrawValueCallBack(i, list[i], (changedValue) =>
                        {
                            bool canChanged = true;
                            if (null != onCheckValueChangeCallBack && !onCheckValueChangeCallBack(oldValue, changedValue))
                                canChanged = false;

                            if (canChanged && indexTmp >= 0 && indexTmp < list.Count)
                                list[indexTmp] = changedValue;
                        });
                        if (needBreak)
                        {
                            break;
                        }
                    }
                    valueChanged |= GUI.changed;
                }

                //因为绘制中一些回调方法如果调用了某些阻塞主线程的逻辑，例如打开一个窗口
                //这样会导致EndHorizontal时候报错，不过不会影响其他问题，所以过滤该报错
                try
                {
                    GUILayout.EndHorizontal();
                }
                catch { }

                //获取当前准备拖拽目标对象
                if (null != currentEvent && null != actionInfo)
                {
                    var lastRectTmp = GUILayoutUtility.GetLastRect();
                    if (lastRectTmp.Contains(currentEvent.mousePosition))
                    {
                        if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.MouseDrag)
                        {
                            actionInfo.mouseDragLastIndex = i;
                            actionInfo.mouseSelectedIndex = -1;
                            mouseInAnyItemArea = true;
                        }
                        else if (currentEvent.type == EventType.MouseDown)
                        {
                            actionInfo.mouseSelectedIndex = i;
                            mouseInAnyItemArea = true;
                            GUI.FocusControl(string.Empty);
                            if (null != EditorWindow.focusedWindow)
                                EditorWindow.focusedWindow.Repaint();
                        }
                    }

                    //绘制当前拖拽终止目标对象
                    if (null != actionInfo && (actionInfo.mouseDragStartIndex != -1 && actionInfo.mouseDragLastIndex == i) || actionInfo.mouseSelectedIndex == i)
                    {
                        var colorOld = GUI.color;
                        GUI.color = COLOR_SELECT;
                        GUI.DrawTexture(lastRectTmp, Texture2D.whiteTexture);
                        GUI.color = colorOld;
                        shouldCallRepaint = true;
                    }
                }
            }

            if (actionInfo.isForeignDragUpdating && list.Count > 0)
            {
                var lastRectTmp = GUILayoutUtility.GetLastRect();
                rectDragValid.yMax = lastRectTmp.yMax;
            }

            shouldCallRepaint |= UpdateDrawListEvent<T>(list, currentEvent, null != actionInfo ? actionInfo.mouseDragLastIndex : -1, actionInfo, rectDragValid, onWillSwapValueCallBack, onCheckValueChangeCallBack);

            //当发生拖拽信息的时候，如果当前拖拽位置不再任一组件内，则取消选中下标
            if (null != actionInfo)
            {
                if (!mouseInAnyItemArea)
                {
                    if (currentEvent.type == EventType.DragUpdated || currentEvent.type == EventType.MouseDrag)
                    {
                        actionInfo.mouseDragLastIndex = -1;
                        if (null != EditorWindow.focusedWindow)
                            EditorWindow.focusedWindow.Repaint();
                    }
                    else if (currentEvent.type == EventType.Ignore || currentEvent.type == EventType.MouseDown)
                    {
                        if (null != EditorWindow.focusedWindow)
                            EditorWindow.focusedWindow.Repaint();
                        actionInfo.mouseSelectedIndex = -1;
                    }
                }

                if (currentEvent.type == EventType.Used)
                    actionInfo.mouseSelectedIndex = -1;
            }

            if (valueChanged)
                GUI.changed = true;

            if (actionInfo.mouseDragLastIndex != -1)
            {
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            }

            if (actionInfo.isForeignDragValidEvent)
            {
                GUIHelper.DrawOutline(rectDragValid, 1, Color.green);
                if (null != EditorWindow.mouseOverWindow)
                    EditorWindow.FocusWindowIfItsOpen(EditorWindow.mouseOverWindow.GetType());
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            }
            return isOpened;
        }

        static private bool UpdateDrawListEvent<T>(IList<T> list, Event currentEvent, int mousePositionInItemIndex, ListActionInfo actionInfo, Rect rectDragValid, System.Func<int, int, bool> onWillSwapValueCallBack, System.Func<T, T, bool> onCheckValueChangeCallBack)
        {
            bool retValue = false;
            if (null == currentEvent || null == actionInfo)
                return retValue;

            switch (currentEvent.type)
            {
                case EventType.MouseDrag:
                    {
                        if (actionInfo.isMousePressed && actionInfo.mouseDragStartIndex == -1 && mousePositionInItemIndex != -1)
                        {
                            actionInfo.mouseDragStartIndex = mousePositionInItemIndex;
                            DragAndDrop.PrepareStartDrag();
                            DragAndDrop.objectReferences = new Object[] { new Object() };
                            DragAndDrop.StartDrag("Item " + actionInfo.mouseDragStartIndex);
                        }
                        break;
                    }
                case EventType.DragExited:
                    {
                        actionInfo.isForeignDragUpdating = false;
                        if (actionInfo.mouseDragStartIndex != -1)
                        {
                            if (mousePositionInItemIndex != -1 && actionInfo.mouseDragStartIndex != mousePositionInItemIndex)
                            {
                                bool canSwap = true;
                                if (null != onWillSwapValueCallBack)
                                    canSwap = onWillSwapValueCallBack(actionInfo.mouseDragStartIndex, mousePositionInItemIndex);
                                if (canSwap)
                                {
                                    //这里需要清理一次重做标记，否则会因为外部调用了Undo.RecordObject导致数据被回滚
                                    //已知bug：这里暂时没办法实现undo回滚‘删除操作’，似乎和Unity的undo机制有冲突？以后有时间再考虑解决吧
                                    Undo.FlushUndoRecordObjects();
                                    list.SwapValue(actionInfo.mouseDragStartIndex, mousePositionInItemIndex);
                                    if (null != EditorWindow.focusedWindow)
                                        EditorWindow.focusedWindow.Repaint();

                                    GUI.changed = true;
                                }
                            }
                            ReleaseDragAndSelectEvent(actionInfo);
                            actionInfo.isMousePressed = false;
                        }
                        else
                        {
                            //因为DragExited会执行两次，所以通过DragUpdated限制它
                            if (!actionInfo.isForeignDragValidEvent)
                                break;
                            actionInfo.isForeignDragValidEvent = false;

                            //这里需要清理一次重做标记，否则会因为外部调用了Undo.RecordObject导致数据被回滚
                            //已知bug：这里暂时没办法实现undo回滚‘删除操作’，似乎和Unity的undo机制有冲突？以后有时间再考虑解决吧
                            Undo.FlushUndoRecordObjects();

                            //拖拽的是string类型
                            if (typeof(T) == typeof(string))
                            {
                                var selection = DragAndDrop.paths;
                                if (null != selection)
                                {
                                    foreach (var iter in selection)
                                    {
                                        var newItem = (T)(object)iter;
                                        if (null == onCheckValueChangeCallBack || onCheckValueChangeCallBack(default(T), newItem))
                                            list.Add(newItem);
                                    }
                                }
                            }
                            //拖拽的可能是unity object类型
                            else
                            {
                                var selection = DragAndDrop.objectReferences;
                                if (null != selection)
                                {
                                    foreach (var iter in selection)
                                    {
                                        if (null == iter || !iter.GetType().IsInherited(typeof(T)))
                                            continue;

                                        var newItem = (T)(object)iter;
                                        if (null == onCheckValueChangeCallBack || onCheckValueChangeCallBack(default(T), newItem))
                                            list.Add(newItem);
                                    }
                                }
                            }
                        }
                        break;
                    }
                case EventType.DragUpdated:
                    {
                        if (actionInfo.mouseDragStartIndex == -1 && actionInfo.isSupportedForeignDragItem)
                        {
                            actionInfo.isForeignDragValidEvent = rectDragValid.Contains(currentEvent.mousePosition);
                            actionInfo.isForeignDragUpdating = true;
                        }
                        break;
                    }
                case EventType.MouseDown:
                    {
                        actionInfo.isMousePressed = true;
                        break;
                    }
                case EventType.MouseUp:
                    {
                        //如果在组件上点击了右键则弹出菜单
                        if (actionInfo.mouseSelectedIndex != -1 && currentEvent.button == 1)
                        {
                            GenericMenu menu = new GenericMenu();
                            var currentSelectItem = list[actionInfo.mouseSelectedIndex];
                            menu.AddDisabledItem(new GUIContent(string.Format("Item {0}: {1}", actionInfo.mouseSelectedIndex, currentSelectItem)));
                            menu.AddItem(new GUIContent("Move Up(↑)"), false, () => MoveItem(list, actionInfo, onWillSwapValueCallBack, -1));
#if UNITY_EDITOR_OSX
                            menu.AddItem(new GUIContent("Move Top(⌘+↑)"), false, () => MoveItem(list, actionInfo, onWillSwapValueCallBack, -actionInfo.mouseSelectedIndex));
#else
                            menu.AddItem(new GUIContent("Move Top(Ctrl+↑)"), false, () => MoveItem(list, actionInfo, onWillSwapValueCallBack, -actionInfo.mouseSelectedIndex));
#endif
                            menu.AddItem(new GUIContent("Move Down(↓)"), false, () => MoveItem(list, actionInfo, onWillSwapValueCallBack, 1));
#if UNITY_EDITOR_OSX
                            menu.AddItem(new GUIContent("Move Bottom(⌘+↓)"), false, () => MoveItem(list, actionInfo, onWillSwapValueCallBack, list.Count - actionInfo.mouseSelectedIndex - 1));
#else
                            menu.AddItem(new GUIContent("Move Bottom(Ctrl+↓)"), false, () => MoveItem(list, actionInfo, onWillSwapValueCallBack, -actionInfo.mouseSelectedIndex)); 
#endif

                            //Unity对象是不能直接复制
                            if (currentSelectItem.GetType().IsInherited<Object>())
                            {
#if UNITY_EDITOR_OSX
                                menu.AddDisabledItem(new GUIContent("Duplicate(⌘+D)"), false);
#else
                                menu.AddDisabledItem(new GUIContent("Duplicate(Ctrl+D)"), false);
#endif
                            }
                            else
                            {
#if UNITY_EDITOR_OSX
                                menu.AddItem(new GUIContent("Duplicate(⌘+D)"), false, () => Duplicate(list, actionInfo, onCheckValueChangeCallBack));
#else
                                menu.AddItem(new GUIContent("Duplicate(Ctrl+D)"), false, () => Duplicate(list, actionInfo, onCheckValueChangeCallBack));
#endif
                            }
                            menu.AddItem(new GUIContent("Delete(Del)"), false, () => Delete(list, actionInfo, onCheckValueChangeCallBack, currentEvent));
                            menu.ShowAsContext();
                        }

                        retValue = true;
                        actionInfo.isMousePressed = false;
                        actionInfo.mouseDragLastIndex = -1;
                        break;
                    }
                case EventType.Used:
                case EventType.MouseEnterWindow:
                case EventType.MouseLeaveWindow:
                    {
                        ReleaseDragAndSelectEvent(actionInfo);
                        break;
                    }
                case EventType.KeyUp:
                    {
                        if (actionInfo.mouseSelectedIndex >= 0 && actionInfo.mouseSelectedIndex <= list.Count - 1)
                        {
                            if (currentEvent.keyCode == KeyCode.Delete)
                            {
                                Delete(list, actionInfo, onCheckValueChangeCallBack, currentEvent);
                            }

                            //同时按下Control或者Command，并按字母D，重复添加一个对象
                            if ((currentEvent.control || currentEvent.command) && currentEvent.keyCode == KeyCode.D)
                            {
                                Duplicate(list, actionInfo, onCheckValueChangeCallBack);
                            }

                            //按上下键盘调整所在列表下标
                            //如果同时按住Control或者Command则一次性移动到最前或者最后
                            if (currentEvent.keyCode == KeyCode.UpArrow && actionInfo.mouseSelectedIndex > 0)
                            {
                                if (currentEvent.control || currentEvent.command)
                                {
                                    MoveItem(list, actionInfo, onWillSwapValueCallBack, -actionInfo.mouseSelectedIndex);
                                }
                                else
                                    MoveItem(list, actionInfo, onWillSwapValueCallBack, -1);
                                if (null != EditorWindow.focusedWindow)
                                    EditorWindow.focusedWindow.Repaint();
                            }
                            if (currentEvent.keyCode == KeyCode.DownArrow && actionInfo.mouseSelectedIndex < list.Count - 1)
                            {
                                if (currentEvent.control || currentEvent.command)
                                {
                                    MoveItem(list, actionInfo, onWillSwapValueCallBack, list.Count - actionInfo.mouseSelectedIndex - 1);
                                }
                                else
                                    MoveItem(list, actionInfo, onWillSwapValueCallBack, 1);
                                if (null != EditorWindow.focusedWindow)
                                    EditorWindow.focusedWindow.Repaint();
                            }
                        }
                        break;
                    }
            }
            return retValue;
        }

        static private void MoveItem<T>(IList<T> list, ListActionInfo actionInfo, System.Func<int, int, bool> onWillSwapValueCallBack, int offsetCount)
        {
            var oldValue = list[actionInfo.mouseSelectedIndex];

            bool canSwap = true;
            if (null != onWillSwapValueCallBack)
                canSwap = onWillSwapValueCallBack(actionInfo.mouseSelectedIndex, actionInfo.mouseSelectedIndex + offsetCount);

            if (canSwap)
            {
                list.RemoveAt(actionInfo.mouseSelectedIndex);
                list.Insert(actionInfo.mouseSelectedIndex + offsetCount, oldValue);
                actionInfo.mouseSelectedIndex += offsetCount;
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();

                GUI.changed = true;
            }
        }

        static private void Duplicate<T>(IList<T> list, ListActionInfo actionInfo, System.Func<T, T, bool> onCheckValueChangeCallBack)
        {
            //Unity对象是不能直接复制
            var currentSelectItem = list[actionInfo.mouseSelectedIndex];
            if (currentSelectItem.GetType().IsInherited<Object>())
                return;

            var newItem = (T)currentSelectItem.GetType().Instantiate();
            if (null == newItem)
            {
                Debug.LogError("GUILayoutHelper+DrawList Duplicate error: can't instantiate item, type=" + list[actionInfo.mouseSelectedIndex].GetType());
                return;
            }
            if (typeof(T) == typeof(string))
                newItem = list[actionInfo.mouseSelectedIndex];
            shaco.Base.Utility.CopyPropertiesAndFields(list[actionInfo.mouseSelectedIndex], newItem,
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (null == onCheckValueChangeCallBack || onCheckValueChangeCallBack(default(T), newItem))
            {
                list.Insert(++actionInfo.mouseSelectedIndex, newItem);
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
            }
        }

        static private void Delete<T>(IList<T> list, ListActionInfo actionInfo, System.Func<T, T, bool> onCheckValueChangeCallBack, Event currentEvent)
        {
            if (null == onCheckValueChangeCallBack || onCheckValueChangeCallBack(list[actionInfo.mouseSelectedIndex], default(T)))
            {
                list.RemoveAt(actionInfo.mouseSelectedIndex);
                GUI.changed = true;

                //这里必须要重置编辑器ui焦点，否则可能意外删除了其他内容
                GUI.FocusControl(string.Empty);
                currentEvent.Use();
                if (null != EditorWindow.focusedWindow)
                    EditorWindow.focusedWindow.Repaint();
                ReleaseDragAndSelectEvent(actionInfo);
            }
        }

        static private void ReleaseDragAndSelectEvent(ListActionInfo actionInfo)
        {
            actionInfo.mouseDragStartIndex = -1;
            actionInfo.mouseSelectedIndex = -1;
            actionInfo.mouseDragStartIndex = -1;
            actionInfo.mouseDragLastIndex = -1;
            actionInfo.isForeignDragValidEvent = false;
            actionInfo.isForeignDragUpdating = false;
        }
    }
}
#endif