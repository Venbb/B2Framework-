using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ProjectWindowCallback;
using System.Text.RegularExpressions;
using System.Text;
using System;

namespace B2Framework.Editor
{
    public static class LuaScriptTemplate
    {
        const string DEFAULT_NAME = "NewLua" + AppConst.LUA_EXTENSION;
        const string TEMPLATE_PATH = "Assets/B2Framework/Scripts/Editor/Template/luatemplate.lua";

        // [MenuItem("Assets/Create/Lua Script", false, 80)]
        public static void CreatNewLua()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0,
            ScriptableObject.CreateInstance<LuaScriptCreateAction>(),
            Path.Combine(GetSelectedPathOrFallback(), DEFAULT_NAME),
            null,
            TEMPLATE_PATH);
        }
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }
    }
    class LuaScriptCreateAction : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            UnityEngine.Object o = CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(o);
        }

        internal static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile)
        {
            string fullPath = Path.GetFullPath(pathName);
            StreamReader streamReader = new StreamReader(resourceFile);
            string text = streamReader.ReadToEnd();
            streamReader.Close();
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathName);
            text = Regex.Replace(text, "SCRIPTNAME", fileNameWithoutExtension);
            text = Regex.Replace(text, "#AUTHOR#", Environment.UserName);
            text = Regex.Replace(text, "#DATE#", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            bool encoderShouldEmitUTF8Identifier = true;
            bool throwOnInvalidBytes = false;
            UTF8Encoding encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier, throwOnInvalidBytes);
            bool append = false;
            StreamWriter streamWriter = new StreamWriter(fullPath, append, encoding);
            streamWriter.Write(text);
            streamWriter.Close();
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        }
    }
}