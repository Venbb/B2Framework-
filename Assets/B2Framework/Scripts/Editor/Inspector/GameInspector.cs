using System.Collections.Generic;
using UnityEditor;

namespace B2Framework.Editor
{
    [CustomEditor(typeof(Game))]
    public class GameInspector : UnityEditor.Editor
    {
        private SerializedProperty m_LogHelperTypeName = null;
        private string[] m_LogHelperTypeNames;
        int m_LogHelperTypeNameIndex = 0;
        private void OnEnable()
        {
            m_LogHelperTypeName = serializedObject.FindProperty("m_LogHelperTypeName");

            RefreshTypeNames();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.BeginVertical("box");
                {
                    EditorGUILayout.LabelField("Scripts", EditorStyles.boldLabel);

                    int logHelperSelectedIndex = EditorGUILayout.Popup("Log Helper", m_LogHelperTypeNameIndex, m_LogHelperTypeNames);
                    if (logHelperSelectedIndex != m_LogHelperTypeNameIndex)
                    {
                        m_LogHelperTypeNameIndex = logHelperSelectedIndex;
                        m_LogHelperTypeName.stringValue = logHelperSelectedIndex <= 0 ? null : m_LogHelperTypeNames[logHelperSelectedIndex];
                    }
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
        void RefreshTypeNames()
        {
            List<string> logHelperTypeNames = new List<string>{"<None>"};

            logHelperTypeNames.AddRange(EditorUtility.Type.GetTypeNames(typeof(B2Framework.ILogHelper)));
            m_LogHelperTypeNames = logHelperTypeNames.ToArray();
            m_LogHelperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(m_LogHelperTypeName.stringValue))
            {
                m_LogHelperTypeNameIndex = logHelperTypeNames.IndexOf(m_LogHelperTypeName.stringValue);
                if (m_LogHelperTypeNameIndex <= 0)
                {
                    m_LogHelperTypeNameIndex = 0;
                    m_LogHelperTypeName.stringValue = null;
                }
            }
        }
    }
}