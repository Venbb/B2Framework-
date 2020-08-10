using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomEditor(typeof(shaco.DataTypesTemplate))]
    public class DataTypesTemplateInspector : Editor
    {
        private shaco.DataTypesTemplate _target = null;
        private bool _hasValueChanged = false;
        private string _searchName = string.Empty;
        private string _searchNameLower = string.Empty;

        void OnEnable()
        {
            _target = target as shaco.DataTypesTemplate;
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

            //绘制搜索框
            // GUILayout.BeginHorizontal();
            // {
            //     GUILayout.Space(Screen.width / 3 * 2);

            // }
            // GUILayout.EndHorizontal();

            //绘制列表
            System.Action onAfterDrawHeaderCallBack = () =>
            {
                GUI.changed = false;
                _searchName = GUILayoutHelper.SearchField(_searchName, GUILayout.Width(Screen.width / 3 * 1));
                if (GUI.changed)
                {
                    _searchNameLower = _searchName.ToLower();
                }
            };
            System.Func<int, shaco.Base.StringTypePair, System.Action<shaco.Base.StringTypePair>, bool> onDrawValueCallBack = (index, value, onChangedValueCallBack) =>
            {
                if (!string.IsNullOrEmpty(_searchNameLower) && !value.customTypeString.Contains(_searchNameLower))
                    return true;
                GUILayoutHelper.DrawValue(value.customTypeString, value, value.GetType());
                return true;
            };

            GUI.changed = false;
            GUILayoutHelper.DrawListBase(_target.stringToTypePairs, "Types", null, null, null, onAfterDrawHeaderCallBack, onDrawValueCallBack, null);
            if (GUI.changed)
            {
                //数据修改后必须设置一次dirty，否则asset不会被修改
                _hasValueChanged = true;
                EditorUtility.SetDirty(_target);
            }
        }
    }

    public class DataTypesTemplateDrawer : ICustomValueDrawer
    {
        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        public System.Type valueType { get { return typeof(shaco.Base.StringTypePair); } }

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
            var typePair = value as shaco.Base.StringTypePair;
            GUILayout.BeginVertical("box");
            {
                typePair.customTypeString = EditorGUILayout.TextField("Custom Type Name", typePair.customTypeString);
                typePair.fullTypeName = EditorGUILayout.TextField("Param Type Name", typePair.fullTypeName);
                typePair.convertFunction = EditorGUILayout.TextField("Convert Scriipt", typePair.convertFunction);
            }
            GUILayout.EndVertical();
            return value;
        }
    }
}