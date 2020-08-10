using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace shaco.Instance.Editor.HeaderAttributeDrawer
{
	//Unity原生HeaderAttribute不支持enum枚举，故此重写了它的绘制方法
	[CustomPropertyDrawer(typeof(HeaderAttribute))]
	public class HeaderAttributeDrawer : PropertyDrawer
	{
		private string[] _displayEnumNames = null;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var att = (HeaderAttribute)attribute;
			var type = property.serializedObject.targetObject.GetType();
			var field = type.GetField(property.name, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static);

			if (null == field)
			{
				Debug.LogWarning("HeaderAttributeDrawer OnGUI warning: not found filed, parent type=" + type + " property name=" + property.name, property.serializedObject.targetObject);
				return;
			}
			var fieldType = field.FieldType;

            position.y += 6;

            //处理枚举
            if (fieldType.BaseType == typeof(System.Enum))
			{
				if (null == _displayEnumNames)
				{
                    _displayEnumNames = new string[property.enumNames.Length];
					for (int i = 0; i < property.enumNames.Length; ++i)
					{
						var enumName = property.enumNames[i];
						var enumfield = fieldType.GetField(enumName);
						var hds = enumfield.GetCustomAttributes(typeof(HeaderAttribute), false);
                        var customEnumName = (hds.Length <= 0 ? enumName : ((HeaderAttribute)hds[0]).header);

						if (null != System.Array.Find(_displayEnumNames, (v) => v == customEnumName))
                            Debug.LogError("HeaderAttributeDrawer OnGUI warning: has duplicate enum name=" + customEnumName + ", parent type=" + type + " property name=" + property.name, property.serializedObject.targetObject);
                        else
                            _displayEnumNames[i] = customEnumName;
					}
				}

                position.height = 16;
				GUI.Label(position, att.header);
                position.y += 16;

				EditorGUI.BeginChangeCheck();
				var value = EditorGUI.Popup(position, property.name, property.enumValueIndex, _displayEnumNames);
				if (EditorGUI.EndChangeCheck())
				{
					property.enumValueIndex = value;
				}
			}
			else
			{
                position.height = 16;
                GUI.Label(position, att.header);
                position.y += 16;
				EditorGUI.PropertyField(position, property, label);
			}
		}

        public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
		{
			return 32 + 6;
		}
    }
}