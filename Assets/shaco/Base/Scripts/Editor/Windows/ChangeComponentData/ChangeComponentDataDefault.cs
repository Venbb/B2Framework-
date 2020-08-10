using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ChangeComponentDataDefault : IChangeComponentData
    {
        public string GetSearchComponentTypeName()
        {
            return typeof(UnityEngine.UI.Image).ToTypeString();
        }

        public List<string> GetSerachPropertyNames()
        {
            return new List<string>() { "m_Sprite" };
        }

        public ChangeComponentDataHelper.ValueType GetChangePropertyValueType()
        {
            return ChangeComponentDataHelper.ValueType.UnityObject;
        }

        public void ChangePropertyValue(UnityEngine.Object unityObject, SerializedObject obj, SerializedProperty property, shaco.AutoValue autoValue)
		{
            ChangeComponentDataHelper.SetSerializedPropertyValue(unityObject, obj, property, autoValue);
		}

        public void DrawCustomSearchCondition()
        {

        }

        public bool IsCustomSearchCondition(ChangeComponentDataWindow.TargetObjectInfo targetInfo)
        {
            return true;
        }
    }
}