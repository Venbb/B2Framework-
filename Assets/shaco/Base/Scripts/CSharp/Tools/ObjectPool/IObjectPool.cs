using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class PoolDataInfo
    {
        //内存对象
        public object value;
        //调用实例化接口堆栈信息
        public StackLocation stackLocationInstantiate = new StackLocation();
        //调用回收接口堆栈信息
        public StackLocation stackLocationRecycling = new StackLocation();
    }

    public interface IObjectPool : IGameInstance
    {
        //内存池堆栈信息临时开关，以防止在某些地方过多调用堆栈信息获取导致cpu占用过高的情况
        bool isOpendStackLocation { get; set; }
        
        //已经实例化的对象数量
        int instantiatedCount { get; }

        //尚未实例化使用的对象数量
        int unsuedCount { get; }


        //从对象池中实例化一个对象，默认使用T类型名字作为key
        T Instantiate<T>(System.Func<T> callbackCreate);

        //从对象池中实例化一个对象
        T Instantiate<T>(string key, System.Func<T> callbackCreate);

        //从对象池中异步实例化一个对象，默认使用T类型名字作为key
        void InstantiateAsync<T>(System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd);

        //从对象池中异步实例化一个对象
        void InstantiateAsync<T>(string key, System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd);

        //从对象池中异步实例化一个对象，默认使用T类型名字作为key
        IEnumeratorRequest<T> InstantiateCoroutine<T>(System.Action<System.Action<T>> callbackCreate);

        //从对象池中异步实例化一个对象
        IEnumeratorRequest<T> InstantiateCoroutine<T>(string key, System.Action<System.Action<T>> callbackCreate);

        //预加载一个对象，并自动放到未使用缓存池中
        T[] PreLoad<T>(string key, int preloadCount) where T : new();

        //预加载一个对象，并自动放到未使用缓存池中
        T[] PreLoad<T>(string key, System.Func<T> callbackCreate, int preloadCount);

        //回收对象以便再次利用
        object RecyclingObject(object obj);

        //回收所有对象以便再次利用
        void RecyclingAllObjects(string key);

        //销毁对象以便再次利用
        object DestroyObject(object obj);

        //销毁所有对象，包含实例化和未使用的
        void DestroyAllObjects(string key);

        //判断对象组是否为没有使用状态
        bool isUnused(string key);

        //判断对象是否为没有使用状态
        bool isUnused(object obj);

        //判断对象组是否从内存池实例化出来了
        bool IsInstantiated(string key);

        /// 判断对象是否从内存池实例化出来了
        bool IsInstantiated(object obj);

        //判断对象是否在缓存池内
        bool IsInPool(string key);

        //判断对象是否在缓存池内
        bool IsInPool(object obj);

        //获取没有使用的缓存对象数量
        int GetUnusedCount(string key);

        //获取已经实例化出来的对象数量
        int GetInstantiatedCount(string key);

        //清空所有缓存对象
        void Clear();

        //销毁没有使用的内存对象
        void UnloadUnusedPoolData();

        //遍历内存缓存池
        void ForeacUnusedPool(System.Func<string, List<shaco.Base.PoolDataInfo>, bool> callback);
        
        //遍历内存实例化池
        void ForeachInstantiatePool(System.Func<string, List<shaco.Base.PoolDataInfo>, bool> callback);
    }
}