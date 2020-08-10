using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        private class DrawInfo
        {
            public bool isDragUpdate = false;
        }

        static private Dictionary<string, DrawInfo> _drawInfos = new Dictionary<string, DrawInfo>();
        static private readonly Color COLOR_SELECT = new Color(62.0f / 255, 95.0f / 255, 150.0f / 255, 80.0f / 255);

        /// <summary>
        /// 地址输入控件
        /// <param name="prefixGUI">UI前缀名字</param>
        /// <param name="path">地址</param>
        /// <param name="extensions">地址后缀名，如果不设定后缀名，默认为文件夹，如果设定后缀名默认为文件，如果为'*'则视为匹配所有后缀名</param>
        /// <param name="defaultBrowsePath">当地址为空的时候Browse默认打开路径</param>
        /// <param name="allowNonUnityAssetPath">是否允许拖拽非Unity项目中的文件</param>
        /// <return>当前地址</return>
        /// </summary>
        static public string PathField(string prefixGUI, string path, string extensions, string defaultBrowsePath = shaco.Base.GlobalParams.EmptyString, bool allowNonUnityAssetPath = false)
        {
            return PathFieldBase(prefixGUI, path, extensions, defaultBrowsePath, allowNonUnityAssetPath);
        }

        /// <summary>
        /// 地址输入控件
        /// <param name="prefixGUI">UI前缀名字</param>
        /// <param name="path">地址</param>
        /// <param name="extensions">地址后缀名，如果不设定后缀名，默认为文件夹，如果设定后缀名默认为文件，如果为'*'则视为匹配所有后缀名</param>
        /// <param name="defaultBrowsePath">当地址为空的时候Browse默认打开路径</param>
        /// <param name="allowNonUnityAssetPath">是否允许拖拽非Unity项目中的文件</param>
        /// <return>当前地址</return>
        /// </summary>
        static public string PathField(GUIContent prefixGUI, string path, string extensions, string defaultBrowsePath = shaco.Base.GlobalParams.EmptyString, bool allowNonUnityAssetPath = false)
        {
            return PathFieldBase(prefixGUI, path, extensions, defaultBrowsePath, allowNonUnityAssetPath);
        }

        /// <summary>
        /// 地址输入控件
        /// <param name="prefixGUI">UI前缀名字</param>
        /// <param name="path">地址</param>
        /// <param name="extensions">地址后缀名，如果不设定后缀名，默认为文件夹，如果设定后缀名默认为文件，如果为'*'则视为匹配所有后缀名</param>
        /// <param name="defaultBrowsePath">当地址为空的时候Browse默认打开路径</param>
        /// <param name="allowNonUnityAssetPath">是否允许拖拽非Unity项目中的文件</param>
        /// <return>当前地址</return>
        /// </summary>
        static private string PathFieldBase(object prefixGUI, string path, string extensions, string defaultBrowsePath = shaco.Base.GlobalParams.EmptyString, bool allowNonUnityAssetPath = false)
        {
            if (null == path)
                path = string.Empty;

            Rect controlRect = new Rect();
            DrawInfo drawInfo = null;
            GUILayout.BeginHorizontal();
            {
                if (path.StartsWith("Assets"))
                {
                    var assetObjectTmp = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
                    UnityEngine.Object newAssetObject = null;
                    if (prefixGUI is string)
                    {
                        newAssetObject = EditorGUILayout.ObjectField((string)prefixGUI, assetObjectTmp, typeof(Object), true);
                    }
                    else if (prefixGUI is GUIContent)
                    {
                        newAssetObject = EditorGUILayout.ObjectField((GUIContent)prefixGUI, assetObjectTmp, typeof(Object), true);
                    }
                    else
                    {
                        Debug.LogError("GUILayoutHelper+PathInput PathField error: unsupport prefixGUI type=" + prefixGUI.ToTypeString());
                    }

                    if (newAssetObject != assetObjectTmp)
                    {
                        if (null != newAssetObject)
                        {
                            var fullPathTmp = EditorHelper.GetFullPath(newAssetObject);
                            if (!string.IsNullOrEmpty(extensions))
                            {
                                if (shaco.Base.FileHelper.ExistsFile(fullPathTmp))
                                {
                                    path = EditorHelper.FullPathToUnityAssetPath(fullPathTmp);
                                }
                                else
                                {
                                    Debug.LogError("GUILayoutHelper PathField error: not a file path=" + fullPathTmp);
                                }
                            }
                            else
                            {
                                if (shaco.Base.FileHelper.ExistsDirectory(fullPathTmp))
                                {
                                    path = EditorHelper.FullPathToUnityAssetPath(fullPathTmp);
                                }
                                else
                                {
                                    Debug.LogError("GUILayoutHelper PathField error: not a directory path=" + fullPathTmp);
                                }
                            }
                        }
                        else
                        {
                            path = string.Empty;
                        }
                    }
                }
                else
                {
                    var oldColor = GUI.color;
                    EditorGUI.BeginDisabledGroup(true);
                    {
                        string prefixName = string.Empty;
                        if (prefixGUI is string)
                        {
                            prefixName = prefixGUI as string;
                            drawInfo = GetDrawInfo(prefixName);
                        }
                        else if (prefixGUI is GUIContent)
                        {
                            prefixName = (prefixGUI as GUIContent).text;
                            drawInfo = GetDrawInfo(prefixName);
                        }
                        else
                        {
                            Debug.LogError("GUILayoutHelper+PathInput PathField error: unsupport prefixGUI type=" + prefixGUI.ToTypeString());
                        }

                        if (null != drawInfo)
                        {
                            if (drawInfo.isDragUpdate)
                                GUI.color = COLOR_SELECT;

                            if (string.IsNullOrEmpty(path))
                            {
                                var contentTmp = string.Empty;
                                if (null != DragAndDrop.paths && DragAndDrop.paths.Length > 1)
                                    contentTmp = "Multiple drag not supported";
                                else
                                    contentTmp = string.IsNullOrEmpty(extensions) ? "Drag a folder to here" : string.Format("Drag a file [{0}] to here", extensions);
                                EditorGUILayout.TextField(prefixName, contentTmp);
                            }
                            else
                                EditorGUILayout.TextField(prefixName, path);

                            if (drawInfo.isDragUpdate)
                                GUI.color = oldColor;
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    if (null != drawInfo)
                        controlRect = GUILayoutUtility.GetLastRect();

                    if (!string.IsNullOrEmpty(path))
                    {
                        if (GUILayout.Button("Delete", GUILayout.ExpandWidth(false)))
                            path = string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(path))
                {
                    if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                        EditorHelper.ShowInFolder(path);
                }

                //现在支持更方便的拖拽操作了，不再需要浏览功能
                // if (GUILayout.Button("Browse", GUILayout.ExpandWidth(false)))
                // {
                //     var defaultPath = !shaco.Base.FileHelper.ExistsFile(path) && !shaco.Base.FileHelper.ExistsDirectory(path) ? defaultBrowsePath : path;
                //     if (string.IsNullOrEmpty(defaultPath))
                //         defaultPath = Application.dataPath;

                //     var selectPath = string.Empty;
                //     if (string.IsNullOrEmpty(extensions))
                //     {
                //         selectPath = EditorUtility.OpenFolderPanel("Select a folder", defaultPath, extensions == "*" ? string.Empty : extensions);
                //     }
                //     else
                //     {
                //         selectPath = EditorUtility.OpenFilePanel("Select a file", defaultPath, extensions == "*" ? string.Empty : extensions);
                //     }
                //     if (!string.IsNullOrEmpty(selectPath))
                //     {
                //         path = selectPath;
                //         if (path.IndexOf(Application.dataPath) >= 0)
                //         {
                //             path = EditorHelper.FullPathToUnityAssetPath(path);
                //         }
                //     }
                //     else
                //         GUI.changed = false;
                // }
            }
            GUILayout.EndHorizontal();

            if (null != drawInfo)
            {
                UpdateDragEvent(drawInfo, controlRect, (dragPath) =>
                {
                    //这里需要清理一次重做标记，否则会因为外部调用了Undo.RecordObject导致数据被回滚
                    //已知bug：这里暂时没办法实现undo回滚‘删除操作’，似乎和Unity的undo机制有冲突？以后有时间再考虑解决吧
                    Undo.FlushUndoRecordObjects();
                    path = dragPath;
                    GUI.changed = true;
                }, extensions, allowNonUnityAssetPath);
            }
            return path;
        }

        static private DrawInfo GetDrawInfo(string prefix)
        {
            DrawInfo retValue = null;
            if (!_drawInfos.TryGetValue(prefix, out retValue))
            {
                retValue = new DrawInfo();
                _drawInfos.Add(prefix, retValue);
            }
            return retValue;
        }

        static private void UpdateDragEvent(DrawInfo drawInfo, Rect controlRect, System.Action<string> callbackInControlRect, string extensions, bool allowNonUnityAssetPath)
        {
            var currentEvent = Event.current;
            if (null == currentEvent || null == drawInfo)
                return;

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                    {
                        if (null != DragAndDrop.paths && DragAndDrop.paths.Length == 1)
                        {
                            var dragPath = DragAndDrop.paths[0];
                            if (IsValidDrag(controlRect, currentEvent.mousePosition, dragPath, extensions, allowNonUnityAssetPath))
                            {
                                drawInfo.isDragUpdate = true;
                            }
                            else
                            {
                                drawInfo.isDragUpdate = false;
                            }

                            //需要重绘拖拽鼠标位置窗口，否则蓝色的选择框颜色不刷新
                            if (null != EditorWindow.mouseOverWindow)
                                EditorWindow.mouseOverWindow.Repaint();
                        }
                        break;
                    }
                case EventType.DragExited:
                    {
                        //因为DragExited会执行两次，所以通过DragUpdated限制它
                        if (!drawInfo.isDragUpdate || null == callbackInControlRect || null == DragAndDrop.paths || DragAndDrop.paths.Length != 1)
                            break;

                        drawInfo.isDragUpdate = false;

                        var dragPath = DragAndDrop.paths[0];
                        if (IsValidDrag(controlRect, currentEvent.mousePosition, dragPath, extensions, allowNonUnityAssetPath))
                        {
                            //这里需要use掉DragExited事件，否则GUILayout相关绘制方法会报错
                            currentEvent.Use();
                            callbackInControlRect(dragPath);
                        }
                        break;
                    }
            }
        }

        static private bool IsValidDrag(Rect controlRect, Vector2 mousePosition, string dragPath, string extensions, bool allowNonUnityAssetPath)
        {
            bool retValue = false;
            if (!controlRect.Contains(mousePosition))
                return retValue;

            var resultPath = string.Empty;
            if (allowNonUnityAssetPath)
            {
                if (dragPath.StartsWith(Application.dataPath))
                    resultPath = dragPath;
            }
            else
                resultPath = dragPath;

            if (string.IsNullOrEmpty(resultPath))
                return retValue;

            //判断为文件夹是否存在
            if (string.IsNullOrEmpty(extensions))
            {
                if (!System.IO.Directory.Exists(resultPath))
                    resultPath = string.Empty;
            }
            //判断文件夹是否存在且后缀名正确
            else
            {
                if (!System.IO.File.Exists(resultPath))
                    resultPath = string.Empty;

                bool isValidExtensions = false;
                foreach (var iter in extensions.Split(','))
                {
                    if (resultPath.EndsWith(iter))
                    {
                        isValidExtensions = true;
                        break;
                    }
                }
                if (!isValidExtensions)
                    resultPath = string.Empty;
            }

            retValue = !string.IsNullOrEmpty(resultPath);
            return retValue;
        }
    }
}