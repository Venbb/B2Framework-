#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class EditorHelper
    {
        /// <summary>
        /// 绘制List的帮助类，现在基本都用GUILayoutHelper.DrawList代替了
        /// </summary>
        public class ListHelper<T>
        {
            public delegate bool IS_NULL_FUNC(T t1);
            public delegate void INDEX_CALLBACK(int index);
            public delegate T DEFAULT_CALLFUNC();

            public DEFAULT_CALLFUNC OnCreateCallBack = null;

            public void AutoListSize(string prefixName, List<T> listData, IS_NULL_FUNC isNullCallFunc, INDEX_CALLBACK onDeleteEndCallFunc = null, INDEX_CALLBACK onCreateEndCallFunc = null)
            {
                if (listData.Count == 0 || !isNullCallFunc(listData[listData.Count - 1]))
                {
                    listData.Add(OnCreateCallBack != null ? OnCreateCallBack() : default(T));

                    if (onCreateEndCallFunc != null)
                        onCreateEndCallFunc(listData.Count - 1);
                }

                for (int i = listData.Count - 2; i >= 0; --i)
                {
                    if (isNullCallFunc(listData[i]))
                    {
                        if (onDeleteEndCallFunc != null)
                            onDeleteEndCallFunc(i);
                        listData.RemoveAt(i);

                        EditorHelper.SetDirty(null);
                    }
                }

                if (!string.IsNullOrEmpty(prefixName))
                    EditorGUILayout.IntField(prefixName, listData.Count - 1);
            }
        }

        public class PositionInput
        {
            static private Rect _rect = new Rect(0, 0, 50, 50);
            static public Rect Draw()
            {
                _rect = EditorGUILayout.RectField(_rect);
                return _rect;
            }
        }

        static public string GetAssetPathLower(Object asset)
        {
            return AssetDatabase.GetAssetPath(asset).ToLower();
        }

        static public bool CloseWindow<T>(T window = null) where T : UnityEditor.EditorWindow
        {
            var findWindowTmp = FindWindow<T>();
            if (null != findWindowTmp)
            {
                findWindowTmp.Close();
                return true;
            }
            else
                return false;
        }

        static public void OpenAsset(string path, int line)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("EditorHelper OpenAsset error: path is invalid");
                return;
            }

            path = path.Replace('\\', shaco.Base.FileDefine.PATH_FLAG_SPLIT);
            var indexTmp = path.IndexOf("Assets/");
            if (indexTmp >= 0)
                path = path.Substring(indexTmp, path.Length - indexTmp);

            var loadAsset = AssetDatabase.LoadAssetAtPath(path, typeof(Object));
            AssetDatabase.OpenAsset(loadAsset, line);

            shaco.Log.Info("EditorHelper OpenAsset, path=" + path + " line=" + line);
        }

        static public void DrawForegourndFrame(float thickness = 2)
        {
            var rectTmp = GUILayoutUtility.GetLastRect();
            var colorOld = GUI.color;
            GUI.color = new Color(62.0f / 255, 95.0f / 255, 150.0f / 255);

            var rectUp = new Rect(rectTmp.x, rectTmp.y, rectTmp.width, thickness);
            var rectDown = new Rect(rectTmp.x, rectTmp.y + (rectTmp.height - thickness), rectTmp.width, thickness);
            var rectLeft = new Rect(rectTmp.x, rectTmp.y, thickness, rectTmp.height);
            var rectRight = new Rect(rectTmp.x + (rectTmp.width - thickness), rectTmp.y, thickness, rectTmp.height);

            GUI.DrawTexture(rectUp, Texture2D.whiteTexture);
            GUI.DrawTexture(rectDown, Texture2D.whiteTexture);
            GUI.DrawTexture(rectLeft, Texture2D.whiteTexture);
            GUI.DrawTexture(rectRight, Texture2D.whiteTexture);

            GUI.color = colorOld;
        }

        //获取编辑器中勾选上的场景
        static public string[] GetEnabledEditorScenes()
        {
            List<string> EditorScenes = new List<string>();

            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (!scene.enabled)
                    continue;
                EditorScenes.Add(scene.path);
            }

            return EditorScenes.ToArray();
        }

        /// <summary>
        /// 获取Unity序列化对象所有数据
        /// <param name="obj">Unity对象</param>
        /// </summary>
        static public void PrintSerializedObjectProperties(Object obj)
        {
            SerializedObject serializedObject = new SerializedObject(obj);
            var iter = serializedObject.GetIterator();
            iter.Next(true);
            while (iter.Next(false))
            {
                var result = AnalyseSerializedProperty(iter);
                if (null != result)
                    Debug.Log("name=" + result.key + " value=" + result.value);
            }
        }

        /// <summary>
        /// 分析并获取序列化对象中所有的值
        /// <param name="obj">序列化对象</param>
        /// <return>序列化对象中所有的值</return>
        /// </summary>
        public class AnalyseObjectResult
        {
            public string key = string.Empty;
            public object value = null;
            public SerializedProperty property = null;
        }
        static public AnalyseObjectResult[] AnalyseObject(UnityEngine.Object obj)
        {
            var retValue = new List<AnalyseObjectResult>();
            SerializedObject serializedObject = new SerializedObject(obj);
            var iter = serializedObject.GetIterator();
            iter.Next(true);

            while (iter.Next(false))
            {
                var anlyseObject = AnalyseSerializedProperty(iter);
                if (null != anlyseObject /*&& !anlyseObject.property.propertyPath.Contains(".")*/)
                    retValue.Add(anlyseObject);
            }
            var retValueArray = retValue.ToArray();
            return retValueArray;
        }

        /// <summary>
        /// 分析并获取unity序列化属性值
        /// <param name="obj">序列化属性</param>
        /// <return>属性值</return>
        /// </summary>
        static public AnalyseObjectResult AnalyseSerializedProperty(SerializedProperty obj)
        {
            object anlyseObject = null;

            if (obj.isArray && SerializedPropertyType.String != obj.propertyType)
            {
                //unity的序列化数组遍历的时候偶尔会出现死循环，所以数组数据就暂时不支持了
                anlyseObject = obj.propertyPath + "[Array]";
            }
            else
            {
                switch (obj.propertyType)
                {
                    case SerializedPropertyType.Generic: anlyseObject = null; break;
                    case SerializedPropertyType.Integer: anlyseObject = obj.intValue; break;
                    case SerializedPropertyType.Boolean: anlyseObject = obj.boolValue; break;
                    case SerializedPropertyType.Float: anlyseObject = obj.floatValue; break;
                    case SerializedPropertyType.String: anlyseObject = obj.stringValue; break;
                    case SerializedPropertyType.Color: anlyseObject = obj.colorValue; break;
                    case SerializedPropertyType.ObjectReference: anlyseObject = obj.objectReferenceValue; break;
                    case SerializedPropertyType.LayerMask: anlyseObject = (LayerMask)obj.intValue; break;
                    case SerializedPropertyType.Enum:
                        {
                            if (obj.enumNames.IsNullOrEmpty())
                                anlyseObject = "undefined";
                            else
                                anlyseObject = obj.enumDisplayNames[obj.enumValueIndex];
                            break;
                        }
                    case SerializedPropertyType.Vector2: anlyseObject = obj.vector2Value; break;
                    case SerializedPropertyType.Vector3: anlyseObject = obj.vector3Value; break;
                    case SerializedPropertyType.Vector4: anlyseObject = obj.vector4Value; break;
                    case SerializedPropertyType.Rect: anlyseObject = obj.rectValue; break;
                    case SerializedPropertyType.ArraySize: anlyseObject = null; break;
                    case SerializedPropertyType.AnimationCurve: anlyseObject = obj.animationCurveValue; break;
                    case SerializedPropertyType.Bounds: anlyseObject = obj.boundsValue; break;
                    case SerializedPropertyType.Gradient: anlyseObject = null; break;
                    case SerializedPropertyType.Quaternion: anlyseObject = obj.quaternionValue; break;
                    case SerializedPropertyType.ExposedReference: anlyseObject = obj.exposedReferenceValue; break;
                    default: Debug.LogError("AssetBundleInspector AnalyseObject error: unsupport type=" + obj.propertyType + " property=" + obj.name); break;
                }
            }

            return null != anlyseObject ? new AnalyseObjectResult() { key = obj.propertyPath, value = anlyseObject, property = obj } : null;
        }

        /// <summary>
        /// 获取绝对路径
        /// <param name="obj">unity对象</param>
        /// <return>绝对路径</return>
        /// </summary>
        static public string GetFullPath(Object obj)
        {
            return shaco.Base.FileHelper.ContactPath(Application.dataPath.Remove("Assets"), AssetDatabase.GetAssetPath(obj));
        }

        /// <summary>
        /// 获取绝对路径
        /// <param name="relativePath">相对路径</param>
        /// <return>绝对路径</return>
        /// </summary>
        static public string GetFullPath(string relativePath)
        {
            return shaco.UnityHelper.UnityPathToFullPath(relativePath);
        }

        /// <summary>
        /// 获取unity对象相对路径
        /// <param name="fullPath">绝对路径</param>
        /// <return>unity对象相对路径</return>
        /// </summary>
        static public string FullPathToUnityAssetPath(string fullPath)
        {
            return shaco.UnityHelper.FullPathToUnityPath(fullPath);
        }

        /// <summary>
        /// 判断是否为Unity项目文件路径
        /// </summary>
        static public bool IsUnityAssetPath(string path)
        {
            return path.StartsWith("Assets");
        }

        /// <summary>
        /// 显示文件或者文件夹在设备的文件夹中
        /// <param name="path">文件或文件夹路径</param>
        /// </summary>
        static public void ShowInFolder(string path)
        {
            if (shaco.Base.FileHelper.ExistsDirectory(path) || shaco.Base.FileHelper.ExistsFile(path))
            {
                EditorUtility.RevealInFinder(path);
            }
            else
            {
                Debug.Log("EditorHelper ShowInfFolder error: not found path=" + path);
            }
        }

        /// <summary>
        /// 注册要撤销的操作对象
        /// <param name="window">对象</param>
        /// </summary>
        static public void RecordObjectWindow(EditorWindow window)
        {
            Undo.RecordObject(window, window.GetType().FullName);
            if (null != Event.current && Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Z && (Event.current.command || Event.current.control))
                window.Repaint();
        }

        /// <summary>
        /// 检查文件夹是否存在并自动创建
        /// </summary>
        static public void CheckFolderPathWithAutoCreate(string folderPath)
        {
            folderPath = folderPath.RemoveLastFlag();
            var fullFolderPath = EditorHelper.GetFullPath(folderPath);
            if (!shaco.Base.FileHelper.ExistsDirectory(fullFolderPath))
            {
                var parentFolderName = shaco.Base.FileHelper.GetFolderNameByPath(folderPath).RemoveLastFlag();
                AssetDatabase.CreateFolder(parentFolderName, shaco.Base.FileHelper.GetLastFileName(folderPath));
            }
        }

        /// <summary>
        /// 安全创建asset，当文件夹不存在时候自动创建
        /// </summary>
        static public void CreateAssetSafe(Object asset, string path)
        {
            var folderPath = shaco.Base.FileHelper.GetFolderNameByPath(path);
            CheckFolderPathWithAutoCreate(folderPath);
            AssetDatabase.CreateAsset(asset, path);
        }

        /// <summary>
        /// 获取或者自动创建包体版本号路径
        /// 版本号文件必须放在Resources目录下，因为要在任何地方都可以动态获取到
        /// </summary>
        static public TextAsset GetBuildVerionFile()
        {
            TextAsset retValue = null;
            string fileAssetPath = string.Empty;
            var searchFilter = string.Format("{0} t:TextAsset", shaco.Base.FileHelper.RemoveLastExtension(shaco.HotUpdateDefine.VERSION_PATH));
            var findResult = AssetDatabase.FindAssets(searchFilter, new string[] { "Assets" });
            var findTag = "Resources".ContactPath(shaco.HotUpdateDefine.VERSION_PATH);
            if (null != findResult && findResult.Length > 0)
            {
                for (int i = findResult.Length - 1; i >= 0; --i)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(findResult[i]);
                    if (assetPath.EndsWith(findTag))
                    {
                        fileAssetPath = assetPath;
                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(fileAssetPath))
            {
                retValue = AssetDatabase.LoadAssetAtPath<TextAsset>(fileAssetPath);
                if (null == retValue)
                {
                    Debug.LogError("EditorHelper GetOrCreateBuildVerionFile error: can't load path=" + fileAssetPath);
                }
            }
            return retValue;
        }

        static public TextAsset SetBuildVersionFile(string version = null)
        {
            var retValue = GetBuildVerionFile();
            var setVersion = string.IsNullOrEmpty(version) ? Application.version : version;

            //新建一个版本号文件 或者 重新写入文件内容
            if (null == retValue)
            {
                var versionFullPath = Application.dataPath.ContactPath("Resources").ContactPath(shaco.HotUpdateDefine.VERSION_PATH);
                var assetPath = EditorHelper.FullPathToUnityAssetPath(versionFullPath);
                shaco.Base.FileHelper.WriteAllByUserPath(versionFullPath, setVersion);
                AssetDatabase.ImportAsset(assetPath);

                retValue = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            }
            //重新设置版本号内容
            else
            {
                //版本号不一致时候才重新设定
                if (retValue.ToString() != setVersion)
                {
                    var assetPath = AssetDatabase.GetAssetPath(retValue);
                    var versionFullPath = GetFullPath(assetPath);
                    shaco.Base.FileHelper.WriteAllByUserPathWithoutLog(versionFullPath, setVersion);
                }
            }
            return retValue;
        }
    }
}
#endif