namespace B2Framework.Unity
{
    public static partial class AppConst
    {
        /// <summary>
        /// 1K = 1024B
        /// </summary>
        public const long SIZE_KB = 1024;
        /// <summary>
        /// 1M = 1024K
        /// </summary>
        public const long SIZE_MB = 1024 * 1024;
        /// <summary>
        /// 1G = 1024M
        /// </summary>
        public const long SIZE_GB = 1024 * 1024 * 1024;
        /// <summary>
        /// 包输出路径
        /// </summary>
        public const string RELEASE_DIR = "release";
        /// <summary>
        /// 存放AssetBundles资源目录名称
        /// </summary>
        public const string ASSETBUNDLES = "AssetBundles";
        /// <summary>
        /// AssetBundle资源路径
        /// </summary>
        public const string ASSETBUNDLE_ASSETS_PATH = "Assets/" + ASSETBUNDLES;
        /// <summary>
        /// AssetBundle默认变体名称
        /// </summary>
        public const string ASSETBUNDLE_VARIANT = ".bundle";
        /// <summary>
        /// AssetBundle 配置表文件名称
        /// </summary>
        public const string BUNDLE_MANIFEST = "Manifest";
        /// <summary>
        /// 版本号显示文本
        /// </summary>
        /// <value></value>
        public const string VERSION = "App v:{0} Res v:{1}";
        /// <summary>
        /// 资源版本号文件
        /// 存放了资源版本号和资源信息
        /// </summary>
        public const string RES_VER_FILE = "ver";
        /// <summary>
        /// Lua脚本资源路径
        /// </summary>
        public const string LUA_SCRIPTS_PATH = "Assets/Scripts/Lua";
        /// <summary>
        /// Lua扩展名
        /// </summary>
        public const string LUA_EXTENSIONS = ".lua,.lua.txt,.lua.bytes";
        /// <summary>
        /// Lua扩展名
        /// </summary>
        public const string LUA_EXTENSION = ".lua.bytes";
        /// <summary>
        /// 日志打印标签
        /// </summary>
        public const string SYMBOL_DEBUG = "DEBUG_LOG";
    }
}