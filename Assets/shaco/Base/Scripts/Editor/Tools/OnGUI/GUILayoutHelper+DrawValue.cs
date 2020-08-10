#if UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class GUILayoutHelper
    {
        static private Dictionary<string, ICustomValueDrawer> _customValueDrawers = new Dictionary<string, ICustomValueDrawer>();
        static private bool _isInitedCustomValueDrawers = false;

        //绘制序列化属性
        static public void DrawObject(object obj, params System.Reflection.BindingFlags[] flags)
        {
            System.Reflection.BindingFlags flagTmp = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance;
            for (int i = 0; i < flags.Length; ++i)
            {
                flagTmp |= flags[i];
            }

            var fields = obj.GetType().GetFields(flagTmp);
            foreach (var field in fields)
            {
                DrawFieldInfo(obj, field);
            }
        }

        static public object DrawValue(string name, object value, System.Type type, object customArg = null)
        {
            return DrawValue(name, value, type, customArg, 0);
        }


        static public Quaternion QuaternionField(string label, Quaternion value, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            {
                value.x = EditorGUILayout.FloatField("x", value.x);
                value.y = EditorGUILayout.FloatField("y", value.y);
                value.z = EditorGUILayout.FloatField("z", value.z);
                value.w = EditorGUILayout.FloatField("w", value.w);
            }
            GUILayout.EndHorizontal();
            return value;
        }

        static private void DrawFieldInfo(object obj, System.Reflection.FieldInfo field)
        {
            var valueTmp = field.GetValue(obj);

            GUI.changed = false;
            valueTmp = DrawValue(field.Name, valueTmp, field.FieldType, 0);
            if (GUI.changed)
            {
                field.SetValue(obj, valueTmp);
            }
        }

        static private object DrawValue(string name, object value, System.Type type, object customArg, int deepIndex)
        {
            if (null == value)
            {
                value = type.Instantiate();

                if (null != value)
                    GUI.changed = true;
            }

            if (type == typeof(bool))
            {
                value = EditorGUILayout.Toggle(name, (bool)value);
            }
            else if (type == typeof(char))
            {
                value = EditorGUILayout.TextField(name, ((char)value).ToString()).ToChar();
            }
            else if (type == typeof(short))
            {
                value = EditorGUILayout.IntField(name, (short)value);
            }
            else if (type == typeof(int))
            {
                value = EditorGUILayout.IntField(name, (int)value);
            }
            else if (type == typeof(long))
            {
#if UNITY_5_3_OR_NEWER
                value = EditorGUILayout.LongField(name, (long)value);
#else
                value = EditorGUILayout.IntField(name, (int)value);
#endif
            }
            else if (type == typeof(float))
            {
                value = EditorGUILayout.FloatField(name, (float)value);
            }
            else if (type == typeof(double))
            {
#if UNITY_5_3_OR_NEWER
                value = EditorGUILayout.DoubleField(name, (double)value);
#else
                value = EditorGUILayout.FloatField(name, (float)value);
#endif
            }
            else if (type == typeof(string))
            {
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(name, GUILayout.Width(146));
                    value = EditorGUILayout.TextArea((string)value);
                }
                GUILayout.EndHorizontal();
            }
            else if (type.IsInherited<System.Enum>())
            {
                value = EditorGUILayout.EnumPopup(name, (System.Enum)value);
            }
            else if (type.IsInherited<UnityEngine.Object>())
            {
                value = EditorGUILayout.ObjectField(name, (UnityEngine.Object)value, type, true);
            }
            else if (type == typeof(UnityEngine.Vector2))
            {
                value = EditorGUILayout.Vector2Field(name, (UnityEngine.Vector2)value);
            }
            else if (type == typeof(UnityEngine.Vector3))
            {
                value = EditorGUILayout.Vector3Field(name, (UnityEngine.Vector3)value);
            }
            else if (type == typeof(UnityEngine.Vector4))
            {
                value = EditorGUILayout.Vector4Field(name, (UnityEngine.Vector4)value);
            }
            else if (type == typeof(UnityEngine.Vector2Int))
            {
                value = EditorGUILayout.Vector2IntField(name, (UnityEngine.Vector2Int)value);
            }
            else if (type == typeof(UnityEngine.Vector3Int))
            {
                value = EditorGUILayout.Vector3IntField(name, (UnityEngine.Vector3Int)value);
            }
            else if (type == typeof(UnityEngine.Rect))
            {
                value = EditorGUILayout.RectField(name, (UnityEngine.Rect)value);
            }
            else if (type == typeof(UnityEngine.Color))
            {
                value = EditorGUILayout.ColorField(name, (UnityEngine.Color)value);
            }
            else if (type == typeof(UnityEngine.Bounds))
            {
                value = EditorGUILayout.BoundsField(name, (UnityEngine.Bounds)value);
            }
            else if (type == typeof(UnityEngine.Quaternion))
            {
                value = GUILayoutHelper.QuaternionField(name, (UnityEngine.Quaternion)value);
            }
            else if (type == typeof(System.Object))
            {
                return DrawValue(name, value.ToString(), typeof(string));
            }
            else if (type.IsInherited<System.Collections.ICollection>())
            {
                var collection = value as System.Collections.ICollection;
                bool isOpen = true;
                int count = 0;
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 * (deepIndex + 1));
                    var keyTmp = name + GetTypeDisplayName(type);
                    isOpen = GUILayoutHelper.DrawHeader(keyTmp, keyTmp);
                }
                GUILayout.EndHorizontal();

                if (isOpen)
                {
                    if (collection is IList)
                    {
                        DrawValueList(collection as IList, count, deepIndex);
                    }
                    else if (collection is IDictionary)
                    {
                        DrawValueDictionary(collection as IDictionary, count, deepIndex);
                    }
                    else
                    {
                        value = DrawCustomValue(name, value, type, customArg);
                    }
                }
            }
            else
            {
                value = DrawCustomValue(name, value, type, customArg);
            }

            return value;
        }

        static private object DrawCustomValue(string name, object value, System.Type type, object customArg)
        {
            ICustomValueDrawer findDrawer = null;

            if (!_isInitedCustomValueDrawers)
            {
                _isInitedCustomValueDrawers = true;
                var customValueDrawers = shaco.Base.Utility.GetTypes<ICustomValueDrawer>(true, typeof(ICustomValueDrawer));
                if (!customValueDrawers.IsNullOrEmpty())
                {
                    for (int i = customValueDrawers.Length - 1; i >= 0; --i)
                    {
                        var typeTmp = customValueDrawers[i];

                        if (typeTmp.IsInterface || typeTmp.IsAbstract)
                        {
                            Debug.LogError("GUILayoutHelper+DrawValue DrawCustomValue error: can't support 'interface' or 'abstrct class', type=" + typeTmp.ToTypeString(), value as Object);
                            continue;
                        }

                        if (!typeTmp.IsGenericType)
                        {
                            var valueDrawer = typeTmp.Instantiate() as ICustomValueDrawer;
                            var baseTypeString = valueDrawer.valueType.ToTypeString();
                            if (_customValueDrawers.ContainsKey(baseTypeString))
                                Debug.LogError("GUILayoutHelper+DrawValue DrawCustomValue error: type=" + baseTypeString, value as Object);
                            _customValueDrawers.Add(baseTypeString, valueDrawer);
                        }
                        else
                        {
                            var valueDrawer = shaco.Base.Utility.InstantiateGeneric(typeTmp, typeof(UnityEngine.Object)) as ICustomValueDrawer;
                            var baseTypeString = shaco.Base.Utility.GetGenericBaseTypeString(valueDrawer.valueType);
                            if (_customValueDrawers.ContainsKey(baseTypeString))
                                Debug.LogError("GUILayoutHelper+DrawValue DrawCustomValue error: base type=" + valueDrawer.valueType + " type=" + baseTypeString, value as Object);
                            _customValueDrawers.Add(baseTypeString, valueDrawer);
                        }
                    }
                }
            }

            var findTypeString = type.ToTypeString();
            if (type.IsGenericType)
                findTypeString = shaco.Base.Utility.GetGenericBaseTypeString(type);

            if (_customValueDrawers.TryGetValue(findTypeString, out findDrawer))
            {
                return findDrawer.DrawValue(name, value, type, customArg);
            }
            else
            {
                EditorGUILayout.HelpBox(string.Format("not support type '{0}' please set your custom drawer and inherit from '{1}'", type.FullName, typeof(ICustomValueDrawer).FullName), MessageType.Warning);
                EditorGUILayout.TextField(name, value == null ? "null" : value.ToString());
                return value;
            }
        }

        static private void DrawValueList(IList collection, int count, int deepIndex)
        {
            var listTmp = collection as IList;
            if (null == listTmp)
                return;

            List<int> listRemoveIndex = null;
            var listType = collection.GetType();
            System.Type elementType = listType.IsGenericType ? collection.GetType().GetGenericArguments()[0] : listType.GetElementType();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5 * (deepIndex + 1) + 16);

                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.IntField("Size", null == collection ? 0 : collection.Count);
                }
                EditorGUI.EndDisabledGroup();

                if (!collection.IsFixedSize && GUILayout.Button("+", GUILayout.Width(20)))
                {
                    listTmp.Add(elementType.Instantiate());
                }
            }
            GUILayout.EndHorizontal();

            for (int i = 0; i < listTmp.Count; ++i)
            {
                var item = listTmp[i];
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 * (deepIndex + 1) + 16);
                    GUILayout.BeginVertical();
                    {
                        listTmp[i] = DrawValue("Element " + i, item, elementType, item is ICollection ? deepIndex + 1 : deepIndex);
                    }
                    GUILayout.EndVertical();

                    if (GUILayout.Button("-", GUILayout.Width(20f)))
                    {
                        if (null == listRemoveIndex)
                            listRemoveIndex = new List<int>();
                        listRemoveIndex.Add(i);
                    }
                }
                GUILayout.EndHorizontal();
            }

            //delay remove
            if (!listTmp.IsFixedSize)
            {
                if (!listRemoveIndex.IsNullOrEmpty())
                {
                    for (int i = 0; i < listRemoveIndex.Count; ++i)
                    {
                        listTmp.RemoveAt(listRemoveIndex[i] - i);
                    }
                }
            }
        }

        static private void DrawValueDictionary(IDictionary collection, int count, int deepIndex)
        {
            var dicTmp = collection as IDictionary;
            if (null == dicTmp)
                return;

            var defalutTypes = dicTmp.GetType().GetGenericArguments();
            var keyType = defalutTypes[0];
            var valueType = defalutTypes[1];

            var listRemovedKeys = new List<object>();
            var listChangedKeys = new List<object>();
            var listChangedValues = new List<object>();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Space(5 * (deepIndex + 1) + 16);
                EditorGUI.BeginDisabledGroup(true);
                {
                    EditorGUILayout.IntField("Size", null == collection ? 0 : collection.Count);
                }
                EditorGUI.EndDisabledGroup();

                if (!dicTmp.IsFixedSize && GUILayout.Button("+", GUILayout.Width(20)))
                {
                    dicTmp.Add(keyType.Instantiate(), valueType.Instantiate());
                }
            }
            GUILayout.EndHorizontal();

            int index = 0;
            foreach (var key in dicTmp.Keys)
            {
                var value = dicTmp[key];

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(5 * (deepIndex + 1) + 16);
                    GUILayout.BeginVertical();
                    {
                        GUILayout.BeginHorizontal();
                        {
                            var keyTmp = DrawValue("Element " + index++, key, keyType, key is ICollection ? deepIndex + 1 : deepIndex);
                            var valueTmp = DrawValue(string.Empty, value, valueType, value is ICollection ? deepIndex + 1 : deepIndex);
                            if (GUI.changed)
                            {
                                listRemovedKeys.Add(key);
                                listChangedKeys.Add(keyTmp);
                                listChangedValues.Add(valueTmp);
                            }
                            if (GUILayout.Button("-", GUILayout.Width(20f)))
                            {
                                listRemovedKeys.Add(key);
                            }
                        }
                        GUILayout.EndHorizontal();

                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }

            //delay update
            if (listChangedKeys.Count > 0)
            {
                for (int i = 0; i < listChangedKeys.Count; ++i)
                {
                    var removeKey = listRemovedKeys[i];
                    var changedKey = listChangedKeys[i];
                    var changedValue = listChangedValues[i];

                    if (null != changedKey)
                    {
                        if (changedKey != removeKey)
                        {
                            dicTmp.Remove(removeKey);
                            dicTmp[changedKey] = changedValue;
                        }
                        else
                        {
                            dicTmp[changedKey] = changedValue;
                        }
                    }
                    else
                    {
                        dicTmp.Remove(removeKey);
                    }
                }
            }

            //delay delete
            if (listRemovedKeys.Count > 0 && listRemovedKeys.Count != listChangedKeys.Count)
            {
                for (int i = 0; i < listRemovedKeys.Count; ++i)
                {
                    dicTmp.Remove(listRemovedKeys[i]);
                }
            }
        }

        static private string GetTypeDisplayName(System.Type type)
        {
            return "[" + (type == null ? "null type" : type.ToString()) + "]";
        }
    }
}

#endif