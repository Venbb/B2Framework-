using System.Collections;
using System;

namespace shaco.Base
{
    public class CalculateTime
    {
        /// <summary>
        /// 当超时的时候自动关闭计时器
        /// </summary>
        public bool AutoCloseTimerWhenTimeout = true;

        private long _lCurTimeTicks = 0;
        private long _lStartTimeTicks = 0;
        private long _lPrevTimeTicks = 0;
        private long _lLastUpdateTime = 0;
        private TimeSpan _timeout = new TimeSpan(0, 0, 10);
        private bool _IsTimeout = false;
        private bool _isRequestStop = false;

        /// <summary>
        /// 过期时间(秒)
        /// </summary>
        public double timeoutSeconds
        {
            get
            {
                return ((double)_timeout.Ticks / 10000000);
            }
            set
            {
                if (value <= 0)
                    return;

                _timeout = new TimeSpan(0, 0, 0, 0, (int)value * 1000);
            }
        }

        /// <summary>
        /// 开始计时
        /// </summary>
        public void Start()
        {
            ResetTimeout();

            _lLastUpdateTime = _lStartTimeTicks = DateTime.Now.Ticks;
            _lPrevTimeTicks = DateTime.Now.Ticks;
            _isRequestStop = false;
            _IsTimeout = false;

            shaco.Base.WaitFor.Run(CallBackIn, CallBackOut);
        }

        /// <summary>
        /// 重置超时计算
        /// </summary>
        public void ResetTimeout()
        {
            _lCurTimeTicks = DateTime.Now.Ticks;
        }

        /// <summary>
        /// 重置已经经过的时间
        /// </summary>
        public void ResetEplaseTime()
        {
            _lLastUpdateTime = _lStartTimeTicks = System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public void Stop()
        {
            _isRequestStop = true;
        }

        //是否超时
        public bool IsTimeout()
        {
            return _IsTimeout;
        }

        /// <summary>
        /// 已经经过的时间(单位:秒)
        /// </summary>
        /// <returns>The time seconds.</returns>
        public double GetElapseTimeSeconds()
        {
            long ticksOffset = _lLastUpdateTime - _lStartTimeTicks;
            double ret = ((double)ticksOffset / 10000000.0);
            return ret;
        }

        /// <summary>
        /// 当前运行的间隔时间
        /// </summary>
        /// <returns>The interval time seconds.</returns>
        public double GetIntervalTimeSeconds()
        {
            long ticks = DateTime.Now.Ticks - _lPrevTimeTicks;
            double ret = (double)ticks / 10000000;
            _lPrevTimeTicks = DateTime.Now.Ticks;
            return ret;
        }

        /// <summary>
        /// 检查是否过期
        /// </summary>
        /// <returns></returns>
        private bool CheckTimeout()
        {
            double timeoutSecondsTmp = this.timeoutSeconds;

            //如果超时时间小于等于0，则视为不使用超时时间判断 
            if (timeoutSecondsTmp <= 0)
                return false;
            long tickTmp = DateTime.Now.Ticks - _lCurTimeTicks;
            return (double)(tickTmp) / 10000000 >= timeoutSecondsTmp;
        }

        private bool CallBackIn()
        {
            if (_isRequestStop)
            {
                _isRequestStop = false;
                return true;
            }
            try
            {
                if (CheckTimeout())
                {
                    _IsTimeout = true;

                    if (AutoCloseTimerWhenTimeout)
                        return true;
                }
                _lLastUpdateTime = DateTime.Now.Ticks;
            }
            catch (Exception e)
            {
                Log.Error("CalculateTimeout catch a exception =" + e);
                Stop();
            }
            return false;
        }

        private void CallBackOut()
        {
            //do nothing
        }
    }
}