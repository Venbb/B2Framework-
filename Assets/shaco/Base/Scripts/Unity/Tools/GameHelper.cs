using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    /// <summary>
    /// 游戏类管理中心，负责unity使用类的管理
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class GameHelper : shaco.Base.GameHelper
    {
        /// <summary>
        /// Unity ui，支持任意以prefab为基础模型的ui框架
        /// </summary>
        static public shaco.IUIManager ui
        {
            get { return GetOrSetDefaultInterface<shaco.IUIManager, shaco.UIManager>(); }
            set { BindInterface<shaco.IUIManager>(value); }
        }

        /// <summary>
        /// Unity ui深度管理，控制IUIManager的显示的ui层级关系
        /// </summary>
        static public shaco.IUIDepthChange uiDepth
        {
            get { return GetOrSetDefaultInterface<shaco.IUIDepthChange, shaco.DefaultUIDepthChange>(); }
            set { BindInterface<shaco.IUIDepthChange>(value); }
        }

        /// <summary>
        /// UnityEngine.Resources类更好的替代方案
        /// 在Resources加载Unity本地Resources目录资源模式外还支持以下模式：
        /// 1、加载Assets目录下任意Unity支持格式文件(仅限Unity编辑器模式下，便于测试)
        /// 2、从热更新资源目录同步或者异步任意格式文件，可以事先调用HotUpdateImportWWW.CheckUpdate更新资源
        /// 3、根据热更新资源配置表异步动态下载并读取任意文件
        /// </summary>
        static public shaco.IResourcesEx res
        {
            get { return GetOrSetDefaultInterface<shaco.IResourcesEx, shaco.ResourcesEx>(); }
            set { BindInterface<shaco.IResourcesEx>(value); }
        }

        /// <summary>
        /// GameHelper.res的资源管理缓存池
        /// </summary>
        static public shaco.IHotUpdateDataCache resCache
        {
            get { return GetOrSetDefaultInterface<shaco.IHotUpdateDataCache, shaco.HotUpdateDataCache>(); }
            set { BindInterface<shaco.IHotUpdateDataCache>(value); }
        }

        /// <summary>
        /// 热更新模块，从服务器下载更新内容并导入到本地
        /// 热更新功能使用流程：
        /// 1、Unity编辑器中打开shaco -> HotUpdateResourceBuilder导出热更新配置和资源
        /// 2、建议调用全局方法进行资源更新GameHelper.hotupdate.CheckUpdate
        /// </summary>
        static public shaco.IHotUpdateImportWWW hotupdate
        {
            get { return GetOrSetDefaultInterface<shaco.IHotUpdateImportWWW, shaco.HotUpdateImportWWW>(); }
            set { BindInterface<shaco.IHotUpdateImportWWW>(value); }
        }

        /// <summary>
        /// http下载类
        /// </summary>
        static public shaco.IHttpDownLoader httpDownloader
        {
            get { return GetOrSetDefaultInterface<shaco.IHttpDownLoader, shaco.HttpDownLoader>(); }
            set { BindInterface<shaco.IHttpDownLoader>(value); }
        }

        /// <summary>
        /// http上传类
        /// </summary>
        static public shaco.IHttpUpLoader httpUpLoader
        {
            get { return GetOrSetDefaultInterface<shaco.IHttpUpLoader, shaco.HttpUpLoader>(); }
            set { BindInterface<shaco.IHttpUpLoader>(value); }
        }

        /// <summary>
        /// 音效管理类
        /// </summary>
        static public shaco.IAudioManager sound
        {
            get { return GetOrSetDefaultInterface<shaco.ISoundManager, shaco.SoundManager>(); }
            set { BindInterface<shaco.IAudioManager>(value); }
        }

        /// <summary>
        /// 人物声音管理类
        /// </summary>
        static public shaco.IAudioManager voice
        {
            get { return GetOrSetDefaultInterface<shaco.IVoiceManager, shaco.VoiceManager>(); }
            set { BindInterface<shaco.IAudioManager>(value); }
        }

        /// <summary>
        /// 音乐管理类
        /// </summary>
        static public shaco.IAudioManager music
        {
            get { return GetOrSetDefaultInterface<shaco.IMusicManager, shaco.MusicManager>(); }
            set { BindInterface<shaco.IAudioManager>(value); }
        }

        //现在XLuaManager是作为静态类实现了
        // /// <summary>
        // /// xlua使用管理类，支持对c#脚本动态在线更新，主要适用于打补丁来修复bug
        // /// </summary>
        // static public shaco.IXLuaManager xlua
        // {
        //     get { return GetOrSetDefaultInterface<shaco.IXLuaManager, shaco.XLuaManager>(); }
        //     set { BindInterface<shaco.IXLuaManager>(value); }
        // }

        /// <summary>
        /// 动画管理类
        /// </summary>
        static public shaco.IAction action
        {
            get { return GetOrSetDefaultInterface<shaco.IAction, shaco.Action>(); }
            set { BindInterface<shaco.IAction>(value); }
        }

        /// <summary>
        /// 图集管理类
        /// </summary>
        static public shaco.ISpriteAtlasManager atlas
        {
            get { return GetOrSetDefaultInterface<shaco.ISpriteAtlasManager, shaco.SpriteAtlasManager>(); }
            set { BindInterface<shaco.ISpriteAtlasManager>(value); }
        }

        /// <summary>
        /// 图集配置类
        /// </summary>
        static public shaco.ISpriteAtlasSettings atlasSettings
        {
            get { return GetOrSetDefaultInterface<shaco.ISpriteAtlasSettings, shaco.SpriteAtlasSettings>(); }
            set { BindInterface<shaco.ISpriteAtlasSettings>(value); }
        }

        /// <summary>
        /// 开启一个协程方法(全局)
        /// <param name="routine">协程方法</param>
        /// <return>协程返回值</return>
        /// </summary>
        static public UnityEngine.Coroutine StartCoroutine(IEnumerator routine)
        {
            var invokeTarget = shaco.GameHelper.action.GetGlobalInvokeTarget();
            return invokeTarget.StartCoroutine(routine);
        }

        /// <summary>
        /// 关闭一个协程方法(全局)
        /// <param name="routine">协程方法</param>
        /// <return>协程返回值</return>
        /// </summary>
        static public void StopCoroutine(IEnumerator routine)
        {
            var invokeTarget = shaco.GameHelper.action.GetGlobalInvokeTarget();
            invokeTarget.StopCoroutine(routine);
        }
    }
}