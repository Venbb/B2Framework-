using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shacoEditor
{
    /// <summary>
    /// 打包完成后回调
    /// </summary>
    public sealed class PostProcessBuildedAttribute : shaco.Base.OrderCallBackAttribute
    {
        public PostProcessBuildedAttribute() { }
        public PostProcessBuildedAttribute(int callbackOrder) :base(callbackOrder) { }

        /// <summary>
        /// 回调方法和参数例子，距离用法参考UnityEditor.Callbacks.PostProcessBuild
        /// </summary>
        // public static void OnPostProcessBuilded(UnityEditor.BuildTarget target, string projectRootPath)
        // {

        // }
    } 
}