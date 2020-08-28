namespace B2Framework
{
    public class DefaultLogHelper : B2Framework.ILogHelper
    {
        private string Format(object message, LogLevel level)
        {
            return string.Format("[{0}]{1}", level.ToString().ToUpper(), message != null ? message : "Null");
        }
        public void Log(LogLevel level, object message)
        {
            var msg = Format(message, level);
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(msg);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(string.Format("<color=#0000FF>{0}</color>", msg));
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(string.Format("<color=#FFEB04>{0}</color>", msg));
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(string.Format("<color=#FF0000>{0}</color>", msg));
                    break;
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(string.Format("<color=#B23333>{0}</color>", msg));
                    break;
            }
        }
    }
}