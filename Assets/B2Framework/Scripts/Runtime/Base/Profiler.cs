using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace B2Framework
{
    public class Profiler
    {
        /// <summary>
        /// 缓存起来的监测器Wachter~
        /// </summary>
        private static Dictionary<string, Stopwatch> m_WachterDictionary = null;

        /// <summary>
        /// Watcher内存埋点
        /// </summary>
        private static Dictionary<Stopwatch, long> m_WachterMems = null;

        /// <summary>
        /// 是否可以Watch监测，为了后续方便修改监测条件
        /// 当前设置成DebugBuild才进行监测和输出
        /// </summary>
        public static bool CanWatch
        {
            get
            {
                return Game.Instance.debugEnable;
                // return UnityEngine.Debug.isDebugBuild;
            }
        }
        /// <summary>
        /// 观察函数耗时
        /// </summary>
        /// <param name="del"></param>
        /// <param name="msg"></param>
        public static void Watch(System.Action del, string msg = null)
        {
            if (!CanWatch) return;
            if (del == null) return;
            if (string.IsNullOrEmpty(msg)) msg = "执行耗费时间:";

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            del();
            watch.Stop();
            Log.Debug("{0} {1}ms", msg, watch.Elapsed.TotalMilliseconds);
        }
        /// <summary>
        /// 使用Stopwatch， debug模式下无行为
        /// </summary>
        /// <param name="key"></param>
        /// <param name="del"></param>
        public static void BeginWatch(string key)
        {
            if (!CanWatch)
                return;

            if (m_WachterDictionary == null)
                m_WachterDictionary = new Dictionary<string, Stopwatch>();
            if (m_WachterMems == null)
                m_WachterMems = new Dictionary<Stopwatch, long>();

            System.Diagnostics.Stopwatch stopwatch;
            if (!m_WachterDictionary.TryGetValue(key, out stopwatch))
            {
                stopwatch = m_WachterDictionary[key] = new System.Diagnostics.Stopwatch();
            }

            m_WachterMems[stopwatch] = GC.GetTotalMemory(false);

            if (stopwatch.IsRunning)
            {
                Log.Error("Running stopwatch need reset: {0}", key);
            }

            stopwatch.Reset();
            stopwatch.Start(); //  开始监视代码运行时间
        }

        /// <summary>
        /// 结束性能监测，输出监测的时间消耗
        /// </summary>
        /// <param name="key"></param>
        public static void EndWatch(string key, string name = null)
        {
            if (!CanWatch)
                return;

            if (m_WachterDictionary == null)
                m_WachterDictionary = new Dictionary<string, Stopwatch>();
            if (m_WachterMems == null)
                m_WachterMems = new Dictionary<Stopwatch, long>();

            System.Diagnostics.Stopwatch stopwatch;
            if (!m_WachterDictionary.TryGetValue(key, out stopwatch))
            {
                Log.Error("Not exist Stopwatch: {0}", key);
                return;
            }
            long lastMem = 0;
            m_WachterMems.TryGetValue(stopwatch, out lastMem);

            stopwatch.Stop(); //  停止监视

            string format = "[Watcher] {0}, Time: {1}ms, MemDiff: {2}KB";
            var memDiff = GC.GetTotalMemory(false) - lastMem; // byte
            Log.Error(string.Format(format,
                string.IsNullOrEmpty(name) ? key : name, stopwatch.Elapsed.TotalMilliseconds.ToString("F7"),
                memDiff / 1000f)); // 7位精度
        }
        /// <summary>
        /// 采样，在Profiler查看性能
        /// </summary>
        /// <param name="del"></param>
        /// <param name="name"></param>
        public static void Sampling(System.Action del, string name = "Test Sample")
        {
            if (!CanWatch) return;
            if (del == null) return;
            UnityEngine.Profiling.Profiler.BeginSample(name);
            del();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        public static void BeginSample(string strName)
        {
            if (!CanWatch) return;
            UnityEngine.Profiling.Profiler.BeginSample(strName);
        }

        public static void EndSample()
        {
            if (!CanWatch) return;
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}