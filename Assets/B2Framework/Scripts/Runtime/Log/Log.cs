using System.Diagnostics;

namespace B2Framework
{
    public static partial class Log
    {
        public const string SYMBOL = "DEBUG_LOG";
        private static ILogHelper m_LogHelper = null;
        public static void SetHelper(ILogHelper logHelper)
        {
            m_LogHelper = logHelper;
        }
        [Conditional(SYMBOL)]
        public static void Debug(object message)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Debug, message);
        }
        [Conditional(SYMBOL)]
        public static void Debug(string format, params object[] args)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Debug, Utility.Text.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Info(object message)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Info, message);
        }
        [Conditional(SYMBOL)]
        public static void Info(string format, params object[] args)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Info, Utility.Text.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Warning(object message)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Warning, message);
        }
        [Conditional(SYMBOL)]
        public static void Warning(string format, params object[] args)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Warning, Utility.Text.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Error(object message)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Error, message);
        }
        [Conditional(SYMBOL)]
        public static void Error(string format, params object[] args)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Error, Utility.Text.Format(format, args));
        }
        [Conditional(SYMBOL)]
        public static void Fatal(object message)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Fatal, message);
        }
        [Conditional(SYMBOL)]
        public static void Fatal(string format, params object[] args)
        {
            if (m_LogHelper == null) return;
            m_LogHelper.Log(LogLevel.Fatal, Utility.Text.Format(format, args));
        }
    }
}