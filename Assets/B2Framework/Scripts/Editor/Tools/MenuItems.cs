using UnityEngine;
using UnityEditor;
using System.IO;
using B2Framework;

namespace B2Framework.Editor
{
    /// <summary>
    ///  B2 菜单扩展
    ///  MenuItem(string itemName, bool isValidateFunction, int priority);
    ///  通过 priority参数控制显示的顺序,当priority参数 相差数值 大于10的时候，将会出现分割线，可以通过这个功能来分类。
    ///  添加快捷键 使用 %(ctrl on Windows, cmd on macOS), #(shift), &(alt).
    /// </summary>
    public static partial class MenuItems
    {
        [MenuItem("B2Framework/Create/Lua Script %L")]
        [MenuItem("Assets/Create/Lua Script", false, 80)]
        public static void CreatNewLua()
        {
            LuaScriptTemplate.CreatNewLua();
        }

        [MenuItem("B2Framework/Create/BuildRules")]
        public static void CreatBuildRules()
        {
            var target = BuildHelper.GetBuildRules();
            Selection.activeObject = target;
            UnityEditor.EditorUtility.FocusProjectWindow();
        }

        [MenuItem("B2Framework/Create/BuildSettings")]
        public static void CreatBuildSettings()
        {
            var target = BuildHelper.GetBuildSettings();
            Selection.activeObject = target;
            UnityEditor.EditorUtility.FocusProjectWindow();
        }

        [MenuItem("B2Framework/Create/ExTools")]
        public static void CreatExTools()
        {
            var target = BuildHelper.GetExTools();
            Selection.activeObject = target;
            UnityEditor.EditorUtility.FocusProjectWindow();
        }
        [MenuItem("B2Framework/AssetBundles &B")]
        /// <summary>
        /// AssetBundle窗口
        /// </summary>
        static void OpenBuildAssetBundle()
        {
            EditorWindow.GetWindow<BuildAssetBundle>("AssetBundles");
        }

        [MenuItem("B2Framework/ExTools &T")]
        static void OpenExTools()
        {
            EditorWindow.GetWindow<ExTools>("ExTools");
        }

        [MenuItem("B2Framework/EditorStyleViewer")]
        public static void OpenEditorStyleViewer()
        {
            EditorWindow.GetWindow(typeof(EditorStyleViewer));
        }

        [MenuItem("B2Framework/ClearAssetBundleNames")]
        private static void ClearAllAssetBundleNames()
        {
            EditorUtility.ClearAssetBundleNames();
        }

        [MenuItem("Assets/LuaToTextAsset")]
        private static void LuaToTextAsset()
        {
            var selection = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.DeepAssets);
            foreach (var o in selection)
            {
                var path = AssetDatabase.GetAssetPath(o);
                if (string.IsNullOrEmpty(path) || Directory.Exists(path)) continue;
                var ext = Utility.Path.GetExtension(path, GameConst.LUA_EXTENSIONS.Split(','));
                if (!string.IsNullOrEmpty(ext) && ext != GameConst.LUA_EXTENSION)
                {
                    Utility.Files.RenameFile(path, path.Replace(ext, GameConst.LUA_EXTENSION));
                    var meta = path + ".meta";
                    if (File.Exists(meta)) File.Delete(meta);
                }
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("Assets/ClearAssetBundleNames")]
        private static void ClearAssetBundleNames()
        {
            EditorUtility.ClearSelectedAssetBundleNames();
        }

        [MenuItem("B2Framework/Take a screenshot")]
        private static void Screenshot()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel("截屏", null, "screenshot_", "png");
            if (string.IsNullOrEmpty(path)) return;

            ScreenCapture.CaptureScreenshot(path);
        }
    }
}