using System.Collections;
using System.Collections.Generic;

namespace shaco
{
    public interface IUIAnimation
    {
        /// <summary>
        /// 播放UI打开的切换过渡动画
        /// *注意：一定要在合适时机执行callbackEnd回调来通知动画结束，否则会导致UI卡死*
        /// <param name="callbackEnd">动画结束回调</param>
        /// </summary>
        void RunOpenAnimation(System.Action callbackEnd);

        /// <summary>
        /// 播放UI关闭的切换过渡动画
        /// *注意：一定要在合适时机执行callbackEnd回调来通知动画结束，否则会导致UI卡死*
        /// <param name="callbackEnd">动画结束回调</param>
        /// </summary>
        void RunCloseAnimation(System.Action callbackEnd);
    }
}