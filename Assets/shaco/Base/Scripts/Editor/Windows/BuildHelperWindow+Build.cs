using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class BuildHelperWindow : EditorWindow
    {
        //xlua支持文件创建完毕回调
        static public void BuildXluaSupportEndCallBack()
        {
            Debug.Log("shacoEditorLog.BuildHelperWindow.BuildXluaSupportEndCallBack");

            //固定为当前平台，不同平台打包的话需要手动切换，自动切换有风险且很慢
            // var buildTarget = shaco.GameHelper.datasave.ReadEnum<BuildTarget>("shaco.BuildHelperWindowParams.buildTarget");
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var serverMode = shaco.GameHelper.datasave.ReadString("shaco.BuildHelperWindowParams.serverMode");
            var buildChannel = shaco.GameHelper.datasave.ReadString("shaco.BuildHelperWindowParams.buildChannel");
            var shoudlBuildPackage = shaco.GameHelper.datasave.ReadBool("shaco.BuildHelperWindowParams.shoudlBuildPackage");
            var isDebugSettings = shaco.GameHelper.datasave.ReadBool("shaco.BuildHelperWindowParams.isDebugSettings");

            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
            }

            StartBuildProcessBase(buildTarget, serverMode, buildChannel, shoudlBuildPackage, isDebugSettings);
        }

        //shell脚本调用打包函数
        static private void StartBuildProcessWithShell()
        {
            // AssetDatabase.Refresh();
            Debug.Log("shacoEditorLog.BuildHelperWindow.StartBuildProcessWithShell start...");

            CheckAndReloadSettings();

            var strBuildTarget = shaco.Base.Utility.GetEnviromentCommandValue("BUILD_TARGET");
            var strBuildChannel = shaco.Base.Utility.GetEnviromentCommandValue("BUILD_CHANNEL");
            var strIsDebugBuild = shaco.Base.Utility.GetEnviromentCommandValue("IS_BUILD_DEBUG");
            bool isDebugSettings = strIsDebugBuild == "1" || strIsDebugBuild == "true";
            var strBuildServer = shaco.Base.Utility.GetEnviromentCommandValue("BUILD_SERVER");
            var strIsBuildXcode = shaco.Base.Utility.GetEnviromentCommandValue("IS_BUILD_XCODE");
            bool isBuildXcode = strIsBuildXcode == "1" || strIsBuildXcode == "true";

            _currentWindow.SetDebugReleaseSettings(isDebugSettings);

            if (!_currentWindow.SetChannel(strBuildChannel))
            {
                Debug.LogError("BuildHelperWindow+Build StartBuildProcessWithShell error: can't set channel=" + strBuildChannel);
                return;
            }

            if (!_currentWindow.SetServerMode(strBuildServer))
            {
                Debug.LogError("BuildHelperWindow+Build StartBuildProcessWithShell error: can't set server=" + strBuildServer);
                return;
            }

#if UNITY_5_3_OR_NEWER
            var buildTarget = BuildTarget.NoTarget;
#else
            var buildTarget = BuildTarget.Android;
#endif
            switch (strBuildTarget)
            {
                case "Android": buildTarget = BuildTarget.Android; break;
                case "iOS": buildTarget = BUILD_TARGET_IOS; break;
                case "WebGL": buildTarget = BuildTarget.WebGL; break;
                //当没有特殊指定打包平台时候，自动根据项目当前设定平台打包
                default: buildTarget = EditorUserBuildSettings.activeBuildTarget; break;
            }

            Debug.Log("shacoEditorLog.BuildHelperWindow.StartBuildProcessWithShell: shell buildTarget=" + buildTarget + " buildServer=" + strBuildServer + " debugMode=" + isDebugSettings + " strIsDebugBuild=" + strIsDebugBuild + " strIsBuildXcode=" + strIsBuildXcode);
            _currentWindow.StartBuildProcessReady(buildTarget, strBuildServer, strBuildChannel, !isBuildXcode, isDebugSettings);
        }

        //写入打包信息到项目根目录的临时文件中 - json格式
        //这样便于外部脚本文件获取详细打包信息
        //该方法仅支持apk和ipa
        private void WriteBuildInformationToTemporaryJsonFile(string packagePath, BuildTarget buildTarget)
        {
            var fullPath = Application.dataPath.ContactPath("../shell_build_result.txt");
            var appendString = new System.Text.StringBuilder();

            appendString.AppendFormat("bundle_id={0}\n", PlayerSettings.applicationIdentifier.ToString());
            appendString.AppendFormat("bundle_version={0}\n", PlayerSettings.bundleVersion.ToString());
            appendString.AppendFormat("bundle_code={0}\n", GetBuildCode());
            appendString.AppendFormat("package_path={0}\n", packagePath);
            appendString.AppendFormat("product_name={0}\n", PlayerSettings.productName);
            appendString.AppendFormat("global_defines={0}\n", _globalDefines.ToContactString(";"));

            var platformString = string.Empty;
            switch (buildTarget)
            {
                case BuildTarget.Android: platformString = "android"; break;
                case BUILD_TARGET_IOS: platformString = "ios"; break;
                default: platformString = buildTarget.ToString(); break;
            }
            appendString.AppendFormat("platform={0}\n", platformString);
            shaco.Base.FileHelper.WriteAllByUserPath(fullPath, appendString.ToString());

            Debug.Log("shacoEditorLog.BuildHelperWindow.WriteBuildInformationToTemporaryJsonFile: wirteJson=" + appendString.ToString());
        }

        //确认是否需要切换平台，返回false表示不需要切换平台，并停止打包
        private static bool CheckChangePlatform(BuildTarget buildTarget)
        {
            if (EditorUserBuildSettings.activeBuildTarget != buildTarget)
            {
                Debug.LogError("BuildHelperWindow CheckChangePlatform error: Current platform(" + EditorUserBuildSettings.activeBuildTarget + "), should change to target platform(" + buildTarget + ")");
                return false;

                //                 if (Application.isBatchMode)
                //                 {
                //                     Debug.LogError("BuildHelperWindow CheckChangePlatform error: Current platform(" + EditorUserBuildSettings.activeBuildTarget + "), should change to target platform(" + buildTarget + ")");
                //                     return false;
                //                 }
                //                 else
                //                 {
                //                     bool changed = EditorUtility.DisplayDialog("Should Change Build Target", "Current platform(" + EditorUserBuildSettings.activeBuildTarget + "), target platform(" + buildTarget + ")", "OK", "Cancel");

                //                     if (changed)
                //                     {
                // #if UNITY_5_3_OR_NEWER
                //                         EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetToBuildTargetGroup(buildTarget), buildTarget);
                // #else
                //                         EditorUserBuildSettings.SwitchActiveBuildTarget(buildTarget);
                // #endif
                //                     }
                //                     return changed;
                //                 }
            }
            else
            {
                return true;
            }
        }

        private static BuildTargetGroup BuildTargetToBuildTargetGroup(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.Android: return BuildTargetGroup.Android;
#if UNITY_5_3_OR_NEWER
                case BUILD_TARGET_IOS: return BuildTargetGroup.iOS;
#else
                case BUILD_TARGET_IOS: return BuildTargetGroup.iPhone;
#endif
                case BuildTarget.WebGL: return BuildTargetGroup.WebGL;
#if !UNITY_2018_1_OR_NEWER
                case BuildTarget.StandaloneOSXIntel: return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneOSXIntel64: return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneOSXUniversal: return BuildTargetGroup.Standalone;
#else
                case BuildTarget.StandaloneOSX: return BuildTargetGroup.Standalone;
#endif
                case BuildTarget.StandaloneWindows: return BuildTargetGroup.Standalone;
                case BuildTarget.StandaloneWindows64: return BuildTargetGroup.Standalone;
                default: Debug.LogError("BuildHelperWindow BuildTargetToBuildTargetGroup error: unsupport type=" + buildTarget); return BuildTargetGroup.Android;
            }
        }

        private void StartBuildProcessReady(BuildTarget buildTarget, string serverMode, string buildChannel, bool shoudlBuildPackage, bool isDebugSettings)
        {
            if (_isBuilding)
            {
                Debug.LogError("BuildHelperWindow+BUild StartBuildProcessReady error: is building, please wait...");
                return;
            }
            _isBuilding = true;

            //提前删除打包参数，等会再重新写入
            var dataSaveInstance = shaco.GameHelper.datasave;
            if (dataSaveInstance.ContainsKey("shaco.BuildHelperWindowParams.serverMode")) dataSaveInstance.Remove("shaco.BuildHelperWindowParams.serverMode");
            if (dataSaveInstance.ContainsKey("shaco.BuildHelperWindowParams.buildChannel")) dataSaveInstance.Remove("shaco.BuildHelperWindowParams.buildChannel");
            if (dataSaveInstance.ContainsKey("shaco.BuildHelperWindowParams.shoudlBuildPackage")) dataSaveInstance.Remove("shaco.BuildHelperWindowParams.shoudlBuildPackage");
            if (dataSaveInstance.ContainsKey("shaco.BuildHelperWindowParams.isDebugSettings")) dataSaveInstance.Remove("shaco.BuildHelperWindowParams.isDebugSettings");
            if (dataSaveInstance.ContainsKey("shaco.BuildHelperWindowParams.isBuildXluaSupport")) dataSaveInstance.Remove("shaco.BuildHelperWindowParams.isBuildXluaSupport");
            if (dataSaveInstance.ContainsKey("BuildHelperWindow+Build.WaitCompile")) dataSaveInstance.Remove("BuildHelperWindow+Build.WaitCompile");

            CheckProjectUpdate((bool success) =>
            {
                if (!Application.isBatchMode)
                {
                    EditorUtility.ClearProgressBar();
                }
                if (success)
                {
                    //保存参数，等项目更新和编译完毕后再打包
                    // dataSaveInstance.Write("shaco.BuildHelperWindowParams.buildTarget", buildTarget);
                    dataSaveInstance.WriteString("shaco.BuildHelperWindowParams.serverMode", serverMode);
                    dataSaveInstance.WriteString("shaco.BuildHelperWindowParams.buildChannel", buildChannel);
                    dataSaveInstance.WriteBool("shaco.BuildHelperWindowParams.shoudlBuildPackage", shoudlBuildPackage);
                    dataSaveInstance.WriteBool("shaco.BuildHelperWindowParams.isDebugSettings", isDebugSettings);

                    //保存本地设置以防止在编译后数据丢失
                    _currentWindow.SaveSettings();

                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetToBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                    var globalDefinesTmp = _globalDefines.ToContactString(";");
                    dataSaveInstance.WriteBool("shaco.BuildHelperWindowParams.isBuildXluaSupport", _globalDefines.Contains("HOTFIX_ENABLE") || _globalDefines.Contains("XLUA_ENABLE"));
                    Debug.Log("shacoEditorLog.BuildHelperWindow.StartBuildProcessReady: global defines, old=" + defines + " new=" + globalDefinesTmp);
                    if (defines != globalDefinesTmp)
                    {
                        //刷新全局宏
                        UpdateProjectDefines();
                        if (!Application.isBatchMode)
                        {
                            EditorUtility.DisplayProgressBar("Buildind Player", "Updating project defines", 0.2f);
                        }

                        //脚本打包模式下需要分为两步，先刷新宏再打包才行，否则宏不生效
                        //非脚本打包时候则可以监听Unity方法并延时自动执行宏刷新完毕回调方法
                        if (!Application.isBatchMode)
                            dataSaveInstance.WriteBool("BuildHelperWindow+Build.WaitCompile", true);
                        else
                            Debug.Log("BuildHelperWindow+Build StartBuildProcessReady: if you want build by batch mode, please call 'shacoEditor.BuildHelperWindow.UpdateProjectDefinesEndCallBack' to continue");
                    }
                    else
                    {
                        //宏一致，直接开始打包
                        if (!Application.isBatchMode)
                            UpdateProjectDefinesEndCallBack();
                        else
                            Debug.Log("BuildHelperWindow+Build StartBuildProcessReady: if you want build by batch mode, please call 'shacoEditor.BuildHelperWindow.UpdateProjectDefinesEndCallBack' to continue");
                    }

                    dataSaveInstance.CheckSaveModifyData();
                }
                else
                {
                    _isBuilding = false;
                    shaco.Log.Error("BuildHelperWindow+Build StartBuildProcessReady error: 'CheckProjectUpdate' failed");
                    this.Repaint();
                }
            });
        }

        public static void UpdateProjectDefinesEndCallBack()
        {
            Debug.Log("shacoEditorLog.BuildHelperWindow.UpdateProjectDefinesEndCallBack");
            var isBuildXluaSupport = shaco.GameHelper.datasave.ReadBool("shaco.BuildHelperWindowParams.isBuildXluaSupport");

            if (isBuildXluaSupport)
            {
                //更新lua支持脚本
                BuildXluaSupport();
            }
            else
            {
                //固定为当前平台，不同平台打包的话需要手动切换，自动切换有风险且很慢
                // var buildTarget = shaco.GameHelper.datasave.ReadEnum<BuildTarget>("shaco.BuildHelperWindowParams.buildTarget");
                var buildTarget = EditorUserBuildSettings.activeBuildTarget;
                var serverMode = shaco.GameHelper.datasave.ReadString("shaco.BuildHelperWindowParams.serverMode");
                var buildChannel = shaco.GameHelper.datasave.ReadString("shaco.BuildHelperWindowParams.buildChannel");
                var shoudlBuildPackage = shaco.GameHelper.datasave.ReadBool("shaco.BuildHelperWindowParams.shoudlBuildPackage");
                var isDebugSettings = shaco.GameHelper.datasave.ReadBool("shaco.BuildHelperWindowParams.isDebugSettings");

                //开始打包
                StartBuildProcessBase(buildTarget, serverMode, buildChannel, shoudlBuildPackage, isDebugSettings);
            }
        }

        //刷新并设置android的keystore
        private void UpdateAndroidKeystore()
        {
            if (!string.IsNullOrEmpty(_androidKeystoreName))
                PlayerSettings.Android.keystoreName = Application.dataPath.ContactPath("../" + _androidKeystoreName);
            PlayerSettings.Android.keystorePass = _androidKeystorePass;
            PlayerSettings.Android.keyaliasName = _androidKeyaliasName;
            PlayerSettings.Android.keyaliasPass = _androidKeyaliasPass;

            shaco.Log.Info("BuildHelperWindow+Build UpdateAndroidKeystore: PlayerSettings.Android.keystoreName=" + PlayerSettings.Android.keystoreName);
            shaco.Log.Info("BuildHelperWindow+Build UpdateAndroidKeystore: PlayerSettings.Android.keystorePass=" + PlayerSettings.Android.keystorePass);
            shaco.Log.Info("BuildHelperWindow+Build UpdateAndroidKeystore: PlayerSettings.Android.keyaliasName=" + PlayerSettings.Android.keyaliasName);
            shaco.Log.Info("BuildHelperWindow+Build UpdateAndroidKeystore: PlayerSettings.Android.keyaliasPass=" + PlayerSettings.Android.keyaliasPass);
        }

        //使用shell脚本打包unity工程的时候，需要加载一次配置
        static private BuildHelperWindow CheckAndReloadSettings()
        {
            if (_currentWindow == null)
            {
                _currentWindow = shacoEditor.EditorHelper.GetWindow<BuildHelperWindow>(null, true, "BuildHelperWindow");
                _currentWindow.InitSettings();
            }
            return _currentWindow;
        }

        //执行打包流程
        private static void StartBuildProcessBase(BuildTarget buildTarget, string serverMode, string buildChannel, bool shoudlBuildPackage, bool isDebugSettings)
        {
            Debug.Log("shacoEditorLog.BuildHelperWindow.StartBuildProcessReady: StartBuildProcess begin...");
            var windowTmp = CheckAndReloadSettings();

            try
            {
                DeleteTemporaryFile(Application.dataPath.ContactPath("../shell_build_result.txt"));

                BuildOptions options = BuildOptions.None;
                if (isDebugSettings)
                {
                    //webgl平台使用Development打包会卡死，所以暂不开启
#if !UNITY_WEBGL
                    options = BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler | BuildOptions.Development;
#endif
#if UNITY_WEBGL
                    PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
                    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
#endif
                }
                else
                {
#if UNITY_WEBGL
                    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                    PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
#endif
                }

                if (buildTarget == BuildTarget.Android)
                    windowTmp.UpdateAndroidKeystore();

                //检查channel是否合法
                if (!windowTmp._channel.Contains(buildChannel))
                {
                    Debug.LogError("BuildHelperWindow StartBuildProcess error: unsupport channel=" + buildChannel);
                    goto PROGRESS_END;
                }

                if (buildTarget == BUILD_TARGET_IOS)
                {
                    if (!string.IsNullOrEmpty(windowTmp._iOSDevelopmentTeam))
                    {
                        if (shoudlBuildPackage && string.IsNullOrEmpty(windowTmp._iOSProvisioningProfileSpecifier))
                        {
                            Debug.LogError("BuildHelperWindow StartBuildProcess error: missing provisioning profile specifier, please check settings");
                            goto PROGRESS_END;
                        }
                    }
                    else
                    {
                        Debug.LogError("BuildHelperWindow StartBuildProcess error: missing development team, please check settings");
                        goto PROGRESS_END;
                    }
                    UpdateiOSCertificate();
                }

#if UNITY_EDITOR_WIN
                if (string.IsNullOrEmpty(_windowsRunShellExePath))
                {
                    Debug.LogError("BuildHelperWindow StartBuildProcess error: Please set up an executable program that can run the shell script");
                    goto PROGRESS_END;
                }
                if (!shaco.Base.FileHelper.ExistsFile(_windowsRunShellExePath))
                {
                    Debug.LogError("BuildHelperWindow StartBuildProcess error: Program is not found, Path:\n" + _windowsRunShellExePath);
                    goto PROGRESS_END;
                }
#endif

                if (!CheckChangePlatform(buildTarget))
                {
                    goto PROGRESS_END;
                }

                var scenesTmp = EditorHelper.GetEnabledEditorScenes();
                if (scenesTmp == null || scenesTmp.Length == 0)
                {
                    Debug.LogError("BuildHelperWindow StartBuildProcess error: no scene need to build");
                    goto PROGRESS_END;
                }

                var exportPackageFolder = Application.dataPath.Remove("/Assets") + "/BuildPackages/";
                var exportPackagePath = exportPackageFolder + GetBuildTargetFlag(buildTarget, serverMode);

                //确认打包文件夹存在
                if (!System.IO.Directory.Exists(exportPackageFolder))
                {
                    System.IO.Directory.CreateDirectory(exportPackageFolder);
                }

                OverriteSettings(buildTarget);
                OverriteSettingsCompatibility(buildTarget);

#if UNITY_2017_1_OR_NEWER
                //打包前需要再pack一次所有图集，防止图集没有更新到
                Debug.Log("BuildHelperWindow BuildPipeline.BuildPlayer: UnityEditor.U2D.SpriteAtlasUtility.PackAllAtlases");
                UnityEditor.U2D.SpriteAtlasUtility.PackAllAtlases(buildTarget);
#endif

                //开始打包
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();

                //通知打包前回调
                shaco.Base.Utility.ExecuteAttributeStaticFunction<shacoEditor.PostProcessWillBuildAttribute>(buildTarget, exportPackagePath);

                Debug.Log("BuildHelperWindow BuildPipeline.BuildPlayer: buildTarget=" + buildTarget + " options=" + options);
                Debug.Log("BuildHelperWindow BuildPipeline.BuildPlayer: exportPackagePath=" + exportPackagePath);
                shaco.Base.FileHelper.CheckFolderPathWithAutoCreate(exportPackagePath);
                BuildPipeline.BuildPlayer(scenesTmp, exportPackagePath, buildTarget, options);

                Debug.Log("BuildHelperWindow BuildPipeline.BuildPlayer: build end");

                //打包并导出ipa
                if (buildTarget == BUILD_TARGET_IOS && shoudlBuildPackage)
                {
                    windowTmp.BuiliPA(exportPackagePath);
                }

                //通知打包完毕回调
                shaco.Base.Utility.ExecuteAttributeStaticFunction<shacoEditor.PostProcessBuildedAttribute>(buildTarget, exportPackagePath);

                //写入打包脚本需要的临时参数
                Debug.Log("BuildHelperWindow BuildPipeline.BuildPlayer: global defines=" + windowTmp._globalDefines.ToContactString(";"));
                if (Application.isBatchMode)
                {
                    windowTmp.WriteBuildInformationToTemporaryJsonFile(exportPackagePath, buildTarget);
                }
                else
                {
                    //打开导出包目录
                    EditorHelper.ShowInFolder(exportPackagePath);
                }

                RevertOverriteSettingsCompatibility(buildTarget);
            }
            catch (System.Exception e)
            {
                Debug.LogError("BuildHelperWindow StartBuildProcess exception: e=" + e);
                goto PROGRESS_END;
            }

        PROGRESS_END:
            if (!Application.isBatchMode)
            {
                EditorUtility.ClearProgressBar();
            }

            var isBuildXluaSupport = shaco.GameHelper.datasave.ReadBool("shaco.BuildHelperWindowParams.isBuildXluaSupport");
            if (isBuildXluaSupport)
                shaco.Base.Utility.ExecuteAttributeStaticFunction<shacoEditor.PostProcessClearGeneratorXLuaAttribute>();
            windowTmp._isBuilding = false;
            Debug.Log("shacoEditorLog.BuildHelperWindow.StartBuildProcessReady: StartBuildProcess end...");
        }

        //删除可能存在的临时文件
        static private void DeleteTemporaryFile(string path)
        {
            if (!shaco.Base.FileHelper.ExistsFile(path))
                return;
            shaco.Base.FileHelper.DeleteByUserPath(path);
        }

        //创建xlua支持文件
        static private void BuildXluaSupport()
        {
            Debug.Log("shacoEditorLog.BuildHelperWindow.BuildXluaSupport");

            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayProgressBar("Buildind Player", "Building xlua support", 0.3f);
            }

            //开始构建文件
            shaco.Base.Utility.ExecuteAttributeStaticFunction<shacoEditor.PostProcessGeneratorXLuaAttribute>();
            if (!Application.isBatchMode)
            {
                EditorUtility.DisplayProgressBar("Buildind Player", "Building xlua support(Wait Script Compile)", 0.4f);
            }

            BuildXluaSupportEndCallBack();
        }

        static private void OverriteSettings(BuildTarget buildTarget)
        {
            //写入版本信息
            if (null != EditorHelper.GetBuildVerionFile())
            {
                EditorHelper.SetBuildVersionFile();
            }

            //设置c# API兼容性级别，至少应该是.Net 2.0以上
#if UNITY_5_3_OR_NEWER
            var targetGroup = BuildTargetToBuildTargetGroup(buildTarget);
            if (PlayerSettings.GetApiCompatibilityLevel(targetGroup) == ApiCompatibilityLevel.NET_2_0_Subset)
            {
                PlayerSettings.SetApiCompatibilityLevel(targetGroup, ApiCompatibilityLevel.NET_2_0);
            }
#else
            if (PlayerSettings.apiCompatibilityLevel == ApiCompatibilityLevel.NET_2_0_Subset)
            {
                PlayerSettings.apiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
            }
#endif
        }

        /// <summary>
        /// 执行shell脚本
        /// <param name="shellScriptPath">脚本文件路径</param>
        /// <param name="isWaitForEnd">是否阻塞线程等待脚本执行完毕</param>
        /// <param name="args">附带参数</param>
        /// <return></return>
        /// </summary>
        private static bool RunShell(string shellScriptPath, bool isWaitForEnd, params string[] args)
        {
            try
            {
                var process = new System.Diagnostics.Process();

                var argsCombine = string.Empty;
                for (int i = 0; i < args.Length; ++i)
                {
                    argsCombine += args[i];
                    if (i < args.Length - 1)
                        argsCombine += " ";
                }

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                process.StartInfo.FileName = "/bin/bash";
#else
                process.StartInfo.FileName = _windowsRunShellExePath;
#endif

                process.StartInfo.Arguments = shellScriptPath + (args.Length > 0 ? " " + argsCombine : string.Empty);

                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                if (isWaitForEnd)
                {
                    process.WaitForExit();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("BuildHelperWindow+Build RunShell error: " + e);
                Debug.LogError("shellScriptPath=" + shellScriptPath);
                return false;
            }
            return true;
        }

        private static string GetFullShellScriptsPath(string replativePath)
        {
            return shaco.Base.FileHelper.GetCurrentSourceFolderPath().RemoveLastPathByLevel(2).ContactPath("/ShellScripts/" + replativePath);
        }

        //从git更新工程，并检查工程是否还有修改内容，如果有修改内容则需要确认是否继续打包
        private void CheckProjectUpdate(System.Action<bool> callbackEnd)
        {
            //脚本打包模式下直接返回结果
            if (Application.isBatchMode)
            {
                Debug.Log("BuildHelperWindow+Build CheckProjectUpdate: not check project update when batch mode");
                callbackEnd(true);
                return;
            }

            var projectDiffPath = GetFullShellScriptsPath("diff.tmp");
            var updateEndFilePath = GetFullShellScriptsPath("is_update_project_success_tmp.txt");
            bool isCancel = false;
            bool isIgnoreCheckProjectUpdate = false;
            var calculateTime = new shaco.Base.CalculateTime();
            calculateTime.Start();

            //删除上一次的项目更新日志文件
            shaco.Base.FileHelper.DeleteByUserPath(projectDiffPath);
            shaco.Base.FileHelper.DeleteByUserPath(updateEndFilePath);

            var shellPath = GetFullShellScriptsPath("CheckProjectUpdate.sh");
            Debug.Log("check project update");

            if (!RunShell(shellPath, false, _projectUpdateType.ToString()))
            {
                callbackEnd(false);
                return;
            }

            //等待项目更新的日志文件写入完毕
            shaco.Base.WaitFor.Run(() =>
            {
                //非jenkins脚本打包模式下才显示进度条
                isCancel = EditorUtility.DisplayCancelableProgressBar("Buildind Player", "check project update...(" + _projectUpdateType + ")", 0.1f);

                //有时因为网络问题检查github会超时，这个时候用户可以选择强制开始打包
                if (calculateTime.IsTimeout())
                {
                    isIgnoreCheckProjectUpdate = EditorUtility.DisplayDialog("Time out", "Can't update project", "Force Continue", "Stop");
                    if (!isIgnoreCheckProjectUpdate)
                    {
                        isCancel = true;
                    }
                }

                return isCancel || isIgnoreCheckProjectUpdate || shaco.Base.FileHelper.ExistsFile(projectDiffPath);
            }, () =>
            {
                if (isCancel)
                {
                    callbackEnd(false);
                    return;
                }

                var gitDataTmp = isIgnoreCheckProjectUpdate ? string.Empty : shaco.Base.FileHelper.ReadAllByUserPath(projectDiffPath);
                if (gitDataTmp.Length > 1)
                {
                    var retButton = EditorUtility.DisplayDialogComplex("Git warning", "Has some files need to commit, Do you need to continue build ?", "Force Continue", "Stop", "Discard All Local Changes");

                    //回滚所有数据
                    if (retButton == 2)
                    {
                        bool success = RunShell(shellPath, true, _projectUpdateType.ToString(), "DiscardAllLocalChanges");

                        //等待回滚完毕
                        shaco.Base.WaitFor.Run(() =>
                        {
                            return System.IO.File.Exists(updateEndFilePath);
                        }, () =>
                        {
                            callbackEnd(success);
                        });
                    }
                    else if (retButton == 0)
                    {
                        callbackEnd(true);
                    }
                    else
                    {
                        callbackEnd(false);
                    }
                }
                else
                {
                    callbackEnd(true);
                }
            });
        }

        //编译xcode，导出ipa
        private void BuiliPA(string exportXcodePath)
        {
            System.Action callbackEnd = () =>
            {
                //开始打包ipa
                var shellPath = GetFullShellScriptsPath("WillBuildXcode.sh");
                Debug.Log("build ipa=" + RunShell(shellPath, false, exportXcodePath, "buildPackage=true"));
            };

            //设置打包前参数
            shaco.Base.Utility.ExecuteAttributeStaticFunction<shacoEditor.PostProcessOverwriteXcodePlistAttribute>(
                exportXcodePath, _iOSExportOption, _iOSProvisioningProfileSpecifier, callbackEnd);
        }

        //刷新iOS打包证书
        private static void UpdateiOSCertificate()
        {
            //已经在XcodeBuildListener中设置过了
        }

        //获取打包平台对应的路径标记
        private static string GetBuildTargetFlag(BuildTarget buildTarget, string serverMode)
        {
            var now = System.DateTime.Now;
            string formatTime = now.Year.ToString("0000") + "_" + now.Month.ToString("00") + "_" + now.Day.ToString("00") + "_" + now.Hour.ToString("00") + "_" + now.Minute.ToString("00");
            var formatVersion = PlayerSettings.bundleVersion + "_";

#if UNITY_5_3_OR_NEWER
            switch (buildTarget)
            {
                case BuildTarget.Android: formatVersion += PlayerSettings.Android.bundleVersionCode; break;
                case BuildTarget.iOS: formatVersion += PlayerSettings.iOS.buildNumber; break;
                case BuildTarget.WebGL: formatVersion += PlayerSettings.bundleVersion; break;
                default: shaco.Log.Error("BuildHelperWindow+Build GetbuildTargetFlag error: unsupport build target=" + buildTarget); break;
            }
#else
            formatVersion += PlayerSettings.shortBundleVersion;
#endif

            var extensions = string.Empty;
            switch (buildTarget)
            {
                case BuildTarget.Android: extensions = ".apk"; break;
                case BUILD_TARGET_IOS: /*extensions = ".ipa";*/ break;
                case BuildTarget.WebGL: /*extensions = ".webgl"*/ break;
                default: Debug.LogError("BuildHelperWindow GetbuildTargetFlag error: unsuppport platform=" + buildTarget); break;
            }

            return serverMode + "/" + formatVersion + "_" + formatTime + extensions;
        }
    }
}