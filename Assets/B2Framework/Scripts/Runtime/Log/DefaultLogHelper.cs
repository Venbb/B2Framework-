
namespace B2Framework
{
    public class DefaultLogHelper : B2Framework.Log.ILogHelper
    {
        private string Format(object message, LogLevel level)
        {
            return Utility.Text.Format("{0:yyyy-MM-dd HH:mm:ss.fff} [{1}] {2}", level.ToString().ToUpper(), System.DateTime.Now, message != null ? message : "Null");
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
                    UnityEngine.Debug.Log(Utility.Text.Format("<color=#0000FF>{0}</color>", msg));
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(Utility.Text.Format("<color=#FFEB04>{0}</color>", msg));
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(Utility.Text.Format("<color=#FF0000>{0}</color>", msg));
                    break;
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(Utility.Text.Format("<color=#B23333>{0}</color>", msg));
                    break;
            }
        }
    }
}