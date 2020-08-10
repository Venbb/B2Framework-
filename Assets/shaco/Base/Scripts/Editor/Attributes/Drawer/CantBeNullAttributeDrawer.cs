using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [CustomPropertyDrawer(typeof(shaco.CantBeNullAttribute))]
    public class CantBeNullAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (IsNullProperty(property))
            {
                var oldColor = GUI.color;
                GUI.color = Color.red;
                EditorGUI.PropertyField(position, property);
                GUI.color = oldColor;
            }
            else
            {
                EditorGUI.PropertyField(position, property);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Rect)
                return 32;
            else
                return 16;
        }

        private bool IsNullProperty(SerializedProperty property)
        {
            bool retValue = false;
            switch (property.propertyType)
            {
                case SerializedPropertyType.AnimationCurve: retValue = null == property.animationCurveValue; break;
                case SerializedPropertyType.ObjectReference: retValue = null == property.objectReferenceValue; break;
                case SerializedPropertyType.String: retValue = string.IsNullOrEmpty(property.stringValue); break;
                default: /*ignore*/ break;
            }
            return retValue;
        }
    }
}