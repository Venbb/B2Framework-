using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class GuideRuntimeObjectDrawer<T> : shacoEditor.GuideResourceObjectDrawer<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        override public System.Type valueType { get { return typeof(shaco.GuideRuntimeObject<T>); } }

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <return>当前数据</return>
        /// </summary>
        override public object DrawValue(string name, object value, System.Type type, object customArg)
        {
            return DrawValueBase(name, value, type, customArg, true);
        }
    }
}