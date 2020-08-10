using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class GuideStepPlaceholder : shaco.Base.GuideStepDefault
    {
        /// <summary>
        /// 新手引导执行逻辑
        /// </summary>
        override public void Execute()
        {
            GameHelper.newguide.OnGuideStepCompleted(this);
        }

        /// <summary>
        /// 新手引导执行完毕逻辑
        /// </summary>
        override public void End()
        {
        }
    }
}