using System;
using System.Text;

namespace B2Framework
{
    public static partial class Utility
    {
        public static class Text
        {
            [ThreadStatic]
            private static StringBuilder m_Cached = null;
            public static string Format(string format, params object[] args)
            {
                if (string.IsNullOrEmpty(format)) return format;
                if (args != null && args.Length > 0)
                {
                    if (m_Cached == null) m_Cached = new StringBuilder(1024);
                    m_Cached.Length = 0;
                    m_Cached.AppendFormat(format, args);
                    return m_Cached.ToString();
                }
                else return format;
            }
        }
    }
}
