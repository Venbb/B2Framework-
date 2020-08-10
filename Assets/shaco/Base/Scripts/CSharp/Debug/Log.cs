using System.Collections;

namespace shaco.Base
{
    public class Log : ILog
    {
        override public void info(string message, object color = null, object context = null)
        {
            System.Console.WriteLine("Info:" + message);
        }

        override public void infoFormat(string message, params object[] args)
        {
            System.Console.WriteLine("InfoFormat:" + string.Format(message, args));
        }

        override public void warning(string message, object context = null)
        {
            System.Console.WriteLine("Warning:" + message);
        }

        override public void warningFormat(string message, params object[] args)
        {
            System.Console.WriteLine("Warning:" + string.Format(message, args));
        }

        override public void error(string message, object context = null)
        {
            System.Console.WriteLine("Error:" + message);
        }

        override public void errorFormat(string message, params object[] args)
        {
            System.Console.WriteLine("Error:" + string.Format(message, args));
        }

        override public void exception(string message)
        {
            throw new System.Exception(message);
        }

        override public void exceptionFormat(string message, params object[] args)
        {
            throw new System.Exception(string.Format(message, args));
        }

        override public void assert(bool codition, string message)
        {
            throw new System.Exception(message);
        }

        override public void assertFormat(bool codition, string message, params object[] args)
        {
            throw new System.Exception(string.Format(message, args));
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Info(string message, object context = null)
        {
            shaco.Base.GameHelper.log.info(message, null, context);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void InfoFormat(string message, params object[] args)
        {
            shaco.Base.GameHelper.log.infoFormat(message, args);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Warning(string message, object context = null)
        {
            shaco.Base.GameHelper.log.warning(message, context);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void WarningFormat(string message, params object[] args)
        {
            shaco.Base.GameHelper.log.warningFormat(message, args);
        }

        // [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Error(string message, object context = null)
        {
            shaco.Base.GameHelper.log.error(message, context);
        }

        // [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void ErrorFormat(string message, params object[] args)
        {
            shaco.Base.GameHelper.log.errorFormat(message, args);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Exception(string message)
        {
            shaco.Base.GameHelper.log.exception(message);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void ExceptionFormat(string message, params object[] args)
        {
            shaco.Base.GameHelper.log.exceptionFormat(message, args);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Assert(bool codition, string message)
        {
            shaco.Base.GameHelper.log.assert(codition, message);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void AssertFormat(bool codition, string message, params object[] args)
        {
            shaco.Base.GameHelper.log.assertFormat(codition, message, args);
        }
    }
}

