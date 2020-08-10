using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace B2Framework
{
    public static class Language
    {
        /// <summary>
        /// 默认简体中文
        /// </summary>
        public static string DEFAULT = "zh";
        /// <summary>
        /// 语言标记映射
        /// </summary>
        /// <typeparam name="SystemLanguage"></typeparam>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        private static readonly Dictionary<SystemLanguage, string> languages = new Dictionary<SystemLanguage, string>()
        {
            { SystemLanguage.Afrikaans, "af" },
            { SystemLanguage.Arabic , "ar" },
            { SystemLanguage.Basque , "eu" },
            { SystemLanguage.Belarusian , "be" },
            { SystemLanguage.Bulgarian , "bg" },
            { SystemLanguage.Catalan , "ca" },
            { SystemLanguage.Chinese , "zh" },
            { SystemLanguage.ChineseSimplified , "zh" },
            { SystemLanguage.ChineseTraditional , "zh-Hant" },
            { SystemLanguage.Czech , "cs" },
            { SystemLanguage.Danish , "da" },
            { SystemLanguage.Dutch , "nl" },
            { SystemLanguage.English , "en" },
            { SystemLanguage.Estonian , "et" },
            { SystemLanguage.Faroese , "fo" },
            { SystemLanguage.Finnish , "fi" },
            { SystemLanguage.French , "fr" },
            { SystemLanguage.German , "de" },
            { SystemLanguage.Greek , "el" },
            { SystemLanguage.Hebrew , "he" },
            { SystemLanguage.Hungarian , "hu" },
            { SystemLanguage.Icelandic , "is" },
            { SystemLanguage.Indonesian , "id" },
            { SystemLanguage.Italian , "it" },
            { SystemLanguage.Japanese , "ja" },
            { SystemLanguage.Korean , "ko" },
            { SystemLanguage.Latvian , "lv" },
            { SystemLanguage.Lithuanian , "lt" },
            { SystemLanguage.Norwegian , "no" },
            { SystemLanguage.Polish , "pl" },
            { SystemLanguage.Portuguese , "pt" },
            { SystemLanguage.Romanian , "ro" },
            { SystemLanguage.Russian , "ru" },
            { SystemLanguage.SerboCroatian , "hr" },
            { SystemLanguage.Slovak , "sk" },
            { SystemLanguage.Slovenian , "sl" },
            { SystemLanguage.Spanish , "es" },
            { SystemLanguage.Swedish , "sv" },
            { SystemLanguage.Thai , "th" },
            { SystemLanguage.Turkish , "tr" },
            { SystemLanguage.Ukrainian , "uk" },
            { SystemLanguage.Vietnamese , "vi" }
        };
        public static string GetLanguage(GameLanguage lan)
        {
           var name = lan.ToString();
            SystemLanguage s_lan = (SystemLanguage)Enum.Parse(typeof(SystemLanguage), name);
            return GetLanguage(s_lan);
        }
        public static string GetLanguage(SystemLanguage lan)
        {
            string code = "";
            if (!languages.TryGetValue(lan, out code))
                code = DEFAULT;
            return code;
        }
        /// <summary>
        /// 当前系统语言
        /// CultureInfo.CurrentCulture.Name
        /// </summary>
        /// <value></value>
        public static string current
        {
            get
            {
                return GetLanguage(Application.systemLanguage);
            }
        }
    }
}