using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public abstract class ISubjectBase
    {
        /// <summary>
        /// 移除所有观察对象
        /// </summary>
        abstract public void Clear();

        /// <summary>
        /// 通知所有观察对象数据有刷新
        /// </summary>
        abstract public void Notify(object oldValue, object newValue);

        /// <summary>
        /// 获取数据主体绑定对象
        /// <return>绑定对象</return>
        /// </summary>
        abstract public object GetBindTarget();

        /// <summary>
        /// 设置数据主体绑定对象，同一时间只能绑定一个数据主体
        /// <param name="bindTarget">绑定对象</param>
        /// </summary>
        abstract public void SetBindTarget(object bindTarget);
    }

    //仅仅作为堆栈日志查找占位用，无其他意义
    public class Subject { }

    /// <summary>
    /// 数据的主体，用于修改数据和通知观测者
    /// </summary>
    public abstract class ISubject<T> : ISubjectBase
    {
        /// <summary>
        /// 所有的观察对象
        /// </summary>
        abstract public System.Collections.ObjectModel.ReadOnlyCollection<IObserver<T>> observers { get; }

        /// <summary>
        /// 被观测的数据
        /// </summary>
        abstract public T value { get; set; }

        /// <summary>
        /// 添加数据观测对象，内部会保存一个value对象
        /// <param name="observer">数据观察者</param>
        /// </summary>
        abstract public IObserver<T> Add(IObserver<T> observer);

        /// <summary>
        /// 设置自定义的观测数据变化方式
        /// <param name="callbackGet">获取数据接口</param>
        /// <param name="callbackSet">设置数据接口</param>
        /// </summary>
        abstract public void LookAt(System.Func<T> callbackGet, System.Action<T> callbackSet);

        /// <summary>
        /// 移除数据观测对象
        /// <param name="observer">数据观察者</param>
        /// </summary>
        abstract public bool Remove(IObserver<T> observer);
    }
}

