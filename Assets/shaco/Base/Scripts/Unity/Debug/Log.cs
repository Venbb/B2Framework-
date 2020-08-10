using System.Collections;
using UnityEngine;
using System;

namespace shaco
{
    public class Log : shaco.Base.ILog
    {
        public class LogConfig
        {
            public string message = string.Empty;
            public Color color = new Color(0.58f, 0.58f, 0.58f, 1);

            public LogConfig(string message) { this.message = message; }
            public LogConfig(string message, Color color) { this.message = message; this.color = color; }
        }

        override public void info(string message, object color = null, object context = null)
        {
            if (null != color && color is UnityEngine.Color)
                Debug.Log(MakeRichtextLog(message, (UnityEngine.Color)color), context as UnityEngine.Object);
            else
                Debug.Log(message, context as UnityEngine.Object);
        }

        override public void infoFormat(string message, params object[] args)
        {
            Debug.Log(string.Format(message, args));
        }

        override public void warning(string message, object context = null)
        {
            Debug.LogWarning(message, context as UnityEngine.Object);
        }

        override public void warningFormat(string message, params object[] args)
        {
            Debug.LogWarning(string.Format(message, args));
        }

        override public void error(string message, object context = null)
        {
            Debug.LogError(message, context as UnityEngine.Object);
        }

        override public void errorFormat(string message, params object[] args)
        {
            Debug.LogError(string.Format(message, args));
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
            shaco.GameHelper.log.info(message, null, context);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Info(string message, UnityEngine.Color color, object context = null)
        {
            shaco.GameHelper.log.info(message, color, context);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void InfoFormat(string message, params object[] args)
        {
            shaco.GameHelper.log.infoFormat(message, args);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Warning(string message, object context = null)
        {
            shaco.GameHelper.log.warning(message, context);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void WarningFormat(string message, params object[] args)
        {
            shaco.Base.GameHelper.log.warningFormat(message, args);
        }

        // [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Error(string message, object context = null)
        {
            shaco.GameHelper.log.error(message, context);
        }

        // [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void ErrorFormat(string message, params object[] args)
        {
            shaco.GameHelper.log.errorFormat(message, args);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Exception(string message)
        {
            shaco.GameHelper.log.exception(message);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void ExceptionFormat(string message, params object[] args)
        {
            shaco.GameHelper.log.exceptionFormat(message, args);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void Assert(bool codition, string message)
        {
            shaco.GameHelper.log.assert(codition, message);
        }

        [System.Diagnostics.Conditional("DEBUG_LOG")]
        static public void AssertFormat(bool codition, string message, params object[] args)
        {
            shaco.GameHelper.log.assertFormat(codition, message, args);
        }

        private string MakeRichtextLog(string message, Color color)
        {
            string r = Convert.ToString((int)(color.r * 255), 16); if (r.Length == 1) r += "0";
            string g = Convert.ToString((int)(color.g * 255), 16); if (g.Length == 1) g += "0";
            string b = Convert.ToString((int)(color.b * 255), 16); if (b.Length == 1) b += "0";
            string a = Convert.ToString((int)(color.a * 255), 16); if (a.Length == 1) a += "0";

            string ret = string.Format("<color=#{0}>{1}</color>", r + g + b + a, message);
            return ret;
        }
    }
}

