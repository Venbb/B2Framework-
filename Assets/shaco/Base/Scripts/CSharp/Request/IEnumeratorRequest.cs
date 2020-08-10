using System.Collections;

namespace shaco
{
    public enum EnumeratorRequestResultType
    {
        None,
        Loading,
        Success,
        Failed
    }

    /// <summary>
    /// 资源异步请求
    /// </summary>
    public interface IEnumeratorRequest<T> : IEnumerator
    {
        /// <summary>
        /// 加载成功的资源，如果还在加载中或者加载失败则为null
        /// </summary>
        T value { get; }

        /// <summary>
        /// 加载进度(范围：0 ~ 1)
        /// </summary>
        float progress { get; }

        /// <summary>
        /// 是否加载完毕
        /// </summary>
        bool isDone { get; }
        
        /// <summary>
        /// 加载结果
        /// </summary>
        EnumeratorRequestResultType resultType { get; }

        /// <summary>
        /// 设置加载进度(范围: 0 ~ 1)
        /// </summary>
        void SetProgress(float progress);

        /// <summary>
        /// 设置是否执行成功
        /// <param name="loadedValue">加载成功对象</param>
        /// </summary>
        void SetResult(T loadedValue);
    }
}
