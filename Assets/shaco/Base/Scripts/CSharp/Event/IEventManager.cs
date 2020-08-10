using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 事件回调信息
    /// <param name="eventID">事件ID</param>
    /// <param name="CallBack">回调方法</param>
    /// </summary>
    public class EventCallBackInfo
    {
        public string eventID = string.Empty;

        public EventCallBack<BaseEventArg> CallBack = new EventCallBack<BaseEventArg>();
    }

    /// <summary>
    /// 事件管理类，用于添加、删除、派发多个事件，具有穿透性和易管理性
    /// </summary>
    public interface IEventManager : IGameInstance
    {
        /// <summary>
        /// 是否启用该事件管理器
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// 当前绑定过的事件数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加(绑定事件)
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="defaultSender">默认事件绑定对象，不能为空</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <param name="invokeOnce">是否只触发1次回调</param>
        bool AddEvent<T>(object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false) where T : BaseEventArg;
        bool AddEvent(System.Type type, object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, bool invokeOnce = false);

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <return>true:移除成功，false:移除失败</return>
        bool RemoveEvent<T>(EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc) where T : BaseEventArg;
        bool RemoveEvent(System.Type type, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc);

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="callfunc">事件回调函数</param>
        /// <param name="type">事件类型</param>
        /// <return>true:移除成功，false:移除失败</return>
        bool RemoveEvent(EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc, System.Type type);

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="defaultSender">事件绑定的对象</param>
        /// <param name="callfunc">事件回调函数</param>
        /// <return>true:移除成功，false:移除失败</return>
        bool RemoveEvent<T>(object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc) where T : BaseEventArg;
        bool RemoveEvent(System.Type type, object defaultSender, EventCallBack<BaseEventArg>.CALL_FUNC_EVENT callfunc);

        /// <summary>
        /// 移除添加过的事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <param name="defaultSender">事件绑定的对象</param>
        bool RemoveEvent<T>(object defaultSender) where T : BaseEventArg;
        bool RemoveEvent(System.Type type, object defaultSender);

        /// <summary>
        /// 移除defaultSender绑定过的所有事件
        /// </summary>
        /// <param name="defaultSender">事件绑定的对象</param>
        void RemoveAllEvent(object defaultSender);

        /// <summary>
        /// 移除T类型相关的所有添加过的事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <return>true:移除成功，false:移除失败</return>
        bool RemoveEvent<T>() where T : BaseEventArg;
        bool RemoveEvent(System.Type type);

        /// <summary>
        /// 清空所有事件监听
        /// </summary>
        void ClearEvent();

        /// <summary>
        /// 获取T类型绑定过该事件的所有回调信息
        /// </summary>
        /// <param name="T">事件类型</param>
        /// <return>所有T类型相关的回调信息</return>
        EventCallBack<BaseEventArg> GetEvents<T>() where T : BaseEventArg;

        /// <summary>
        /// 获取默认绑定对象相关的所有回调信息
        /// </summary>
        /// <param name="defaultSender">事件绑定的对象</param>
        /// <returns>返回字典对象[key:事件ID, value:回调方法信息]</returns>
        Dictionary<string, List<EventCallBack<BaseEventArg>.CallBackInfo>> GetEvents(object defaultSender);

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="sender">派发事件的对象，如果为空，则会使用默认事件绑定对象</param>
        /// <param name="arg">事件参数</param>
        /// <return>true:派发成功，false:派发失败</return>
        bool InvokeEvent(object sender, BaseEventArg arg);

        /// <summary>
        /// 派发事件
        /// </summary>
        /// <param name="arg">事件参数</param>
        /// <return>true:派发成功，false:派发失败</return>
        bool InvokeEvent(BaseEventArg arg);

        /// <summary>
        /// 是否已经添加过T类型绑定过的事件
        /// </summary>
        /// <param name="type">事件类型</param>
        /// <return>true:包含该事件，false:不包含该事件</return>
        bool HasEventID<T>() where T : BaseEventArg;
        bool HasEventID(System.Type type);

        /// <summary>
        /// 遍历所有的事件
        /// </summary>
        /// <param name="callfunc">遍历事件回调方法<事件ID，事件回调信息, [返回值，true：继续遍历 false：停止遍历]></param>
        void Foreach(System.Func<string, EventCallBackInfo, bool> callfunc);


        /// <summary>
        /// 设置当前使用的事件管理器，同时会激活它
        /// </summary>
        /// <param name="managerID">事件管理器识别符</param>
        void SetCurrentEventManager(string managerID);

        /// <summary>
        /// 获取当前使用的事件管理器
        /// </summary>
        EventManager GetCurrentEventManager();

        void UseCurrentEventManagerEnd();

        /// <summary>
        /// 设置当前使用的事件管理器识别符
        /// </summary>
        string GetCurrentEventManagerID();

        /// <summary>
        /// 移除当前事件管理器
        /// *注意* 
        /// 该方法会导致当前没有事件管理器运行，所有的事件派发都会失败 
		/// </summary>
        bool RemoveCurrentEventManager();

        /// <summary>
        /// 移除没有运行的事件管理器，释放资源
        /// *注意*
        /// 该方法同时会移除对应的事件管理器添加过的事件监听
		/// </summary>
        void RemoveUnuseEventManagers();

        /// <summary>
        /// 清空所有事件管理器
		/// *注意* 
		/// 该方法清理所有事件，建议只在游戏退出或者重启的时候调用
        /// </summary>
        void ClearEventManager();
    }
}

