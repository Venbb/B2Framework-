#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        static public void DrawSeparatorLine()
        {
            GUILayout.Space(5);
            GUILayout.Box(string.Empty, GUILayout.ExpandWidth(true), GUILayout.Height(3));
            GUILayout.Space(5);
        }

        public static string SearchField(string value, params GUILayoutOption[] options)
        {
            //这种文本输入框不是特别美观，暂时不考虑使用
            // EditorGUILayout.BeginHorizontal();
            // {
            //     value = EditorGUILayout.TextField(value, new GUIStyle("SearchTextField"));
            //     if (!string.IsNullOrEmpty(value) && GUILayout.Button(string.Empty, new GUIStyle("SearchCancelButton")))
            //     {
            //         value = string.Empty;
            //         GUIUtility.keyboardControl = 0;
            //     }
            // }
            // EditorGUILayout.EndHorizontal();

            System.Reflection.MethodInfo info = typeof(EditorGUILayout).GetMethod("ToolbarSearchField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static, null, new System.Type[] { typeof(string), typeof(GUILayoutOption[]) }, null);
            if (info != null)
            {
                value = (string)info.Invoke(null, new object[] { value, options });
            }
            return value;
        }

        static public bool DrawHeader(string text, string key, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            return DrawHeader(text, key, false, onHeaderDrawCallBack, options);
        }

        static public bool DrawHeader(string text, string key, bool defaultShow, string backgroundStyle, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            if (string.IsNullOrEmpty(backgroundStyle))
                backgroundStyle = "TE NodeBackground";

            bool state = EditorPrefs.GetBool(key, defaultShow);
            // var oldColor = GUI.backgroundColor;

            // if (!state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);

            // text = "<b><size=11>" + text + "</size></b>";

            bool isEmptyText = string.IsNullOrEmpty(text);
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;

            GUILayout.BeginHorizontal(backgroundStyle, options);
            {
                GUILayout.BeginVertical();
                {
                    GUILayout.Space(4);

                    if (isEmptyText)
                    {
                        options = new GUILayoutOption[] { GUILayout.Width(50) };
                    }

                    var oldTextColor = EditorStyles.label.normal.textColor;
                    EditorStyles.label.normal.textColor = state ? Color.white : Color.grey;
                    {
                        if (GUILayout.Button(text, EditorStyles.label, options))
                        {
                            state = !state;
                            EditorPrefs.SetBool(key, state);
                            // GUI.FocusControl(string.Empty);
                        }
                    }
                    EditorStyles.label.normal.textColor = oldTextColor;
                }
                GUILayout.EndVertical();
                if (null != onHeaderDrawCallBack)
                {
                    onHeaderDrawCallBack();
                }
            }
            GUILayout.EndHorizontal();
            return state;
        }

        static public bool DrawHeader(string text, string key, bool defaultShow, System.Action onHeaderDrawCallBack = null, params GUILayoutOption[] options)
        {
            return DrawHeader(text, key, defaultShow, "TE NodeBackground", onHeaderDrawCallBack, options);
        }

        static public void DrawHeaderText(string text)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(5);
                GUILayout.Label(text);
            }
            GUILayout.EndVertical();
        }

        static public void ResetPopTypeField()
        {
            shaco.GameHelper.datasave.RemoveStartWith("PopupTypeField+Types_");
        }

        /// <summary>
        /// 类型输入弹出框
        /// <param name="prefix">前缀描述</param>
        /// <param name="select">当前选择</param>
        /// <param name="createFunction">实例化对象函数，如果为空默认使用shaco.Base.Utility.Instantiate进行实例化</param>
        /// <param name="ignoreTypes">需要忽略的类型</param>
        /// <return>当前选择</return>
        /// </summary>
        static public T PopupTypeField<T>(string prefix, T select, System.Func<System.Type, object> createFunction = null, params System.Type[] ignoreTypes)
        {
            return PopupTypeField<T>((object)prefix, select, createFunction, ignoreTypes);
        }
        static public T PopupTypeField<T>(GUIContent prefix, T select, System.Func<System.Type, object> createFunction = null, params System.Type[] ignoreTypes)
        {
            return PopupTypeField<T>((object)prefix, select, createFunction, ignoreTypes);
        }
        static public T PopupTypeField<T>(object prefix, T select, System.Func<System.Type, object> createFunction = null, params System.Type[] ignoreTypes)
        {
            var typesKey = "PopupTypeField+Types_" + prefix;
            var typeNames = shaco.GameHelper.datasave.ReadString(typesKey);
            string[] splitTypeNames = null;
            var selectIndex = -1;

            if (string.IsNullOrEmpty(typeNames))
            {
                splitTypeNames = GetPopupTypesName<T>(typesKey, ignoreTypes);
            }
            else
            {
                splitTypeNames = typeNames.Split(",");
            }

            if (null == splitTypeNames || splitTypeNames.Length == 0)
            {
                EditorGUI.BeginDisabledGroup(true);
                {
                    if (prefix is GUIContent)
                        EditorGUILayout.Popup((GUIContent)prefix, 0, new string[] { string.Format("Not implemented class '{0}'", typeof(T).ToTypeString()) });
                    else
                        EditorGUILayout.Popup(prefix.ToString(), 0, new string[] { string.Format("Not implemented class '{0}'", typeof(T).ToTypeString()) });
                }
                EditorGUI.EndDisabledGroup();
                return default(T);
            }

            //确认类型是否匹配，如果不匹配需要重新加载类型
            if (null != select)
            {
                selectIndex = splitTypeNames.IndexOf(select.ToTypeString());
            }
            else
            {
                shaco.GameHelper.datasave.Remove(typesKey);
            }

            //初始化当前选择
            if (selectIndex < 0 || splitTypeNames.IsNullOrEmpty())
            {
                splitTypeNames = GetPopupTypesName<T>(typesKey, ignoreTypes);
                selectIndex = 0;
                select = null != createFunction ? (T)createFunction(shaco.Base.Utility.GetClasses<T>(ignoreTypes)[selectIndex]) : (T)shaco.Base.Utility.Instantiate(splitTypeNames[selectIndex]);
            }

            GUI.changed = false;
            int currentSelectIndex = 0;
            if (prefix is GUIContent)
                currentSelectIndex = EditorGUILayout.Popup((GUIContent)prefix, selectIndex, splitTypeNames);
            else
                currentSelectIndex = EditorGUILayout.Popup(prefix.ToString(), selectIndex, splitTypeNames);
            if (currentSelectIndex != selectIndex)
            {
                select = null != createFunction ? (T)createFunction(shaco.Base.Utility.GetClasses<T>(ignoreTypes)[currentSelectIndex]) : (T)shaco.Base.Utility.Instantiate(splitTypeNames[currentSelectIndex]);
                shaco.GameHelper.datasave.Remove(typesKey);
            }

            //刷新一次类型
            if (GUI.changed)
            {
                GetPopupTypesName<T>(typesKey, ignoreTypes);
            }
            return select;
        }

        /// <summary>
        /// 获取编辑器弹出类型名字
        /// <return>类型名字</return>
        /// </summary>
        static private string[] GetPopupTypesName<T>(string typesKey, params System.Type[] ignoreTypes)
        {
            var strBuilder = new System.Text.StringBuilder();
            var classNamesTmp = shaco.Base.Utility.GetClassNames<T>(ignoreTypes);

            for (int i = 0; i < classNamesTmp.Length; ++i)
            {
                strBuilder.Append(classNamesTmp[i]);
                strBuilder.Append(",");
            }
            if (strBuilder.Length > 0)
            {
                strBuilder.Remove(strBuilder.Length - 1, 1);
            }

            var writeStringTmp = strBuilder.ToString();
            shaco.GameHelper.datasave.WriteString(typesKey, writeStringTmp);
            return writeStringTmp.Split(",");
        }
    }
}

#endif