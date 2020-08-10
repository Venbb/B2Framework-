using System.Collections;
using UnityEngine;

namespace shaco.UIStateChangedEvents 
{
    /// <summary>
    /// UI状态变化事件
    /// </summary>
    public class OnUIPreLoadEvent : OnUIStateChangedBaseEvent {}
    public class OnUIInitEvent : OnUIStateChangedBaseEvent {}
	public class OnUIOpenEvent : OnUIStateChangedBaseEvent {}
    public class OnUIResumeEvent : OnUIStateChangedBaseEvent {}
    public class OnUIHideEvent : OnUIStateChangedBaseEvent {}
    public class OnUICloseEvent : OnUIStateChangedBaseEvent {}
    public class OnUIRefreshEvent : OnUIStateChangedBaseEvent { }
    public class OnUIBringToFrontEvent : OnUIStateChangedBaseEvent {}

    /// <summary>
    /// 如果ui设置了UILayerOpenAsyncAttribute属性，则需要通过监听该事件来异步获取ui对象
    /// </summary>
    public class OnUIOpenEndEvent<T> : shaco.Base.BaseEventArg where T : UnityEngine.Component
    {
        //ui对象，如果加载失败则为null
        public T uiTarget = null;
    }

    /// <summary>
    /// ui状态发生变化基类事件
    /// </summary>
    public class OnUIStateChangedBaseEvent : shaco.Base.BaseEventArg
	{
		public string uiKey = string.Empty;
		public Component uiTarget = null;
	}
    
    /// <summary>
    /// ui加载超时开始事件
    /// </summary>
    public class OpenUITimeoutStartEvent : shaco.Base.BaseEventArg
    {
        public string uiKey = string.Empty;
    }

    /// <summary>
    /// ui加载超时结束事件 - 表示ui加载结束了，所以不再有超时
    /// </summary>
    public class OpenUITimeoutEndEvent : shaco.Base.BaseEventArg
    {
        public string uiKey = string.Empty;
    }

    /// <summary>
    /// 异步加载UI失败事件
    /// </summary>
    public class OpenAysncUIErrorEvent : shaco.Base.BaseEventArg
    {
        public string uiKey = string.Empty;
        public System.Action requestReloadUI = null;
    }
}