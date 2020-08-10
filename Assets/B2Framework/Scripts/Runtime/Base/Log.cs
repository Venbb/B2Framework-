using System.Diagnostics;
using UDebug = UnityEngine.Debug;

namespace B2Framework
{
    public static class Debug
    {
        private static string Format(object message)
        {
            return Utility.Text.Format("{0:yyyy-MM-dd HH:mm:ss.fff} {1}", System.DateTime.Now, message != null ? message : "Null");
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void Log(object message)
        {
            UDebug.Log(Format(message));
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void LogFormat(string format, params object[] args)
        {
            UDebug.LogFormat(Format(format), args);
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void LogWarning(object message)
        {
            UDebug.LogWarning(Format(message));
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void LogWarningFormat(string format, params object[] args)
        {
            UDebug.LogWarningFormat(Format(format), args);
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void LogError(object message)
        {
            UDebug.LogError(Format(message));
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void LogErrorFormat(object format, params object[] args)
        {
            UDebug.LogErrorFormat(Format(format), args);
        }
        [Conditional(AppConst.SYMBOL_DEBUG)]
        public static void LogException(System.Exception exception)
        {
            UDebug.LogException(exception);
        }
    }
}
