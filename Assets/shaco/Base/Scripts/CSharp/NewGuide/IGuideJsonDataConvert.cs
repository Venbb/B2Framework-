using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace shaco.Base
{
    public interface IGuideJsonDataConvert
    {
        /// <summary>
        /// 字符串转引导数据
        /// </summary>
        void LoadFromString(string json);
    }
}	