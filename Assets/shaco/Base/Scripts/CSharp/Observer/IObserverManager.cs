using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class SubjectLocation
    {
        public Dictionary<IObserverBase, ObserverLocation> observserLocations = new Dictionary<IObserverBase, ObserverLocation>();
    }

    public class ObserverLocation
    {
        /// <summary>
        /// 初始化观察对象与数据绑定时候的堆栈信息
        /// </summary>
        public StackLocation stackLocationObserverInit = new StackLocation();

        /// <summary>
        /// 观测数据被改变时候的堆栈信息
        /// </summary>
        public StackLocation stackLocationValueChange = new StackLocation();

        /// <summary>
        /// 观察者的初始回调方法
        /// </summary>
        public System.Delegate callbackInitDelegate = null;

        /// <summary>
        /// 观察者的数据刷新回调方法
        /// </summary>
        public System.Delegate callbackUpdateDelegate = null;
    }

    public interface IObserverManager : IGameInstance
    {
        /// <summary>
        /// 添加观察者回调
        /// </summary>
        EventCallBack<ISubjectBase> callbackAddObserver { get; }

        /// <summary>
        /// 移除观察者回调
        /// </summary>
        EventCallBack<ISubjectBase> callbackRemoveObserver { get; }

        /// <summary>
        /// 添加数据观测者
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        bool AddObserver<T>(ISubject<T> subject, IObserver<T> observer);

        /// <summary>
        /// 移除数据观测者
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        bool RemoveObserver<T>(ISubject<T> subject, IObserver<T> observer);

		/// <summary>
		/// 移除数据主体
		/// <param name="subject">数据主体</param>
		/// </summary>
        bool RemoveSubject<T>(ISubject<T> subject);

        /// <summary>
        /// 数据即将初始化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        bool WillValueInit<T>(ISubject<T> subject, IObserver<T> observer);

        /// <summary>
        /// 数据初始化完毕
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        bool ValueInited<T>(ISubject<T> subject, IObserver<T> observer);

        /// <summary>
        /// 数据即将发生变化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        bool WillValueUpdate<T>(ISubject<T> subject, IObserver<T> observer);

        /// <summary>
        /// 数据已经发生变化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        bool ValueUpdated<T>(ISubject<T> subject, IObserver<T> observer);

        /// <summary>
        /// 获取绑定对象的所有数据调用定位信息
        /// <param name="bindTarget">绑定对象</param>
        /// </summary>
        Dictionary<ISubjectBase, SubjectLocation> GetSubjectLocation(object bindTarget);

        /// <summary>
        /// 获取哦所有观察数据主体
        /// </summary>
        ICollection<ISubjectBase> GetSubjects();

        /// <summary>
        /// 清理所有观察者和数据体
        /// </summary>
        void Clear();
    }
}