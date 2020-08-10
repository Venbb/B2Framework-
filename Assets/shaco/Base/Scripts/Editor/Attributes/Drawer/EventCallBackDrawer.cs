using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace shacoEditor
{
    public class EventCallBackDrawer<T> : ICustomValueDrawer
    {
        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        public System.Type valueType { get { return typeof(shaco.Base.EventCallBack<T>); } }

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <return>当前数据</return>
        /// </summary>
        public object DrawValue(string name, object value, System.Type type, object customArg)
		{
            //暂时不绘制回调方法，以后有需要再添加吧
            return null;
		}
    }

    public class EventCallBackDrawer : ICustomValueDrawer
    {
        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        public System.Type valueType { get { return typeof(shaco.Base.EventCallBack); } }

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <return>当前数据</return>
        /// </summary>
        public object DrawValue(string name, object value, System.Type type, object customArg)
        {
            //暂时不绘制回调方法，以后有需要再添加吧
            return null;
        }
    }
}