using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 事件回调信息，用于添加、删除、派发单个事件
    /// </summary>
    public class EventCallBack<TValue>
    {
        public delegate void CALL_FUNC_EVENT(object sender, TValue arg);

        /// <summary>
        /// 事件回调信息
        /// </summary>
        public class CallBackInfo
        {
            public object DefaultSender = null;
            public CALL_FUNC_EVENT CallFunc = null;
            public bool InvokeOnce = false;
            public bool WillRemove = false;
            public StackLocation CallAddEventStack = new StackLocation();
            public StackLocation CallInvokeEventStack = new StackLocation();
        }

        /// <summary>
        /// 所有回调信息
        /// </summary>
        private List<CallBackInfo> _callBacks = new List<CallBackInfo>();

        /// <summary>
        /// 即将添加的回调信息
        /// </summary>
        private List<CallBackInfo> _willAddCallBacks = new List<CallBackInfo>();

        /// <summary>
        /// 即将删除的回调信息
        /// </summary>
        private List<CallBackInfo> _willRemoveCallBacks = new List<CallBackInfo>();

        /// <summary>
        /// 应该重新触发本次回调的次数(可能在回调中又调用了一次相同回调)
        /// </summary>
        private int _shouldInvokeAgainTimes = 0;

        /// <summary>
        /// 是否正在派发事件标志，用于解决线程冲突和事件回调锁死
        /// </summary>
        private bool _isInvoking = false;

        /// <summary>
        /// 绑定过的事件数量
        /// </summary>
        public int Count
        {
            get { return _callBacks.Count; }
        }

        /// <summary>
        /// 通过下标获取事件信息
        /// <param name="index">事件下标</param>
        /// </summary>
        public CallBackInfo this[int index]
        {
            get
            {
                if (index < 0 || index > _callBacks.Count - 1)
                {
                    Log.Exception("EventCallBackArg this[int index] error: out of range, index=" + index + " count=" + _callBacks.Count);
                    return null;
                }
                else
                {
                    return _callBacks[index];
                }
            }
        }

        /// <summary>
        /// 添加事件
        /// </summary>
        /// <param name="defaultSender">绑定对象</param>
        /// <param name="callfunc">派发事件时的回调方法</param>
        /// <param name="InvokeOnce">是否只触发1次回调</param>
        public void AddCallBack(object defaultSender, CALL_FUNC_EVENT callfunc, bool invokeOnce = false)
        {
            lock (_callBacks)
            {
                if (null == defaultSender || callfunc == null)
                {
                    Log.Exception("EventCallBackArg AddCallBack error: has invalid parameters" + GetCallBackInformation(callfunc));
                }

                var callbacksTmp = GetCallBacks(defaultSender, callfunc, true);
                if (!callbacksTmp.IsNullOrEmpty())
                {
                    Log.Exception("EventCallBackArg AddCallBack error: has added callback by defaultSender=" + defaultSender.ToTypeString()
                                              + GetCallBackInformation(callbacksTmp[0].CallFunc));
                }

                CallBackInfo newCallBackTmp = new CallBackInfo();
                newCallBackTmp.CallAddEventStack.StartTimeSpanCalculate();
                {
                    newCallBackTmp.DefaultSender = defaultSender;
                    newCallBackTmp.CallFunc = callfunc;
                    newCallBackTmp.InvokeOnce = invokeOnce;
                    newCallBackTmp.CallAddEventStack.GetStack();
                    if (_isInvoking)
                    {
                        _willAddCallBacks.Add(newCallBackTmp);
                    }
                    else
                    {
                        _callBacks.Add(newCallBackTmp);
                    }
                }
                newCallBackTmp.CallAddEventStack.StopTimeSpanCalculate();
            }
        }

        /// <summary>
        /// 移除事件
        /// <param name="defaultSender">绑定对象</param>
        /// <param name="callfunc">派发事件时的回调方法</param>
        /// <return>被移除的事件信息</return>
        /// </summary>
        public CallBackInfo[] RemoveCallBack(object defaultSender, CALL_FUNC_EVENT callfunc)
        {
            lock (_callBacks)
            {
                CallBackInfo[] retValue = null;
                var callbacksTmp = GetCallBacks(defaultSender, callfunc, false);
                if (callbacksTmp.IsNullOrEmpty() && null != callfunc)
                {
                    Log.Exception("EventCallBackArg RemoveCallBack error: not find callback by defaultSender=" + defaultSender.ToTypeString()
                                              + GetCallBackInformation(callfunc));
                }

                if (callbacksTmp.IsNullOrEmpty())
                    return retValue;

                retValue = new CallBackInfo[callbacksTmp.Count];
                for (int i = 0; i < callbacksTmp.Count; ++i)
                {
                    retValue[i] = callbacksTmp[i];

                    if (_isInvoking)
                    {
                        if (!_willRemoveCallBacks.Contains(callbacksTmp[i]))
                        {
                            callbacksTmp[i].WillRemove = true;
                            _willRemoveCallBacks.Add(callbacksTmp[i]);
                        }
                    }
                    else
                    {
                        _callBacks.Remove(callbacksTmp[i]);
                    }
                }
                return retValue;
            }
        }

        /// <summary>
        /// 移除事件
        /// <param name="callfunc">派发事件时的回调方法</param>
        /// <return>被移除的事件信息</return>
        /// </summary>
        public CallBackInfo[] RemoveCallBack(CALL_FUNC_EVENT callfunc)
        {
            return RemoveCallBack(null, callfunc);
        }

        /// <summary>
        /// 移除事件
        /// <param name="defaultSender">绑定对象</param>
        /// <return>被移除的事件信息</return>
        /// </summary>
        public CallBackInfo[] RemoveCallBack(object defaultSender)
        {
            return RemoveCallBack(defaultSender, null);
        }

        /// <summary>
        /// 清空所有事件
        /// </summary>
        public void ClearCallBack()
        {
            lock (_callBacks)
            {
                if (_isInvoking)
                {
                    for (int i = _callBacks.Count - 1; i >= 0; --i)
                    {
                        if (!_willRemoveCallBacks.Contains(_callBacks[i]))
                        {
                            _callBacks[i].WillRemove = true;
                            _willRemoveCallBacks.Add(_callBacks[i]);
                        }
                    }
                }
                else
                {
                    _callBacks.Clear();
                }
            }
        }

        public bool HasCallBack(object defaultSender)
        {
            return !GetCallBacksIndex(_callBacks, defaultSender, null, true).IsNullOrEmpty()
            || !GetCallBacksIndex(_willAddCallBacks, defaultSender, null, true).IsNullOrEmpty();
        }

        public bool HasCallBack(CALL_FUNC_EVENT callfunc)
        {
            return !GetCallBacksIndex(_callBacks, null, callfunc, true).IsNullOrEmpty()
            || !GetCallBacksIndex(_willAddCallBacks, null, callfunc, true).IsNullOrEmpty();
        }

        public void InvokeAllCallBack(object defaultSender, TValue arg)
        {
            InvokeAllCallBackBase(defaultSender, arg);
        }

        public void InvokeAllCallBack(TValue arg)
        {
            InvokeAllCallBackBase(null, arg);
        }

        public EventCallBack<TValue> Clone()
        {
            var retValue = new EventCallBack<TValue>();
            retValue._callBacks = new List<CallBackInfo>(this._callBacks);
            retValue._willAddCallBacks = new List<CallBackInfo>(this._willAddCallBacks);
            retValue._willRemoveCallBacks = new List<CallBackInfo>(this._willRemoveCallBacks);
            retValue._shouldInvokeAgainTimes = this._shouldInvokeAgainTimes;
            retValue._isInvoking = this._isInvoking;
            return retValue;
        }

        private object GetInvokeSender(object sender, CallBackInfo callInfo)
        {
            if (null != sender)
                return sender;
            else
                return null == callInfo ? null : callInfo.DefaultSender;        
        }

        private void InvokeAllCallBackBase(object sender, TValue arg)
        {
            lock (_callBacks)
            {
                if (_isInvoking)
                {
                    ++_shouldInvokeAgainTimes;
                    return;
                }

                _isInvoking = true;
                for (int i = 0; i < _callBacks.Count; ++i)
                {
                    if (null == _callBacks)
                    {
                        Log.Exception("EventCallBackArg invokeAllCallBack erorr: _callBacks is null !");
                        break;
                    }
                    
                    if (i >= _callBacks.Count)
                    {
                        Log.Exception("EventCallBackArg invokeAllCallBack erorr: index out of range ! index=" + i + " callback count=" + _callBacks.Count);
                        break;
                    }

                    CallBackInfo callbackInfoTmp = _callBacks[i];
                    callbackInfoTmp.CallInvokeEventStack.GetStack();
                    if (null == callbackInfoTmp)
                    {
                        Log.Exception("EventCallBackArg invokeAllCallBack erorr: callback information is null !");
                        continue;
                    }

                    if (null == callbackInfoTmp.DefaultSender)
                    {
                        if (!_willRemoveCallBacks.Contains(callbackInfoTmp)) _willRemoveCallBacks.Add(callbackInfoTmp);
                        Log.Exception("EventCallBackArg invokeAllCallBack error: sender is missing, we will remove it" + GetCallBackInformation(callbackInfoTmp.CallFunc));
                    }

                    if (null != callbackInfoTmp.CallFunc)
                    {
                        if (!callbackInfoTmp.WillRemove)
                        {
                            callbackInfoTmp.CallInvokeEventStack.StartTimeSpanCalculate("EventCallBack Invoke: target=" + callbackInfoTmp.CallFunc.Target + " method=" + callbackInfoTmp.CallFunc.Method);
                            {
                                try
                                {
                                    callbackInfoTmp.CallFunc(sender == null ? callbackInfoTmp.DefaultSender : sender, arg);
                                }
                                catch (System.Exception e)
                                {
                                    Log.Exception("EventCallBackArg invoke error: target=" + callbackInfoTmp.CallFunc.Target + " method=" + callbackInfoTmp.CallFunc.Method + " e=" + e);
                                }
                            }
                            callbackInfoTmp.CallInvokeEventStack.StopTimeSpanCalculate();
                        }
                    }
                    else
                    {
                        Log.Warning("EventCallBackArg invokeAllCallBack warning: callback function is missing" + GetCallBackInformation(callbackInfoTmp.CallFunc), GetInvokeSender(sender, callbackInfoTmp));
                    }

                    if (callbackInfoTmp.InvokeOnce)
                    {
                        if (!_willRemoveCallBacks.Contains(callbackInfoTmp)) _willRemoveCallBacks.Add(callbackInfoTmp);
                    }
                }
                _isInvoking = false;

                //check remove 
                if (_willRemoveCallBacks.Count > 0)
                {
                    for (int i = _willRemoveCallBacks.Count - 1; i >= 0; --i)
                    {
                        var callbackInfoTmp = _willRemoveCallBacks[i];
                        RemoveCallBack(callbackInfoTmp.DefaultSender, callbackInfoTmp.CallFunc);
                    }
                    _willRemoveCallBacks.Clear();
                }

                //check add
                if (_willAddCallBacks.Count > 0)
                {
                    for (int i = _willAddCallBacks.Count - 1; i >= 0; --i)
                    {
                        var callbackInfoTmp = _willAddCallBacks[i];
                        _willAddCallBacks.RemoveAt(i);
                        AddCallBack(callbackInfoTmp.DefaultSender, callbackInfoTmp.CallFunc, callbackInfoTmp.InvokeOnce);
                    }
                }

                //check invoke
                if (_shouldInvokeAgainTimes > 0)
                {
                    --_shouldInvokeAgainTimes;
                    InvokeAllCallBackBase(sender, arg);
                }
            }
        }

        private List<CallBackInfo> GetCallBacks(object defaultSender, CALL_FUNC_EVENT callfunc, bool findOne)
        {
            lock (_callBacks)
            {
                var indexesTmp1 = GetCallBacksIndex(_callBacks, defaultSender, callfunc, findOne);
                var indexesTmp2 = GetCallBacksIndex(_willAddCallBacks, defaultSender, callfunc, findOne);
                List<CallBackInfo> retValue = null;

                if (!indexesTmp1.IsNullOrEmpty())
                {
                    for (int i = 0; i < indexesTmp1.Count; ++i)
                    {
                        if (null == retValue)
                            retValue = new List<CallBackInfo>();
                        retValue.Add(_callBacks[indexesTmp1[i]]);
                    }
                }
                if (!indexesTmp2.IsNullOrEmpty())
                {
                    for (int i = 0; i < indexesTmp2.Count; ++i)
                    {
                        if (null == retValue)
                            retValue = new List<CallBackInfo>();
                        retValue.Add(_callBacks[indexesTmp2[i]]);
                    }
                }
                return retValue;
            }
        }

        private List<int> GetCallBacksIndex(List<CallBackInfo> callbacks, object defaultSender, CALL_FUNC_EVENT callfunc, bool findOne)
        {
            lock (_callBacks)
            {
                if (callbacks.IsNullOrEmpty())
                    return null;

                var retValue = new List<int>();

                if (defaultSender == null && callfunc == null)
                {
                    return retValue;
                }
                else if (defaultSender != null && callfunc == null)
                {
                    for (int i = callbacks.Count - 1; i >= 0; --i)
                    {
                        if (callbacks[i].DefaultSender.Equals(defaultSender))
                        {
                            retValue.Add(i);
                            if (findOne)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (defaultSender == null && callfunc != null)
                {
                    for (int i = callbacks.Count - 1; i >= 0; --i)
                    {
                        if (callbacks[i].CallFunc == callfunc)
                        {
                            retValue.Add(i);
                            if (findOne)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (defaultSender != null && callfunc != null)
                {
                    for (int i = callbacks.Count - 1; i >= 0; --i)
                    {
                        if (callbacks[i].DefaultSender.Equals(defaultSender) && callbacks[i].CallFunc == callfunc)
                        {
                            retValue.Add(i);
                            if (findOne)
                            {
                                break;
                            }
                        }
                    }
                }
                return retValue;
            }
        }

        private string GetCallBackInformation(CALL_FUNC_EVENT callfunc)
        {
            if (null == callfunc)
            {
                return "<<CallBack is null>>";
            }
            else
            {
                string targetDescription = null == callfunc || null == callfunc.Target ? "null" : callfunc.Target.GetType().ToString();
                string methodDescription = null == callfunc ? "null" : callfunc.Method.ToString();
                return "<<Sender type=" + targetDescription + ">> <<Callfunc method=" + methodDescription + ">>";
            }
        }
    }
}

