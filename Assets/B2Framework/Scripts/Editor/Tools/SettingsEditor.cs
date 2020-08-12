using UnityEngine;
using UnityEditor;
using B2Framework.Unity;

namespace B2Framework.Editor
{
    [CustomEditor(typeof(Settings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        Settings settings;
        private void OnEnable()
        {
            settings = (Settings)target;
            if (string.IsNullOrEmpty(settings.buildPlatform))
            {
                settings.buildPlatform = BuildHelper.GetPlatformName(EditorUserBuildSettings.activeBuildTarget);
            }
            settings.debugEnable = EditorUtility.HasScriptingDefineSymbols(AppConst.SYMBOL_DEBUG);
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUI.changed)
            {
                EditorUtility.SetScriptingDefineSymbols(AppConst.SYMBOL_DEBUG, settings.debugEnable);
            }
        }
    }
}