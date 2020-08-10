using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class CreateScriptableObjectMenu : CreateScriptBaseMenu
    {
        override public string templateFileName { get { return "ScriptableObjectTemplate.cs"; }}

		[MenuItem("Assets/Create/shaco/ScriptableObject(Script)", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_SCRIPTABLE_OBJECT)]
        static public void CreateScriptableObject()
        {
            new CreateScriptableObjectMenu().CreateScriptBaseScript();
        }

        [MenuItem("Assets/Create/shaco/ScriptableObject(Asset)", false, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_SCRIPTABLE_OBJECT_ASSET)]
        static public void CreateScriptableObjectAsset()
		{
			var selectedScripts = GetCurrentSelectScriptableScripts();
			if (selectedScripts.IsNullOrEmpty())
				return;

			var saveFolderPath = EditorHelper.FullPathToUnityAssetPath(GetCurrentSelectFolderPath());
			for (int i = 0; i < selectedScripts.Length; ++i)
			{
                CreateScriptableObjectAsset(saveFolderPath, selectedScripts[i]);
			}
		}

        [MenuItem("Assets/Create/shaco/ScriptableObject(Asset)", true, (int)ToolsGlobalDefine.ProjectMenuPriority.CREATE_SCRIPTABLE_OBJECT_ASSET)]
        static public bool CreateScriptableObjectAssetValid()
        {
            return !GetCurrentSelectScriptableScripts().IsNullOrEmpty();
        }

		/// <summary>
		/// 根据脚本文本获取unity序列化脚本
		/// <param name="scriptText">脚本文件字符串内容</param>
		/// </summary>
		static public string CreateScriptableObjectAsset(string saveFolderPath, TextAsset scriptAsset)
		{
			var assetPath = string.Empty;
			var scriptText = scriptAsset.ToString();
            var fullClassName = shaco.Base.Utility.GetFullClassName(ref scriptText, typeof(UnityEngine.ScriptableObject));

            if (!string.IsNullOrEmpty(fullClassName))
            {
				var className = fullClassName.RemoveFront(".", fullClassName.LastIndexOf("."));
				var fileName = shaco.Base.FileHelper.ReplaceLastExtension(className, "asset");
				assetPath = saveFolderPath.ContactPath(fileName);

				if (!shaco.Base.FileHelper.ExistsFile(EditorHelper.GetFullPath(assetPath)))
				{
					var instantiateScript = UnityEngine.ScriptableObject.CreateInstance(fullClassName);
					if (null != instantiateScript)
					{
						assetPath = EditorHelper.FullPathToUnityAssetPath(assetPath);
						AssetDatabase.CreateAsset(instantiateScript, assetPath);
					}
				}
            }
			return assetPath;
		}

		/// <summary>
		/// 获取当前选中的脚本文本
		/// </summary>
		static public TextAsset[] GetCurrentSelectScriptableScripts()
		{
            TextAsset[] retValue = null;
            if (Selection.objects == null || Selection.objects.Length == 0)
                return retValue;

			List<TextAsset> findScriptsAsset = new List<TextAsset>();
			System.Func<Object, bool> checkAddScriptFuntion = (Object obj) =>
			{
				if (null == obj)
					return false;

				var textAssetTmp = obj as TextAsset;
				if (null != textAssetTmp && AssetDatabase.GetAssetPath(obj).EndsWith(".cs"))
				{
					return obj.ToString().Contains("ScriptableObject");
				}
				else
					return false;
			};

            for (int i = 0; i < Selection.objects.Length; ++i)
            {
				var scriptObject = Selection.objects[i];

                //判断对象是否继承自ScriptableObject
				var assetPath = AssetDatabase.GetAssetPath(scriptObject);

				//如果是文件夹，则遍历一级目录
				if (shaco.Base.FileHelper.ExistsDirectory(EditorHelper.GetFullPath(assetPath)))
				{
					var allFiles = shaco.Base.FileHelper.GetFiles(assetPath, "*.cs", System.IO.SearchOption.TopDirectoryOnly);
					if (null != allFiles)
					{
						foreach (var iter in allFiles)
						{
							var assetTmp = AssetDatabase.LoadAssetAtPath<TextAsset>(EditorHelper.FullPathToUnityAssetPath(iter));
							if (checkAddScriptFuntion(assetTmp))
								findScriptsAsset.Add(assetTmp);
						}
					}
				}
				else
				{
					if (checkAddScriptFuntion(scriptObject))
						findScriptsAsset.Add(scriptObject as TextAsset);
				}
            }

			retValue = findScriptsAsset.ToArray();
			return retValue;
		}

		/// <summary>
		/// 获取当前选择的文件夹路径
		/// </summary>
		static private string GetCurrentSelectFolderPath()
		{
            string retValue = string.Empty;
            if (Selection.objects == null || Selection.objects.Length == 0)
                return retValue;

            //获取以当前打开的文件夹为父节点的路径
            var assetPath = AssetDatabase.GetAssetPath(Selection.objects[0]);
            if (shaco.Base.FileHelper.ExistsFile(assetPath))
                assetPath = shaco.Base.FileHelper.GetFolderNameByPath(assetPath);

            retValue = assetPath;
			return retValue;
		}
    }
}
