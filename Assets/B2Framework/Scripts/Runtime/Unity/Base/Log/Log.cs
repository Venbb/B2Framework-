
namespace B2Framework.Unity
{
    public static class Log
    {
        private static string Format(object message)
        {
            return Utility.Text.Format("{0:yyyy-MM-dd HH:mm:ss.fff} {1}", System.DateTime.Now, message != null ? message : "Null");
        }
        public static void Debug(object message)
        {
            B2Framework.Log.Debug(Format(message));
        }
        public static void Debug(string format, params object[] args)
        {
            B2Framework.Log.Debug(Format(format), args);
        }
        public static void Info(object message)
        {
            B2Framework.Log.Info(Format(message));
        }
        public static void Info(string format, params object[] args)
        {
            B2Framework.Log.Info(Format(format), args);
        }
        public static void Warning(object message)
        {
            B2Framework.Log.Warning(Format(message));
        }
        public static void Warning(string format, params object[] args)
        {
            B2Framework.Log.Warning(Format(format), args);
        }
        public static void Error(object message)
        {
            B2Framework.Log.Error(Format(message));
        }
        public static void Error(object format, params object[] args)
        {
            B2Framework.Log.Error(Format(format), args);
        }
    }
}
