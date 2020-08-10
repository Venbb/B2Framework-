using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ChangeComponentDataWindow : EditorWindow
    {
        public class TargetObjectInfo
        {
            public Object target;
            public List<Component> components = null;
        }

        private Object _sourceTarget = null;
        private Dictionary<Object, TargetObjectInfo> _targetObjects = new Dictionary<Object, TargetObjectInfo>();
        private List<Object> _inputTargetObjects = null;
        private List<Object> _willRemoveTargetObjects = new List<Object>();
        [SerializeField]
        private List<string> _inputPropertyPaths = null;
        [SerializeField]
        private string _inputComponentType = null;
        [SerializeField]
        private ChangeComponentDataHelper.ValueType _selectValueType = ChangeComponentDataHelper.ValueType.None;
        [SerializeField]
        private System.Type _unityObjectType;
        [SerializeField]
        private shaco.AutoValue _autoValue = new shaco.AutoValue();
        private Vector2 _srollPosition = Vector2.zero;
        [SerializeField]
        private string _searchName = string.Empty;
        [SerializeField]
        private bool _isGlobalSearch = false;
        private bool _isChanging = false;
        private shaco.Instance.Editor.TreeView.WindowSplitter _dragLineSeparator = new shaco.Instance.Editor.TreeView.WindowSplitter();
        private IChangeComponentData _changeComponentInterface = new ChangeComponentDataDefault();

        private readonly int MAX_SHOW_COUNT = 50;


        [MenuItem("shaco/Tools/ChangeComponentData", false, (int)ToolsGlobalDefine.MenuPriority.Tools.CHANGE_COMPONENT_DATA)]
        static public void OpenChangeComponentDataWindowInProjectMenu()
        {
            var window = OpenChangeComponentDataWindowInProjectMenu(null, null);
            window.Init();
        }

        static public ChangeComponentDataWindow OpenChangeComponentDataWindowInProjectMenu(Object sourceTarget, List<Object> objs)
        {
            var retValue = EditorHelper.GetWindow<ChangeComponentDataWindow>(null, true, "ChangeComponentData");
            retValue.Init();
            if (!retValue.SetSourceTarget(sourceTarget, objs))
            {
                retValue.Close();
                Debug.LogWarning("ChangeComponentDataWindow OpenChangeComponentDataWindowInProjectMenu warning: no data");
            }
            retValue._isGlobalSearch = false;
            return retValue;
        }

        void OnEnable()
        {
            EditorHelper.GetWindow<ChangeComponentDataWindow>(this, true, "ChangeComponentData").Init();
        }

        void OnGUI()
        {
            if (_isChanging)
                return;
            
            EditorHelper.RecordObjectWindow(this);

            //左边窗口
            _dragLineSeparator.BeginLayout(true);
            {
                DrawChangeComponentInterface();
                DrawSourceTarget();
                DrawInputParameters();
                DrawUpdateButton();
            }
            _dragLineSeparator.EndLayout();

            //右边窗口
            _dragLineSeparator.BeginLayout();
            {
                DrawSearchName();
                EditorGUILayout.LabelField("Total Count", _targetObjects.Count.ToString());

                _srollPosition = GUILayout.BeginScrollView(_srollPosition);
                {
                    int index = 0;
                    foreach (var iter in _targetObjects)
                    {
                        try
                        {
                            DrawTarget(iter.Value);
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError("DrawTarget erorr: e=" + e);
                            _willRemoveTargetObjects.Add(iter.Key);
                        }

                        if (++index > MAX_SHOW_COUNT)
                        {
                            break;
                        }
                    }

                    if (_willRemoveTargetObjects.Count > 0 && !_targetObjects.IsNullOrEmpty())
                    {
                        foreach (var iter in _willRemoveTargetObjects)
                        {
                            if (null != iter)
                                _targetObjects.Remove(iter);
                        }
                    }
                }
                GUILayout.EndScrollView();
            }
            _dragLineSeparator.EndLayout();
        }

        private void DrawChangeComponentInterface()
        {
            GUI.changed = false;
            _changeComponentInterface = GUILayoutHelper.PopupTypeField("Change Data Interface", _changeComponentInterface);
            if (GUI.changed)
            {
                UdpateInputParameters();
            }
        }

        private void DrawSourceTarget()
        {
            _isGlobalSearch = EditorGUILayout.Toggle("Search All In Project", _isGlobalSearch);
            if (!_isGlobalSearch)
            {
                EditorGUILayout.ObjectField("Search Target", _sourceTarget, typeof(Object), true);
            }
        }

        private void DrawInputParameters()
        {
            GUILayout.BeginVertical("box");
            {
                _inputComponentType = EditorGUILayout.TextField("ComponentTypeName", _inputComponentType);
                ChangeComponentDataHelper.DrawValueInput(_autoValue, _selectValueType, _unityObjectType);
                
                GUILayoutHelper.DrawStringList(_inputPropertyPaths, "PropertyPath", null, () => 
                {
                    if (GUILayout.Button("PrintProperty"))
                    {
                        var findType = GetLastFiledType();
                        if (null == findType)
                            findType = _inputComponentType.ToType();

                        if (null != findType)
                        {
                            var printInformation = GetSerializableInformation(findType);
                            Debug.Log(printInformation);
                        }
                        else
                            Debug.Log("ChangeComponentDataWindow DrawInputParameters error: not found type=" + _inputComponentType);
                    }
                });
                _changeComponentInterface.DrawCustomSearchCondition();
            }
            GUILayout.EndVertical();
        }

        private void DrawUpdateButton()
        {
            GUILayout.BeginHorizontal();
            {
                if (!string.IsNullOrEmpty(_inputComponentType) && !_inputPropertyPaths.IsNullOrEmpty() && GUILayout.Button("Update"))
                {
                    UpdateTargetComponent();
                }
                DrawChangeButton();
            }
            GUILayout.EndHorizontal();
        }

        private void DrawChangeButton()
        {
            if (!_inputPropertyPaths.IsNullOrEmpty() && _selectValueType >= 0 && GUILayout.Button("Change"))
            {
                try
                {
                    _isChanging = true;
                    bool shouldCancel = false;
                    shouldCancel = EditorUtility.DisplayCancelableProgressBar("change datas", "please wait...", 0);
                    shaco.Base.Coroutine.Foreach(_targetObjects, (object data) =>
                    {
                        var iter = (KeyValuePair<Object, TargetObjectInfo>)data;

                        //如果是场景需要特殊处理
                        if (IsSceneAsset(iter.Value.target))
                        {
                            var dependenciesTmp = CollectionSceneDependencies(_sourceTarget, new List<string>() { AssetDatabase.GetAssetPath(iter.Value.target) }, _isGlobalSearch);
                            foreach (var iter2 in dependenciesTmp)
                            {
                                var newItem = new TargetObjectInfo();
                                newItem.target = iter2;
                                newItem.components = GetComponentsWithDependencies(newItem.target);
                                SetComponentValue(newItem);
                            }
                        }
                        else
                        {
                            SetComponentValue(iter.Value);
                        }
                        return !shouldCancel;
                    }, (float percent) =>
                    {
                        shouldCancel = EditorUtility.DisplayCancelableProgressBar("change datas", "please wait...", percent);

                        if (percent >= 1)
                        {
                            _isChanging = false;
                            EditorUtility.ClearProgressBar();
                        }
                    }, _targetObjects.Count > MAX_SHOW_COUNT ? 0.02f : 0.1f);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("ChangeComponentDataWindow Change exception: e=" + e);
                    EditorUtility.ClearProgressBar();
                }
            }
        }

        private void UdpateInputParameters()
        {
            if (null == _changeComponentInterface)
                return;

            _inputComponentType = _changeComponentInterface.GetSearchComponentTypeName();
            _inputPropertyPaths = _changeComponentInterface.GetSerachPropertyNames();
            _selectValueType = _changeComponentInterface.GetChangePropertyValueType();
            if (_selectValueType == ChangeComponentDataHelper.ValueType.UnityObject)
                _unityObjectType = GetLastFiledType();
        }

        private List<Component> GetComponentsWithDependencies(Object obj)
        {
            var retValue = new List<Component>();
            var gameObjTmp = obj as GameObject;
            if (null == gameObjTmp)
            {
                return retValue;
            }
            var comopnents = gameObjTmp.GetComponentsInChildren(typeof(Component));
            if (null == comopnents)
            {
                return retValue;
            }

            var pathTmp = AssetDatabase.GetAssetPath(_sourceTarget);
            for (int i = 0; i < comopnents.Length; ++i)
            {
                if (null == comopnents[i])
                    continue;

                var typeString = comopnents[i].GetType().ToString();
                if (typeString != _inputComponentType)
                {
                    continue;
                }

                SerializedObject serializedObject = new SerializedObject(comopnents[i]);
                var propertyTmp = ChangeComponentDataHelper.FindPropertyValue(serializedObject, _inputPropertyPaths.ToArray());
                if (null == propertyTmp)
                {
                    continue;
                }

                if (_isGlobalSearch || (propertyTmp.propertyType != SerializedPropertyType.ObjectReference || AssetDatabase.GetAssetPath(propertyTmp.objectReferenceValue) == pathTmp))
                {
                    retValue.Add(comopnents[i]);
                }
            }
            return retValue;
        }

        private void DrawSearchName()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Search Type Name");
                _searchName = GUILayoutHelper.SearchField(_searchName, GUILayout.Width(Screen.width / 3 * 1));
            }
            GUILayout.EndHorizontal();
        }

        private bool IsSceneAsset(Object asset)
        {
#if UNITY_5_3_OR_NEWER
            return null != asset as SceneAsset;
#else
            var pathTmp = AssetDatabase.GetAssetPath(asset);
            return shaco.Base.FileHelper.GetFilNameExtension(pathTmp) == "unity";
#endif
        }

        private void DrawTarget(TargetObjectInfo targetInfo)
        {
            if (GUILayoutHelper.DrawHeader(targetInfo.target.name, targetInfo.target.name, () => { EditorGUILayout.ObjectField(targetInfo.target, typeof(Object), true); }))
            {
                for (int i = 0; i < targetInfo.components.Count; ++i)
                {
                    if (null == targetInfo.components[i])
                        continue;

                    var typeString = targetInfo.components[i].GetType().ToString();
                    if (typeString != _inputComponentType)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(_searchName))
                    {
                        if (!typeString.ToLower().Contains(_searchName.ToLower()))
                        {
                            continue;
                        }
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.ObjectField(targetInfo.components[i], typeof(Object), true);
                        if (GUILayout.Button("Print"))
                        {
                            EditorHelper.PrintSerializedObjectProperties(targetInfo.components[i]);
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        private void Init()
        {
            _isGlobalSearch = true;
            _dragLineSeparator.SetSplitWindow(this, 0.5f, 0.5f);
            UdpateInputParameters();
        }

        private System.Type GetLastFiledType()
        {
            System.Type nextChildType = null;
            if (_inputPropertyPaths.IsNullOrEmpty())
                return nextChildType;

            var componentTypeTmp = _inputComponentType.ToType();
            if (null == componentTypeTmp)
                return nextChildType;

            nextChildType = componentTypeTmp;
            var reflectionBindingFlags = System.Reflection.BindingFlags.Public
                                        | System.Reflection.BindingFlags.Instance
                                        | System.Reflection.BindingFlags.DeclaredOnly
                                        | System.Reflection.BindingFlags.NonPublic
                                        | System.Reflection.BindingFlags.Static;

            foreach (var iter in _inputPropertyPaths)
            {
                if (string.IsNullOrEmpty(iter))
                    continue;

                System.Type currentFindType = null;

                //Unity的序列化对象不支持查找Property，如果重构内部结构去Apply一个prefab比较麻烦，所以暂时屏蔽这里
                // var propertyFind = nextChildType.GetProperty(iter, reflectionBindingFlags);
                // if (null == propertyFind)
                {
                    var filedFind = nextChildType.GetField(iter, reflectionBindingFlags);
                    if (null != filedFind)
                        currentFindType = filedFind.FieldType;
                }
                // else
                    // currentFindType = propertyFind.PropertyType;
                    
                if (null == currentFindType)
                {
                    Debug.LogError("ChangeComponentDataWindow GetLastFiledType error: not found property or filed, name=" + iter + " parent type=" + nextChildType.GetType().FullName);
                    nextChildType = null;
                    break;
                }
                else
                    nextChildType = currentFindType;
            }
            return nextChildType;
        }

        private string GetSerializableInformation(System.Type type)
        {
            var reflectionBindingFlags = System.Reflection.BindingFlags.Public
                                        | System.Reflection.BindingFlags.Instance
                                        | System.Reflection.BindingFlags.DeclaredOnly
                                        | System.Reflection.BindingFlags.NonPublic
                                        | System.Reflection.BindingFlags.Static;
                                        
            var retValue = new System.Text.StringBuilder();
            var properties = type.GetProperties(reflectionBindingFlags);

            // retValue.AppendFormat("Properties[{0}]:\n", properties.Length);
            // foreach (var iter in properties)
            // {
            //     retValue.AppendFormat("{0} 【{1}】\n", iter.PropertyType, iter.Name);
            // }

            var fields = type.GetFields(reflectionBindingFlags);
            retValue.AppendFormat("Fields[{0}]:\n", fields.Length);
            foreach (var iter in fields)
            {
                retValue.AppendFormat("{0} {1} 【{2}】\n",
                                        iter.IsPublic ? "public" : iter.IsPrivate ? "private" : "protected",
                                        iter.FieldType, iter.Name);
            }

            return retValue.ToString();
        }

        private bool SetSourceTarget(Object sourceTarget, List<Object> objs)
        {
            if ((sourceTarget == null || objs.IsNullOrEmpty()))
                return true;

            _sourceTarget = sourceTarget;
            _inputTargetObjects = objs;
            _inputComponentType = _sourceTarget.GetType().ToTypeString();
            return _inputTargetObjects.Count > 0;
        }

        //根据输入的属性名字，查找包含该属性的组件 
        private void UpdateTargetComponent()
        {
            _targetObjects.Clear();

            if (_isGlobalSearch)
            {
                var directoryInfo = new System.IO.DirectoryInfo(Application.dataPath);
                var findAllPrefabs = AssetDatabase.FindAssets("t:prefab", new string[] { directoryInfo.Name });
                if (!findAllPrefabs.IsNullOrEmpty())
                {
                    AddTargetObjectToCache(findAllPrefabs.ConvertList<string, Object>((v) => AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(v)) ));
                    var scenes = EditorHelper.GetEnabledEditorScenes();
                    var scenesList = new List<Object>();
                    for (int i = 0; i < scenes.Length; ++i)
                    {
                        scenesList.Add(AssetDatabase.LoadAssetAtPath(scenes[i], typeof(Object)));
                    }
                    AddTargetObjectToCache(scenesList);
                }
            }
            else 
            {
                AddTargetObjectToCache(_inputTargetObjects);
            }

            var lastFiledType = GetLastFiledType();

            //当前选择类型根据实际数据类型自动修改
            if (null != lastFiledType)
            {
                if (lastFiledType == typeof(string))
                    _selectValueType = ChangeComponentDataHelper.ValueType.String;
                else if (lastFiledType == typeof(bool))
                    _selectValueType = ChangeComponentDataHelper.ValueType.Bool;
                else if (lastFiledType == typeof(int))
                    _selectValueType = ChangeComponentDataHelper.ValueType.Int;
                else if (lastFiledType == typeof(float))
                    _selectValueType = ChangeComponentDataHelper.ValueType.Float;
                else if (lastFiledType == typeof(Object))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityObject;
                else if (lastFiledType == typeof(Vector2))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityVector2;
                else if (lastFiledType == typeof(Vector3))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityVector3;
                else if (lastFiledType == typeof(Vector4))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityVector4;
                else if (lastFiledType == typeof(Rect))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityRect;
                else if (lastFiledType == typeof(Color))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityColor;
                else if (lastFiledType == typeof(Bounds))
                    _selectValueType = ChangeComponentDataHelper.ValueType.UnityBounds;

                if (_selectValueType == ChangeComponentDataHelper.ValueType.UnityObject)
                {
                    _unityObjectType = GetLastFiledType();
                }
            }
        }

        private void AddTargetObjectToCache(IList<Object> targetObjects)
        {
            foreach (var iter in targetObjects)
            {
                if (_targetObjects.ContainsKey(iter))
                    continue;

                var newItem = new TargetObjectInfo();
                newItem.target = iter;
                newItem.components = GetComponentsWithDependencies(newItem.target);
                if (newItem.components.Count > 0 || IsSceneAsset(newItem.target) && _changeComponentInterface.IsCustomSearchCondition(newItem))
                {
                    _targetObjects.Add(newItem.target, newItem);
                }
            }
        }

        // //查找数组中含有场景的对象，并筛选出正确的对象
        // private void CheckTargets(List<Object> objs, out List<Object> normalTargets, out List<string> sceneTargetsPath)
        // {
        //     normalTargets = new List<Object>();
        //     sceneTargetsPath = new List<string>();
        //     for (int i = objs.Count - 1; i >= 0; --i)
        //     {
        //         var objTmp = objs[i];
        //         var scenePath = AssetDatabase.GetAssetPath(objTmp);
        //         var extensionTmp = shaco.Base.FileHelper.GetFilNameExtension(scenePath);
        //         if ("unity" == extensionTmp)
        //         {
        //             sceneTargetsPath.Add(scenePath);
        //         }
        //         else
        //         {
        //             normalTargets.Add(objs[i]);
        //         }
        //     }
        // }

        private void SetComponentValue(TargetObjectInfo targetInfo)
        {
            if (null == targetInfo.target)
                return;

            if (null == _changeComponentInterface)
            {
                Debug.LogError("ChangeComponentDataWindow SetComponentValue error: not set change component delegate");
                return;
            }

            var components = targetInfo.components;
            if (components.Count == 0)
                return;

            Object findComponent = null;
            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (null != components[i] && components[i].GetType().ToTypeString() == _inputComponentType)
                {
                    findComponent = components[i];

                    SerializedObject serializedObject = new SerializedObject(findComponent);
                    var propertyTmp = ChangeComponentDataHelper.FindPropertyValue(serializedObject, _inputPropertyPaths.ToArray());
                    _changeComponentInterface.ChangePropertyValue(findComponent, serializedObject, propertyTmp, _autoValue);
                }
            }
        }

        //收集场景中包含的引用
        static private List<Object> CollectionSceneDependencies(Object sourceTarget, List<string> scenePaths, bool isGlobalSearch)
        {
            List<Object> retValue = new List<Object>();
            for (int i = 0; i < scenePaths.Count; ++i)
            {
                EditorHelper.SaveCurrentScene();
                EditorHelper.OpenScene(scenePaths[i]);
#if UNITY_5_3_OR_NEWER
                var rootsTmp = Resources.FindObjectsOfTypeAll<GameObject>();
#else
                var rootsTmp = GameObject.FindObjectsOfTypeAll(typeof(GameObject)).ToArrayConvert<Object, GameObject>();
#endif

                for (int j = 0; j < rootsTmp.Length; ++j)
                {
                    shaco.UnityHelper.ForeachChildren(rootsTmp[j], (int index, GameObject child) =>
                    {
                        var listDependence = EditorUtility.CollectDependencies(new Object[] { child });
                        if (isGlobalSearch)
                        {
                            retValue.Add(child);
                        }
                        else 
                        {
                            for (int k = 0; k < listDependence.Length; ++k)
                            {
                                if (sourceTarget == listDependence[k])
                                {
                                    retValue.Add(child);
                                    break;
                                }
                            }
                        }
                        return true;
                    });
                }
            }
            return retValue;
        }
    }
}
