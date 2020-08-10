using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public static partial class Utility
    {
        /// <summary>
        /// 支持的语言判断方法表
        /// </summary>
        static private System.Func<char, bool>[] supportLanguageJudgeRangeCallBacks = new System.Func<char, bool>[] 
        { 
            shaco.Base.Utility.IsChinese,
            shaco.Base.Utility.IsEnglish,
            shaco.Base.Utility.IsJapanese,
        };

        /// <summary>
        /// 支持操作的语言类型
        /// </summary>
        public enum LanguageType
        {
            SimpleChinese,
            TraditionalChinese,
            English,
            Japanese
        }

        /// <summary>
        /// 导入语言收集信息
        /// </summary>
        public class LocalizationCollectnfo
        {
            //文件路径
            public string path = string.Empty;

            //文本内容
            public string languageString = string.Empty;

            //额外参数
            public string parameter = "*";
        }

        /// <summary>
        /// 导出语言信息
        /// </summary>
        public class LocalizationExportInfo
        {
            //文件路径
            public string path = string.Empty;

            //原文，默认不允许为空字符串
            public string textOriginal = "*";

            //译文，默认不允许为空字符串
            public string textTranslation = "*";

            //原文所在行(不包含注释)
            public int lineOriginal = -1;

            //额外参数，默认不允许为空字符串
            public string parameter = "*";
        }

        /// <summary>
        /// 收集语言标记符号
        /// </summary>
        [System.Serializable]
        public class LocalizationCollectPairFlag
        {
            //起始符号
            public string startFlag = string.Empty;

            //终止符号
            public string endFlag = string.Empty;
        }

        /// <summary>
        /// 判断字符串是否为简体中文
        /// <param name="str">中文字符串</param>
        /// <return>true：简体中文 false：繁体中文</return>
        /// </summary>
        static public bool IsContainSimpleChinese(string chineseString)
        {
            var gb2312 = System.Text.Encoding.GetEncoding("gb2312");
            bool retValue = false;

            //不是中文则直接返回false
            if (!IsChinese(chineseString))
                return retValue;

            for (int i = chineseString.Length - 1; i >= 0; --i)
            {
                var charValue = chineseString[i];
                byte[] gb2312Bytes = gb2312.GetBytes(charValue.ToString());

                //only 1 or 2
                if (gb2312Bytes.Length == 2) //double bytes char
                {
                    if (gb2312Bytes[0] >= 0xB0
                        && gb2312Bytes[1] <= 0xF7
                        && gb2312Bytes[1] >= 0xA1
                        && gb2312Bytes[1] <= 0xFE)
                    {
                        retValue = true;
                        break;
                    }
                }
            }
            return retValue;
        }

        /// <summary>
        /// 判断字符串是否为繁体中文
        /// <param name="str">中文字符串</param>
        /// <return>true：繁体中文 false：简体中文</return>
        /// </summary>
        static public bool IsTraditionalChinese(string chineseString)
        {
            return IsChinese(chineseString) && !IsContainSimpleChinese(chineseString);
        }

        /// <summary>
        /// 判断字符串是否为中文
        /// <param name="japaneseString">中文字符串</param>
        /// <return>true：中文 false：不是中文</return>
        /// </summary>
        static public bool IsChinese(string chineseString)
        {
            return JudgeLanguageWithCodeRange(chineseString, true, shaco.Base.Utility.IsChinese);
        }

        /// <summary>
        /// 判断字符串是否为英文
        /// <param name="englishString">英文字符串</param>
        /// <return>true：英文 false：不是英文</return>
        /// </summary>
        static public bool IsEnglish(string englishString)
        {
            return JudgeLanguageWithCodeRange(englishString, true, shaco.Base.Utility.IsEnglish);
        }

        /// <summary>
        /// 判断字符串是否为日文
        /// <param name="japaneseString">日文字符串</param>
        /// <return>true：日文 false：不是日文</return>
        /// </summary>
        static public bool IsJapanese(string japaneseString)
        {
            return JudgeLanguageWithCodeRange(japaneseString, false, shaco.Base.Utility.IsJapanese);
        }

        static private bool IsChinese(char chineseChar)
        {
            return chineseChar >= 0x4e00 && chineseChar <= 0x9fbb;
        }

        static private bool IsEnglish(char englishChar)
        {
            return (englishChar >= 'a' && englishChar <= 'z') || (englishChar >= 'A' && englishChar <= 'Z');
        }

        static private bool IsJapanese(char japaneseChr)
        {
            return (japaneseChr >= 0x3040 && japaneseChr <= 0x3090) || (japaneseChr >= 0x30A0 && japaneseChr <= 0x30FF);
        }

        /// <summary>
        /// 判断字符串语言是否在指定编码范围内
        /// <param name="languageString">语言字符串</param>
        /// <param name="isAllInRange">判断包含范围规则，true：全部字符串包含在范围内，false：其中一个字符串包含在范围内</param>
        /// <param name="inRangeCallBack">范围内编码判断</param>
        /// <return>所有字符串是否在编码范围内</return>
        /// </summary>
        static private bool JudgeLanguageWithCodeRange(string languageString, bool isAllInRange, System.Func<char, bool> inRangeCallBack)
        {
            if (null == inRangeCallBack)
            {
                shaco.Base.Log.Error("Utility+Localization JudgeLanguageWithCodeRange error: invalid param");
                return false;
            }

            var outRangeCallBacks = new List<System.Func<char, bool>>();
            if (!supportLanguageJudgeRangeCallBacks.IsNullOrEmpty())
            {
                for (int i = supportLanguageJudgeRangeCallBacks.Length - 1; i >= 0; --i)
                {
                    if (supportLanguageJudgeRangeCallBacks[i] != inRangeCallBack)
                    {
                        outRangeCallBacks.Add(supportLanguageJudgeRangeCallBacks[i]);
                    }
                }
            }

            bool hasOneInRange = false;
            for (int i = languageString.Length - 1; i >= 0; --i)
            {
                var codeTmp = languageString[i];

                //默认为在编码范围内，如果编码范围外也找不到则判定为需要过滤的字符
                bool isInRange = true;

                //在编码范围内
                if (inRangeCallBack(codeTmp))
                {
                    isInRange = true;
                    hasOneInRange = true;
                    break;
                }
                else 
                {
                    for (int j = outRangeCallBacks.Count - 1; j >= 0; --j)
                    {
                        //在编码范围外
                        if (outRangeCallBacks[j](codeTmp))
                        {
                            isInRange = false;
                            break;
                        }
                    }
                }
                //只有其中有一个不是编码内的字符串就返回false
                if (isAllInRange && !isInRange)
                    return false;
            }

            //所有字符串都在编码范围内
            return hasOneInRange;
        }

        /// <summary>
        /// 筛选简体中文
        /// <param name="value">需要筛选的文本内容</param>
        /// <param name="collectFlags">需要筛选的符号</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectSimpleChineseCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            return SelectLanguageCharacter(value, collectFlags, shaco.Base.Utility.IsContainSimpleChinese);
        }

        /// <summary>
        /// 筛选繁体中文
        /// <param name="value">需要筛选的文本内容</param>
        /// <param name="collectFlags">需要筛选的符号</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectTraditionalChineseCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            return SelectLanguageCharacter(value, collectFlags, shaco.Base.Utility.IsTraditionalChinese);
        }

        /// <summary>
        /// 获取其中的日文字符，只能判断文字中是否包含片假名或者平假名，无法判断日文中的汉字
        /// <param name="value">需要筛选的文本内容</param>
        /// <param name="collectFlags">需要筛选的符号</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectJapaneseCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            return SelectLanguageCharacter(value, collectFlags, shaco.Base.Utility.IsJapanese);
        }

        /// <summary>
        /// 获取其中的英文字符
        /// <param name="value">需要筛选的文本内容</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static public Dictionary<string, LocalizationExportInfo> SelectEnglishCharacter(string value, List<LocalizationCollectPairFlag> collectFlags)
        {
            return SelectLanguageCharacter(value, collectFlags, shaco.Base.Utility.IsEnglish);
        }

        /// <summary>
        /// 获取其中的中文字符
        /// <param name="value">字符串</param>
        /// <param name="startCode">字符开始编码</param>
        /// <param name="endCode">字符终止编码</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static private Dictionary<string, LocalizationExportInfo> SelectLanguageCharacter(string value, List<LocalizationCollectPairFlag> collectFlags, System.Func<string, bool> judgeLanguageCallBack)
        {
            Dictionary<string, LocalizationExportInfo> retValue = null;

            if (!collectFlags.IsNullOrEmpty())
            {
                //筛选文本
                for (int i = 0; i < collectFlags.Count; ++i)
                {
                    var startSplitCode = collectFlags[i].startFlag;
                    var endSplitCode = collectFlags[i].endFlag;

                    retValue = SelectLanguageCharacterWithSplitCode(value, judgeLanguageCallBack, startSplitCode, endSplitCode);
                }
            }
            else
            {
                retValue = SelectLanguageCharacterWithSplitCode(value, judgeLanguageCallBack, string.Empty, string.Empty);
            }

            return retValue;
        }

        /// <summary>
        /// 根据指定的拆分符号筛选语言
        /// <param name="value">字符串</param>
        /// <param name="judgeLanguageCallBack">判断字符串语言的回调方法</param>
        /// <param name="splitStartCode">拆分起始符号</param>
        /// <param name="splitEndCode">拆分终止符号</param>
        /// <return>筛选出的文本内容</return>
        /// </summary>
        static private Dictionary<string, LocalizationExportInfo> SelectLanguageCharacterWithSplitCode(string value, System.Func<string, bool> judgeLanguageCallBack, string splitStartCode, string splitEndCode)
        {
            Dictionary<string, LocalizationExportInfo> retValue = new Dictionary<string, LocalizationExportInfo>();

            var splitList = value.SplitWithoutTransfer(splitStartCode, splitEndCode);

            for (int i = splitList.Count - 1; i >= 0; --i)
            {
                var strTmp = splitList[i];
                bool isChecked = judgeLanguageCallBack(strTmp);

                //如果不是目标语言，则从列表中删除
                if (!isChecked)
                {
                    splitList.RemoveAt(i);
                }
                //添加目标语言信息
                else
                {
                    if (!retValue.ContainsKey(strTmp))
                    {
                        retValue.Add(strTmp, new LocalizationExportInfo() { textOriginal = strTmp });
                    }
                }
            }

            return retValue;
        }
    }
}