using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class ExcelDefine
    {
        //一行excel数据字符串
        [System.Serializable]
        public class RowData
        {
            public string[] r = null;
        }

        public enum ExcelInitResult
        {
            Suceess,
            PathEmpty,
            NotSupportFileFormat,
            NothingCanRead,
            ReadError,
            SharingViolation,
            EmptyData
        }

        /// <summary>
        /// excel导出文件后缀名
        /// </summary>
        public const string EXTENSION_TXT = ".csv";

        /// <summary>
        /// excel导出序列化文件后缀名
        /// </summary>
        public const string EXTENSION_ASSET = ".asset";

        /// <summary>
        /// 表中数据注释用符号
        /// </summary>
        public const string IGNORE_FLAG = "//";
    }
}