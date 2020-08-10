using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class CreateExcelScriptMenu : Editor
    {
        /// <summary>
        /// 同步项目中配置过的所有表数据为asset文件
        /// </summary>
        static public void SyncAllExcelAsset()
        {
            var customExportScriptPath = shaco.Base.GameHelper.excelSetting.GetAllTextExportPath();

            if (customExportScriptPath.IsNullOrEmpty())
            {
                Debug.LogError("CreateExcelScriptMenu+Shell SyncAllExcelAsset error: no export text path");
                return;
            }

            var foldersAsset = new List<string>();
            for (int i = 0; i < customExportScriptPath.Length; ++i)
            {
                var folderAsset = EditorHelper.FullPathToUnityAssetPath(customExportScriptPath[i]);
                if (null == folderAsset)
                {
                    Debug.LogError("CreateExcelScriptMenu+Shell SyncAllExcelAsset error: can't load path=" + customExportScriptPath[i]);
                    continue;
                }
                foldersAsset.Add(folderAsset);
            }

            if (foldersAsset.IsNullOrEmpty())
                return;

            Debug.Log("CreateExcelScriptMenu+Shell SyncAllExcelAsset: start create... count=" + foldersAsset.Count);
            CreateExcelSerializableAsset(foldersAsset.ToArray());
        }
    }
}