
namespace B2Framework
{
    public static partial class Log
    {
        public interface ILogHelper
        {
            void Log(LogLevel level, object message);
        }
    }
}