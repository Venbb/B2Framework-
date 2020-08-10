using System.Collections;

namespace shaco
{
    /// <summary>
    /// 该类主要用于加载包体资源(Resources.Load)或者下载资源(HotUpdateWWW)
    /// </summary>
    public partial class ResourcesEx : shaco.IResourcesEx
    {
        public ResourcesLoadMode resourcesLoadMode { get { CheckInitResourceMode(); return _resourcesLoadMode; } set { _resourcesLoadMode = value; }}
        public ResourcesLoadOrder resourcesLoadOrder { get { CheckInitResourceMode(); return _resourcesLoadOrder; } set { _resourcesLoadOrder = value; } }

        private ResourcesLoadMode _resourcesLoadMode;
        private ResourcesLoadOrder _resourcesLoadOrder;

        virtual public string DEFAULT_PREFIX_PATH { get { return "Assets/Resources_HotUpdate/"; } }
        virtual public string DEFAULT_PREFIX_PATH_LOWER { get { return "assets/resources_hotupdate/"; } }

        public T LoadResourcesOrLocal<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object
        {
            return (T)_LoadResourcesOrLocal(path, multiVersionControlRelativePath, autoCheckDepencies, typeof(T));
        }
        public UnityEngine.Object LoadResourcesOrLocal(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            return _LoadResourcesOrLocal(path, multiVersionControlRelativePath, autoCheckDepencies, type);
        }

        public void LoadResourcesOrLocalAsync<T>(string path, System.Action<T> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, typeof(T), obj => callbackLoadEnd(obj as T), callbackProgress);
        }
        public void LoadResourcesOrLocalAsync(string path, System.Type type, System.Action<UnityEngine.Object> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            _LoadResourcesOrLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, type, callbackLoadEnd, callbackProgress);
        }
        
        public IEnumeratorRequest<T> LoadResourcesOrLocalCoroutine<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object
        {
            var retValue = new ResourcesRequest<T>();
            retValue.Init(path, typeof(T), multiVersionControlRelativePath);
            return retValue;
        }

        public IEnumeratorRequest<UnityEngine.Object> LoadResourcesOrLocalCoroutine(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            var retValue = new ResourcesRequest<UnityEngine.Object>();
            retValue.Init(path, type, multiVersionControlRelativePath);
            return retValue;
        }

        public T[] LoadResourcesOrLocalAll<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true) where T : UnityEngine.Object
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, autoCheckDepencies, typeof(T)).Convert(v => (T)(object)v);
        }

        public UnityEngine.Object[] LoadResourcesOrLocalAll(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, autoCheckDepencies, type);
        }
        public UnityEngine.Object[] LoadResourcesOrLocalAll(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            return _LoadAllResourcesOrLocal(path, multiVersionControlRelativePath, autoCheckDepencies, typeof(UnityEngine.Object));
        }

        public void LoadResourcesOrLocalAsyncAll(string path, System.Type type, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, type, callbackLoadEnd, callbackProgress);
        }
        public void LoadResourcesOrLocalAsyncAll(string path, System.Action<UnityEngine.Object[]> callbackLoadEnd, System.Action<float> callbackProgress = null, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            _LoadAllResourcesOrLocalAsync(path, multiVersionControlRelativePath, autoCheckDepencies, typeof(UnityEngine.Object), callbackLoadEnd, callbackProgress);
        }

        public bool UnloadAssetBundleLocal(string path, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString, bool autoCheckDepencies = true)
        {
            return UnloadAssetBundleLocal(path, unloadAllLoadedObjects, multiVersionControlRelativePath);
        }

        public bool ExistsResourcesOrLocal(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            return ExistsResourcesOrLocal(path, multiVersionControlRelativePath, typeof(UnityEngine.Object));
        }

        public bool ExistsResourcesOrLocal<T>(string path, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString) where T : UnityEngine.Object
        {
            return ExistsResourcesOrLocal(path, multiVersionControlRelativePath, typeof(T));
        }

        public bool ExistsResourcesOrLocal(string path, System.Type type, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString)
        {
            return ExistsResourcesOrLocal(path, multiVersionControlRelativePath, type);
        }
    }
}
