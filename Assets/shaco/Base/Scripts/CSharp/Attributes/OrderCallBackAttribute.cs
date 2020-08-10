namespace shaco.Base
{
    /// <summary>
    /// 带执行前后顺序的回调属性
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class OrderCallBackAttribute : System.Attribute
    {
        public OrderCallBackAttribute() { }
        public OrderCallBackAttribute(int callbackOrder) { this.callbackOrder = callbackOrder; }

        /// <summary>
        /// 回调执行前后顺序，数值越小越优先被调用
        /// </summary>
        public int callbackOrder = 0;
    }
}