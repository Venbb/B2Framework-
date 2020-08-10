
namespace shaco.Base
{
    public abstract class ILog : IGameInstance
    {
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void info(string message, object color = null, object context = null);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void infoFormat(string message, params object[] args);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void warning(string message, object context = null);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void warningFormat(string message, params object[] args);
        // [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void error(string message, object context = null);
        // [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void errorFormat(string message, params object[] args);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void exception(string message);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void exceptionFormat(string message, params object[] args);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void assert(bool codition, string message);
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void assertFormat(bool codition, string message, params object[] args);
    }
}
