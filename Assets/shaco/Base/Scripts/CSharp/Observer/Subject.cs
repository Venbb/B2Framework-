using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 数据的主体，用于修改数据和通知观测者
    /// </summary>
    public class Subject<T> : ISubject<T>
    {
        /// <summary>
        /// 所有的观察对象
        /// </summary>
        override public System.Collections.ObjectModel.ReadOnlyCollection<IObserver<T>> observers { get { return _observers.AsReadOnly(); } }

        /// <summary>
        /// 被观测的数据
        /// </summary>
        override public T value
        {
            get
            {
                return null == callbackGet ? _value : callbackGet();
            }
            set
            {
                bool isEqualValue = false;
                if (null != value)
                {
                    isEqualValue = value.Equals(_value);
                }
                else if (null != _value)
                {
                    isEqualValue = _value.Equals(value);
                }
                else
                    isEqualValue = System.Object.Equals(_value, value);

                if (!isEqualValue)
                {
                    Notify(_value, value);
                }
            }
        }

        /// <summary>
        /// 被观测的数据实例
        /// </summary>
        private T _value = default(T);

        /// <summary>
        /// 观测数据回调方法
        /// </summary>
        private System.Func<T> callbackGet = null;
        private System.Action<T> callbackSet = null;

        /// <summary>
        /// 所有的观察对象实例
        /// </summary>
        private List<IObserver<T>> _observers = new List<IObserver<T>>();

        /// <summary>
        /// 数据主体绑定对象
        /// </summary>
        private object _bindTarget = "null";

        /// <summary>
        /// 绑定的变量
        /// </summary>
        private string _bindParamName = string.Empty;

        /// <summary>
        /// 添加数据观测对象，内部会保存一个value对象
        /// <param name="observer">数据观测对象</param>
        /// </summary>
        override public IObserver<T> Add(IObserver<T> observer)
        {
            if (null == observer)
            {
                Log.Error("Subject Add error: observer is null");
                return observer;
            }

            if (_observers.Contains(observer))
            {
                Log.Error("Subject Add erorr: has duplicate observer=" + observer.GetType().FullName);
                return observer;
            }

            _observers.Add(observer);
            GameHelper.observer.AddObserver(this, observer);
            observer.subject = this;

            GameHelper.observer.WillValueInit(this, observer);
            {
                if (null != observer.callbackInit)
                {
                    observer.callbackInit(this);
                }
                observer.OnInitCallBack();
            }
            GameHelper.observer.ValueInited(this, observer);

            //强制触发初始化的update
            Notify(this.value, this.value);
            return observer;
        }

        /// <summary>
        /// 设置自定义的观测数据变化方式
        /// <param name="callbackGet">获取数据接口</param>
        /// <param name="callbackSet">设置数据接口</param>
        /// </summary>
        override public void LookAt(System.Func<T> callbackGet, System.Action<T> callbackSet)
        {
            this.callbackGet = callbackGet;
            this.callbackSet = callbackSet;
        }

        /// <summary>
        /// 移除数据观测对象
        /// <param name="observer">数据观测对象</param>
        /// </summary>
        override public bool Remove(IObserver<T> observer)
        {
            int findIndex = -1;
            for (int i = _observers.Count - 1; i >= 0; --i)
            {
                if (_observers[i] == observer)
                {
                    findIndex = i;
                    break;
                }
            }

            if (findIndex < 0)
            {
                Log.Error("Subject Remove error: not found observer=" + observer.GetType().FullName);
                return false;
            }
            else
            {
                if (null != observer.callbackDestroy)
                {
                    observer.callbackDestroy(this);
                }
                observer.OnDestroyCallBack();
                _observers.RemoveAt(findIndex);
                GameHelper.observer.RemoveObserver(this, observer);
                return true;
            }
        }

        /// <summary>
        /// 移除所有观察对象
        /// </summary>
        override public void Clear()
        {
            _observers.Clear();
            GameHelper.observer.RemoveSubject(this);
        }

        /// <summary>
        /// 通知所有观察对象数据有刷新
        /// </summary>
        override public void Notify(object oldValue, object newValue)
        {
            if (_observers.Count == 0)
            {
                Log.Error("Subject Notify error: observer is empty, value=" + value);
            }

            //如果设置了其他观察数据回调，则不用保存数据实例
            if (null != callbackSet)
                callbackSet((T)newValue);
            else
                _value = (T)newValue;

            for (int i = 0; i < _observers.Count; ++i)
            {
                var observerTmp = _observers[i];
                if (null != observerTmp)
                {
                    GameHelper.observer.WillValueUpdate(this, observerTmp);
                    {
                        if (null != observerTmp.callbackUpdate)
                        {
                            observerTmp.callbackUpdate((T)oldValue, this);
                        }
                        observerTmp.OnUpdateCallBack();
                    }
                    GameHelper.observer.ValueUpdated(this, observerTmp);
                }
            }
        }

        /// <summary>
        /// 获取数据主体绑定对象
        /// <return>绑定对象</return>
        /// </summary>
        override public object GetBindTarget()
        {
            return _bindTarget;
        }

        /// <summary>
        /// 设置数据主体绑定对象，同一时间只能绑定一个数据主体
        /// <param name="bindTarget">绑定对象</param>
        /// </summary>
        override public void SetBindTarget(object bindTarget)
        {
            if (null == bindTarget)
            {
                Log.Error("Subect SetBindTarge error: bindTarget is null");
                return;
            }

            if (_bindTarget == bindTarget)
                return;

            _bindTarget = bindTarget;
        }
    }
}