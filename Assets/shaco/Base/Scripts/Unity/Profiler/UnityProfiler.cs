namespace shaco
{
	/// <summary>
	/// 性能分析接口
	/// </summary>
    public class UnityProfiler : shaco.Base.IProfiler
    {
        /// <summary>
        /// 开始分析
        /// </summary>
        override public void BeginSample(string tag)
        {
            UnityEngine.Profiling.Profiler.BeginSample(tag);
        }

        /// <summary>
        /// 分析结束
        /// </summary>
        override public void EndSample()
        {
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}