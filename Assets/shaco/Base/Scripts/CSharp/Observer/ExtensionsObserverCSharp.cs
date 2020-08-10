using System.Collections;
using System.Collections.Generic;

static public class shaco_ExtensionsObserverCSharp
{
    /// <summary>
    /// 监听数据发生变化
    /// <param name="subject">数据主体</param>
    /// <param name="callbackUpdate">数据发生变化时的回调方法</param>
    /// <return>数据主体</return>
    /// </summary>
    static public shaco.Base.IObserver<object> OnSubjectValueUpdate(this shaco.Base.ISubject<object> subject, System.Action<object, shaco.Base.ISubject<object>> callbackUpdate)
    {
        return OnSubjectValueUpdate<object>(subject, callbackUpdate);
    }
    static public shaco.Base.IObserver<T> OnSubjectValueUpdate<T>(this shaco.Base.ISubject<T> subject, System.Action<T, shaco.Base.ISubject<T>> callbackUpdate)
    {
        var retValue = new shaco.Base.Observer<T>()
        {
            callbackUpdate = callbackUpdate
        };
        retValue.subject = subject;
        return retValue;
    }

    /// <summary>
    /// 监听数据发生变化
    /// <param name="subject">数据主体</param>
    /// <param name="callbackUpdate">数据发生变化时的回调方法</param>
    /// <return>数据主体</return>
    /// </summary>
    static public shaco.Base.IObserver<object> OnSubjectValueUpdate(this shaco.Base.ISubject<object> subject, System.Action<shaco.Base.ISubject<object>> callbackUpdate)
    {
        return OnSubjectValueUpdate(subject, callbackUpdate);
    }
    static public shaco.Base.IObserver<T> OnSubjectValueUpdate<T>(this shaco.Base.ISubject<T> subject, System.Action<shaco.Base.ISubject<T>> callbackUpdate)
    {
        return subject.OnSubjectValueUpdate((oldValue, newValue) => callbackUpdate(newValue));
    }

    /// <summary>
    /// 监听数据发生变化
    /// <param name="subject">数据主体</param>
    /// <param name="callbackUpdate">数据发生变化时的回调方法</param>
    /// <return>数据主体</return>
    /// </summary>
    static public shaco.Base.IObserver<object> OnValueUpdateFromTo(this shaco.Base.ISubject<object> subject, System.Action<object, object> callbackUpdate)
    {
        return OnValueUpdateFromTo<object>(subject, callbackUpdate);
    }
    static public shaco.Base.IObserver<T> OnValueUpdateFromTo<T>(this shaco.Base.ISubject<T> subject, System.Action<T, T> callbackUpdate)
    {
        var retValue = new shaco.Base.Observer<T>()
        {
            callbackUpdate = (T oldValue, shaco.Base.ISubject<T> subjectTmp) =>
            {
                callbackUpdate(oldValue, subjectTmp.value);
            }
        };
        retValue.subject = subject;
        return retValue;
    }

    /// <summary>
    /// 监听数据发生变化
    /// <param name="subject">数据主体</param>
    /// <param name="callbackUpdate">数据发生变化时的回调方法</param>
    /// <return>数据主体</return>
    /// </summary>
    static public shaco.Base.IObserver<object> OnValueUpdate(this shaco.Base.ISubject<object> subject, System.Action<object> callbackUpdate)
    {
        return OnValueUpdate<object>(subject, callbackUpdate);
    }
    static public shaco.Base.IObserver<T> OnValueUpdate<T>(this shaco.Base.ISubject<T> subject, System.Action<T> callbackUpdate)
    {
        return subject.OnValueUpdateFromTo((oldValue, newValue) => callbackUpdate(newValue));
    }

    /// <summary>
    /// 监听数据初始化
    /// <param name="observer">观察者</param>
    /// <param name="callbackInit">数据初始化的时候调用</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<object> OnValueInit(this shaco.Base.IObserver<object> observer, System.Action<object> callbackInit)
    {
        return OnValueInit<object>(observer, callbackInit);
    }
    static public shaco.Base.IObserver<T> OnValueInit<T>(this shaco.Base.IObserver<T> observer, System.Action<T> callbackInit)
    {
        observer.callbackInit = (shaco.Base.ISubject<T> subjectTmp) =>
        {
            callbackInit(subjectTmp.value);
        };
        return observer;
    }

    /// <summary>
    /// 监听数据被销毁
    /// <param name="observer">观察者</param>
    /// <param name="callbackDestroy">数据被销毁时的回调方法</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<object> OnValueDestroy(this shaco.Base.IObserver<object> observer, System.Action<object> callbackDestroy)
    {
        return OnValueDestroy<object>(observer, callbackDestroy);
    }
    static public shaco.Base.IObserver<T> OnValueDestroy<T>(this shaco.Base.IObserver<T> observer, System.Action<T> callbackDestroy)
    {
        observer.callbackDestroy = (shaco.Base.ISubject<T> subjectTmp) =>
        {
            callbackDestroy(subjectTmp.value);
        };
        return observer;
    }

    /// <summary>
    /// 观察者开始观察数据
    /// <param name="observer">观察者</param>
    /// <param name="bindTarget">绑定对象</param>
    /// <return>观察者</return>
    /// </summary>
    static public shaco.Base.IObserver<object> Start(this shaco.Base.IObserver<object> observer, System.Object bindTarget)
    {
        return Start<object>(observer, bindTarget);
    }
    static public shaco.Base.IObserver<T> Start<T>(this shaco.Base.IObserver<T> observer, System.Object bindTarget)
    {
        observer.subject.SetBindTarget(bindTarget);
        observer.subject.Add(observer);
        return observer;
    }

    /// <summary>
    /// 观察者停止观察数据
    /// <param name="observer">观察者</param>
    /// <return>观察者</return>
    /// </summary>
    static public bool End(this shaco.Base.IObserver<object> observer)
    {
        return End<object>(observer);
    }
    static public bool End<T>(this shaco.Base.IObserver<T> observer)
    {
        return observer.subject.Remove(observer);
    }

    /// <summary>
    /// 数据停止被观察
    /// <param name="observer">观察者</param>
    /// <return>数据主题</return>
    /// </summary>
    static public void End(this shaco.Base.ISubject<object> subject)
    {
        End<object>(subject);
    }
    static public void End<T>(this shaco.Base.ISubject<T> subject)
    {
        subject.Clear();
    }
}