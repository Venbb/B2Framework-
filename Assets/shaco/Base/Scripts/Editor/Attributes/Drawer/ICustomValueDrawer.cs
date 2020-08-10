using System.Collections;
using System.Collections.Generic;

namespace shacoEditor
{
    /// <summary>
    /// shaco框架中自定义的对象绘制方法
    /// 在GUILayoutHelper+DrawValue.DrawValue方法中，当没有找到匹配的默认绘制类型时候自动生效
    /// </summary>
    public interface ICustomValueDrawer
    {
        /// <summary>
        /// 绘制的数据类型
        /// </summary>
        System.Type valueType { get; }

        /// <summary>
        /// 绘制数据的方法
        /// <param name="name">数据名称，可能为string.Empty</param>
        /// <param name="value">数据(类型同valueType参数一致)</param>
        /// <param name="type">数据类型(类型同valueType参数一致)</param>
        /// <param name="customArg">外部传入的自定义参数，可能为空</param>
        /// <return>当前数据</return>
        /// </summary>
        object DrawValue(string name, object value, System.Type type, object customArg);
    }
}