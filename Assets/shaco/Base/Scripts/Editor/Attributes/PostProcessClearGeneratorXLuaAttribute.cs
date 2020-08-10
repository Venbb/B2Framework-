using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shacoEditor
{
    /// <summary>
    /// 需要清空构建xlua脚本信息
    /// </summary>
    public sealed class PostProcessClearGeneratorXLuaAttribute : shaco.Base.OrderCallBackAttribute
    {
        public PostProcessClearGeneratorXLuaAttribute() { }
        public PostProcessClearGeneratorXLuaAttribute(int callbackOrder) : base(callbackOrder) { }

        /// <summary>
        /// 回调方法和参数例子，距离用法参考UnityEditor.Callbacks.PostProcessBuild
        /// </summary>
        // public static void OnPostProcessBuilded()
        // {

        // }
    } 
}