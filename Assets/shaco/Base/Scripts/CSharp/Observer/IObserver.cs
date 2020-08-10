using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 数据观测者基础信息
    /// </summary>
    public abstract class IObserverBase
    {
        /// <summary>
        /// 数据初始化的方法
        /// </summary>
        abstract public void OnInitCallBack();

        /// <summary>
        /// 数据发生变化的刷新方法
        /// </summary>
        abstract public void OnUpdateCallBack();

        /// <summary>
        /// 数据被销毁的方法
        /// </summary>
        abstract public void OnDestroyCallBack();
    }

    /// <summary>
    /// 数据观测者，用于观测数据变化并作出逻辑动作
    /// </summary>
    public abstract class IObserver<T> : IObserverBase
    {
        /// <summary>
        /// 数据主体
        /// </summary>
        abstract public ISubject<T> subject { get; set; }

        /// <summary>
        /// 数据初始化的方法
        /// </summary>
        abstract public System.Action<ISubject<T>> callbackInit { get; set; }

        /// <summary>
        /// 数据发生变化的刷新方法
        /// </summary>
        abstract public System.Action<T, ISubject<T>> callbackUpdate { get; set; }

        /// <summary>
        /// 数据被销毁的方法
        /// </summary>
        abstract public System.Action<ISubject<T>> callbackDestroy { get; set; }
    }
}