using System.Diagnostics;

namespace B2Framework
{
    public static partial class Log
    {
        public const string SYMBOL = "DEBUG_LOG";
        private static ILogHelper m_LogHelper = null;
        private static ILogHelper logHelper
        {
            get
            {
                if (m_LogHelper == null) m_LogHelper = new DefaultLogHelper();
                return m_LogHelper;
            }
        }
        public static void SetHelper(ILogHelper logHelper)
        {
            m_LogHelper = logHelper;
        }
        [Conditional(SYMBOL)]
        public static void Debug(object message)
        {
            logHelper.Log(LogLevel.Debug, message);
        }
        [Conditional(SYMBOL)]
        public static void Debug(string format, params object[] args)
        {
            logHelper.Log(LogLevel.Debug, string.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Info(object message)
        {
            logHelper.Log(LogLevel.Info, message);
        }
        [Conditional(SYMBOL)]
        public static void Info(string format, params object[] args)
        {
            logHelper.Log(LogLevel.Info, string.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Warning(object message)
        {
            logHelper.Log(LogLevel.Warning, message);
        }
        [Conditional(SYMBOL)]
        public static void Warning(string format, params object[] args)
        {
            logHelper.Log(LogLevel.Warning, string.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Error(object message)
        {
            logHelper.Log(LogLevel.Error, message);
        }
        [Conditional(SYMBOL)]
        public static void Error(string format, params object[] args)
        {
            logHelper.Log(LogLevel.Error, string.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Fatal(object message)
        {
            logHelper.Log(LogLevel.Fatal, message);
        }
        [Conditional(SYMBOL)]
        public static void Fatal(string format, params object[] args)
        {
            logHelper.Log(LogLevel.Fatal, string.Format(format, args));
        }
    }
}