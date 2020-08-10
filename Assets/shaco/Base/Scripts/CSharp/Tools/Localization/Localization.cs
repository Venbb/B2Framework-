using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class Localization : ILocalization
    {
        override public string IGNORE_EMPTY_KEY { get { return "//empty"; } }

        private Dictionary<string, string> _localizationDictionary = new Dictionary<string, string>();

        private const string NULL_KEY = "[null_key]";
        private const string EMPTY_KEY = "[empty_key]";
        private const string MISSING_VALUE = "[missing:{0}]";

        /// <summary>
        /// 从json文件路径中加载
        /// <param name="path">json文件路径</param>
        /// <return>是否加载成功</return>
        /// </summary>
        override public bool LoadWithJsonString(string json)
        {
            var jsonObjects = shaco.LitJson.JsonMapper.ToObject(json);
            if (0 == jsonObjects.Count)
            {
                Log.Error("Localization LoadWithJson error: no json data!");
                return false;
            }

            for (int i = jsonObjects.Count - 1; i >= 0; --i)
            {
                var jsonObjectTmp = jsonObjects[i];

                var key = jsonObjectTmp.ToProperty();
                var value = jsonObjectTmp.ToString();

                if (this._localizationDictionary.ContainsKey(key))
                {
                    Log.Error("Localization LoadWithJson error: has same key=" + key);
                }
                else
                {
                    this._localizationDictionary.Add(key, value);
                }
            }
            return true;
        }

        /// <summary>
        /// 从字符串中添加
        /// <param name="key">键值</param>
        /// <param name="value">文本内容</param>
        /// <return>是否加载成功</return>
        /// </summary>
        override public bool AddWithString(string key, string value)
        {
            if (this._localizationDictionary.ContainsKey(key))
            {
                Log.Error("Localization AddWithString error: has same key=" + key);
                return false;
            }
            else
            {
                this._localizationDictionary.Add(key, value);
                return true;
            }
        }

        /// <summary>
        /// 获取本地化文本
        /// <param name="key">语言表中键值</param>
        /// <param name="defaultText">当查找失败显示的默认文本，如果为空则使用key作为默认文本</param>
        /// <return>本地化文本</return>
        /// </summary>
        override public string GetText(string key, string defaultText = shaco.Base.GlobalParams.EmptyString)
        {
            if (null == key)
                return NULL_KEY;
            else if (key == string.Empty)
            {
                return EMPTY_KEY;
            }

            var retValue = string.Empty;
            if (!this._localizationDictionary.ContainsKey(key))
            {
                retValue = string.IsNullOrEmpty(defaultText) ? key : defaultText;
            }
            else
            {
                retValue = this._localizationDictionary[key];
            }

            if (string.IsNullOrEmpty(retValue))
                retValue = string.Format(MISSING_VALUE, key);

            if (IGNORE_EMPTY_KEY == retValue)
            {
                //Escape to empty character
                retValue = string.Empty;
            }
            return retValue;
        }

        /// <summary>
        /// 获取本地化格式化文本
        /// <param name="key">语言表中键值</param>
        /// <param name="param">格式化附加参数，建议为字符串</param>
        /// <return>本地化文本</return>
        /// </summary>
        override public string GetTextFormat(string key, params object[] param)
        {
            return GetTextFormatWithDefaultValue(key, string.Empty, param);
        }

        /// <summary>
        /// 获取本地化格式化文本
        /// <param name="key">语言表中键值</param>
        /// <param name="defaultText">当查找失败显示的默认文本，如果为空则使用key作为默认文本</param>
        /// <param name="param">格式化附加参数，建议为字符串</param>
        /// <return>本地化文本</return>
        /// </summary>
        override public string GetTextFormatWithDefaultValue(string key, string defaultText = shaco.Base.GlobalParams.EmptyString, params object[] param)
        {
            if (null == key)
                return NULL_KEY;
            else if (key == string.Empty)
                return EMPTY_KEY;

            var retValue = string.Empty;
            if (!this._localizationDictionary.ContainsKey(key))
            {
                retValue = string.IsNullOrEmpty(defaultText) ? key : defaultText;
            }
            else
            {
                var textTmp = this._localizationDictionary[key];
                if (null != param && param.Length > 0)
                {
                    try
                    {
                        retValue = string.Format(textTmp, param);
                    }
                    catch (System.Exception)
                    {
                        retValue = textTmp;
                    }
                }
                else
                    retValue = textTmp;
            }

            if (string.IsNullOrEmpty(retValue))
                retValue = string.Format(MISSING_VALUE, key);

            if (IGNORE_EMPTY_KEY == retValue)
            {
                //Escape to empty character
                retValue = string.Empty;
            }
            return retValue;
        }

        /// <summary>
        /// 清空本地化配置
        /// </summary>
        override public void Clear()
        {
            this._localizationDictionary.Clear();
        }

        /// <summary>
        /// 获取本地化语言配置字段数量
        /// </summary>
        override public int GetLocalizationCount()
        {
            return this._localizationDictionary.Count;
        }
    }
}

