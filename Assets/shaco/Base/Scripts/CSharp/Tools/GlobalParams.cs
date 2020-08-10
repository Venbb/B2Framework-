namespace shaco.Base
{
    public class GlobalParams
    {

#if DEBUG_LOG
        public const bool DEBUG_LOG = true;
#endif

        //shaco游戏框架版本号
        public const string SHACO_GAME_FRAMEWORK_VERSION = "1.7.7";

        //自定义数据文件后缀名
        public const string DATA_SAVE_EXTENSIONS = "sdata";

        //lua脚本后缀名
        public const string LUA_FILE_EXTENSIONS = ".lua.txt";

        //Unity c# dll库名字
        // public const string CSHARP_ASSEMBLY = "Assembly-CSharp";
        // public const string CSHARP_EDITOR_ASSEMBLY = "Assembly-CSharp-Editor";

        //默认分隔符
        public const string SIGN_FLAG = "@@";

        //空字符串
        public const string EmptyString = "";

        //强制定位标记，当日志行带该标记时候忽略框架代码过滤
        public const string FORCE_LOG_LOCATION_TAG = "double click to location";

        //强制定位日志颜色
        public const string FORCE_LOG_LOCATION_COLOR = "314D79FF";

        //在获取堆栈信息时候默认过滤的字符串标记
        static public readonly string[] DEFAULT_IGNORE_STACK_TAG = { "shaco/Base", "shaco." };

        /// <summary>
        /// 获取shaco框架所在文件夹路径
        /// </summary>
        static public string GetShacoFrameworkRootPath()
        {
            var currentFilePath = shaco.Base.FileHelper.GetCurrentSourceFolderPath();
            var retValue = currentFilePath.Substring("", "shaco/Base") + "shaco";
            if (retValue.StartsWith("//"))
            {
                retValue = retValue.ReplaceFromBegin("//", "\\\\", 1);
            }

#if UNITY_EDITOR
            //如果在Unity中使用xlua注入过那么根目录会发生变化，这里需要使用unity的路径重新获取下
            if (!retValue.Contains("/Assets/"))
                retValue = UnityEngine.Application.dataPath.ContactPath("shaco");
#endif
            return retValue;
        }

        /// <summary>
        /// 获取编辑器模板文件夹路径
        /// </summary>
        static public string GetShacoUnityEditorResourcesPath()
        {
            return GetShacoFrameworkRootPath().ContactPath("Base/EditorResources");
        }
    }
}