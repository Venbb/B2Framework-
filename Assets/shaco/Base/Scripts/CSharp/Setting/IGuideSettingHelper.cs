using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	/// <summary>
	/// 引导步骤数据读写接口
	/// </summary>
    public interface IGuideSettingHelper : IGameInstance
    {
        /// <summary>
        /// 允许序列化成员变量的属性类型
        /// </summary>
        System.Type serializeAttributeType { get; set; }

        /// <summary>
        /// 保存数据
        /// </summary>
        void SaveTo(shaco.Base.IGuideStep step, shaco.LitJson.JsonData data);

        /// <summary>
        /// 读取数据
        /// </summary>
        void LoadFrom(shaco.Base.IGuideStep step, shaco.LitJson.JsonData data);
    }
}