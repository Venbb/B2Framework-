using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class CreateScriptBaseMenu : Editor
    {
        //模板文件名字
        virtual public string templateFileName { get; set; }

        //icon名字
        virtual public string scriptIconName { get { return "cs Script Icon"; } }

        //默认文件名字
        virtual public System.Action<string> callbackCreate { get; }

        private class DoCreateScriptObjectCallBack : UnityEditor.ProjectWindowCallback.EndNameEditAction
        {
            //编辑结束回调
            public System.Action<string> callbackEnd = null;
            public System.Action<string> callbackCreate = null;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                var assetPath = EditorHelper.FullPathToUnityAssetPath(pathName);
                if (null != callbackCreate)
                {
                    callbackCreate(assetPath);

                    if (null != callbackEnd)
                    {
                        callbackEnd(assetPath);
                    }
                    return;
                }
                var readTemplateScriptText = shaco.Base.FileHelper.ReadAllByUserPath(resourceFile);

                //根据写入名字重新设定类名
                var className = shaco.Base.FileHelper.GetLastFileName(pathName, false);
                readTemplateScriptText = readTemplateScriptText.Replace("#ClassName#", className);

                //写入新的脚本文件
                shaco.Base.FileHelper.WriteAllByUserPath(pathName, readTemplateScriptText);

                //刷新编辑器
                AssetDatabase.ImportAsset(assetPath);

                if (null != callbackEnd)
                {
                    callbackEnd(assetPath);
                }
            }
        }

        public void CreateScriptBaseScript(System.Action<string> callbackEnd = null)
        {
            if (string.IsNullOrEmpty(templateFileName))
            {
                throw new System.NotImplementedException("Should override 'templateFileName' parameter");
            }

            //输入需要添加的脚本名字
            var fullTemplateFileName = templateFileName.EndsWith(".txt") ? templateFileName : templateFileName + ".txt";
            var templatePath = shaco.Base.GlobalParams.GetShacoUnityEditorResourcesPath().ContactPath(fullTemplateFileName);

            Texture2D csIcon = EditorGUIUtility.IconContent(scriptIconName).image as Texture2D;
            var endNameEditAction = ScriptableObject.CreateInstance<DoCreateScriptObjectCallBack>();
            endNameEditAction.callbackEnd = callbackEnd;
            endNameEditAction.callbackCreate = callbackCreate;
            UnityEditor.ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, endNameEditAction, templateFileName, csIcon, templatePath);
        }
    }
}