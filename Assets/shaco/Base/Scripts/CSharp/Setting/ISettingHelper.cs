using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public interface ISettingHelper : IGameInstance
    {
        /// <summary>
        /// 转换为json数据
        /// <param name="guideSetting">引导配置</param>
        /// <return>数据</return>
        /// </summary>
        string ToDataString(shaco.Base.ISetting setting, shaco.LitJson.JsonData valueData);

        /// <summary>
        /// 将json节点数据转换为引导配置
        /// </summary>
        T FromDataString<T>(string data) where T : shaco.Base.ISetting;
    }
}
