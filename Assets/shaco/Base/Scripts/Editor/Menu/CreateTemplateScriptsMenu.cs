using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

namespace shacoEditor
{
    public class CreateLuaBehaviourMenu : CreateScriptBaseMenu
    {
        //模板文件名字
        override public string templateFileName { get { return "LuaBehaviourTemplate" + shaco.Base.GlobalParams.LUA_FILE_EXTENSIONS; } }

        //图标
        override public string scriptIconName { get { return "TextAsset Icon"; } }

        [MenuItem("Assets/Create/shaco/LuaBehaviourScript", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_LUA_BEHAVIOUR_SCRIPT)]
        static public void CreateBehaviourProcessScript()
        {
            new CreateLuaBehaviourMenu().CreateScriptBaseScript(assetPath =>
            {
                //检查是否有重名的情况，并报错提示
                var fileName = System.IO.Path.GetFileName(assetPath);

                var fileNameWithExtension = fileName;
                var indexFind = fileNameWithExtension.LastIndexOf('.');
                if (indexFind > 0)
                    fileNameWithExtension = fileNameWithExtension.Remove(indexFind);

                var searchResult = AssetDatabase.FindAssets(fileNameWithExtension + " t:TextAsset", new string[] { "Assets" });
                if (null != searchResult && searchResult.Length > 1)
                {
                    var searchPaths = searchResult.Convert(v => AssetDatabase.GUIDToAssetPath(v)).Where(v => System.IO.Path.GetFileName(v) == fileName).ToArray();
                    if (searchPaths.Length > 1)
                        Debug.LogError("DoCreateScriptObjectCallBack Action error: has duplicate file name\n" + searchPaths.ToContactString("\n"));
                }
            });
        }
    }
    
    public class CreateBehaviourProcessMenu : CreateScriptBaseMenu
    {
        //模板文件名字
        override public string templateFileName { get { return "BehaviourProcessTemplate.cs"; } }

        [MenuItem("Assets/Create/shaco/BehaviourProcessScript", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_BEHAVIOUR_PROCESS_SCRIPT)]
        static public void CreateBehaviourProcessScript()
        {
            new CreateBehaviourProcessMenu().CreateScriptBaseScript();
        }
    }

}