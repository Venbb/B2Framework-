using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public abstract class GuideStepDefault : shaco.Base.IGuideStep
    {
        /// <summary>
        /// 步骤id
        /// </summary>
        public string guideStepID { get; set; }

        /// <summary>
        /// 第一步引导步骤
        /// </summary>
        public string firstStepID { get; set; }

        /// <summary>
        /// 配置数据
        /// </summary>
        public shaco.LitJson.JsonData settingValue { get; set; }

        /// <summary>
        /// 引导是否开放
        /// </summary>
        public bool isOpen { get; set; }

        /// <summary>
        /// 是否已经引导结束
        /// </summary>
        public bool isEnd { get; set; }

        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        abstract public void Execute();

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        abstract public void End();

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveTo(shaco.LitJson.JsonData data)
        {
            var fileds = this.GetType().GetFields(shaco.Base.GuideManagerDefine.BINDING_FLAGS_GET_FILED);
            var guidesettings = GameHelper.guideSettingHelper;
            
            foreach (var iter in fileds)
            {
                //过滤私有变量(标记了特殊属性除外)
                if (iter.IsPrivate && !iter.IsDefined(guidesettings.serializeAttributeType, false))
                    continue;

                //过滤默认引导步骤，因为它会在引导管理器中进行数据管理
                if (iter.DeclaringType == typeof(shaco.Base.GuideStepDefault))
                    continue;

                //过滤空字段
                var getValue = iter.GetValue(this);
                if (null == getValue)
                    continue;

                var jsonData = new shaco.LitJson.JsonData(getValue.ToString());
                data[iter.Name] = jsonData;
            }

            var properties = this.GetType().GetProperties(shaco.Base.GuideManagerDefine.BINDING_FLAGS_GET_PROPERTY);
            foreach (var iter in properties)
            {
                //过滤私有变量(标记了特殊属性除外)
                if (!iter.PropertyType.IsPublic && !iter.PropertyType.IsDefined(guidesettings.serializeAttributeType, false))
                    continue;

                //过滤默认引导步骤，因为它会在引导管理器中进行数据管理
                if (iter.DeclaringType == typeof(shaco.Base.GuideStepDefault))
                    continue;

                //过滤空字段
                var getValue = iter.GetValue(this);
                if (null == getValue)
                    continue;

                var jsonData = new shaco.LitJson.JsonData(getValue.ToString());
                data[iter.Name] = jsonData;
            }
        }

        /// <summary>
        /// 读取数据
        /// </summary>
        public void LoadFrom(shaco.LitJson.JsonData data)
        {
            var fileds = this.GetType().GetFields(shaco.Base.GuideManagerDefine.BINDING_FLAGS_GET_FILED);
            var guidesettings = GameHelper.guideSettingHelper;

            foreach (var iter in fileds)
            {
                //过滤私有变量(标记了特殊属性除外)
                if (iter.IsPrivate && !iter.IsDefined(guidesettings.serializeAttributeType, false))
                    continue;

                //过滤默认引导步骤，因为它会在引导管理器中进行数据管理
                if (iter.DeclaringType == typeof(shaco.Base.GuideStepDefault))
                    continue;

                //过滤不存在的key，因为它可能是空字段所有没有写入到json结构中
                if (!data.ContainsKey(iter.Name))
                    continue;

                LoadGuideDataFromString(iter.GetValue(this), iter.FieldType, data[iter.Name].ToString(), (newValue) =>
                {
                    iter.SetValue(this, newValue);
                });
            }

            var properties = this.GetType().GetProperties(shaco.Base.GuideManagerDefine.BINDING_FLAGS_GET_PROPERTY);
            foreach (var iter in properties)
            {
                //过滤私有变量(标记了特殊属性除外)
                if (!iter.PropertyType.IsPublic && !iter.PropertyType.IsDefined(guidesettings.serializeAttributeType, false))
                    continue;

                //过滤默认引导步骤，因为它会在引导管理器中进行数据管理
                if (iter.DeclaringType == typeof(shaco.Base.GuideStepDefault))
                    continue;

                //过滤不存在的key，因为它可能是空字段所有没有写入到json结构中
                if (!data.ContainsKey(iter.Name))
                    continue;

                //过滤不能写入的属性
                if (!iter.CanWrite)
                    continue;

                LoadGuideDataFromString(iter.GetValue(this), iter.PropertyType, data[iter.Name].ToString(), (newValue) =>
                {
                    iter.SetValue(this, newValue);
                });
            }
        }

        /// <summary>
        /// 通知引导步骤完毕
        /// <param name="forceGotoStepID">强制继续跳转到的引导步骤</param>
        /// </summary>
        protected void OnGuideStepCompleted(string forceGotoStepID = null)
        {
            GameHelper.newguide.OnGuideStepCompleted(this, forceGotoStepID);
        }

        private void LoadGuideDataFromString(object value, System.Type type, string str, System.Action<object> callbackSetValue)
        {
            if (type.IsInherited<shaco.Base.IGuideJsonDataConvert>())
            {
                if (null == value)
                    value = type.Instantiate();
                if (null == value)
                {
                    Log.Error("GuideStepDefault LoadGuideDataFromString error: can't instantiate type=" + type);
                    return;
                }

                var convertTmp = value as shaco.Base.IGuideJsonDataConvert;
                convertTmp.LoadFromString(str);
                callbackSetValue(convertTmp);
            }
            else
            {
                //仅处理基本数据类型，复合数据类型需要都继承 IGuideJsonDataConvert 接口
                var newValue = shaco.Base.AutoValue.BaseTypeParse(str, type);
                if (null == newValue)
                {
                    Log.Error(string.Format("ID'{0}' type'{1}' not a base c# type, methods of interface '{2}' should be inherited and implemented",this.guideStepID, type, typeof(shaco.Base.IGuideJsonDataConvert).FullName));
                }
                callbackSetValue(newValue);
            }
        }
    }
}