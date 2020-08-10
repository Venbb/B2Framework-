using System.Collections;
using UnityEngine;

/// <summary>
/// 事件扩展方法，用于更加方便调用事件
/// </summary>
static public class shaco_ExtensionsEventManagerUnity
{
    /// <summary>
    /// 添加自动销毁的事件，会随着defaultSender被销毁的时候，自动释放
    /// <param name="defaultSender">事件默认绑定对象</param>
    /// <param name="callfunc">事件回调方法</param>
    /// <return>添加事件是否成功</return>
    /// </summary>
    static public bool AddAutoRealeaseEvent<T>(this GameObject defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        BindAutoReleaseComponent(defaultSender, defaultSender);
        return shaco.GameHelper.Event.AddEvent<T>(defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg as T);
        }, false);
    }

    static public bool AddAutoRealeaseEvent(this GameObject defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        BindAutoReleaseComponent(defaultSender, defaultSender);
        return shaco.GameHelper.Event.AddEvent(type, defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg);
        }, false);
    }

    static public bool AddAutoRealeaseOnceEvent<T>(this GameObject defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        BindAutoReleaseComponent(defaultSender, defaultSender);
        return shaco.GameHelper.Event.AddEvent<T>(defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg as T);
        }, true);
    }

    static public bool AddAutoRealeaseOnceEvent(this GameObject defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        BindAutoReleaseComponent(defaultSender, defaultSender);
        return shaco.GameHelper.Event.AddEvent(type, defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg);
        }, true);
    }

    static public bool AddAutoRealeaseOnceEvent<T>(this Component defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        return AddAutoRealeaseOnceEvent(defaultSender.gameObject, callfunc);
    }

    static public bool AddAutoRealeaseOnceEvent(this Component defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        return AddAutoRealeaseOnceEvent(defaultSender.gameObject, type, callfunc);
    }

    static public bool AddAutoRealeaseEvent<T>(this Component defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        return AddAutoRealeaseEvent(defaultSender.gameObject, callfunc);
    }

    static public bool AddAutoRealeaseEvent(this Component defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        return AddAutoRealeaseEvent(defaultSender.gameObject, type, callfunc);
    }
    
    static public bool AddAutoRealeaseEvent<T>(this Transform defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        return AddAutoRealeaseEvent(defaultSender.gameObject, callfunc);
    }

    static public bool AddAutoRealeaseEvent(this Transform defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        return AddAutoRealeaseEvent(defaultSender.gameObject, type, callfunc);
    }

    static public bool AddAutoRealeaseOnceEvent<T>(this Transform defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        return AddAutoRealeaseOnceEvent(defaultSender.gameObject, callfunc);
    }

    static public bool AddAutoRealeaseOnceEvent(this Transform defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        return AddAutoRealeaseOnceEvent(defaultSender.gameObject, type, callfunc);
    }

    /// <summary>
    /// 按照队列顺序依次执行事件
    /// </summary>
    static public shaco.SequeueEventComponent InvokeSequeueEvent(this object unuseSender, params IEnumerator[] coroutines)
    {
        var sequeueEvent = new shaco.SequeueEvent();
        for (int i = 0; i < coroutines.Length; ++i)
        {
            sequeueEvent = sequeueEvent.AppendEvent(coroutines[i]);
        }
        return sequeueEvent.Run();
    }

    /// <summary>
    /// 绑定自动销毁组件对象，在绑定对象销毁时候，自动销毁相关联事件
    /// <param name="defaultSender">事件默认绑定对象</param>
    /// </summary>
    static private void BindAutoReleaseComponent(GameObject bindTarget, object defaultSender)
    {
        var autoReleaseComponent = bindTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
        if (null == autoReleaseComponent)
        {
            autoReleaseComponent = bindTarget.AddComponent<shaco.UnityObjectAutoReleaseComponent>();
        }

        autoReleaseComponent.AddOnDestroyCallBack(bindTarget, () =>
        {
            shaco.GameHelper.Event.RemoveAllEvent(defaultSender);
        });
    }
}