using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 字符串类型与实际数据类型配对数据
    /// </summary>
    [System.Serializable]
    public class StringTypePair
    {
        static public readonly shaco.Base.StringTypePair[] DEFAULT_TYPES_PAIRS = new shaco.Base.StringTypePair[]
        {
            new shaco.Base.StringTypePair("byte", typeof(byte).FullName, ".ToByte(0, false)"),
            new shaco.Base.StringTypePair("sbyte", typeof(sbyte).FullName, ".ToSByte(0, false)"),
            new shaco.Base.StringTypePair("int", typeof(int).FullName, ".ToInt(0, false)"),
            new shaco.Base.StringTypePair("uint", typeof(uint).FullName, ".ToUInt(0, false)"),
            new shaco.Base.StringTypePair("short", typeof(short).FullName, ".ToShort(0, false)"),
            new shaco.Base.StringTypePair("ushort", typeof(ushort).FullName, ".ToUShort(0, false)"),
            new shaco.Base.StringTypePair("long", typeof(long).FullName, ".ToLong(0, false)"),
            new shaco.Base.StringTypePair("ulong", typeof(ulong).FullName, ".ToULong(0, false)"),

            new shaco.Base.StringTypePair("float", typeof(float).FullName, ".ToFloat(0, false)"),
            new shaco.Base.StringTypePair("double", typeof(double).FullName, ".ToDouble(0, false)"),
            new shaco.Base.StringTypePair("decimal", typeof(decimal).FullName, ".ToDecimal(0, false)"),

            new shaco.Base.StringTypePair("char", typeof(char).FullName, ".ToChar(' ', false)"),
            new shaco.Base.StringTypePair("bool", typeof(bool).FullName, ".ToBool(false, false)"),
            new shaco.Base.StringTypePair("string", typeof(string).FullName, string.Empty),

            new shaco.Base.StringTypePair("date", typeof(System.DateTime).FullName, ".ToDateTime(new System.DateTime(), false)"),
            new shaco.Base.StringTypePair("time", typeof(System.DateTime).FullName, ".ToDateTime(new System.DateTime(), false)"),
        };

        //类型标记查找字符串
        public string customTypeString;
        //c#数据类型全称，可以通过typeof(xxx).FullName查看
        public string fullTypeName;
        //字符串转指定数据类型的脚本，参考在shaco_ExtensionsString的扩展方法
        public string convertFunction;

        public StringTypePair(string customTypeString, string fullTypeName, string convertFunction)
        {
            this.customTypeString = customTypeString;
            this.fullTypeName = fullTypeName;
            this.convertFunction = convertFunction;
        }

        public StringTypePair() { }
    }
}