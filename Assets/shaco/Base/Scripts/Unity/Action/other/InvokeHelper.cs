using UnityEngine;
using System.Collections;
using System;

namespace shaco
{
    static public class Delay
    {
        static public ActionBase Run(System.Action func, float delaySeconds, GameObject autoReleaseTarget = null)
        {
            ActionBase retValue = null;

            if (null == func)
            {
                Log.Error("InvokeHelper Run error: func is null");
                return null;
            }

            System.Action callbackAutoRelease = null;
            if (null != autoReleaseTarget)
            {
                callbackAutoRelease = () =>
                {
                    if (null != retValue)
                        retValue.StopMe(false);
                };
            }

            retValue = shaco.DelayTime.Create(delaySeconds);
            retValue.onCompleteFunc += (shaco.ActionBase ac) =>
            {
                //当执行完毕后，关闭自动销毁事件
                if (null != autoReleaseTarget)
                {
                    var autoReleaseComponnet = autoReleaseTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
                    if (null != autoReleaseComponnet)
                    {
                        autoReleaseComponnet.RemoveOnDestroyCallBack(autoReleaseTarget, callbackAutoRelease);
                    }
                }

                try
                {
                    shaco.Base.GameHelper.profiler.BeginSample("Delay Run: target=" + func.Target + " method=" + func.Method);
                    func();
                    shaco.Base.GameHelper.profiler.EndSample();
                }
                catch (System.Exception e)
                {
                    Log.Error("InvokeHelper Run error: func=" + func + " e=" + e);
                }
            };

            //当有绑定对象的时候添加自动销毁逻辑
            if (null != autoReleaseTarget)
            {
                var autoReleaseComponnet = autoReleaseTarget.GetOrAddComponent<shaco.UnityObjectAutoReleaseComponent>();
                autoReleaseComponnet.AddOnDestroyCallBack(autoReleaseTarget, callbackAutoRelease);
            }
            retValue.RunAction(autoReleaseTarget == null ? shaco.GameHelper.action.GetGlobalInvokeTarget().gameObject : autoReleaseTarget);
            return retValue;
        }

        //func: loopCount, isNextLoop
        static public ActionBase RunRepeat(Action<int> func, float delaySeconds, float intervalSeconds, int loop, GameObject autoReleaseTarget = null)
        {
            Repeat retValue = null;

            if (null == func)
            {
                Log.Error("InvokeHelper RunRepeat error: func is null");
                return null;
            }

            System.Action callbackAutoRelease = null;
            if (null != autoReleaseTarget)
            {
                callbackAutoRelease = () =>
                {
                    if (null != retValue)
                        retValue.StopMe(false);
                };
            }

            //初次循环使用延迟时间
            var actionTmp = shaco.DelayTime.Create(delaySeconds);

            if (loop < 0)
                retValue = Repeat.CreateRepeatForever(actionTmp);
            else
                retValue = Repeat.Create(actionTmp, loop);
            retValue.onLoopCompleteFunc = (int loopCount) =>
            {
                var currentLoopTimes = retValue.GetCurrentLoop();

                //从第二回合循环开始就使用间隔时间了
                if (currentLoopTimes > 0)
                {
                    actionTmp.SetDuration(intervalSeconds);
                }

                //当执行完毕后，关闭自动销毁事件
                if (null != autoReleaseTarget && loop >= 0 && currentLoopTimes > loop)
                {
                    var autoReleaseComponnet = autoReleaseTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
                    if (null != autoReleaseComponnet)
                    {
                        autoReleaseComponnet.RemoveOnDestroyCallBack(autoReleaseTarget, callbackAutoRelease);
                    }
                }

                try
                {
                    shaco.Base.GameHelper.profiler.BeginSample("Delay RunRepeat: target=" + func.Target + " method=" + func.Method);
                    func(currentLoopTimes);
                    shaco.Base.GameHelper.profiler.EndSample();
                }
                catch (System.Exception e)
                {
                    Log.Error("InvokeHelper RunRepeat error: func=" + func + " e=" + e);
                }
            };

            //当有绑定对象的时候添加自动销毁逻辑
            if (null != autoReleaseTarget)
            {
                var autoReleaseComponnet = autoReleaseTarget.GetOrAddComponent<shaco.UnityObjectAutoReleaseComponent>();
                autoReleaseComponnet.AddOnDestroyCallBack(autoReleaseTarget, callbackAutoRelease);
            }
            retValue.RunAction(autoReleaseTarget == null ? shaco.GameHelper.action.GetGlobalInvokeTarget().gameObject : autoReleaseTarget);
            return retValue;
        }

        static public void CancelAllInvoke()
        {
            var delegateTmp = shaco.GameHelper.action.GetGlobalInvokeTarget().gameObject;
            if (delegateTmp != null)
                shaco.GameHelper.action.StopAllAction(delegateTmp);
        }
    }

