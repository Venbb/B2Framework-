namespace shaco.Base
{
    /// <summary>
    /// 内存池引用对象，如果需要实现内存池对象自动化操作，请实现继承并实现相关接口
    /// </summary>
    public partial interface IObjectPoolData
    {
        /// <summary>
        /// 数据清理函数，在内存对象被回收时候调用
        /// </summary>
        void Dispose();
    }
}