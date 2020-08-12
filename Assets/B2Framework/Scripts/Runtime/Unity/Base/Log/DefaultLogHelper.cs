
namespace B2Framework.Unity
{
    public class DefaultLogHelper : B2Framework.Log.ILogHelper
    {
        public void Log(LogLevel level, object message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(Utility.Text.Format("<color=#888888>{0}</color>", message.ToString()));
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(message.ToString());
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message.ToString());
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(message.ToString());
                    break;
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(Utility.Text.Format("!!!!{0}", message.ToString()));
                    break;
            }
        }
    }
}