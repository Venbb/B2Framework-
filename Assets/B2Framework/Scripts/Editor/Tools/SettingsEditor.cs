using UnityEngine;
using UnityEditor;

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
            settings.debugEnable = EditorUtility.HasScriptingDefineSymbols(B2Framework.Log.SYMBOL);
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUI.changed)
            {
                EditorUtility.SetScriptingDefineSymbols(B2Framework.Log.SYMBOL, settings.debugEnable);
            }
        }
    }
}