using System;
using UnityEngine;

namespace B2Framework
{
    public abstract class Singleton<T> where T : class, new()//使用关键字 new() 限定，必须含有无参构造函数的单例 
    {
        // 用于lock块的对象,使用 双重锁确保单例在多线程初始化时的线程安全性
        private static readonly object _synclock = new object();
        private static T _instance;
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_synclock)
                    {
                        _instance = Activator.CreateInstance<T>();//new T();
                    }
                }
                return _instance;
            }
        }
        internal virtual void Update(float elapseSeconds, float realElapseSeconds) { }
        public virtual void Dispose()
        {
            _instance = null;
        }
    }
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static bool _destroyed = false;
        public static T Instance
        {
            get
            {
                if (_instance == null && !_destroyed)
                {
                    _instance = Create();
                }
                return _instance;
            }
        }
        private static T Create()
        {
            var go = new GameObject(typeof(T).Name, typeof(T));
            DontDestroyOnLoad(go);
            return go.GetComponent<T>();
        }
        protected virtual void Awake()
        {
            if (_instance != null)
            {
                if (_instance != this) DestroyImmediate(gameObject);
            }
            else
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
        }
        public abstract void Dispose();
        protected virtual void OnDestroy()
        {
            Dispose();
            _destroyed = true;
        }
        protected virtual void OnApplicationQuit()
        {
            if (_instance == null) return;
            DestroyImmediate(_instance.gameObject);
            _instance = null;
        }
    }
}