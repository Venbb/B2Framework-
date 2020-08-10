using System.Collections;

namespace shaco
{
    /// <summary>
    /// 资源读取模式
    /// </summary>
    public enum ResourcesLoadMode
    {
        //真机环境模式
        //资源读取目录支持，Resources, 本地下载目录
        RunTime,
        //编辑器开发模式
        //资源读取目录支持，Resources，DEFAULT_PREFIX_PATH_LOWER, 本地下载目录
        EditorDevelop,
    }

    /// <summary>
    /// 资源读取目录顺序
    /// </summary>
    public enum ResourcesLoadOrder
    {
        //优先读取本地下载目录
        DownloadFirst,  
        //优先读取Resorces目录，DEFAULT_PREFIX_PATH_LOWER目录(仅限EditorDevelop模式)
        ResourcesFirst,
    }

    public interface IResourcesEx : shaco.Base.IGameInstance
    {
        ResourcesLoadMode resourcesLoadMode { get; set; }
        ResourcesLoadOrder resourcesLoadOrder { get; set; }

        string DEFAULT_PREFIX_PATH { get; }
        string DEFAULT_PREFIX_PATH_LOWER { get; }

        T LoadResourcesOrLocal<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object;
        UnityEngine.Object LoadResourcesOrLocal(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        void LoadResourcesOrLocalAsync<T>(string path, System.Action<T> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object;
        void LoadResourcesOrLocalAsync(string path, System.Type type, System.Action<UnityEngine.Object> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        IEnumeratorRequest<T> LoadResourcesOrLocalCoroutine<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object;
        IEnumeratorRequest<UnityEngine.Object> LoadResourcesOrLocalCoroutine(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        T[] LoadResourcesOrLocalAll<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object;

        UnityEngine.Object[] LoadResourcesOrLocalAll(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);
        UnityEngine.Object[] LoadResourcesOrLocalAll(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        void LoadResourcesOrLocalAsyncAll(string path, System.Type type, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        void LoadResourcesOrLocalAsyncAll(string path, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        bool UnloadAssetBundleLocal(string path, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true);

        bool ExistsResourcesOrLocal(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);
        bool ExistsResourcesOrLocal<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString) where T : UnityEngine.Object;
        bool ExistsResourcesOrLocal(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);
    }
}
