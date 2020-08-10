namespace shaco.Base
{
	/// <summary>
	/// 性能分析接口
	/// </summary>
    public class Profiler : IProfiler
    {
        /// <summary>
        /// 开始分析
        /// </summary>
        override public void BeginSample(string tag)
        {
            throw new System.NotImplementedException("Profiler.Sample");
        }

        /// <summary>
        /// 分析结束
        /// </summary>
        override public void EndSample()
        {
            throw new System.NotImplementedException("Profiler.EndSample");
        }
    }
}