using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
	/// <summary>
	/// 性能分析接口
	/// </summary>
    public abstract class IProfiler : IGameInstance
    {
        /// <summary>
        /// 开始分析
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void BeginSample(string tag);

        /// <summary>
        /// 分析结束
        /// </summary>
        [System.Diagnostics.Conditional("DEBUG_LOG")]
        abstract public void EndSample();
    }
}