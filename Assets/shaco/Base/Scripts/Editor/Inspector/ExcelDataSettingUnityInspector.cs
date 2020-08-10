using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.ExcelDataSettingUnity))]
    public class ExcelDataSettingUnityInspector : Editor
    {
        private shaco.ExcelDataSettingUnity _target = null;
        private bool _hasValueChanged = false;
        private string _searchName = string.Empty;
        private string _searchNameLower = string.Empty;

        private List<shaco.ExcelDataSettingUnity.ExcelSettingInfo> _delayAddInfo = new List<shaco.ExcelDataSettingUnity.ExcelSettingInfo>();
        private List<shaco.ExcelDataSettingUnity.ExcelSettingInfo> _delayRemoveInfo = new List<shaco.ExcelDataSettingUnity.ExcelSettingInfo>();

        private readonly GUIContent GUI_CONTENT_EXPORT_SOURCE = new GUIContent("Export Source", "导出配置源文件目录，一般为xlsx或者csv文件\n其中csv支持在编辑器环境下直接代码读取");
        private readonly GUIContent GUI_CONTENT_EXPORT_SCRIPT = new GUIContent("Export Script", "导出配置脚本目录，自动化生成的读取配置脚本");
        private readonly GUIContent GUI_CONTENT_EXPORT_ASSET = new GUIContent("Export Asset", "导出配置序列化文件目录，主要用于编辑器或者真机环境使用，效率更高");
        private readonly GUIContent GUI_CONTENT_DATA_LOADER = new GUIContent("Data Loader", "excel数据加载类，支持自定义");

        void OnEnable()
        {
            _target = target as shaco.ExcelDataSettingUnity;
        }

        private void OnDestroy()
        {
            if (_hasValueChanged)
            {
                //数据发生修改后，在关闭该窗口时应该主动保存一次资源
                //否则unity本身不会保存文件，修改的asset数据还在内存中
                AssetDatabase.SaveAssets();
            }
        }

        public override void OnInspectorGUI()
        {
            Undo.RecordObject(target, target.GetType().FullName);
            shaco.ExcelDataSettingUnity.ExcelSettingInfo currentDrawDataInfo = null;

            var newRuntimeLoadFileType = (shaco.Base.ExcelData.RuntimeLoadFileType)EditorGUILayout.EnumPopup("Runtime Load File Type", _target.runtimeLoadFileType);
            if (_target.runtimeLoadFileType != newRuntimeLoadFileType)
            {
                _target.runtimeLoadFileType = newRuntimeLoadFileType;
                _hasValueChanged = true;
            }

            GUI.changed = false;
            var newLoader = GUILayoutHelper.PopupTypeField<shaco.Base.IExcelDataLoader>(GUI_CONTENT_DATA_LOADER, _target.dataLoader);
            if (newLoader != _target.dataLoader)
            {
                _target.dataLoader = newLoader;
                _hasValueChanged = true;
            }

            //绘制列表
            System.Action onAfterDrawHeaderCallBack = () =>
            {
                //绘制搜索框
                GUI.changed = false;
                _searchName = GUILayoutHelper.SearchField(_searchName, GUILayout.Width(Screen.width / 3 * 1));
                if (GUI.changed)
                {
                    _searchNameLower = _searchName.ToLower();
                }

                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    _delayAddInfo.Add(new shaco.ExcelDataSettingUnity.ExcelSettingInfo());
                    GUI.changed = true;
                }

                EditorGUI.BeginDisabledGroup(null == currentDrawDataInfo || _target.settingsInfo.Count <= 1);
                {
                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        if (!_delayRemoveInfo.Contains(currentDrawDataInfo))
                            _delayRemoveInfo.Add(currentDrawDataInfo);
                        GUI.changed = true;
                    }
                }
                EditorGUI.EndDisabledGroup();
            };

            for (int i = 0; i < _target.settingsInfo.Count; ++i)
            {
                var value = _target.settingsInfo[i];
                currentDrawDataInfo = value;
                var title = "Excel Settings(" + i + ")";

                GUI.changed = false;
                if (GUILayoutHelper.DrawHeader(title, title, onAfterDrawHeaderCallBack))
                {
                    value.customExportTextPath = GUILayoutHelper.PathField(GUI_CONTENT_EXPORT_SOURCE, value.customExportTextPath, string.Empty);
                    value.customExportScriptPath = GUILayoutHelper.PathField(GUI_CONTENT_EXPORT_SCRIPT, value.customExportScriptPath, string.Empty);
                    value.customExportAssetPath = GUILayoutHelper.PathField(GUI_CONTENT_EXPORT_ASSET, value.customExportAssetPath, string.Empty);
                    value.defalutDataType = (shaco.Base.ExcelData.TabelDataType)EditorGUILayout.EnumPopup("Default Data Type", value.defalutDataType);
                    value.customNamespace = EditorGUILayout.TextField("Namespace", value.customNamespace);
                    if (GUI.changed)
                    {
                        //数据修改后必须设置一次dirty，否则asset不会被修改
                        _hasValueChanged = true;
                        EditorUtility.SetDirty(_target);
                    }

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Space(10);
                        
                        GUILayout.BeginVertical();
                        {
                            DrawExcelDataList(value.customNamespace, value.excelDatasInfo);
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();
                }
            }

            if (_delayRemoveInfo.Count > 0)
            {
                foreach (var iter in _delayRemoveInfo)
                {
                    _target.settingsInfo.Remove(iter);
                }
                _delayRemoveInfo.Clear();

                //数据修改后必须设置一次dirty，否则asset不会被修改
                _hasValueChanged = true;
                EditorUtility.SetDirty(_target);
            }

            if (_delayAddInfo.Count > 0)
            {
                foreach (var iter in _delayAddInfo)
                {
                    _target.settingsInfo.Add(iter);
                }
                _delayAddInfo.Clear();

                //数据修改后必须设置一次dirty，否则asset不会被修改
                _hasValueChanged = true;
            }

            if (_hasValueChanged)
            {
                EditorUtility.SetDirty(_target);
            }
        }

        private void DrawExcelDataList(string title, List<shaco.ExcelDataSettingUnity.ExcelSettingInfo.ExcelDataInfo> excelDatasInfo)
        {
            System.Func<int, shaco.ExcelDataSettingUnity.ExcelSettingInfo.ExcelDataInfo, System.Action<shaco.ExcelDataSettingUnity.ExcelSettingInfo.ExcelDataInfo>, bool> onDrawValueCallBack = (index, value, onChangedValueCallBack) =>
            {
                if (!string.IsNullOrEmpty(_searchNameLower) && !value.path.Contains(_searchNameLower))
                    return true;

                GUILayoutHelper.DrawValue(string.Empty, value, typeof(shaco.ExcelDataSettingUnity.ExcelSettingInfo.ExcelDataInfo));
                return true;
            };

            GUI.changed = false;
            GUILayoutHelper.DrawListBase(excelDatasInfo, title, null, null, null, null, onDrawValueCallBack, null);
            if (GUI.changed)
            {
                //数据修改后必须设置一次dirty，否则asset不会被修改
                _hasValueChanged = true;
                EditorUtility.SetDirty(_target);
            }
        }
    }

    public class ExcelDataSettingUnityDrawer : ICustomValueDrawer
    {
        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        public System.Type valueType { get { return typeof(shaco.ExcelDataSettingUnity.ExcelSettingInfo.ExcelDataInfo); } }

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <return>当前数据</return>
        /// </summary>
        public object DrawValue(string name, object value, System.Type type, object customArg)
        {
            var settingInfo = value as shaco.ExcelDataSettingUnity.ExcelSettingInfo.ExcelDataInfo;
            GUILayout.BeginVertical("box");
            {
                var oldPath = settingInfo.path;
                settingInfo.path = GUILayoutHelper.PathField("File", settingInfo.path, shaco.Base.ExcelDefine.EXTENSION_TXT);
                if (settingInfo.path != oldPath)
                {
                    if (!IsSupportPath(settingInfo.path))
                    {
                        settingInfo.path = oldPath;
                    }
                }
                settingInfo.dataType = (shaco.Base.ExcelData.TabelDataType)EditorGUILayout.EnumPopup("Data Type", settingInfo.dataType);
            }
            GUILayout.EndVertical();
            return value;
        }

        private bool IsSupportPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("ExcelDataSettingUnityDrawer IsSupportPath error: path is empty");
                return false;
            }

            if (!EditorHelper.IsUnityAssetPath(path))
            {
                Debug.LogError("ExcelDataSettingUnityDrawer IsSupportPath error: only support unity asset");
                return false;
            }

            if (!path.EndsWith(shaco.Base.ExcelDefine.EXTENSION_TXT))
            {
                Debug.LogError("ExcelDataSettingUnityDrawer IsSupportPath error: only support extension=" + shaco.Base.ExcelDefine.EXTENSION_TXT);
                return false;
            }

            return true;
        }
    }
}