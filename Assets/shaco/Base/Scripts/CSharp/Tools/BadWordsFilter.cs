using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace shaco.Base
{
    public class BadWordsFilter
    {
        static public BadWordsFilter Instance { get { return GameEntry.GetInstance<BadWordsFilter>(); } }

        /// <summary>
        /// 敏感字数组
        /// </summary>
        private string[] _filters = null;

        public void LoadFromString(string readString, System.Action<float> callbackProgress = null, string splitFlag = "\n")
        {
            if (null == callbackProgress)
            {
                if (readString.Contains("\r\n"))
                    _filters = readString.Replace("\r\n", "\n").Split(splitFlag);
                else
                    _filters = readString.Split(splitFlag);
            }
            else
            {
                shaco.Base.ThreadPool.RunThread(() =>
                {
                    if (readString.Contains("\r\n"))
                        _filters = readString.Replace("\r\n", "\n").Split(splitFlag);
                    else
                        _filters = readString.Split(splitFlag);
                    callbackProgress(1);
                });
            }
        }

        /// <summary>
        /// 检查输入内容是否包含脏词（包含返回true）
        /// <param name="content">欲过滤的内容</param>
        /// <param name="filter_deep">检测深度，即_filters数组中的每个词中的插入几个字以内会被过滤掉，例：检测深度为2，s_filters中有个词是中国，那么“中国”、“中*国”，“中**国”都会被过滤掉（*是任意字）。</param>
        /// </summary>
        public bool HasBadWords(string content, int filter_deep = 1)
        {
            string result = string.Empty;
            return Filter(content, out result, filter_deep, true, false);
        }

        /// <summary>
        /// 脏词替换成*号
        /// </summary>
        public string Filter(string content, int filter_deep = 1, string replacement = "*")
        {
            string result = string.Empty;
            Filter(content, out result, filter_deep, false, false, replacement);
            return result;
        }

        /// <summary>
        /// 初始化s_filters之后调用filter函数
        /// </summary>
        /// <param name="content">欲过滤的内容</param>
        /// <param name="result_str">执行过滤之后的内容</param>
        /// <param name="filter_deep">检测深度，即_filters数组中的每个词中的插入几个字以内会被过滤掉，例：检测深度为2，s_filters中有个词是中国，那么“中国”、“中*国”，“中**国”都会被过滤掉（*是任意字）。</param>
        /// <param name="check_only">是否只检测而不执行过滤操作</param>
        /// <param name="bTrim">过滤之前是否要去掉头尾的空字符</param>
        /// <param name="replace_str">将检测到的敏感字替换成的字符</param>
        /// <returns></returns>
        public bool Filter(string content, out string result_str, int filter_deep = 1, bool check_only = false, bool bTrim = false, string replace_str = "*")
        {
            string result = content;
            if (bTrim)
            {
                result = result.Trim();
            }
            result_str = result;

            if (_filters == null)
            {
                return false;
            }

            bool check = false;
            for (int i = _filters.Length - 1; i >= 0; --i)
            {
                string s = _filters[i].TrimEnd();
                if (s.Length == 0)
                {
                    continue;
                }

                bool bFiltered = true;
                while (bFiltered)
                {
                    int result_index_start = -1;
                    int result_index_end = -1;
                    int idx = 0;
                    while (idx < s.Length)
                    {
                        string one_s = s.Substring(idx, 1);
                        if (one_s == replace_str)
                        {
                            ++idx;
                            continue;
                        }
                        if (result_index_end + 1 >= result.Length)
                        {
                            bFiltered = false;
                            break;
                        }
                        int new_index = result.IndexOf(one_s, result_index_end + 1, System.StringComparison.OrdinalIgnoreCase);
                        if (new_index == -1)
                        {
                            bFiltered = false;
                            break;
                        }
                        if (idx > 0 && new_index - result_index_end > filter_deep + 1)
                        {
                            bFiltered = false;
                            break;
                        }
                        result_index_end = new_index;

                        if (result_index_start == -1)
                        {
                            result_index_start = new_index;
                        }
                        idx++;
                    }

                    if (bFiltered)
                    {
                        if (check_only)
                        {
                            return true;
                        }
                        check = true;
                        string result_left = result.Substring(0, result_index_start);
                        for (int j = result_index_start; j <= result_index_end; ++j)
                        {
                            result_left += replace_str;
                        }
                        string result_right = result.Substring(result_index_end + 1);
                        result = result_left + result_right;
                    }
                }
            }
            result_str = result;
            return check;
        }
    }
}