using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 本地化语言类
    /// </summary>
    public abstract class ILocalization : IGameInstance
    {
        /// <summary>
        /// 特殊的空文本key，该key的value会转换为string.Empy，且没有警告
        /// </summary>
        abstract public string IGNORE_EMPTY_KEY { get; }

        /// <summary>
        /// 从json文件路径中加载
        /// <param name="path">json文件路径</param>
        /// <return>是否加载成功</return>
        /// </summary>
        abstract public bool LoadWithJsonString(string json);

        /// <summary>
        /// 从字符串中添加
        /// <param name="key">键值</param>
        /// <param name="value">文本内容</param>
        /// <return>是否加载成功</return>
        /// </summary>
        abstract public bool AddWithString(string key, string value);

        /// <summary>
        /// 获取本地化文本
        /// <param name="key">语言表中键值</param>
        /// <param name="defaultText">当查找失败显示的默认文本，如果为空则使用key作为默认文本</param>
        /// <return>本地化文本</return>
        /// </summary>
        abstract public string GetText(string key, string defaultText = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 获取本地化格式化文本
        /// <param name="key">语言表中键值</param>
        /// <param name="param">格式化附加参数，建议为字符串</param>
        /// <return>本地化文本</return>
        /// </summary>
        abstract public string GetTextFormat(string key, params object[] param);

        /// <summary>
        /// 获取本地化格式化文本
        /// <param name="key">语言表中键值</param>
        /// <param name="defaultText">当查找失败显示的默认文本，如果为空则使用key作为默认文本</param>
        /// <param name="param">格式化附加参数，建议为字符串</param>
        /// <return>本地化文本</return>
        /// </summary>
        abstract public string GetTextFormatWithDefaultValue(string key, string defaultText = shaco.Base.GlobalParams.EmptyString, params object[] param);

        /// <summary>
        /// 清空本地化配置
        /// </summary>
        abstract public void Clear();

        /// <summary>
        /// 获取本地化语言配置字段数量
        /// </summary>
        abstract public int GetLocalizationCount();
    }
}

