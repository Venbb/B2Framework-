using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public class DataCache
    {
        //是否为Resources目录资源
        public bool isResourcesAsset = false;
        //是否正在加载
        public bool isLoading = false;
        public HotUpdateImportMemory hotUpdateDelMemory = new HotUpdateImportMemory();
        // public List<System.Action<DataCache>> listCallBackReadEnd = new List<System.Action<DataCache>>();

        //资源版本号
        public string multiVersionControlRelativePath = string.Empty;

        //创建ab缓存的堆栈信息
        public shaco.Base.StackLocation stackLocationCreate = new shaco.Base.StackLocation();
        //读取ab缓存数据的堆栈信息
        public shaco.Base.StackLocation stackLocationRead = new shaco.Base.StackLocation();
        //已经读取过的资源对象列表，用于查找引用，最大保存上限HotUpdateDefine.MAX_SAVE_READED_ASSET_COUNT
        public List<UnityEngine.Object> readedAssets = new List<UnityEngine.Object>();
        public int referenceCount = 0;
    }

    public interface IHotUpdateDataCache : shaco.Base.IGameInstance
    {
        int loadedCount { get; }

        Object Read(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string fileName);

        T Read<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string fileName) where T : UnityEngine.Object;

        Object Read(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string fileName, System.Type type);

        Object[] ReadAll(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type);

        Object[] ReadAll(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies);

        Object[] ReadAll<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies) where T : UnityEngine.Object;

        void ReadAsync<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string filename, System.Action<T> callbackReadEnd, System.Action<float> callbackProgress) where T : UnityEngine.Object;

        void ReadAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string filename, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress);

        void ReadAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, string filename, System.Type type, System.Action<UnityEngine.Object> callbackReadEnd, System.Action<float> callbackProgress);

        void ReadAllAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Type type, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress);
        void ReadAllAsync<T>(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress) where T : UnityEngine.Object;
        void ReadAllAsync(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies, System.Action<UnityEngine.Object[]> callbackReadEnd, System.Action<float> callbackProgress);

        void UnloadUnusedDatas(bool unloadAllLoadedObjects = false);

        bool UnloadAssetBundle(string pathAssetBundle, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 根据ab包名字标记批量卸载有关联的ab包
        /// <param name="nameFlag">ab包名标记</param>
        /// <param name="unloadAllLoadedObjects">是否清理ab包所有相关资源，为false则只清理ab包的引用</param>
        /// <param name="multiVersionControlRelativePath">资源版本相对路径，用于多资源版本管理，如果填空默认为全局资源</param>
        /// <return>清理的ab包数量</return>
        /// </summary>
        int UnloadAssetBundlesWithNameFlag(string nameFlag, bool unloadAllLoadedObjects, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 手动添加资源读取引用计数，本质上该方法已经在读取时候调用过了，不用再调用
        /// </summary>
        void AddReferenceCount(string pathAssetBundle, string multiVersionControlRelativePath, bool isResourcesAsset);

        bool IsLoadedAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        bool IsLoadingAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        string GetFullAssetBundleKey(string pathAssetBundle, string multiVersionControlRelativePath);

        /// <summary>
        /// 保持在使用状态，防止UnloadUnusedDatas时候被卸载掉
        /// </summary>
        bool RetainStart(string pathAssetBundle, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 关闭在使用状态，一般和RetainStart对应 
        /// </summary>
        bool RetainEnd(string pathAssetBundle, string multiVersionControlRelativePath = shaco.Base.GlobalParams.EmptyString);

        /// <summary>
        /// 获取所有缓存中的assetbundle信息
        /// </summary>
        Dictionary<string, DataCache> GetAllCachedAssetbundle();

        /// <summary>
        /// 同步读取一个assetbundle
        /// </summary>
        bool LoadAssetBundle(string pathAssetBundle, string multiVersionControlRelativePath, bool autoCheckDepencies);

        /// <summary>
        /// 异步读取一个assetbundle
        /// </summary>
        void LoadAssetBundleAsync(string pathAssetBundle, System.Action<bool> callbackLoadEnd, System.Action<float> callbackProgress, string multiVersionControlRelativePath, bool autoCheckDepencies);
    }
}