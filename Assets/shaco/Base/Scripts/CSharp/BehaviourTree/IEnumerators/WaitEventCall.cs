namespace shaco.Base
{
    public class WaitEventCall : shaco.Base.IBehaviourEnumerator
    {
        private bool isEventCalled = false;
        private bool isOnceInvokeEvent = false;

        /// <summary>
        /// 创建事件等待对象
        /// <param name="T">事件类型</param>
        /// <param name="bindTarget">事件绑定对象，以便统一销毁</param>
        /// <param name="invokeOnce">是否只执行1次</param>
        /// <return></return>
        /// </summary>
        static public WaitEventCall Create<T>(bool invokeOnce) where T : shaco.Base.BaseEventArg
        {
            var retValue = CreateWithPool(() => new WaitEventCall());
            retValue.isOnceInvokeEvent = invokeOnce;
            if (invokeOnce)
            {
                retValue.AddOnceEvent<T>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
            }
            else
            {
                retValue.AddEvent<T>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
            }
            return retValue;
        }

        /// <summary>
        /// 创建事件等待对象
        /// <param name="T">事件类型</param>
        /// <param name="bindTarget">事件绑定对象，以便统一销毁</param>
        /// <param name="invokeOnce">是否只执行1次</param>
        /// <return></return>
        /// </summary>
        static public WaitEventCall Create<T1, T2>(bool invokeOnce) where T1 : shaco.Base.BaseEventArg where T2 : shaco.Base.BaseEventArg
        {
            var retValue = CreateWithPool(() => new WaitEventCall());
            retValue.isOnceInvokeEvent = invokeOnce;
            if (invokeOnce)
            {
                retValue.AddOnceEvent<T1>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
                retValue.AddOnceEvent<T2>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
            }
            else
            {
                retValue.AddEvent<T1>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
                retValue.AddEvent<T2>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
            }
            return retValue;
        }

        /// <summary>
        /// 创建事件等待对象
        /// <param name="T">事件类型</param>
        /// <param name="bindTarget">事件绑定对象，以便统一销毁</param>
        /// <param name="invokeOnce">是否只执行1次</param>
        /// <return></return>
        /// </summary>
        static public WaitEventCall Create<T1, T2, T3>(bool invokeOnce) where T1 : shaco.Base.BaseEventArg where T2 : shaco.Base.BaseEventArg where T3 : shaco.Base.BaseEventArg
        {
            var retValue = CreateWithPool(() => new WaitEventCall());
            retValue.isOnceInvokeEvent = invokeOnce;
            if (invokeOnce)
            {
                retValue.AddOnceEvent<T1>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
                retValue.AddOnceEvent<T2>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
                retValue.AddOnceEvent<T3>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
            }
            else
            {
                retValue.AddEvent<T1>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
                retValue.AddEvent<T2>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
                retValue.AddEvent<T3>((sender, arg) =>
                {
                    retValue.isEventCalled = true;
                });
            }
            return retValue;
        }

        public override bool IsRunning()
        {
            return !isEventCalled;
        }

        public override void Reset()
        {
            isEventCalled = false;
            this.RemoveAllEvent();
        }

        private WaitEventCall() {}
    }
}

