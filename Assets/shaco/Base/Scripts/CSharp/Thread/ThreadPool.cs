using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// 线程池管理类
/// </summary>
namespace shaco.Base
{
    public class ThreadPool
    {
        /// <summary>
        /// 设置线程池队列中最大的线程数量，超过该数量的新增线程会持续等待，直到有旧线程退出腾出新的坑位
        /// <param name="count">最大线程数量</param>
        /// </summary>
        static public void SetMaxThreads(int count)
        {
#if UNITY_WEBGL
            shaco.Base.Log.Error("ThreadPool SetMaxThreads exception: unsupport on 'WebGL' platform");
#else
            shaco.Base.Log.Info("ThreadPool SetMaxThreads: count=" + count);
            System.Threading.ThreadPool.SetMaxThreads(count, count);
#endif
        }

        /// <summary>
        /// 获取当前最大内存池数量
        /// </summary>
        static public void GetMaxThreads(out int workerThreads, out int completionPortThreads)
        {
#if UNITY_WEBGL
            workerThreads = completionPortThreads = 0;
#else
            System.Threading.ThreadPool.GetMaxThreads(out workerThreads, out completionPortThreads);
#endif
        }

        /// <summary>
        /// 运行线程
        /// <param name="callbackInvoke">线程内执行方法</param>
        /// </summary>
        static public void RunThread(System.Action callbackInvoke)
        {
#if UNITY_WEBGL
            RunSafeCallback(callbackInvoke);
#else
            // shaco.Base.Log.Info("ThreadPool RunThread: callbackInvoke=" + callbackInvoke);
            System.Threading.ThreadPool.QueueUserWorkItem((object value) =>
            {
                callbackInvoke();
            });
#endif
        }

        static public Thread GetCurrentThread()
        {
#if UNITY_WEBGL
            shaco.Base.Log.Error("ThreadPool GetCurrentThread exception: unsupport on 'WebGL' platform");
            return null;
#else
            var retValue = Thread.CurrentThread;
            shaco.Base.Log.Info("ThreadPool GetCurrentThread: current thread=" + retValue);
            return retValue;
#endif
        }

        /// <summary>
        /// 运行带安全回调的线程
        /// <param name="callbackInvoke">子线程调用，执行任务方法，不允许调用Unity相关函数</param>
        /// <param name="callbackEnd">主线程调用，任务执行完毕回调，可以安全使用Unity相关函数</param>
        /// </summary>
        static public void RunThreadSafeCallBack(System.Action callbackInvoke, System.Action callbackEnd)
        {
            bool isCompleted = false;

            shaco.Base.WaitFor.Run(() =>
            {
                return isCompleted;
            }, () =>
            {
                if (null != callbackEnd)
                {
                    try
                    {
                        callbackEnd();
                    }
                    catch (System.Exception e)
                    {
                        shaco.Base.Log.Error("ThreadPool RunThreadSafeCallBack exception: e=" + e);
                    }
                }
            });

            RunThread(() =>
            {
                callbackInvoke();
                isCompleted = true;
            });
        }

        /// <summary>
        /// 运行完全安全的回到方式，主要是为了适配webgl平台
        /// </summary>
        static private void RunSafeCallback(System.Action callbackInvoke)
        {
            callbackInvoke();
        }
    }
}