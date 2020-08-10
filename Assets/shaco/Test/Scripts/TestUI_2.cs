using UnityEngine;
using System.Collections;
using System;

namespace shaco.Test
{
    [shaco.UILayerIndex(0)]
    [shaco.UILayerAllowDuplicate]
    [shaco.UILayerOpenAsync]
    [shaco.UILayerCustomPrefabPath("Test/haha.prefab")]
    [shaco.UILayerMultiVersionControlRelativePath(TestResourceUpdate.multiVersionControlRelativePath)]
    [shaco.UILayerFullScreen]
    public class TestUI_2 : MonoBehaviour, shaco.IUIAnimation
    {
        static private int uiIndexAll = 0;
        private int uiIndexMe = 0;

        //这两种写法都是可以正常回调的，如果定义了TestArg类型则会将shaco.Base.BaseEventArg自动转换为TestArg
        //ps: 需要注意的是如果直接使用TestArg则只能传递TestArg或者继承自TestArg的参数，否则会调用该方法(OnUIPreLoad)失败
        // void OnUIPreLoad(shaco.Base.BaseEventArg arg)
        void OnUIPreLoad()
        {
            shaco.Log.Info("TestUI_2 OnUIPreLoad", this);
        }

        void OnUIInit(TestArg arg)
        {
            var testArg = arg as TestArg;
            uiIndexMe = uiIndexAll++;
            shaco.Log.Info("TestUI_2 OnUIInit====" + testArg.message + " name=" + this.name + " index=" + (uiIndexMe), this);
            this.name = uiIndexMe.ToString();
        }

        void OnUIOpen(TestArg arg)
        {
            shaco.Log.Info("TestUI_2 OnUIOpen====" + arg.message + " name=" + this.name + " index=" + uiIndexMe, this);
        }

        void OnUIHide()
        {
            //挂载UILayerFullScreen作为全屏ui后，当一个全屏ui打开时候会自动隐藏最上层全屏属性ui，这个时候的arg是空的，所有需要判断下
            shaco.Log.Info("TestUI_2 OnUIHide", this);
        }

        void OnUIResume()
        {
            //挂载UILayerFullScreen作为全屏ui后，当一个全屏ui隐藏或者关闭时候会自动打开最上层全屏属性ui，这个时候的arg是空的，所有需要判断下
            shaco.Log.Info("TestUI_2 OnUIResume", this);
        }

        void OnUIClose()
        {
            shaco.Log.Info("TestUI_2 OnUIClose", this);
        }

        void OnUIRefresh(TestArg arg)
        {
            shaco.Log.Info("TestUI_2 OnUIRefresh====" + arg.message + " name=" + this.name + " index=" + uiIndexMe, this);
        }

        void OnUIBringToFront()
        {
            //挂载UILayerFullScreen作为全屏ui后，当一个全屏ui隐藏或者关闭时候会自动打开最上层全屏属性ui，这个时候的arg是空的，所有需要判断下
            shaco.Log.Info("TestUI_2 OnUIBringToFront", this);
        }

        public void OnClickHideMe()
        {
            //用下面的方法更加优雅
            // shaco.GameHelper.ui.HideUITarget<TestUI_2>(this, new TestArg());
            this.HideMe();
        }

        public void OnClickCloseMe()
        {
            //用下面的方法更加优雅
            // shaco.GameHelper.ui.CloseUITarget<TestUI_2>(this, new TestArg());
            this.CloseMe();
        }

        void Start()
        {

        }

        public void RunOpenAnimation(System.Action callbackEnd)
        {
            this.gameObject.RotateBy(new Vector3(0, 0, -90f), 0.5f)
                            .RotateBy(new Vector3(0, 0, 90), 0.5f)
                            .onCompleteFunc += (ac) =>
                            {
                                callbackEnd();
                            };
        }

        public void RunCloseAnimation(System.Action callbackEnd)
        {
            this.gameObject.RotateBy(new Vector3(0, 0, 90f), 0.5f)
                           .RotateBy(new Vector3(0, 0, -90), 0.5f)
                           .onCompleteFunc += (ac) =>
                           {
                               callbackEnd();
                           };
        }

        // public void RunOpenAnimation(Action callbackEnd)
        // {
        //     this.gameObject.ScaleTo(new Vector3(0.1f, 0.1f, 0.1f), 0)
        //                     .ScaleTo(new Vector3(1.0f, 1.0f, 1.0f), 0.5f)
        //                     .onCompleteFunc += (ac) =>
        //                     {
        //                         callbackEnd();
        //                     };
        // }

        // public void RunCloseAnimation(Action callbackEnd)
        // {
        //     this.gameObject.ScaleTo(new Vector3(1.0f, 1.0f, 1.0f), 0)
        //                     .ScaleTo(new Vector3(0.1f, 0.1f, 0.1f), 0.5f)
        //                     .onCompleteFunc += (ac) =>
        //                     {
        //                         callbackEnd();
        //                     };
        // }
    }
}