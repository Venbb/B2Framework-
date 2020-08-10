using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public class CreateResourcePackMenu : CreateScriptBaseMenu
    {
        override public string templateFileName { get { return _templateFileName; } set { _templateFileName = value; } }
        override public string scriptIconName { get { return "ScriptableObject Icon"; } }
        override public System.Action<string> callbackCreate { get { return OnCreateCallBack; } }

        private string _templateFileName = "ResourcePackSettingTemplate.asset";

        [MenuItem("Assets/Create/shaco/ResourcePackSetting", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_RESOURCE_PACK_SETTING)]
        static public void CreateResourcePackSetting()
        {
            if (null == Selection.assetGUIDs || Selection.assetGUIDs.Length == 0)
            {
                Debug.LogError("CreateResourcePackMenu CreateResourcePackSetting error: no select");
                return;
            }
            
            var assetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs[0]);
            if (!AssetDatabase.IsValidFolder(assetPath))
                assetPath = System.IO.Path.GetDirectoryName(assetPath);
            var directoryInfo = new System.IO.DirectoryInfo(assetPath);

            //如果已经存在重名文件，则ping到它，而不是再创建一个
            var fullPath = directoryInfo.FullName.ContactPath(directoryInfo.Name + ".asset");
            if (System.IO.File.Exists(fullPath))
            {
                var loadAsset = AssetDatabase.LoadAssetAtPath<Object>(EditorHelper.FullPathToUnityAssetPath(fullPath));
                EditorGUIUtility.PingObject(loadAsset);
                return;
            }

            var packMenu = new CreateResourcePackMenu();
            packMenu._templateFileName = directoryInfo.Name + ".asset";
            packMenu.CreateScriptBaseScript();
        }

        [MenuItem("Assets/Create/shaco/ResourcePackSetting", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_RESOURCE_PACK_SETTING)]
        static public bool CreateResourcePackSettingValid()
        {
            return null != Selection.assetGUIDs && Selection.assetGUIDs.Length > 0;
        }

        private void OnCreateCallBack(string assetPath)
        {
            var newAsset = ScriptableObject.CreateInstance<shaco.ResourcePackSetting>();
            AssetDatabase.CreateAsset(newAsset, assetPath);
        }
    }
}