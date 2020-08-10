using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class ObjectPool : shaco.Base.IObjectPool
    {
        //内存池堆栈信息临时开关，以防止在某些地方过多调用堆栈信息获取导致cpu占用过高的情况
        public bool isOpendStackLocation { get; set; }

        //已经实例化的对象数量
        public int instantiatedCount { get { return _instantiateObjectToKeys.Count; } }

        //尚未实例化使用的对象数量
        public int unsuedCount { get { return _unUsedObjectToKeys.Count; } }

        private Dictionary<string, List<PoolDataInfo>> _objectsUnusedPool = new Dictionary<string, List<PoolDataInfo>>();
        private Dictionary<string, List<PoolDataInfo>> _objectsInstantiatePool = new Dictionary<string, List<PoolDataInfo>>();
        private Dictionary<object, string> _instantiateObjectToKeys = new Dictionary<object, string>();
        private Dictionary<object, string> _unUsedObjectToKeys = new Dictionary<object, string>();

        private System.Threading.Mutex _mutex = new System.Threading.Mutex();
        private PoolDataInfo _invalidPoolDataInfo = new PoolDataInfo();
        private List<PoolDataInfo> _removePoolDataResult = new List<PoolDataInfo>();

        //遍历内存缓存池
        public void ForeacUnusedPool(System.Func<string, List<PoolDataInfo>, bool> callback)
        {
            lock (_mutex)
            {
                bool result = true;
                foreach (var item in _objectsUnusedPool)
                {
                    try
                    {
                        result = callback(item.Key, item.Value);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("ObjectPool ForeacUnusedPool error: callback=" + callback + " e=" + e);
                        result = false;
                    }
                    if (!result)
                        break;
                }
            }
        }

        //遍历内存实例化池
        public void ForeachInstantiatePool(System.Func<string, List<PoolDataInfo>, bool> callback)
        {
            lock (_mutex)
            {
                bool result = true;
                foreach (var item in _objectsInstantiatePool)
                {
                    try
                    {
                        result = callback(item.Key, item.Value);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("ObjectPool ForeacUnusedPool error: callback=" + callback + " e=" + e);
                        result = false;
                    }
                    if (!result)
                        break;
                }
            }
        }

        //清理没有使用的缓存对象
        public void UnloadUnusedPoolData()
        {
            lock (_mutex)
            {
                foreach (var iter in _objectsUnusedPool)
                {
                    foreach (var obj in iter.Value)
                        GetObjectSpawn().DestroyObject(obj.value);
                }
                _objectsUnusedPool.Clear();
                _unUsedObjectToKeys.Clear();
            }
            System.GC.Collect();
        }

        //从对象池中实例化一个对象，默认使用T类型名字作为key
        public T Instantiate<T>(System.Func<T> callbackCreate)
        {
            return Instantiate(null, callbackCreate);
        }

        //从对象池中实例化一个对象
        public T Instantiate<T>(string key, System.Func<T> callbackCreate)
        {
            lock (_mutex)
            {
                T retValue = default(T);
                InstantiateBase<T>(key,
                    (callback) =>
                    {
                        var newValue = GetObjectSpawn().CreateNewObject<T>(callbackCreate);
                        try
                        {
                            callback(newValue);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("ObjectPool Instantiate error: key=" + key + " create=" + callbackCreate + " e=" + e);
                        }
                    },
                    (value) =>
                    {
                        retValue = value;
                    });
                return retValue;
            }
        }

        //从对象池中异步实例化一个对象，默认使用T类型名字作为key
        public void InstantiateAsync<T>(System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd)
        {
            InstantiateAsync(null, callbackCreate, callbackEnd);
        }

        //从对象池中异步实例化一个对象
        public void InstantiateAsync<T>(string key, System.Action<System.Action<T>> callbackCreate, System.Action<T> callbackEnd)
        {
            lock (_mutex)
            {
                InstantiateBase<T>(key,
                    (callback) =>
                    {
                        GetObjectSpawn().CreateNewObjectAsync<T>(callbackCreate, (newValue) =>
                        {
                            try
                            {
                                callback(newValue);
                            }
                            catch (System.Exception e)
                            {
                                Log.Error("ObjectPool InstantiateAsync error: 1 key=" + key + " create=" + callbackCreate + " callbackEnd=" + callbackEnd + " e=" + e);
                            }
                        });
                    },
                    (value) =>
                    {
                        try
                        {
                            callbackEnd(value);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("ObjectPool InstantiateAsync error: 2 key=" + key + " create=" + callbackCreate + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    });
            }
        }

        //从对象池中异步实例化一个对象，默认使用T类型名字作为key
        public IEnumeratorRequest<T> InstantiateCoroutine<T>(System.Action<System.Action<T>> callbackCreate)
        {
            return InstantiateCoroutine<T>(null, callbackCreate);
        }

        //从对象池中异步实例化一个对象
        public IEnumeratorRequest<T> InstantiateCoroutine<T>(string key, System.Action<System.Action<T>> callbackCreate)
        {
            var retValue = new EnumeratorRequest<T>();
            GameHelper.objectpool.InstantiateAsync(callbackCreate, (obj) =>
            {
                retValue.SetResult(obj);
            });
            return retValue;
        }

        //预加载一个对象，并自动放到未使用缓存池中
        public T[] PreLoad<T>(string key, int preloadCount) where T : new()
        {
            var retValue = new T[preloadCount];
            for (int i = 0; i < preloadCount; ++i)
            {
                var instantiateTmp = Instantiate(key, () => new T());
                RecyclingObject(instantiateTmp);
                retValue[i] = instantiateTmp;
            }
            return retValue;
        }

        //预加载一个对象，并自动放到未使用缓存池中
        public T[] PreLoad<T>(string key, System.Func<T> callbackCreate, int preloadCount)
        {
            var retValue = new T[preloadCount];
            for (int i = 0; i < preloadCount; ++i)
            {
                var instantiateTmp = Instantiate<T>(key, callbackCreate);
                RecyclingObject(instantiateTmp);
                retValue[i] = instantiateTmp;
            }
            return retValue;
        }

        /// <summary>
        /// 回收key中所有内存对象信息
        /// <param name="key">对象key值</param>
        /// <return>已经回收的对象组</return>
        /// </summary>
        public void RecyclingAllObjects(string key)
        {
            lock (_mutex)
            {
                var poolDatas = RemoveObjectFromPool(_objectsInstantiatePool, key, -1);
                RemoveObjectToKeyReference(_instantiateObjectToKeys, poolDatas);

                if (!poolDatas.IsNullOrEmpty())
                {
                    AddObjectToPool(_objectsUnusedPool, key, poolDatas);
                    UpdateObjectToKeyReference(_unUsedObjectToKeys, key, poolDatas);
                    for (int i = 0; i < poolDatas.Length; ++i)
                    {
                        if (isOpendStackLocation)
                            poolDatas[i].stackLocationRecycling.StartTimeSpanCalculate();
                        GetObjectSpawn().RecyclingObject(poolDatas[i].value);
                        CheckObjectPoolDataDispose(poolDatas[i].value);

                        if (isOpendStackLocation)
                        {
                            poolDatas[i].stackLocationRecycling.GetStack();
                            poolDatas[i].stackLocationRecycling.StopTimeSpanCalculate();
                        }
                    }
                }
                else
                    Log.Error("ObjectPool RecyclingAllObjectsBase error: not found object in instantiate pool, key=" + key);
            }
        }

        /// <summary>
        /// 回收单个对象
        /// <param name="obj">对象</param>
        /// <return>已经回收的对象</return>
        /// </summary>
        public object RecyclingObject(object obj)
        {
            lock (_mutex)
            {
                var key = ObjectToKey(obj);
                var retValue = RemoveObjectFromPool(_objectsInstantiatePool, key, obj);
                RemoveObjectToKeyReference(_instantiateObjectToKeys, retValue);

                if (null != retValue.value)
                {
                    if (isOpendStackLocation)
                        retValue.stackLocationRecycling.StartTimeSpanCalculate();
                    AddObjectToPool(_objectsUnusedPool, key, retValue);
                    UpdateObjectToKeyReference(_unUsedObjectToKeys, key, retValue);
                    GetObjectSpawn().RecyclingObject(obj);
                    CheckObjectPoolDataDispose(obj);

                    if (isOpendStackLocation)
                    {
                        retValue.stackLocationRecycling.GetStack();
                        retValue.stackLocationRecycling.StopTimeSpanCalculate();
                    }
                }
                else
                    Log.Error("ObjectPool RecyclingObjectBase error: not found object in instantiate pool, obj=" + obj + " key=" + key);
            }
            return obj;
        }

        /// <summary>
        /// 销毁key中所有对象
        /// <param name="key">对象key值</param>
        /// </summary>
        public void DestroyAllObjects(string key)
        {
            lock (_mutex)
            {
                var poolDatasWithInstantiate = RemoveObjectFromPool(_objectsInstantiatePool, key, -1);
                var poolDatasWithUnused = RemoveObjectFromPool(_objectsUnusedPool, key, -1);

                RemoveObjectToKeyReference(_instantiateObjectToKeys, poolDatasWithInstantiate);
                RemoveObjectToKeyReference(_unUsedObjectToKeys, poolDatasWithUnused);

                if (!poolDatasWithInstantiate.IsNullOrEmpty())
                {
                    for (int i = 0; i < poolDatasWithInstantiate.Length; ++i)
                    {
                        CheckObjectPoolDataDispose(poolDatasWithInstantiate[i].value);
                        GetObjectSpawn().DestroyObject(poolDatasWithInstantiate[i].value);
                    }
                }
                else if (poolDatasWithUnused.IsNullOrEmpty())
                    Log.Error("BaseObjectPool DestroyAllObjectsBase error: not found object in instantiate pool, key=" + key);

                if (!poolDatasWithUnused.IsNullOrEmpty())
                {
                    for (int i = 0; i < poolDatasWithUnused.Length; ++i)
                    {
                        CheckObjectPoolDataDispose(poolDatasWithUnused[i].value);
                        GetObjectSpawn().DestroyObject(poolDatasWithUnused[i].value);
                    }
                }
            }
        }

        public object DestroyObject(object obj)
        {
            lock (_mutex)
            {
                var key = ObjectToKey(obj);
                var instantiateInfo = RemoveObjectFromPool(_objectsInstantiatePool, key, obj);
                var unusedInfo = RemoveObjectFromPool(_objectsUnusedPool, key, obj);

                var objsWithInstantiate = instantiateInfo.value;
                var objsWithUnused = unusedInfo.value;

                RemoveObjectToKeyReference(_instantiateObjectToKeys, instantiateInfo);
                RemoveObjectToKeyReference(_unUsedObjectToKeys, unusedInfo);

                if (null == objsWithUnused && null == objsWithInstantiate)
                    Log.Error("ObjectPool DestroyObjectBase error: not found object in instantiate pool, obj=" + obj + " key=" + key);
                else
                {
                    CheckObjectPoolDataDispose(obj);
                }

                if (null != objsWithInstantiate)
                    GetObjectSpawn().DestroyObject(objsWithInstantiate);
                if (null != objsWithUnused)
                    GetObjectSpawn().DestroyObject(objsWithUnused);
            }
            return obj;
        }

        public void Clear()
        {
            lock (_mutex)
            {
                UnloadUnusedPoolData();

                foreach (var iter in _objectsInstantiatePool)
                {
                    foreach (var obj in iter.Value)
                        GetObjectSpawn().DestroyObject(obj.value);
                }

                _objectsInstantiatePool.Clear();
                _instantiateObjectToKeys.Clear();
            }
            System.GC.Collect();
        }

        //判断对象组是否为没有使用状态
        public bool isUnused(string key)
        {
            return _objectsUnusedPool.ContainsKey(key);
        }

        //判断对象是否为没有使用状态
        public bool isUnused(object obj)
        {
            return _unUsedObjectToKeys.ContainsKey(obj);
        }

        //判断对象组是否从内存池实例化出来了
        public bool IsInstantiated(string key)
        {
            return _objectsInstantiatePool.ContainsKey(key);
        }

        /// 判断对象是否从内存池实例化出来了
        public bool IsInstantiated(object obj)
        {
            return _instantiateObjectToKeys.ContainsKey(obj);
        }

        //判断对象是否在缓存池内
        public bool IsInPool(string key)
        {
            return IsInstantiated(key) || isUnused(key);
        }

        //判断对象是否在缓存池内
        public bool IsInPool(object obj)
        {
            return IsInstantiated(obj) || isUnused(obj);
        }

        //获取没有使用的缓存对象数量
        public int GetUnusedCount(string key)
        {
            List<PoolDataInfo> findValue = null;
            if (_objectsUnusedPool.TryGetValue(key, out findValue))
                return findValue.Count;
            else
                return 0;
        }

        //获取已经实例化出来的对象数量
        public int GetInstantiatedCount(string key)
        {
            List<PoolDataInfo> findValue = null;
            if (_objectsUnusedPool.TryGetValue(key, out findValue))
                return findValue.Count;
            else
                return 0;
        }

        /// <summary>
        /// 检查内存对象销毁方法并自动调用
        /// </summary>
        private void CheckObjectPoolDataDispose(object obj)
        {
            var convertObj = obj as IObjectPoolData;
            if (null != convertObj)
                convertObj.Dispose();
            // else
            //     Log.Warning("ObjectPool CheckObjectPoolDataDispose error: not inherit from 'IObjectPoolData', obj=" + obj + " type=" + obj.ToTypeString());
        }

        private PoolDataInfo[] AddObjectToPool(Dictionary<string, List<PoolDataInfo>> objectsPool, string key, params PoolDataInfo[] poolDatas)
        {
            lock (_mutex)
            {
                List<PoolDataInfo> objects = null;
                if (!objectsPool.ContainsKey(key))
                {
                    objectsPool.Add(key, new List<PoolDataInfo>());
                }
                objects = objectsPool[key];

                for (int i = poolDatas.Length - 1; i >= 0; --i)
                {
                    objects.Add(poolDatas[i]);
                }
                return objects.ToArray();
            }
        }

        private void RemoveObjectToKeyReference(Dictionary<object, string> targetDic, params PoolDataInfo[] removePoolDataInfo)
        {
            if (removePoolDataInfo.IsNullOrEmpty())
                return;

            for (int i = removePoolDataInfo.Length - 1; i >= 0; --i)
            {
                var infoTmp = removePoolDataInfo[i];
                if (null != infoTmp.value && targetDic.ContainsKey(infoTmp.value))
                    targetDic.Remove(infoTmp.value);
            }
        }

        private void UpdateObjectToKeyReference(Dictionary<object, string> targetDic, string key, params PoolDataInfo[] newPoolDataInfo)
        {
            if (newPoolDataInfo.IsNullOrEmpty())
                return;

            for (int i = newPoolDataInfo.Length - 1; i >= 0; --i)
            {
                var infoTmp = newPoolDataInfo[i];
                targetDic[infoTmp.value] = key;
            }
        }

        private PoolDataInfo[] RemoveObjectFromPool(Dictionary<string, List<PoolDataInfo>> objectsPool, string key, int removeCount)
        {
            lock (_mutex)
            {
                List<PoolDataInfo> poolDatas = null;
                _removePoolDataResult.Clear();
                if (null != key && objectsPool.TryGetValue(key, out poolDatas))
                {
                    if (!poolDatas.IsNullOrEmpty())
                    {
                        //should remove all
                        if (removeCount < 0)
                        {
                            removeCount = poolDatas.Count;
                        }

                        int removeEndIndex = poolDatas.Count - 1;
                        int removeStartIndex = poolDatas.Count - removeCount;

                        for (int i = removeEndIndex; i >= removeStartIndex; --i)
                        {
                            var removeObjTmp = poolDatas[i];
                            _removePoolDataResult.Add(removeObjTmp);
                            poolDatas.RemoveAt(i);
                        }
                    }

                    if (poolDatas.IsNullOrEmpty())
                    {
                        objectsPool.Remove(key);
                    }
                }
                return _removePoolDataResult.ToArray();
            }
        }

        private PoolDataInfo RemoveObjectFromPool<T>(Dictionary<string, List<PoolDataInfo>> objectsPool, string key, T obj)
        {
            lock (_mutex)
            {
                PoolDataInfo retValue = _invalidPoolDataInfo;
                List<PoolDataInfo> poolDatas = null;
                if (null != key && objectsPool.TryGetValue(key, out poolDatas))
                {
                    for (int i = poolDatas.Count - 1; i >= 0; --i)
                    {
                        var removeObjTmp = poolDatas[i];
                        if (removeObjTmp.value == (object)obj)
                        {
                            retValue = removeObjTmp;
                            poolDatas.RemoveAt(i);
                            break;
                        }
                    }
                    if (poolDatas.IsNullOrEmpty())
                    {
                        objectsPool.Remove(key);
                    }
                }
                return retValue;
            }
        }

        private void InstantiateBase<T>(string key, System.Action<System.Action<T>> createFunc, System.Action<T> callbackEnd)
        {
            lock (_mutex)
            {
                PoolDataInfo instantiatePoolData = _invalidPoolDataInfo;

                if (null == key)
                    key = typeof(T).FullName;

                if (!_objectsUnusedPool.ContainsKey(key))
                {
                    instantiatePoolData = new PoolDataInfo();
                    if (isOpendStackLocation)
                    {
                        instantiatePoolData.stackLocationInstantiate = new StackLocation();
                        instantiatePoolData.stackLocationRecycling = new StackLocation();
                        instantiatePoolData.stackLocationInstantiate.StartTimeSpanCalculate();
                    }

                    createFunc(v =>
                    {
                        if (null == v)
                        {
                            Log.Error("ObjectPool InstantiateBase error: create value is null, type=" + typeof(T).FullName + " key=" + key);
                            try
                            {
                                callbackEnd(v);
                            }
                            catch (System.Exception e)
                            {
                                Log.Error("ObjectPool InstantiateBase error: 1 key=" + key + " create=" + createFunc + " callbackEnd=" + callbackEnd + " e=" + e);
                            }
                            return;
                        }
                        instantiatePoolData.value = v;

                        InstantiateBaseEnd(key, instantiatePoolData);

                        try
                        {
                            callbackEnd(v);
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("ObjectPool InstantiateBase error: 2 key=" + key + " create=" + createFunc + " callbackEnd=" + callbackEnd + " e=" + e);
                        }
                    });
                }
                else
                {
                    T retValue = default(T);
                    var objectsList = _objectsUnusedPool[key];
                    instantiatePoolData = objectsList[objectsList.Count - 1];

                    if (isOpendStackLocation)
                        instantiatePoolData.stackLocationInstantiate.StartTimeSpanCalculate();
                    retValue = (T)(object)(instantiatePoolData.value);
                    var removedPoolData = RemoveObjectFromPool(_objectsUnusedPool, key, 1);
                    RemoveObjectToKeyReference(_unUsedObjectToKeys, removedPoolData);
                    GetObjectSpawn().ActiveObject(retValue);

                    InstantiateBaseEnd(key, instantiatePoolData);

                    try
                    {
                        callbackEnd(retValue);
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("ObjectPool InstantiateBase error: 3 key=" + key + " create=" + createFunc + " callbackEnd=" + callbackEnd + " e=" + e);
                    }
                }
            }
        }

        private void InstantiateBaseEnd(string key, PoolDataInfo instantiatePoolData)
        {
            AddObjectToPool(_objectsInstantiatePool, key, instantiatePoolData);
            UpdateObjectToKeyReference(_instantiateObjectToKeys, key, instantiatePoolData);

            if (isOpendStackLocation)
            {
                instantiatePoolData.stackLocationInstantiate.GetStack();
                instantiatePoolData.stackLocationInstantiate.StopTimeSpanCalculate();
            }
        }

        private string ObjectToKey(object obj)
        {
            lock (_mutex)
            {
                if (null == obj)
                {
                    Log.Error("ObjectPool ObjectToKey error: object is null");
                    return string.Empty;
                }
                else
                {
                    var retValue = string.Empty;
                    if (!_instantiateObjectToKeys.TryGetValue(obj, out retValue))
                    {
                        if (!_unUsedObjectToKeys.TryGetValue(obj, out retValue))
                            Log.Error("ObjectPool ObjectToKey error: not found key by object=" + obj + " in pool=" + IsInPool(obj));
                    }
                    return retValue;
                }
            }
        }

        //获取对象生成器
        private shaco.Base.IObjectSpawn GetObjectSpawn()
        {
            return shaco.Base.GameHelper.objectpoolSpawn;
        }
    }
}