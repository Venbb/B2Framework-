using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public partial class BuildHelperWindow : EditorWindow
    {
        /// <summary>
        /// 重写兼容性配置
        /// </summary>
        static private void OverriteSettingsCompatibility(BuildTarget buildTarget)
        {
            //强制设置资源加载模式为真机环境
            shaco.GameHelper.gameConfig.WriteEnum(shaco.GameHelper.res.resourcesLoadMode.ToTypeString(), shaco.ResourcesLoadMode.RunTime);

            //强制设置本地优先读取资源模式
            shaco.GameHelper.gameConfig.WriteEnum(shaco.GameHelper.res.resourcesLoadOrder.ToTypeString(), shaco.ResourcesLoadOrder.DownloadFirst);

            //强制设置本地使用UnityAsset方式读取excel文件效率很高
            shaco.GameHelper.excelSetting.runtimeLoadFileType = shaco.Base.ExcelData.RuntimeLoadFileType.UnityAsset;

            //如果是webgl平台不能使用HttpHelper
            //1、HttpHelper是多线程实现
            //2、Unity没有在WebGL平台没有实现System.Net内容
            //所以使用UnityHttpHelper代替，它使用的是UnityWebRequest
            if (IsWebGLPlatform(buildTarget))
            {
                var newHttpHelperTypeString = shaco.GameHelper.gameConfig.ReadString(typeof(shaco.Base.IHttpHelper).ToTypeString());
                shaco.GameHelper.gameConfig.WriteString("BuildHelpwerWindow+Compatibility.oldHttpHelper", newHttpHelperTypeString);
                if (newHttpHelperTypeString == typeof(shaco.Base.HttpHelper).ToTypeString())
                {
                    shaco.GameHelper.gameConfig.WriteString(typeof(shaco.Base.IHttpHelper).ToTypeString(), typeof(shaco.UnityHttpHelper).ToTypeString());
                }
            }

            shaco.GameHelper.gameConfig.CheckSaveModifyData();
        }

        /// <summary>
        /// 恢复之前临时重写的兼容性配置
        /// </summary>
        static private void RevertOverriteSettingsCompatibility(BuildTarget buildTarget)
        {
            //恢复设置资源加载模式
            var oldResLoadModeString = shaco.GameHelper.gameConfig.ReadString("BuildHelpwerWindow+Compatibility.oldResLoadModeString");
            if (!string.IsNullOrEmpty(oldResLoadModeString))
            {
                shaco.GameHelper.gameConfig.Remove("BuildHelpwerWindow+Compatibility.oldResLoadModeString");
                shaco.GameHelper.gameConfig.WriteString(shaco.GameHelper.res.resourcesLoadMode.ToTypeString(), oldResLoadModeString);
                shaco.GameHelper.res.resourcesLoadMode = oldResLoadModeString.ToEnum<shaco.ResourcesLoadMode>();
            }

            //恢复读取资源模式
            var oldResLoadOrderString = shaco.GameHelper.gameConfig.ReadString("BuildHelpwerWindow+Compatibility.oldResLoadOrderString");
            if (!string.IsNullOrEmpty(oldResLoadOrderString))
            {
                shaco.GameHelper.gameConfig.Remove("BuildHelpwerWindow+Compatibility.oldResLoadOrderString");
                shaco.GameHelper.gameConfig.WriteString(shaco.GameHelper.res.resourcesLoadOrder.ToTypeString(), oldResLoadOrderString);
                shaco.GameHelper.res.resourcesLoadOrder = oldResLoadOrderString.ToEnum<shaco.ResourcesLoadOrder>();
            }

            if (IsWebGLPlatform(buildTarget))
            {
                //如果打包前后的httphelper不一致则是被修改过了
                //打包完成后恢复原来的httphelper设置
                var oldHttpHelperTypeString = shaco.GameHelper.gameConfig.ReadString("BuildHelpwerWindow+Compatibility.oldHttpHelper");
                var newHttpHelperTypeString = shaco.GameHelper.gameConfig.ReadString(typeof(shaco.Base.IHttpHelper).ToTypeString());
                if (!string.IsNullOrEmpty(oldHttpHelperTypeString) && !string.IsNullOrEmpty(newHttpHelperTypeString))
                {
                    if (oldHttpHelperTypeString != newHttpHelperTypeString) 
                    {
                        shaco.GameHelper.gameConfig.Remove("BuildHelpwerWindow+Compatibility.oldHttpHelper");
                        shaco.GameHelper.gameConfig.WriteString(typeof(shaco.Base.IHttpHelper).ToTypeString(), oldHttpHelperTypeString);
                        AssetDatabase.Refresh();
                    }
                }
            }
            
            shaco.GameHelper.gameConfig.CheckSaveModifyData();
        }

        static private bool IsWebGLPlatform(BuildTarget buildTarget)
        {
#if UNITY_5_4_OR_NEWER
            if (buildTarget == BuildTarget.WebGL)
                return true;
#else
            if (buildTarget == BuildTarget.WebGL || buildTarget == BuildTarget.Web√Player || buildTarget == BuildTarget.WebPlayerStreamed)
                return true;
#endif
            return false;
        }
    }
}