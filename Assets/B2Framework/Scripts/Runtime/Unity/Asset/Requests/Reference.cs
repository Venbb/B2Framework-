using System;
using System.Collections.Generic;

namespace B2Framework.Unity
{
    /// <summary>
    /// 引用计数
    /// </summary>
    public class Reference
    {
        public int refCount;
        /// <summary>
        /// 被引用的实例对象列表
        /// </summary>
        private List<Object> _requires;
        public bool IsUnused()
        {
            if (_requires != null)
            {
                for (var i = 0; i < _requires.Count; i++)
                {
                    var item = _requires[i];
                    if (item != null) continue;
                    Release();
                    _requires.RemoveAt(i);
                    i--;
                }
                if (_requires.Count == 0) _requires = null;
            }
            return refCount <= 0;
        }
        public void Retain()
        {
            refCount++;
        }
        public void Release()
        {
            refCount--;
        }
        /// <summary>
        /// 添加实例对象引用
        /// </summary>
        /// <param name="obj"></param>
        public void Require(Object obj)
        {
            if (_requires == null) _requires = new List<Object>();
            _requires.Add(obj);
            Retain();
        }
        /// <summary>
        /// 移除实例对象引用
        /// </summary>
        /// <param name="obj"></param>
        public void Dequire(Object obj)
        {
            if (_requires == null) return;
            if (_requires.Remove(obj)) Release();
        }
    }
}
