using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

static public class shaco_ExtensionsObserverUnity
{
    /// <summary>
    /// 观察者开始观察数据，并绑定一个对象，数据主体和观测者会在对象销毁时候，也自动销毁
    /// <param name="observer">观察者</param>
    /// <param name="bindTarget">绑定对象</param>
    /// </summary>
    static public shaco.Base.IObserver<object> Start(this shaco.Base.IObserver<object> observer, UnityEngine.Component bindTarget)
    {
        return Start<object>(observer, bindTarget);
    }
    static public shaco.Base.IObserver<T> Start<T>(this shaco.Base.IObserver<T> observer, UnityEngine.Component bindTarget)
	{
        if (null == bindTarget)
        {
            shaco.Base.Log.Error("ObserverExtensionUnity Start Component error: bind target is null");
            return observer;
        }

        if (null == observer)
        {
            shaco.Base.Log.Error("ObserverExtensionUnity Start Component error: observer is null");
            return observer;
        }

        if (null == observer.subject)
        {
            shaco.Base.Log.Error("ObserverExtensionUnity Start Component error: subject is null");
            return observer;
        }

        var autoReleaseComponent = bindTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
        if (null == autoReleaseComponent)
        {
            autoReleaseComponent = bindTarget.gameObject.AddComponent<shaco.UnityObjectAutoReleaseComponent>();
            if (null == autoReleaseComponent)
            {
                shaco.Base.Log.Error("ObserverExtensionUnity Start Component error: can't add component 'ObserverAutoReleaseComponent'");
                return observer;
            }
        }

        autoReleaseComponent.AddOnDestroyCallBack(bindTarget, () =>
        {
            observer.subject.Clear();
        });

        observer.subject.SetBindTarget(bindTarget);
        observer.subject.Add(observer);
        return observer;
	}
}