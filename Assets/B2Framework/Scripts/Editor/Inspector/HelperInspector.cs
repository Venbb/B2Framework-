using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    internal sealed class HelperInspector<T> where T : MonoBehaviour
    {
        private const string CustomOptionName = "<Custom>";

        private readonly string m_Name;

        private SerializedProperty m_HelperTypeName;
        private SerializedProperty m_CustomHelper;
        private string[] m_HelperTypeNames;
        private int m_HelperTypeNameIndex;

        public HelperInspector(string name)
        {
            m_Name = name;

            m_HelperTypeName = null;
            m_CustomHelper = null;
            m_HelperTypeNames = null;
            m_HelperTypeNameIndex = 0;
        }

        public void Init(SerializedObject serializedObject)
        {
            m_HelperTypeName = serializedObject.FindProperty(string.Format("m_{0}HelperTypeName", m_Name));
            m_CustomHelper = serializedObject.FindProperty(string.Format("m_Custom{0}Helper", m_Name));
        }

        public void Draw()
        {
            string displayName = FieldNameForDisplay(m_Name);
            int selectedIndex = EditorGUILayout.Popup(string.Format("{0} Helper", displayName), m_HelperTypeNameIndex, m_HelperTypeNames);
            if (selectedIndex != m_HelperTypeNameIndex)
            {
                m_HelperTypeNameIndex = selectedIndex;
                m_HelperTypeName.stringValue = selectedIndex <= 0 ? null : m_HelperTypeNames[selectedIndex];
            }

            if (m_HelperTypeNameIndex <= 0)
            {
                EditorGUILayout.PropertyField(m_CustomHelper);
                if (m_CustomHelper.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(string.Format("You must set Custom {0} Helper.", displayName), MessageType.Error);
                }
            }
        }

        public void Refresh()
        {
            List<string> helperTypeNameList = new List<string>
            {
                CustomOptionName
            };

            helperTypeNameList.AddRange(EditorUtility.Type.GetTypeNames(typeof(T)));
            m_HelperTypeNames = helperTypeNameList.ToArray();

            m_HelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_HelperTypeName.stringValue))
            {
                m_HelperTypeNameIndex = helperTypeNameList.IndexOf(m_HelperTypeName.stringValue);
                if (m_HelperTypeNameIndex <= 0)
                {
                    m_HelperTypeNameIndex = 0;
                    m_HelperTypeName.stringValue = null;
                }
            }
        }

        private string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }
    }
}
