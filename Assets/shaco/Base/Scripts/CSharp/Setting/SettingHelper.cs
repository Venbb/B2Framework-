using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	/// <summary>
	/// 引导配置存取类
	/// </summary>
    public class SettingsHelper : ISettingHelper
    {
        /// <summary>
        /// 转换为json数据节点
        /// <param name="setting">配置</param>
        /// <return>json数据节点</return>
        /// </summary>
        public string ToDataString(shaco.Base.ISetting setting, shaco.LitJson.JsonData valueData)
        {
            if (null == setting)
                return string.Empty;

            shaco.LitJson.JsonData retValue = new shaco.LitJson.JsonData();
            setting.SaveTo(valueData);
            retValue["type"] = setting.ToTypeString();
            retValue["value"] = valueData.ToJson();
            return retValue.ToJson();
        }

		/// <summary>
		/// 将json节点数据转换为引导配置
		/// </summary>
        public T FromDataString<T>(string data) where T : shaco.Base.ISetting
        {
            T retValue = default(T);

            //忽略空的数据
            if (string.IsNullOrEmpty(data))
                return retValue;

            shaco.LitJson.JsonData jsonData = shaco.LitJson.JsonMapper.ToObject(data);

            if (null == jsonData)
                return retValue;

            if (!jsonData.ContainsKey("type"))
            {
                Log.Error("GuideStepDefault FromJsonData error: not found 'type' node, json=" + data);
                return retValue;
            }

            if (!jsonData.ContainsKey("value"))
            {
                Log.Error("GuideStepDefault FromJsonData error: not found 'value' node, json=" + data);
                return retValue;
            }

            var fullTypeName = jsonData["type"].ToString();

            retValue = (T)shaco.Base.Utility.Assembly.GetTypeWithinLoadedAssemblies(fullTypeName).Instantiate();
            if (null != retValue)
            {
                var valueTmp = jsonData["value"];
                if (null != valueTmp)
                    retValue.settingValue = shaco.LitJson.JsonMapper.ToObject(valueTmp.ToString());
            }

            if (null == retValue)
            {
                Log.Error("GuideStepDefault FromJsonData error: instantiate error, type=" + fullTypeName + " json=" + data);
                return retValue;
            }
            return retValue;
        }
    }
}