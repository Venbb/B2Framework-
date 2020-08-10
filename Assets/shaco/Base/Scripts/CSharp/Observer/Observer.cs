using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class Observer<T> : IObserver<T>
    {
        /// <summary>
        /// 数据主体
        /// </summary>
        override public ISubject<T> subject { get; set; }

        /// <summary>
        /// 数据初始化的方法
        /// </summary>
        override public System.Action<ISubject<T>> callbackInit { get; set; }
        override public void OnInitCallBack() { }

        /// <summary>
        /// 数据发生变化的刷新方法
        /// </summary>
        override public System.Action<T, ISubject<T>> callbackUpdate { get; set; }
        override public void OnUpdateCallBack() { }

        /// <summary>
        /// 数据被销毁的方法
        /// </summary>
        override public System.Action<ISubject<T>> callbackDestroy { get; set; }
        override public void OnDestroyCallBack() { }
    }
}

