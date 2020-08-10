using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface IDataTypesTemplate : IGameInstance
    {
        /// <summary>
        /// 获取unity默认的类型模板
        /// </summary>
        shaco.Base.StringTypePair[] GetTypesTemplate();

		/// <summary>
        /// 根据配置表将类型转换为字符串，如果转换失败则使用c#默认的类型标签
        /// <param name="type">数据类型</param>
        /// <return>类型字符串</return>
        /// </summary>
        string TypeToCustomString(System.Type type);

        /// <summary>
        /// 根据配置表将类型转换为字符串，如果转换失败则使用c#默认的类型标签
        /// <param name="typeString">数据类型字符串</param>
        /// <return>类型字符串</return>
        /// </summary>
        string TypeToCustomString(string typeString);
    }
}