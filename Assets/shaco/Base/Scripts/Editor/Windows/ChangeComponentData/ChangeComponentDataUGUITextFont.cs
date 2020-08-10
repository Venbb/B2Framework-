using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class ChangeComponentDataUGUITextFont : IChangeComponentData
    {
        public string GetSearchComponentTypeName()
        {
            return typeof(UnityEngine.UI.Text).ToTypeString();
        }

        public List<string> GetSerachPropertyNames()
        {
            return new List<string>() { "m_FontData", "m_Font" };
        }

        public ChangeComponentDataHelper.ValueType GetChangePropertyValueType()
        {
            return ChangeComponentDataHelper.ValueType.UnityObject;
        }

        public void ChangePropertyValue(UnityEngine.Object unityObject, SerializedObject obj, SerializedProperty property, shaco.AutoValue autoValue)
        {
            var text = unityObject as UnityEngine.UI.Text;
            if (null == text)
            {
                Debug.LogError("ChangeComponentDataUGUITextFont ChangePropertyValue error: not UGUI text");
                return;
            }

            var setObj = (UnityEngine.Object)autoValue;
            if (null == setObj)
            {
                Debug.LogError("ChangeComponentDataUGUITextFont ChangePropertyValue error: set value is null");
                return;
            }

            var setFont = setObj as UnityEngine.Font;
            if (null == setFont)
            {
                Debug.LogError("ChangeComponentDataUGUITextFont ChangePropertyValue error: set value not 'Font', value=" + setObj);
                return;
            }
            text.font = setFont;
            EditorHelper.SetDirty(unityObject);
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