using System;

namespace B2Framework
{
    public enum Platform
    {
        Unknown = 0,
        /// <summary>
        /// Editor
        /// </summary>
        Editor = 1 << 0,

        /// <summary>
        /// Windows 32 位
        /// </summary>
        Windows = 1 << 1,

        /// <summary>
        /// Windows 64 位
        /// </summary>
        Windows64 = 1 << 2,

        /// <summary>
        /// macOS
        /// </summary>
        MacOSX = 1 << 3,

        /// <summary>
        /// Linux
        /// </summary>
        Linux = 1 << 4,

        /// <summary>
        /// iOS
        /// </summary>
        iOS = 1 << 5,

        /// <summary>
        /// Android
        /// </summary>
        Android = 1 << 6,

        /// <summary>
        /// Windows Store
        /// </summary>
        WindowsStore = 1 << 7,

        /// <summary>
        /// WebGL
        /// </summary>
        WebGL = 1 << 8,
    }
    public enum Scenes
    {
        Splash,
        Launcher,
        Updater,
        Main,
        Loading,
        Battle,
    }
    /// <summary>
    /// 对应AssetBundles目录下的文件夹
    /// </summary>
    public enum AssetBundles
    {
        Animations,
        Assets,// ScriptableObject .asset
        Audio,
        Fonts,
        Localization,
        Lua,// Lua脚本 ，.bytes
        Materials,
        Prefabs,
        Protos,//pb文件，.bytes
        Shaders,
        Textures,
    }
    /// <summary>
    /// 游戏支持的语言
    /// </summary>
    [Serializable]
    public enum GameLanguage
    {
        None = 0, // Custom name for "Nothing" option
        English = 1 << 0,
        ChineseSimplified = 1 << 1,
        // AB = A | B, // Combination of two flags
        ChineseTraditional = 1 << 2,
        Japanese = 1 << 3,
        All = ~0, // Custom name for "Everything" option
    }
    /// <summary>
    ///  资源加载状态
    /// </summary>
    public enum LoadState
    {
        Init,
        LoadAssetBundle,// 正在加载AB
        LoadAsset,      // 正在加载Asset
        Loaded,         // 加载完成
        Unload,         // 已卸载
    }
    public enum VerifyBy
    {
        Size,
        Hash,
    }
    /// <summary>
    /// 资源更新状态
    /// </summary>
    public enum UStatus
    {
        Idle,
        Requesting,     // "请求版本信息..."
        Checking,       // "正在检测资源更新..."
        CheckOver,      // "检测完毕...";
        Downloading,    // "正在更新资源...";
        Completed,      // "正在准备资源...";
    }
    /// <summary>
    /// 资源更新方式
    /// </summary>
    public enum UAct
    {
        No,     // 不需要更新
        App,    // 大版本更新，需要重新下载游戏
        Res,    // 资源热更新
    }
    /// <summary>
    /// 绑定对象类型
    /// </summary>
    [System.Serializable]
    public enum VariableEnum
    {
        Component,
        GameObject,
        Object,
        Boolean,
        Integer,
        Float,
        String,
        Color,
        Vector2,
        Vector3,
        Vector4
    }
    /// <summary>
    /// Lua脚本绑定方式
    /// </summary>
    public enum LuaScriptBindEnum
    {
        TextAsset,
        Filename
    }
    /// <summary>
    /// 资源更新模式。
    /// </summary>
    public enum ResourceMode : byte
    {
        /// <summary>
        /// 未指定。
        /// </summary>
        Unspecified = 0,

        /// <summary>
        /// 单机模式。
        /// </summary>
        Package,

        /// <summary>
        /// 预下载的可更新模式。
        /// </summary>
        PreUpdate,

        /// <summary>
        /// 使用时下载的可更新模式。
        /// </summary>
        RuntimeUpdate
    }
}