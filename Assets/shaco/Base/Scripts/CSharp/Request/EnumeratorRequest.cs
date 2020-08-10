using System.Collections;

namespace shaco
{
    /// <summary>
    /// 资源异步请求
    /// </summary>
    public class EnumeratorRequest<T> : IEnumeratorRequest<T>
    {
        public object Current { get { return _value; } }

        /// <summary>
        /// 加载成功的资源，如果还在加载中或者加载失败则为null
        /// </summary>
        public T value { get { return _value; } }

        /// <summary>
        /// 加载进度(范围：0 ~ 1)
        /// </summary>
        public float progress { get; private set; }

        /// <summary>
        /// 是否加载完毕
        /// </summary>
        public bool isDone { get { return progress >= 1.0f && resultType != EnumeratorRequestResultType.Loading; } }

        /// <summary>
        /// 加载结果
        /// </summary>
        public EnumeratorRequestResultType resultType { get; private set; }

        /// <summary>
        /// 加载成功的资源，如果还在加载中或者加载失败则为null
        /// </summary>
        private T _value = default(T);

        public EnumeratorRequest()
        {
            this.Init();
        }

        /// <summary>
        /// 设置加载进度(范围: 0 ~ 1)
        /// </summary>
        public void SetProgress(float progress)
        {
            if (progress < 0)
                progress = 0;
            else if (progress > 1.0f)
                progress = 1.0f;
            this.progress = progress;
        }

        /// <summary>
        /// 设置是否执行成功
        /// <param name="loadedValue">加载成功对象</param>
        /// </summary>
        public void SetResult(T loadedValue)
        {
            _value = loadedValue;
            this.resultType = null != loadedValue ? EnumeratorRequestResultType.Success : EnumeratorRequestResultType.Failed;
            this.progress = 1.0f;
        }

        /// <summary>
        /// 协程是否执行完毕的判断方法，返回false表示执行完毕
        /// </summary>
        public bool MoveNext()
        {
            return !this.isDone;
        }

        /// <summary>
        /// 重置数据方法
        /// </summary>
        public void Reset()
        {
            this.progress = 0;
            _value = default(T);
            this.resultType = EnumeratorRequestResultType.None;
        }

        /// <summary>
        /// 初始化方法
        /// </summary>
        private void Init()
        {
            this.progress = 0;
            this._value = default(T);
            this.resultType = EnumeratorRequestResultType.Loading;
        }
    }
}
