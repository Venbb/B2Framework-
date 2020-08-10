using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 持久化数据存读类
    /// *注意* 为了缓解同一帧反复调用写入方法导致IO过高问题
    ///       写入数据的当前帧不会立即保存到文件中，如需立即保存则调用CheckSaveModifyData方法
    /// </summary>
    public abstract class IDataSave : IGameInstance
    {
        /// <summary>
        /// 数据分隔符
        /// </summary>
        abstract public string SPLIT_FLAG { get; }

        /// <summary>
        /// 文件存读路径
        /// </summary>
        abstract public string savePath { get; set; }

        /// <summary>
		/// 写入数据
		/// <param name="key">数据键值</param>
		/// <param name="value">数据</param>
		/// </summary>
        abstract public void WriteChar(string key, char value);
        abstract public void WriteBool(string key, bool value);
        abstract public void WriteShort(string key, short value);
        abstract public void WriteInt(string key, int value);
        abstract public void WriteFloat(string key, float value);
        abstract public void WriteDouble(string key, double value);
        abstract public void WriteLong(string key, long value);
        abstract public void WriteString(string key, string value);
        abstract public void WriteEnum<T>(string key, T value) where T : System.Enum;
        abstract public void WriteArray<T>(string key, T[] args);
        abstract public void WriteList<T>(string key, List<T> args);
        abstract public void WriteList(string key, List<object> args);
        abstract public void WriteArrayCustom<T>(string key, T[] args, System.Func<T, string> convert);
        abstract public void WriteListCustom<T>(string key, List<T> args, System.Func<T, string> convert);
        abstract public void WriteDictionary<KEY, VALUE>(string key, Dictionary<KEY, VALUE> args);

        /// <summary>
		/// 读取数据
		/// <param name="key">数据键值</param>
		/// <param name="defaultValue">当读取失败后返回的默认值</param>
		/// <return>数据</return>
		/// </summary>
        abstract public char ReadChar(string key, char defaultValue = ' ');
        abstract public bool ReadBool(string key, bool defaultValue = false);
        abstract public int ReadInt(string key, int defaultValue = 0);
        abstract public float ReadFloat(string key, float defaultValue = 0);
        abstract public double ReadDouble(string key, double defaultValue = 0);
        abstract public long ReadLong(string key, long defaultValue = 0);
        abstract public T ReadEnum<T>(string key, T defaultValue = default(T));
        abstract public string[] ReadArrayString(string key, string[] defaultValue = null);
        abstract public List<string> ReadListString(string key, List<string> defaultValue = null);
        abstract public string ReadString(string key, string defaultValue = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 读取数据数组
        /// <param name="key">数据键值</param>
        /// <param name="convert">数据转换方法，通常读取的原始数据统一为string，需要手动转换为自定义类型</param>
        /// <param name="defaultValue">当读取失败后返回的默认值</param>
        /// <return>数据</return>
        /// </summary>
        abstract public T[] ReadArray<T>(string key, System.Func<string, T> convert, T[] defaultValue = null);
        abstract public List<T> ReadList<T>(string key, System.Func<string, T> convert, List<T> defaultValue = null);
        abstract public List<object> ReadList(string key, System.Func<string, object> convert, List<object> defaultValue = null);

        /// <summary>
        /// 读取数据字典
        /// <param name="key">数据键值</param>
        /// <param name="convertKey">键值转换方法，通常读取的原始数据统一为string，需要手动转换为自定义类型</param>
        /// <param name="convertValue">数据转换方法，通常读取的原始数据统一为string，需要手动转换为自定义类型</param>
        /// <param name="defaultValue">当读取失败后返回的默认值</param>
        /// <return>数据</return>
        /// </summary>
        abstract public Dictionary<KEY, VALUE> ReadDictionary<KEY, VALUE>(string key, System.Func<string, KEY> convertKey, System.Func<string, VALUE> convertValue, Dictionary<KEY, VALUE> defaultValue = null);

        /// <summary>
        /// 读取用户自定义类型的值
        /// <param name="key">数据键值</param>
        /// <param name="callbackConvert">数据转换方法，通常读取的原始数据统一为string，需要手动转换为自定义类型</param>
        /// <param name="defaultValue">当读取失败后返回的默认值</param>
        /// <return>数据</return>
        /// </summary>
        abstract public T ReadUserType<T>(string key, System.Func<string[], T> callbackConvert, T defaultValue);

        /// <summary>
        /// 删除数据
        /// <param name="key">数据键值</param>
        /// </summary>
        abstract public void Remove(string key);

        /// <summary>
        /// 删除以指定键值开头的所有数据，主要用于批量删除类似数据使用
		/// 例如keyPrefix为test，则会删除test_1,test_2,test_3...类似键值
        /// <param name="keyPrefix">数据键值开头</param>
        /// </summary>
        abstract public void RemoveStartWith(string keyPrefix);

        /// <summary>
        /// 查看数据是否存在
        /// <param name="key">数据键值</param>
        /// <return>true存在，false不存在</return>
        /// </summary>
        abstract public bool ContainsKey(string key);

        /// <summary>
		/// 清空所有数据
		/// </summary>
        abstract public void Clear();

        /// <summary>
        /// 获取所有数据的格式化字符串，主要用于打印和测试数据内容
        /// <return>格式数据字符串</return>
        /// </summary>
        abstract public string GetFormatString();

        /// <summary>
        /// 保存数据到指定路径中
        /// <param name="path">保存文件路径</param>
        /// </summary>
        abstract public void SaveAsFile(string path);

        /// <summary>
        /// 重新从文件中读取数据配置
        /// <param name="path">文件绝对路径</param>
        /// <param name="autoEncrypt">是否自动加密</param>
        /// </summary>
        abstract public void ReloadFromFile(string path = null, bool autoEncrypt = true);

        /// <summary>
        /// 重新从二进制数据中读取数据配置
        /// <param name="bytes">二进制数据</param>
        /// <param name="autoEncrypt">是否自动加密</param>
        /// </summary>
        abstract public void ReloadFromBytes(byte[] bytes, bool autoEncrypt = true);

        /// <summary>
        /// 顺序写入参数
        /// <param name="key">数据键值</param>
        /// <param name="args">数据参数</param>
        /// </summary>
        abstract public void WriteArguments(string key, params object[] args);

        /// <summary>
        /// 检查数据保存
        /// 默认逻辑为在数据发生变化后的下一帧中保存，如果想当前帧立即保存数据，则调用该方法
        /// </summary>
        abstract public void CheckSaveModifyData();
    }
}