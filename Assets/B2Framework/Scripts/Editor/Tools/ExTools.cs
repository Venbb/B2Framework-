using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using B2Framework.Unity;

namespace B2Framework.Editor
{
    public class ExTools : EditorWindow
    {
        string progressMsg = string.Empty;
        bool isDone = false;
        bool isError = false;
        float progress = 0f;
        float filesCount;
        ExToolsObject tools;
        Queue<System.Action> exeFuns = new Queue<System.Action>();
        void OnEnable()
        {
            tools = BuildHelper.GetExTools();
        }
        void OnGUI()
        {
            EditorGUILayout.HelpBox("【提示】" +
            "\n     - 提供pb文件、Lua配置文件、多语言json文件的生成" +
            "\n     - 这个工具比较鸡肋，目的是希望数据生成比较灵活，如：选择要生成的数据，配置路径等" +
            "\n     - 但是可能会出现异常无法捕获的情况，所以建议直接执行build.py，一键生成所有数据" +
            "\n     - 依赖Python2.7版本和xlrd的Python库，请预先安装" +
            "\n     - 想生成哪个数据就勾选哪个，不想生成的不勾选" +
            "\n     - 原则上不用修改这里的路径，直接生成即可，一般存在可变的是源数据的路径" +
            "\n     - 没有看到Success的提示窗口则表示生成失败", MessageType.None);
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            tools.homePath = EditorGUILayout.TextField(new GUIContent("Home Path", "工具根目录"), tools.homePath);
            if (GUILayout.Button("Browse", GUILayout.Width(100)))
            {
                var path = UnityEditor.EditorUtility.OpenFolderPanel("Select tools folder", tools.homePath, "");
                if (!string.IsNullOrEmpty(path)) tools.homePath = path;
            }
            GUILayout.EndHorizontal();

            // proto
            GUILayout.Space(10);
            EditorGUILayout.BeginVertical("FrameBox");
            tools.genPb = EditorGUILayout.BeginToggleGroup(new GUIContent("proto to pb", "生成pb文件，勾选则生成"), tools.genPb);
            if (tools.genPb)
            {
                GUILayout.BeginHorizontal();
                tools.protoPath = EditorGUILayout.TextField(new GUIContent("Proto Path", "Proto源文件路径，可变"), tools.protoPath);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select proto folder", tools.protoPath, "");
                    if (!string.IsNullOrEmpty(path)) tools.protoPath = path;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                tools.pbOutPath = EditorGUILayout.TextField(new GUIContent("Pb Out Path", "pb文件生成路径，不建议修改"), tools.pbOutPath);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select pbPath folder", tools.pbOutPath, "");
                    if (!string.IsNullOrEmpty(path)) tools.pbOutPath = path;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                tools.protoBatchScript = EditorGUILayout.TextField(new GUIContent("Script Path", "批处理脚本路径，不用修改"), tools.protoBatchScript);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFilePanel("Select scprit file", tools.homePath, "py");
                    if (!string.IsNullOrEmpty(path)) tools.protoBatchScript = path;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            // config
            EditorGUILayout.BeginVertical("FrameBox");
            tools.genCfg = EditorGUILayout.BeginToggleGroup(new GUIContent("xlsx to lua", "生成Lua配置文件，勾选则生成"), tools.genCfg);
            if (tools.genCfg)
            {
                GUILayout.BeginHorizontal();
                tools.xlsxPath = EditorGUILayout.TextField(new GUIContent("xlsx Path","配置Excel源文件路径，可变"), tools.xlsxPath);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select xlsxPath folder", tools.xlsxPath, "");
                    if (!string.IsNullOrEmpty(path)) tools.xlsxPath = path;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                tools.luaCfgPath = EditorGUILayout.TextField(new GUIContent("Lua Out Path", "Lua文件生成路径，不建议修改"), tools.luaCfgPath);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select luaCfgPath folder", tools.luaCfgPath, "");
                    if (!string.IsNullOrEmpty(path)) tools.luaCfgPath = path;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                tools.cfgBatchScript = EditorGUILayout.TextField(new GUIContent("Script Path", "批处理脚本路径，不用修改"), tools.cfgBatchScript);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFilePanel("Select scprit file", tools.homePath, "py");
                    if (!string.IsNullOrEmpty(path)) tools.cfgBatchScript = path;
                }
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            // localization
            EditorGUILayout.BeginVertical("FrameBox");
            tools.genLoc = EditorGUILayout.BeginToggleGroup(new GUIContent("lan to json", "生成多语言配置文件，勾选则生成"), tools.genLoc);
            if (tools.genLoc)
            {
                GUILayout.BeginHorizontal();
                tools.locXlsxPath = EditorGUILayout.TextField(new GUIContent("xlsx Path","多语言Excel源文件路径，可变"), tools.locXlsxPath);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select locXlsxPath folder", tools.locXlsxPath, "");
                    if (!string.IsNullOrEmpty(path)) tools.locXlsxPath = path;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                tools.locCfgPath = EditorGUILayout.TextField(new GUIContent("Json Out Path", "Json文件生成路径，不建议修改"), tools.locCfgPath);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFolderPanel("Select locCfgPath folder", tools.locCfgPath, "");
                    if (!string.IsNullOrEmpty(path)) tools.locCfgPath = path;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                tools.locBatchScript = EditorGUILayout.TextField(new GUIContent("Script Path", "批处理脚本路径，不用修改"), tools.locBatchScript);
                if (GUILayout.Button("Browse", GUILayout.Width(100)))
                {
                    var path = UnityEditor.EditorUtility.OpenFilePanel("Select scprit file", tools.homePath, "py");
                    if (!string.IsNullOrEmpty(path)) tools.locBatchScript = path;
                }
                GUILayout.EndHorizontal();
                tools.language = (GameLanguage)EditorGUILayout.EnumFlagsField("Select Language", tools.language);
            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset"))
            {
                tools.Reset();
            }
            if (GUILayout.Button("Generate"))
            {
                ExecuteScript();
            }
            EditorGUILayout.EndHorizontal();
        }
        void ExecuteScript()
        {
            if (tools.genPb)
            {
                if (string.IsNullOrEmpty(tools.protoBatchScript))
                {
                    bool b = UnityEditor.EditorUtility.DisplayDialog("ExTools", "Proto Batch Script Path Empty !", "OK", "Reset");
                    if (!b) tools.Reset();
                    return;
                }
                exeFuns.Enqueue(ExecuteProtoScript);
            }
            if (tools.genCfg)
            {
                if (string.IsNullOrEmpty(tools.cfgBatchScript))
                {
                    bool b = UnityEditor.EditorUtility.DisplayDialog("ExTools", "Config Batch Script Path Empty !", "OK", "Reset");
                    if (!b) tools.Reset();
                    return;
                }
                exeFuns.Enqueue(ExecuteCfgScript);
            }
            if (tools.genLoc)
            {
                if (string.IsNullOrEmpty(tools.locBatchScript))
                {
                    bool b = UnityEditor.EditorUtility.DisplayDialog("ExTools", "Localization Batch Script Path Empty !", "OK", "Reset");
                    if (!b) tools.Reset();
                    return;
                }
                if (string.IsNullOrEmpty(tools.GetLan()))
                {
                    bool b = UnityEditor.EditorUtility.DisplayDialog("ExTools", "You must select a language !", "OK", "Reset");
                    if (!b) tools.Reset();
                    return;
                }
                exeFuns.Enqueue(ExecuteLocScript);
            }
            if (exeFuns.Count > 0) exeFuns.Dequeue().Invoke();
        }
        /// <summary>
        /// 执行生成pb文件
        /// </summary>
        void ExecuteProtoScript()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            progress = 0.0f;
            isError = false;
            if (!string.IsNullOrEmpty(tools.protoPath))
                filesCount = Utility.Files.GetFiles(tools.protoPath, ".proto").Length;
            var protoPath = Utility.Path.GetFullPath(tools.protoPath);
            var pbOutPath = Utility.Path.GetFullPath(tools.pbOutPath);
            EditorUtility.ExecuteScript(tools.protoBatchScript, (succ, msg, error) =>
            {
                isError = !string.IsNullOrEmpty(error);
                progressMsg = isError ? error : msg;
                isDone = succ;
            }, protoPath, pbOutPath);
        }
        /// <summary>
        /// 执行生成配置文件
        /// </summary>
        void ExecuteCfgScript()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            progress = 0.0f;
            isError = false;
            if (!string.IsNullOrEmpty(tools.xlsxPath))
                filesCount = Utility.Files.GetFiles(tools.xlsxPath, ".xls", ".xlsx").Length;
            var xlsxPath = Utility.Path.GetFullPath(tools.xlsxPath);
            var luaCfgPath = Utility.Path.GetFullPath(tools.luaCfgPath);
            EditorUtility.ExecuteScript(tools.cfgBatchScript, (succ, msg, error) =>
            {
                isError = !string.IsNullOrEmpty(error);
                progressMsg = isError ? error : msg;
                isDone = succ;
            }, xlsxPath, luaCfgPath);
        }
        /// <summary>
        /// 执行生成多语言表
        /// </summary>
        void ExecuteLocScript()
        {
            UnityEditor.EditorUtility.ClearProgressBar();
            progress = 0.0f;
            isError = false;
            if (!string.IsNullOrEmpty(tools.locXlsxPath))
                filesCount = Utility.Files.GetFiles(tools.locXlsxPath, ".xls", ".xlsx").Length;
            var locXlsxPath = Utility.Path.GetFullPath(tools.locXlsxPath);
            var locCfgPath = Utility.Path.GetFullPath(tools.locCfgPath);
            EditorUtility.ExecuteScript(tools.locBatchScript, (succ, msg, error) =>
            {
                isError = !string.IsNullOrEmpty(error);
                progressMsg = isError ? error : msg;
                isDone = succ;
            }, locXlsxPath, locCfgPath, tools.GetLan());

            Log.Debug(tools.GetLan());
        }
        void Update()
        {
            if (isDone)
            {
                isDone = false;
                if (exeFuns.Count > 0) { exeFuns.Dequeue().Invoke(); return; }
                progressMsg = string.Empty;
                UnityEditor.EditorUtility.ClearProgressBar();
                UnityEditor.EditorUtility.DisplayDialog("Succes", "Succes!!!", "ok");

                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            if (!string.IsNullOrEmpty(progressMsg))
            {
                if (isError)
                {
                    exeFuns.Clear();

                    UnityEditor.EditorUtility.ClearProgressBar();
                    UnityEditor.EditorUtility.DisplayDialog("Error", progressMsg, "ok");
                    progressMsg = string.Empty;

                    AssetDatabase.Refresh();
                    AssetDatabase.SaveAssets();
                }
                else
                {
                    if (progressMsg.Contains("parse"))
                    {
                        progress += 1;
                    }
                    UnityEditor.EditorUtility.DisplayProgressBar("parse...", progressMsg, progress / filesCount);
                }
            }
        }
    }
}