
using System;

namespace B2Framework
{
    public static class StringEx
    {
        public static bool IsInt(this string str)
        {
            int result;
            return int.TryParse(str, out result);
        }
        public static int ToInt(this string str, int def = 0)
        {
            int result;
            if (int.TryParse(str, out result))
                return result;
            return def;
        }
        public static uint ToUInt(this string str, uint def = 0)
        {
            uint result;
            if (uint.TryParse(str, out result))
                return result;
            return def;
        }
        public static long ToInt64(this string str)
        {
            long result;
            if (long.TryParse(str, out result))
                return result;
            return 0;
        }
        public static string ToPath(this string str, bool forward = true)
        {
            return Utility.Path.GetRegularPath(str, forward);
        }
        public static bool IsDateTime(this string str)
        {
            DateTime result;
            return DateTime.TryParse(str, out result);
        }
        public static DateTime ToDateTime(this string str)
        {
            DateTime result;
            if (!DateTime.TryParse(str, out result))
                return DateTime.Now;
            return result;
        }
        public static string ToDateTime(this string str, string format)
        {
            DateTime result;
            if (!DateTime.TryParse(str, out result))
                return str;
            return result.ToString(format);
        }
    }
}