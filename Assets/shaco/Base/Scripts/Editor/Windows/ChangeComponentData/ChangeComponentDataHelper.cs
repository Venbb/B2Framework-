using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public static class ChangeComponentDataHelper
    {
		public enum ValueType
		{
            None = -1,
			String = 0,
			Bool = 1,
            Int = 2,
			Float = 3,
			UnityObject = 4,
			UnityVector2 = 5,
            UnityVector3 = 6,
            UnityVector4 = 7,
            UnityRect = 8,
            UnityColor = 9,
            UnityBounds = 10,
        }

        static private readonly string[] VALUE_TYPES = new string[]
        {
            "String", "Bool", "Int", "Float",
            "UnityObject", "UnityVector2" , "UnityVector3" , "UnityVector4", 
			"UnityRect", "UnityColor" , "UnityBounds"
        };

        /// <summary>
        /// 绘制自动类型输出对象
        /// <param name="autoValue">自动类型数据</param>
        /// <param name="valueType">期望类型</param>
        /// <param name="unityObjectType">Unity对象类型，当valueType为UnityObject类型时候生效</param>
        /// <return>当前数据</return>
        /// </summary>
        static public ValueType DrawValueInput(shaco.AutoValue autoValue, ValueType valueType, System.Type unityObjectType)
        {
            ValueType retValue = valueType;
            EditorGUI.BeginDisabledGroup(true);
            {
                retValue = (ValueType)EditorGUILayout.Popup("Value Type", (int)retValue, VALUE_TYPES);
            }
            EditorGUI.EndDisabledGroup();

            switch (valueType)
            {
                case ValueType.Bool: autoValue.Set(EditorGUILayout.Toggle(autoValue)); break;
                case ValueType.Int: autoValue.Set(EditorGUILayout.IntField(autoValue)); break;
                case ValueType.Float: autoValue.Set(EditorGUILayout.FloatField(autoValue)); break;
                case ValueType.String: autoValue.Set(EditorGUILayout.TextArea(autoValue)); break;
                case ValueType.UnityObject: autoValue.Set(EditorGUILayout.ObjectField(autoValue, null == unityObjectType ? typeof(Object) : unityObjectType, true)); break;
                case ValueType.UnityVector2: autoValue.Set(EditorGUILayout.Vector2Field(string.Empty, autoValue)); break;
                case ValueType.UnityVector3: autoValue.Set(EditorGUILayout.Vector3Field(string.Empty, autoValue)); break;
                case ValueType.UnityVector4: autoValue.Set(EditorGUILayout.Vector4Field(string.Empty, autoValue)); break;
                case ValueType.UnityRect: autoValue.Set(EditorGUILayout.RectField(autoValue)); break;
                case ValueType.UnityColor: autoValue.Set(EditorGUILayout.ColorField(autoValue)); break;
                case ValueType.UnityBounds: autoValue.Set(EditorGUILayout.BoundsField(autoValue)); break;
                default: Debug.LogError("ChangeComponentDataHelper DrawValueInput error: unsupport type=" + valueType.ToString()); break;
            }
			return retValue;
        }

        /// <summary>
        /// 获取对象的值
        /// <param name="target">对象</param>
        /// <param name="propertyPath">属性名字</param>
        /// <return>属性值</return>
        /// </summary>
        static public object GetSerializedPropertyValue(UnityEngine.Object target, params string[] propertyPaths)
        {
			object retValue = null;
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty serializedProperty = FindPropertyValue(serializedObject, propertyPaths);
			if (null == serializedProperty)
			{
				Debug.LogError("ChangeComponentDataHelper GetSerializedPropertyValue error: not found property by name=" + propertyPaths.ToSerializeString() + " target=" + target);
				return retValue;
			}

            switch (serializedProperty.propertyType)
            {
                case SerializedPropertyType.Boolean: retValue = serializedProperty.boolValue; break;
                case SerializedPropertyType.Integer: retValue = serializedProperty.intValue; break;
                case SerializedPropertyType.Float: retValue = serializedProperty.floatValue; break;
                case SerializedPropertyType.String: retValue = serializedProperty.stringValue; break;
                case SerializedPropertyType.ObjectReference: retValue = serializedProperty.objectReferenceValue; break;
                case SerializedPropertyType.Vector2: retValue = serializedProperty.vector2Value; break;
                case SerializedPropertyType.Vector3: retValue = serializedProperty.vector3Value; break;
                case SerializedPropertyType.Vector4: retValue = serializedProperty.vector4Value; break;
                case SerializedPropertyType.Rect: retValue = serializedProperty.rectValue; break;
                case SerializedPropertyType.Color: retValue = serializedProperty.colorValue; break;
                case SerializedPropertyType.Bounds: retValue = serializedProperty.boundsValue; break;
                default: Debug.LogError("ChangeComponentDataHelper GetSerializedPropertyValue error: unsupport type=" + serializedProperty.propertyType); break;
            }
			return retValue;
        }

        /// <summary>
        /// 设置对象的值
        /// <param name="unityObject">Unity对象</param>
        /// <param name="target">序列化对象</param>
        /// <param name="property">序列化对象参数</param>
        /// <param name="autoValue">自动类型数据</param>
        /// </summary>
        static public void SetSerializedPropertyValue(UnityEngine.Object unityObject, SerializedObject target, SerializedProperty property, shaco.AutoValue autoValue)
        {
            if (null == property)
            {
                Debug.LogError("ChangeComponentDataHelper SetSerializedPropertyValue error: property is null, target=" + target);
                return;
            }

            switch (property.propertyType)
            {
                case SerializedPropertyType.Boolean: property.boolValue = autoValue; break;
                case SerializedPropertyType.Integer: property.intValue = autoValue; break;
                case SerializedPropertyType.Float: property.floatValue = autoValue; break;
                case SerializedPropertyType.String: property.stringValue = autoValue; break;
                case SerializedPropertyType.ObjectReference:
                    {
                        property.objectReferenceValue = autoValue;
                        if (null == property.objectReferenceValue && autoValue.IsType(typeof(Texture2D)))
                        {
                            Texture2D texTmp = (Texture2D)autoValue;
                            var filePath = AssetDatabase.GetAssetPath(texTmp);
                            property.objectReferenceValue = AssetDatabase.LoadAssetAtPath(filePath, typeof(Sprite)) as Sprite;
                        }
                        break;
                    }
                case SerializedPropertyType.Vector2: property.vector2Value = autoValue; break;
                case SerializedPropertyType.Vector3: property.vector3Value = autoValue; break;
                case SerializedPropertyType.Vector4: property.vector4Value = autoValue; break;
                case SerializedPropertyType.Rect: property.rectValue = autoValue; break;
                case SerializedPropertyType.Color: property.colorValue = autoValue; break;
                case SerializedPropertyType.Bounds: property.boundsValue = autoValue; break;
                default: Debug.LogError("ChangeComponentDataHelper SetSerializedPropertyValue error: unsupport type=" + property.propertyType); break;
            }
            target.ApplyModifiedProperties();
            EditorHelper.SetDirty(unityObject);
        }

        /// <summary>
        /// 设置对象的值
        /// <param name="target">对象</param>
        /// <param name="propertyPath">属性名字</param>
        /// <param name="autoValue">自动类型数据</param>
        /// </summary>
        static public void SetSerializedPropertyValue(UnityEngine.Object target, shaco.AutoValue autoValue, params string[] propertyPaths)
        {
			SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty serializedProperty = FindPropertyValue(serializedObject, propertyPaths);

            if (null == serializedProperty)
            {
                Debug.LogError("ChangeComponentDataHelper SetSerializedPropertyValue error: not found property by name=" + propertyPaths.ToSerializeString() + " target=" + target);
                return;
            }
            else 
            {
                SetSerializedPropertyValue(target, serializedObject, serializedProperty, autoValue);
            }
        }

        static public SerializedProperty FindPropertyValue(SerializedObject target, params string[] propertyPaths)
        {
            SerializedProperty retValue = null;

            if (propertyPaths.IsNullOrEmpty())
                return retValue;

            retValue = target.FindProperty(propertyPaths[0]);
            if (null != retValue)
            {
                for (int i = 1; i < propertyPaths.Length; ++i)
                {
                    retValue = retValue.FindPropertyRelative(propertyPaths[i]);
                    if (null == retValue)
                        break;
                }
            }
            
            return retValue;
        }
    }
}