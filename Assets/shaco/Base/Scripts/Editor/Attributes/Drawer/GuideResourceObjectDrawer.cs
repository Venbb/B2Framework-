using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class GuideResourceObjectDrawer<T> : ICustomValueDrawer where T : UnityEngine.Object
    {
        private class ReflectionInfo
        {
            public System.Reflection.PropertyInfo propertyPath = null;
            public System.Reflection.PropertyInfo propertyType = null;
            public System.Reflection.PropertyInfo propertySetValue = null;
            public System.Reflection.FieldInfo fieldValue = null;
        }

        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        virtual public System.Type valueType { get { return typeof(shaco.GuideResourceObject<T>); } }

        private Color _colorWarning = Color.yellow;
        private Color _colorError = Color.red;

        private Dictionary<System.Type, ReflectionInfo> _reflectionInfos = new Dictionary<System.Type, ReflectionInfo>();

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <return>当前数据</return>
        /// </summary>
        virtual public object DrawValue(string name, object value, System.Type type, object customArg)
        {
            return DrawValueBase(name, value, type, customArg, false);
        }

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <param name="allowSceneObjects">是否允许场景内动态对象</param>
        /// <return>当前数据</return>
        /// </summary>
        protected object DrawValueBase(string name, object value, System.Type type, object customArg, bool allowSceneObjects)
        {
            if (null == value)
                value = type.Instantiate();


            //为了防止泛型对象频繁反射获取属性，这里需要缓存一下提高效率
            ReflectionInfo findInfo = null;
            if (!_reflectionInfos.TryGetValue(type, out findInfo))
            {
                var bindingFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;
                findInfo = new ReflectionInfo();
                findInfo.propertyPath = type.GetProperty("path", bindingFlags);
                findInfo.propertyType = type.GetProperty("valueType", bindingFlags);
                findInfo.fieldValue = type.GetField("_value", bindingFlags | System.Reflection.BindingFlags.NonPublic);
                findInfo.propertySetValue = type.GetProperty("value", bindingFlags | System.Reflection.BindingFlags.NonPublic);
                _reflectionInfos.Add(type, findInfo);
            }

            var valueTmp = (UnityEngine.Object)findInfo.fieldValue.GetValue(value);
            var pathTmp = (string)findInfo.propertyPath.GetValue(value, null);
            var safePathTmp = string.IsNullOrEmpty(pathTmp) ? "[NoSet]" : pathTmp;
            var objectTypeTmp = (System.Type)findInfo.propertyType.GetValue(value, null);

            GUILayout.BeginHorizontal();
            {
                Color oldColor = GUI.color;
                if (null == valueTmp)
                    GUI.color = (objectTypeTmp.IsInherited<Component>() || objectTypeTmp.IsInherited<GameObject>()) && !string.IsNullOrEmpty(pathTmp) ? _colorWarning : _colorError;
                var changedValue = EditorGUILayout.ObjectField(new GUIContent(name, safePathTmp), valueTmp, objectTypeTmp, allowSceneObjects);
                if (null == valueTmp)
                    GUI.color = oldColor;

                if (null == valueTmp)
                {
                    GUI.changed = false;
                    EditorGUILayout.TextField(safePathTmp, GUILayout.Width(Screen.width * 0.2f));
                    if (GUI.changed)
                        GUI.FocusControl(string.Empty);
                }

                if (changedValue != valueTmp)
                {
                    findInfo.propertySetValue.SetValue(value, changedValue, null);
                    GUI.changed = true;
                }
            }
            GUILayout.EndHorizontal();
            return value;
        }
    }
}