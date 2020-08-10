using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class DataTypesTemplate : IDataTypesTemplate
    {
        /// <summary>
        /// 获取unity默认的类型模板
        /// </summary>
        public shaco.Base.StringTypePair[] GetTypesTemplate()
        {
            return shaco.Base.StringTypePair.DEFAULT_TYPES_PAIRS;
        }

		/// <summary>
        /// 根据配置表将类型转换为字符串，如果转换失败则使用c#默认的类型标签
        /// <param name="type">数据类型</param>
        /// <return>类型字符串</return>
        /// </summary>
        public string TypeToCustomString(System.Type type)
        {
            return TypeToCustomString(type.FullName);
        }

        /// <summary>
        /// 根据配置表将类型转换为字符串，如果转换失败则使用c#默认的类型标签
        /// <param name="typeString">数据类型字符串</param>
        /// <return>类型字符串</return>
        /// </summary>
        public string TypeToCustomString(string typeString)
        {
            var pariTmp = shaco.Base.StringTypePair.DEFAULT_TYPES_PAIRS;
            shaco.Base.StringTypePair findValue = null;
            if (!pariTmp.IsNullOrEmpty())
            {
                for (int i = pariTmp.Length - 1; i >= 0; --i)
                {
                    if (typeString == pariTmp[i].fullTypeName)
                    {
                        findValue = pariTmp[i];
                        break;
                    }
                }
            }
            return null != findValue ? findValue.customTypeString : typeString;
        }
    }
}