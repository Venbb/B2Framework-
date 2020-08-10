using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 引导步骤数据读写类
    /// </summary>
    public class GuideSettingHelper : IGuideSettingHelper
    {
        /// <summary>
        /// 允许序列化成员变量的属性类型
        /// </summary>
        public System.Type serializeAttributeType
        {
            get
            {
                if (null == _serializeAttributeType && GameHelper.gameConfig.ContainsKey("GuideSettingHelper.serializeAttributeType"))
                {
                    var attributeTypeName = GameHelper.gameConfig.ReadString("GuideSettingHelper.serializeAttributeType");
                    _serializeAttributeType = shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(attributeTypeName);
                }
                return _serializeAttributeType;
            }
            set { _serializeAttributeType = value; }
        }

        private System.Type _serializeAttributeType = null;

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveTo(shaco.Base.IGuideStep step, shaco.LitJson.JsonData data)
        {
            if (null == data)
                return;

            data["guideStepID"] = step.guideStepID;
            data["firstStepID"] = step.firstStepID;
            data["isOpen"] = step.isOpen;
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public void LoadFrom(shaco.Base.IGuideStep step, shaco.LitJson.JsonData data)
        {
            if (null == data)
                return;

            step.guideStepID = data["guideStepID"].ToString();
            step.firstStepID = data["firstStepID"].ToString();
            step.isOpen = data["isOpen"].ToString().ToBool();
        }
    }
}