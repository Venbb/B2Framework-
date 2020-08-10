using System;
using System.Text;

namespace B2Framework
{
    public static partial class Utility
    {
        public static class Text
        {
            [ThreadStatic]
            private static StringBuilder s_CachedStringBuilder = null;
            public static string Format(string format, params object[] args)
            {
                if (string.IsNullOrEmpty(format)) return format;

                if (args == null) return format;

                if (s_CachedStringBuilder == null) s_CachedStringBuilder = new StringBuilder(1024);
                s_CachedStringBuilder.Length = 0;
                s_CachedStringBuilder.AppendFormat(format, args);
                return s_CachedStringBuilder.ToString();
            }
        }
    }
}
