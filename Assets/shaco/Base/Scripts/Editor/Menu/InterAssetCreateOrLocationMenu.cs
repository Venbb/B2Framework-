using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

namespace shacoEditor
{
    public partial class InterAssetCreateOrLocationMenu : Editor
    {
        static private string DATA_TYPES_SETTING_PATH { get { return System.IO.Path.Combine(Application.dataPath, "Resources/" + shaco.GameHelper.dateTypesTemplate.GetType().Name + ".asset"); } }
        static private string EXCEL_DATA_SETTING_PATH { get { return System.IO.Path.Combine(Application.dataPath, "Resources/" + shaco.GameHelper.excelSetting.GetType().Name + ".asset"); } }
        static private string SPRITE_ATLAS_SETTING_PATH { get { return shaco.GameHelper.gameConfig.ReadString(shaco.SpriteAtlasSettings.SETTINGS_PATH_KEY); } }

        [MenuItem("shaco/InternalAsset/Build Version", false, (int)ToolsGlobalDefine.InternalAssetMenuPriority.BUILD_VERION)]
        static private void CreateBuildVerionFile()
        {
            var fileAsset = EditorHelper.GetBuildVerionFile();
            if (null == fileAsset)
            {
                fileAsset = EditorHelper.SetBuildVersionFile();
            }
            EditorGUIUtility.PingObject(fileAsset);
        }

        [MenuItem("shaco/InternalAsset/Local Save Data", false, (int)ToolsGlobalDefine.InternalAssetMenuPriority.LOCAL_SAVE_DATA)]
        static private void ShowDataSaveFile()
        {
            EditorHelper.ShowInFolder(shaco.GameHelper.datasave.savePath);
        }

        [MenuItem("shaco/InternalAsset/DataTypesSetting", false, (int)ToolsGlobalDefine.InternalAssetMenuPriority.DATA_TYPES_SETTING)]
        static private void CreateDataTypesSetting()
        {
            CreateOrLocacationSetting(shaco.GameHelper.dateTypesTemplate.GetType(), DATA_TYPES_SETTING_PATH);
        }

        [MenuItem("shaco/InternalAsset/DataTypesSetting", true, (int)ToolsGlobalDefine.InternalAssetMenuPriority.DATA_TYPES_SETTING)]
        static private bool CreateDataTypesSettingValid()
        {
            return shaco.GameHelper.dateTypesTemplate.GetType().IsInherited<ScriptableObject>();
        }

        [MenuItem("shaco/InternalAsset/ExcelDataSetting", false, (int)ToolsGlobalDefine.InternalAssetMenuPriority.EXCEL_DATA_SETTING)]
        static private void CreateExcelDataSetting()
        {
            CreateOrLocacationSetting(shaco.GameHelper.excelSetting.GetType(), EXCEL_DATA_SETTING_PATH);
        }

        [MenuItem("shaco/InternalAsset/ExcelDataSetting", true, (int)ToolsGlobalDefine.InternalAssetMenuPriority.EXCEL_DATA_SETTING)]
        static private bool CreateExcelDataSettingValid()
        {
            return shaco.GameHelper.excelSetting.GetType().IsInherited<ScriptableObject>();
        }

        [MenuItem("shaco/InternalAsset/SpriteAtlasSetting", false, (int)ToolsGlobalDefine.InternalAssetMenuPriority.EXCEL_DATA_SETTING)]
        static private void CreateSpriteAtlasSetting()
        {
            if (System.IO.File.Exists(SPRITE_ATLAS_SETTING_PATH))
            {
                var loadAsset = AssetDatabase.LoadAssetAtPath(SPRITE_ATLAS_SETTING_PATH, shaco.GameHelper.atlasSettings.GetType());
                EditorGUIUtility.PingObject(loadAsset);
                AssetDatabase.OpenAsset(loadAsset);
            }
            else
            {
                SpriteAtlasSettingsWindow.OpenSpriteAtlasSettingsWindow();
            }
        }

        [MenuItem("shaco/InternalAsset/SpriteAtlasSetting", true, (int)ToolsGlobalDefine.InternalAssetMenuPriority.EXCEL_DATA_SETTING)]
        static private bool CreateSpriteAtlasSettingValid()
        {
            return shaco.GameHelper.atlasSettings.GetType().IsInherited<ScriptableObject>();
        }

        static private void CreateOrLocacationSetting(System.Type type, string settingPath)
        {
            var loadAsset = LoadSetingAsset(type, settingPath);
            if (null != loadAsset)
            {
                EditorGUIUtility.PingObject(loadAsset);
                AssetDatabase.OpenAsset(loadAsset);
                return;
            }
            var assetFolder = System.IO.Path.GetDirectoryName(settingPath);
            if (!System.IO.Directory.Exists(assetFolder))
            {
                System.IO.Directory.CreateDirectory(assetFolder);
                AssetDatabase.ImportAsset(assetFolder);
            }

            var relativeAssetPath = settingPath.Remove(0, Application.dataPath.Length - "Assets".Length);
            var newAsset = ScriptableObject.CreateInstance(type);
            AssetDatabase.CreateAsset(newAsset, relativeAssetPath);
            EditorGUIUtility.PingObject(newAsset);
            AssetDatabase.OpenAsset(newAsset);
        }

        static private Object LoadSetingAsset(System.Type type, string settingPath)
        {
            var relativePath = settingPath.Remove(0, settingPath.IndexOf("/Resources/") + "/Resources/".Length);
            relativePath = relativePath.Remove(relativePath.LastIndexOf('.'));
            var retValue = Resources.Load(relativePath, type);
            return retValue;
        }
    }
}