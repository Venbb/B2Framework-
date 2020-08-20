using System;
using UnityEngine;

namespace B2Framework
{
    public abstract class Singleton<T> where T : class, new()//使用关键字 new() 限定，必须含有无参构造函数的单例 
    {
        // 用于lock块的对象,使用 双重锁确保单例在多线程初始化时的线程安全性
        private static readonly object _synclock = new object();
        private static T m_instance;
        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (_synclock)
                    {
                        m_instance = Activator.CreateInstance<T>();//new T();
                    }
                }
                return m_instance;
            }
        }
        internal virtual void Update(float deltaTime, float unscaledDeltaTime) { }
        public virtual void Dispose()
        {
            m_instance = null;
        }
    }
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T m_instance;
        protected static bool _destroyed = false;
        public static T Instance
        {
            get
            {
                if (m_instance == null && !_destroyed)
                {
                    m_instance = Create();
                }
                return m_instance;
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
            if (m_instance != null)
            {
                if (m_instance != this) DestroyImmediate(gameObject);
            }
            else
            {
                m_instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
        }
        protected virtual void OnDestroy()
        {
            _destroyed = true;
        }
        protected virtual void OnApplicationQuit()
        {
            if (m_instance == null) return;
            DestroyImmediate(m_instance.gameObject);
            m_instance = null;
        }
    }
}