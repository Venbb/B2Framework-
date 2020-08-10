using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    /// <summary>
    /// 保存打包设置数据
    /// </summary>
    public class BuildSettings : ScriptableObject
    {
        public BuildTarget buildTarget;
        public string outPutPath;
        public bool clearFolders;
        public bool copytostreamingAssets;
        public bool zip;
        public BuildAssetBundleOptions options;
        [SerializeField] private string _version;
        public string version
        {
            get
            {
                _version = Application.version;
                return _version;
            }
            set { _version = value; }
        }
        public int bundleVersionCode;
        void Awake()
        {
            ReSet();
        }
        public void ReSet()
        {
            _version = Application.version;
            options = BuildAssetBundleOptions.None;
            buildTarget = EditorUserBuildSettings.activeBuildTarget;
            outPutPath = BuildHelper.GetAssetBundlesOutPutPath(buildTarget);
        }
        public void AddOption(BuildAssetBundleOptions option)
        {
            options |= option;
        }
        public void RemoveOption(BuildAssetBundleOptions option)
        {
            options &= (~option);
        }
        public bool HasOption(BuildAssetBundleOptions option)
        {
            return (options & option) != 0;
        }
    }
    [CustomEditor(typeof(BuildSettings))]
    public class BuildSettingsEditor : UnityEditor.Editor
    {
        public SerializedProperty version;
        private void OnEnable()
        {
            version = serializedObject.FindProperty("_version");
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUI.changed)
            {
                PlayerSettings.bundleVersion = version.stringValue;
            }
        }
    }
}