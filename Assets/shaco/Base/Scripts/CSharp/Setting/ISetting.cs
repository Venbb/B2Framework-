using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface ISetting
    {
        /// <summary>
        /// 配置数据
        /// </summary>
        shaco.LitJson.JsonData settingValue { get; set; }

        /// <summary>
        /// 保存数据为字符串
        /// <return>格式字符串，建议为json格式</return>
        /// </summary>
        void SaveTo(shaco.LitJson.JsonData data);

        /// <summary>
        /// 读取数据
        /// </summary>
        void LoadFrom(shaco.LitJson.JsonData data);
    }
}	