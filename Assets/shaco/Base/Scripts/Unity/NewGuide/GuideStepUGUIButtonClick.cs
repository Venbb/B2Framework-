using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco
{
    public sealed class GuideStepUGUIButtonClick : shaco.Base.GuideStepDefault
    {
        /// <summary>
        /// 按钮对象名字
        /// </summary>
        [SerializeField]
        private GuideRuntimeObject<UnityEngine.UI.Button> _buttonTarget = null;

        //测试动画
        private shaco.ActionBase _testAction = null;

        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        override public void Execute() 
        {
            if (null == _buttonTarget || null == _buttonTarget.value)
            {
                Log.Error("GuideStepUGUIButtonClick Execute error: not found gameobject, name=" + _buttonTarget);
                return;
            }

            var buttonComponent = (UnityEngine.UI.Button)_buttonTarget;
            if (null == buttonComponent)
            {
                Log.Error("GuideStepUGUIButtonClick Execute error: not found component 'UnityEngine.UI.Button'");
                return;
            }

            //播放当前按钮放大缩小动画
            var scale1 = shaco.ScaleTo.Create(new Vector3(1.2f, 1.2f, 1.2f), 1.0f);
            var scale2 = shaco.ScaleTo.Create(new Vector3(1, 1, 1), 1.0f);
            var sequeueAction = shaco.Sequeue.Create(scale1, scale2);
            var reapeatAction = shaco.Repeat.CreateRepeatForever(sequeueAction);
            reapeatAction.RunAction(buttonComponent.gameObject);
            _testAction = reapeatAction;

            //监听ui点击事件
            buttonComponent.onClick.RemoveListener(OnClickCallBack);
            buttonComponent.onClick.AddListener(OnClickCallBack);
        }

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        override public void End()
        {
            var buttonComponent = (UnityEngine.UI.Button)_buttonTarget;
            if (null != buttonComponent)
                buttonComponent.onClick.RemoveListener(OnClickCallBack);

            if (null != _testAction)
            {
                _testAction.StopMe();
                _testAction = null;
            }
        }

        private void OnClickCallBack()
        {
            shaco.GameHelper.newguide.OnGuideStepCompleted(this);
        }
    }
}