using System.Collections;

/// <summary>
/// 事件扩展方法，用于更加方便调用事件
/// </summary>
static public class shaco_ExtensionsEventManagerCSharp
{
    static public bool AddEvent<T>(this object defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        return shaco.Base.GameHelper.Event.AddEvent<T>(defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg as T);
        }, false);
    }

    static public bool AddEvent(this object defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        return shaco.Base.GameHelper.Event.AddEvent(type, defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg);
        }, false);
    }

    static public bool AddOnceEvent<T>(this object defaultSender, shaco.Base.EventCallBack<T>.CALL_FUNC_EVENT callfunc) where T : shaco.Base.BaseEventArg
    {
        return shaco.Base.GameHelper.Event.AddEvent<T>(defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg as T);
        }, true);
    }

    static public bool AddOnceEvent(this object defaultSender, System.Type type, shaco.Base.EventCallBack<shaco.Base.BaseEventArg>.CALL_FUNC_EVENT callfunc)
    {
        return shaco.Base.GameHelper.Event.AddEvent(type, defaultSender, (senderTmp, arg) =>
        {
            callfunc(senderTmp, arg);
        }, true);
    }

    static public bool RemoveEvent<T>(this object defaultSender) where T : shaco.Base.BaseEventArg
    {
        return shaco.Base.GameHelper.Event.RemoveEvent<T>(defaultSender);
    }

    static public bool RemoveEvent(this object defaultSender, System.Type type)
    {
        return shaco.Base.GameHelper.Event.RemoveEvent(type, defaultSender);
    }

    static public void RemoveAllEvent(this object defaultSender)
    {
        shaco.Base.GameHelper.Event.RemoveAllEvent(defaultSender);
    }

    static public bool InvokeEvent(this object sender, shaco.Base.BaseEventArg arg)
    {
        return shaco.Base.GameHelper.Event.InvokeEvent(sender, arg);
    }
}