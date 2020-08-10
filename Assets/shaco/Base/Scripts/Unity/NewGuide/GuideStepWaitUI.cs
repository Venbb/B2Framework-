using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public class GuideStepWaitUI : shaco.Base.GuideStepDefault
    {
        [SerializeField]
        [Header("描述")]
        private string _descrption;

        [SerializeField]
        [Header("需要等待加载的界面名字")]
        private string _waitUIName = string.Empty;

        [SerializeField]
        [Header("界面触发条件")]
        private shaco.UIEvent.EventType _when = shaco.UIEvent.EventType.None;

        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        override public void Execute()
        {
            switch (_when)
            {
                case shaco.UIEvent.EventType.OnPreLoad:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUIPreLoadEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnPreLoad, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnInit:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUIInitEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnInit, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnOpen:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUIOpenEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnOpen, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnHide:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUIHideEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnHide, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnResume:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUIResumeEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnResume, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnClose:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUICloseEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnClose, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnBringToFront:
                    {
                        //如果已经在最上层了，则直接返回
                        if (shaco.GameHelper.ui.IsTopUI(_waitUIName))
                            ExecuteGuideStep(shaco.UIEvent.EventType.OnBringToFront, new shaco.UIStateChangedEvents.OnUIBringToFrontEvent() { uiKey = _waitUIName });
                        else
                            this.AddEvent<shaco.UIStateChangedEvents.OnUIBringToFrontEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnBringToFront, arg));
                        break;
                    }
                case shaco.UIEvent.EventType.OnRefresh:
                    {
                        this.AddEvent<shaco.UIStateChangedEvents.OnUIRefreshEvent>((sender, arg) => ExecuteGuideStep(shaco.UIEvent.EventType.OnRefresh, arg));
                        break;
                    }
                default: shaco.Log.Error("GuideStepWaitUI Execute error: unsupport type=" + _when); break;
            }
        }

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        override public void End()
        {
            this.RemoveAllEvent();
        }

        private void ExecuteGuideStep(shaco.UIEvent.EventType status, shaco.UIStateChangedEvents.OnUIStateChangedBaseEvent arg)
        {
            if (status == _when && arg.uiKey == this._waitUIName)
            {
                shaco.GameHelper.newguide.OnGuideStepCompleted(this);
            }
        }
    }
}