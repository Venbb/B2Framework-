using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class ObserverManager : IObserverManager
    {
        /// <summary>
        /// 添加观察者回调
        /// </summary>
        public EventCallBack<ISubjectBase> callbackAddObserver { get { return _callbackAddObserver; } }
        private EventCallBack<ISubjectBase> _callbackAddObserver = new EventCallBack<ISubjectBase>();

        /// <summary>
        /// 移除观察者回调
        /// </summary>
        public EventCallBack<ISubjectBase> callbackRemoveObserver { get { return _callbackRemoveObserver; } }
        private EventCallBack<ISubjectBase> _callbackRemoveObserver = new EventCallBack<ISubjectBase>();

        /// <summary>
        /// 所有正在使用的数据主体，[数据主体绑定对象，[数据主体，观测者堆栈信息]]]
        /// </summary>
        private Dictionary<object, Dictionary<ISubjectBase, SubjectLocation>> _subjectStackLocations = new Dictionary<object, Dictionary<ISubjectBase, SubjectLocation>>();

        /// <summary>
        /// 添加数据观测者
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        public bool AddObserver<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findObserverLocationInfo = GetOrCreateObserverLocation(subject, observer, true);
            if (null != findObserverLocationInfo)
            {
                findObserverLocationInfo.callbackInitDelegate = observer.callbackInit;
                findObserverLocationInfo.callbackUpdateDelegate = observer.callbackUpdate;

                _callbackAddObserver.InvokeAllCallBack(subject.GetBindTarget(), subject);
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 移除数据观测者
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        public bool RemoveObserver<T>(ISubject<T> subject, IObserver<T> observer)
        {
            SubjectLocation findSubjectLocationInfo = GetOrCreateSubjectLocation(subject, false);
            if (null == findSubjectLocationInfo)
                return false;

            _callbackRemoveObserver.InvokeAllCallBack(subject.GetBindTarget(), subject);

            findSubjectLocationInfo.observserLocations.Remove(observer);
            if (0 == findSubjectLocationInfo.observserLocations.Count)
            {
                var key = subject.GetBindTarget();
                this._subjectStackLocations.Remove(key);
            }
            return true;
        }

        /// <summary>
        /// 移除数据主体
        /// <param name="subject">数据主体</param>
        /// </summary>
        public bool RemoveSubject<T>(ISubject<T> subject)
        {
            SubjectLocation findSubjectLocationInfo = GetOrCreateSubjectLocation(subject, false);
            if (null == findSubjectLocationInfo)
                return false;

            _callbackRemoveObserver.InvokeAllCallBack(subject.GetBindTarget(), subject);

            var key = subject.GetBindTarget();
            this._subjectStackLocations.Remove(key);
            return true;
        }

        /// <summary>
        /// 数据即将初始化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        public bool WillValueInit<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationObserverInit.StartTimeSpanCalculate();
            }
            return true;
        }

        /// <summary>
        /// 数据初始化完毕
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        public bool ValueInited<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationObserverInit.StopTimeSpanCalculate();
                findLocationInfo.stackLocationObserverInit.GetStack();
            }
            return true;
        }

        /// <summary>
        /// 数据即将发生变化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        public bool WillValueUpdate<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationValueChange.StartTimeSpanCalculate();
            }
            return true;
        }

        /// <summary>
        /// 数据已经发生变化
        /// <param name="subject">数据主体</param>
        /// <param name="observer">数据观测者</param>
        /// </summary>
        public bool ValueUpdated<T>(ISubject<T> subject, IObserver<T> observer)
        {
            ObserverLocation findLocationInfo = GetOrCreateObserverLocation(subject, observer, false);
            if (null != findLocationInfo)
            {
                findLocationInfo.stackLocationValueChange.StopTimeSpanCalculate();
                findLocationInfo.stackLocationValueChange.GetStack();
            }
            return true;
        }

        /// <summary>
        /// 获取绑定对象的所有数据调用定位信息
        /// <param name="bindTarget">绑定对象</param>
        /// </summary>
        public Dictionary<ISubjectBase, SubjectLocation> GetSubjectLocation(object bindTarget)
        {
            Dictionary<ISubjectBase, SubjectLocation> retValue = null;
            if (!_subjectStackLocations.TryGetValue(bindTarget, out retValue))
            {
                Log.Error("SubjectManager GetSubjectLocation erorr: not found bindTarget=" + bindTarget);
            }
            return retValue;
        }

        /// <summary>
        /// 获取哦所有观察数据主体
        /// </summary>
        public ICollection<ISubjectBase> GetSubjects()
        {
            var retValue = new List<ISubjectBase>();

            foreach (var iter in _subjectStackLocations)
            {
                foreach (var iter2 in iter.Value)
                {
                    retValue.Add(iter2.Key);
                }
            }
            return retValue;
        }

        /// <summary>
        /// 清理所有观察者和数据体
        /// </summary>
        public void Clear()
        {
            var willRemoveSubjects = new List<ISubjectBase>();
            foreach (var iter in _subjectStackLocations)
            {
                foreach (var iter2 in iter.Value)
                {
                    willRemoveSubjects.Add(iter2.Key);
                }
            }

            for (int i = willRemoveSubjects.Count - 1; i >= 0; --i)
                willRemoveSubjects[i].Clear();
        }

        /// <summary>
        /// 获取或者创建数据主体堆栈信息
        /// <param name="subject">数据主体</param>
        /// <param name="autoCreateWhenNotFound">是否当没有找到观测者的时候，自动创建并添加到列表中</param>
        /// <return>观测者堆栈信息</return>
        /// </summary>
        private SubjectLocation GetOrCreateSubjectLocation<T>(ISubject<T> subject, bool autoCreateWhenNotFound)
        {
            SubjectLocation retVale = null;
            Dictionary<ISubjectBase, SubjectLocation> findLocationInfos = null;

            var key = subject.GetBindTarget();

            if (null == key)
            {
                Log.Error("SubjectManager GetOrCreateSubjectLocation error: key is null");
                return retVale;
            }

            if (!this._subjectStackLocations.TryGetValue(key, out findLocationInfos))
            {
                findLocationInfos = new Dictionary<ISubjectBase, SubjectLocation>();

                if (autoCreateWhenNotFound)
                {
                    this._subjectStackLocations.Add(key, findLocationInfos);
                }
                else
                {
                    // Log.Error("SubjectManager GetOrCreateSubjectLocation error: not found subjects location, target=" + key + " subject=" + subject);
                    return retVale;
                }
            }

            if (!findLocationInfos.TryGetValue(subject, out retVale))
            {
                retVale = new SubjectLocation();

                if (autoCreateWhenNotFound)
                {
                    findLocationInfos.Add(subject, retVale);
                }
                else
                {
                    Log.Error("SubjectManager GetOrCreateSubjectLocation error: not found subject location, subject=" + subject);
                    return retVale;
                }
            }
            return retVale;
        }

        /// <summary>
        /// 获取或者创建观测者堆栈信息
        /// <param name="subject">数据主体</param>
        /// <param name="observer">观测者</param>
        /// <param name="autoCreateWhenNotFound">是否当没有找到观测者的时候，自动创建并添加到列表中</param>
        /// <return>观测者堆栈信息</return>
        /// </summary>
        private ObserverLocation GetOrCreateObserverLocation<T>(ISubject<T> subject, IObserver<T> observer, bool autoCreateWhenNotFound)
        {
            ObserverLocation retVale = null;

            SubjectLocation findSubjectLocationInfo = GetOrCreateSubjectLocation(subject, autoCreateWhenNotFound);

            if (null != findSubjectLocationInfo)
            {
                bool isFindObserverLocation = findSubjectLocationInfo.observserLocations.TryGetValue(observer, out retVale);
                if (autoCreateWhenNotFound)
                {
                    if (isFindObserverLocation)
                    {
                        Log.Error("SubjectManager GetOrCreateObserverLocation error: has duplicate observer=" + observer + " in subject=" + subject);
                        return retVale;
                    }
                    else
                    {
                        retVale = new ObserverLocation();
                        findSubjectLocationInfo.observserLocations.Add(observer, retVale);
                    }
                }
                else if (!isFindObserverLocation)
                {
                    Log.Error("SubjectManager GetOrCreateObserverLocation error: not found observer=" + observer + " in subject=" + subject);
                }
            }
            return retVale;
        }
    }
}