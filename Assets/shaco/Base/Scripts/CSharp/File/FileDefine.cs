using System.Collections;

namespace shaco.Base
{
    public class FileDefine
    {
        public enum FileExtension
        {
            None = -1,
            AssetBundle = 0,
            Prefab,
            Png,
            Jpg,
            Txt,
            Json,
            Xml,
            Lua,
            Bytes,
        }

        static public string persistentDataPath = string.Empty;
       
        public const char PATH_FLAG_SPLIT = '/';
        public const string PATH_FLAG_SPLIT_STRING = "/";
        public const char DOT_SPLIT = '.';
        public const string DOT_SPLIT_STRING = ".";
        public const long ONE_KB = 1024;
        public const long ONE_MB = 1024 * 1024;
        public const long ONE_GB = 1024 * 1024 * 1024;
    }
}
