using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace B2Framework.Editor
{
    internal static partial class EditorUtility
    {
        /// <summary>
        /// 选中到对象
        /// </summary>
        /// <param name="path"></param>
        internal static void PingObject(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            var idx = path.IndexOf("Assets");
            if (idx == -1) return;
            if (idx > 0) path = path.Substring(idx);
            // Debug.Log(path);
            var obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj != null)
                EditorGUIUtility.PingObject(obj);
        }
        /// <summary>
        /// 是否点击检视面板某个控件
        /// </summary>
        internal static bool IsFieldClick(Rect rect, Event @event)
        {
            if (rect.Contains(@event.mousePosition))
            {
                return @event.isMouse;
            }
            return false;
        }
        /// <summary>
        /// 打开代码编辑器并定位到指定行
        /// </summary>
        /// <param name="fileName">要打开的代码文件名</param>
        /// <param name="line">代码所在行号</param>
        /// <returns></returns>
        internal static bool OpenFileAtLineExternal(string fileName, int line)
        {
            string externalScriptEditor = UnityEditorInternal.ScriptEditorUtility.GetExternalScriptEditor();
            //  @"/Applications/Visual Studio Code.app/Contents/Resources/app/bin/code";
            //  @"/Applications/Sublime Text.app/Contents/SharedSupport/bin/subl";
            if (File.Exists(externalScriptEditor))
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = externalScriptEditor;
                proc.StartInfo.Arguments = Utility.Text.Format("-g {0}:{1}:0", fileName, line);
                proc.Start();
                return true;
            }
            else
            {
                return UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fileName, line, 0);
            }
        }
        /// <summary>
        /// 打开文件夹
        /// </summary>
        /// <param name="path"></param>
        internal static void ExplorerFolder(string path)
        {
            if (Directory.Exists(path) || File.Exists(path))
            {
                UnityEditor.EditorUtility.RevealInFinder(path);
            }
            else
            {
                Log.Error("ExplorerFolder: not found path = " + path);
            }
        }
        internal static bool HasScriptingDefineSymbols(string symbol)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var idx = System.Array.FindIndex(symbolStr.Split(';'), element => element.Equals(symbol));
            return idx != -1;
        }
        /// <summary>
        /// 添加移除宏定义
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="append"></param>
        internal static void SetScriptingDefineSymbols(string symbol, bool append = true)
        {
            var symbolStr = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            var symbols = new List<string>(symbolStr.Split(';'));
            int len = symbols.Count;
            if (symbols.Contains(symbol))
            {
                if (!append) symbols.Remove(symbol);
            }
            else
            {
                if (append) symbols.Add(symbol);
            }
            if (len != symbols.Count)
            {
                symbolStr = string.Join(";", symbols.ToArray());
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, symbolStr);
            }
        }
        /// <summary>
        /// 执行External脚本
        /// </summary>
        /// <param name="path">脚本路径</param>
        /// <param name="extents">传递的参数</param>
        internal static void ExecuteScript(string path, System.Action<bool, string, string> onProcess, params string[] extents)
        {
            string fileName = string.Empty;
            path = System.IO.Path.GetFullPath(path);
            var suffix = System.IO.Path.GetExtension(path);
            switch (suffix)
            {
                case ".py":
                    // 如果没有配置环境变量，这个写成解释器的绝对路径
#if UNITY_EDITOR_WIN
                    fileName = @"python.exe";
#else
                    fileName ="/usr/bin/python";
#endif
                    //fileName = @"C:\Program Files\Python35\python.exe";
                    break;
            }
            if (string.IsNullOrEmpty(fileName)) return;
            // 用文件名当透传参数
            var trans = Utility.Text.Format("{0} Done", System.IO.Path.GetFileNameWithoutExtension(path));
            // 这里就不透传到脚本了，太麻烦，记得在执行完成后输出一下这个参数
            var args = path; //+ " " + trans;
            if (extents.Length > 0)
            {
                for (int i = 0; i < extents.Length; i++)
                    args += " " + extents[i];
            }
            ExecuteExternalScript(fileName, args, trans, onProcess);
        }
        /// <summary>
        /// 执行External脚本
        /// </summary>
        /// <param name="fileName">解释器（执行程序）</param>
        /// <param name="args">传递的参数</param>
        /// <param name="trans">透传参数（用于判断是否执行完成）</param>
        /// <param name="onProcess">打印回调</param>
        internal static void ExecuteExternalScript(string fileName, string args, string trans = "Done", System.Action<bool, string, string> onProcess = null)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = fileName;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.Arguments = args;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.BeginOutputReadLine();
                p.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        var done = e.Data.Contains(trans);
                        var error = e.Data.Contains("Error");

                        if (onProcess != null) onProcess(done, error ? null : e.Data, error ? e.Data : null);
                        if (error)
                        {
                            Log.Error(e.Data);
                            onProcess = null;
                        }
                        else
                            Log.Debug(e.Data);

                        if (done)
                        {
                            Process pr = sender as Process;
                            if (pr != null)
                            {
                                pr.Close();
                            }
                        }
                    }
                });
                // p.WaitForExit();
            }
            catch (System.Exception e)
            {
                Log.Error(e.Message);
            }
        }
    }
}