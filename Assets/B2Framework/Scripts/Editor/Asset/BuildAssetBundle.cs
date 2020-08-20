using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    public class BuildAssetBundle : EditorWindow
    {
        bool foldout;
        BuildSettings buildSettings;
        void OnEnable()
        {
            buildSettings = BuildHelper.GetBuildSettings();
        }
        void Reset()
        {
            buildSettings?.ReSet();
        }
        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("Player Settings", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("CompanyName", GUILayout.Width(120f));
                        var companyName = EditorGUILayout.TextField(PlayerSettings.companyName, GUILayout.Width(150f));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("ProductName", GUILayout.Width(120f));
                        var productName = EditorGUILayout.TextField(PlayerSettings.productName, GUILayout.Width(150f));
                        EditorGUILayout.EndHorizontal();

                        if (companyName != PlayerSettings.companyName || productName != PlayerSettings.productName)
                        {
                            PlayerSettings.companyName = companyName;
                            PlayerSettings.productName = productName;
                            var identifier = string.Format("com.{0}.{1}", companyName?.ToLower(), productName?.ToLower());
                            PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, identifier);
                        }

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(new GUIContent("Bundle Identifier", "Package Name"), GUILayout.Width(120f));
                            var identifier = EditorGUILayout.TextField(PlayerSettings.applicationIdentifier, GUILayout.Width(150f));
                            if (identifier != PlayerSettings.applicationIdentifier)
                                PlayerSettings.SetApplicationIdentifier(EditorUserBuildSettings.selectedBuildTargetGroup, identifier);
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(new GUIContent("Version*", "App Version"), GUILayout.Width(120f));
                            var version = EditorGUILayout.TextField(PlayerSettings.bundleVersion, GUILayout.Width(80f));
                            if (version != buildSettings.version)
                            {
                                buildSettings.version = version;
                                PlayerSettings.bundleVersion = version;
                            }
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(new GUIContent("Build", "BundleVersionCode. Auto increase when building assetbundle."), GUILayout.Width(120f));
                            buildSettings.bundleVersionCode = EditorGUILayout.IntField(buildSettings.bundleVersionCode, GUILayout.Width(50f));
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        {
                            EditorGUILayout.LabelField(new GUIContent("Bundle Version", "Version + Build. Final release version number."), GUILayout.Width(120f));
                            GUILayout.Label(string.Format("{0}.{1}", buildSettings.version, buildSettings.bundleVersionCode.ToString()));
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }
                buildSettings.zip = GUILayout.Toggle(buildSettings.zip, "Zip All Assets");
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField("AssetBundle Options", EditorStyles.boldLabel);
                    EditorGUILayout.BeginVertical("box");
                    {
                        var selected = DrawOption("Uncompressed AssetBundle", BuildAssetBundleOptions.UncompressedAssetBundle);
                        if (selected)
                        {
                            buildSettings.RemoveOption(BuildAssetBundleOptions.ChunkBasedCompression);
                        }
                        selected = DrawOption("Chunk Based Compression (LZ4)", BuildAssetBundleOptions.ChunkBasedCompression);
                        if (selected)
                        {
                            buildSettings.RemoveOption(BuildAssetBundleOptions.UncompressedAssetBundle);
                        }
                        DrawOption("Disable Write TypeTree", BuildAssetBundleOptions.DisableWriteTypeTree);
                        DrawOption("Deterministic AssetBundle", BuildAssetBundleOptions.DeterministicAssetBundle);
                        DrawOption("Force Rebuild AssetBundle", BuildAssetBundleOptions.ForceRebuildAssetBundle);
                        DrawOption("Ignore TypeTree Changes", BuildAssetBundleOptions.IgnoreTypeTreeChanges);
                        EditorGUI.BeginDisabledGroup(true);
                        {
                            DrawOption("Append Hash To AssetBundle Name", BuildAssetBundleOptions.AppendHashToAssetBundleName);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            var compressMessage = string.Empty;
            var compressMessageType = MessageType.None;
            GetCompressMessage(out compressMessage, out compressMessageType);
            EditorGUILayout.HelpBox(compressMessage, compressMessageType);

            EditorGUILayout.LabelField("Build Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            {
                var target = (BuildTarget)EditorGUILayout.EnumPopup(new GUIContent("Build Target"), buildSettings.buildTarget, t => EditorUtility.GetBuildPlatform((BuildTarget)t) != Platform.Unknown, false);
                if (target != buildSettings.buildTarget)
                {
                    buildSettings.buildTarget = target;
                    buildSettings.outPutPath = BuildHelper.GetAssetBundlesOutPutPath(target);
                }
                GUILayout.Space(10);
                buildSettings.outPutPath = EditorGUILayout.TextField(new GUIContent("OutPut Path"), buildSettings.outPutPath);

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select Out Put Path", System.Environment.CurrentDirectory, "");
                    if (!string.IsNullOrEmpty(path))
                        buildSettings.outPutPath = path;
                }
                if (GUILayout.Button("Reset", GUILayout.Width(100)))
                {
                    Reset();
                }
                GUILayout.EndHorizontal();

                buildSettings.clearFolders = GUILayout.Toggle(buildSettings.clearFolders, "Clear Folders");
                buildSettings.copytostreamingAssets = GUILayout.Toggle(buildSettings.copytostreamingAssets, "Copy To StreamingAssets");

                GUILayout.Space(10);
                // EditorGUILayout.DropdownButton("Advanced Settings",)

                // foldout = EditorGUILayout.Foldout(foldout, "Advanced Settings");
                // if (foldout)
                // {
                //     buildSettings.options = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField("Bundle Options", buildSettings.options);
                // }
            }
            EditorGUILayout.EndVertical();
            var buildMessage = string.Empty;
            var buildMessageType = MessageType.None;
            GetBuildMessage(out buildMessage, out buildMessageType);
            EditorGUILayout.HelpBox(buildMessage, buildMessageType);
            // GUILayout.Space(20);
            // EditorGUILayout.TextField("???", "", "SearchTextField");
            EditorGUI.BeginDisabledGroup(buildMessageType == MessageType.Error);
            {
                if (GUILayout.Button("Build"))
                {
                    BuildAssetBundles();
                }
            }
            EditorGUI.EndDisabledGroup();
        }
        private bool DrawOption(string name, BuildAssetBundleOptions option)
        {
            var hasSelected = buildSettings.HasOption(option);
            var selected = EditorGUILayout.ToggleLeft(name, hasSelected);
            if (hasSelected != selected)
            {
                if (selected)
                    buildSettings.AddOption(option);
                else
                    buildSettings.RemoveOption(option);
            }
            return selected;
        }
        private void GetCompressMessage(out string message, out MessageType messageType)
        {
            if (buildSettings.zip)
            {
                if (buildSettings.HasOption(BuildAssetBundleOptions.UncompressedAssetBundle))
                {
                    message = "Compresses AssetBundles with ZIP only. It uses more storage but it's faster when loading the AssetBundles.";
                    messageType = MessageType.Info;
                }
                else if (buildSettings.HasOption(BuildAssetBundleOptions.ChunkBasedCompression))
                {
                    message = "Compresses AssetBundles with both chunk-based compression and ZIP. Recommended when you use 'AssetBundle.LoadFromFile'.";
                    messageType = MessageType.Info;
                }
                else
                {
                    message = "Compresses AssetBundles with both LZMA and ZIP. Not recommended.";
                    messageType = MessageType.Warning;
                }
            }
            else
            {
                if (buildSettings.HasOption(BuildAssetBundleOptions.UncompressedAssetBundle))
                {
                    message = "Doesn't compress AssetBundles at all. Not recommended.";
                    messageType = MessageType.Warning;
                }
                else if (buildSettings.HasOption(BuildAssetBundleOptions.ChunkBasedCompression))
                {
                    message = "Compresses AssetBundles with chunk-based compression only. Recommended when you use 'AssetBundle.LoadFromFile'.";
                    messageType = MessageType.Info;
                }
                else
                {
                    message = "Compresses AssetBundles with LZMA only. Recommended when you use 'AssetBundle.LoadFromMemory'.";
                    messageType = MessageType.Info;
                }
            }
        }
        private void GetBuildMessage(out string message, out MessageType messageType)
        {
            messageType = MessageType.None;
            message = string.Empty;
            var companyName = PlayerSettings.companyName;
            if (string.IsNullOrEmpty(companyName))
            {
                message = "CompanyName is empty.";
                messageType = MessageType.Error;
                return;
            }
            messageType = MessageType.Info;
            message = "Ready to build.";
        }
        /// <summary>
        /// AssetBundle打包
        /// </summary>
        public void BuildAssetBundles()
        {
            if (string.IsNullOrEmpty(buildSettings.outPutPath))
            {
                if (UnityEditor.EditorUtility.DisplayDialog("Warning", "OutPutPath is Empty!", "Reset")) Reset();
                return;
            }
            var watch = new Stopwatch();
            watch.Start();

            // 是否删除目录
            if (buildSettings.clearFolders && Directory.Exists(buildSettings.outPutPath)) Directory.Delete(buildSettings.outPutPath, true);
            // 执行打包规则
            BuildHelper.ApplyBuildRules();
            // 资源打包
            BuildHelper.BuildAssetBundles(buildSettings.outPutPath, buildSettings.options, buildSettings.buildTarget);

            // 复制到StreamingAssets
            if (buildSettings.copytostreamingAssets)
            {
                var dest = Utility.Path.Combine(Application.streamingAssetsPath, GameConst.ASSETBUNDLES);
                if (!Directory.Exists(dest)) Directory.CreateDirectory(dest);
                EditorUtility.CopyAssets(buildSettings.outPutPath, dest);
            }
            AssetDatabase.Refresh();

            watch.Stop();
            Log.Debug("BuildAssetBundles " + watch.ElapsedMilliseconds + " ms.");

            EditorUtility.ExplorerFolder(buildSettings.outPutPath);
        }
    }
}