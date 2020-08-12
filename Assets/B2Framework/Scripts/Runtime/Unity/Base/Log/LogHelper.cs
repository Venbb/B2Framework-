
namespace B2Framework.Unity
{
    public class LogHelper : B2Framework.Log.ILogHelper
    {
        public void Log(LogLevel level, object message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(message);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(Utility.Text.Format("<color=#0000FF>{0}</color>", message));
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(Utility.Text.Format("<color=#FFEB04>{0}</color>", message));
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(Utility.Text.Format("<color=#FF0000>{0}</color>", message));
                    break;
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(Utility.Text.Format("<color=#B23333>{0}</color>", message));
                    break;
            }
        }
    }
}