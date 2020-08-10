using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace shacoEditor
{
    /// <summary>
    /// 打包前回调
    /// </summary>
    public sealed class PostProcessWillBuildAttribute : shaco.Base.OrderCallBackAttribute
    {
        public PostProcessWillBuildAttribute() { }
        public PostProcessWillBuildAttribute(int callbackOrder) : base(callbackOrder) { }

        /// <summary>
        /// 回调方法和参数例子，距离用法参考UnityEditor.Callbacks.PostProcessBuild
        /// </summary>
        // public static void OnPostProcessWillBuild(UnityEditor.BuildTarget target, string projectRootPath)
        // {

        // }
    } 
}