using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace shaco.Test
{
    public class TestGuideStepRotate : shaco.Base.GuideStepDefault
    {
        [Header("点击按钮后停止动画")]
        [SerializeField]
        private shaco.GuideRuntimeObject<UnityEngine.UI.Button> button = null;

        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        override public void Execute() 
        {
            button.value.gameObject.RotateBy(new Vector3(0, 0, 360), 3.0f).RepeatForever();
            button.value.onClick.AddListener(() => 
            {
                this.OnGuideStepCompleted();
            });
        }

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        override public void End()
        {
            button.value.onClick.RemoveAllListeners();
            button.value.gameObject.StopAllAction();
        }
    }
}