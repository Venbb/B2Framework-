namespace B2Framework
{
    public static partial class Utility
    {
        public static T ToEnum<T>(string e)
        {
            return (T)System.Enum.Parse(typeof(T), e);
        }
        /// <summary>
        /// 观察函数耗时
        /// </summary>
        /// <param name="del"></param>
        /// <param name="msg"></param>
        public static void Watch(System.Action del, string msg = null)
        {
            if (del == null) return;
            if (string.IsNullOrEmpty(msg)) msg = "执行耗费时间:";

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            del();
            watch.Stop();
            Log.Debug("{0} {1}ms", msg, watch.Elapsed.TotalMilliseconds);
        }
    }
}