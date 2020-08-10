using System.Collections;

namespace shaco.Base
{
    [System.Serializable]
    public class AutoValue
    {
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        protected System.Type _requestType = typeof(string);
#if UNITY_5_3_OR_NEWER
        [UnityEngine.SerializeField]
#endif
        protected object _value = false;

        public AutoValue() { }
        public AutoValue(object Value)
        {
            Set(Value);
        }

        /// <summary>
        /// 字符串转基本数据类型
        /// </summary>
        static public object BaseTypeParse(string str, System.Type type)
        {
            object retValue = null;
            switch (type.FullName)
            {
                case TYPE_NAME_BYTE: retValue = System.Byte.Parse(str); break;
                case TYPE_NAME_SBYTE: retValue = System.SByte.Parse(str); break;
                case TYPE_NAME_INT: retValue = System.Int32.Parse(str); break;
                case TYPE_NAME_UINT: retValue = System.UInt32.Parse(str); break;
                case TYPE_NAME_SHORT: retValue = System.Int16.Parse(str); break;
                case TYPE_NAME_USHORT: retValue = System.UInt16.Parse(str); break;
                case TYPE_NAME_LONG: retValue = System.Int64.Parse(str); break;
                case TYPE_NAME_ULONG: retValue = System.UInt64.Parse(str); break;
                case TYPE_NAME_FLOAT: retValue = System.Single.Parse(str); break;
                case TYPE_NAME_DOUBLE: retValue = System.Double.Parse(str); break;
                case TYPE_NAME_DECIMAL: retValue = System.Decimal.Parse(str); break;
                case TYPE_NAME_CHAR: retValue = System.Char.Parse(str); break;
                case TYPE_NAME_BOOL: retValue = System.Boolean.Parse(str); break;
                case TYPE_NAME_STRING: retValue = str; break;
                default: Log.Error("AutoValue BaseTypeFromString erorr: unsupport base type=" + type.FullName + " str=" + str); retValue = null; return false;
            }
            return retValue;
        }

        public const string TYPE_NAME_BYTE = "System.Byte";
        public const string TYPE_NAME_SBYTE = "System.SByte";
        public const string TYPE_NAME_INT = "System.Int32";
        public const string TYPE_NAME_UINT = "System.UInt32";
        public const string TYPE_NAME_SHORT = "System.Int16";
        public const string TYPE_NAME_USHORT = "System.UInt16";
        public const string TYPE_NAME_LONG = "System.Int64";
        public const string TYPE_NAME_ULONG = "System.UInt64";
        public const string TYPE_NAME_FLOAT = "System.Single";
        public const string TYPE_NAME_DOUBLE = "System.Double";
        public const string TYPE_NAME_DECIMAL = "System.Decimal";
        public const string TYPE_NAME_CHAR = "System.Char";
        public const string TYPE_NAME_BOOL = "System.Boolean";
        public const string TYPE_NAME_STRING = "System.String";

        public static implicit operator bool(AutoValue value) { return (bool)value._value; }
        public static implicit operator char(AutoValue value) { return (char)value._value; }
        public static implicit operator short(AutoValue value) { return (short)value._value; }
        public static implicit operator int(AutoValue value) { return (int)value._value; }
        public static implicit operator long(AutoValue value) { return (long)value._value; }
        public static implicit operator float(AutoValue value) { return (float)value._value; }
        public static implicit operator double(AutoValue value) { return (double)value._value; }
        public static implicit operator string(AutoValue value) { return (string)value._value; }

        public object value
        {
            get { return Get(); }
            set { Set(value); }
        }

        public bool IsType(System.Type type)
        {
            return _requestType == type;
        }

        virtual public object Get()
        {
            return _value;
        }

        virtual public bool Set(object setValue)
        {
            if (setValue == null)
                return false;

            System.Type typeTmp = setValue.GetType();
            _requestType = typeTmp;

            switch (typeTmp.FullName)
			{
				case TYPE_NAME_BYTE: _value = (byte)setValue; break;
                case TYPE_NAME_SBYTE: _value = (sbyte)setValue; break;
                case TYPE_NAME_INT: _value = (int)setValue; break;
                case TYPE_NAME_UINT: _value = (uint)setValue; break;
                case TYPE_NAME_SHORT: _value = (short)setValue; break;
                case TYPE_NAME_USHORT: _value = (ushort)setValue; break;
                case TYPE_NAME_LONG: _value = (long)setValue; break;
                case TYPE_NAME_ULONG: _value = (ulong)setValue; break;
                case TYPE_NAME_FLOAT: _value = (float)setValue; break;
                case TYPE_NAME_DOUBLE: _value = (double)setValue; break;
                case TYPE_NAME_DECIMAL: _value = (decimal)setValue; break;
                case TYPE_NAME_CHAR: _value = (char)setValue; break;
                case TYPE_NAME_BOOL: _value = (bool)setValue; break;
                case TYPE_NAME_STRING: _value = (string)setValue; break;
				default: Log.Error("AutoValue Set erorr: unsupport base type=" + typeTmp.FullName); _value = null; return false;
			}

			return true;
        }
    }
}