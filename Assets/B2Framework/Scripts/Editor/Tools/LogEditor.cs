using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;

namespace B2Framework.Editor
{
    public class LogEditor
    {
        const int STATCK_INDEX = 2;
        static bool opening = false;
        [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            if (opening) return false;
            opening = true;

            var statckTrack = GetStackTrace();
            if (string.IsNullOrEmpty(statckTrack)) return false;

            var luaPath = IsLua(statckTrack);
            if (!string.IsNullOrEmpty(luaPath))
            {
                var arr = luaPath.Split(':');
                // var filePath = Path.Combine(Application.dataPath, AppConst.LUA_SCRIPTS_PATH);
                // filePath = Path.Combine(filePath, arr[0]);
                var files = EditorUtility.GetAssetsPath(arr[0]);
                if (files.Length > 0)
                {
                    var li = int.Parse(arr[1]);
                    // fileName = fileName.Replace("/", "\\");
                    var filePath = Path.GetFullPath(files[0]);
                    // Debug.Log(filePath + ":" + li);
                    opening = false;
                    return EditorUtility.OpenFileAtLineExternal(filePath, li);
                }
                // UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filePath, li, 0);
            }
            //以换行分割堆栈信息
            var statcks = statckTrack.Split('\n');
            var fileNames = new List<string>();
            foreach (var fileName in statcks)
            {
                var index = fileName.IndexOf("(at");
                if (index != -1)
                {
                    index += "(at".Length + 1;
                    var path = fileName.Substring(index, fileName.Length - index - 1);
                    fileNames.Add(path);
                }
            }
            if (fileNames.Count > 0)
            {
                if (fileNames.Count > STATCK_INDEX)
                {
                    var fileName = fileNames[STATCK_INDEX];
                    var files = fileName.Split(':');
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(files[0]), int.Parse(files[1]));
                    opening = false;
                    return true;
                }
            }
            opening = false;
            return false;
        }
        /// <summary>
        /// 匹配lua路径，根据.lua后缀
        /// </summary>
        /// <param name="statckTrack"></param>
        /// <returns></returns>
        static string IsLua(string statckTrack)
        {
            Match matches = Regex.Match(statckTrack, @"(\w+\/+)+(\w+.lua:[0-9]+)", RegexOptions.IgnoreCase);
            // for(int j=0;j<matches.Groups.Count;j++)Debug.Log(matches.Groups[j].Value);
            if (matches.Success)
            {
                // 匹配成功
                var luaPath = matches.Groups[0].Value;
                // matches = matches.NextMatch();
                if (luaPath.Contains(".lua")) return luaPath;
            }
            return "";
        }
        /// <summary>
        /// 反射出日志堆栈
        /// </summary>
        /// <returns></returns>
        private static string GetStackTrace()
        {
            var consoleWindowType = typeof(EditorWindow).Assembly.GetType("UnityEditor.ConsoleWindow");
            var fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            var consoleWindowInstance = fieldInfo.GetValue(null);

            if (null != consoleWindowInstance)
            {
                if ((object)EditorWindow.focusedWindow == consoleWindowInstance)
                {
                    fieldInfo = consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    string activeText = fieldInfo.GetValue(consoleWindowInstance).ToString();
                    return activeText;
                }
            }
            return "";
        }
    }
}