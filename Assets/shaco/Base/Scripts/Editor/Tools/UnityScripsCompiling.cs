using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    [InitializeOnLoad]
    public class UnityScripsCompiling
    {
        static UnityScripsCompiling()
        {
            EditorApplication.update -= DidReloadScriptsCallBack;
            EditorApplication.update += DidReloadScriptsCallBack;
        }

        //该属性不太安全，在shell调用unity脚本时候会导致各种意外bug
        // [UnityEditor.Callbacks.DidReloadScripts]
        private static void DidReloadScriptsCallBack()
        {
            if (!EditorApplication.isCompiling)
            {
                CreateUIScriptMenu.AddScriptToPrefab();

                if (shaco.GameHelper.datasave.ReadBool("BuildHelperWindow+Build.WaitCompile"))
                {
                    Debug.Log("shacoEditorLog.UnityScripsCompiling.DidReloadScriptsCallBack: [BuildHelperWindow+Build.WaitCompile]");
                    shaco.GameHelper.datasave.Remove("BuildHelperWindow+Build.WaitCompile");
                    BuildHelperWindow.UpdateProjectDefinesEndCallBack();
                }
                EditorApplication.update -= DidReloadScriptsCallBack;
            }
        }
    }
}