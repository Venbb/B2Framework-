using System.Collections;
using System.Collections.Generic;

namespace shaco.Base
{
    /// <summary>
    /// 资源异步请求
    /// </summary>
    public class ObjectPoolRequest<T> : EnumeratorRequest<T>
    {
        /// <summary>
        /// 资源异步请求初始化
        /// <param name="key">对象键值</param>
        /// <param name="callbackCreate">创建对象的异步回调方法</param>
        /// </summary>
        public void Init(string key, System.Action<System.Action<T>> callbackCreate)
        {
            GameHelper.objectpool.InstantiateAsync(callbackCreate, (obj) =>
            {
                this.SetResult(obj);
            });
        }
    }
}