    //该方法效率不是很高，建议全部使用shaco.Base.WaitFor替代
    public static class WaitFor
    {
        static public ActionBase Run(System.Func<bool> callbackIn, System.Action callbackOut, GameObject autoReleaseTarget = null)
        {
            ActionBase retValue = null;
            System.Action callbackAutoRelease = null;

            if (null == callbackIn)
            {
                Log.Error("InvokeHelper RunRepeat error: callbackIn is null");
                return null;
            }

            if (null == callbackOut)
            {
                Log.Error("InvokeHelper RunRepeat error: callbackOut is null");
                return null;
            }

            if (null != autoReleaseTarget)
            {
                callbackAutoRelease = () =>
                {
                    if (null != retValue)
                        retValue.StopMe(false);
                };
            }

            var repeat = Repeat.CreateRepeatForever(DelayTime.Create(10.0f));
            repeat.onFrameFunc += (float percent) =>
            {
                bool result;
                try
                {
                    shaco.Base.GameHelper.profiler.BeginSample("WaitFor Run callbackIn: target=" + callbackIn.Target + " method=" + callbackIn.Method);
                    result = !repeat.isRemoved && callbackIn();
                    shaco.Base.GameHelper.profiler.EndSample();
                }
                catch (System.Exception e)
                {
                    Log.Error("WaitFor Run error: callbackIn=" + callbackIn + " e=" + e);
                    if (null != repeat)
                        repeat.StopMe();
                    return;
                }

                if (result)
                {
                    //当执行完毕后，关闭自动销毁事件
                    if (null != autoReleaseTarget)
                    {
                        var autoReleaseComponnet = autoReleaseTarget.GetComponent<shaco.UnityObjectAutoReleaseComponent>();
                        if (null != autoReleaseComponnet)
                        {
                            autoReleaseComponnet.RemoveOnDestroyCallBack(autoReleaseTarget, callbackAutoRelease);
                        }
                    }

                    repeat.StopMe();

                    try
                    {
                        shaco.Base.GameHelper.profiler.BeginSample("WaitFor Run callbackOut: target=" + callbackOut.Target + " method=" + callbackOut.Method);
                        callbackOut();
                        shaco.Base.GameHelper.profiler.EndSample();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("WaitFor Run error: callbackOut=" + callbackOut + " e=" + e);
                    }
                }
            };

            //当有绑定对象的时候添加自动销毁逻辑
            if (null != autoReleaseTarget)
            {
                var autoReleaseComponnet = autoReleaseTarget.GetOrAddComponent<shaco.UnityObjectAutoReleaseComponent>();
                autoReleaseComponnet.AddOnDestroyCallBack(autoReleaseTarget, callbackAutoRelease);
            }
            repeat.RunAction(autoReleaseTarget == null ? shaco.GameHelper.action.GetGlobalInvokeTarget().gameObject : autoReleaseTarget);
            return repeat;
        }
    }
}

static public class shaco_ExtensionsInvokeHelper
{
    static public shaco.ActionBase Delay(this GameObject autoReleaseTarget, Action func, float delaySeconds)
    {
        return shaco.Delay.Run(func, delaySeconds, autoReleaseTarget);
    }

    static public shaco.ActionBase Delay(this Component autoReleaseTarget, Action func, float delaySeconds)
    {
        return shaco.Delay.Run(func, delaySeconds, autoReleaseTarget.gameObject);
    }

    static public shaco.ActionBase DelayRepeat(this GameObject autoReleaseTarget, Action<int> func, float delaySeconds, float intervalSeconds, int loop)
    {
        return shaco.Delay.RunRepeat(func, delaySeconds, intervalSeconds, loop, autoReleaseTarget);
    }

    static public shaco.ActionBase DelayRepeat(this Component autoReleaseTarget, Action<int> func, float delaySeconds, float intervalSeconds, int loop)
    {
        return shaco.Delay.RunRepeat(func, delaySeconds, intervalSeconds, loop, autoReleaseTarget.gameObject);
    }

    static public shaco.ActionBase DelayRepeatForever(this GameObject autoReleaseTarget, Action<int> func, float delaySeconds, float intervalSeconds)
    {
        return shaco.Delay.RunRepeat(func, delaySeconds, intervalSeconds, -1, autoReleaseTarget);
    }

    static public shaco.ActionBase DelayRepeatForever(this Component autoReleaseTarget, Action<int> func, float delaySeconds, float intervalSeconds)
    {
        return shaco.Delay.RunRepeat(func, delaySeconds, intervalSeconds, -1, autoReleaseTarget.gameObject);
    }

    static public shaco.ActionBase WaitFor(this GameObject autoReleaseTarget, System.Func<bool> callbackIn, System.Action callbackOut)
    {
        return shaco.WaitFor.Run(callbackIn, callbackOut, autoReleaseTarget);
    }

    static public shaco.ActionBase WaitFor(this Component autoReleaseTarget, System.Func<bool> callbackIn, System.Action callbackOut)
    {
        return shaco.WaitFor.Run(callbackIn, callbackOut, autoReleaseTarget.gameObject);
    }
}