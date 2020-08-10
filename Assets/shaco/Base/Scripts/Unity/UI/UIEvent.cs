using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace shaco
{
    public class UIEvent
    {
        public enum EventType
        {
            None,
            OnPreLoad,
            OnInit,
            OnOpen,
            OnHide,
            OnResume,
            OnClose,
            OnBringToFront,
            OnRefresh,

            EventCount
        }

        public class UIEventInfo
        {
            public shaco.Base.StackLocation stackLocationUI = new shaco.Base.StackLocation();
        }

        public UIEventInfo[] uiEventInfo { get { return _uiEventInfo; }}
        public UIEventInfo[] _uiEventInfo = new UIEventInfo[(int)EventType.EventCount];

        public UIEvent()
        {
            RestStackLocations();
        }

        public void RestStackLocations()
        {
            for (int i = this._uiEventInfo.Length - 1; i >= 0; --i)
            {
                if (this._uiEventInfo[i] == null)
                    this._uiEventInfo[i] = new UIEventInfo();
                else
                {
                    this._uiEventInfo[i].stackLocationUI.Reset();
                }
            }
        }
        
        public void GetOnUIInitStatck()
        {
            var uiEventInfo = this._uiEventInfo[(int)EventType.OnInit];
            uiEventInfo.stackLocationUI.GetStack();
        }

        public void GetOnUIOpenStatck()
        {
            var uiEventInfo = this._uiEventInfo[(int)EventType.OnOpen];
            uiEventInfo.stackLocationUI.GetStack();
        }

        public bool DispatchEvent(shaco.IUIState uiState, shaco.Base.BaseEventArg arg, EventType type)
        {
            bool hasError = false;
            for (int i = uiState.uiPrefabs.Count - 1; i >= 0; --i)
            {
                var prefabTmp = uiState.uiPrefabs[i];
                hasError |= !DispatchEvent(uiState.key, prefabTmp, arg, type);
            }
            return !hasError;
        }

        public bool DispatchEvent(string key, UIPrefab uiPrefab, shaco.Base.BaseEventArg arg, EventType type)
        {
            var uiEventInfo = this._uiEventInfo[(int)type];
            var methodTarget = uiPrefab.prefab;

            //dont dispatch event when target is inactive in hierarchy
            if (null == methodTarget && type != EventType.OnPreLoad)
            {
                return false;
            }

            //第一次调用ui事件需要初始ui方法
            if (type == EventType.OnPreLoad || type == EventType.OnInit)
            {
                if (!uiPrefab.isInited)
                {
                    uiPrefab.isInited = true;
                    InitEventMethod(uiPrefab);
                }
            }

            //因为OnUIOpen支持异步操作比较特殊，这里获取的stack并不准确
            if (type != EventType.OnOpen && type != EventType.OnInit)
            {
                uiEventInfo.stackLocationUI.GetStack();
            }
            uiEventInfo.stackLocationUI.StartTimeSpanCalculate("UIEvent DispatchEvent: type=" + type + " key=" + key);

            switch (type)
            {
                case EventType.OnPreLoad: InvokeMethod(methodTarget, uiPrefab.methodOnPreLoad, arg); break;
                case EventType.OnInit: InvokeMethod(methodTarget, uiPrefab.methodOnInit, arg); break;
                case EventType.OnOpen: InvokeMethod(methodTarget, uiPrefab.methodOnOpen, arg); break;
                case EventType.OnHide: InvokeMethod(methodTarget, uiPrefab.methodOnHide, arg); break;
                case EventType.OnResume: InvokeMethod(methodTarget, uiPrefab.methodOnResume, arg); break;
                case EventType.OnClose: InvokeMethod(methodTarget, uiPrefab.methodOnClose, arg); break;
                case EventType.OnRefresh: InvokeMethod(methodTarget, uiPrefab.methodOnRefresh, arg); break;
                case EventType.OnBringToFront: InvokeMethod(methodTarget, uiPrefab.methodOnBringToFront, arg); break;
                default: Log.Error("DispatchEvent error: unsupport event type=" + type, methodTarget); break;
            }

            uiEventInfo.stackLocationUI.StopTimeSpanCalculate();

            UIStateChangeSave.SaveUIStateChangedInfo(key, uiPrefab, uiEventInfo.stackLocationUI, type);
            DispatchUIStateChangedEvent(key, uiPrefab.mainComponent, type);
            return true;
        }
        
        private void DispatchUIStateChangedEvent(string key, Component mainComponent, EventType type)
        {
            UIStateChangedEvents.OnUIStateChangedBaseEvent dispatchArgTmp = null;

            switch (type)
            {
                case EventType.OnPreLoad:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIPreLoadEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUIPreLoadEvent();
                        break;
                    }
                case EventType.OnInit:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIInitEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUIInitEvent();
                        break;
                    }
                case EventType.OnOpen:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIOpenEvent>())
                        {
                            dispatchArgTmp = new UIStateChangedEvents.OnUIOpenEvent();
                        }
                        break;
                    }
                case EventType.OnHide:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIHideEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUIHideEvent();
                        break;
                    }
                case EventType.OnResume:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIResumeEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUIResumeEvent();
                        break;
                    }
                case EventType.OnClose:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUICloseEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUICloseEvent();
                        break;
                    }
                case EventType.OnRefresh:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIRefreshEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUIRefreshEvent();
                        break;
                    }
                case EventType.OnBringToFront:
                    {
                        if (shaco.GameHelper.Event.HasEventID<UIStateChangedEvents.OnUIBringToFrontEvent>())
                            dispatchArgTmp = new UIStateChangedEvents.OnUIBringToFrontEvent();
                        break;
                    }
                default: Log.Error("DispatchUIStateChangedEvent error: unsupport event type=" + type, mainComponent); break;
            }

            if (null != dispatchArgTmp)
            {
                dispatchArgTmp.uiKey = key;
                dispatchArgTmp.uiTarget = mainComponent;
                dispatchArgTmp.uiTarget.InvokeEvent(dispatchArgTmp);
            }
        }

        private void InitEventMethod(shaco.UIPrefab uiPrefab)
        {
            bool haveMethod = false;

            //clear all invoke method
            uiPrefab.ClearAllMethod();

            for (int j = uiPrefab.componets.Length - 1; j >= 0; --j)
            {
                var componet = uiPrefab.componets[j];
                haveMethod |= SaveMethod(uiPrefab.methodOnPreLoad, componet, "OnUIPreLoad", false);
                haveMethod |= SaveMethod(uiPrefab.methodOnInit, componet, "OnUIInit", true);
                haveMethod |= SaveMethod(uiPrefab.methodOnOpen, componet, "OnUIOpen", true);
                haveMethod |= SaveMethod(uiPrefab.methodOnHide, componet, "OnUIHide", false);
                haveMethod |= SaveMethod(uiPrefab.methodOnResume, componet, "OnUIResume", false);
                haveMethod |= SaveMethod(uiPrefab.methodOnClose, componet, "OnUIClose", false);
                haveMethod |= SaveMethod(uiPrefab.methodOnRefresh, componet, "OnUIRefresh", true);
                haveMethod |= SaveMethod(uiPrefab.methodOnBringToFront, componet, "OnUIBringToFront", false);
            }

            //不再需要这个警告了
            // if (!haveMethod)
            // {
            //     Log.Warning("UIEvent InitEventMethod error: no method found, target=" + uiPrefab.prefab);
            // }
        }

        private void InvokeMethod(object target, List<MethodInfoEx> methods, shaco.Base.BaseEventArg arg)
        {
            for (int i = methods.Count - 1; i >= 0; --i)
            {
                var method = methods[i];
                if (null == method || null == method.method)
                    Log.Error("UIEvent InvokeMethod error: method is missing, target=" + target, target);
                else
                {
                    var argsType = method.method.GetParameters();
                    if (!argsType.IsNullOrEmpty())
                    {
                        //当UI方法参数大于1的后续参数是无效的
                        if (argsType.Length > 1)
                        {
                            Log.Error("UIEvent InvokeMethod error: Only the first parameter is valid, and next parameters will be automatically ignored, target=" + target + " method=" + method.method, method.target);
                        }

                        //当参数不是基类shaco.Base.BaseEventArg的时候，需要检测传入参数与方法参数类型是否匹配
                        if (argsType[0].ParameterType != typeof(shaco.Base.BaseEventArg) && (null != arg && !arg.GetType().IsInherited(argsType[0].ParameterType)))
                            shaco.Log.Error("UIEvent InvokeMethod erorr: not parid parameter, need type=" + argsType[0].ParameterType + " invoke type=" + (null == arg ? "null type" : arg.GetType().FullName) + " target=" + target + " method=" + method.method, method.target);
                        else
                        {
                            try
                            {
                                method.method.Invoke(method.target, new object[] { arg });
                            }
                            catch (System.Exception e)
                            {
                                shaco.Log.Error("UIEvent InvokeMethod Param error: target=" + method.target + " method=" + method.method.Name + " e=" + e, method.target);
                            }
                        }
                    }
                    else
                    {
                        if (null != arg)
                        {
                            var serializedInfo = shaco.Base.Utility.GetSerializableInformation(arg);
                            Log.Error("UIEvent InvokeMethod NoParam error: Method does not need any parameters, it will be automatically ignored, target=" + target + " method=" + method.method + "\narg=" + serializedInfo, method.target);
                        }

                        try
                        {
                            method.method.Invoke(method.target, null);
                        }
                        catch (System.Exception e)
                        {
                            shaco.Log.Error("UIEvent InvokeMethod NoParam error: target=" + method.target + " method=" + method.method.Name + " e=" + e, method.target);
                        }
                    }
                }
            }
        }

        private bool SaveMethod(List<MethodInfoEx> methods, object target, string methodName, bool needParameter)
        {
            if (null == target)
                return false;

            
            BindingFlags flag = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

            var method = target.GetType().GetMethod(methodName, flag);
            if (null != method)
            {
                var newMethod = new MethodInfoEx();
                newMethod.target = target;
                newMethod.method = method;

                var argsType = newMethod.method.GetParameters();
                if (needParameter)
                {
                    if (argsType.IsNullOrEmpty())
                        shaco.Log.Error("UIEvent SaveMethod erorr: Method need parameter, target=" + target + " method=" + methodName, target);
                    else if (argsType.Length != 1)
                        shaco.Log.Error("UIEvent SaveMethod erorr: Method parameter length should be 1, len" + argsType.Length + " target=" + target + " method=" + methodName, target);
                }
                else if (!argsType.IsNullOrEmpty())
                    shaco.Log.Error("UIEvent SaveMethod erorr: Method does not need any parameters, target=" + target + " method=" + methodName, target);
                methods.Add(newMethod);
            }
            return methods.Count > 0;
        }
    }
}