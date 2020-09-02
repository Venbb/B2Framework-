using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    [CustomEditor(typeof(Game))]
    public class GameInspector : UnityEditor.Editor
    {
        Game game;
        private SerializedProperty m_LogHelperTypeName = null;
        private string[] m_LogHelperTypeNames;
        int m_LogHelperTypeNameIndex = 0;
        bool helpersFadeOut = true;
        private void OnEnable()
        {
            game = (Game)target;
            game.debugEnable = EditorUtility.HasScriptingDefineSymbols(B2Framework.Log.SYMBOL);
            if (string.IsNullOrEmpty(game.buildPlatform))
            {
                game.buildPlatform = BuildHelper.GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
            }
            m_LogHelperTypeName = serializedObject.FindProperty("m_LogHelperTypeName");
            RefreshTypeNames();
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                helpersFadeOut = EditorGUILayout.BeginFoldoutHeaderGroup(helpersFadeOut, "Helpers");
                if (helpersFadeOut)
                {
                    int logHelperSelectedIndex = EditorGUILayout.Popup("Log Helper", m_LogHelperTypeNameIndex, m_LogHelperTypeNames);
                    if (logHelperSelectedIndex != m_LogHelperTypeNameIndex)
                    {
                        m_LogHelperTypeNameIndex = logHelperSelectedIndex;
                        m_LogHelperTypeName.stringValue = logHelperSelectedIndex <= 0 ? null : m_LogHelperTypeNames[logHelperSelectedIndex];
                    }
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
            EditorGUI.EndDisabledGroup();

            if (GUI.changed)
            {
                EditorUtility.SetScriptingDefineSymbols(B2Framework.Log.SYMBOL, game.debugEnable);
            }

            serializedObject.ApplyModifiedProperties();
        }
        void RefreshTypeNames()
        {
            List<string> logHelperTypeNames = new List<string>();
            logHelperTypeNames.Add("<None>");
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
            else
            {
                m_LogHelperTypeNameIndex = m_LogHelperTypeNames.Length > 1 ? 1 : 0;
            }
        }
    }
}