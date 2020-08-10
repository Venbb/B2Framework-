using UnityEngine;
using System.Collections;
using System;
using System.IO;
using UnityEditor;

namespace shacoEditor
{
    public static class BuildHelperListener
    {
        [shacoEditor.PostProcessGeneratorXLuaAttribute]
        static private void OnGeneratorXluaCallBack()
        {
            CSObjectWrapEditor.Generator.GenAll();
        }

        [shacoEditor.PostProcessClearGeneratorXLuaAttribute]
        static private void OnClearGeneratorXluaCallBack()
        {
            //不再需要每次都清理xlua脚本，实际上没啥必要了
            // CSObjectWrapEditor.Generator.ClearAll();
        }

        [shacoEditor.PostProcessOverwriteXcodePlistAttribute]
        static private void OnOverwriteXcodePlistAttribute(string exportXcodePath, shacoEditor.BuildHelperWindow.IOSExportOption option, string profileSpecifier, System.Action callbackEnd)
        {
            //设置plist内容
            var exportOptionPlistPath = GetFullShellScriptsPath("iOS/export_option.plist");
            if (!shaco.Base.FileHelper.ExistsFile(exportOptionPlistPath))
            {
                Debug.LogError("BuildHelperWindow BuildXcodeAndiPA erorr: missing exportOption.plist");
                return;
            }
            var exportOptionPlist = new shaco.iOS.Xcode.PlistDocument();
            exportOptionPlist.ReadFromFile(exportOptionPlistPath);
            var exportOptionDict = exportOptionPlist.root.AsDict();

            //set method
            switch (option)
            {
                case BuildHelperWindow.IOSExportOption.AdHoc: exportOptionDict.SetString("method", "ad-hoc"); break;
                case BuildHelperWindow.IOSExportOption.AppStore: exportOptionDict.SetString("method", "app-store"); break;
                case BuildHelperWindow.IOSExportOption.Enterprise: exportOptionDict.SetString("method", "enterprise"); break;
                case BuildHelperWindow.IOSExportOption.Development: exportOptionDict.SetString("method", "development"); break;
                default: Debug.LogError("BuildHelperWindow BuildXcodeAndiPA error: unsupport export option=" + option); return;
            }

            //set provisioningProfiles
#if UNITY_5_3_OR_NEWER
            var bundleIdentifier = PlayerSettings.applicationIdentifier;
#else
            var bundleIdentifier = PlayerSettings.bundleIdentifier;
#endif
            var exportOptionProvisioningDic = exportOptionDict["provisioningProfiles"].AsDict();
            exportOptionProvisioningDic.values.Clear();
            exportOptionDict["provisioningProfiles"].AsDict().SetString(bundleIdentifier, profileSpecifier);
            exportOptionPlist.WriteToFile(exportOptionPlistPath);

            //这里必须有回调，所以不判断为空了
            callbackEnd();
        }

        static private string GetFullShellScriptsPath(string replativePath)
        {
            return shaco.Base.FileHelper.GetCurrentSourceFolderPath().RemoveLastPathByLevel(2).ContactPath("/ShellScripts/" + replativePath);
        }
    }
}