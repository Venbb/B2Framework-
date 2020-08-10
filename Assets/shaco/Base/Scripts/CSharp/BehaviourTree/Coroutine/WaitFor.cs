using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    public class WaitFor
    {
        private class InvokeInfo
        {
            public System.Func<bool> callbackIn = null;
            public System.Action callbackOut = null;

        }
        static private List<InvokeInfo> _invokes = new List<InvokeInfo>();

        /// <summary>
        /// 计时器刷新函数
        /// </summary>
        static public void Update()
        {
            if (_invokes.Count > 0)
            {
                for (int i = _invokes.Count - 1; i >= 0; --i)
                {
                    var invokeInfo = _invokes[i];
                    bool result = false;

                    try
                    {
                        result = invokeInfo.callbackIn();
                    }
                    catch (System.Exception e)
                    {
                        Log.Error("WaitFor exception: InTarget" + invokeInfo.callbackIn.Target + " InMethod" + invokeInfo.callbackIn.Method + " e=" + e);
                        _invokes.RemoveAt(i);
                        continue;
                    }
                    if (result)
                    {
                        try
                        {
                            _invokes.RemoveAt(i);

                            GameHelper.profiler.BeginSample("WaitFor callbackOut: target=" + invokeInfo.callbackOut.Target + " method=" + invokeInfo.callbackOut.Method);
                            {
                                invokeInfo.callbackOut();
                            }
                            GameHelper.profiler.EndSample();
                        }
                        catch (System.Exception e)
                        {
                            Log.Error("WaitFor exception: OutTarget" + invokeInfo.callbackOut.Target + " OutMethod" + invokeInfo.callbackOut.Method + " e=" + e);
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 开启一个条件计时器
        /// <param name="callbackIn">条件判断方法</param>
        /// <param name="callbackOut">条件执行方法，条件判断方法返回true的时候执行</param>
        /// </summary>
        static public void Run(System.Func<bool> callbackIn, System.Action callbackOut)
        {
            //如果非unity引擎，需要手动调用shaco.Base.WaitFor.Update来执行计时器
            _invokes.Add(new InvokeInfo() { callbackIn = callbackIn, callbackOut = callbackOut });
        }
    }
}
